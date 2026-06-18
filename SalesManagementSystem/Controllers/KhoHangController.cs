using System;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Services.Interfaces;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class KhoHangController : BaseController
    {
        private readonly IDmKhoHangRepository _khoHangRepo;
        private readonly IExcelExportService _excelExportService;

        public KhoHangController(IDmKhoHangRepository khoHangRepo, IExcelExportService excelExportService)
        {
            _khoHangRepo = khoHangRepo;
            _excelExportService = excelExportService;
        }

        public ActionResult Index(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var list = _khoHangRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<DmKhoHangViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "Index"
            };

            ViewBag.Keyword = keyword;
            ViewBag.Title = "Danh mục kho hàng";

            if (Request.IsAjaxRequest())
            {
                return PartialView("_KhoHangList", model);
            }

            return View("Index", model);
        }

        public ActionResult GetList(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var list = _khoHangRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<DmKhoHangViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetList"
            };

            ViewBag.Keyword = keyword;

            return PartialView("_KhoHangList", model);
        }

        // GET: KhoHang/Create
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create()
        {
            return PartialView(new DmKhoHangCreateEditViewModel());
        }

        // POST: KhoHang/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create(DmKhoHangCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var maKhoHang = model.MaKhoHang?.Trim();
                var tenKhoHang = model.TenKhoHang?.Trim();

                if (string.IsNullOrEmpty(maKhoHang) || string.IsNullOrEmpty(tenKhoHang))
                {
                    ModelState.AddModelError("", "MÃ£ vÃ  TÃªn kho hÃ ng khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.");
                    return PartialView("Create", model);
                }

                if (_khoHangRepo.CheckDuplicateCode(maKhoHang, 0))
                {
                    ModelState.AddModelError("MaKhoHang", $"MÃ£ kho hÃ ng {maKhoHang} Ä‘Ã£ tá»“n táº¡i trong há»‡ thá»‘ng.");
                    return PartialView("Create", model);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                int userId = session?.IDNhanSu ?? 0;

                var kh = new DM_KhoHang
                {
                    MaKhoHang = maKhoHang,
                    TenKhoHang = tenKhoHang,
                    DiaChi = model.DiaChi,
                    NguoiDaiDien = model.NguoiDaiDien,
                    STT = model.STT,
                    NgayTao = DateTime.Now,
                    NguoiTao = userId
                };

                _khoHangRepo.Insert(kh);
                return Json(new { success = true, message = "ThÃªm má»›i thÃ nh cÃ´ng" });
            }
            return PartialView("Create", model);
        }

        // GET: KhoHang/Edit/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(int id)
        {
            var kh = _khoHangRepo.GetById(id);
            if (kh == null) return HttpNotFound();

            var model = new DmKhoHangCreateEditViewModel
            {
                ID = kh.ID,
                MaKhoHang = kh.MaKhoHang,
                TenKhoHang = kh.TenKhoHang,
                DiaChi = kh.DiaChi,
                NguoiDaiDien = kh.NguoiDaiDien,
                STT = kh.STT
            };
            return PartialView(model);
        }

        // POST: KhoHang/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(DmKhoHangCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var maKhoHang = model.MaKhoHang?.Trim();
                var tenKhoHang = model.TenKhoHang?.Trim();

                if (string.IsNullOrEmpty(maKhoHang) || string.IsNullOrEmpty(tenKhoHang))
                {
                    ModelState.AddModelError("", "MÃ£ vÃ  TÃªn kho hÃ ng khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.");
                    return PartialView("Edit", model);
                }

                if (_khoHangRepo.CheckDuplicateCode(maKhoHang, model.ID))
                {
                    ModelState.AddModelError("MaKhoHang", $"MÃ£ kho hÃ ng {maKhoHang} Ä‘Ã£ tá»“n táº¡i trong há»‡ thá»‘ng.");
                    return PartialView("Edit", model);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                int userId = session?.IDNhanSu ?? 0;

                var kh = new DM_KhoHang
                {
                    ID = model.ID,
                    MaKhoHang = maKhoHang,
                    TenKhoHang = tenKhoHang,
                    DiaChi = model.DiaChi,
                    NguoiDaiDien = model.NguoiDaiDien,
                    STT = model.STT,
                    NgayCapNhat = DateTime.Now,
                    NguoiCapNhat = userId
                };

                _khoHangRepo.Update(kh);
                return Json(new { success = true, message = "Cập nhật thành công" });
            }
            return PartialView("Edit", model);
        }

        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Delete(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                var kh = _khoHangRepo.GetById(id.Value);
                if (kh != null)
                {
                    _khoHangRepo.Delete(id.Value);
                }
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    var kh = _khoHangRepo.GetById(item);
                    if (kh != null)
                    {
                        _khoHangRepo.Delete(item);
                    }
                }
            }
            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }

        // GET: KhoHang/ExportExcel
        public ActionResult ExportExcel()
        {
            try
            {
                int total;
                var data = _khoHangRepo.GetPaged(1, 10000, "", out total);

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                string nguoiLapBieu = session != null ? (session.HoDem + " " + session.Ten).Trim() : "";
                if (string.IsNullOrEmpty(nguoiLapBieu)) nguoiLapBieu = session?.UserName ?? "";

                var variables = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "Ngay", DateTime.Now.ToString("dd") },
                    { "Thang", DateTime.Now.ToString("MM") },
                    { "Nam", DateTime.Now.ToString("yyyy") },
                    { "NguoiLapBieu", nguoiLapBieu }
                };

                int stt = 1;
                var exportData = data.Select(x => new {
                    STT = stt++,
                    MaKhoHang = x.MaKhoHang,
                    TenKhoHang = x.TenKhoHang,
                    TenNguoiTao = x.TenNguoiTao,
                    NgayTao = x.NgayTao.HasValue ? x.NgayTao.Value.ToString("dd/MM/yyyy HH:mm") : ""
                });

                string fileExtension;
                var fileBytes = _excelExportService.Export("KH02", exportData, out fileExtension, variables);

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"DanhSachKhoHang_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lá»—i xuáº¥t Excel: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }
        }
    }
}
