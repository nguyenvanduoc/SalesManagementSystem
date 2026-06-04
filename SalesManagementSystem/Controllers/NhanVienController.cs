using System;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Controllers
{
    public class NhanVienController : BaseController
    {
        private readonly INhanVienRepository _employeeRepo;

        public NhanVienController(INhanVienRepository employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        // GET: Employee
        public ActionResult Index(int page = 1, int pageSize = 10, string keyword = "", bool? gender = null)
        {
            int totalRecords;
            var nhanViens = _employeeRepo.GetPaged(page, pageSize, keyword, gender, out totalRecords);

            var model = new PagedListViewModel<NhanVien>
            {
                Items = nhanViens,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "Index"
            };

            ViewBag.Keyword = keyword;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_NhanVienList", model);
            }

            return View(model);
        }

        // GET: Employee/Create
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create()
        {
            return PartialView(new NhanVien());
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create([Bind(Exclude = "ID,NgayTao,NguoiTao,NgayCapNhat,NguoiCapNhat")] NhanVien employee)
        {
            if (ModelState.IsValid)
            {
                if (_employeeRepo.IsDuplicateCode(employee.MaNhanVien))
                {
                    ModelState.AddModelError("MaNhanVien", "Mã nhân viên đã tồn tại trong hệ thống.");
                    return PartialView(employee);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                employee.NguoiTao = session?.IDNhanVien ?? 0;
                _employeeRepo.Insert(employee);

                // AUDIT LOG
                AuditLog.AddInsert("NS_NhanVien", employee.ID.ToString(), employee);

                return Json(new { success = true, message = "Thêm mới nhân viên thành công!" });
            }
            return PartialView(employee);
        }

        // GET: Employee/Update/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Update(int id)
        {
            var employee = _employeeRepo.GetById(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return PartialView(employee);
        }

        // POST: Employee/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Update([Bind(Exclude = "NgayTao,NguoiTao,NgayCapNhat,NguoiCapNhat")] NhanVien employee)
        {
            if (ModelState.IsValid)
            {
                if (_employeeRepo.IsDuplicateCode(employee.MaNhanVien, employee.ID))
                {
                    ModelState.AddModelError("MaNhanVien", "Mã nhân viên đã tồn tại trong hệ thống.");
                    return PartialView(employee);
                }

                // FETCH OLD OBJ FOR AUDIT
                var oldEmployee = _employeeRepo.GetById(employee.ID);

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                employee.NguoiCapNhat = session?.IDNhanVien ?? 0;
                _employeeRepo.Update(employee);

                // AUDIT LOG
                AuditLog.AddUpdate("NS_NhanVien", employee.ID.ToString(), oldEmployee, employee);

                return Json(new { success = true, message = "Cập nhật nhân viên thành công!" });
            }
            return PartialView(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Delete(int id)
        {
            var oldEmployee = _employeeRepo.GetById(id);
            if (oldEmployee != null)
                AuditLog.AddDelete("NS_NhanVien", id.ToString(), oldEmployee);
            
            ForceSaveAudit();
            _employeeRepo.Delete(id);

            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }

        // POST: Employee/BatchDelete
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult BatchDelete(int[] ids)
        {
            if (ids != null && ids.Length > 0)
            {
                foreach (var id in ids)
                {
                    var oldEmployee = _employeeRepo.GetById(id);
                    if (oldEmployee != null)
                        AuditLog.AddDelete("NS_NhanVien", id.ToString(), oldEmployee);
                    
                    ForceSaveAudit();
                    _employeeRepo.Delete(id);
                }
            }
            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }
    }
}
