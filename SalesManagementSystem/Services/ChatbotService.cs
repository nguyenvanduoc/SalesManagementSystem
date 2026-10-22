using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Services.Interfaces;

namespace SalesManagementSystem.Services
{
    /// <summary>
    /// Trợ lý ảo trả lời câu hỏi cơ bản về số liệu trong hệ thống.
    /// Nhận diện ý định bằng từ khóa tiếng Việt (có/không dấu), tra cứu qua Repository,
    /// và chỉ trả dữ liệu thuộc màn hình mà người dùng có quyền Xem.
    /// </summary>
    public class ChatbotService : IChatbotService
    {
        public const string INTENT_HELP = "HELP";
        public const string INTENT_CONG_NO_KH = "CONG_NO_KH";
        public const string INTENT_CONG_NO_NCC = "CONG_NO_NCC";
        public const string INTENT_TON_KHO = "TON_KHO";
        public const string INTENT_HANG_DI_DUONG = "HANG_DI_DUONG";
        public const string INTENT_DOANH_THU = "DOANH_THU";
        public const string INTENT_QUY_TIEN = "QUY_TIEN";

        private const int DEFAULT_ROWS = 15;
        private const int MAX_ROWS = 50;
        private const int MAX_GOI_Y = 8;
        private const decimal NGUONG_SAP_HET_MAC_DINH = 500; // Giống màn hình Tồn kho khi SP chưa khai báo mức tồn tối thiểu

        private static readonly CultureInfo VN = new CultureInfo("vi-VN");

        // Từ dùng để hỏi, không phải tên khách hàng / sản phẩm / NCC
        private static readonly HashSet<string> StopWords = new HashSet<string>((
            "cong no con thieu phai thu tra chua thanh toan qua han tre lau xau khach hang kh ncc nha cung cap cc " +
            "ton kho trong het sap am can nhap xuat ve di duong dang van chuyen giao don phieu " +
            "san pham sp mat ma bao nhieu nhieu it nhat lon so luong gia tri tong cong danh sach liet ke top " +
            "cua la gi ai nao dau the sao ra hien tai nay bay gio luc hom qua tuan thang nam quy ngay " +
            "cho toi minh em anh chi ban xem hoi giup biet kiem tra tra cuu tim thong tin chi tiet " +
            "va voi mot cac nhung duoc co khong ko k roi a ah nhe nha vay oi u ha thi con lai ke ca " +
            "doanh thu so ban loi nhuan lai lo tien quy du tai khoan ngan hang " +
            "dang nhu bi o tai den tu trong ngoai hon nua di day do kia truoc sau nua"
        ).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);

        private static readonly Dictionary<string, string> VietTat = new Dictionary<string, string>
        {
            { "bn", "bao nhieu" }, { "bnh", "bao nhieu" }, { "nhiu", "nhieu" }, { "bnhiu", "bao nhieu" },
            { "ko", "khong" }, { "k", "khong" }, { "kg", "khong" },
            { "kh", "khach hang" }, { "cn", "cong no" }, { "nc", "nha cung" },
            { "j", "gi" }, { "ntn", "nhu the nao" }, { "dc", "duoc" }
        };

        private readonly IChatbotRepository _chatbotRepo;
        private readonly ICongNoKhachHangRepository _congNoKhachHangRepo;
        private readonly ITonKhoRepository _tonKhoRepo;
        private readonly IDashboardRepository _dashboardRepo;

        public ChatbotService(
            IChatbotRepository chatbotRepo,
            ICongNoKhachHangRepository congNoKhachHangRepo,
            ITonKhoRepository tonKhoRepo,
            IDashboardRepository dashboardRepo)
        {
            _chatbotRepo = chatbotRepo;
            _congNoKhachHangRepo = congNoKhachHangRepo;
            _tonKhoRepo = tonKhoRepo;
            _dashboardRepo = dashboardRepo;
        }

        public ChatbotResponseViewModel Ask(string message, string previousIntent)
        {
            string q = ExpandVietTat(Normalize(message));
            if (q.Length == 0) return Help();

            if (HasAny(q, "cam on", "thank", "thanks", "tks"))
                return new ChatbotResponseViewModel { Intent = IsDataIntent(previousIntent) ? previousIntent : null, Text = "Không có gì ạ! Bạn cần tra cứu thêm gì cứ hỏi mình nhé." };

            string intent = DetectIntent(q);

            if (intent == null)
            {
                // Câu hỏi nối tiếp ("còn khách ABC thì sao?", "tháng trước thì sao?") → dùng lại ý định của câu trước
                if (IsDataIntent(previousIntent) && (TuKhoaConLai(q).Count == 0 || MatchEntities(q, DanhMucTheoIntent(previousIntent)).Count > 0))
                    intent = previousIntent;
                // Chỉ gõ tên sản phẩm / khách hàng
                else if (MatchEntities(q, _chatbotRepo.GetDanhMucSanPham()).Count > 0) intent = INTENT_TON_KHO;
                else if (MatchEntities(q, _chatbotRepo.GetDanhMucKhachHang()).Count > 0) intent = INTENT_CONG_NO_KH;
            }

            switch (intent)
            {
                case INTENT_CONG_NO_KH: return CongNoKhachHang(q);
                case INTENT_CONG_NO_NCC: return CongNoNhaCungCap(q);
                case INTENT_TON_KHO: return TonKho(q);
                case INTENT_HANG_DI_DUONG: return HangDiDuong(q);
                case INTENT_DOANH_THU: return DoanhThu(q);
                case INTENT_QUY_TIEN: return QuyTien();
                case INTENT_HELP: return Help();
                default: return KhongHieu();
            }
        }

        #region Nhận diện ý định

