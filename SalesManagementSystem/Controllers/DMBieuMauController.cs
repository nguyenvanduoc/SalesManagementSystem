using System;
using System.IO;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class DMBieuMauController : BaseController
    {
        private readonly IDMBieuMauRepository _bieuMauRepo;

        public DMBieuMauController(IDMBieuMauRepository bieuMauRepo)
        {
            _bieuMauRepo = bieuMauRepo;
        }

        public ActionResult Index(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var list = _bieuMauRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<DMBieuMauViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "Index"
            };

            ViewBag.Keyword = keyword;
            ViewBag.Title = "Danh mục biểu mẫu";

            if (Request.IsAjaxRequest())
            {
                return PartialView("_BieuMauList", model);
            }

            return View(model);
        }

        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreateEdit(int id = 0)
        {
            var model = new DMBieuMauCreateEditViewModel();
            if (id > 0)
            {
                var bm = _bieuMauRepo.GetById(id);
                if (bm != null)
                {
                    model.ID = bm.ID;
                    model.MaBieuMau = bm.MaBieuMau;
                    model.TenBieuMau = bm.TenBieuMau;
                    model.TenFile = bm.TenFile;
                }
            }
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult CreateEdit(DMBieuMauCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var maBieuMau = model.MaBieuMau?.Trim();
                var tenBieuMau = model.TenBieuMau?.Trim();

                if (string.IsNullOrEmpty(maBieuMau) || string.IsNullOrEmpty(tenBieuMau))
                {
                    ModelState.AddModelError("", "Mã và Tên biểu mẫu không được để trống.");
                    return PartialView(model);
                }

                if (_bieuMauRepo.CheckDuplicateCode(maBieuMau, model.ID))
                {
                    ModelState.AddModelError("MaBieuMau", $"Mã biểu mẫu {maBieuMau} đã tồn tại trong hệ thống.");
                    return PartialView(model);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                int userId = session?.IDNhanSu ?? 0;

                var bieuMau = new DM_BieuMau
                {
                    ID = model.ID,
                    MaBieuMau = maBieuMau,
                    TenBieuMau = tenBieuMau
                };

                if (model.UploadedFile != null && model.UploadedFile.ContentLength > 0)
                {
                    bieuMau.TenFile = model.UploadedFile.FileName;
                    bieuMau.DuoiFile = Path.GetExtension(model.UploadedFile.FileName).Replace(".", "").ToUpper();
                    using (var reader = new BinaryReader(model.UploadedFile.InputStream))
                    {
                        bieuMau.NoiDung = reader.ReadBytes(model.UploadedFile.ContentLength);
                    }
                }
                else if (model.ID == 0)
                {
                    ModelState.AddModelError("UploadedFile", "Vui lòng chọn file biểu mẫu.");
                    return PartialView(model);
                }

                if (model.ID == 0)
                {
                    bieuMau.NgayTao = DateTime.Now;
                    bieuMau.NguoiTao = userId;
                    _bieuMauRepo.Insert(bieuMau);
                }
                else
                {
                    _bieuMauRepo.Update(bieuMau);
                }

                return Json(new { success = true, message = model.ID == 0 ? "Thêm mới thành công" : "Cập nhật thành công" });
            }

            return PartialView(model);
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Delete(int id)
        {
            var bm = _bieuMauRepo.GetById(id);
            if (bm != null)
            {
                _bieuMauRepo.Delete(id);
                return Json(new { success = true, message = "Xóa dữ liệu thành công" });
            }
            return Json(new { success = false, message = "Không tìm thấy biểu mẫu cần xóa" });
        }

        [HttpGet]
        public ActionResult Download(int id)
        {
            var bm = _bieuMauRepo.GetById(id);
            if (bm == null || bm.NoiDung == null)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Không tìm thấy file.";
                return RedirectToAction("Index");
            }

            string contentType = System.Web.MimeMapping.GetMimeMapping(bm.TenFile);
            return File(bm.NoiDung, contentType, bm.TenFile);
        }
    }
}
