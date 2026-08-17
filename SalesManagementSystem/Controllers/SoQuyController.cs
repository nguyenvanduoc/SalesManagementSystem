using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class SoQuyController : BaseController
    {
        private readonly ISoQuyRepository _repo;
        private readonly ITaiKhoanThanhToanRepository _taiKhoanRepo;
        private readonly SalesManagementSystem.Services.Interfaces.IExcelExportService _excelExportService;

        public SoQuyController(
            ISoQuyRepository repo, 
            ITaiKhoanThanhToanRepository taiKhoanRepo,
            SalesManagementSystem.Services.Interfaces.IExcelExportService excelExportService)
        {
            _repo               = repo;
            _taiKhoanRepo       = taiKhoanRepo;
            _excelExportService = excelExportService;
        }

        // GET: /so-quy
        public ActionResult Index(
            string tuNgay = "",
            string denNgay = "",
            int? idTaiKhoanThanhToan = null,
            int page = 1,
            int pageSize = 20)
        {
            if (!PermissionHelper.HasPermission("SoQuy", LoaiPhanQuyen.Xem))
                return View("AccessDenied");

            // Default to current month if dates are completely empty
            if (string.IsNullOrEmpty(tuNgay) && string.IsNullOrEmpty(denNgay))
            {
                var now = DateTime.Now;
                tuNgay = new DateTime(now.Year, 1, 1).ToString("yyyy-MM-dd");
                denNgay = DateTime.Now.ToString("yyyy-MM-dd");
            }

            // Perform server-side validation: if dates are empty, do not query data
            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay))
            {
                var emptyModel = new PagedListViewModel<TaiKhoanSummaryViewModel>
                {
                    Items        = new List<TaiKhoanSummaryViewModel>(),
                    CurrentPage  = 1,
                    PageSize     = pageSize,
                    TotalRecords = 0,
                    ActionName   = "GetList"
                };

                ViewBag.Title               = "Sổ Quỹ - Tài Khoản Thanh Toán";
                ViewBag.TuNgay              = tuNgay;
                ViewBag.DenNgay             = denNgay;
                ViewBag.IDTaiKhoanThanhToan = idTaiKhoanThanhToan;
                ViewBag.TongDauKy           = 0M;
                ViewBag.TongThu             = 0M;
                ViewBag.TongChi             = 0M;
                ViewBag.TongCuoiKy          = 0M;

                PopulateTaiKhoanDropdown(idTaiKhoanThanhToan);

                if ((Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest") && Request.Headers["X-SPA-Load"] != "true")
                    return PartialView("_SoQuyList", emptyModel);

                return View("Index", emptyModel);
            }

            var list = _repo.GetTaiKhoanSummary(tuNgay, denNgay, idTaiKhoanThanhToan).ToList();

            ViewBag.Title               = "Sổ Quỹ - Tài Khoản Thanh Toán";
            ViewBag.TuNgay              = tuNgay;
            ViewBag.DenNgay             = denNgay;
            ViewBag.IDTaiKhoanThanhToan = idTaiKhoanThanhToan;
            
            ViewBag.TongDauKy           = list.Sum(x => x.SoDuDauKy);
            ViewBag.TongThu             = list.Sum(x => x.ThuTrongKy);
            ViewBag.TongChi             = list.Sum(x => x.ChiTrongKy);
            ViewBag.TongCuoiKy          = list.Sum(x => x.SoDuCuoiKy);

            int totalRecords = list.Count;
            var pagedItems   = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var model = new PagedListViewModel<TaiKhoanSummaryViewModel>
            {
                Items        = pagedItems,
                CurrentPage  = page,
                PageSize     = pageSize,
                TotalRecords = totalRecords,
                ActionName   = "GetList"
            };

            PopulateTaiKhoanDropdown(idTaiKhoanThanhToan);

            if ((Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest") && Request.Headers["X-SPA-Load"] != "true")
                return PartialView("_SoQuyList", model);

            return View("Index", model);
        }

        // GET: /so-quy/danh-sach
        public ActionResult GetList(
            string tuNgay = "",
            string denNgay = "",
            int? idTaiKhoanThanhToan = null,
            int page = 1,
            int pageSize = 20)
        {
            if (!PermissionHelper.HasPermission("SoQuy", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay))
            {
                var emptyModel = new PagedListViewModel<TaiKhoanSummaryViewModel>
                {
                    Items        = new List<TaiKhoanSummaryViewModel>(),
                    CurrentPage  = 1,
                    PageSize     = pageSize,
                    TotalRecords = 0,
                    ActionName   = "GetList"
                };
                ViewBag.TongDauKy  = 0M;
                ViewBag.TongThu    = 0M;
                ViewBag.TongChi    = 0M;
                ViewBag.TongCuoiKy = 0M;
                return PartialView("_SoQuyList", emptyModel);
            }

            try
            {
                var list       = _repo.GetTaiKhoanSummary(tuNgay, denNgay, idTaiKhoanThanhToan).ToList();
                int totalRecords = list.Count;
                var pagedItems = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var model = new PagedListViewModel<TaiKhoanSummaryViewModel>
                {
                    Items        = pagedItems,
                    CurrentPage  = page,
                    PageSize     = pageSize,
                    TotalRecords = totalRecords,
                    ActionName   = "GetList"
                };

                ViewBag.TongDauKy  = list.Sum(x => x.SoDuDauKy);
                ViewBag.TongThu    = list.Sum(x => x.ThuTrongKy);
                ViewBag.TongChi    = list.Sum(x => x.ChiTrongKy);
                ViewBag.TongCuoiKy = list.Sum(x => x.SoDuCuoiKy);

                return PartialView("_SoQuyList", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>");
            }
        }

        private DateTime ParseDate(string dateStr, DateTime defaultDate)
        {
            if (string.IsNullOrEmpty(dateStr)) return defaultDate;
            string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "d/M/yyyy", "yyyy-M-d" };
            if (DateTime.TryParseExact(dateStr.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
            if (DateTime.TryParse(dateStr, out result))
            {
                return result;
            }
            return defaultDate;
        }

        // GET: /so-quy/chi-tiet
        public ActionResult Details(
            int idTaiKhoanThanhToan,
            string tuNgay = "",
            string denNgay = "",
            int page = 1,
            int pageSize = 20)
        {
            if (!PermissionHelper.HasPermission("SoQuy", LoaiPhanQuyen.Xem))
                return Content("<div class='alert alert-danger'>Không có quyền truy cập</div>");

            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay))
            {
                return Content("<div class='alert alert-warning'>Vui lòng chọn khoảng thời gian tra cứu.</div>");
            }

            var taiKhoan = _taiKhoanRepo.GetByID(idTaiKhoanThanhToan);
            if (taiKhoan == null)
                return HttpNotFound("Không tìm thấy tài khoản thanh toán");

            // Calculate opening balance
            decimal openingBalance = _repo.GetOpeningBalance(tuNgay, idTaiKhoanThanhToan);

            // Get all transactions
            var allTransactions = _repo.GetGiaoDichChiTiet(tuNgay, denNgay, idTaiKhoanThanhToan).ToList();

            // Compute running balance dynamically
            decimal currentBalance = openingBalance;
            int stt = 1;
            foreach (var tx in allTransactions)
            {
                tx.STT = stt++;
                currentBalance = currentBalance + tx.SoTienThu - tx.SoTienChi;
                tx.SoDuLuyKe = currentBalance;
            }

            // Calculations for pop-up dashboard
            decimal tongThu = allTransactions.Sum(x => x.SoTienThu);
            decimal tongChi = allTransactions.Sum(x => x.SoTienChi);
            decimal closingBalance = openingBalance + tongThu - tongChi;

            DateTime start = ParseDate(tuNgay, DateTime.Today.AddMonths(-1));
            DateTime end   = ParseDate(denNgay, DateTime.Today);

            ViewBag.OpeningBalance  = openingBalance;
            ViewBag.TongThu         = tongThu;
            ViewBag.TongChi         = tongChi;
            ViewBag.ClosingBalance  = closingBalance;
            ViewBag.TaiKhoan        = taiKhoan;
            ViewBag.TuNgay          = start.ToString("yyyy-MM-dd");
            ViewBag.DenNgay         = end.ToString("yyyy-MM-dd");
            ViewBag.TuNgayFormatted  = start.ToString("dd/MM/yyyy");
            ViewBag.DenNgayFormatted = end.ToString("dd/MM/yyyy");

            // Generate daily balance details for Chart.js
            var dailyGroups = allTransactions
                .GroupBy(x => x.NgayGiaoDich.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            var chartLabels = new List<string>();
            var chartData   = new List<decimal>();

            decimal tempBalance = openingBalance;
            
            // Initial point at start of period
            chartLabels.Add(start.AddDays(-1).ToString("dd/MM"));
            chartData.Add(tempBalance);

            for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (dailyGroups.TryGetValue(date, out var dayTxs))
                {
                    foreach (var tx in dayTxs)
                    {
                        tempBalance += tx.SoTienThu - tx.SoTienChi;
                    }
                }
                chartLabels.Add(date.ToString("dd/MM"));
                chartData.Add(tempBalance);
            }

            ViewBag.ChartLabels = Newtonsoft.Json.JsonConvert.SerializeObject(chartLabels);
            ViewBag.ChartData   = Newtonsoft.Json.JsonConvert.SerializeObject(chartData);

            // Paginate results
            int totalRecords = allTransactions.Count;
            var pagedItems   = allTransactions.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var model = new PagedListViewModel<GiaoDichChiTietViewModel>
            {
                Items        = pagedItems,
                CurrentPage  = page,
                PageSize     = pageSize,
                TotalRecords = totalRecords,
                ActionName   = "Details" // Trùng Action để phân trang tự động trỏ về đây
            };

            // If it is just table pagination refresh inside the modal
            if (Request.QueryString["isGridOnly"] == "true")
            {
                return PartialView("_ChiTietGiaoDichList", model);
            }

            return PartialView("_ChiTietTaiKhoan", model);
        }

        // GET: /so-quy/xuat-excel-tong-hop
        public ActionResult ExportExcelTongHop(string tuNgay = "", string denNgay = "", int? idTaiKhoanThanhToan = null)
        {
            if (!PermissionHelper.HasPermission("SoQuy", LoaiPhanQuyen.Xem))
                return new HttpStatusCodeResult(403);

            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay))
            {
                return Content("Vui lòng chọn khoảng thời gian tra cứu.");
            }

            try
            {
                var list = _repo.GetTaiKhoanSummary(tuNgay, denNgay, idTaiKhoanThanhToan).ToList();

                var session = (SalesManagementSystem.Models.ViewModels.UserLoginViewModel)Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION];
                string nguoiLapBieu = session != null ? (session.HoDem + " " + session.Ten).Trim() : "";
                if (string.IsNullOrEmpty(nguoiLapBieu)) nguoiLapBieu = session?.UserName ?? "";

                var variables = new Dictionary<string, object>
                {
                    { "TuNgay", DateTime.Parse(tuNgay).ToString("dd/MM/yyyy") },
                    { "DenNgay", DateTime.Parse(denNgay).ToString("dd/MM/yyyy") },
                    { "NguoiLapBieu", nguoiLapBieu },
                    { "Ngay", DateTime.Now.ToString("dd") },
                    { "Thang", DateTime.Now.ToString("MM") },
                    { "Nam", DateTime.Now.ToString("yyyy") },
                    { "TongDauKy", list.Sum(x => x.SoDuDauKy) },
                    { "TongThu", list.Sum(x => x.ThuTrongKy) },
                    { "TongChi", list.Sum(x => x.ChiTrongKy) },
                    { "TongCuoiKy", list.Sum(x => x.SoDuCuoiKy) }
                };

                var exportData = list.Select(x => new
                {
                    TenTaiKhoan = x.TenTaiKhoan,
                    NganHang = string.IsNullOrEmpty(x.NganHang) ? "---" : x.NganHang,
                    SoTaiKhoan = string.IsNullOrEmpty(x.SoTaiKhoan) ? "---" : x.SoTaiKhoan,
                    ChuTaiKhoan = string.IsNullOrEmpty(x.ChuTaiKhoan) ? "---" : x.ChuTaiKhoan,
                    SoDuDauKy = x.SoDuDauKy,
                    ThuTrongKy = x.ThuTrongKy,
                    ChiTrongKy = x.ChiTrongKy,
                    SoDuCuoiKy = x.SoDuCuoiKy
                });

                string fileExtension;
                var fileBytes = _excelExportService.Export(BieuMauConstants.DS_SO_QUY, exportData, out fileExtension, variables);

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                string fileName = $"BangTongHopTaiKhoan_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}";
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi xuất Excel: " + ex.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }
        }

        // GET: /so-quy/xuat-excel-chi-tiet
        public ActionResult ExportExcelChiTiet(int idTaiKhoanThanhToan, string tuNgay = "", string denNgay = "")
        {
            if (!PermissionHelper.HasPermission("SoQuy", LoaiPhanQuyen.Xem))
                return new HttpStatusCodeResult(403);

            if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay))
            {
                return Content("Vui lòng chọn khoảng thời gian tra cứu.");
            }

            var taiKhoan = _taiKhoanRepo.GetByID(idTaiKhoanThanhToan);
            if (taiKhoan == null)
                return HttpNotFound("Không tìm thấy tài khoản thanh toán");

            // Calculate opening balance
            decimal openingBalance = _repo.GetOpeningBalance(tuNgay, idTaiKhoanThanhToan);
            
            // Get all transactions
            var allTransactions = _repo.GetGiaoDichChiTiet(tuNgay, denNgay, idTaiKhoanThanhToan).ToList();
            
            // Compute running balance dynamically
            decimal currentBalance = openingBalance;
            int stt = 1;
            foreach (var tx in allTransactions)
            {
                tx.STT = stt++;
                currentBalance = currentBalance + tx.SoTienThu - tx.SoTienChi;
                tx.SoDuLuyKe = currentBalance;
            }

            string title = $"SỔ CHI TIẾT TÀI KHOẢN: {taiKhoan.TenTaiKhoan} ({DateTime.Parse(tuNgay):dd/MM/yyyy} - {DateTime.Parse(denNgay):dd/MM/yyyy})";
            string[] headers = { "STT", "Ngày giao dịch", "Số chứng từ", "Loại giao dịch", "Diễn giải", "Thu", "Chi", "Số dư lũy kế" };

            var exportList = new List<object[]>();
            // Dòng mở đầu là Số dư đầu kỳ
            exportList.Add(new object[] { "-", "-", "-", "-", "Số dư đầu kỳ", 0M, 0M, openingBalance });
            foreach (var tx in allTransactions)
            {
                exportList.Add(new object[]
                {
                    tx.STT,
                    tx.NgayGiaoDich,
                    tx.SoChungTu,
                    tx.LoaiChungTu == "THU" || tx.LoaiChungTu == "PHIEU_THU" ? "Phiếu thu" : "Phiếu chi",
                    tx.DienGiai,
                    tx.SoTienThu,
                    tx.SoTienChi,
                    tx.SoDuLuyKe
                });
            }

            var fileBytes = ExportToExcelDynamic(title, headers, exportList, (item, idx) => item);

            string fileName = $"SoChiTiet_{taiKhoan.MaTaiKhoan}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private byte[] ExportToExcelDynamic<T>(string title, string[] headers, IEnumerable<T> items, Func<T, int, object[]> rowDataSelector)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");

            // Title Row
            var titleRow = sheet.CreateRow(0);
            var titleCell = titleRow.CreateCell(0);
            titleCell.SetCellValue(title);
            var titleFont = workbook.CreateFont();
            titleFont.IsBold = true;
            titleFont.FontHeightInPoints = 14;
            
            var titleStyle = workbook.CreateCellStyle();
            titleStyle.SetFont(titleFont);
            titleCell.CellStyle = titleStyle;

            // Merged region for title
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, headers.Length - 1));

            // Header Row
            var headerRow = sheet.CreateRow(2);
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            
            var headerStyle = workbook.CreateCellStyle();
            headerStyle.SetFont(headerFont);
            headerStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.Alignment = HorizontalAlignment.Center;
            headerStyle.VerticalAlignment = VerticalAlignment.Center;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BorderTop = BorderStyle.Thin;
            headerStyle.BorderLeft = BorderStyle.Thin;
            headerStyle.BorderRight = BorderStyle.Thin;

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            // Cell Styles
            var borderStyle = workbook.CreateCellStyle();
            borderStyle.BorderBottom = BorderStyle.Thin;
            borderStyle.BorderTop = BorderStyle.Thin;
            borderStyle.BorderLeft = BorderStyle.Thin;
            borderStyle.BorderRight = BorderStyle.Thin;
            borderStyle.VerticalAlignment = VerticalAlignment.Center;

            var numericStyle = workbook.CreateCellStyle();
            numericStyle.CloneStyleFrom(borderStyle);
            numericStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0");
            numericStyle.Alignment = HorizontalAlignment.Right;

            var centerStyle = workbook.CreateCellStyle();
            centerStyle.CloneStyleFrom(borderStyle);
            centerStyle.Alignment = HorizontalAlignment.Center;

            int rowIndex = 3;
            int stt = 1;
            foreach (var item in items)
            {
                var row = sheet.CreateRow(rowIndex++);
                var values = rowDataSelector(item, stt++);
                for (int i = 0; i < values.Length; i++)
                {
                    var cell = row.CreateCell(i);
                    var val = values[i];
                    
                    cell.CellStyle = borderStyle;

                    if (val == null)
                    {
                        cell.SetCellValue("");
                    }
                    else if (val is decimal decVal)
                    {
                        cell.SetCellValue(Convert.ToDouble(decVal));
                        cell.CellStyle = numericStyle;
                    }
                    else if (val is double dVal)
                    {
                        cell.SetCellValue(dVal);
                        cell.CellStyle = numericStyle;
                    }
                    else if (val is int iVal)
                    {
                        cell.SetCellValue(iVal);
                        cell.CellStyle = numericStyle;
                    }
                    else if (val is DateTime dt)
                    {
                        cell.SetCellValue(dt.ToString("dd/MM/yyyy"));
                        cell.CellStyle = centerStyle;
                    }
                    else
                    {
                        string strVal = val.ToString();
                        cell.SetCellValue(strVal);
                        if (strVal == "THU" || strVal == "CHI" || strVal == "Phiếu thu" || strVal == "Phiếu chi" || strVal == "-")
                        {
                            cell.CellStyle = centerStyle;
                        }
                    }
                }
            }

            // Auto-fit columns
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
                int currentWidth = sheet.GetColumnWidth(i);
                sheet.SetColumnWidth(i, currentWidth + 1000);
            }

            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);
                return ms.ToArray();
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
