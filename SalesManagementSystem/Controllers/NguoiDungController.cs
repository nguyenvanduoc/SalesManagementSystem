using System;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Controllers
{
    public class NguoiDungController : BaseController
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

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreateNguoiDung(bool isManager = false)
        {
            var employees = _aclLoginRepo.GetEmployeesWithoutAccount();
            ViewBag.Employees = employees;
            ViewBag.IsManager = isManager;
            ViewBag.Title = isManager ? "Thêm Cấp Trên" : "Thêm Mới Tài Khoản";
            if (!isManager)
            {
                ViewBag.Managers = _aclLoginRepo.GetManagers();
            }
            return PartialView();
        }

        // POST: NguoiDung/CreateNguoiDung
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreateNguoiDung(int[] EmployeeIds, bool IsActive = true, bool isManager = false, int? IDThamChieu = null)
        {
            if (EmployeeIds != null && EmployeeIds.Length > 0)
            {
                foreach (var empId in EmployeeIds)
                {
                    var emp = _aclLoginRepo.GetEmployeeById(empId);
                    if (emp != null)
                    {
                        var tenDangNhap = emp.MaNhanVien;
                        var existingLogin = _aclLoginRepo.GetByEmployeeId(emp.ID);
                        
                        // Nếu đang khôi phục tài khoản, phải truyền ID của tài khoản cũ vào để bỏ qua check trùng lặp với chính nó
                        if (!_aclLoginRepo.IsDuplicateUsername(tenDangNhap, existingLogin?.ID ?? 0))
                        {
                            var session = (SalesManagementSystem.Models.ViewModels.UserLogin)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                            if (existingLogin != null)
                            {
                                // Tài khoản đã từng bị xóa mềm -> Khôi phục
                                existingLogin.TenDangNhap = tenDangNhap;
                                existingLogin.MatKhau = SecurityHelper.GetMd5Hash("1111");
                                existingLogin.IsActive = IsActive;
                                existingLogin.IDThamChieu = isManager ? null : IDThamChieu;
                                existingLogin.NgayCapNhat = DateTime.Now;
                                existingLogin.NguoiCapNhat = session?.UserID ?? 0;
                                existingLogin.NgayXoa = null;
                                existingLogin.NguoiXoa = null;
                                
                                _aclLoginRepo.Update(existingLogin);
                            }
                            else
                            {
                                // Thêm mới hoàn toàn
                                var login = new AclLogin
                                {
                                    IDNhanVien = emp.ID,
                                    TenDangNhap = tenDangNhap,
                                    MatKhau = SecurityHelper.GetMd5Hash("1111"),
                                    HoDem = emp.HoDem,
                                    Ten = emp.TenNhanVien,
                                    IsActive = IsActive,
                                    IDThamChieu = isManager ? null : IDThamChieu,
                                    NguoiTao = session?.UserID ?? 0
                                };
                                _aclLoginRepo.Insert(login);
                            }
                        }
                    }
                }
                return Json(new { success = true, message = "Thêm mới tài khoản thành công! Mật khẩu mặc định là 1234." });
            }
            
            ModelState.AddModelError("", "Vui lòng chọn ít nhất một nhân viên.");
            var employeesReload = _aclLoginRepo.GetEmployeesWithoutAccount();
            ViewBag.Employees = employeesReload;
            ViewBag.IsManager = isManager;
            if (!isManager)
            {
                ViewBag.Managers = _aclLoginRepo.GetManagers();
            }
            return PartialView();
        }

        // GET: NguoiDung/EditNguoiDung/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult EditNguoiDung(int id)
        {
            var login = _aclLoginRepo.GetById(id);
            if (login == null) return HttpNotFound();
            
            var emp = _aclLoginRepo.GetEmployeeById(login.IDNhanVien);
            ViewBag.TenNhanVien = emp != null ? $"{emp.HoDem} {emp.TenNhanVien}" : "";
            ViewBag.Managers = _aclLoginRepo.GetManagers();
            
            // Xóa mật khẩu khi hiển thị
            login.MatKhau = ""; 
            return PartialView(login);
        }

        // POST: NguoiDung/EditNguoiDung
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult EditNguoiDung(AclLogin login)
        {
            if (ModelState.IsValid)
            {
                if (_aclLoginRepo.IsDuplicateUsername(login.TenDangNhap, login.ID))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại trong hệ thống.");
                    var emp = _aclLoginRepo.GetEmployeeById(login.IDNhanVien);
                    ViewBag.TenNhanVien = emp != null ? emp.TenNhanVien : "";
                    ViewBag.Managers = _aclLoginRepo.GetManagers();
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
                    existing.IDThamChieu = login.IDThamChieu;
                    
                    var emp = _aclLoginRepo.GetEmployeeById(existing.IDNhanVien);
                    if (emp != null)
                    {
                        existing.HoDem = emp.HoDem;
                        existing.Ten = emp.TenNhanVien;
                    }
                    
                    var session = (SalesManagementSystem.Models.ViewModels.UserLogin)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                    existing.NguoiCapNhat = session?.UserID ?? 0;
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
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult DeleteNguoiDung(int id)
        {
            _aclLoginRepo.Delete(id);
            return RedirectToAction("GetNguoiDung");
        }

        // GET: NguoiDung/ChangePassword
        public ActionResult ChangePassword()
        {
            return PartialView(new ChangePasswordVM());
        }

        // POST: NguoiDung/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var session = (UserLogin)Session[CommonConstants.USER_SESSION];
                if (session == null)
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
                }

                var user = _aclLoginRepo.GetById(session.UserID);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin tài khoản." });
                }

                string hashedOldPassword = SecurityHelper.GetMd5Hash(model.OldPassword);
                if (user.MatKhau != hashedOldPassword)
                {
                    ModelState.AddModelError("OldPassword", "Mật khẩu cũ không chính xác.");
                    return PartialView(model);
                }

                user.MatKhau = SecurityHelper.GetMd5Hash(model.NewPassword);
                _aclLoginRepo.Update(user);

                return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
            }

            return PartialView(model);
        }
    }
}
