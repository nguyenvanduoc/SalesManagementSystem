using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class BAN_TraHangBanController : BaseController
    {
        private readonly ITraHangBanRepository _repo;
        private readonly DbConnectionFactory _db;

        public BAN_TraHangBanController(ITraHangBanRepository repo, DbConnectionFactory db)
        {
            _repo = repo;
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
        
        private SelectList GetKhoList(int? selectedId = null)
        {
            using (var conn = _db.CreateConnection())
            {
                var items = conn.Query<DropdownItem>(
                    "SELECT ID, ISNULL(MaKhoHang, '') + ' - ' + LTRIM(RTRIM(TenKhoHang)) AS Name FROM DM_KhoHang ORDER BY TenKhoHang").ToList();
                return new SelectList(items, "ID", "Name", selectedId);
            }
        }
        
        private SelectList GetTrangThaiList(int? selectedId = null)
        {
            var items = new List<DropdownItem>
            {
                new DropdownItem { ID = 1, Name = "Lưu nháp" },
                new DropdownItem { ID = 2, Name = "Đã ghi" },
                new DropdownItem { ID = 3, Name = "Đã hủy" }
            };
            return new SelectList(items, "ID", "Name", selectedId);
        }
        
        private UserLoginViewModel GetCurrentUser()
            => (UserLoginViewModel)Session[CommonConstants.USER_SESSION];

        // ── Index / GetList ───────────────────────────────────────────────────
        
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Index(
            int page = 1, int pageSize = 10,
            string tuNgay = "", string denNgay = "",
            int? idKhachHang = null, int? trangThai = null, string soChungTu = "")
        {
            int totalRecords;
            var list = _repo.GetPaged(page, pageSize, tuNgay, denNgay, idKhachHang, trangThai, soChungTu, out totalRecords);

            var model = new PagedListViewModel<TraHangBanViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList"
            };

            ViewBag.Title = "Trả hàng bán";
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.SoChungTu = soChungTu;
            ViewBag.KhachHangs = GetKhachHangList(idKhachHang);
            ViewBag.TrangThais = GetTrangThaiList(trangThai);
            
            if (Request.IsAjaxRequest())
                return PartialView("_TraHangBanList", model);

            return View("Index", model);
        }

        public ActionResult GetList(
            int page = 1, int pageSize = 10,
            string tuNgay = "", string denNgay = "",
            int? idKhachHang = null, int? trangThai = null, string soChungTu = "")
        {
            int totalRecords;
            var list = _repo.GetPaged(page, pageSize, tuNgay, denNgay, idKhachHang, trangThai, soChungTu, out totalRecords);

            var model = new PagedListViewModel<TraHangBanViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList"
            };

            return PartialView("_TraHangBanList", model);
        }

        // ── Create ────────────────────────────────────────────────────────────

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create()
        {
            var model = new TraHangBanCreateEditViewModel
            {
                NgayChungTu = DateTime.Now,
                TrangThai = 1,
                SoChungTu = _repo.GenerateSoChungTu()
            };
            
            model.KhoList = GetKhoList();

            ViewBag.Title = "Lập phiếu trả hàng bán";
            return View("Create", model);
        }

        [HttpPost]
        public JsonResult Create(TraHangBanCreateEditViewModel model, bool ghiSo = false)
        {
            if (!PermissionHelper.HasPermission("BAN_TraHangBan", LoaiPhanQuyen.Them)) return Json(new { success = false, message = "Bạn không có quyền." });
            try
            {
                var traHang = new TraHangBan
                {
                    SoChungTu = model.SoChungTu,
                    NgayChungTu = model.NgayChungTu,
                    IDDonDatHang = model.IDDonDatHang,
                    IDKhachHang = model.IDKhachHang,
                    IDKho = model.IDKho,
                    LyDoTraHang = model.LyDoTraHang,
                    TongSoLuong = model.TongSoLuong ?? 0,
                    TongTienHang = model.TongTienHang ?? 0,
                    TongTienDaHoan = model.TongTienDaHoan ?? 0,
                    ConPhaiHoan = model.ConPhaiHoan ?? 0,
                    TrangThai = 1, // Luon la 1 (Luu nhap) khi moi tao
                    NguoiTao = GetCurrentUser()?.UserID ?? 0,
                    PhiBocXep = model.PhiBocXep ?? 0
                };

                var chiTiets = model.ChiTiets.Select(x => new TraHangBanChiTiet
                {
                    IDSanPham = x.IDSanPham,
                    SoLuongBan = x.SoLuongBan ?? 0,
                    SoLuongDaTra = x.SoLuongDaTra ?? 0,
                    SoLuongConLai = x.SoLuongConLai ?? 0,
                    SoLuongTra = x.SoLuongTra ?? 0,
                    DonGia = x.DonGia ?? 0,
                    ThanhTien = x.ThanhTien ?? 0,
                    GhiChu = x.GhiChu
                }).ToList();

                var newId = _repo.Insert(traHang, chiTiets);

                if (ghiSo)
                {
                    _repo.GhiSo(newId, GetCurrentUser()?.UserID ?? 0);
                }

                return Json(new { success = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        // ── Edit ────────────────────────────────────────────────────────────

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(int id)
        {
            var traHang = _repo.GetById(id);
            if (traHang == null) return HttpNotFound();
            
            var chiTiets = _repo.GetChiTietByTraHangId(id);
            
            var model = new TraHangBanCreateEditViewModel
            {
                ID = traHang.ID,
                SoChungTu = traHang.SoChungTu,
                NgayChungTu = traHang.NgayChungTu,
                IDDonDatHang = traHang.IDDonDatHang,
                SoDonHang = traHang.SoDonHang,
                IDKhachHang = traHang.IDKhachHang,
                TenKhachHang = traHang.TenKhachHang,
                MaKhachHang = traHang.MaKhachHang,
                IDKho = traHang.IDKho,
                LyDoTraHang = traHang.LyDoTraHang,
                TongSoLuong = traHang.TongSoLuong,
                TongTienHang = traHang.TongTienHang,
                TongTienDaHoan = traHang.TongTienDaHoan,
                ConPhaiHoan = traHang.ConPhaiHoan,
                TrangThai = traHang.TrangThai,
                DaThanhToan = traHang.DaThanhToan,
                TongTienDonHang = traHang.TongTienDonHang,
                PhiBocXep = traHang.PhiBocXep,
                ChiTiets = chiTiets.ToList()
            };
            
            model.KhoList = GetKhoList(model.IDKho);

            ViewBag.Title = "Cập nhật phiếu trả hàng bán";
            return View("Edit", model);
        }

        [HttpPost]
        public JsonResult Edit(TraHangBanCreateEditViewModel model, bool ghiSo = false)
        {
            if (!PermissionHelper.HasPermission("BAN_TraHangBan", LoaiPhanQuyen.CapNhat)) return Json(new { success = false, message = "Bạn không có quyền." });
            try
            {
                var traHang = new TraHangBan
                {
                    ID = model.ID,
                    SoChungTu = model.SoChungTu,
                    NgayChungTu = model.NgayChungTu,
                    IDDonDatHang = model.IDDonDatHang,
                    IDKhachHang = model.IDKhachHang,
                    IDKho = model.IDKho,
                    LyDoTraHang = model.LyDoTraHang,
                    TongSoLuong = model.TongSoLuong ?? 0,
                    TongTienHang = model.TongTienHang ?? 0,
                    TongTienDaHoan = model.TongTienDaHoan ?? 0,
                    ConPhaiHoan = model.ConPhaiHoan ?? 0,
                    NguoiCapNhat = GetCurrentUser()?.UserID ?? 0,
                    PhiBocXep = model.PhiBocXep ?? 0
                };

                var chiTiets = model.ChiTiets.Select(x => new TraHangBanChiTiet
                {
                    IDSanPham = x.IDSanPham,
                    SoLuongBan = x.SoLuongBan ?? 0,
                    SoLuongDaTra = x.SoLuongDaTra ?? 0,
                    SoLuongConLai = x.SoLuongConLai ?? 0,
                    SoLuongTra = x.SoLuongTra ?? 0,
                    DonGia = x.DonGia ?? 0,
                    ThanhTien = x.ThanhTien ?? 0,
                    GhiChu = x.GhiChu
                }).ToList();

                _repo.Update(traHang, chiTiets);

                if (ghiSo)
                {
                    _repo.GhiSo(model.ID, GetCurrentUser()?.UserID ?? 0);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── Delete, Ghi So, Huy ───────────────────────────────────────────────

        [HttpPost]
        public JsonResult Delete(int id)
        {
            if (!PermissionHelper.HasPermission("BAN_TraHangBan", LoaiPhanQuyen.Xoa)) return Json(new { success = false, message = "Bạn không có quyền." });
            try
            {
                _repo.Delete(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult GhiSo(int id)
        {
            if (!PermissionHelper.HasPermission("BAN_TraHangBan", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Bạn không có quyền." });
            try
            {
                var user = GetCurrentUser();
                _repo.GhiSo(id, user?.UserID ?? 0);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Huy(int id)
        {
            if (!PermissionHelper.HasPermission("BAN_TraHangBan", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Bạn không có quyền." });
            try
            {
                var user = GetCurrentUser();
                _repo.Huy(id, user?.UserID ?? 0);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        // ── Ajax Load Don Hang ────────────────────────────────────────────────

        public ActionResult LoadDonHangTra(string tuNgay = "", string denNgay = "", string soDonHang = "", int page = 1, int pageSize = 10)
        {
            int totalRecords;
            var list = _repo.LoadDonHangTra(tuNgay, denNgay, soDonHang, page, pageSize, out totalRecords);

            var model = new PagedListViewModel<TraHangBanViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "LoadDonHangTra"
            };

            return PartialView("_ChonDonHangPopup", model);
        }
        
        public JsonResult LoadChiTietDonHang(int idDonDatHang)
        {
            try
            {
                var list = _repo.LoadChiTietDonHang(idDonDatHang);
                return Json(new { success = true, data = list }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
