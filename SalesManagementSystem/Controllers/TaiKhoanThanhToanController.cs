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
    public class TaiKhoanThanhToanController : BaseController
    {
        private readonly ITaiKhoanThanhToanRepository _repo;
        private readonly ITaiKhoanKeToanRepository _taiKhoanKeToanRepo;

        public TaiKhoanThanhToanController(ITaiKhoanThanhToanRepository repo, ITaiKhoanKeToanRepository taiKhoanKeToanRepo)
        {
            _repo = repo;
            _taiKhoanKeToanRepo = taiKhoanKeToanRepo;
        }

        private void PopulateDropdowns()
        {
            var activeAccounts = _taiKhoanKeToanRepo.GetActive() ?? new List<KT_TaiKhoanKeToan>();
            ViewBag.TaiKhoanKeToanList = activeAccounts.Select(x => new SelectListItem
            {
                Value = x.ID.ToString(),
                Text = $"{x.SoTaiKhoan} - {x.TenTaiKhoan}"
            }).ToList();
        }

        public ActionResult Index(int page = 1, int pageSize = 10, string keyword = "", int? isHoatDong = null)
        {
            if (!PermissionHelper.HasPermission("TaiKhoanThanhToan", LoaiPhanQuyen.Xem))
                return RedirectToAction("Index", "Home");

            int totalRecords;
            var list = _repo.GetList(page, pageSize, keyword, isHoatDong, out totalRecords);

            var model = new PagedListViewModel<TaiKhoanThanhToanListViewModel>
            {
                Items = list,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Keyword = keyword,
                ActionName = "GetList"
            };

            ViewBag.Keyword = keyword;
            ViewBag.IsHoatDong = isHoatDong;
            ViewBag.Title = "Tài khoản thanh toán";

            if (Request.IsAjaxRequest())
            {
                return PartialView("_TaiKhoanThanhToanList", model);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult GetList(int page = 1, int pageSize = 10, string keyword = "", int? isHoatDong = null)
        {
            try
            {
                if (!PermissionHelper.HasPermission("TaiKhoanThanhToan", LoaiPhanQuyen.Xem))
                    return Content("Không có quyền truy cập");

                int totalRecords;
                var list = _repo.GetList(page, pageSize, keyword, isHoatDong, out totalRecords);

                var model = new PagedListViewModel<TaiKhoanThanhToanListViewModel>
                {
                    Items = list,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    Keyword = keyword,
                    ActionName = "GetList"
                };

                ViewBag.Keyword = keyword;
                ViewBag.IsHoatDong = isHoatDong;

                return PartialView("_TaiKhoanThanhToanList", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi 500: {ex.Message} <br/> {ex.StackTrace}</div>");
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            if (!PermissionHelper.HasPermission("TaiKhoanThanhToan", LoaiPhanQuyen.Them))
                return Content("Không có quyền thêm mới");

            ViewBag.Title = "Thêm mới Tài khoản thanh toán";
            PopulateDropdowns();
            var model = new TaiKhoanThanhToanViewModel { IsHoatDong = true };
            return PartialView("_Form", model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!PermissionHelper.HasPermission("TaiKhoanThanhToan", LoaiPhanQuyen.CapNhat))
                return Content("Không có quyền cập nhật");

            var model = _repo.GetByID(id);
            if (model == null) return Content("Không tìm thấy thông tin.");

            ViewBag.Title = "Cập nhật Tài khoản thanh toán";
            PopulateDropdowns();
            return PartialView("_Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(TaiKhoanThanhToanViewModel model)
        {
            try
            {
                // Kiểm tra quyền
                if (model.ID == 0 && !PermissionHelper.HasPermission("TaiKhoanThanhToan", LoaiPhanQuyen.Them))
                    return Json(new { success = false, message = "Bạn không có quyền thêm mới." });

                if (model.ID > 0 && !PermissionHelper.HasPermission("TaiKhoanThanhToan", LoaiPhanQuyen.CapNhat))
                    return Json(new { success = false, message = "Bạn không có quyền cập nhật." });

                // Kiểm tra trùng mã
                if (!string.IsNullOrEmpty(model.MaTaiKhoan) && _repo.IsDuplicateCode(model.MaTaiKhoan, model.ID))
                {
                    ModelState.AddModelError("MaTaiKhoan", "Mã tài khoản đã tồn tại trong hệ thống.");
                }

                if (ModelState.IsValid)
                {
                    int userId = GetCurrentUser()?.UserID ?? 0; // Derived from BaseController
                    _repo.Save(model, userId);
                    return Json(new { success = true, message = "Lưu thành công." });
                }
                
                PopulateDropdowns();
                return PartialView("_Form", model);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!PermissionHelper.HasPermission("TaiKhoanThanhToan", LoaiPhanQuyen.Xoa))
                return Json(new { success = false, message = "Bạn không có quyền xóa." });

            try
            {
                _repo.Delete(id);
                return Json(new { success = true, message = "Xóa dữ liệu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // GET: TaiKhoanThanhToan/ExportExcel
        public ActionResult ExportExcel(string keyword = "", int? isHoatDong = null)
        {
            try
            {
                int totalRecords;
                var data = _repo.GetList(1, 10000, keyword, isHoatDong, out totalRecords);

                int stt = 1;
                var exportData = data.Select(x => new
                {
                    STT = stt++,
                    MaTaiKhoan = x.MaTaiKhoan,
                    TenTaiKhoan = x.TenTaiKhoan,
                    SoTaiKhoan = x.SoTaiKhoan,
                    NganHang = x.NganHang,
                    ChuTaiKhoan = x.ChuTaiKhoan,
                    TaiKhoanKeToan = string.IsNullOrEmpty(x.SoTaiKhoanKeToan) ? "" : x.SoTaiKhoanKeToan + " - " + x.TenTaiKhoanKeToan,
                    TrangThai = x.IsHoatDong ? "Hoạt động" : "Ngừng hoạt động"
                });

                return ExportDanhMucToExcel("TKTT01", exportData, "Danh mục tài khoản thanh toán", "DanhMucTaiKhoanThanhToan");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi xuất excel: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", new { keyword = keyword, isHoatDong = isHoatDong });
            }
        }
    }
}
