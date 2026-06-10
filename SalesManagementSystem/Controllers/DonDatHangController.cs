using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using Newtonsoft.Json;
using SalesManagementSystem.Data;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.Enums;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.IO;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class DonDatHangController : BaseController
    {
        private readonly IDonDatHangRepository _repo;
        private readonly DbConnectionFactory   _db;
        private readonly SalesManagementSystem.Services.Interfaces.IExcelExportService _excelExportService;

        public DonDatHangController(IDonDatHangRepository repo, DbConnectionFactory db, SalesManagementSystem.Services.Interfaces.IExcelExportService excelExportService)
        {
            _repo = repo;
            _db   = db;
            _excelExportService = excelExportService;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private class DropdownItem { public int ID { get; set; } public string Name { get; set; } }

        private SelectList GetNhanVienList(int? selectedId = null)
        {
            using (var conn = _db.CreateConnection())
            {
                var items = conn.Query<DropdownItem>(
                    "SELECT ID, ISNULL(MaNhanSu, '') + ' - ' + LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) AS Name FROM NS_NhanSu ORDER BY Ten").ToList();
                return new SelectList(items, "ID", "Name", selectedId);
            }
        }

        private SelectList GetKhachHangList(int? selectedId = null)
        {
            using (var conn = _db.CreateConnection())
            {
                var items = conn.Query<DropdownItem>(
                    "SELECT ID, ISNULL(MaKhachHang, '') + ' - ' + LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) AS Name FROM NS_KhachHang ORDER BY Ten").ToList();
                return new SelectList(items, "ID", "Name", selectedId);
            }
        }

        private SelectList GetTrangThaiList(int? selectedId = null)
        {
            var items = new[]
            {
                new { ID = 1, Name = "Chưa giao"      },
                new { ID = 2, Name = "Đang đi đường"  },
                new { ID = 3, Name = "Đã giao"        }
            };
            return new SelectList(items, "ID", "Name", selectedId);
        }

        private UserLoginViewModel GetCurrentUser()
            => (UserLoginViewModel)Session[CommonConstants.USER_SESSION];

        // ── Index / GetList ───────────────────────────────────────────────────

        public ActionResult Index(
            int page = 1, int pageSize = 15,
            string tuNgay = "", string denNgay = "",
            int? idKhachHang = null, int? idNhanVien = null,
            int? trangThai = null, string soDonHang = "")
        {
            int totalRecords;
            var list = _repo.GetPaged(page, pageSize,
                tuNgay, denNgay, idKhachHang, idNhanVien, trangThai, soDonHang,
                out totalRecords);

            var model = new PagedListViewModel<DonDatHangViewModel>
            {
                Items       = list,
                CurrentPage = page,
                PageSize    = pageSize,
                TotalRecords= totalRecords,
                ActionName  = "GetList"
            };

            ViewBag.Title      = "Danh sách đơn đặt hàng";
            ViewBag.TuNgay     = tuNgay;
            ViewBag.DenNgay    = denNgay;
            ViewBag.SoDonHang  = soDonHang;
            ViewBag.KhachHangs = GetKhachHangList(idKhachHang);
            ViewBag.NhanViens  = GetNhanVienList(idNhanVien);
            ViewBag.TrangThais = GetTrangThaiList(trangThai);

            if (Request.IsAjaxRequest())
                return PartialView("_DonDatHangList", model);

            return View("Index", model);
        }

        public ActionResult GetList(
            int page = 1, int pageSize = 15,
            string tuNgay = "", string denNgay = "",
            int? idKhachHang = null, int? idNhanVien = null,
            int? trangThai = null, string soDonHang = "")
        {
            int totalRecords;
            var list = _repo.GetPaged(page, pageSize,
                tuNgay, denNgay, idKhachHang, idNhanVien, trangThai, soDonHang,
                out totalRecords);

            var model = new PagedListViewModel<DonDatHangViewModel>
            {
                Items       = list,
                CurrentPage = page,
                PageSize    = pageSize,
                TotalRecords= totalRecords,
                ActionName  = "GetList"
            };

            return PartialView("_DonDatHangList", model);
        }

        // ── Create ────────────────────────────────────────────────────────────

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create()
        {
            var model = new DonDatHangCreateEditViewModel
            {
                NgayTaoDon   = DateTime.Now,
                TrangThaiDon = (int)TrangThaiDonHang.ChuaGiao,
                SoDonHang    = _repo.GenerateSoDonHang()
            };
            model.NhanVienList  = GetNhanVienList();
            model.TrangThaiList = GetTrangThaiList();

            ViewBag.Title = "Tạo đơn đặt hàng";
            return View("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create(DonDatHangCreateEditViewModel model, string chiTietsJson)
        {
            // Parse chi tiết từ JSON
            var chiTiets = ParseChiTiets(chiTietsJson);

            // Validate header
            if (!model.IDKhachHang.HasValue || model.IDKhachHang == 0)
                ModelState.AddModelError("IDKhachHang", "Vui lòng chọn khách hàng");

            if (string.IsNullOrWhiteSpace(model.SoDonHang))
            {
                model.SoDonHang = "AUTO"; // Sẽ sinh tự động trong Repository
                if (ModelState.ContainsKey("SoDonHang")) ModelState["SoDonHang"].Errors.Clear();
            }
            else if (_repo.CheckDuplicateSoDon(model.SoDonHang.Trim()))
                ModelState.AddModelError("SoDonHang", "Số đơn hàng đã tồn tại trong hệ thống");

            if (!model.IDNhanVien.HasValue || model.IDNhanVien == 0)
                ModelState.AddModelError("IDNhanVien", "Vui lòng chọn nhân viên phụ trách");

            // Validate chi tiết
            if (chiTiets == null || chiTiets.Count == 0)
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một sản phẩm vào đơn hàng");
            else
            {
                for (int i = 0; i < chiTiets.Count; i++)
                {
                    if (!chiTiets[i].IDSanPham.HasValue || chiTiets[i].IDSanPham == 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Vui lòng chọn sản phẩm");
                    if (chiTiets[i].DonGia < 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Đơn giá không được âm");
                    if (chiTiets[i].SoLuong < 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Số lượng không được âm");
                    if (chiTiets[i].ThueGTGT < 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Thuế GTGT không được âm");
                }
            }

            if (!ModelState.IsValid)
            {
                model.NhanVienList  = GetNhanVienList(model.IDNhanVien);
                model.TrangThaiList = GetTrangThaiList(model.TrangThaiDon);
                ViewBag.Title       = "Tạo đơn đặt hàng";
                ViewBag.ChiTietsJson = chiTietsJson;
                return View("Create", model);
            }

            var session  = GetCurrentUser();
            int userId   = session?.IDNhanSu ?? 0;
            NormalizeChiTiets(chiTiets);
            decimal tong = chiTiets.Sum(x => x.ThanhTienSauThue) - model.PhiBocXep;

            var header = new NS_DonDatHang
            {
                IDKhachHang     = model.IDKhachHang,
                NgayTaoDon      = model.NgayTaoDon ?? DateTime.Now,
                SoDonHang       = model.SoDonHang.Trim(),
                IDNhanVien      = model.IDNhanVien,
                ThoiHanGiaoHang = model.ThoiHanGiaoHang,
                TrangThaiDon    = model.TrangThaiDon,
                TongTien        = tong,
                PhiBocXep       = model.PhiBocXep,
                GhiChu          = model.GhiChu,
                NgayTao         = DateTime.Now,
                NguoiTao        = userId
            };

            var details = chiTiets.Select(c => new NS_DonDatHangChiTiet
            {
                IDSanPham       = c.IDSanPham,
                SoLuong         = c.SoLuong >= 0 ? c.SoLuong : 1,
                DonGia          = c.DonGia,
                ThanhTien       = c.ThanhTien,
                ThanhTienSauThue= c.ThanhTienSauThue,
                ThueGTGT        = c.ThueGTGT,
                IsHangKhuyenMai = c.IsHangKhuyenMai,
                GhiChu          = c.GhiChu
            }).ToList();

            _repo.Insert(header, details);
            TempData["ToastMessage"] = "Tạo đơn đặt hàng thành công!";
            TempData["ToastType"]    = "success";

            return RedirectToAction("Index");
        }

        // ── Edit ──────────────────────────────────────────────────────────────

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(int id)
        {
            var don = _repo.GetById(id);
            if (don == null) return HttpNotFound();

            var chiTiets = _repo.GetChiTietByDonId(id);

            // Lấy thông tin KH để hiển thị
            string maKH = "", tenKH = "", maST = "", diaChi = "", sdT = "";
            if (don.IDKhachHang.HasValue)
            {
                using (var conn = _db.CreateConnection())
                {
                    var kh = conn.QueryFirstOrDefault<dynamic>(
                        "SELECT MaKhachHang, ISNULL(HoDem,'') + ' ' + ISNULL(Ten,'') AS HoTen, MaSoThue, DiaChi, SoDienThoai FROM NS_KhachHang WHERE ID = @ID",
                        new { ID = don.IDKhachHang });
                    if (kh != null)
                    {
                        maKH   = kh.MaKhachHang ?? "";
                        tenKH  = kh.HoTen       ?? "";
                        maST   = kh.MaSoThue    ?? "";
                        diaChi = kh.DiaChi      ?? "";
                        sdT    = kh.SoDienThoai ?? "";
                    }
                }
            }

            var model = new DonDatHangCreateEditViewModel
            {
                ID              = don.ID,
                IDKhachHang     = don.IDKhachHang,
                MaKhachHang     = maKH,
                TenKhachHang    = tenKH,
                MaSoThue        = maST,
                DiaChi          = diaChi,
                SoDienThoai     = sdT,
                SoDonHang       = don.SoDonHang,
                NgayTaoDon      = don.NgayTaoDon,
                IDNhanVien      = don.IDNhanVien,
                ThoiHanGiaoHang = don.ThoiHanGiaoHang,
                TrangThaiDon    = don.TrangThaiDon,
                TongTien        = don.TongTien,
                PhiBocXep       = don.PhiBocXep,
                GhiChu          = don.GhiChu,
                ChiTiets        = chiTiets
            };
            model.NhanVienList  = GetNhanVienList(don.IDNhanVien);
            model.TrangThaiList = GetTrangThaiList(don.TrangThaiDon);

            ViewBag.Title        = "Chỉnh sửa đơn đặt hàng";
            ViewBag.ChiTietsJson = JsonConvert.SerializeObject(chiTiets);
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(DonDatHangCreateEditViewModel model, string chiTietsJson)
        {
            var oldDon = _repo.GetById(model.ID);
            if (oldDon == null) return HttpNotFound();
            if (oldDon.TrangThaiDon == 3) return new HttpStatusCodeResult(400, "Đơn hàng đã giao không được chỉnh sửa.");
            if (oldDon.TrangThaiDon == 4) return new HttpStatusCodeResult(400, "Đơn hàng đã hủy không được chỉnh sửa.");

            var chiTiets = ParseChiTiets(chiTietsJson);

            if (!model.IDKhachHang.HasValue || model.IDKhachHang == 0)
                ModelState.AddModelError("IDKhachHang", "Vui lòng chọn khách hàng");

            if (string.IsNullOrWhiteSpace(model.SoDonHang))
                ModelState.AddModelError("SoDonHang", "Vui lòng nhập số đơn hàng");
            else if (_repo.CheckDuplicateSoDon(model.SoDonHang.Trim(), model.ID))
                ModelState.AddModelError("SoDonHang", "Số đơn hàng đã tồn tại trong hệ thống");

            if (!model.IDNhanVien.HasValue || model.IDNhanVien == 0)
                ModelState.AddModelError("IDNhanVien", "Vui lòng chọn nhân viên phụ trách");

            if (chiTiets == null || chiTiets.Count == 0)
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một sản phẩm vào đơn hàng");
            else
            {
                for (int i = 0; i < chiTiets.Count; i++)
                {
                    if (!chiTiets[i].IDSanPham.HasValue || chiTiets[i].IDSanPham == 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Vui lòng chọn sản phẩm");
                    if (chiTiets[i].DonGia < 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Đơn giá không được âm");
                    if (chiTiets[i].SoLuong < 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Số lượng không được âm");
                    if (chiTiets[i].ThueGTGT < 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Thuế GTGT không được âm");
                }
            }

            if (!ModelState.IsValid)
            {
                model.NhanVienList   = GetNhanVienList(model.IDNhanVien);
                model.TrangThaiList  = GetTrangThaiList(model.TrangThaiDon);
                ViewBag.Title        = "Chỉnh sửa đơn đặt hàng";
                ViewBag.ChiTietsJson = chiTietsJson;
                return View("Edit", model);
            }

            var session = GetCurrentUser();
            int userId  = session?.IDNhanSu ?? 0;
            NormalizeChiTiets(chiTiets);
            decimal tong = chiTiets.Sum(x => x.ThanhTienSauThue) - model.PhiBocXep;

            var header = new NS_DonDatHang
            {
                ID              = model.ID,
                IDKhachHang     = model.IDKhachHang,
                NgayTaoDon      = model.NgayTaoDon ?? DateTime.Now,
                SoDonHang       = model.SoDonHang.Trim(),
                IDNhanVien      = model.IDNhanVien,
                ThoiHanGiaoHang = model.ThoiHanGiaoHang,
                TrangThaiDon    = model.TrangThaiDon,
                TongTien        = tong,
                PhiBocXep       = model.PhiBocXep,
                GhiChu          = model.GhiChu,
                NgayCapNhat     = DateTime.Now,
                NguoiCapNhat    = userId
            };

            var details = chiTiets.Select(c => new NS_DonDatHangChiTiet
            {
                ID              = c.ID,
                IDSanPham       = c.IDSanPham,
                SoLuong         = c.SoLuong >= 0 ? c.SoLuong : 1,
                DonGia          = c.DonGia,
                ThanhTien       = c.ThanhTien,
                ThanhTienSauThue= c.ThanhTienSauThue,
                ThueGTGT        = c.ThueGTGT,
                IsHangKhuyenMai = c.IsHangKhuyenMai,
                GhiChu          = c.GhiChu
            }).ToList();

            _repo.Update(header, details);
            TempData["ToastMessage"] = "Cập nhật đơn đặt hàng thành công!";
            TempData["ToastType"]    = "success";

            return RedirectToAction("Index");
        }

        // ── Delete ────────────────────────────────────────────────────────────

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Delete(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                var don = _repo.GetById(id.Value);
                if (don != null && don.TrangThaiDon == 3)
                {
                    return Json(new { success = false, message = "Không thể xóa đơn đặt hàng đã giao." });
                }
                _repo.Delete(id.Value);
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    var don = _repo.GetById(item);
                    if (don != null && don.TrangThaiDon == 3)
                    {
                        return Json(new { success = false, message = "Một số đơn đặt hàng đã giao, không thể xóa." });
                    }
                }
                foreach (var item in ids)
                    _repo.Delete(item);
            }
            return Json(new { success = true, message = "Xóa đơn đặt hàng thành công" });
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CancelOrder(int id)
        {
            var oldDon = _repo.GetById(id);
            if (oldDon == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            if (oldDon.TrangThaiDon == 3) return Json(new { success = false, message = "Không thể hủy đơn hàng đã giao." });
            if (oldDon.TrangThaiDon == 4) return Json(new { success = false, message = "Đơn hàng này đã bị hủy trước đó." });

            var session = GetCurrentUser();
            int userId = session?.IDNhanSu ?? 0;

            bool result = _repo.CancelOrder(id, userId);
            if (result)
                return Json(new { success = true, message = "Hủy đơn hàng thành công." });
            return Json(new { success = false, message = "Lỗi khi hủy đơn hàng." });
        }

        // ── Export Excel ──────────────────────────────────────────────────────

        public ActionResult ExportExcel(int id)
        {
            try
            {
                var don = _repo.GetById(id);
                if (don == null) return HttpNotFound();
                
                var chiTiets = _repo.GetChiTietByDonId(id);

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                string nguoiLapBieu = session != null ? (session.HoDem + " " + session.Ten).Trim() : "";
                if (string.IsNullOrEmpty(nguoiLapBieu)) nguoiLapBieu = session?.UserName ?? "";

                decimal totalSoLuong = 0;
                decimal totalThanhTien = 0;
                foreach (var ct in chiTiets)
                {
                    totalSoLuong += ct.SoLuong;
                    totalThanhTien += ct.ThanhTienSauThue;
                }
                decimal donGiaBocXep = don.PhiBocXep;
                string tenKhachHang = "";
                string soDienThoai = "";
                string diaChiGiaoHang = "";

                if (don.IDKhachHang.HasValue)
                {
                    using (var conn = _db.CreateConnection())
                    {
                        var kh = conn.QueryFirstOrDefault<SalesManagementSystem.Models.Entities.NS_KhachHang>(
                            "SELECT * FROM NS_KhachHang WHERE ID = @Id", new { Id = don.IDKhachHang.Value });
                        if (kh != null)
                        {
                            tenKhachHang = (kh.HoDem + " " + kh.Ten).Trim();
                            soDienThoai = kh.SoDienThoai;
                            diaChiGiaoHang = kh.DiaChi;
                        }
                    }
                }

                var variables = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "Ngay", DateTime.Now.ToString("dd") },
                    { "Thang", DateTime.Now.ToString("MM") },
                    { "Nam", DateTime.Now.ToString("yyyy") },
                    { "NguoiLapBieu", nguoiLapBieu },
                    { "SoDonHang", don.SoDonHang },
                    { "NgayTaoDon", don.NgayTaoDon?.ToString("dd/MM/yyyy") },
                    { "TenKhachHang", tenKhachHang },
                    { "DiaChiGiaoHang", diaChiGiaoHang },
                    { "SoDienThoai", soDienThoai },
                    { "TongSoLuong", totalSoLuong },
                    { "TongThanhTien", totalThanhTien },
                    { "PhiBocXep", don.PhiBocXep },
                    { "DonGiaBocXep", donGiaBocXep > 0 ? donGiaBocXep.ToString("N0"): "" },
                    { "TongTienThanhToan", totalThanhTien - don.PhiBocXep },
                    { "ThoiGianGiaoHang", don.ThoiHanGiaoHang?.ToString("dd/MM/yyyy") },
                    { "SoTienBangChu", SalesManagementSystem.Helpers.NumberToTextHelper.DocTienBangChu(totalThanhTien - don.PhiBocXep) }
                };

                // The prefix will be %DH01. since we use maBieuMau = "DH01"
                var exportData = chiTiets.Select((x, index) => new {
                    STT = index + 1,
                    TenSanPham = x.TenSanPham,
                    DVT = x.DVT,
                    QuyCach = "",
                    DonGia = x.DonGia,
                    SoLuong = x.SoLuong,
                    TongSLNhan = x.SoLuong,
                    ThanhTien = x.ThanhTien,
                    GhiChu = x.GhiChu
                });

                string fileExtension;
                // Assuming "DH01" is the template code for DonDatHang
                var fileBytes = _excelExportService.Export(BieuMauConstants.DS_CHI_TIET_DON_HANG, exportData, out fileExtension, variables);

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"DonDatHang_{don.SoDonHang}_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi xuất Excel: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }
        }

        public ActionResult SearchKhachHang(string q)
        {
            using (var conn = _db.CreateConnection())
            {
                string kw = (q ?? "").Trim().ToLower();
                string sql = @"
                    SELECT TOP 20
                        ID,
                        MaKhachHang,
                        ISNULL(HoDem,'') + ' ' + ISNULL(Ten,'') AS HoTen,
                        MaSoThue,
                        SoDienThoai,
                        DiaChi,
                        IDNhanVien
                    FROM NS_KhachHang
                    WHERE @KW = ''
                       OR LOWER(MaKhachHang) LIKE '%' + @KW + '%'
                       OR LOWER(ISNULL(HoDem,'') + ' ' + ISNULL(Ten,'')) LIKE '%' + @KW + '%'
                       OR LOWER(SoDienThoai)  LIKE '%' + @KW + '%'
                       OR LOWER(MaSoThue)     LIKE '%' + @KW + '%'
                    ORDER BY Ten";

                var rows = conn.Query(sql, new { KW = kw }).ToList();
                var result = rows.Select(r => new
                {
                    id        = r.ID,
                    text      = $"{r.MaKhachHang} - {r.HoTen}",
                    maKH      = r.MaKhachHang ?? "",
                    hoTen     = r.HoTen       ?? "",
                    maSoThue  = r.MaSoThue    ?? "",
                    sdt       = r.SoDienThoai ?? "",
                    diaChi    = r.DiaChi      ?? "",
                    idNhanVien= r.IDNhanVien
                });

                return Json(new { results = result }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SearchSanPham(string q)
        {
            using (var conn = _db.CreateConnection())
            {
                string kw = (q ?? "").Trim().ToLower();
                string sql = @"
                    SELECT TOP 20 ID, MaSanPham, TenSanPham, DVT
                    FROM DM_SanPham
                    WHERE @KW = ''
                       OR LOWER(MaSanPham)  LIKE '%' + @KW + '%'
                       OR LOWER(TenSanPham) LIKE '%' + @KW + '%'
                    ORDER BY TenSanPham";

                var rows = conn.Query(sql, new { KW = kw }).ToList();
                var result = rows.Select(r => new
                {
                    id         = r.ID,
                    text       = $"{r.MaSanPham} - {r.TenSanPham}",
                    maSanPham  = r.MaSanPham  ?? "",
                    tenSanPham = r.TenSanPham ?? "",
                    dvt        = r.DVT        ?? ""
                });

                return Json(new { results = result }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<DonDatHangChiTietViewModel> ParseChiTiets(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<DonDatHangChiTietViewModel>();
            try
            {
                return JsonConvert.DeserializeObject<List<DonDatHangChiTietViewModel>>(json)
                       ?? new List<DonDatHangChiTietViewModel>();
            }
            catch { return new List<DonDatHangChiTietViewModel>(); }
        }

        private void NormalizeChiTiets(List<DonDatHangChiTietViewModel> chiTiets)
        {
            foreach (var ct in chiTiets)
            {
                if (ct.SoLuong < 0) ct.SoLuong = 1;
                ct.ThanhTien = Math.Round(ct.DonGia * ct.SoLuong, 0);
                ct.ThanhTienSauThue = Math.Round(ct.ThanhTien + (ct.ThanhTien * ct.ThueGTGT / 100), 0);
            }
        }
    }
}
