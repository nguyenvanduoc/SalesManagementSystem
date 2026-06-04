using System;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;

namespace SalesManagementSystem.Controllers
{
    public class AclManHinhController : BaseController
    {
        private readonly IAclManHinhRepository _manHinhRepo;

        public AclManHinhController(IAclManHinhRepository manHinhRepo)
        {
            _manHinhRepo = manHinhRepo;
        }

        // ==========================================
        // QUẢN LÝ MÀN HÌNH
        // ==========================================

        // GET: AclManHinh/GetManHinh
        public ActionResult GetManHinh(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var manHinhs = _manHinhRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            ViewBag.Total = totalRecords;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalRecords > 0 ? (int)Math.Ceiling((double)totalRecords / pageSize) : 1;
            ViewBag.Keyword = keyword;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ManHinhList", manHinhs);
            }

            return View("GetManHinh", manHinhs);
        }

        // GET: AclManHinh/CreateManHinh
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreateManHinh()
        {
            return PartialView("CreateManHinh", new AclManHinh() { IsSuDung = 1 });
        }

        // POST: AclManHinh/CreateManHinh
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreateManHinh(AclManHinh manHinh)
        {
            if (ModelState.IsValid)
            {
                _manHinhRepo.Insert(manHinh);
                AuditLog.AddInsert("AclManHinh", manHinh.ID.ToString(), manHinh);
                return Json(new { success = true, message = "Thêm mới màn hình thành công!" });
            }
            return PartialView("CreateManHinh", manHinh);
        }

        // GET: AclManHinh/UpdateManHinh/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult UpdateManHinh(int id)
        {
            var manHinh = _manHinhRepo.GetById(id);
            if (manHinh == null)
            {
                return HttpNotFound();
            }
            return PartialView("UpdateManHinh", manHinh);
        }

        // POST: AclManHinh/UpdateManHinh/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult UpdateManHinh(AclManHinh manHinh)
        {
            if (ModelState.IsValid)
            {
                var oldManHinh = _manHinhRepo.GetById(manHinh.ID);
                _manHinhRepo.Update(manHinh);
                AuditLog.AddUpdate("AclManHinh", manHinh.ID.ToString(), oldManHinh, manHinh);
                return Json(new { success = true, message = "Cập nhật màn hình thành công!" });
            }
            return PartialView("UpdateManHinh", manHinh);
        }

        // POST: AclManHinh/DeleteManHinh
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult DeleteManHinh(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                var oldObj = _manHinhRepo.GetById(id.Value);
                _manHinhRepo.Delete(id.Value);
                if (oldObj != null) AuditLog.AddDelete("AclManHinh", id.Value.ToString(), oldObj);
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    var oldObj = _manHinhRepo.GetById(item);
                    _manHinhRepo.Delete(item);
                    if (oldObj != null) AuditLog.AddDelete("AclManHinh", item.ToString(), oldObj);
                }
            }
            return RedirectToAction("GetManHinh");
        }
    }
}