        private static string DetectIntent(string q)
        {
            if (HasAny(q, "di duong", "dang ve", "van chuyen", "dang giao", "chua ve", "tren duong", "hang ve", "sap ve", "chua ghi so"))
                return INTENT_HANG_DI_DUONG;

            bool nhacNcc = HasAny(q, "ncc", "nha cung cap", "nha cung", "nha cc");
            if ((nhacNcc && HasAny(q, "no", "cong no", "tra", "thanh toan", "thieu")) || HasAny(q, "phai tra", "no ncc"))
                return INTENT_CONG_NO_NCC;

            // Cụm từ chắc chắn về kho, kiểm tra trước để "thiếu hàng" không bị hiểu là công nợ
            if (HasAny(q, "ton kho", "thieu hang", "het hang", "sap het", "am kho", "con hang"))
                return INTENT_TON_KHO;

            if (HasAny(q, "cong no", "no", "thieu", "con thieu", "phai thu", "chua tra", "chua thanh toan", "qua han", "no xau", "thu tien"))
                return INTENT_CONG_NO_KH;

            if (HasAny(q, "doanh thu", "doanh so", "ban duoc", "loi nhuan", "lai lo", "tien lai", "lai bao nhieu", "ban chay", "ket qua kinh doanh"))
                return INTENT_DOANH_THU;

            if (HasAny(q, "tien mat", "so du", "quy", "tai khoan", "tien hien co", "ngan hang", "bao nhieu tien", "tien trong"))
                return INTENT_QUY_TIEN;

            if (HasAny(q, "ton", "ton kho", "trong kho", "con hang", "het hang", "sap het", "am kho", "con bao nhieu", "con lai bao nhieu", "hang ton", "thieu hang", "can nhap"))
                return INTENT_TON_KHO;

            if (HasAny(q, "giup", "huong dan", "help", "lam duoc gi", "lam gi", "biet gi", "chao", "xin chao", "hello", "hi", "alo"))
                return INTENT_HELP;

            return null;
        }

        private IEnumerable<ChatbotDanhMucItem> DanhMucTheoIntent(string intent)
        {
            switch (intent)
            {
                case INTENT_CONG_NO_KH: return _chatbotRepo.GetDanhMucKhachHang();
                case INTENT_CONG_NO_NCC: return _chatbotRepo.GetDanhMucNhaCungCap();
                case INTENT_TON_KHO:
                case INTENT_HANG_DI_DUONG: return _chatbotRepo.GetDanhMucSanPham();
                default: return Enumerable.Empty<ChatbotDanhMucItem>();
            }
        }

        // Các từ trong câu hỏi không phải từ để hỏi — thường là tên khách hàng / sản phẩm / NCC
        private static List<string> TuKhoaConLai(string q)
        {
            return q.Split(' ').Where(t => t.Length > 0 && !StopWords.Contains(t) && !t.All(char.IsDigit)).Distinct().ToList();
        }

        private static bool IsDataIntent(string intent)
        {
            return intent == INTENT_CONG_NO_KH || intent == INTENT_CONG_NO_NCC || intent == INTENT_TON_KHO
                || intent == INTENT_HANG_DI_DUONG || intent == INTENT_DOANH_THU || intent == INTENT_QUY_TIEN;
        }

        /// <summary>
        /// Tìm khách hàng / sản phẩm / NCC được nhắc tới trong câu hỏi.
        /// Ưu tiên: khớp mã → khớp nguyên tên → khớp phần lớn từ khóa còn lại.
        /// Trả về các mục có điểm cao nhất (nhiều mục = câu hỏi chưa rõ).
        /// </summary>
        private static List<ChatbotDanhMucItem> MatchEntities(string q, IEnumerable<ChatbotDanhMucItem> source)
        {
            string padded = " " + q + " ";
            var tuKhoa = TuKhoaConLai(q);

            var scored = new List<KeyValuePair<ChatbotDanhMucItem, int>>();
            foreach (var item in source)
            {
                string ma = Normalize(item.Ma);
                string ten = Normalize(item.Ten);
                if (ten.Length == 0) continue;

                int score = 0;
                if (ma.Length >= 3 && padded.Contains(" " + ma + " "))
                {
                    score = 1000;
                }
                else if (ten.Length >= 3 && padded.Contains(" " + ten + " "))
                {
                    score = 500 + ten.Length;
                }
                else if (tuKhoa.Count > 0)
                {
                    var tuTrongTen = ten.Split(' ');
                    var khop = tuKhoa.Where(t => tuTrongTen.Any(w => w == t || (t.Length >= 3 && w.StartsWith(t, StringComparison.Ordinal)))).ToList();
                    // Phải khớp ít nhất một nửa số từ khóa và đủ dài để tránh khớp nhầm vì từ 1-2 ký tự
                    if (khop.Count > 0 && khop.Count * 2 >= tuKhoa.Count && khop.Sum(t => t.Length) >= 3)
                        score = khop.Count * 10 + (khop.Count == tuKhoa.Count ? 5 : 0);
                }

                if (score > 0) scored.Add(new KeyValuePair<ChatbotDanhMucItem, int>(item, score));
            }

            if (scored.Count == 0) return new List<ChatbotDanhMucItem>();
            int top = scored.Max(x => x.Value);
            return scored.Where(x => x.Value == top).Select(x => x.Key).ToList();
        }

        #endregion

        #region Công nợ khách hàng

