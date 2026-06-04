using System;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;

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
            var employees = _employeeRepo.GetPaged(page, pageSize, keyword, gender, out totalRecords);

            ViewBag.Total = totalRecords;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalRecords > 0 ? (int)Math.Ceiling((double)totalRecords / pageSize) : 1;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_NhanVienList", employees);
            }

            return View(employees);
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
        public ActionResult Create(NhanVien employee)
        {
            if (ModelState.IsValid)
            {
                if (_employeeRepo.IsDuplicateCode(employee.MaNhanVien))
                {
                    ModelState.AddModelError("MaNhanVien", "Mã nhân viên đã tồn tại trong hệ thống.");
                    return PartialView(employee);
                }

                // Optionally set NguoiTao from user session/identity here
                var session = (SalesManagementSystem.Models.ViewModels.UserLogin)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                employee.NguoiTao = session?.UserID ?? 0;
                _employeeRepo.Insert(employee);
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
        public ActionResult Update(NhanVien employee)
        {
            if (ModelState.IsValid)
            {
                if (_employeeRepo.IsDuplicateCode(employee.MaNhanVien, employee.ID))
                {
                    ModelState.AddModelError("MaNhanVien", "Mã nhân viên đã tồn tại trong hệ thống.");
                    return PartialView(employee);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLogin)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                employee.NguoiCapNhat = session?.UserID ?? 0;
                _employeeRepo.Update(employee);
                return Json(new { success = true, message = "Cập nhật nhân viên thành công!" });
            }
            return PartialView(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Delete(int id)
        {
            _employeeRepo.Delete(id);
            return RedirectToAction("Index");
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
                    _employeeRepo.Delete(id);
                }
            }
            return RedirectToAction("Index");
        }
    }
}
