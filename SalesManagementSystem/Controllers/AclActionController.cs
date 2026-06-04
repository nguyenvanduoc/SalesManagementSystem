using System;
using System.Web.Mvc;
using System.Linq;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;

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
        // QUẢN LÝ HÀNH ĐỘNG
        // ==========================================

        // GET: AclAction/GetAction
        public ActionResult GetAction(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var actions = _actionRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            ViewBag.Total = totalRecords;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalRecords > 0 ? (int)Math.Ceiling((double)totalRecords / pageSize) : 1;
            ViewBag.Keyword = keyword;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ActionList", actions);
            }

            return View("GetAction", actions);
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
        public ActionResult CreateAction(AclAction action)
        {
            if (ModelState.IsValid)
            {
                _actionRepo.Insert(action);
                return Json(new { success = true, message = "Thêm mới Action thành công!" });
            }
            ViewBag.ManHinhList = new SelectList(_manHinhRepo.GetAll().Where(m => m.IsSuDung == 1), "ID", "TenManHinh", action.IDManHinh);
            return PartialView("CreateAction", action);
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
        public ActionResult UpdateAction(AclAction action)
        {
            if (ModelState.IsValid)
            {
                _actionRepo.Update(action);
                return Json(new { success = true, message = "Cập nhật Action thành công!" });
            }
            ViewBag.ManHinhList = new SelectList(_manHinhRepo.GetAll().Where(m => m.IsSuDung == 1), "ID", "TenManHinh", action.IDManHinh);
            return PartialView("UpdateAction", action);
        }

        // POST: AclAction/DeleteAction
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult DeleteAction(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                _actionRepo.Delete(id.Value);
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    _actionRepo.Delete(item);
                }
            }
            return RedirectToAction("GetAction");
        }
    }
}
