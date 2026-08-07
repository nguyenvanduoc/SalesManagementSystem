using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Services.Interfaces;
using SalesManagementSystem.Data;
using Dapper;

namespace SalesManagementSystem.Controllers
{
    public class BaoCaoDoiChieuCongNoKhachHangController : BaseController
    {
        private readonly IBaoCaoDoiChieuCongNoKhachHangRepository _repo;
        private readonly IExcelExportService _excelExportService;

        public BaoCaoDoiChieuCongNoKhachHangController(
            IBaoCaoDoiChieuCongNoKhachHangRepository repo, 
            IExcelExportService excelExportService)
        {
            _repo = repo;
            _excelExportService = excelExportService;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "BÁO CÁO ĐỐI CHIẾU CÔNG NỢ KHÁCH HÀNG";

            var khachHangs = _repo.GetKhachHangDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.KhachHangList = new SelectList(khachHangs.ToList(), "Value", "Text");

            ViewBag.TuNgay = new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyy-MM-dd");
            ViewBag.DenNgay = DateTime.Now.ToString("yyyy-MM-dd");

            return View();
        }

        [HttpGet]
        public ActionResult GetList(int? idKhachHang, string tuNgay, string denNgay, string soChungTu = null, int page = 1, int pageSize = 20)
        {
            DateTime? parsedTuNgay = null;
            DateTime? parsedDenNgay = null;

            if (DateTime.TryParse(tuNgay, out DateTime dTu)) parsedTuNgay = dTu;
            if (DateTime.TryParse(denNgay, out DateTime dDen)) parsedDenNgay = dDen;

            if (!parsedTuNgay.HasValue)
                return Content("<div class='alert alert-warning text-center mt-3'>Vui lòng chọn từ ngày hợp lệ</div>");

            if (!parsedDenNgay.HasValue)
                return Content("<div class='alert alert-warning text-center mt-3'>Vui lòng chọn đến ngày hợp lệ</div>");

            if (parsedTuNgay.Value > parsedDenNgay.Value)
                return Content("<div class='alert alert-danger text-center mt-3'>Từ ngày không được lớn hơn đến ngày</div>");

            try
            {
                var data = _repo.GetList(idKhachHang, parsedTuNgay.Value, parsedDenNgay.Value, soChungTu).ToList();
                var totalRecords = data.Count;
                var pagedItems = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                decimal tongSoLuongBan = 0;
                decimal tongPhaiThu = 0;
                decimal tongDaThanhToan = 0;
                decimal conNoCuoiKy = 0;

                if (data.Any())
                {
                    tongSoLuongBan = data.Sum(x => x.SoLuongBan);
                    tongPhaiThu = data.Sum(x => x.PhaiThu);
                    tongDaThanhToan = data.Sum(x => x.DaThanhToan);
                    conNoCuoiKy = data.Last().ConNoLuyKe;
                }

                ViewBag.TongSoLuongBan = tongSoLuongBan;
                ViewBag.TongPhaiThu = tongPhaiThu;
                ViewBag.TongDaThanhToan = tongDaThanhToan;
                ViewBag.ConNoCuoiKy = conNoCuoiKy;

                var model = new PagedListViewModel<BaoCaoDoiChieuCongNoKhachHangViewModel>
                {
                    Items = pagedItems,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList",
                    Keyword = ""
                };

                return PartialView("_DanhSach", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger text-center mt-3'>Lỗi tải dữ liệu: {ex.Message}</div>");
            }
        }

        [HttpGet]
        public ActionResult GetDetails(int idPhatSinh, int loaiDong)
        {
            try
            {
                using (var conn = new DbConnectionFactory().CreateConnection())
                {
                    ViewBag.LoaiDong = loaiDong;
                    ViewBag.IDPhatSinh = idPhatSinh;

                    if (loaiDong == 1) // Bán hàng
                    {
                        var chungTu = conn.QueryFirstOrDefault(@"
                            SELECT bh.ID, bh.SoChungTu, bh.NgayChungTu, kh.TenKhachHang,
                                   bh.TongTienHang, bh.TongTienThue, bh.TongCong, bh.PhiBocXep, bh.DaThanhToan, bh.ConLai,
                                   ISNULL(nv.HoTen, ISNULL(LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))), '')) AS NguoiTaoTen
                            FROM BAN_ChungTuBanHang bh
                            LEFT JOIN NS_KhachHang kh ON bh.IDKhachHang = kh.ID
                            LEFT JOIN NS_NhanVien nv ON kh.IDNhanVien = nv.ID
                            LEFT JOIN NS_NhanSu ns ON bh.NguoiTao = ns.ID
                            WHERE bh.ID = @ID AND bh.IsDeleted = 0", new { ID = idPhatSinh });

                        var chiTiets = conn.Query(@"
                            SELECT ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, sp.DVT,
                                   ct.SoLuong, ct.DonGia, ct.ThanhTien, ct.TongSauThue, ct.GhiChu
                            FROM BAN_ChungTuBanHang_ChiTiet ct
                            LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                            WHERE ct.IDChungTuBanHang = @IDChungTu
                            ORDER BY ct.ID", new { IDChungTu = idPhatSinh }).ToList();

                        if (chungTu != null)
                        {
                            ViewBag.BH_SoChungTu   = (string)(chungTu.SoChungTu ?? "");
                            ViewBag.BH_TenKH        = (string)(chungTu.TenKhachHang ?? "");
                            ViewBag.BH_NgayChungTu  = (DateTime?)chungTu.NgayChungTu;
                            ViewBag.BH_TongTienHang = (decimal)(chungTu.TongTienHang ?? 0m);
                            ViewBag.BH_TongTienThue = (decimal)(chungTu.TongTienThue ?? 0m);
                            ViewBag.BH_TongCong     = (decimal)(chungTu.TongCong ?? 0m);
                            ViewBag.BH_PhiBocXep    = (decimal)(chungTu.PhiBocXep ?? 0m);
                            ViewBag.BH_DaThanhToan  = (decimal)(chungTu.DaThanhToan ?? 0m);
                            ViewBag.BH_ConLai       = (decimal)(chungTu.ConLai ?? 0m);
                            ViewBag.BH_NguoiTao     = (string)(chungTu.NguoiTaoTen ?? "");
                        }

                        var chiTietList = chiTiets.Select(ct => new BaoCaoDoiChieuCongNoKhachHangDetailViewModel
                        {
                            MaSanPham   = (string)(ct.MaSanPham ?? ""),
                            TenSanPham  = (string)(ct.TenSanPham ?? ""),
                            DVT         = (string)(ct.DVT ?? ""),
                            SoLuong     = (decimal)(ct.SoLuong ?? 0m),
                            DonGia      = (decimal)(ct.DonGia ?? 0m),
                            ThanhTien   = (decimal)(ct.ThanhTien ?? 0m),
                            TongSauThue = (decimal)(ct.TongSauThue ?? 0m),
                            GhiChu      = (string)(ct.GhiChu ?? "")
                        }).ToList();

                        ViewBag.ChiTiets = chiTietList;
                        ViewBag.TongSoLuong = chiTietList.Sum(x => x.SoLuong);
                        ViewBag.TongThanhTien = chiTietList.Sum(x => x.ThanhTien);
                    }
                    else if (loaiDong == 2) // Trả hàng bán
                    {
                        var traHang = conn.QueryFirstOrDefault(@"
                            SELECT th.ID, th.SoChungTu, th.NgayChungTu, kh.TenKhachHang,
                                   th.TongTienHang, th.TongTienDaHoan, th.ConPhaiHoan, th.LyDoTraHang, th.PhiBocXep,
                                   LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))) AS NguoiTaoTen
                            FROM BAN_TraHangBan th
                            LEFT JOIN NS_KhachHang kh ON th.IDKhachHang = kh.ID
                            LEFT JOIN NS_NhanSu ns ON th.NguoiTao = ns.ID
                            WHERE th.ID = @ID", new { ID = idPhatSinh });

                        var chiTiets = conn.Query(@"
                            SELECT ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, sp.DVT,
                                   ct.SoLuongTra, ct.DonGia, ct.ThanhTien, ct.GhiChu
                            FROM BAN_TraHangBanChiTiet ct
                            LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                            WHERE ct.IDTraHang = @IDTraHang
                            ORDER BY ct.ID", new { IDTraHang = idPhatSinh }).ToList();

                        if (traHang != null)
                        {
                            ViewBag.TH_SoChungTu      = (string)(traHang.SoChungTu ?? "");
                            ViewBag.TH_TenKH           = (string)(traHang.TenKhachHang ?? "");
                            ViewBag.TH_NgayChungTu     = (DateTime?)traHang.NgayChungTu;
                            ViewBag.TH_TongTienHang    = (decimal)(traHang.TongTienHang ?? 0m);
                            ViewBag.TH_TongTienDaHoan  = (decimal)(traHang.TongTienDaHoan ?? 0m);
                            ViewBag.TH_ConPhaiHoan     = (decimal)(traHang.ConPhaiHoan ?? 0m);
                            ViewBag.TH_LyDoTraHang     = (string)(traHang.LyDoTraHang ?? "");
                            ViewBag.TH_PhiBocXep       = (decimal)(traHang.PhiBocXep ?? 0m);
                            ViewBag.TH_NguoiTao        = (string)(traHang.NguoiTaoTen ?? "");
                        }

                        var chiTietList = chiTiets.Select(ct => new BaoCaoDoiChieuCongNoKhachHangDetailViewModel
                        {
                            MaSanPham   = (string)(ct.MaSanPham ?? ""),
                            TenSanPham  = (string)(ct.TenSanPham ?? ""),
                            DVT         = (string)(ct.DVT ?? ""),
                            SoLuong     = (decimal)(ct.SoLuongTra ?? 0m),
                            DonGia      = (decimal)(ct.DonGia ?? 0m),
                            ThanhTien   = (decimal)(ct.ThanhTien ?? 0m),
                            TongSauThue = (decimal)(ct.ThanhTien ?? 0m),
                            GhiChu      = (string)(ct.GhiChu ?? "")
                        }).ToList();

                        ViewBag.ChiTiets = chiTietList;
                        ViewBag.TongSoLuong = chiTietList.Sum(x => x.SoLuong);
                        ViewBag.TongThanhTien = chiTietList.Sum(x => x.ThanhTien);
                    }
                    else if (loaiDong == 3) // Thu tiền khách hàng
                    {
                        var phieuThu = conn.QueryFirstOrDefault(@"
                            SELECT pt.ID, pt.SoPhieuThu, pt.NgayThu, kh.TenKhachHang,
                                   pt.SoTienThu, pt.GhiChu,
                                   tk.TenTaiKhoan, tk.LoaiTaiKhoan,
                                   ISNULL(tk.NganHang, '') AS NganHang,
                                   LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))) AS NguoiTaoTen
                            FROM BAN_PhieuThuKhachHang pt
                            LEFT JOIN NS_KhachHang kh ON pt.IDKhachHang = kh.ID
                            LEFT JOIN DM_TaiKhoanThanhToan tk ON pt.IDTaiKhoanThanhToan = tk.ID
                            LEFT JOIN NS_NhanSu ns ON pt.NguoiTao = ns.ID
                            WHERE pt.ID = @ID AND pt.IsDeleted = 0", new { ID = idPhatSinh });

                        if (phieuThu != null)
                        {
                            ViewBag.PT_SoPhieuThu  = (string)(phieuThu.SoPhieuThu ?? "");
                            ViewBag.PT_TenKH        = (string)(phieuThu.TenKhachHang ?? "");
                            ViewBag.PT_NgayThu      = (DateTime?)phieuThu.NgayThu;
                            ViewBag.PT_SoTienThu    = (decimal)(phieuThu.SoTienThu ?? 0m);
                            ViewBag.PT_GhiChu       = (string)(phieuThu.GhiChu ?? "");
                            
                            string hinhThuc = "";
                            var ltk = phieuThu.LoaiTaiKhoan;
                            if (ltk != null)
                            {
                                int ltkInt = 0;
                                if (int.TryParse(ltk.ToString(), out ltkInt))
                                {
                                    if (ltkInt == 1) hinhThuc = "Tiền mặt";
                                    else if (ltkInt == 2) hinhThuc = "Chuyển khoản";
                                    else hinhThuc = "Loại khác (" + ltkInt + ")";
                                }
                            }
                            ViewBag.PT_HinhThuc    = hinhThuc;
                            ViewBag.PT_TenTaiKhoan = (string)(phieuThu.TenTaiKhoan ?? "");
                            ViewBag.PT_NganHang    = (string)(phieuThu.NganHang ?? "");
                            ViewBag.PT_NguoiTao    = (string)(phieuThu.NguoiTaoTen ?? "");
                        }
                    }

