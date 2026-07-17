using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using Newtonsoft.Json;
using SalesManagementSystem.Data;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class DonDieuChinhDonHangController : BaseController
    {
        private readonly IDonDieuChinhDonHangRepository _repo;
        private readonly IDonDatHangRepository          _orderRepo;
        private readonly DbConnectionFactory            _db;

        public DonDieuChinhDonHangController(
            IDonDieuChinhDonHangRepository repo,
            IDonDatHangRepository orderRepo,
            DbConnectionFactory db)
        {
            _repo = repo;
            _orderRepo = orderRepo;
            _db = db;
        }

        private class DropdownItem { public int ID { get; set; } public string Name { get; set; } }

        private SelectList GetKhachHangList(int? selectedId = null)
        {
            using (var conn = _db.CreateConnection())
            {
                var items = conn.Query<DropdownItem>(
                    "SELECT ID, ISNULL(MaKhachHang, '') + ' - ' + LTRIM(RTRIM(TenKhachHang)) AS Name FROM NS_KhachHang ORDER BY TenKhachHang").ToList();
                return new SelectList(items, "ID", "Name", selectedId);
            }
        }

        private SelectList GetNhanVienList(int? selectedId = null)
        {
            using (var conn = _db.CreateConnection())
            {
                var items = conn.Query<DropdownItem>(
                    "SELECT ID, ISNULL(MaNhanSu, '') + ' - ' + LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) AS Name FROM NS_NhanSu ORDER BY Ten").ToList();
                return new SelectList(items, "ID", "Name", selectedId);
            }
        }

        private SelectList GetTrangThaiList(int? selectedId = null)
        {
            var items = _orderRepo.GetTrangThaiList().Select(x => new DropdownItem { ID = x.ID, Name = x.TenTrangThai }).ToList();
            return new SelectList(items, "ID", "Name", selectedId);
        }

        private UserLoginViewModel GetCurrentUser()
            => (UserLoginViewModel)Session[CommonConstants.USER_SESSION];

        public ActionResult Index(
            int page = 1, int pageSize = 20,
            string tuNgay = "", string denNgay = "",
            int? idKhachHang = null, string soDonHang = "",
            bool chiDonDieuChinh = false)
        {
            int totalRecords;
            var list = _repo.GetPaged(page, pageSize, tuNgay, denNgay, idKhachHang, soDonHang, chiDonDieuChinh, out totalRecords);

            var model = new PagedListViewModel<DonDieuChinhListViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList"
            };

            ViewBag.Title = "Điều chỉnh đơn hàng sau bán hàng";
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.SoDonHang = soDonHang;
            ViewBag.KhachHangs = GetKhachHangList(idKhachHang);
            ViewBag.ChiDonDieuChinh = chiDonDieuChinh;

            if (Request.IsAjaxRequest())
                return PartialView("_AdjustList", model);

            return View(model);
        }

        public ActionResult GetList(
            int page = 1, int pageSize = 20,
            string tuNgay = "", string denNgay = "",
            int? idKhachHang = null, string soDonHang = "",
            bool chiDonDieuChinh = false)
        {
            int totalRecords;
            var list = _repo.GetPaged(page, pageSize, tuNgay, denNgay, idKhachHang, soDonHang, chiDonDieuChinh, out totalRecords);

            var model = new PagedListViewModel<DonDieuChinhListViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList"
            };

            return PartialView("_AdjustList", model);
        }

        // ── Adjust (GET) ──────────────────────────────────────────────────────

        private SelectList GetPhuongTienList(int? selectedId = null)
        {
            using (var conn = _db.CreateConnection())
            {
                var items = conn.Query("SELECT ID, ISNULL(MaPhuongTien, '') + ' - ' + ISNULL(TenPhuongTien, '') AS Name FROM DM_PhuongTien ORDER BY STT, TenPhuongTien")
                    .Select(x => new { ID = (int)x.ID, Name = (string)x.Name }).ToList();
                return new SelectList(items, "ID", "Name", selectedId);
            }
        }

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Adjust(int id)
        {
            var don = _orderRepo.GetById(id);
            if (don == null) return HttpNotFound();

            // Chỉ cho phép điều chỉnh các đơn: đã lập CTBH hoặc xuất kho hoặc đã thu tiền
            using (var conn = _db.CreateConnection())
            {
                bool eligible = conn.ExecuteScalar<bool>(@"
                    SELECT CASE WHEN EXISTS (SELECT 1 FROM BAN_ChungTuBanHang c WHERE c.IDDonDatHang = @ID AND c.IsDeleted = 0)
                                  OR EXISTS (SELECT 1 FROM KHO_PhieuXuat px WHERE px.IDDonDatHang = @ID AND px.TrangThai = 2 AND px.IsDeleted = 0)
                                  OR EXISTS (
                                      SELECT 1 
                                      FROM BAN_PhieuThuKhachHang pt 
                                      INNER JOIN BAN_ChungTuBanHang c2 ON pt.IDChungTuBanHang = c2.ID 
                                      WHERE c2.IDDonDatHang = @ID AND pt.TrangThai = 2 AND pt.IsDeleted = 0 AND c2.IsDeleted = 0
                                  ) THEN 1 ELSE 0 END", new { ID = id });
                if (!eligible)
                {
                    return new HttpStatusCodeResult(400, "Đơn hàng chưa đủ điều kiện để thực hiện điều chỉnh (Chưa có chứng từ, chưa xuất kho hoặc chưa thanh toán).");
                }
            }

            var chiTiets = _orderRepo.GetChiTietByDonId(id);

            string maKH = "", tenKH = "", maST = "", diaChi = "", sdT = "";
            if (don.IDKhachHang.HasValue)
            {
                using (var conn = _db.CreateConnection())
                {
                    var kh = conn.QueryFirstOrDefault<dynamic>(
                        "SELECT MaKhachHang, TenKhachHang AS HoTen, MaSoThue, DiaChi, SoDienThoai FROM NS_KhachHang WHERE ID = @ID",
                        new { ID = don.IDKhachHang });
                    if (kh != null)
                    {
                        maKH = kh.MaKhachHang ?? "";
                        tenKH = kh.HoTen ?? "";
                        maST = kh.MaSoThue ?? "";
                        diaChi = kh.DiaChi ?? "";
                        sdT = kh.SoDienThoai ?? "";
                    }
                }
            }

            var model = new DonDatHangCreateEditViewModel
            {
                ID = don.ID,
                IDKhachHang = don.IDKhachHang,
                MaKhachHang = maKH,
                TenKhachHang = tenKH,
                MaSoThue = maST,
                DiaChi = diaChi,
                SoDienThoai = sdT,
                SoDonHang = don.SoDonHang,
                NgayTaoDon = don.NgayTaoDon,
                IDNhanVien = don.IDNhanVien,
                ThoiHanGiaoHang = don.ThoiHanGiaoHang,
                TrangThaiDon = don.TrangThaiDon,
                TongTien = don.TongTien,
                PhiBocXep = don.PhiBocXep,
                ThanhTienHang = don.ThanhTienHang ?? 0,
                ThanhTienThue = don.ThanhTienThue ?? 0,
                GhiChu = don.GhiChu,
                HoTenTaiXe = don.HoTenTaiXe,
                IDPhuongTien = don.IDPhuongTien,
                ChiTiets = chiTiets
            };
            model.NhanVienList = GetNhanVienList(don.IDNhanVien);
            model.TrangThaiList = GetTrangThaiList(don.TrangThaiDon);
            model.PhuongTienList = GetPhuongTienList(don.IDPhuongTien);

            // Truy vấn ngày giao hàng (ngày xuất kho hoặc ngày chứng từ bán hàng)
            DateTime? ngayGiaoHang = null;
            using (var conn = _db.CreateConnection())
            {
                var ngayInfo = conn.QueryFirstOrDefault<dynamic>(@"
                    SELECT TOP 1 NgayGiao
                    FROM (
                        SELECT px.NgayXuat AS NgayGiao, 1 AS Priority
                        FROM KHO_PhieuXuat px
                        WHERE px.IDDonDatHang = @ID AND px.IsDeleted = 0
                        UNION ALL
                        SELECT c.NgayChungTu AS NgayGiao, 2 AS Priority
                        FROM BAN_ChungTuBanHang c
                        WHERE c.IDDonDatHang = @ID AND c.IsDeleted = 0
                    ) t
                    ORDER BY Priority", new { ID = id });
                if (ngayInfo != null)
                {
                    ngayGiaoHang = (DateTime?)ngayInfo.NgayGiao;
                }
            }
            model.NgayGiaoHang = ngayGiaoHang;

            // Truy vấn kho hiện tại của đơn hàng
            int currentKhoId = 0;
            string currentTenKho = "";
            using (var conn = _db.CreateConnection())
            {
                var khoInfo = conn.QueryFirstOrDefault<dynamic>(@"
                    SELECT TOP 1 IDKho, TenKhoHang
                    FROM (
                        SELECT px.IDKho, kh.TenKhoHang, 1 AS Priority
                        FROM KHO_PhieuXuat px
                        INNER JOIN DM_KhoHang kh ON px.IDKho = kh.ID
                        WHERE px.IDDonDatHang = @ID AND px.IsDeleted = 0
                        UNION ALL
                        SELECT c.IDKho, kh.TenKhoHang, 2 AS Priority
                        FROM BAN_ChungTuBanHang c
                        INNER JOIN DM_KhoHang kh ON c.IDKho = kh.ID
                        WHERE c.IDDonDatHang = @ID AND c.IsDeleted = 0
                    ) t
                    ORDER BY Priority", new { ID = id });
                if (khoInfo != null)
                {
                    currentKhoId = (int)khoInfo.IDKho;
                    currentTenKho = (string)khoInfo.TenKhoHang;
                }
            }
            ViewBag.IDKho = currentKhoId;
            ViewBag.TenKhoHang = currentTenKho;

            ViewBag.Title = "Điều chỉnh đơn đặt hàng";
            ViewBag.ChiTietsJson = JsonConvert.SerializeObject(chiTiets);
            
            return View(model);
        }

        // ── Adjust (POST) ─────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Adjust(DonDieuChinhPostModel model)
        {
            try
            {
                System.IO.File.AppendAllText(Server.MapPath("~/App_Data/adjust_log.txt"), 
                    $"[{DateTime.Now}] Adjust POST: IDDonHang={model.IDDonHang}, GhiChu={model.GhiChu}, HoTenTaiXe={model.HoTenTaiXe}, IDPhuongTien={model.IDPhuongTien}\n");
            }
            catch {}

            if (string.IsNullOrWhiteSpace(model.LyDoDieuChinh))
            {
                ModelState.AddModelError("LyDoDieuChinh", "Vui lòng nhập lý do điều chỉnh");
            }

            if (model.IDKho <= 0)
            {
                ModelState.AddModelError("IDKho", "Vui lòng chọn kho xuất");
            }

            var rawDetails = JsonConvert.DeserializeObject<List<DonDatHangChiTietViewModel>>(model.ChiTietsJson) ?? new List<DonDatHangChiTietViewModel>();
            if (rawDetails.Count == 0)
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một sản phẩm vào đơn hàng");
            }
            else
            {
                for (int i = 0; i < rawDetails.Count; i++)
                {
                    if (!rawDetails[i].IDSanPham.HasValue || rawDetails[i].IDSanPham == 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Vui lòng chọn sản phẩm");
                    if (rawDetails[i].DonGia < 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Đơn giá không được âm");
                    if (rawDetails[i].SoLuong < 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Số lượng không được âm");
                }
            }

            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest() || Request.Headers["X-SPA-Load"] == "true")
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join("<br/>", errors) });
                }

                TempData["ToastMessage"] = "Vui lòng nhập đầy đủ thông tin bắt buộc.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Adjust", new { id = model.IDDonHang });
            }

            var session = GetCurrentUser();
            int userId = session?.IDNhanSu ?? 0;

            try
            {
                _repo.SaveAdjustment(model, userId);
                
                if (Request.IsAjaxRequest() || Request.Headers["X-SPA-Load"] == "true")
                {
                    return Json(new { success = true, message = "Điều chỉnh đơn hàng thành công!", closeTab = true });
                }

                TempData["ToastMessage"] = "Điều chỉnh đơn hàng thành công!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest() || Request.Headers["X-SPA-Load"] == "true")
                {
                    return Json(new { success = false, message = "Lỗi điều chỉnh đơn hàng: " + ex.Message });
                }

                TempData["ToastMessage"] = "Lỗi: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("Adjust", new { id = model.IDDonHang });
            }
        }

        // ── History (AJAX Modal) ──────────────────────────────────────────────

        [HttpGet]
        public ActionResult History(int id)
        {
            var order = _orderRepo.GetById(id);
            if (order == null) return HttpNotFound();

            var history = _repo.GetAdjustHistory(id);

            ViewBag.SoDonHang = order.SoDonHang;
            return PartialView("_HistoryModal", history);
        }

        [HttpPost]
        public ActionResult CheckTonKhoAllKho(int idDonHang, List<CheckTonKhoRequestItem> sanPhams)
        {
            try
            {
                if (sanPhams == null || !sanPhams.Any())
                    return Json(new { success = false, message = "Không có sản phẩm nào để kiểm tra" });

                // 1. Lấy kho hiện tại của đơn hàng
                int currentKhoId = 0;
                using (var conn = _db.CreateConnection())
                {
                    var khoInfo = conn.QueryFirstOrDefault<dynamic>(@"
                        SELECT TOP 1 IDKho
                        FROM (
                            SELECT px.IDKho, 1 AS Priority
                            FROM KHO_PhieuXuat px
                            WHERE px.IDDonDatHang = @ID AND px.IsDeleted = 0
                            UNION ALL
                            SELECT c.IDKho, 2 AS Priority
                            FROM BAN_ChungTuBanHang c
                            WHERE c.IDDonDatHang = @ID AND c.IsDeleted = 0
                        ) t
                        ORDER BY Priority", new { ID = idDonHang });
                    if (khoInfo != null)
                    {
                        currentKhoId = (int)khoInfo.IDKho;
                    }
                }

                // 2. Lấy số lượng sản phẩm cũ trong đơn hàng chi tiết
                var oldQuantities = new Dictionary<int, decimal>();
                using (var conn = _db.CreateConnection())
                {
                    var details = conn.Query<dynamic>(
                        "SELECT IDSanPham, ISNULL(SoLuong, 0) AS SoLuong FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @ID",
                        new { ID = idDonHang }).ToList();
                    foreach (var d in details)
                    {
                        if (d.IDSanPham != null)
                        {
                            int spId = (int)d.IDSanPham;
                            decimal qty = (decimal)d.SoLuong;
                            if (oldQuantities.ContainsKey(spId))
                                oldQuantities[spId] += qty;
                            else
                                oldQuantities[spId] = qty;
                        }
                    }
                }

                // 3. Thực hiện kiểm tra tồn kho từ database (gọi SP)
                List<CheckTonKhoResponseViewModel> result;
                using (var conn = _db.CreateConnection())
                {
                    var p = new DynamicParameters();
                    p.Add("@ListSanPham", Newtonsoft.Json.JsonConvert.SerializeObject(sanPhams));
                    result = conn.Query<CheckTonKhoResponseViewModel>("sp_KHO_TonKho_CheckAllKho", p, commandType: System.Data.CommandType.StoredProcedure).ToList();
                }

                // 4. Cộng lại số lượng của đơn hàng cũ cho kho ban đầu
                foreach (var item in result)
                {
                    if (item.IDKho == currentKhoId && oldQuantities.TryGetValue(item.IDSanPham, out decimal oldQty))
                    {
                        item.SoLuongTon += oldQty;
                        item.ChenhLech = item.SoLuongTon - item.SoLuongCanXuat;
                        item.IsDuTon = item.SoLuongTon >= item.SoLuongCanXuat;
                    }
                }

                // 5. Nhóm theo Kho
                var groupedByKho = result.GroupBy(x => new { x.IDKho, x.TenKhoHang })
                    .Select(g => new
                    {
                        IDKho = g.Key.IDKho,
                        TenKhoHang = g.Key.TenKhoHang,
                        IsDuTonAll = g.All(x => x.IsDuTon),
                        ChiTiets = g.Select(x => new
                        {
                            x.IDSanPham,
                            x.MaSanPham,
                            x.TenSanPham,
                            x.SoLuongCanXuat,
                            x.SoLuongTon,
                            x.ChenhLech,
                            x.IsDuTon
                        }).ToList()
                    }).ToList();

                return Json(new { success = true, data = groupedByKho });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult CheckTonKho(int idDonHang, int idKho, List<CheckTonKhoRequestItem> sanPhams)
        {
            try
            {
                if (sanPhams == null || !sanPhams.Any())
                    return Json(new { success = false, hasError = false, message = "Không có sản phẩm nào để kiểm tra" });

                // 1. Lấy kho hiện tại của đơn hàng
                int currentKhoId = 0;
                using (var conn = _db.CreateConnection())
                {
                    var khoInfo = conn.QueryFirstOrDefault<dynamic>(@"
                        SELECT TOP 1 IDKho
                        FROM (
                            SELECT px.IDKho, 1 AS Priority
                            FROM KHO_PhieuXuat px
                            WHERE px.IDDonDatHang = @ID AND px.IsDeleted = 0
                            UNION ALL
                            SELECT c.IDKho, 2 AS Priority
                            FROM BAN_ChungTuBanHang c
                            WHERE c.IDDonDatHang = @ID AND c.IsDeleted = 0
                        ) t
                        ORDER BY Priority", new { ID = idDonHang });
                    if (khoInfo != null)
                    {
                        currentKhoId = (int)khoInfo.IDKho;
                    }
                }

                // 2. Lấy số lượng sản phẩm cũ trong đơn hàng chi tiết
                var oldQuantities = new Dictionary<int, decimal>();
                using (var conn = _db.CreateConnection())
                {
                    var details = conn.Query<dynamic>(
                        "SELECT IDSanPham, ISNULL(SoLuong, 0) AS SoLuong FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @ID",
                        new { ID = idDonHang }).ToList();
                    foreach (var d in details)
                    {
                        if (d.IDSanPham != null)
                        {
                            int spId = (int)d.IDSanPham;
                            decimal qty = (decimal)d.SoLuong;
                            if (oldQuantities.ContainsKey(spId))
                                oldQuantities[spId] += qty;
                            else
                                oldQuantities[spId] = qty;
                        }
                    }
                }

                // 3. Thực hiện kiểm tra tồn kho từ database cho kho được chọn (gọi SP)
                List<CheckTonKhoResponseViewModel> result;
                using (var conn = _db.CreateConnection())
                {
                    var p = new DynamicParameters();
                    p.Add("@IDKho", idKho);
                    p.Add("@ListSanPham", Newtonsoft.Json.JsonConvert.SerializeObject(sanPhams));
                    result = conn.Query<CheckTonKhoResponseViewModel>("sp_KHO_TonKho_CheckByKho", p, commandType: System.Data.CommandType.StoredProcedure).ToList();
                }

                // 4. Cộng lại số lượng của đơn hàng cũ nếu kho được kiểm tra trùng với kho ban đầu
                foreach (var item in result)
                {
                    if (item.IDKho == currentKhoId && oldQuantities.TryGetValue(item.IDSanPham, out decimal oldQty))
                    {
                        item.SoLuongTon += oldQty;
                        item.ChenhLech = item.SoLuongTon - item.SoLuongCanXuat;
                        item.IsDuTon = item.SoLuongTon >= item.SoLuongCanXuat;
                    }
                }

                bool hasError = result.Any(x => !x.IsDuTon);
                return Json(new { success = true, data = result, hasError = hasError });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, hasError = true, message = ex.Message });
            }
        }
    }
}
