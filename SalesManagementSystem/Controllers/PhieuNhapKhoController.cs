using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class PhieuNhapKhoController : Controller
    {
        private readonly IPhieuNhapKhoRepository _repo;

        public PhieuNhapKhoController(IPhieuNhapKhoRepository repo)
        {
            _repo = repo;
        }

        private UserLoginViewModel GetCurrentUser()
            => (UserLoginViewModel)Session[CommonConstants.USER_SESSION];

        public ActionResult Index()
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) return RedirectToAction("AccessDenied", "Error");
            return View();
        }

        [HttpGet]
        public ActionResult GetList(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKho = null, int? idNhaCungCap = null, int? trangThai = null, int? idNhanSuNhan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try 
            {
                var list = _repo.GetPaged(page, pageSize, tuNgay, denNgay, soChungTu, idKho, idNhaCungCap, trangThai, idNhanSuNhan, out int totalRecords);

                var model = new PagedListViewModel<PhieuNhapKhoListViewModel>
                {
                    Items = list,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList"
                };

                return PartialView("_PhieuNhapKhoList", model);
            }
            catch(Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi Server: {ex.Message} <br/> {ex.StackTrace}</div>");
            }
        }

        public ActionResult Create()
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Them)) return RedirectToAction("AccessDenied", "Error");

            var model = new PhieuNhapKhoViewModel();
            model.SoChungTu = _repo.GenerateSoChungTu();
            
            return View("Edit", model);
        }

        public ActionResult Edit(int id, bool isView = false)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xem)) return RedirectToAction("AccessDenied", "Error");

            var entity = _repo.GetByID(id);
            if (entity == null) return HttpNotFound();

            var model = new PhieuNhapKhoViewModel
            {
                ID = entity.ID,
                SoChungTu = entity.SoChungTu,
                NgayNhap = entity.NgayNhap,
                IDKho = entity.IDKho,
                IDNhaCungCap = entity.IDNhaCungCap,
                SoHoaDon = entity.SoHoaDon,
                NgayHoaDon = entity.NgayHoaDon,
                TenNguoiGiao = entity.TenNguoiGiao,
                SoDienThoaiNguoiGiao = entity.SoDienThoaiNguoiGiao,
                IDNhanSuNhan = entity.IDNhanSuNhan,
                GhiChu = entity.GhiChu,
                TrangThai = entity.TrangThai,
                IsReadOnly = isView || entity.TrangThai == 2 || entity.TrangThai == 3
            };

            int total;
            var list = _repo.GetPaged(1, 1, null, null, entity.SoChungTu, null, null, null, null, out total);
            var item = list.FirstOrDefault();
            if (item != null)
            {
                model.TenKho = item.TenKho;
                model.TenNhaCungCap = item.TenNhaCungCap;
                model.TenNhanSuNhan = item.TenNhanSuNhan;
            }

            model.ChiTiets = _repo.GetChiTiet(id);
            ViewBag.IsView = isView;
            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Save(PhieuNhapKhoViewModel model)
        {
            if (model.ID == 0 && !PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Them)) return Json(new { success = false, message = "Không có quyền thêm mới" });
            if (model.ID > 0 && !PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.CapNhat)) return Json(new { success = false, message = "Không có quyền sửa" });

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var user = GetCurrentUser();
                int userId = user?.UserID ?? 0;

                if (model.ID > 0)
                {
                    _repo.Save(model, userId);
                    return Json(new { success = true, message = "Cập nhật phiếu nhập kho thành công" });
                }
                else
                {
                    int newId = _repo.Save(model, userId);
                    return Json(new { success = true, id = newId, soChungTu = model.SoChungTu, message = "Lưu nháp thành công" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult GhiSo(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền ghi sổ" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.UserID ?? 0;

                _repo.GhiSo(id, userId);
                return Json(new { success = true, message = "Ghi sổ thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult HuyPhieu(int id, string lyDoHuy)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền hủy phiếu" });

            if (string.IsNullOrWhiteSpace(lyDoHuy))
            {
                return Json(new { success = false, message = "Vui lòng nhập lý do hủy" });
            }

            try
            {
                var user = GetCurrentUser();
                int userId = user?.UserID ?? 0;

                _repo.HuyPhieu(id, lyDoHuy, userId);
                return Json(new { success = true, message = "Hủy phiếu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!PermissionHelper.HasPermission("PhieuNhapKho", LoaiPhanQuyen.Xoa)) return Json(new { success = false, message = "Không có quyền xóa" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.UserID ?? 0;

                _repo.Delete(id, userId);
                return Json(new { success = true, message = "Xóa phiếu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Dropdowns endpoints
        [HttpGet]
        public ActionResult SearchKhoHang(string q)
        {
            var data = _repo.GetKhoForDropdown(q);
            return Json(data.Select(x => new { id = x.ID, text = x.MaKhoHang + " - " + x.TenKhoHang }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchNhaCungCap(string q)
        {
            var data = _repo.GetNhaCungCapForDropdown(q);
            return Json(data.Select(x => new { id = x.ID, text = x.MaNhaCungCap + " - " + x.TenNhaCungCap }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchNhanSu(string q)
        {
            var data = _repo.GetNhanSuForDropdown(q);
            return Json(data.Select(x => new { id = x.ID, text = x.MaNhanSu + " - " + x.HoTen }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchSanPham(string q)
        {
            var data = _repo.GetSanPhamForDropdown(q);
            return Json(data.Select(x => new { id = x.ID, text = x.MaSanPham + " - " + x.TenSanPham, dvt = x.DVT }), JsonRequestBehavior.AllowGet);
        }
    }
}
