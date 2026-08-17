using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Helpers;
using System.Linq;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class NhaCungCapController : BaseController
    {
        private readonly INhaCungCapRepository _nhaCungCapRepo;

        public NhaCungCapController(INhaCungCapRepository nhaCungCapRepo)
        {
            _nhaCungCapRepo = nhaCungCapRepo;
        }

        // GET: NhaCungCap/Index
        public ActionResult Index(int page = 1, int pageSize = 10, string ma = "", string ten = "", string dt = "", string email = "")
        {
            int totalRecords;
            var list = _nhaCungCapRepo.GetPaged(page, pageSize, ma, ten, dt, email, out totalRecords);

            var model = new PagedListViewModel<NhaCungCapViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList"
            };

            ViewBag.Ma = ma;
            ViewBag.Ten = ten;
            ViewBag.Dt = dt;
            ViewBag.Email = email;
            ViewBag.Title = "Danh mục nhà cung cấp";

            if (Request.IsAjaxRequest() && Request.Headers["X-SPA-Load"] != "true")
            {
                return PartialView("_NhaCungCapList", model);
            }

            return View("Index", model);
        }

        // GET: NhaCungCap/GetList
        public ActionResult GetList(int page = 1, int pageSize = 10, string ma = "", string ten = "", string dt = "", string email = "")
        {
            int totalRecords;
            var list = _nhaCungCapRepo.GetPaged(page, pageSize, ma, ten, dt, email, out totalRecords);

            var model = new PagedListViewModel<NhaCungCapViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList"
            };

            ViewBag.Ma = ma;
            ViewBag.Ten = ten;
            ViewBag.Dt = dt;
            ViewBag.Email = email;

            return PartialView("_NhaCungCapList", model);
        }

        // GET: NhaCungCap/Create
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create()
        {
            ViewBag.Title = "Thêm mới nhà cung cấp";
            return PartialView("Create", new NhaCungCapCreateEditViewModel());
        }

        // POST: NhaCungCap/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Create(NhaCungCapCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_nhaCungCapRepo.CheckDuplicate(model.MaNhaCungCap, 0))
                {
                    ModelState.AddModelError("MaNhaCungCap", $"Mã nhà cung cấp '{model.MaNhaCungCap}' đã tồn tại trong hệ thống.");
                    return PartialView("Create", model);
                }

                var session = (UserLoginViewModel)Session[CommonConstants.USER_SESSION];
                int userId = session?.IDNhanSu ?? 0;

                var ncc = new DM_NhaCungCap
                {
                    MaNhaCungCap = model.MaNhaCungCap?.Trim(),
                    TenNhaCungCap = model.TenNhaCungCap?.Trim(),
                    SoDienThoai = model.DienThoai?.Trim(),
                    Email = model.Email?.Trim(),
                    DiaChi = model.DiaChi?.Trim(),
                    MaSoThue = model.MaSoThue?.Trim(),
                    NguoiTao = userId
                };

                _nhaCungCapRepo.Save(ncc);
                SalesManagementSystem.Helpers.CacheHelper.ClearAllDropdowns();
                return Json(new { success = true, message = "Thêm mới thành công" });
            }
            return PartialView("Create", model);
        }

        // GET: NhaCungCap/Edit/5
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(int id)
        {
            var ncc = _nhaCungCapRepo.GetById(id);
            if (ncc == null) return HttpNotFound();

            ViewBag.Title = "Cập nhật nhà cung cấp";
            var model = new NhaCungCapCreateEditViewModel
            {
                ID = ncc.ID,
                MaNhaCungCap = ncc.MaNhaCungCap,
                TenNhaCungCap = ncc.TenNhaCungCap,
                DienThoai = ncc.SoDienThoai,
                Email = ncc.Email,
                DiaChi = ncc.DiaChi,
                MaSoThue = ncc.MaSoThue
            };
            return PartialView("Edit", model);
        }

        // POST: NhaCungCap/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Edit(NhaCungCapCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_nhaCungCapRepo.CheckDuplicate(model.MaNhaCungCap, model.ID))
                {
                    ModelState.AddModelError("MaNhaCungCap", $"Mã nhà cung cấp '{model.MaNhaCungCap}' đã tồn tại trong hệ thống.");
                    return PartialView("Edit", model);
                }

                var ncc = new DM_NhaCungCap
                {
                    ID = model.ID,
                    MaNhaCungCap = model.MaNhaCungCap?.Trim(),
                    TenNhaCungCap = model.TenNhaCungCap?.Trim(),
                    SoDienThoai = model.DienThoai?.Trim(),
                    Email = model.Email?.Trim(),
                    DiaChi = model.DiaChi?.Trim(),
                    MaSoThue = model.MaSoThue?.Trim()
                };

                _nhaCungCapRepo.Save(ncc);
                SalesManagementSystem.Helpers.CacheHelper.ClearAllDropdowns();
                return Json(new { success = true, message = "Cập nhật thành công" });
            }
            return PartialView("Edit", model);
        }

        // POST: NhaCungCap/Delete
        [HttpPost]
        [CustomAuthorize(AuthorizeTypes.MustHavePermission)]
        public ActionResult Delete(int? id, int[] ids)
        {
            if (id.HasValue)
            {
                string message;
                bool success = _nhaCungCapRepo.Delete(id.Value, out message);
                if (!success)
                {
                    return Json(new { success = false, message = message });
                }
            }
            else if (ids != null && ids.Length > 0)
            {
                List<string> failedMessages = new List<string>();
                foreach (var item in ids)
                {
                    string message;
                    bool success = _nhaCungCapRepo.Delete(item, out message);
                    if (!success)
                    {
                        var ncc = _nhaCungCapRepo.GetById(item);
                        string name = ncc != null ? ncc.MaNhaCungCap : item.ToString();
                        failedMessages.Add($"{name}: {message}");
                    }
                }
                if (failedMessages.Count > 0)
                {
                    return Json(new { success = false, message = "Không thể xóa một số nhà cung cấp:<br/>" + string.Join("<br/>", failedMessages) });
                }
            }
            return Json(new { success = true, message = "Xóa dữ liệu thành công" });
        }

        // GET: NhaCungCap/SearchNhaCungCap (Dropdown Select2)
        public ActionResult SearchNhaCungCap(string q)
        {
            var data = _nhaCungCapRepo.GetForDropdown(q);
            return Json(data.Select(x => new { id = x.ID, text = x.MaNhaCungCap + " - " + x.TenNhaCungCap }), JsonRequestBehavior.AllowGet);
        }

        // GET: NhaCungCap/ExportExcel
        public ActionResult ExportExcel(string ma = "", string ten = "", string dt = "", string email = "")
        {
            try
            {
                int totalRecords;
                var data = _nhaCungCapRepo.GetPaged(1, 10000, ma, ten, dt, email, out totalRecords);

                int stt = 1;
                var exportData = data.Select(x => new
                {
                    STT = stt++,
                    MaNhaCungCap = x.MaNhaCungCap,
                    TenNhaCungCap = x.TenNhaCungCap,
                    DienThoai = x.DienThoai,
                    Email = x.Email,
                    MaSoThue = x.MaSoThue,
                    DiaChi = x.DiaChi,
                    NgayTao = x.NgayTao.HasValue ? x.NgayTao.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    TenNguoiTao = x.TenNguoiTao
                });

                return ExportDanhMucToExcel("NCC01", exportData, "Danh mục nhà cung cấp", "DanhMucNhaCungCap");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi xuất excel: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", new { ma = ma, ten = ten, dt = dt, email = email });
            }
        }
    }
}
