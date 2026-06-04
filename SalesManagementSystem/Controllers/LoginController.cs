using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAclLoginRepository _loginRepo;

        public LoginController(IAclLoginRepository loginRepo)
        {
            _loginRepo = loginRepo;
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
            Session.Remove(CommonConstants.USER_SESSION);
            return RedirectToAction("Index", "Login");
        }
    }
}