                    return PartialView("_DetailsModal");
                }
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger p-2'>Lỗi tải dữ liệu chi tiết: {ex.Message}</div>");
            }
        }

        [HttpGet]
        public ActionResult ExportExcel(int? idKhachHang, string tuNgay, string denNgay, string soChungTu = null)
        {
            DateTime? parsedTuNgay = null;
            DateTime? parsedDenNgay = null;

            if (DateTime.TryParse(tuNgay, out DateTime dTu)) parsedTuNgay = dTu;
            if (DateTime.TryParse(denNgay, out DateTime dDen)) parsedDenNgay = dDen;

            if (!parsedTuNgay.HasValue || !parsedDenNgay.HasValue)
            {
                TempData["ToastMessage"] = "Thời gian không hợp lệ";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }

            try
            {
                var data = _repo.GetList(idKhachHang, parsedTuNgay.Value, parsedDenNgay.Value, soChungTu).ToList();

                decimal noDauKyLuyke = data.FirstOrDefault(x => x.LoaiDong == 0)?.ConNoLuyKe ?? 0M;

                var exportItems = data.Select(x => new
                {
                    STT = x.STT,
                    NgayPhatSinh = x.NgayPhatSinh.HasValue ? x.NgayPhatSinh.Value.ToString("dd/MM/yyyy") : "",
                    Ngay = x.NgayPhatSinh.HasValue ? x.NgayPhatSinh.Value.ToString("dd/MM/yyyy") : "",
                    SoChungTu = x.SoChungTu ?? "",
                    TenNhanVienPhuTrach = x.TenNhanVien ?? "",
                    TenNhanVien = x.TenNhanVien ?? "",
                    NhanVien = x.TenNhanVien ?? "",
                    PhuTrach = x.TenNhanVien ?? "",
                    TenKhuVuc = x.TenKhuVuc ?? "",
                    KhuVuc = x.TenKhuVuc ?? "",
                    KV = x.TenKhuVuc ?? "",
                    TenTinh = x.TenTinh ?? "",
                    TinhThanh = x.TenTinh ?? "",
                    Tinh = x.TenTinh ?? "",
                    TenKhachHang = x.TenKhachHang ?? "",
                    KhachHang = x.TenKhachHang ?? "",
                    LoaiPhatSinh = x.LoaiPhatSinh ?? "",
                    MaSanPham = x.MaSanPham ?? "",
                    TenSanPham = x.TenSanPham ?? "",
                    DienGiai = x.DienGiai ?? "",
                    SoLuong = x.SoLuongBan,
                    SoLuongBan = x.SoLuongBan,
                    DonGia = x.DonGiaBan,
                    DonGiaBan = x.DonGiaBan,
                    PhaiThu = x.PhaiThu,
                    DaThanhToan = x.DaThanhToan,
                    NoLuyKe = x.ConNoLuyKe,
                    ConNoLuyKe = x.ConNoLuyKe,
                    SoDuLuyKe = x.ConNoLuyKe,
                    GhiChu = x.GhiChu ?? ""
                }).ToList();

                string khName = "Tất cả";
                if (idKhachHang.HasValue)
                {
                    var khList = _repo.GetKhachHangDropdown();
                    var found = khList.FirstOrDefault(x => (int)x.ID == idKhachHang.Value);
                    if (found != null) khName = found.TenHienThi;
                }

                var variables = new Dictionary<string, object>
                {
                    { "NoDauKyLuyke", noDauKyLuyke },
                    { "NoDauKy", noDauKyLuyke },
                    { "TuNgay", parsedTuNgay.Value.ToString("dd/MM/yyyy") },
                    { "DenNgay", parsedDenNgay.Value.ToString("dd/MM/yyyy") },
                    { "KhachHang", khName },
                    { "Ngay", DateTime.Now.Day.ToString("00") },
                    { "Thang", DateTime.Now.Month.ToString("00") },
                    { "Nam", DateTime.Now.Year.ToString() }
                };

                string fileExtension;
                byte[] bytes = _excelExportService.Export(BieuMauConstants.DCKH01, exportItems, out fileExtension, variables);

                if (bytes == null || bytes.Length == 0)
                {
                    TempData["ToastMessage"] = "Không tìm thấy biểu mẫu DCKH01 hoặc lỗi khi tạo file Excel.";
                    TempData["ToastType"] = "error";
                    return RedirectToAction("Index");
                }

                string contentType = fileExtension == "xls" ? "application/vnd.ms-excel" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, $"BaoCaoDoiChieuCongNoKH_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi xuất Excel: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }
        }
    }
}
