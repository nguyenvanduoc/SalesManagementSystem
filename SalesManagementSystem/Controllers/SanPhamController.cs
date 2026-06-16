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
    public class SanPhamController : BaseController
    {
        private readonly IDmSanPhamRepository _sanPhamRepo;
        private readonly IExcelExportService _excelExportService;

        public SanPhamController(IDmSanPhamRepository sanPhamRepo, IExcelExportService excelExportService)
        {
            _sanPhamRepo = sanPhamRepo;
            _excelExportService = excelExportService;
        }

        public ActionResult Index(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var list = _sanPhamRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<DmSanPhamViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "Index"
            };

            ViewBag.Keyword = keyword;
            ViewBag.Title = "Danh mục sản phẩm";

            if (Request.IsAjaxRequest())
            {
                return PartialView("_SanPhamList", model);
            }

            return View("Index", model);
        }

        // GET: SanPham/GetList
        public ActionResult GetList(int page = 1, int pageSize = 10, string keyword = "")
        {
            int totalRecords;
            var list = _sanPhamRepo.GetPaged(page, pageSize, keyword, out totalRecords);

            var model = new PagedListViewModel<DmSanPhamViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetList"
            };

            ViewBag.Keyword = keyword;

            return PartialView("_SanPhamList", model);
        }

        // GET: SanPham/Create
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create()
        {
            return PartialView(new DmSanPhamCreateEditViewModel());
        }

        // POST: SanPham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create(DmSanPhamCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var maSanPham = model.MaSanPham?.Trim();
                var tenSanPham = model.TenSanPham?.Trim();

                if (string.IsNullOrEmpty(maSanPham) || string.IsNullOrEmpty(tenSanPham))
                {
                    ModelState.AddModelError("", "MÃ£ vÃ  TÃªn sáº£n pháº©m khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.");
                    return PartialView("Create", model);
                }

                if (_sanPhamRepo.CheckDuplicateCode(maSanPham, 0))
                {
                    ModelState.AddModelError("MaSanPham", $"MÃ£ sáº£n pháº©m {maSanPham} Ä‘Ã£ tá»“n táº¡i trong há»‡ thá»‘ng.");
                    return PartialView("Create", model);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                int userId = session?.IDNhanSu ?? 0;

                var sp = new DM_SanPham
                {
                    MaSanPham = maSanPham,
                    TenSanPham = tenSanPham,
                    DVT = model.DVT?.Trim(),
                    STT = model.STT,
                    NgayTao = DateTime.Now,
                    NguoiTao = userId
                };

                _sanPhamRepo.Insert(sp);
                return Json(new { success = true, message = "ThÃªm má»›i thÃ nh cÃ´ng" });
            }
            return PartialView("Create", model);
        }

        // GET: SanPham/Edit/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(int id)
        {
            var sp = _sanPhamRepo.GetById(id);
            if (sp == null) return HttpNotFound();

            var model = new DmSanPhamCreateEditViewModel
            {
                ID = sp.ID,
                MaSanPham = sp.MaSanPham,
                TenSanPham = sp.TenSanPham,
                DVT = sp.DVT,
                STT = sp.STT
            };
            return PartialView(model);
        }

        // POST: SanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(DmSanPhamCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var maSanPham = model.MaSanPham?.Trim();
                var tenSanPham = model.TenSanPham?.Trim();

                if (string.IsNullOrEmpty(maSanPham) || string.IsNullOrEmpty(tenSanPham))
                {
                    ModelState.AddModelError("", "MÃ£ vÃ  TÃªn sáº£n pháº©m khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.");
                    return PartialView("Edit", model);
                }

                if (_sanPhamRepo.CheckDuplicateCode(maSanPham, model.ID))
                {
                    ModelState.AddModelError("MaSanPham", $"MÃ£ sáº£n pháº©m {maSanPham} Ä‘Ã£ tá»“n táº¡i trong há»‡ thá»‘ng.");
                    return PartialView("Edit", model);
                }

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                int userId = session?.IDNhanSu ?? 0;

                var sp = new DM_SanPham
                {
                    ID = model.ID,
                    MaSanPham = maSanPham,
                    TenSanPham = tenSanPham,
                    DVT = model.DVT?.Trim(),
                    STT = model.STT,
                    NgayCapNhat = DateTime.Now,
                    NguoiCapNhat = userId
                };

                _sanPhamRepo.Update(sp);
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
                var sp = _sanPhamRepo.GetById(id.Value);
                if (sp != null)
                {
                    _sanPhamRepo.Delete(id.Value);
                }
            }
            else if (ids != null && ids.Length > 0)
            {
                foreach (var item in ids)
                {
                    var sp = _sanPhamRepo.GetById(item);
                    if (sp != null)
                    {
                        _sanPhamRepo.Delete(item);
                    }
                }
            }
            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }

        // GET: SanPham/ExportExcel
        public ActionResult ExportExcel()
        {
            try
            {
                int total;
                var data = _sanPhamRepo.GetPaged(1, 10000, "", out total);

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
                    MaSanPham = x.MaSanPham,
                    TenSanPham = x.TenSanPham,
                    TenNguoiTao = x.TenNguoiTao,
                    NgayTao = x.NgayTao.HasValue ? x.NgayTao.Value.ToString("dd/MM/yyyy HH:mm") : ""
                });

                string fileExtension;
                var fileBytes = _excelExportService.Export("SP01", exportData, out fileExtension, variables);

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"DanhSachSanPham_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
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
