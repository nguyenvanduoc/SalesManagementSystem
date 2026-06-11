using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Services.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class PhieuXuatKhoController : BaseController
    {
        private readonly IPhieuXuatKhoRepository _repo;
        private readonly IExcelExportService _excelExportService;
        private readonly INhanSuRepository _nhanSuRepo;

        public PhieuXuatKhoController(IPhieuXuatKhoRepository repo, IExcelExportService excelExportService, INhanSuRepository nhanSuRepo)
        {
            _repo = repo;
            _excelExportService = excelExportService;
            _nhanSuRepo = nhanSuRepo;
        }

        private UserLoginViewModel GetCurrentUser()
            => (UserLoginViewModel)Session[CommonConstants.USER_SESSION];

        private SelectList GetKhoList(int? selectedId = null)
        {
            var items = _repo.GetKhoForDropdown("").Select(x => new { ID = x.ID, Name = x.MaKhoHang + " - " + x.TenKhoHang }).ToList();
            return new SelectList(items, "ID", "Name", selectedId);
        }

        public ActionResult Index(
            int page = 1, int pageSize = 20,
            string tuNgay = "", string denNgay = "",
            string soChungTu = "", int? idKho = null,
            int? trangThai = null, int? idNhanSuNhan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            int totalRecords;
            var list = _repo.GetPaged(page, pageSize, tuNgay, denNgay, soChungTu, idKho, trangThai, idNhanSuNhan, out totalRecords);

            var model = new PagedListViewModel<PhieuXuatKhoListViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList"
            };

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.SoChungTu = soChungTu;
            ViewBag.Khos = GetKhoList(idKho);
            ViewBag.TrangThai = trangThai;

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_PhieuXuatKhoList", model);

            return View("Index", model);
        }

        [HttpGet]
        public ActionResult GetList(int page = 1, int pageSize = 20, string tuNgay = "", string denNgay = "", string soChungTu = "", int? idKho = null, int? trangThai = null, int? idNhanSuNhan = null)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try 
            {
                var list = _repo.GetPaged(page, pageSize, tuNgay, denNgay, soChungTu, idKho, trangThai, idNhanSuNhan, out int totalRecords);

                var model = new PagedListViewModel<PhieuXuatKhoListViewModel>
                {
                    Items = list,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList"
                };

                return PartialView("_PhieuXuatKhoList", model);
            }
            catch(Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi Server: {ex.Message} <br/> {ex.StackTrace}</div>");
            }
        }

        public ActionResult Create()
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Them)) return View("AccessDenied");

            var model = new PhieuXuatKhoViewModel();
            model.SoChungTu = _repo.GenerateSoChungTu();
            
            return View("Edit", model);
        }

        public ActionResult Edit(int id, bool isView = false)
        {
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            var entity = _repo.GetByID(id);
            if (entity == null) return HttpNotFound();

            entity.IsReadOnly = isView || entity.TrangThai == 2 || entity.TrangThai == 3;

            int total;
            var list = _repo.GetPaged(1, 1, null, null, entity.SoChungTu, null, null, null, out total);
            var item = list.FirstOrDefault();
            if (item != null)
            {
                entity.TenKho = item.TenKho;
                entity.TenNhanSuNhan = item.TenNhanSuNhan;
            }

            if (entity.IDNhanSuNhan.HasValue)
            {
                var ns = _nhanSuRepo.GetById(entity.IDNhanSuNhan.Value);
                if (ns != null)
                {
                    entity.TenNhanSuNhan = ns.MaNhanSu + " - " + ns.HoDem + " " + ns.Ten;
                }
            }

            entity.ChiTiets = _repo.GetChiTiet(id);
            ViewBag.IsView = isView;
            return View("Edit", entity);
        }

        [HttpPost]
        public ActionResult Save(PhieuXuatKhoViewModel model)
        {
            if (model.ID == 0 && !PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Them)) return Json(new { success = false, message = "Không có quyền thêm mới" });
            if (model.ID > 0 && !PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.CapNhat)) return Json(new { success = false, message = "Không có quyền sửa" });

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                if (model.ID > 0)
                {
                    _repo.Save(model, userId);
                    return Json(new { success = true, message = "Cập nhật phiếu xuất kho thành công" });
                }
                else
                {
                    int newId = _repo.Save(model, userId);
                    return Json(new { success = true, id = newId, soChungTu = model.SoChungTu, message = "Đề nghị xuất thành công" });
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
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền ghi sổ" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

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
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.TuyChon)) return Json(new { success = false, message = "Không có quyền hủy phiếu" });

            if (string.IsNullOrWhiteSpace(lyDoHuy))
            {
                return Json(new { success = false, message = "Vui lòng nhập lý do hủy" });
            }

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

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
            if (!PermissionHelper.HasPermission("PhieuXuatKho", LoaiPhanQuyen.Xoa)) return Json(new { success = false, message = "Không có quyền xóa" });

            try
            {
                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

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
        public ActionResult SearchNhanSu(string q)
        {
            var data = _nhanSuRepo.GetPaged(1, 20, q, null, out _);
            return Json(data.Select(x => new { 
                id = x.ID, 
                text = x.MaNhanSu + " - " + x.HoDem + " " + x.Ten,
                hoten = (x.HoDem + " " + x.Ten).Trim(),
                sdt = x.SoDienThoai
            }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchSanPham(string q)
        {
            var data = _repo.GetSanPhamForDropdown(q);
            return Json(data.Select(x => new { id = x.ID, text = x.MaSanPham + " - " + x.TenSanPham, dvt = x.DVT }), JsonRequestBehavior.AllowGet);
        }
    }
}