        private ChatbotResponseViewModel CongNoKhachHang(string q)
        {
            var res = new ChatbotResponseViewModel { Intent = INTENT_CONG_NO_KH };
            if (!PermissionHelper.HasPermission("CongNoKhachHang", LoaiPhanQuyen.Xem))
                return KhongCoQuyen(res, "công nợ khách hàng");

            var matches = MatchEntities(q, _chatbotRepo.GetDanhMucKhachHang());
            if (matches.Count > 1) return ChuaRo(res, "khách hàng", matches, "Công nợ khách hàng {0}");

            string homNay = DateTime.Today.ToString("yyyy-MM-dd");
            res.Links.Add(Link("~/CongNoKhachHang", "CongNoKhachHang", "Công nợ khách hàng"));

            if (matches.Count == 1)
            {
                var kh = matches[0];
                var row = _congNoKhachHangRepo.GetList(null, homNay, kh.ID, null).FirstOrDefault();
                if (row == null || row.ConPhaiThu == 0)
                {
                    res.Text = string.Format("Khách hàng {0} hiện không còn công nợ.", kh.Ten);
                }
                else if (row.ConPhaiThu < 0)
                {
                    res.Text = string.Format("Khách hàng {0} đang trả dư {1} (đã thu nhiều hơn tiền hàng).", kh.Ten, Tien(-row.ConPhaiThu));
                }
                else
                {
                    res.Text = string.Format("Khách hàng {0} còn nợ {1}.", kh.Ten, Tien(row.ConPhaiThu));
                    if (row.TienQuaHan > 0)
                        res.Text += string.Format(" Trong đó quá hạn (trên 30 ngày): {0}.", Tien(row.TienQuaHan));
                }

                if (row != null)
                {
                    var table = NewTable(null, new[] { "Chỉ tiêu", "Giá trị" }, new[] { false, true });
                    table.Rows.Add(new List<string> { "Tổng tiền hàng (đã ghi sổ)", Tien(row.DoanhThu) });
                    table.Rows.Add(new List<string> { "Đã thu", Tien(row.DaThu) });
                    table.Rows.Add(new List<string> { "Còn phải thu", Tien(row.ConPhaiThu) });
                    table.Rows.Add(new List<string> { "Quá hạn > 30 ngày", Tien(row.TienQuaHan) });
                    if (!string.IsNullOrWhiteSpace(row.TenNhanVienPhuTrach))
                        table.Rows.Add(new List<string> { "NV phụ trách", row.TenNhanVienPhuTrach });
                    if (!string.IsNullOrWhiteSpace(row.DienThoai))
                        table.Rows.Add(new List<string> { "Điện thoại", row.DienThoai });
                    res.Tables.Add(table);
                }

                res.Suggestions.Add("Khách hàng nào nợ quá hạn?");
                res.Suggestions.Add("Top 10 khách hàng nợ nhiều nhất");
                return res;
            }

            bool chiQuaHan = HasAny(q, "qua han", "tre han", "no xau", "lau");
            var list = _congNoKhachHangRepo.GetList(null, homNay, null, null).Where(x => x.ConPhaiThu > 0).ToList();
            decimal tongNo = list.Sum(x => x.ConPhaiThu);
            decimal tongQuaHan = list.Sum(x => x.TienQuaHan);

            var hienThi = chiQuaHan
                ? list.Where(x => x.TienQuaHan > 0).OrderByDescending(x => x.TienQuaHan).ToList()
                : list.OrderByDescending(x => x.ConPhaiThu).ToList();

            if (hienThi.Count == 0)
            {
                res.Text = chiQuaHan ? "Hiện không có khách hàng nào nợ quá hạn." : "Hiện không có khách hàng nào còn nợ.";
                return res;
            }

            res.Text = chiQuaHan
                ? string.Format("Có {0} khách hàng nợ quá hạn (trên 30 ngày), tổng tiền quá hạn {1}.", hienThi.Count, Tien(tongQuaHan))
                : string.Format("Có {0} khách hàng còn nợ, tổng phải thu {1}. Trong đó quá hạn (trên 30 ngày): {2}.", list.Count, Tien(tongNo), Tien(tongQuaHan));

            int soDong = SoDong(q);
            var tbl = NewTable(null, new[] { "#", "Khách hàng", "Còn phải thu", "Quá hạn", "NV phụ trách" }, new[] { true, false, true, true, false });
            int stt = 0;
            foreach (var x in hienThi.Take(soDong))
                tbl.Rows.Add(new List<string> { (++stt).ToString(), x.TenKhachHang, Tien(x.ConPhaiThu), Tien(x.TienQuaHan), x.TenNhanVienPhuTrach ?? "" });
            res.Tables.Add(tbl);
            res.Note = GhiChuSoDong(hienThi.Count, soDong, "khách hàng");

            res.Suggestions.Add(chiQuaHan ? "Tổng công nợ khách hàng" : "Khách hàng nào nợ quá hạn?");
            if (hienThi.Count > 0) res.Suggestions.Add("Công nợ khách hàng " + hienThi[0].TenKhachHang);
            res.Suggestions.Add("Công nợ phải trả nhà cung cấp");
            return res;
        }

        #endregion

        #region Công nợ nhà cung cấp

