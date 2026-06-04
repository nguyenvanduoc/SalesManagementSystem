using System;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Controllers
{
    public class ChucVuController : BaseController
    {
        private readonly IChucVuRepository _chucVuRepo;

        public ChucVuController(IChucVuRepository chucVuRepo)
        {
            _chucVuRepo = chucVuRepo;
        }

        // ==========================================
        // QUẢN LÝ CHỨC VỤ
        // ==========================================

        // GET: DanhMuc/GetChucVu
        public ActionResult GetChucVu(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var chucVus = _chucVuRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<ChucVu>
            {
                Items = chucVus,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetChucVu"
            };

            ViewBag.Keyword = keyword;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ChucVuList", model);
            }

            return View("GetChucVu", model);
        }

        // GET: DanhMuc/CreateChucVu
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreateChucVu()
        {
            return PartialView("CreateChucVu", new ChucVu());
        }

        // POST: DanhMuc/CreateChucVu
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreateChucVu(ChucVu chucVu)
        {
            if (ModelState.IsValid)
            {
                if (_chucVuRepo.IsDuplicateCode(chucVu.MaChucVu))
                {
                    ModelState.AddModelError("MaChucVu", "Mã chức vụ đã tồn tại trong hệ thống.");
                    return PartialView("CreateChucVu", chucVu);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                chucVu.NguoiTao = session?.IDNhanSu ?? 0;
                _chucVuRepo.Insert(chucVu);

                // AUDIT LOG
                AuditLog.AddInsert("DM_ChucVu", chucVu.ID.ToString(), chucVu);

                return Json(new { success = true, message = "Thêm mới chức vụ thành công!" });
            }
            return PartialView("CreateChucVu", chucVu);
        }

        // GET: DanhMuc/UpdateChucVu/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult UpdateChucVu(int id)
        {
            var chucVu = _chucVuRepo.GetById(id);
            if (chucVu == null)
            {
                return HttpNotFound();
            }
            return PartialView("UpdateChucVu", chucVu);
        }

        // POST: DanhMuc/UpdateChucVu/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult UpdateChucVu(ChucVu chucVu)
        {
            if (ModelState.IsValid)
            {
                if (_chucVuRepo.IsDuplicateCode(chucVu.MaChucVu, chucVu.ID))
                {
                    ModelState.AddModelError("MaChucVu", "Mã chức vụ đã tồn tại trong hệ thống.");
                    return PartialView("UpdateChucVu", chucVu);
                }

                var oldChucVu = _chucVuRepo.GetById(chucVu.ID);

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                chucVu.NguoiCapNhat = session?.IDNhanSu ?? 0;
                _chucVuRepo.Update(chucVu);

                // AUDIT LOG
                AuditLog.AddUpdate("DM_ChucVu", chucVu.ID.ToString(), oldChucVu, chucVu);

                return Json(new { success = true, message = "Cập nhật chức vụ thành công!" });
            }
            return PartialView("UpdateChucVu", chucVu);
        }

        // POST: DanhMuc/DeleteChucVu
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult DeleteChucVu(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                var oldObj = _chucVuRepo.GetById(id.Value);
                if (oldObj != null)
                    AuditLog.AddDelete("DM_ChucVu", id.Value.ToString(), oldObj);
                
                ForceSaveAudit();
                _chucVuRepo.Delete(id.Value);
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    var oldObj = _chucVuRepo.GetById(item);
                    if (oldObj != null)
                        AuditLog.AddDelete("DM_ChucVu", item.ToString(), oldObj);
                    
                    ForceSaveAudit();
                    _chucVuRepo.Delete(item);
                }
            }
            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }
    }
}
