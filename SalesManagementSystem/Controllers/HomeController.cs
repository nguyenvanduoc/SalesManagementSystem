using System.Web.Mvc;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMenuRepository _menuRepo;

        // Unity inject MenuRepository qua interface
        public HomeController(IMenuRepository menuRepo)
        {
            _menuRepo = menuRepo;
        }

        public ActionResult Index()
        {
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
    }
}