using System.Web.Mvc;
using SalesManagementSystem.Models.ViewModels;
using System;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;

namespace SalesManagementSystem.Controllers
{
    public class NhatKyController : BaseController
    {
        private readonly INKTongHopRepository _nhatKyRepo;

        public NhatKyController(INKTongHopRepository nhatKyRepo)
        {
            _nhatKyRepo = nhatKyRepo;
        }

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Index(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var logs = _nhatKyRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<NKTongHopViewModel>
            {
                Items = logs,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "Index"
            };

            ViewBag.Keyword = keyword;
            ViewBag.Title = "Nháº­t kÃ½ há»‡ thá»‘ng";

            if (Request.IsAjaxRequest())
            {
                return PartialView("_NhatKyList", model);
            }

            return View(model);
        }

        [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
        public ActionResult Detail(int id)
        {
            if (!PermissionHelper.HasActionPermission("NhatKy", "Index"))
            {
                if (Request.IsAjaxRequest()) return new HttpStatusCodeResult(403);
                return View("AccessDenied");
            }

            var log = _nhatKyRepo.GetById(id);
            if (log == null) return HttpNotFound();
            return PartialView(log);
        }
    }
}
