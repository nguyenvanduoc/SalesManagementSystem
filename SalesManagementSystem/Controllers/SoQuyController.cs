using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class SoQuyController : BaseController
    {
        private readonly ISoQuyRepository _repo;
        private readonly ITaiKhoanThanhToanRepository _taiKhoanRepo;

        public SoQuyController(ISoQuyRepository repo, ITaiKhoanThanhToanRepository taiKhoanRepo)
        {
            _repo         = repo;
            _taiKhoanRepo = taiKhoanRepo;
        }

        // GET: /so-quy
        public ActionResult Index(
            string tuNgay = "",
            string denNgay = "",
            int? idTaiKhoanThanhToan = null)
        {
            if (!PermissionHelper.HasPermission("SoQuy", LoaiPhanQuyen.Xem))
                return View("AccessDenied");

            var list = _repo.GetList(tuNgay, denNgay, idTaiKhoanThanhToan).ToList();

            ViewBag.Title               = "Sổ Quỹ";
            ViewBag.TuNgay              = tuNgay;
            ViewBag.DenNgay             = denNgay;
            ViewBag.IDTaiKhoanThanhToan = idTaiKhoanThanhToan;
            ViewBag.TongThu             = list.Sum(x => x.SoTienThu);
            ViewBag.TongChi             = list.Sum(x => x.SoTienChi);

            int totalRecords = list.Count;
            int page         = 1;
            int pageSize     = 50;
            var pagedItems   = list.Skip((page - 1) * pageSize).Take(pageSize);

            var model = new PagedListViewModel<SoQuyViewModel>
            {
                Items        = pagedItems,
                CurrentPage  = page,
                PageSize     = pageSize,
                TotalRecords = totalRecords,
                ActionName   = "GetList"
            };

            PopulateTaiKhoanDropdown(idTaiKhoanThanhToan);

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_SoQuyList", model);

            return View("Index", model);
        }

        // GET: /so-quy/danh-sach
        public ActionResult GetList(
            int page = 1, int pageSize = 50,
            string tuNgay = "",
            string denNgay = "",
            int? idTaiKhoanThanhToan = null)
        {
            if (!PermissionHelper.HasPermission("SoQuy", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var list       = _repo.GetList(tuNgay, denNgay, idTaiKhoanThanhToan).ToList();
                int totalRecords = list.Count;
                var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

                var model = new PagedListViewModel<SoQuyViewModel>
                {
                    Items        = pagedItems,
                    CurrentPage  = page,
                    PageSize     = pageSize,
                    TotalRecords = totalRecords,
                    ActionName   = "GetList"
                };

                ViewBag.TongThu = list.Sum(x => x.SoTienThu);
                ViewBag.TongChi = list.Sum(x => x.SoTienChi);

                return PartialView("_SoQuyList", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>");
            }
        }

        private void PopulateTaiKhoanDropdown(int? selectedId = null)
        {
            int dummy;
            var list = _taiKhoanRepo.GetList(1, 1000, "", 1, out dummy)
                .Select(x => new SelectListItem
                {
                    Value    = x.ID.ToString(),
                    Text     = x.TenTaiKhoan,
                    Selected = selectedId.HasValue && x.ID == selectedId.Value
                }).ToList();

            ViewBag.TaiKhoanList = list;
        }
    }
}