        private ChatbotResponseViewModel CongNoNhaCungCap(string q)
        {
            var res = new ChatbotResponseViewModel { Intent = INTENT_CONG_NO_NCC };
            if (!PermissionHelper.HasPermission("CongNoNCC", LoaiPhanQuyen.Xem))
                return KhongCoQuyen(res, "công nợ nhà cung cấp");

            var matches = MatchEntities(q, _chatbotRepo.GetDanhMucNhaCungCap());
            if (matches.Count > 1) return ChuaRo(res, "nhà cung cấp", matches, "Công nợ nhà cung cấp {0}");

            res.Links.Add(Link("~/cong-no-ncc", "CongNoNCC", "Công nợ phải trả NCC"));
            const string ghiChuCachTinh = "Tính trên phiếu nhập đã ghi sổ; quá hạn là sau 30 ngày kể từ ngày nhập.";

            if (matches.Count == 1)
            {
                var ncc = matches[0];
                var row = _chatbotRepo.GetCongNoNhaCungCap(ncc.ID).FirstOrDefault();
                if (row == null || row.ConNo <= 0)
                {
                    res.Text = string.Format("Hiện không còn nợ nhà cung cấp {0}.", ncc.Ten);
                }
                else
                {
                    res.Text = string.Format("Còn phải trả nhà cung cấp {0}: {1}.", ncc.Ten, Tien(row.ConNo));
                    if (row.QuaHan > 0) res.Text += string.Format(" Trong đó quá hạn: {0}.", Tien(row.QuaHan));
                }

                if (row != null)
                {
                    var table = NewTable(null, new[] { "Chỉ tiêu", "Giá trị" }, new[] { false, true });
                    table.Rows.Add(new List<string> { "Tổng tiền hàng đã nhập", Tien(row.TongPhatSinh) });
                    table.Rows.Add(new List<string> { "Đã thanh toán", Tien(row.DaThanhToan) });
                    table.Rows.Add(new List<string> { "Còn phải trả", Tien(row.ConNo) });
                    table.Rows.Add(new List<string> { "Quá hạn > 30 ngày", Tien(row.QuaHan) });
                    table.Rows.Add(new List<string> { "Số phiếu nhập còn nợ", row.SoPhieuConNo.ToString() });
                    res.Tables.Add(table);
                }
                res.Note = ghiChuCachTinh;
                res.Suggestions.Add("Tổng công nợ nhà cung cấp");
                return res;
            }

            bool chiQuaHan = HasAny(q, "qua han", "tre han", "lau");
            var list = _chatbotRepo.GetCongNoNhaCungCap(null).Where(x => x.ConNo > 0).ToList();
            var hienThi = chiQuaHan
                ? list.Where(x => x.QuaHan > 0).OrderByDescending(x => x.QuaHan).ToList()
                : list.OrderByDescending(x => x.ConNo).ToList();

            if (hienThi.Count == 0)
            {
                res.Text = chiQuaHan ? "Hiện không có khoản nợ nhà cung cấp nào quá hạn." : "Hiện không còn nợ nhà cung cấp nào.";
                return res;
            }

            res.Text = string.Format("Đang nợ {0} nhà cung cấp, tổng còn phải trả {1}. Trong đó quá hạn: {2}.",
                list.Count, Tien(list.Sum(x => x.ConNo)), Tien(list.Sum(x => x.QuaHan)));

            int soDong = SoDong(q);
            var tbl = NewTable(null, new[] { "#", "Nhà cung cấp", "Còn phải trả", "Quá hạn" }, new[] { true, false, true, true });
            int stt = 0;
            foreach (var x in hienThi.Take(soDong))
                tbl.Rows.Add(new List<string> { (++stt).ToString(), x.Ten, Tien(x.ConNo), Tien(x.QuaHan) });
            res.Tables.Add(tbl);
            res.Note = JoinNote(GhiChuSoDong(hienThi.Count, soDong, "nhà cung cấp"), ghiChuCachTinh);

            res.Suggestions.Add("Công nợ khách hàng");
            res.Suggestions.Add("Hàng đang đi đường");
            return res;
        }

        #endregion

        #region Tồn kho

        private class TonKhoSanPham
        {
            public int IDSanPham { get; set; }
            public string MaSanPham { get; set; }
            public string TenSanPham { get; set; }
            public string DVT { get; set; }
            public decimal TonKho { get; set; }
            public decimal GiaTriTon { get; set; }
            public decimal MucTonToiThieu { get; set; }
        }

        private static List<TonKhoSanPham> GopTheoSanPham(IEnumerable<TonKhoListViewModel> rows)
        {
            return rows.GroupBy(x => x.IDSanPham).Select(g => new TonKhoSanPham
            {
                IDSanPham = g.Key,
                MaSanPham = g.First().MaSanPham,
                TenSanPham = g.First().TenSanPham,
                DVT = g.First().DVT,
                TonKho = g.Sum(x => x.TonKho),
                GiaTriTon = g.Sum(x => x.GiaTriTon),
                MucTonToiThieu = g.Max(x => x.MucTonToiThieu)
            }).ToList();
        }

        private ChatbotResponseViewModel TonKho(string q)
        {
            var res = new ChatbotResponseViewModel { Intent = INTENT_TON_KHO };
            if (!PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem))
                return KhongCoQuyen(res, "tồn kho");

            // Nhân viên kho (không có quyền Tùy chọn) không được xem giá trị tồn — giống màn hình Tồn kho
            bool xemGiaTri = PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.TuyChon);
            string homNay = DateTime.Today.ToString("yyyy-MM-dd");
            res.Links.Add(Link("~/TonKho", "TonKho", "Tồn kho"));

            var matches = MatchEntities(q, _chatbotRepo.GetDanhMucSanPham());
            if (matches.Count > 1) return ChuaRo(res, "sản phẩm", matches, "Tồn kho {0}");

            if (matches.Count == 1)
            {
                var sp = matches[0];
                var rows = _tonKhoRepo.GetList(null, sp.ID, null, homNay, false).Where(x => x.TonKho != 0).ToList();
                string dvt = rows.Select(x => x.DVT).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "";
                decimal tong = rows.Sum(x => x.TonKho);

                res.Text = rows.Count == 0
                    ? string.Format("Sản phẩm {0} hiện không còn tồn kho.", sp.Ten)
                    : tong < 0
                    ? string.Format("Sản phẩm {0} đang âm kho {1} {2}, cần kiểm tra lại chứng từ nhập/xuất.", sp.Ten, SoLuong(-tong), dvt)
                    : string.Format("Sản phẩm {0} hiện tồn {1} {2}{3}.", sp.Ten, SoLuong(tong), dvt,
                        rows.Count > 1 ? string.Format(" tại {0} kho", rows.Count) : "");

                if (rows.Count > 0)
                {
                    var tbl = xemGiaTri
                        ? NewTable(null, new[] { "Kho", "Tồn", "ĐVT", "Giá trị tồn" }, new[] { false, true, false, true })
                        : NewTable(null, new[] { "Kho", "Tồn", "ĐVT" }, new[] { false, true, false });
                    foreach (var x in rows.OrderByDescending(r => r.TonKho))
                    {
                        var cells = new List<string> { x.TenKho, SoLuong(x.TonKho), x.DVT ?? "" };
                        if (xemGiaTri) cells.Add(Tien(x.GiaTriTon));
                        tbl.Rows.Add(cells);
                    }
                    res.Tables.Add(tbl);
                }

                // Bổ sung hàng đang đi đường của sản phẩm này
                if (PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem))
                {
                    var diDuong = _chatbotRepo.GetHangDangVeKho(sp.ID).ToList();
                    if (diDuong.Count > 0)
                    {
                        res.Note = string.Format("Đang có thêm {0} {1} đi đường về kho ({2} phiếu nhập chưa ghi sổ: {3}).",
                            SoLuong(diDuong.Sum(x => x.SoLuong)), dvt,
                            diDuong.Select(x => x.IDChungTu).Distinct().Count(),
                            string.Join(", ", diDuong.Select(x => x.SoChungTu).Distinct().Take(5)));
                    }
                }

