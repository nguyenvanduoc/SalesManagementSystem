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

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "TÃªn Ä‘Äƒng nháº­p hoáº·c máº­t kháº©u khÃ´ng Ä‘Ãºng.");
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
            return RedirectToAction("Index", "Login");
        }
    }
}
