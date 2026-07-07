using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class CongNoKhachHangController : BaseController
    {
        private readonly ICongNoKhachHangRepository _repo;
        private readonly IKhachHangRepository _khachHangRepo;

        public CongNoKhachHangController(
            ICongNoKhachHangRepository repo,
            IKhachHangRepository khachHangRepo)
        {
            _repo = repo;
            _khachHangRepo = khachHangRepo;
        }

        // GET: /CongNoKhachHang
        public ActionResult Index(
            string tuNgay = "",
            string denNgay = "",
            int? idKhachHang = null,
            int? trangThaiCongNo = null)
        {
            if (!PermissionHelper.HasPermission("CongNoKhachHang", LoaiPhanQuyen.Xem))
                return View("AccessDenied");

            var list = _repo.GetList(tuNgay, denNgay, idKhachHang, trangThaiCongNo).ToList();
            var dbModel = _repo.GetDashboard(tuNgay, denNgay, idKhachHang);

            int totalRecords = list.Count;
            int page = 1;
            int pageSize = 20;
            var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

            var model = new PagedListViewModel<CongNoKhachHangViewModel>
            {
                Items = pagedItems,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                ActionName = "GetList"
            };

            ViewBag.Title = "Công nợ khách hàng";
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.IDKhachHang = idKhachHang;
            ViewBag.TrangThaiCongNo = trangThaiCongNo;

            // Dashboard ViewBags
            ViewBag.TongPhaiThu = list.Sum(x => x.DoanhThu);
            ViewBag.DaThu = list.Sum(x => x.DaThu);
            ViewBag.ConPhaiThu = list.Sum(x => x.ConPhaiThu);
            ViewBag.KhachTraTruoc = dbModel.KhachTraTruoc;
            ViewBag.CongNoQuaHan = list.Sum(x => x.TienQuaHan);

            PopulateKhachHangDropdown(idKhachHang);

            if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_CongNoKhachHangList", model);

            return View("Index", model);
        }

        // GET: /CongNoKhachHang/GetList
        public ActionResult GetList(
            int page = 1, int pageSize = 20,
            string tuNgay = "",
            string denNgay = "",
            int? idKhachHang = null,
            int? trangThaiCongNo = null)
        {
            if (!PermissionHelper.HasPermission("CongNoKhachHang", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var list = _repo.GetList(tuNgay, denNgay, idKhachHang, trangThaiCongNo).ToList();
                var dbModel = _repo.GetDashboard(tuNgay, denNgay, idKhachHang);

                int totalRecords = list.Count;
                var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize);

                var model = new PagedListViewModel<CongNoKhachHangViewModel>
                {
                    Items = pagedItems,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList"
                };

                // Dashboard ViewBags
                ViewBag.TongPhaiThu = list.Sum(x => x.DoanhThu);
                ViewBag.DaThu = list.Sum(x => x.DaThu);
                ViewBag.ConPhaiThu = list.Sum(x => x.ConPhaiThu);
                ViewBag.KhachTraTruoc = dbModel.KhachTraTruoc;
                ViewBag.CongNoQuaHan = list.Sum(x => x.TienQuaHan);

                return PartialView("_CongNoKhachHangList", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>");
            }
        }

        // GET: /CongNoKhachHang/GetDetail
        public ActionResult GetDetail(int idKhachHang, string tuNgay = "", string denNgay = "")
        {
            if (!PermissionHelper.HasPermission("CongNoKhachHang", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var kh = _khachHangRepo.GetById(idKhachHang);
                if (kh == null)
                    return Content("<div class='alert alert-danger'>Khách hàng không tồn tại</div>");

                var details = _repo.GetDetail(idKhachHang, tuNgay, denNgay).ToList();

                ViewBag.CustomerName = kh.TenKhachHang;
                ViewBag.CustomerAddress = kh.DiaChi;
                ViewBag.CustomerPhone = kh.SoDienThoai;
                ViewBag.TuNgay = tuNgay;
                ViewBag.DenNgay = denNgay;

                // Summary stats for the ledger modal
                ViewBag.TotalPhaiThu = details.Where(x => x.LoaiChungTu == "BÁN HÀNG").Sum(x => x.PhaiThu);
                ViewBag.TotalThanhToan = details.Where(x => x.LoaiChungTu == "PHIẾU THU").Sum(x => x.ThanhToan);
                var lastRow = details.LastOrDefault();
                ViewBag.CurrentBalance = lastRow != null ? lastRow.ConLai : 0M;

                return PartialView("_DetailModal", details);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>");
            }
        }

        // GET: /CongNoKhachHang/GetHistory
        public ActionResult GetHistory(int idChungTuBanHang)
        {
            if (!PermissionHelper.HasPermission("CongNoKhachHang", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var history = _repo.GetHistory(idChungTuBanHang).ToList();
                return PartialView("_HistoryModal", history);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>");
            }
        }

        private void PopulateKhachHangDropdown(int? selectedId = null)
        {
            var list = _repo.GetKhachHangDropdown()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x.ID).ToString(),
                    Text = (string)x.TenHienThi,
                    Selected = selectedId.HasValue && (int)x.ID == selectedId.Value
                }).ToList();
            ViewBag.KhachHangList = list;
        }
    }
}
