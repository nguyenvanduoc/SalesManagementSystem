using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace SalesManagementSystem.Controllers
{
    public class TonKhoController : BaseController
    {
        private readonly ITonKhoRepository _tonKhoRepo;
        private readonly IDmKhoHangRepository _khoHangRepo;
        private readonly IDmSanPhamRepository _sanPhamRepo;
        private readonly SalesManagementSystem.Services.Interfaces.IExcelExportService _excelExportService;

        public TonKhoController(
            ITonKhoRepository tonKhoRepo,
            IDmKhoHangRepository khoHangRepo,
            IDmSanPhamRepository sanPhamRepo,
            SalesManagementSystem.Services.Interfaces.IExcelExportService excelExportService)
        {
            _tonKhoRepo = tonKhoRepo;
            _khoHangRepo = khoHangRepo;
            _sanPhamRepo = sanPhamRepo;
            _excelExportService = excelExportService;
        }

        public ActionResult Index()
        {
            if (!PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");
            return View();
        }

        [HttpGet]
        public ActionResult GetDashboard(int? idKho = null, string tuNgay = "", string denNgay = "")
        {
            if (!PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            try
            {
                var dashboard = _tonKhoRepo.GetDashboard(idKho, tuNgay, denNgay);
                return Json(new { success = true, data = dashboard }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetList(int? idKho = null, int? idSanPham = null, string tuNgay = "", string denNgay = "", bool chiConTon = false, int page = 1, int pageSize = 20)
        {
            if (!PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var list = _tonKhoRepo.GetList(idKho, idSanPham, tuNgay, denNgay, chiConTon).ToList();
                int totalRecords = list.Count;
                var pagedList = new PagedListViewModel<TonKhoListViewModel>
                {
                    Items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList"
                };

                return PartialView("_TonKhoList", pagedList);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi Server: {ex.Message} <br/> {ex.StackTrace}</div>");
            }
        }

        [HttpGet]
        public ActionResult GetTheKho(int idKho, int idSanPham, string tuNgay = "", string denNgay = "")
        {
            if (!PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            try
            {
                var list = _tonKhoRepo.GetTheKho(idKho, idSanPham, tuNgay, denNgay);
                return PartialView("_TheKhoModal", list);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi Server: {ex.Message} <br/> {ex.StackTrace}</div>");
            }
        }

        [HttpGet]
        public ActionResult ExportExcel(int? idKho = null, int? idSanPham = null, string tuNgay = "", string denNgay = "", bool chiConTon = false)
        {
            if (!PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) return Content("Không có quyền xuất Excel");

            var list = _tonKhoRepo.GetList(idKho, idSanPham, tuNgay, denNgay, chiConTon).ToList();

            try
            {
                var variables = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "TuNgay", string.IsNullOrEmpty(tuNgay) ? "" : $"Từ ngày: {DateTime.Parse(tuNgay):dd/MM/yyyy}" },
                    { "DenNgay", string.IsNullOrEmpty(denNgay) ? "" : $"Đến ngày: {DateTime.Parse(denNgay):dd/MM/yyyy}" }
                };

                string fileExtension;
                var bytes = _excelExportService.Export("TK01", list, out fileExtension, variables);
                string contentType = fileExtension == "xls" ? "application/vnd.ms-excel" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, $"TonKho_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi xuất Excel: {ex.Message}");
            }
        }

        [HttpGet]
        public ActionResult Print(int? idKho = null, int? idSanPham = null, string tuNgay = "", string denNgay = "", bool chiConTon = false)
        {
            if (!PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) return Content("Không có quyền in");

            var list = _tonKhoRepo.GetList(idKho, idSanPham, tuNgay, denNgay, chiConTon);
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;

            string tenKho = "Tất cả kho";
            if (idKho.HasValue)
            {
                int total;
                var kho = _khoHangRepo.GetPaged(1, 1, null, out total).FirstOrDefault(x => x.ID == idKho.Value);
                if (kho != null) tenKho = kho.TenKhoHang;
            }
            ViewBag.TenKho = tenKho;

            return View(list);
        }

        [HttpGet]
        public ActionResult SearchKhoHang(string q)
        {
            int total;
            var data = _khoHangRepo.GetPaged(1, 20, q, out total);
            return Json(data.Select(x => new { id = x.ID, text = x.MaKhoHang + " - " + x.TenKhoHang }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchSanPham(string q)
        {
            int total;
            var data = _sanPhamRepo.GetPaged(1, 20, q, out total);
            return Json(data.Select(x => new { id = x.ID, text = x.MaSanPham + " - " + x.TenSanPham }), JsonRequestBehavior.AllowGet);
        }
    }
}
