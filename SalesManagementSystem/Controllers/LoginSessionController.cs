using System.Web.Mvc;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;

namespace SalesManagementSystem.Controllers
{
    public class LoginSessionController : BaseController
    {
        private readonly IAclLoginSessionRepository _sessionRepo;

        public LoginSessionController(IAclLoginSessionRepository sessionRepo)
        {
            _sessionRepo = sessionRepo;
        }

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Index(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var list = _sessionRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<AclLoginSessionViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "Index"
            };

            ViewBag.Keyword = keyword;
            ViewBag.Title = "Lịch sử đăng nhập";

            if (Request.IsAjaxRequest())
            {
                return PartialView("_LoginSessionList", model);
            }

            return View(model);
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult KickUser(int id)
        {
            _sessionRepo.KickSession(id);
            return Json(new { success = true, message = "Đã ngắt phiên đăng nhập thành công." });
        }
    }
}
