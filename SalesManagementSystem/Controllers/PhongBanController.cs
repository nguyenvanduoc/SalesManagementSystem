using System;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using System.Linq;

namespace SalesManagementSystem.Controllers
{
    public class PhongBanController : BaseController
    {
        private readonly IPhongBanRepository _phongBanRepo;

        public PhongBanController(IPhongBanRepository phongBanRepo)
        {
            _phongBanRepo = phongBanRepo;
        }

        // ==========================================
        // QUáº¢N LÃ PHÃ’NG BAN
        // ==========================================

        // GET: PhongBan/GetPhongBan
        public ActionResult GetPhongBan(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var phongBans = _phongBanRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<PhongBan>
            {
                Items = phongBans,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetPhongBan"
            };

            ViewBag.Keyword = keyword;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_PhongBanList", model);
            }

            return View("GetPhongBan", model);
        }

        // GET: PhongBan/CreatePhongBan
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreatePhongBan()
        {
            return PartialView("CreatePhongBan", new PhongBan());
        }

        // POST: PhongBan/CreatePhongBan
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreatePhongBan(PhongBan phongBan)
        {
            if (ModelState.IsValid)
            {
                if (_phongBanRepo.IsDuplicateCode(phongBan.MaPhongBan))
                {
                    ModelState.AddModelError("MaPhongBan", "Mã phòng ban đã tồn tại");
                    return PartialView("CreatePhongBan", phongBan);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                phongBan.NguoiTao = session?.IDNhanSu ?? 0;
                _phongBanRepo.Insert(phongBan);

                // AUDIT LOG
                AuditLog.AddInsert("DM_PhongBan", phongBan.ID.ToString(), phongBan);

                return Json(new { success = true, message = "Them dữ liệu thành công" });
            }
            return PartialView("CreatePhongBan", phongBan);
        }

        // GET: PhongBan/UpdatePhongBan/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult UpdatePhongBan(int id)
        {
            var phongBan = _phongBanRepo.GetById(id);
            if (phongBan == null)
            {
                return HttpNotFound();
            }
            return PartialView("UpdatePhongBan", phongBan);
        }

        // POST: PhongBan/UpdatePhongBan/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult UpdatePhongBan(PhongBan phongBan)
        {
            if (ModelState.IsValid)
            {
                if (_phongBanRepo.IsDuplicateCode(phongBan.MaPhongBan, phongBan.ID))
                {
                    ModelState.AddModelError("MaPhongBan", "Mã phòng ban đã tồn tại");
                    return PartialView("UpdatePhongBan", phongBan);
                }

                var oldPhongBan = _phongBanRepo.GetById(phongBan.ID);

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                phongBan.NguoiCapNhat = session?.IDNhanSu ?? 0;
                _phongBanRepo.Update(phongBan);

                // AUDIT LOG
                AuditLog.AddUpdate("DM_PhongBan", phongBan.ID.ToString(), oldPhongBan, phongBan);

                return Json(new { success = true, message = "Cập nhật phòng ban thành công!" });
            }
            return PartialView("UpdatePhongBan", phongBan);
        }

        // POST: PhongBan/DeletePhongBan
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult DeletePhongBan(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                var oldObj = _phongBanRepo.GetById(id.Value);
                if (oldObj != null)
                    AuditLog.AddDelete("DM_PhongBan", id.Value.ToString(), oldObj);
                
                ForceSaveAudit();
                _phongBanRepo.Delete(id.Value);
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    var oldObj = _phongBanRepo.GetById(item);
                    if (oldObj != null)
                        AuditLog.AddDelete("DM_PhongBan", item.ToString(), oldObj);
                    
                    ForceSaveAudit();
                    _phongBanRepo.Delete(item);
                }
            }
            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }

        // GET: PhongBan/ExportExcel
        public ActionResult ExportExcel(string keyword = "")
        {
            try
            {
                int totalRecords;
                var data = _phongBanRepo.GetPaged(1, 10000, keyword, out totalRecords);

                int stt = 1;
                var exportData = data.Select(x => new
                {
                    STT = stt++,
                    ID = x.ID,
                    MaPhongBan = x.MaPhongBan,
                    TenPhongBan = x.TenPhongBan,
                    NgayTao = x.NgayTao.HasValue ? x.NgayTao.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    NgayCapNhat = x.NgayCapNhat.HasValue ? x.NgayCapNhat.Value.ToString("dd/MM/yyyy HH:mm") : ""
                });

                return ExportDanhMucToExcel("PB01", exportData, "Danh mục phòng ban", "DanhMucPhongBan");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi xuất excel: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("GetPhongBan", new { keyword = keyword });
            }
        }
    }
}
