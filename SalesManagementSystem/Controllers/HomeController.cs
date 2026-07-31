using System.Collections;
using System.Web;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IMenuRepository _menuRepo;

        // Unity inject MenuRepository qua interface
        public HomeController(IMenuRepository menuRepo)
        {
            _menuRepo = menuRepo;
        }

        public ActionResult Index()
        {
            if (PermissionHelper.HasPermission("Dashboard", LoaiPhanQuyen.Xem))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        /// <summary>
        /// Render sidebar menu động từ ACL_ManHinh + ACL_Action.
        /// [ChildActionOnly] = chỉ gọi được bằng @Html.Action(), không thể truy cập trực tiếp qua URL.
        /// </summary>
        [ChildActionOnly]
        public ActionResult Menu()
        {
            var groups = _menuRepo.GetSidebarGroups();

            var vm = new SidebarVM
            {
                Groups           = groups,
                ActiveController = RouteData.Values["controller"]?.ToString() ?? "",
                ActiveAction     = RouteData.Values["action"]?.ToString()     ?? ""
            };

            return PartialView("_Menu", vm);
        }

        public ActionResult ClearCache()
        {
            var enumerator = HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                HttpRuntime.Cache.Remove(enumerator.Key.ToString());
            }

            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult SearchMenu(string q)
        {
            try
            {
                var results = _menuRepo.SearchMenu(q);
                return Json(results, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult KeepAlive()
        {
            // Refresh session để tránh app pool idle timeout
            // Trả về trạng thái để client biết session còn sống
            var userSession = Session[CommonConstants.USER_SESSION] as UserLoginViewModel;
            if (userSession == null)
            {
                return Json(new { alive = false, message = "Session expired" });
            }

            // Cập nhật timestamp để session không bị coi là idle
            Session["KeepAliveAt"] = System.DateTime.Now;
            
            // Cập nhật thời gian hoạt động thực tế vào DB
            var sessionId = Session["LoginSessionID"];
            if (sessionId != null && (int)sessionId > 0)
            {
                var sessionRepo = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(SalesManagementSystem.Repositories.Interfaces.IAclLoginSessionRepository)) as SalesManagementSystem.Repositories.Interfaces.IAclLoginSessionRepository;
                if (sessionRepo != null)
                {
                    sessionRepo.UpdateLastActive((int)sessionId);
                }
            }
            
            return Json(new { alive = true, user = userSession.UserName });
        }
    }
}
