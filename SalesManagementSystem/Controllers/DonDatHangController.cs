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
                    "SELECT ID, ISNULL(MaKhachHang, '') + ' - ' + LTRIM(RTRIM(TenKhachHang)) AS Name FROM NS_KhachHang ORDER BY TenKhachHang").ToList();
                return new SelectList(items, "ID", "Name", selectedId);
            }
        }

        private SelectList GetTrangThaiList(int? selectedId = null)
        {
            var items = _repo.GetTrangThaiList().Select(x => new DropdownItem { ID = x.ID, Name = x.TenTrangThai }).ToList();
            return new SelectList(items, "ID", "Name", selectedId);
        }

        private UserLoginViewModel GetCurrentUser()
            => (UserLoginViewModel)Session[CommonConstants.USER_SESSION];

        // ── Index / GetList ───────────────────────────────────────────────────

        public ActionResult Index(
            int page = 1, int pageSize = 10,
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
            int page = 1, int pageSize = 10,
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
                TrangThaiDon = 1,
                SoDonHang    = _repo.GenerateSoDonHang()
            };
            model.NhanVienList  = GetNhanVienList();
            model.TrangThaiList = GetTrangThaiList();

            ViewBag.Title = "Tạo đơn đặt hàng";
            return View("Create", model);
        }

        public ActionResult Copy(int id)
        {
            if (!PermissionHelper.HasPermission("DonDatHang", LoaiPhanQuyen.Them)) return View("AccessDenied");

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
                        "SELECT MaKhachHang, TenKhachHang AS HoTen, MaSoThue, DiaChi, SoDienThoai FROM NS_KhachHang WHERE ID = @ID",
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
                ID              = 0,
                IDKhachHang     = don.IDKhachHang,
                MaKhachHang     = maKH,
                TenKhachHang    = tenKH,
                MaSoThue        = maST,
                DiaChi          = diaChi,
                SoDienThoai     = sdT,
                SoDonHang       = _repo.GenerateSoDonHang(),
                NgayTaoDon      = DateTime.Now,
                IDNhanVien      = don.IDNhanVien,
                ThoiHanGiaoHang = don.ThoiHanGiaoHang,
                TrangThaiDon    = 1, // Reset default status to 1 (do not copy old business status)
                TongTien        = don.TongTien,
                PhiBocXep       = don.PhiBocXep,
                ThanhTienHang   = don.ThanhTienHang ?? 0,
                ThanhTienThue   = don.ThanhTienThue ?? 0,
                GhiChu          = don.GhiChu
            };

            if (chiTiets != null)
            {
                model.ChiTiets = chiTiets.Select(c => new DonDatHangChiTietViewModel
                {
                    ID              = 0,
                    IDDonDatHang    = 0,
                    IDSanPham       = c.IDSanPham,
                    MaSanPham       = c.MaSanPham,
                    TenSanPham      = c.TenSanPham,
                    DVT             = c.DVT,
                    SoLuong         = c.SoLuong,
                    DonGia          = c.DonGia,
                    ThanhTien       = c.ThanhTien,
                    ThueGTGT        = c.ThueGTGT,
                    ThanhTienThue   = c.ThanhTienThue,
                    ThanhTienSauThue= c.ThanhTienSauThue,
                    IsHangKhuyenMai = c.IsHangKhuyenMai,
                    GhiChu          = c.GhiChu
                }).ToList();
            }
            else
            {
                model.ChiTiets = new List<DonDatHangChiTietViewModel>();
            }

            model.NhanVienList  = GetNhanVienList(don.IDNhanVien);
            model.TrangThaiList = GetTrangThaiList(1); // Set default status (1) select list item selected

            ViewBag.Title        = "Tạo đơn đặt hàng";
            ViewBag.ChiTietsJson = JsonConvert.SerializeObject(model.ChiTiets);
            ViewBag.IsView       = false;

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
            decimal thanhTienHang = chiTiets.Sum(x => x.ThanhTienHang ?? 0m);
            decimal phiBocXep = chiTiets.Sum(x => x.ThanhTienBocXep ?? 0m);
            decimal thanhTienThue = chiTiets.Sum(x => x.ThanhTienThue);
            decimal tong = chiTiets.Sum(x => x.ThanhTienSauThue);

            var header = new NS_DonDatHang
            {
                IDKhachHang     = model.IDKhachHang,
                NgayTaoDon      = model.NgayTaoDon ?? DateTime.Now,
                SoDonHang       = model.SoDonHang.Trim(),
                IDNhanVien      = model.IDNhanVien,
                ThoiHanGiaoHang = model.ThoiHanGiaoHang,
                TrangThaiDon    = model.TrangThaiDon,
                TongTien        = tong,
                PhiBocXep       = phiBocXep,
                ThanhTienHang   = thanhTienHang,
                ThanhTienThue   = thanhTienThue,
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
                ThanhTienThue   = c.ThanhTienThue,
                ThanhTienSauThue= c.ThanhTienSauThue,
                ThueGTGT        = c.ThueGTGT,
                IsHangKhuyenMai = c.IsHangKhuyenMai,
                GhiChu          = c.GhiChu,
                DonGiaBocXep    = c.DonGiaBocXep,
                ThanhTienBocXep = c.ThanhTienBocXep,
                ThanhTienHang   = c.ThanhTienHang
            }).ToList();

            _repo.Insert(header, details);
            
            if (Request.IsAjaxRequest() || Request.Headers["X-SPA-Load"] == "true") {
                return Json(new { success = true, message = "Tạo đơn đặt hàng thành công!", closeTab = true });
            }

            TempData["ToastMessage"] = "Tạo đơn đặt hàng thành công!";
            TempData["ToastType"]    = "success";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult GetDetailInline(int id)
        {
            if (!PermissionHelper.HasPermission("DonDatHang", LoaiPhanQuyen.Xem)) 
                return Content("<div class='alert alert-danger p-2 mb-0'>Không có quyền xem chi tiết</div>");

            var don = _repo.GetById(id);
            if (don == null) return HttpNotFound();

            var chiTiets = _repo.GetChiTietByDonId(id);

            var model = new DonDatHangCreateEditViewModel
            {
                ID              = don.ID,
                SoDonHang       = don.SoDonHang,
                TongTien        = don.TongTien,
                ChiTiets        = chiTiets
            };

            return PartialView("_DetailInline", model);
        }

        // â”€â”€ Edit â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(int id, bool isView = false)
        {
            var don = _repo.GetById(id);
            if (don == null) return HttpNotFound();

            var chiTiets = _repo.GetChiTietByDonId(id);

            // Láº¥y thÃ´ng tin KH Ä‘á»ƒ hiá»ƒn thá»‹
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
                ThanhTienHang   = don.ThanhTienHang ?? 0,
                ThanhTienThue   = don.ThanhTienThue ?? 0,
                GhiChu          = don.GhiChu,
                ChiTiets        = chiTiets
            };
            model.NhanVienList  = GetNhanVienList(don.IDNhanVien);
            model.TrangThaiList = GetTrangThaiList(don.TrangThaiDon);

            ViewBag.Title        = isView ? "Chi tiết đơn đặt hàng" : "Chỉnh sửa đơn đặt hàng";
            ViewBag.ChiTietsJson = JsonConvert.SerializeObject(chiTiets);
            ViewBag.IsView       = isView;
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
                ModelState.AddModelError("", "Vui lÃ²ng thÃªm Ã­t nháº¥t má»™t sáº£n pháº©m vÃ o Ä‘Æ¡n hÃ ng");
            else
            {
                for (int i = 0; i < chiTiets.Count; i++)
                {
                    if (!chiTiets[i].IDSanPham.HasValue || chiTiets[i].IDSanPham == 0)
                        ModelState.AddModelError("", $"Dòng {i + 1}: Vui lòng chọn sản phẩm");
                    if (chiTiets[i].DonGia < 0)
                        ModelState.AddModelError("", $"DÃ²ng {i + 1}: ÄÆ¡n giÃ¡ khÃ´ng Ä‘Æ°á»£c Ã¢m");
                    if (chiTiets[i].SoLuong < 0)
                        ModelState.AddModelError("", $"DÃ²ng {i + 1}: Sá»‘ lÆ°á»£ng khÃ´ng Ä‘Æ°á»£c Ã¢m");
                    if (chiTiets[i].ThueGTGT < 0)
                        ModelState.AddModelError("", $"DÃ²ng {i + 1}: Thuáº¿ GTGT khÃ´ng Ä‘Æ°á»£c Ã¢m");
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
            decimal thanhTienHang = chiTiets.Sum(x => x.ThanhTienHang ?? 0m);
            decimal phiBocXep = chiTiets.Sum(x => x.ThanhTienBocXep ?? 0m);
            decimal thanhTienThue = chiTiets.Sum(x => x.ThanhTienThue);
            decimal tong = chiTiets.Sum(x => x.ThanhTienSauThue);

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
                PhiBocXep       = phiBocXep,
                ThanhTienHang   = thanhTienHang,
                ThanhTienThue   = thanhTienThue,
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
                ThanhTienThue   = c.ThanhTienThue,
                ThanhTienSauThue= c.ThanhTienSauThue,
                ThueGTGT        = c.ThueGTGT,
                IsHangKhuyenMai = c.IsHangKhuyenMai,
                GhiChu          = c.GhiChu,
                DonGiaBocXep    = c.DonGiaBocXep,
                ThanhTienBocXep = c.ThanhTienBocXep,
                ThanhTienHang   = c.ThanhTienHang
            }).ToList();

            _repo.Update(header, details);
            
            if (Request.IsAjaxRequest() || Request.Headers["X-SPA-Load"] == "true") {
                return Json(new { success = true, message = "Cập nhật đơn đặt hàng thành công!", closeTab = true });
            }

            TempData["ToastMessage"] = "Cập nhật đơn đặt hàng thành công!";
            TempData["ToastType"]    = "success";

            return RedirectToAction("Index");
        }

        // â”€â”€ Delete â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
        public ActionResult UpdateStatus(int id, int trangThaiMoi)
        {
            var oldDon = _repo.GetById(id);
            if (oldDon == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            if (oldDon.TrangThaiDon == 3) return Json(new { success = false, message = "Không thể chuyển trạng thái đơn hàng đã giao." });
            if (oldDon.TrangThaiDon == 4) return Json(new { success = false, message = "Đơn hàng này đã bị hủy trước đó." });

            var session = GetCurrentUser();
            int userId = session?.IDNhanSu ?? 0;

            bool result = _repo.UpdateStatus(id, trangThaiMoi, userId);
            if (result)
            {
                var newDon = _repo.GetById(id);
                AuditLog.AddUpdate("NS_DonDatHang", id.ToString(), oldDon, newDon);
                AuditLog.SaveAudit(userId, "Đơn đặt hàng", "DonDatHangController", "UpdateStatus");
                return Json(new { success = true, message = "Chuyển trạng thái đơn hàng thành công." });
            }
            return Json(new { success = false, message = "Lỗi khi chuyển trạng thái đơn hàng." });
        }

        // ——— Export Excel ————————————————————————————————————————————————

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
                            tenKhachHang = (kh.TenKhachHang ?? "").Trim();
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
                    { "TongTienThanhToan", totalThanhTien },
                    { "ThoiGianGiaoHang", don.ThoiHanGiaoHang?.ToString("dd/MM/yyyy") },
                    { "SoTienBangChu", SalesManagementSystem.Helpers.NumberToTextHelper.DocTienBangChu(totalThanhTien) }
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

        public ActionResult ExportExcelList(
            string tuNgay = "", string denNgay = "",
            int? idKhachHang = null, int? idNhanVien = null,
            int? trangThai = null, string soDonHang = "")
        {
            try
            {
                int totalRecords;
                var list = _repo.GetPaged(1, int.MaxValue, tuNgay, denNgay, idKhachHang, idNhanVien, trangThai, soDonHang, out totalRecords);

                var variables = new Dictionary<string, object>
                {
                    { "TuNgay", tuNgay },
                    { "DenNgay", denNgay },
                    { "Ngay", DateTime.Now.ToString("dd") },
                    { "Thang", DateTime.Now.ToString("MM") },
                    { "Nam", DateTime.Now.ToString("yyyy") }
                };

                var exportData = list.Select((x, index) => new
                {
                    STT = index + 1,
                    SoDonHang = x.SoDonHang,
                    NgayLenDon = x.NgayTaoDon?.ToString("dd/MM/yyyy") ?? "",
                    MaKhachHang = x.MaKhachHang ?? "",
                    TenKhachHang = x.TenKhachHang ?? "",
                    NhanSuPhuTrach = x.TenNhanVien ?? "",
                    HanGiao = x.ThoiHanGiaoHang?.ToString("dd/MM/yyyy") ?? "",
                    TrangThai = x.TenTrangThai ?? "",
                    TongTien = x.TongTien
                }).ToList();

                string ext;
                var fileBytes = _excelExportService.Export("DH02", exportData, out ext, variables);

                string contentType = ext == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"DanhSachDonHang_{DateTime.Now:yyyyMMddHHmmss}.{ext}");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi xuất Excel: {ex.ToString()}");
            }
        }

        public ActionResult ExportExcelDH03(
            string tuNgay = "", string denNgay = "",
            int? idKhachHang = null, int? idNhanVien = null,
            int? trangThai = null, string soDonHang = "")
        {
            try
            {
                int totalRecords;
                var list = _repo.GetPaged(1, int.MaxValue, tuNgay, denNgay, idKhachHang, idNhanVien, trangThai, soDonHang, out totalRecords);

                decimal totalTongTien = list.Sum(x => x.TongTien);
                decimal totalPhiBocXep = 0m;

                List<SalesManagementSystem.Models.ViewModels.DonDatHangChiTietViewModel> allDetails = new List<SalesManagementSystem.Models.ViewModels.DonDatHangChiTietViewModel>();
                var orderIds = list.Select(x => x.ID).ToList();
                Dictionary<int, decimal> orderPhiBocXepDict = new Dictionary<int, decimal>();

                if (orderIds.Count > 0)
                {
                    using (var conn = _db.CreateConnection())
                    {
                        string sql = @"
                            SELECT
                                ct.ID, ct.IDDonDatHang, ct.IDSanPham,
                                sp.MaSanPham, sp.TenSanPham, sp.DVT,
                                ct.SoLuong, ct.DonGia, ct.ThueGTGT, ct.ThanhTien, ct.ThanhTienThue, ct.ThanhTienSauThue,
                                ct.IsHangKhuyenMai, ct.GhiChu
                            FROM NS_DonDatHangChiTiet ct
                            LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                            WHERE ct.IDDonDatHang IN @IDs
                            ORDER BY ct.ID";
                        allDetails = conn.Query<SalesManagementSystem.Models.ViewModels.DonDatHangChiTietViewModel>(sql, new { IDs = orderIds }).ToList();

                        totalPhiBocXep = conn.ExecuteScalar<decimal?>("SELECT SUM(PhiBocXep) FROM NS_DonDatHang WHERE ID IN @IDs", new { IDs = orderIds }) ?? 0m;
                        
                        var phiBocXeps = conn.Query("SELECT ID, PhiBocXep FROM NS_DonDatHang WHERE ID IN @IDs", new { IDs = orderIds });
                        foreach (var row in phiBocXeps)
                        {
                            orderPhiBocXepDict[(int)row.ID] = (decimal?)row.PhiBocXep ?? 0m;
                        }
                    }
                }

                var variables = new Dictionary<string, object>
                {
                    { "TuNgay", tuNgay },
                    { "DenNgay", denNgay },
                    { "Ngay", DateTime.Now.ToString("dd") },
                    { "Thang", DateTime.Now.ToString("MM") },
                    { "Nam", DateTime.Now.ToString("yyyy") },
                    { "TongTien", totalTongTien },
                    { "PhiBocXep", totalPhiBocXep },
                    { "TongTienBangChu", SalesManagementSystem.Helpers.NumberToTextHelper.DocTienBangChu(totalTongTien) }
                };

                var flatData = list.SelectMany(order =>
                {
                    var details = allDetails.Where(d => d.IDDonDatHang == order.ID).ToList();
                    string tenGroup = $"{order.SoDonHang} - {order.TenKhachHang}";
                    decimal phiBocXep = orderPhiBocXepDict.ContainsKey(order.ID) ? orderPhiBocXepDict[order.ID] : 0m;

                    if (details.Count == 0)
                    {
                        return new[] { new {
                            TenGroup = tenGroup,
                            STT = 1,
                            TenSanPham = "",
                            MaDonHang = "",
                            DVT = "",
                            SoLuong = 0m,
                            DonGia = 0m,
                            ThanhTienHang = 0m,
                            GhiChu = "",
                            TongTienTungGroup = order.TongTien,
                            PhiBocXepTungGroup = phiBocXep
                        } }.AsEnumerable();
                    }
                    return details.Select((d, i) => new {
                        TenGroup = tenGroup,
                        STT = i + 1,
                        TenSanPham = d.TenSanPham ?? "",
                        MaDonHang = d.TenSanPham ?? "",
                        DVT = d.DVT ?? "",
                        SoLuong = d.SoLuong,
                        DonGia = d.DonGia,
                        ThanhTienHang = d.ThanhTien,
                        GhiChu = d.GhiChu ?? "",
                        TongTienTungGroup = order.TongTien,
                        PhiBocXepTungGroup = phiBocXep
                    });
                }).ToList();

                var groupedData = flatData.GroupBy(x => x.TenGroup);

                string ext;
                var fileBytes = _excelExportService.ExportGrouped("DH03", groupedData, out ext, variables);

                string contentType = ext == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"DanhSachDonHangChiTiet_{DateTime.Now:yyyyMMddHHmmss}.{ext}");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi xuất Excel: {ex.ToString()}");
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
                        TenKhachHang AS HoTen,
                        MaSoThue,
                        SoDienThoai,
                        DiaChi,
                        IDNhanVien
                    FROM NS_KhachHang
                    WHERE @KW = ''
                       OR LOWER(MaKhachHang) LIKE '%' + @KW + '%'
                       OR LOWER(TenKhachHang) LIKE '%' + @KW + '%'
                       OR LOWER(SoDienThoai)  LIKE '%' + @KW + '%'
                       OR LOWER(MaSoThue)     LIKE '%' + @KW + '%'
                    ORDER BY TenKhachHang";

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
                
                ct.ThanhTienHang = Math.Round(ct.DonGia * ct.SoLuong, 0);
                ct.ThanhTienBocXep = Math.Round((ct.DonGiaBocXep ?? 0m) * ct.SoLuong, 0);
                ct.ThanhTien = Math.Round((ct.ThanhTienHang ?? 0m) - (ct.ThanhTienBocXep ?? 0m), 0);
                
                ct.ThanhTienThue = Math.Round(ct.ThanhTien * ct.ThueGTGT / 100, 0);
                ct.ThanhTienSauThue = Math.Round(ct.ThanhTien + ct.ThanhTienThue, 0);
            }
        }
    }
}