                res.Suggestions.Add("Sản phẩm nào sắp hết hàng?");
                res.Suggestions.Add("Hàng " + sp.Ten + " đang đi đường");
                return res;
            }

            bool sapHet = HasAny(q, "sap het", "het hang", "can nhap", "thieu hang", "am kho", "it nhat", "sap can");
            if (sapHet)
            {
                bool chiAmKho = HasAny(q, "am kho");
                var tatCa = GopTheoSanPham(_tonKhoRepo.GetList(null, null, null, homNay, false));
                var hienThi = (chiAmKho
                        ? tatCa.Where(x => x.TonKho < 0)
                        : tatCa.Where(x => x.TonKho < (x.MucTonToiThieu > 0 ? x.MucTonToiThieu : NGUONG_SAP_HET_MAC_DINH)))
                    .OrderBy(x => x.TonKho).ToList();

                if (hienThi.Count == 0)
                {
                    res.Text = chiAmKho ? "Không có sản phẩm nào bị âm kho." : "Chưa có sản phẩm nào sắp hết hàng.";
                    return res;
                }

                res.Text = chiAmKho
                    ? string.Format("Có {0} sản phẩm đang âm kho, cần kiểm tra lại chứng từ.", hienThi.Count)
                    : string.Format("Có {0} sản phẩm sắp hết hàng (dưới mức tồn tối thiểu, hoặc dưới {1} nếu chưa khai báo mức).", hienThi.Count, SoLuong(NGUONG_SAP_HET_MAC_DINH));

                int soDongSapHet = SoDong(q);
                var tbl = NewTable(null, new[] { "#", "Sản phẩm", "Tồn", "ĐVT", "Mức tối thiểu" }, new[] { true, false, true, false, true });
                int stt = 0;
                foreach (var x in hienThi.Take(soDongSapHet))
                    tbl.Rows.Add(new List<string> { (++stt).ToString(), x.TenSanPham, SoLuong(x.TonKho), x.DVT ?? "", x.MucTonToiThieu > 0 ? SoLuong(x.MucTonToiThieu) : "" });
                res.Tables.Add(tbl);
                res.Note = GhiChuSoDong(hienThi.Count, soDongSapHet, "sản phẩm");
                res.Suggestions.Add("Hàng đang đi đường");
                res.Suggestions.Add("Tồn kho tổng quan");
                return res;
            }

            var dsTon = GopTheoSanPham(_tonKhoRepo.GetList(null, null, null, homNay, true)).Where(x => x.TonKho > 0).ToList();
            if (dsTon.Count == 0)
            {
                res.Text = "Hiện không có sản phẩm nào còn tồn kho.";
                return res;
            }

            res.Text = string.Format("Hiện có {0} sản phẩm còn tồn, tổng số lượng {1}", dsTon.Count, SoLuong(dsTon.Sum(x => x.TonKho)));
            res.Text += xemGiaTri ? string.Format(", tổng giá trị tồn {0}.", Tien(dsTon.Sum(x => x.GiaTriTon))) : ".";

            bool tangDan = HasAny(q, "it nhat", "thap nhat");
            var sapXep = tangDan ? dsTon.OrderBy(x => x.TonKho).ToList() : dsTon.OrderByDescending(x => x.TonKho).ToList();
            int soDongTon = SoDong(q);
            var tblTon = xemGiaTri
                ? NewTable(null, new[] { "#", "Sản phẩm", "Tồn", "ĐVT", "Giá trị tồn" }, new[] { true, false, true, false, true })
                : NewTable(null, new[] { "#", "Sản phẩm", "Tồn", "ĐVT" }, new[] { true, false, true, false });
            int sttTon = 0;
            foreach (var x in sapXep.Take(soDongTon))
            {
                var cells = new List<string> { (++sttTon).ToString(), x.TenSanPham, SoLuong(x.TonKho), x.DVT ?? "" };
                if (xemGiaTri) cells.Add(Tien(x.GiaTriTon));
                tblTon.Rows.Add(cells);
            }
            res.Tables.Add(tblTon);
            res.Note = GhiChuSoDong(sapXep.Count, soDongTon, "sản phẩm");

            res.Suggestions.Add("Sản phẩm nào sắp hết hàng?");
            res.Suggestions.Add("Tồn kho " + sapXep[0].TenSanPham);
            res.Suggestions.Add("Hàng đang đi đường");
            return res;
        }

        #endregion

        #region Hàng đi đường

        private ChatbotResponseViewModel HangDiDuong(string q)
        {
            var res = new ChatbotResponseViewModel { Intent = INTENT_HANG_DI_DUONG };

            bool hoiGiao = HasAny(q, "dang giao", "giao cho khach", "giao khach", "giao hang", "khach hang", "don hang", "xuat di");
            bool hoiNhap = HasAny(q, "dang ve", "ve kho", "nhap", "ncc", "nha cung cap", "chua ghi so", "sap ve", "hang ve");
            if (!hoiGiao && !hoiNhap) { hoiGiao = true; hoiNhap = true; }

            bool quyenNhap = PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem);
            bool quyenGiao = PermissionHelper.HasPermission("DonDatHang", LoaiPhanQuyen.Xem);
            hoiNhap = hoiNhap && quyenNhap;
            hoiGiao = hoiGiao && quyenGiao;
            if (!hoiNhap && !hoiGiao) return KhongCoQuyen(res, "hàng đi đường (phiếu nhập kho / đơn đặt hàng)");

            var matches = MatchEntities(q, _chatbotRepo.GetDanhMucSanPham());
            if (matches.Count > 1) return ChuaRo(res, "sản phẩm", matches, "Hàng {0} đang đi đường");
            int? idSanPham = matches.Count == 1 ? matches[0].ID : (int?)null;
            string tenLoc = matches.Count == 1 ? " của " + matches[0].Ten : "";

            int soDong = SoDong(q);
            var dongTomTat = new List<string>();

            if (hoiNhap)
            {
                var nhap = _chatbotRepo.GetHangDangVeKho(idSanPham).ToList();
                int soPhieu = nhap.Select(x => x.IDChungTu).Distinct().Count();
                if (soPhieu == 0)
                {
                    dongTomTat.Add("• Không có hàng" + tenLoc + " đang đi đường về kho.");
                }
                else
                {
                    dongTomTat.Add(string.Format("• Đang về kho: {0} phiếu nhập chưa ghi sổ, tổng {1} (trị giá {2}).",
                        soPhieu, SoLuong(nhap.Sum(x => x.SoLuong)), Tien(nhap.Sum(x => x.ThanhTien))));
                    res.Tables.Add(BangTheoSanPham("Hàng đang về kho theo sản phẩm", nhap, soDong));
                    res.Tables.Add(BangTheoChungTu("Phiếu nhập đang đi đường", "Nhà cung cấp", nhap, soDong));
                }
                res.Links.Add(Link("~/PhieuNhapKho", "PhieuNhapKho", "Phiếu nhập kho"));
            }

            if (hoiGiao)
            {
                var giao = _chatbotRepo.GetHangDangGiao(idSanPham).ToList();
                int soDon = giao.Select(x => x.IDChungTu).Distinct().Count();
                if (soDon == 0)
                {
                    dongTomTat.Add("• Không có đơn hàng" + tenLoc + " nào đang giao cho khách.");
                }
                else
                {
                    dongTomTat.Add(string.Format("• Đang giao cho khách: {0} đơn hàng, tổng {1} (trị giá {2}).",
                        soDon, SoLuong(giao.Sum(x => x.SoLuong)), Tien(giao.Sum(x => x.ThanhTien))));
                    res.Tables.Add(BangTheoChungTu("Đơn hàng đang giao", "Khách hàng", giao, soDong));
                }
                res.Links.Add(Link("~/DonDatHang", "DonDatHang", "Đơn đặt hàng"));
            }

            res.Text = "Hàng đang đi đường" + tenLoc + ":\n" + string.Join("\n", dongTomTat);
            res.Suggestions.Add("Tồn kho tổng quan");
            res.Suggestions.Add("Sản phẩm nào sắp hết hàng?");
            return res;
        }

        private static ChatbotTableViewModel BangTheoSanPham(string title, List<ChatbotHangDiDuongItem> items, int soDong)
        {
            var tbl = NewTable(title, new[] { "Sản phẩm", "Số lượng", "ĐVT", "Số phiếu" }, new[] { false, true, false, true });
            var nhom = items.GroupBy(x => x.IDSanPham)
                .Select(g => new
                {
                    Ten = g.First().TenSanPham,
                    DVT = g.First().DVT,
                    SoLuong = g.Sum(x => x.SoLuong),
                    SoPhieu = g.Select(x => x.IDChungTu).Distinct().Count()
                })
                .OrderByDescending(x => x.SoLuong);
            foreach (var x in nhom.Take(soDong))
                tbl.Rows.Add(new List<string> { x.Ten, SoLuong(x.SoLuong), x.DVT ?? "", x.SoPhieu.ToString() });
            return tbl;
        }

        private static ChatbotTableViewModel BangTheoChungTu(string title, string tenCotDoiTac, List<ChatbotHangDiDuongItem> items, int soDong)
        {
            var tbl = NewTable(title, new[] { "Số CT", "Ngày", tenCotDoiTac, "Xe / Tài xế", "Số lượng" }, new[] { false, false, false, false, true });
            var nhom = items.GroupBy(x => x.IDChungTu)
                .Select(g => new
                {
                    g.First().SoChungTu,
                    g.First().Ngay,
                    g.First().DoiTac,
                    Xe = string.Join(" / ", new[] { g.First().PhuongTien, g.First().TaiXe }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    SoLuong = g.Sum(x => x.SoLuong)
                });
            foreach (var x in nhom.Take(soDong))
                tbl.Rows.Add(new List<string> { x.SoChungTu, x.Ngay.HasValue ? x.Ngay.Value.ToString("dd/MM/yyyy") : "", x.DoiTac, x.Xe, SoLuong(x.SoLuong) });
            return tbl;
        }

        #endregion

        #region Doanh thu & Quỹ tiền

        private ChatbotResponseViewModel DoanhThu(string q)
        {
            var res = new ChatbotResponseViewModel { Intent = INTENT_DOANH_THU };
            if (!PermissionHelper.HasPermission("Dashboard", LoaiPhanQuyen.Xem))
                return KhongCoQuyen(res, "doanh thu (Dashboard)");

            DateTime tuNgay, denNgay;
            string tenKy;
            XacDinhKy(q, out tuNgay, out denNgay, out tenKy);

            var data = _dashboardRepo.GetDashboardData(tuNgay, denNgay);
            var s = data.Summary;

            res.Text = string.Format("Doanh thu {0}: {1}", tenKy, Tien(s.DoanhThu));
            if (s.DoanhThuKyTruoc != 0)
                res.Text += string.Format(" ({0}{1}% so với kỳ trước liền kề)", s.TyLeTangGiamDoanhThu >= 0 ? "+" : "", s.TyLeTangGiamDoanhThu.ToString("0.#", VN));
            res.Text += string.Format(".\nLợi nhuận gộp (doanh thu - giá vốn): {0}.", Tien(s.LoiNhuan));

            if (data.TopBanChay.Count > 0)
            {
                var tbl = NewTable("Sản phẩm bán chạy", new[] { "#", "Sản phẩm", "Số lượng" }, new[] { true, false, true });
                int stt = 0;
                foreach (var x in data.TopBanChay.Take(5))
                    tbl.Rows.Add(new List<string> { (++stt).ToString(), x.Label, SoLuong(x.Value) });
                res.Tables.Add(tbl);
            }

            res.Links.Add(Link("~/Dashboard", "Dashboard", "Dashboard"));
            res.Suggestions.Add("Doanh thu tháng trước");
            res.Suggestions.Add("Doanh thu hôm nay");
            res.Suggestions.Add("Doanh thu năm nay");
            return res;
        }

        private ChatbotResponseViewModel QuyTien()
        {
            var res = new ChatbotResponseViewModel { Intent = INTENT_QUY_TIEN };
            if (!PermissionHelper.HasPermission("SoQuy", LoaiPhanQuyen.Xem) && !PermissionHelper.HasPermission("Dashboard", LoaiPhanQuyen.Xem))
                return KhongCoQuyen(res, "sổ quỹ");

            var data = _dashboardRepo.GetDashboardData(null, null);
            res.Text = string.Format("Tiền mặt hiện có: {0}.\nTổng số dư các tài khoản thanh toán: {1}.",
                Tien(data.Summary.TienMat), Tien(data.Summary.TongSoDuTaiKhoan));

            if (data.TaiKhoanThanhToan.Count > 0)
            {
                var tbl = NewTable(null, new[] { "Tài khoản", "Ngân hàng", "Số dư" }, new[] { false, false, true });
                foreach (var tk in data.TaiKhoanThanhToan)
                    tbl.Rows.Add(new List<string> { tk.TenTaiKhoan, tk.NganHang ?? "", Tien(tk.SoDuCuoiKy) });
                res.Tables.Add(tbl);
            }

            res.Links.Add(Link("~/SoQuy", "SoQuy", "Sổ quỹ"));
            res.Suggestions.Add("Doanh thu tháng này");
            res.Suggestions.Add("Công nợ khách hàng");
            return res;
        }

        /// <summary>
        /// Đọc kỳ báo cáo trong câu hỏi: hôm nay, hôm qua, tuần này/trước, tháng này/trước, tháng 5 (năm 2026), quý 2, năm nay/trước.
        /// Mặc định: từ đầu tháng tới hôm nay.
        /// </summary>
        private static void XacDinhKy(string q, out DateTime tuNgay, out DateTime denNgay, out string tenKy)
        {
            DateTime homNay = DateTime.Today;
            int namHoi = homNay.Year;
            var mNam = Regex.Match(q, @"\bnam (\d{4})\b");
            if (mNam.Success) namHoi = int.Parse(mNam.Groups[1].Value);

            var mThang = Regex.Match(q, @"\bthang (\d{1,2})\b");
            var mQuy = Regex.Match(q, @"\bquy ([1-4])\b");

            if (HasAny(q, "hom nay"))
            {
                tuNgay = denNgay = homNay; tenKy = "hôm nay";
            }
            else if (HasAny(q, "hom qua"))
            {
                tuNgay = denNgay = homNay.AddDays(-1); tenKy = "hôm qua";
            }
            else if (HasAny(q, "tuan nay", "tuan truoc"))
            {
                int lechThu = ((int)homNay.DayOfWeek + 6) % 7; // Thứ 2 là đầu tuần
                DateTime dauTuan = homNay.AddDays(-lechThu);
                if (HasAny(q, "tuan truoc"))
                {
                    tuNgay = dauTuan.AddDays(-7); denNgay = dauTuan.AddDays(-1); tenKy = "tuần trước";
                }
                else
                {
                    tuNgay = dauTuan; denNgay = homNay; tenKy = "tuần này";
                }
            }
            else if (mThang.Success && int.Parse(mThang.Groups[1].Value) >= 1 && int.Parse(mThang.Groups[1].Value) <= 12)
            {
                int thang = int.Parse(mThang.Groups[1].Value);
                tuNgay = new DateTime(namHoi, thang, 1);
                denNgay = tuNgay.AddMonths(1).AddDays(-1);
                tenKy = string.Format("tháng {0}/{1}", thang, namHoi);
            }
            else if (mQuy.Success)
            {
                int quy = int.Parse(mQuy.Groups[1].Value);
                tuNgay = new DateTime(namHoi, (quy - 1) * 3 + 1, 1);
                denNgay = tuNgay.AddMonths(3).AddDays(-1);
                tenKy = string.Format("quý {0}/{1}", quy, namHoi);
            }
            else if (HasAny(q, "thang truoc"))
            {
                tuNgay = new DateTime(homNay.Year, homNay.Month, 1).AddMonths(-1);
                denNgay = tuNgay.AddMonths(1).AddDays(-1);
                tenKy = "tháng trước";
            }
            else if (HasAny(q, "nam truoc", "nam ngoai"))
            {
                tuNgay = new DateTime(homNay.Year - 1, 1, 1);
                denNgay = new DateTime(homNay.Year - 1, 12, 31);
                tenKy = "năm " + (homNay.Year - 1);
            }
            else if (HasAny(q, "nam nay") || mNam.Success)
            {
                tuNgay = new DateTime(namHoi, 1, 1);
                denNgay = namHoi == homNay.Year ? homNay : new DateTime(namHoi, 12, 31);
                tenKy = "năm " + namHoi;
            }
            else
            {
                tuNgay = new DateTime(homNay.Year, homNay.Month, 1);
                denNgay = homNay;
                tenKy = "tháng này";
            }

            if (tuNgay != denNgay)
                tenKy += string.Format(" ({0:dd/MM} - {1:dd/MM/yyyy})", tuNgay, denNgay);
            else
                tenKy += string.Format(" ({0:dd/MM/yyyy})", tuNgay);
        }

        #endregion

        #region Câu trả lời chung

        private static ChatbotResponseViewModel Help()
        {
            var res = new ChatbotResponseViewModel
            {
                Intent = INTENT_HELP,
                Text = "Xin chào! Mình là trợ lý ảo của hệ thống. Bạn có thể hỏi mình:\n" +
                       "• Công nợ: khách hàng nào còn nợ, nợ quá hạn, công nợ của một khách hàng / nhà cung cấp\n" +
                       "• Tồn kho: tổng quan, tồn của từng sản phẩm, sản phẩm sắp hết / âm kho\n" +
                       "• Hàng đi đường: hàng đang về kho, đơn hàng đang giao\n" +
                       "• Doanh thu, lợi nhuận theo kỳ và số dư quỹ tiền\n" +
                       "Gõ tự nhiên, có dấu hoặc không dấu đều được."
            };
            res.Suggestions.AddRange(GoiYMau());
            return res;
        }

        private static ChatbotResponseViewModel KhongHieu()
        {
            var res = new ChatbotResponseViewModel
            {
                Text = "Xin lỗi, mình chưa hiểu câu hỏi. Mình đang trả lời được về công nợ, tồn kho, hàng đi đường, doanh thu và quỹ tiền. Bạn thử hỏi theo các gợi ý dưới đây nhé."
            };
            res.Suggestions.AddRange(GoiYMau());
            return res;
        }

        private static IEnumerable<string> GoiYMau()
        {
            var goiY = new List<string>();
            if (PermissionHelper.HasPermission("CongNoKhachHang", LoaiPhanQuyen.Xem)) goiY.Add("Khách hàng nào còn nợ?");
            if (PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) goiY.Add("Tồn kho tổng quan");
            if (PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem) || PermissionHelper.HasPermission("DonDatHang", LoaiPhanQuyen.Xem))
                goiY.Add("Hàng đi đường còn bao nhiêu?");
            if (PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) goiY.Add("Sản phẩm nào sắp hết hàng?");
            if (PermissionHelper.HasPermission("CongNoNCC", LoaiPhanQuyen.Xem)) goiY.Add("Công nợ phải trả nhà cung cấp");
            if (PermissionHelper.HasPermission("Dashboard", LoaiPhanQuyen.Xem)) goiY.Add("Doanh thu tháng này");
            return goiY;
        }

        private static ChatbotResponseViewModel KhongCoQuyen(ChatbotResponseViewModel res, string tenDuLieu)
        {
            res.Text = string.Format("Bạn chưa được phân quyền xem {0}. Vui lòng liên hệ quản trị viên nếu cần.", tenDuLieu);
            res.Tables.Clear();
            res.Links.Clear();
            return res;
        }

        private static ChatbotResponseViewModel ChuaRo(ChatbotResponseViewModel res, string loai, List<ChatbotDanhMucItem> matches, string mauCauHoi)
        {
            res.Text = string.Format("Có {0} {1} khớp với câu hỏi, bạn muốn xem {1} nào?", matches.Count, loai);
            res.Links.Clear();
            // Gửi kèm mã để lần hỏi tiếp theo khớp chính xác một mục
            foreach (var m in matches.Take(MAX_GOI_Y))
            {
                string ten = string.IsNullOrWhiteSpace(m.Ma) || Normalize(m.Ma).Length < 3 ? m.Ten : m.Ten + " - " + m.Ma;
                res.Suggestions.Add(string.Format(mauCauHoi, ten));
            }
            if (matches.Count > MAX_GOI_Y)
                res.Note = string.Format("Chỉ hiển thị {0} kết quả đầu, hãy gõ tên cụ thể hơn.", MAX_GOI_Y);
            return res;
        }

        #endregion

        #region Tiện ích

        /// <summary>
        /// Chuẩn hóa tiếng Việt để so khớp: chữ thường, bỏ dấu, đ → d, ký tự đặc biệt → khoảng trắng.
        /// </summary>
        public static string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            string formD = s.ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (char c in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
            }
            return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        }

        private static string ExpandVietTat(string q)
        {
            return string.Join(" ", q.Split(' ').Select(t => VietTat.ContainsKey(t) ? VietTat[t] : t));
        }

        // So khớp theo cụm từ nguyên vẹn (không khớp một phần của từ khác)
        private static bool HasAny(string q, params string[] phrases)
        {
            string padded = " " + q + " ";
            return phrases.Any(p => padded.Contains(" " + p + " "));
        }

        // "top 10", "10 khách hàng" → số dòng muốn xem
        private static int SoDong(string q)
        {
            var m = Regex.Match(q, @"\btop (\d{1,3})\b");
            if (!m.Success) return DEFAULT_ROWS;
            int n = int.Parse(m.Groups[1].Value);
            return Math.Max(1, Math.Min(n, MAX_ROWS));
        }

        private static string GhiChuSoDong(int tong, int soDong, string loai)
        {
            return tong > soDong ? string.Format("Hiển thị {0}/{1} {2}. Gõ \"top 30\" để xem nhiều hơn hoặc mở màn hình chi tiết.", soDong, tong, loai) : null;
        }

        private static string JoinNote(params string[] notes)
        {
            var list = notes.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
            return list.Count == 0 ? null : string.Join(" ", list);
        }

        private static ChatbotLinkViewModel Link(string url, string key, string title)
        {
            return new ChatbotLinkViewModel { Url = url, Key = key, Title = title };
        }

        private static ChatbotTableViewModel NewTable(string title, string[] headers, bool[] alignRight)
        {
            return new ChatbotTableViewModel { Title = title, Headers = headers.ToList(), AlignRight = alignRight.ToList() };
        }

        private static string Tien(decimal v)
        {
            return v.ToString("N0", VN) + " đ";
        }

        private static string SoLuong(decimal v)
        {
            return v.ToString("#,##0.##", VN);
        }

        #endregion
    }
}
