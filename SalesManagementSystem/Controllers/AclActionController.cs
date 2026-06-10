using System;
using System.Web.Mvc;
using System.Linq;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Controllers
{
    public class AclActionController : BaseController
    {
        private readonly IAclActionRepository _actionRepo;
        private readonly IAclManHinhRepository _manHinhRepo;

        public AclActionController(IAclActionRepository actionRepo, IAclManHinhRepository manHinhRepo)
        {
            _actionRepo = actionRepo;
            _manHinhRepo = manHinhRepo;
        }

        // ==========================================
        // QUáº¢N LÃ HÃ€NH Äá»˜NG
        // ==========================================

        // GET: AclAction/GetAction
        public ActionResult GetAction(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var list = _actionRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<AclAction>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetAction"
            };

            ViewBag.Keyword = keyword;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ActionList", model);
            }

            return View("GetAction", model);
        }

        // GET: AclAction/CreateAction
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreateAction()
        {
            ViewBag.ManHinhList = new SelectList(_manHinhRepo.GetAll().Where(m => m.IsSuDung == 1), "ID", "TenManHinh");
            return PartialView("CreateAction", new AclAction());
        }

        // POST: AclAction/CreateAction
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreateAction(AclAction aclAction)
        {
            if (ModelState.IsValid)
            {
                _actionRepo.Insert(aclAction);
                AuditLog.AddInsert("ACL_Action", aclAction.ID.ToString(), aclAction);
                return Json(new { success = true, message = "ThÃªm má»›i Action thÃ nh cÃ´ng!" });
            }
            ViewBag.ManHinhList = new SelectList(_manHinhRepo.GetAll().Where(m => m.IsSuDung == 1), "ID", "TenManHinh", aclAction.IDManHinh);
            return PartialView("CreateAction", aclAction);
        }

        // GET: AclAction/UpdateAction/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult UpdateAction(int id)
        {
            var action = _actionRepo.GetById(id);
            if (action == null)
            {
                return HttpNotFound();
            }
            ViewBag.ManHinhList = new SelectList(_manHinhRepo.GetAll().Where(m => m.IsSuDung == 1), "ID", "TenManHinh", action.IDManHinh);
            return PartialView("UpdateAction", action);
        }

        // POST: AclAction/UpdateAction/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult UpdateAction(AclAction aclAction)
        {
            if (ModelState.IsValid)
            {
                var oldAction = _actionRepo.GetById(aclAction.ID);
                _actionRepo.Update(aclAction);
                AuditLog.AddUpdate("ACL_Action", aclAction.ID.ToString(), oldAction, aclAction);
                return Json(new { success = true, message = "Cáº­p nháº­t Action thÃ nh cÃ´ng!" });
            }
            ViewBag.ManHinhList = new SelectList(_manHinhRepo.GetAll().Where(m => m.IsSuDung == 1), "ID", "TenManHinh", aclAction.IDManHinh);
            return PartialView("UpdateAction", aclAction);
        }

        // POST: AclAction/DeleteAction
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult DeleteAction(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                var oldObj = _actionRepo.GetById(id.Value);
                if (oldObj != null) AuditLog.AddDelete("ACL_Action", id.Value.ToString(), oldObj);
                ForceSaveAudit();
                _actionRepo.Delete(id.Value);
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    var oldObj = _actionRepo.GetById(item);
                    if (oldObj != null) AuditLog.AddDelete("ACL_Action", item.ToString(), oldObj);
                    ForceSaveAudit();
                    _actionRepo.Delete(item);
                }
            }
            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }
    }
}
