using System;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class NguoiDungController : Controller
    {
        private readonly IAclLoginRepository _aclLoginRepo;

        public NguoiDungController(IAclLoginRepository aclLoginRepo)
        {
            _aclLoginRepo = aclLoginRepo;
        }

        // GET: NguoiDung/GetNguoiDung
        public ActionResult GetNguoiDung(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var logins = _aclLoginRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            ViewBag.Total = totalRecords;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalRecords > 0 ? (int)Math.Ceiling((double)totalRecords / pageSize) : 1;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_NguoiDungList", logins);
            }

            return View(logins);
        }

        public ActionResult CreateNguoiDung()
        {
            var employees = _aclLoginRepo.GetEmployeesWithoutAccount();
            ViewBag.Employees = employees;
            return PartialView();
        }

        // POST: NguoiDung/CreateNguoiDung
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNguoiDung(int[] EmployeeIds, bool IsActive = true)
        {
            if (EmployeeIds != null && EmployeeIds.Length > 0)
            {
                foreach (var empId in EmployeeIds)
                {
                    var emp = _aclLoginRepo.GetEmployeeById(empId);
                    if (emp != null)
                    {
                        var tenDangNhap = emp.MaNhanVien;
                        // Bỏ qua nếu mã nhân viên đã được dùng làm tên đăng nhập
                        if (!_aclLoginRepo.IsDuplicateUsername(tenDangNhap))
                        {
                            var login = new AclLogin
                            {
                                IDNhanVien = emp.ID,
                                TenDangNhap = tenDangNhap,
                                MatKhau = SecurityHelper.GetMd5Hash("1234"),
                                HoDem = emp.HoDem,
                                Ten = emp.TenNhanVien,
                                IsActive = IsActive,
                                NguoiTao = 0 // Thay bằng UserId nếu có
                            };
                            _aclLoginRepo.Insert(login);
                        }
                    }
                }
                return Json(new { success = true, message = "Thêm mới tài khoản thành công! Mật khẩu mặc định là 1234." });
            }
            
            ModelState.AddModelError("", "Vui lòng chọn ít nhất một nhân viên.");
            var employeesReload = _aclLoginRepo.GetEmployeesWithoutAccount();
            ViewBag.Employees = employeesReload;
            return PartialView();
        }

        // GET: NguoiDung/EditNguoiDung/5
        public ActionResult EditNguoiDung(int id)
        {
            var login = _aclLoginRepo.GetById(id);
            if (login == null) return HttpNotFound();
            
            var emp = _aclLoginRepo.GetEmployeeById(login.IDNhanVien);
            ViewBag.TenNhanVien = emp != null ? $"{emp.HoDem} {emp.TenNhanVien}" : "";
            
            // Xóa mật khẩu khi hiển thị
            login.MatKhau = ""; 
            return PartialView(login);
        }

        // POST: NguoiDung/EditNguoiDung
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNguoiDung(AclLogin login)
        {
            if (ModelState.IsValid)
            {
                if (_aclLoginRepo.IsDuplicateUsername(login.TenDangNhap, login.ID))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại trong hệ thống.");
                    var emp = _aclLoginRepo.GetEmployeeById(login.IDNhanVien);
                    ViewBag.TenNhanVien = emp != null ? emp.TenNhanVien : "";
                    return PartialView(login);
                }

                var existing = _aclLoginRepo.GetById(login.ID);
                if (existing != null)
                {
                    existing.TenDangNhap = login.TenDangNhap;
                    existing.IsActive = login.IsActive;
                    
                    if (!string.IsNullOrEmpty(login.MatKhau))
                    {
                        existing.MatKhau = SecurityHelper.GetMd5Hash(login.MatKhau);
                    }
                    
                    var emp = _aclLoginRepo.GetEmployeeById(existing.IDNhanVien);
                    if (emp != null)
                    {
                        existing.HoDem = emp.HoDem;
                        existing.Ten = emp.TenNhanVien;
                    }
                    
                    _aclLoginRepo.Update(existing);
                    return Json(new { success = true, message = "Cập nhật tài khoản thành công!" });
                }
            }
            
            var empReload = _aclLoginRepo.GetEmployeeById(login.IDNhanVien);
            ViewBag.TenNhanVien = empReload != null ? empReload.TenNhanVien : "";
            return PartialView(login);
        }

        // POST: NguoiDung/DeleteNguoiDung/5
        [HttpPost]
        public ActionResult DeleteNguoiDung(int id)
        {
            _aclLoginRepo.Delete(id);
            return RedirectToAction("GetNguoiDung");
        }
    }
}
