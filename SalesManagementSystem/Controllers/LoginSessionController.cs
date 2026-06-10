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
            ViewBag.Title = "Lá»‹ch sá»­ Ä‘Äƒng nháº­p";

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
            return Json(new { success = true, message = "ÄÃ£ ngáº¯t phiÃªn Ä‘Äƒng nháº­p thÃ nh cÃ´ng." });
        }
    }
}
