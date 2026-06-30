using System;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class DmKhoanMucChiController : BaseController
    {
        private readonly IDmKhoanMucChiRepository _khoanMucChiRepo;

        public DmKhoanMucChiController(IDmKhoanMucChiRepository khoanMucChiRepo)
        {
            _khoanMucChiRepo = khoanMucChiRepo;
        }

        // GET: DmKhoanMucChi
        public ActionResult Index(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var list = _khoanMucChiRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<DmKhoanMucChiViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "Index"
            };

            ViewBag.Keyword = keyword;
            ViewBag.Title = "Danh mục khoản mục chi";

            if (Request.IsAjaxRequest())
            {
                return PartialView("_List", model);
            }

            return View("Index", model);
        }

        // GET: DmKhoanMucChi/GetList (for AJAX refresh/search/pagination)
        public ActionResult GetList(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var list = _khoanMucChiRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<DmKhoanMucChiViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetList"
            };

            ViewBag.Keyword = keyword;

            return PartialView("_List", model);
        }

        // GET: DmKhoanMucChi/Create
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create()
        {
            var model = new DmKhoanMucChiCreateEditViewModel
            {
                IsHoatDong = true
            };
            return PartialView(model);
        }

        // POST: DmKhoanMucChi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create(DmKhoanMucChiCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var maKhoanMuc = model.MaKhoanMuc?.Trim();
                var tenKhoanMuc = model.TenKhoanMuc?.Trim();

                if (string.IsNullOrEmpty(maKhoanMuc) || string.IsNullOrEmpty(tenKhoanMuc))
                {
                    ModelState.AddModelError("", "Mã khoản mục và tên khoản mục không được để trống.");
                    return PartialView("Create", model);
                }

                if (_khoanMucChiRepo.CheckDuplicateCode(maKhoanMuc, 0))
                {
                    ModelState.AddModelError("MaKhoanMuc", string.Format("Mã khoản mục chi '{0}' đã tồn tại.", maKhoanMuc));
                    return PartialView("Create", model);
                }

                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                var entity = new DM_KhoanMucChi
                {
                    MaKhoanMuc = maKhoanMuc,
                    TenKhoanMuc = tenKhoanMuc,
                    IsHoatDong = model.IsHoatDong,
                    NgayTao = DateTime.Now,
                    NguoiTao = userId
                };

                int newId = _khoanMucChiRepo.Insert(entity);
                entity.ID = newId;

                // Ghi nhận Audit Log
                AuditLog.AddInsert("DM_KhoanMucChi", newId.ToString(), entity);

                return Json(new { success = true, message = "Thêm mới khoản mục chi thành công!" });
            }
            return PartialView("Create", model);
        }

        // GET: DmKhoanMucChi/Edit/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(int id)
        {
            var entity = _khoanMucChiRepo.GetById(id);
            if (entity == null) return HttpNotFound();

            var model = new DmKhoanMucChiCreateEditViewModel
            {
                ID = entity.ID,
                MaKhoanMuc = entity.MaKhoanMuc,
                TenKhoanMuc = entity.TenKhoanMuc,
                IsHoatDong = entity.IsHoatDong
            };
            return PartialView(model);
        }

        // POST: DmKhoanMucChi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(DmKhoanMucChiCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var maKhoanMuc = model.MaKhoanMuc?.Trim();
                var tenKhoanMuc = model.TenKhoanMuc?.Trim();

                if (string.IsNullOrEmpty(maKhoanMuc) || string.IsNullOrEmpty(tenKhoanMuc))
                {
                    ModelState.AddModelError("", "Mã khoản mục và tên khoản mục không được để trống.");
                    return PartialView("Edit", model);
                }

                if (_khoanMucChiRepo.CheckDuplicateCode(maKhoanMuc, model.ID))
                {
                    ModelState.AddModelError("MaKhoanMuc", string.Format("Mã khoản mục chi '{0}' đã tồn tại.", maKhoanMuc));
                    return PartialView("Edit", model);
                }

                var entity = _khoanMucChiRepo.GetById(model.ID);
                if (entity == null) return HttpNotFound();

                var oldEntity = new DM_KhoanMucChi
                {
                    ID = entity.ID,
                    MaKhoanMuc = entity.MaKhoanMuc,
                    TenKhoanMuc = entity.TenKhoanMuc,
                    IsHoatDong = entity.IsHoatDong,
                    NgayTao = entity.NgayTao,
                    NguoiTao = entity.NguoiTao,
                    NgayCapNhat = entity.NgayCapNhat,
                    NguoiCapNhat = entity.NguoiCapNhat
                };

                var user = GetCurrentUser();
                int userId = user?.IDNhanSu ?? 0;

                entity.MaKhoanMuc = maKhoanMuc;
                entity.TenKhoanMuc = tenKhoanMuc;
                entity.IsHoatDong = model.IsHoatDong;
                entity.NgayCapNhat = DateTime.Now;
                entity.NguoiCapNhat = userId;

                _khoanMucChiRepo.Update(entity);

                // Ghi nhận Audit Log
                AuditLog.AddUpdate("DM_KhoanMucChi", entity.ID.ToString(), oldEntity, entity);

                return Json(new { success = true, message = "Cập nhật khoản mục chi thành công!" });
            }
            return PartialView("Edit", model);
        }

        // POST: DmKhoanMucChi/Delete
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Delete(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                var oldObj = _khoanMucChiRepo.GetById(id.Value);
                if (oldObj != null)
                {
                    AuditLog.AddDelete("DM_KhoanMucChi", id.Value.ToString(), oldObj);
                    ForceSaveAudit();
                    _khoanMucChiRepo.Delete(id.Value);
                }
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    var oldObj = _khoanMucChiRepo.GetById(item);
                    if (oldObj != null)
                    {
                        AuditLog.AddDelete("DM_KhoanMucChi", item.ToString(), oldObj);
                        ForceSaveAudit();
                        _khoanMucChiRepo.Delete(item);
                    }
                }
            }
            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }
    }
}
