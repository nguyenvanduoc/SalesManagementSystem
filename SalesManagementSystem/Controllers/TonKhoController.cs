using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using Dapper;
using SalesManagementSystem.Data;

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

        private bool CheckIsNhanVienKho()
        {
            // Nếu có Quyền phụ (Tùy chọn) thì được xem tất cả (không phải nhân viên kho -> false)
            // Ngược lại nếu không có quyền phụ thì bị ẩn (là nhân viên kho -> true)
            return !PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.TuyChon);
        }

        public ActionResult Index(string tuNgay = "", string denNgay = "")
        {
            if (!PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            if (string.IsNullOrEmpty(tuNgay) && string.IsNullOrEmpty(denNgay))
            {
                var now = DateTime.Now;
                tuNgay = new DateTime(now.Year, 1, 1).ToString("yyyy-MM-dd");
                denNgay = DateTime.Now.ToString("yyyy-MM-dd");
            }

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.IsNhanVienKho = CheckIsNhanVienKho();
            return View();
        }

        [HttpGet]
        public ActionResult GetDashboard(int? idKho = null, int? idSanPham = null, string tuNgay = "", string denNgay = "", bool chiConTon = false)
        {
            if (!PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay))
            {
                return Json(new { success = true, data = new { TongSoSanPham = 0, TongSoLuongTon = 0, TongGiaTriTon = 0, SoSanPhamAmKho = 0, SoSanPhamSapHetHang = 0 } }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var dashboard = _tonKhoRepo.GetDashboard(idKho, idSanPham, tuNgay, denNgay, chiConTon);
                if (CheckIsNhanVienKho())
                {
                    dashboard.TongGiaTriTon = 0;
                }
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

            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay))
            {
                var emptyModel = new PagedListViewModel<TonKhoListViewModel>
                {
                    Items = new List<TonKhoListViewModel>(),
                    CurrentPage = 1,
                    PageSize = pageSize,
                    TotalRecords = 0,
                    ActionName = "GetList"
                };
                ViewBag.IsNhanVienKho = CheckIsNhanVienKho();
                return PartialView("_TonKhoList", emptyModel);
            }

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

                bool isNvk = CheckIsNhanVienKho();
                ViewBag.IsNhanVienKho = isNvk;

                ViewBag.TongTonDauKy = list.Sum(x => x.TonDauKy);
                ViewBag.TongTongNhap = list.Sum(x => x.TongNhap);
                ViewBag.TongTongXuat = list.Sum(x => x.TongXuat);
                ViewBag.TongTonKho = list.Sum(x => x.TonKho);
                ViewBag.TongGiaTriTon = isNvk ? 0m : list.Sum(x => x.GiaTriTon);

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

            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay))
            {
                return Content("<div class='alert alert-warning'>Vui lòng chọn khoảng thời gian tra cứu.</div>");
            }

            try
            {
                var list = _tonKhoRepo.GetTheKho(idKho, idSanPham, tuNgay, denNgay);
                ViewBag.IdKho = idKho;
                ViewBag.IdSanPham = idSanPham;
                ViewBag.TuNgay = tuNgay;
                ViewBag.DenNgay = denNgay;
                ViewBag.IsNhanVienKho = CheckIsNhanVienKho();
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
            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay)) return Content("Vui lòng chọn khoảng thời gian tra cứu.");

            var list = _tonKhoRepo.GetList(idKho, idSanPham, tuNgay, denNgay, chiConTon).ToList();
            bool isNvk = CheckIsNhanVienKho();

            try
            {
                string strTuNgay = "";
                string strDenNgay = "";
                if (DateTime.TryParse(tuNgay, out DateTime dTu)) strTuNgay = dTu.ToString("dd/MM/yyyy");
                else strTuNgay = tuNgay;

                if (DateTime.TryParse(denNgay, out DateTime dDen)) strDenNgay = dDen.ToString("dd/MM/yyyy");
                else strDenNgay = denNgay;

                var variables = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "TuNgay", string.IsNullOrEmpty(strTuNgay) ? "" : $"Từ ngày: {strTuNgay}" },
                    { "DenNgay", string.IsNullOrEmpty(strDenNgay) ? "" : $"Đến ngày: {strDenNgay}" }
                };

                int stt = 1;
                var exportData = list.Select(item => new {
                    STT = stt++,
                    MaKho = item.MaKho,
                    MaKhoHang = item.MaKho,
                    TenKho = item.TenKho,
                    TenKhoHang = item.TenKho,
                    MaSanPham = item.MaSanPham,
                    TenSanPham = item.TenSanPham,
                    DVT = item.DVT,
                    TonDauKy = item.TonDauKy,
                    TongNhap = item.TongNhap,
                    TongXuat = item.TongXuat,
                    TonKho = item.TonKho,
                    SoLuongTon = item.TonKho,
                    TonHienTai = item.TonKho,
                    DonGiaTon = isNvk ? 0m : item.DonGiaTon,
                    DonGiaCuoi = isNvk ? 0m : item.DonGiaTon,
                    DonGiaTonCuoi = isNvk ? 0m : item.DonGiaTon,
                    GiaTriTon = isNvk ? 0m : item.GiaTriTon,
                    NgayNhapCuoi = item.NgayNhapCuoi.HasValue ? item.NgayNhapCuoi.Value.ToString("dd/MM/yyyy") : "",
                    NgayXuatCuoi = item.NgayXuatCuoi.HasValue ? item.NgayXuatCuoi.Value.ToString("dd/MM/yyyy") : ""
                }).ToList();

                string fileExtension;
                var bytes = _excelExportService.Export("TK01", exportData, out fileExtension, variables);
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
            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay)) return Content("Vui lòng chọn khoảng thời gian tra cứu.");

            var list = _tonKhoRepo.GetList(idKho, idSanPham, tuNgay, denNgay, chiConTon);
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.IsNhanVienKho = CheckIsNhanVienKho();

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

        [HttpGet]
        public ActionResult PrintTheKho(int idKho, int idSanPham, string tuNgay = "", string denNgay = "")
        {
            if (!PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) return Content("Không có quyền in");
            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay)) return Content("Vui lòng chọn khoảng thời gian tra cứu.");

            var list = _tonKhoRepo.GetTheKho(idKho, idSanPham, tuNgay, denNgay);
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.IsNhanVienKho = CheckIsNhanVienKho();

            int totalKho;
            var kho = _khoHangRepo.GetPaged(1, 1, null, out totalKho).FirstOrDefault(x => x.ID == idKho);
            ViewBag.TenKho = kho != null ? kho.TenKhoHang : "Tất cả kho";

            int totalSP;
            var sp = _sanPhamRepo.GetPaged(1, 1, null, out totalSP).FirstOrDefault(x => x.ID == idSanPham);
            ViewBag.TenSanPham = sp != null ? sp.TenSanPham : "Không xác định";

            return View(list);
        }

        [HttpGet]
        public ActionResult PrintTheKhoMulti(int? idKho = null, int? idSanPham = null, string tuNgay = "", string denNgay = "", bool chiConTon = false)
        {
            if (!PermissionHelper.HasPermission("TonKho", LoaiPhanQuyen.Xem)) return Content("Không có quyền in");
            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay)) return Content("Vui lòng chọn khoảng thời gian tra cứu.");

            var products = _tonKhoRepo.GetList(idKho, idSanPham, tuNgay, denNgay, chiConTon)
                                      .GroupBy(p => p.IDSanPham)
                                      .Select(g => g.First())
                                      .ToList();
            var model = new List<PrintTheKhoMultiViewModel>();

            foreach (var p in products)
            {
                var cards = _tonKhoRepo.GetTheKho(idKho, p.IDSanPham, tuNgay, denNgay);
                model.Add(new PrintTheKhoMultiViewModel
                {
                    TenKho = idKho.HasValue ? p.TenKho : "Tất cả kho",
                    TenSanPham = p.MaSanPham + " - " + p.TenSanPham,
                    TheKhoList = cards
                });
            }

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.IsNhanVienKho = CheckIsNhanVienKho();
            ViewBag.TenKho = idKho.HasValue ? (products.FirstOrDefault()?.TenKho ?? "Không xác định") : "Tất cả kho";

            return View(model);
        }
    }
}
