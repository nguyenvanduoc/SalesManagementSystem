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
            var cookie = Request.Cookies["SMS_AutoLogin"];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                try
                {
                    var ticket = System.Web.Security.FormsAuthentication.Decrypt(cookie.Value);
                    if (ticket != null && !ticket.Expired)
                    {
                        UserLoginViewModel userSession = null;
                        string cookieIp = null;
                        string cookieAgent = null;

                        if (!string.IsNullOrEmpty(ticket.UserData) && ticket.UserData.Contains("UserSession"))
                        {
                            var payload = Newtonsoft.Json.JsonConvert.DeserializeObject<AutoLoginCookiePayload>(ticket.UserData);
                            if (payload != null)
                            {
                                userSession = payload.UserSession;
                                cookieIp = payload.ClientIP;
                                cookieAgent = payload.UserAgent;
                            }
                        }
                        else if (!string.IsNullOrEmpty(ticket.UserData))
                        {
                            userSession = Newtonsoft.Json.JsonConvert.DeserializeObject<UserLoginViewModel>(ticket.UserData);
                        }

                        string currentIp = Request.ServerVariables["REMOTE_ADDR"] ?? Request.UserHostAddress ?? "";

                        // BẢO MẬT NÂNG CAO: Kiểm tra Địa chỉ IP đã khóa với Cookie (Chống copy Cookie sang Postman / máy khác)
                        if (!string.IsNullOrEmpty(cookieIp) && !string.Equals(cookieIp, currentIp, System.StringComparison.OrdinalIgnoreCase))
                        {
                            // IP không khớp => Hủy Cookie ngay lập tức
                            var invalidCookie = new System.Web.HttpCookie("SMS_AutoLogin", "") { Expires = System.DateTime.Now.AddDays(-1) };
                            Response.Cookies.Add(invalidCookie);
                            return View();
                        }

                        if (userSession != null && userSession.UserID > 0)
                        {
                            // AN TOÀN BẢO MẬT: Kiểm tra lại CSDL xem tài khoản có tồn tại và đang hoạt động (IsActive) hay không
                            var dbUser = _loginRepo.GetById(userSession.UserID);
                            if (dbUser != null && dbUser.IsActive == true)
                            {
                                userSession.UserName = dbUser.TenDangNhap;
                                userSession.HoDem = dbUser.HoDem;
                                userSession.Ten = dbUser.Ten;
                                userSession.IDNhanSu = dbUser.IDNhanSu;

                                Session.Add(CommonConstants.USER_SESSION, userSession);

                                int sessionId = _sessionRepo.LogLogin(new SalesManagementSystem.Models.Entities.AclLoginSession
                                {
                                    IDLogin = userSession.UserID,
                                    HoTen = ((userSession.HoDem ?? "") + " " + (userSession.Ten ?? "")).Trim(),
                                    HostName = Request.UserHostName,
                                    HostAddress = Request.UserHostAddress,
                                    TrinhDuyet = (Request.Browser != null ? Request.Browser.Browser + " " + Request.Browser.Version : "Unknown") + (string.IsNullOrEmpty(Request.UserAgent) ? "" : " | " + Request.UserAgent),
                                    IP = currentIp
                                });
                                Session["LoginSessionID"] = sessionId;

                                bool hasDashboardPerm = SalesManagementSystem.Helpers.PermissionHelper.HasActionPermission("Dashboard", "Index") || SalesManagementSystem.Helpers.PermissionHelper.HasPermission("Dashboard", SalesManagementSystem.Helpers.LoaiPhanQuyen.Xem);
                                if (hasDashboardPerm)
                                {
                                    return RedirectToAction("Index", "Dashboard");
                                }
                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore exception, fall through to remove cookie
                }

                // Nếu cookie hết hạn, bị lỗi mã hóa, hoặc tài khoản đã bị vô hiệu hóa trong CSDL => Hủy cookie ngay
                var expiredCookie = new System.Web.HttpCookie("SMS_AutoLogin", "") { Expires = System.DateTime.Now.AddDays(-1) };
                Response.Cookies.Add(expiredCookie);
            }

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

                    string currentIp = Request.ServerVariables["REMOTE_ADDR"] ?? Request.UserHostAddress ?? "";
                    string currentAgent = Request.UserAgent ?? "";

                    // LOG LOGIN SESSION (Explicit user login -> force new session)
                    int sessionId = _sessionRepo.LogLogin(new SalesManagementSystem.Models.Entities.AclLoginSession
                    {
                        IDLogin = result.ID,
                        HoTen = result.HoDem + " " + result.Ten,
                        HostName = Request.UserHostName,
                        HostAddress = Request.UserHostAddress,
                        TrinhDuyet = (Request.Browser != null ? Request.Browser.Browser + " " + Request.Browser.Version : "Unknown") + (string.IsNullOrEmpty(Request.UserAgent) ? "" : " | " + Request.UserAgent),
                        IP = currentIp
                    }, forceNew: true);
                    Session["LoginSessionID"] = sessionId;

                    // Set persistent cookie bound to IP & UserAgent for 7 days
                    var payload = new AutoLoginCookiePayload
                    {
                        UserSession = userSession,
                        ClientIP = currentIp,
                        UserAgent = currentAgent
                    };
                    string userData = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
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
