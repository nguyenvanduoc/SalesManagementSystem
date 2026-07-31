using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAclLoginRepository _loginRepo;
        private readonly IAclLoginSessionRepository _sessionRepo;

        public LoginController(IAclLoginRepository loginRepo, IAclLoginSessionRepository sessionRepo)
        {
            _loginRepo = loginRepo;
            _sessionRepo = sessionRepo;
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hashPassword = Encryptor.MD5Hash(model.Password);
                var result = _loginRepo.Login(model.UserName, hashPassword);

                if (result != null)
                {
                    var userSession = new UserLoginViewModel
                    {
                        UserName = result.TenDangNhap,
                        UserID = result.ID,
                        HoDem = result.HoDem,
                        Ten = result.Ten,
                        IDNhanSu = result.IDNhanSu
                        
                    };

                    Session.Add(CommonConstants.USER_SESSION, userSession);

                    // LOG LOGIN SESSION
                    int sessionId = _sessionRepo.LogLogin(new SalesManagementSystem.Models.Entities.AclLoginSession
                    {
                        IDLogin = result.ID,
                        HoTen = result.HoDem + " " + result.Ten,
                        HostName = Request.UserHostName,
                        HostAddress = Request.UserHostAddress,
                        TrinhDuyet = Request.Browser != null ? Request.Browser.Browser + " " + Request.Browser.Version : "Unknown",
                        IP = Request.ServerVariables["REMOTE_ADDR"] ?? Request.UserHostAddress
                    });
                    Session["LoginSessionID"] = sessionId;

                    // Set persistent cookie to survive AppDomain restarts (IIS file changes) - Auto-login for 7 days
                    string userData = Newtonsoft.Json.JsonConvert.SerializeObject(userSession);
                    var ticket = new System.Web.Security.FormsAuthenticationTicket(1, result.TenDangNhap, System.DateTime.Now, System.DateTime.Now.AddDays(7), true, userData);
                    var encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(ticket);
                    var cookie = new System.Web.HttpCookie("SMS_AutoLogin", encryptedTicket) { HttpOnly = true, Expires = ticket.Expiration };
                    Response.Cookies.Add(cookie);

                    // Kiểm tra quyền Dashboard
                    bool hasDashboardPerm = SalesManagementSystem.Helpers.PermissionHelper.HasActionPermission("Dashboard", "Index") || SalesManagementSystem.Helpers.PermissionHelper.HasPermission("Dashboard", SalesManagementSystem.Helpers.LoaiPhanQuyen.Xem);
                    if (hasDashboardPerm)
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            var userSession = Session[CommonConstants.USER_SESSION] as UserLoginViewModel;
            if (userSession != null)
            {
                _sessionRepo.LogLogout(userSession.UserID);
            }
            Session.Remove(CommonConstants.USER_SESSION);

            var cookie = new System.Web.HttpCookie("SMS_AutoLogin", "") { Expires = System.DateTime.Now.AddDays(-1) };
            Response.Cookies.Add(cookie);

            return RedirectToAction("Index", "Login");
        }
    }
}
