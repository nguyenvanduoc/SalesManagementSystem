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
            var users = _aclLoginRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<AclLoginViewModel>
            {
                Items = users,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetNguoiDung"
            };

            ViewBag.Keyword = keyword;
            ViewBag.Title = "Danh sÃ¡ch ngÆ°á»i dÃ¹ng";

            if (Request.IsAjaxRequest())
            {
                return PartialView("_NguoiDungList", model);
            }

            return View("GetNguoiDung", model);
        }

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreateNguoiDung(bool isManager = false)
        {
            var employees = _aclLoginRepo.GetEmployeesWithoutAccount();
            ViewBag.Employees = employees;
            ViewBag.IsManager = isManager;
            ViewBag.Title = isManager ? "ThÃªm Cáº¥p TrÃªn" : "ThÃªm Má»›i TÃ i Khoáº£n";
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
                        var tenDangNhap = emp.MaNhanSu;
                        var existingLogin = _aclLoginRepo.GetByEmployeeId(emp.ID);
                        
                        // Náº¿u Ä‘ang khÃ´i phá»¥c tÃ i khoáº£n, pháº£i truyá»n ID cá»§a tÃ i khoáº£n cÅ© vÃ o Ä‘á»ƒ bá» qua check trÃ¹ng láº·p vá»›i chÃ­nh nÃ³
                        if (!_aclLoginRepo.IsDuplicateUsername(tenDangNhap, existingLogin?.ID ?? 0))
                        {
                            var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                            if (existingLogin != null)
                            {
                                var oldLogin = _aclLoginRepo.GetById(existingLogin.ID);
                                existingLogin.TenDangNhap = tenDangNhap;
                                existingLogin.MatKhau = SecurityHelper.GetMd5Hash("1111");
                                existingLogin.IsActive = IsActive;
                                existingLogin.IDThamChieu = isManager ? null : IDThamChieu;
                                existingLogin.NgayCapNhat = DateTime.Now;
                                existingLogin.NguoiCapNhat = session?.IDNhanSu ?? 0;
                                existingLogin.NgayXoa = null;
                                existingLogin.NguoiXoa = null;
                                
                                _aclLoginRepo.Update(existingLogin);
                                AuditLog.AddUpdate("ACL_Login", existingLogin.ID.ToString(), oldLogin, existingLogin);
                            }
                            else
                            {
                                // ThÃªm má»›i hoÃ n toÃ n
                                var login = new AclLogin
                                {
                                    IDNhanSu = emp.ID,
                                    TenDangNhap = tenDangNhap,
                                    MatKhau = SecurityHelper.GetMd5Hash("1111"),
                                    HoDem = emp.HoDem,
                                    Ten = emp.Ten,
                                    IsActive = IsActive,
                                    IDThamChieu = isManager ? null : IDThamChieu,
                                    NguoiTao = session?.IDNhanSu ?? 0
                                };
                                _aclLoginRepo.Insert(login);
                                AuditLog.AddInsert("ACL_Login", login.ID.ToString(), login);
                            }
                        }
                    }
                }
                return Json(new { success = true, message = "ThÃªm má»›i tÃ i khoáº£n thÃ nh cÃ´ng! Máº­t kháº©u máº·c Ä‘á»‹nh lÃ  1234." });
            }
            
            ModelState.AddModelError("", "Vui lÃ²ng chá»n Ã­t nháº¥t má»™t nhÃ¢n sá»±.");
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
            
            var emp = _aclLoginRepo.GetEmployeeById(login.IDNhanSu);
            ViewBag.Ten = emp != null ? $"{emp.HoDem} {emp.Ten}" : "";
            ViewBag.Managers = _aclLoginRepo.GetManagers();
            
            // XÃ³a máº­t kháº©u khi hiá»ƒn thá»‹
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
                    ModelState.AddModelError("TenDangNhap", "TÃªn Ä‘Äƒng nháº­p Ä‘Ã£ tá»“n táº¡i trong há»‡ thá»‘ng.");
                    var emp = _aclLoginRepo.GetEmployeeById(login.IDNhanSu);
                    ViewBag.Ten = emp != null ? emp.Ten : "";
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
                    
                    var emp = _aclLoginRepo.GetEmployeeById(existing.IDNhanSu);
                    if (emp != null)
                    {
                        existing.HoDem = emp.HoDem;
                        existing.Ten = emp.Ten;
                    }
                    
                    var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                    existing.NguoiCapNhat = session?.IDNhanSu ?? 0;

                    var oldExisting = _aclLoginRepo.GetById(existing.ID);
                    _aclLoginRepo.Update(existing);
                    AuditLog.AddUpdate("ACL_Login", existing.ID.ToString(), oldExisting, existing);
                    return Json(new { success = true, message = "Cáº­p nháº­t tÃ i khoáº£n thÃ nh cÃ´ng!" });
                }
            }
            
            var empReload = _aclLoginRepo.GetEmployeeById(login.IDNhanSu);
            ViewBag.Ten = empReload != null ? empReload.Ten : "";
            return PartialView(login);
        }

        // POST: NguoiDung/DeleteNguoiDung/5
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult DeleteNguoiDung(int id)
        {
            var oldObj = _aclLoginRepo.GetById(id);
            var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
            int userId = session?.IDNhanSu ?? 0;

            if (oldObj != null) 
            {
                AuditLog.AddDelete("ACL_Login", id.ToString(), oldObj);
            }
            ForceSaveAudit();

            _aclLoginRepo.Delete(id, userId);
            
            return Json(new { success = true, message = "XÃ³a dá»¯ liá»‡u thÃ nh cÃ´ng" });
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult TransferManager(int id)
        {
            var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
            int userId = session?.IDNhanSu ?? 0;

            try
            {
                _aclLoginRepo.TransferManager(id, userId);
                return Json(new { success = true, message = "Chuyá»ƒn quyá»n cáº¥p trÃªn thÃ nh cÃ´ng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lá»—i: " + ex.Message });
            }
        }

        // GET: NguoiDung/ChangePassword
        public ActionResult ChangePassword()
        {
            return PartialView(new ChangePasswordViewModel());
        }

        // POST: NguoiDung/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var session = (UserLoginViewModel)Session[CommonConstants.USER_SESSION];
                if (session == null)
                {
                    return Json(new { success = false, message = "PhiÃªn Ä‘Äƒng nháº­p Ä‘Ã£ háº¿t háº¡n. Vui lÃ²ng Ä‘Äƒng nháº­p láº¡i." });
                }

                var user = _aclLoginRepo.GetById(session.UserID);
                if (user == null)
                {
                    return Json(new { success = false, message = "KhÃ´ng tÃ¬m tháº¥y thÃ´ng tin tÃ i khoáº£n." });
                }

                string hashedOldPassword = SecurityHelper.GetMd5Hash(model.OldPassword);
                if (user.MatKhau != hashedOldPassword)
                {
                    ModelState.AddModelError("OldPassword", "Máº­t kháº©u cÅ© khÃ´ng chÃ­nh xÃ¡c.");
                    return PartialView(model);
                }

                user.MatKhau = SecurityHelper.GetMd5Hash(model.NewPassword);
                
                var oldUser = _aclLoginRepo.GetById(user.ID);
                _aclLoginRepo.Update(user);
                AuditLog.AddUpdate("ACL_Login", user.ID.ToString(), oldUser, user);

                return Json(new { success = true, message = "Äá»•i máº­t kháº©u thÃ nh cÃ´ng!" });
            }

            return PartialView(model);
        }
    }
}
