using System.Web.Mvc;
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
        public ActionResult Index(int page = 1, int pageSize = 20, string keyword = "")
        {
            int totalRecords;
            var logs = _nhatKyRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            ViewBag.Total = totalRecords;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalRecords > 0 ? (int)Math.Ceiling((double)totalRecords / pageSize) : 1;
            ViewBag.Keyword = keyword;
            ViewBag.Title = "Nhật ký hệ thống";

            if (Request.IsAjaxRequest())
            {
                return PartialView("_NhatKyList", logs);
            }

            return View(logs);
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
