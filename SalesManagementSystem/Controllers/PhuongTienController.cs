using System;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using System.Linq;

namespace SalesManagementSystem.Controllers
{
    public class PhuongTienController : BaseController
    {
        private readonly IPhuongTienRepository _phuongTienRepo;

        public PhuongTienController(IPhuongTienRepository phuongTienRepo)
        {
            _phuongTienRepo = phuongTienRepo;
        }

        // ==========================================
        // QUẢN LÝ PHƯƠNG TIỆN
        // ==========================================

        // GET: PhuongTien/GetPhuongTien
        public ActionResult GetPhuongTien(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var phuongTiens = _phuongTienRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<PhuongTien>
            {
                Items = phuongTiens,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetPhuongTien"
            };

            ViewBag.Keyword = keyword;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_PhuongTienList", model);
            }

            return View("GetPhuongTien", model);
        }

        // GET: PhuongTien/CreatePhuongTien
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreatePhuongTien()
        {
            return PartialView("CreatePhuongTien", new PhuongTien());
        }

        // POST: PhuongTien/CreatePhuongTien
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreatePhuongTien(PhuongTien phuongTien)
        {
            if (ModelState.IsValid)
            {
                if (_phuongTienRepo.IsDuplicateCode(phuongTien.MaPhuongTien))
                {
                    ModelState.AddModelError("MaPhuongTien", "Mã phương tiện đã tồn tại trong hệ thống.");
                    return PartialView("CreatePhuongTien", phuongTien);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                phuongTien.NguoiTao = session?.IDNhanSu ?? 0;
                _phuongTienRepo.Insert(phuongTien);

                // AUDIT LOG
                AuditLog.AddInsert("DM_PhuongTien", phuongTien.ID.ToString(), phuongTien);

                return Json(new { success = true, message = "Thêm mới phương tiện thành công!" });
            }
            return PartialView("CreatePhuongTien", phuongTien);
        }

        // GET: PhuongTien/UpdatePhuongTien/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult UpdatePhuongTien(int id)
        {
            var phuongTien = _phuongTienRepo.GetById(id);
            if (phuongTien == null)
            {
                return HttpNotFound();
            }
            return PartialView("UpdatePhuongTien", phuongTien);
        }

        // POST: PhuongTien/UpdatePhuongTien/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult UpdatePhuongTien(PhuongTien phuongTien)
        {
            if (ModelState.IsValid)
            {
                if (_phuongTienRepo.IsDuplicateCode(phuongTien.MaPhuongTien, phuongTien.ID))
                {
                    ModelState.AddModelError("MaPhuongTien", "Mã phương tiện đã tồn tại trong hệ thống.");
                    return PartialView("UpdatePhuongTien", phuongTien);
                }

                var oldPhuongTien = _phuongTienRepo.GetById(phuongTien.ID);

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                phuongTien.NguoiCapNhat = session?.IDNhanSu ?? 0;
                _phuongTienRepo.Update(phuongTien);

                // AUDIT LOG
                AuditLog.AddUpdate("DM_PhuongTien", phuongTien.ID.ToString(), oldPhuongTien, phuongTien);

                return Json(new { success = true, message = "Cập nhật phương tiện thành công!" });
            }
            return PartialView("UpdatePhuongTien", phuongTien);
        }

        // POST: PhuongTien/DeletePhuongTien
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult DeletePhuongTien(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                var oldObj = _phuongTienRepo.GetById(id.Value);
                if (oldObj != null)
                    AuditLog.AddDelete("DM_PhuongTien", id.Value.ToString(), oldObj);
                
                ForceSaveAudit();
                _phuongTienRepo.Delete(id.Value);
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    var oldObj = _phuongTienRepo.GetById(item);
                    if (oldObj != null)
                        AuditLog.AddDelete("DM_PhuongTien", item.ToString(), oldObj);
                    
                    ForceSaveAudit();
                    _phuongTienRepo.Delete(item);
                }
            }
            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }

        // GET: PhuongTien/ExportExcel
        public ActionResult ExportExcel(string keyword = "")
        {
            try
            {
                int totalRecords;
                var data = _phuongTienRepo.GetPaged(1, 10000, keyword, out totalRecords);

                int stt = 1;
                var exportData = data.Select(x => new
                {
                    STT = stt++,
                    ID = x.ID,
                    MaPhuongTien = x.MaPhuongTien,
                    TenPhuongTien = x.TenPhuongTien,
                    NgayTao = x.NgayTao.HasValue ? x.NgayTao.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    NgayCapNhat = x.NgayCapNhat.HasValue ? x.NgayCapNhat.Value.ToString("dd/MM/yyyy HH:mm") : ""
                });

                return ExportDanhMucToExcel("PT01", exportData, "Danh mục phương tiện", "DanhMucPhuongTien");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi xuất excel: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("GetPhuongTien", new { keyword = keyword });
            }
        }
    }
}
