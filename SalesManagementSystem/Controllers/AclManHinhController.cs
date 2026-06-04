using System;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;

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
            var list = _manHinhRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<AclManHinh>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetManHinh"
            };

            ViewBag.Keyword = keyword;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ManHinhList", model);
            }

            return View("GetManHinh", model);
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
                AuditLog.AddInsert("ACL_ManHinh", manHinh.ID.ToString(), manHinh);
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
                AuditLog.AddUpdate("ACL_ManHinh", manHinh.ID.ToString(), oldManHinh, manHinh);
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
                if (oldObj != null) AuditLog.AddDelete("ACL_ManHinh", id.Value.ToString(), oldObj);
                ForceSaveAudit();
                _manHinhRepo.Delete(id.Value);
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    var oldObj = _manHinhRepo.GetById(item);
                    if (oldObj != null) AuditLog.AddDelete("ACL_ManHinh", item.ToString(), oldObj);
                    ForceSaveAudit();
                    _manHinhRepo.Delete(item);
                }
            }
            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }
    }
}
