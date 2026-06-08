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

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class DonDatHangController : BaseController
    {
        private readonly IDonDatHangRepository _repo;
        private readonly DbConnectionFactory   _db;

        public DonDatHangController(IDonDatHangRepository repo, DbConnectionFactory db)
        {
            _repo = repo;
            _db   = db;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private class DropdownItem { public int ID { get; set; } public string Name { get; set; } }

        private SelectList GetNhanVienList(int? selectedId = null)
        {
            using (var conn = _db.CreateConnection())
            {
                var items = conn.Query<DropdownItem>(
                    "SELECT ID, HoTen AS Name FROM NS_NhanVien ORDER BY HoTen").ToList();
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
            return new SelectList(items, "ID", "Name", selectedId ?? 1);
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
                TrangThaiDon = (int)TrangThaiDonHang.ChuaGiao
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
                ModelState.AddModelError("SoDonHang", "Vui lòng nhập số đơn hàng");
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
            decimal tong = chiTiets.Sum(x => x.ThanhTien);

            var header = new NS_DonDatHang
            {
                IDKhachHang     = model.IDKhachHang,
                NgayTaoDon      = model.NgayTaoDon ?? DateTime.Now,
                SoDonHang       = model.SoDonHang.Trim(),
                IDNhanVien      = model.IDNhanVien,
                ThoiHanGiaoHang = model.ThoiHanGiaoHang,
                TrangThaiDon    = model.TrangThaiDon,
                TongTien        = tong,
                GhiChu          = model.GhiChu,
                NgayTao         = DateTime.Now,
                NguoiTao        = userId
            };

            var details = chiTiets.Select(c => new NS_DonDatHangChiTiet
            {
                IDSanPham       = c.IDSanPham,
                SoLuong         = c.SoLuong > 0 ? c.SoLuong : 1,
                DonGia          = c.DonGia,
                ThanhTien       = c.ThanhTien,
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
            decimal tong = chiTiets.Sum(x => x.ThanhTien);

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
                GhiChu          = model.GhiChu,
                NgayCapNhat     = DateTime.Now,
                NguoiCapNhat    = userId
            };

            var details = chiTiets.Select(c => new NS_DonDatHangChiTiet
            {
                IDSanPham       = c.IDSanPham,
                SoLuong         = c.SoLuong > 0 ? c.SoLuong : 1,
                DonGia          = c.DonGia,
                ThanhTien       = c.ThanhTien,
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
                _repo.Delete(id.Value);
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                    _repo.Delete(item);
            }
            return Json(new { success = true, message = "Xóa đơn đặt hàng thành công" });
        }

        // ── AJAX: SearchKhachHang (Select2) ───────────────────────────────────

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

        // ── AJAX: SearchSanPham (Select2) ─────────────────────────────────────

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

        // ── Private Helper ────────────────────────────────────────────────────

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
    }
}
