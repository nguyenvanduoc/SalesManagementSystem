using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Services.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class CongNoKhachHangController : BaseController
    {
        private readonly ICongNoKhachHangRepository _repo;
        private readonly IKhachHangRepository _khachHangRepo;
        private readonly IExcelExportService _excelExportService;

        public CongNoKhachHangController(
            ICongNoKhachHangRepository repo,
            IKhachHangRepository khachHangRepo,
            IExcelExportService excelExportService)
        {
            _repo = repo;
            _khachHangRepo = khachHangRepo;
            _excelExportService = excelExportService;
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
            ViewBag.TongTonDauKy = list.Sum(x => x.TonDauKy);
            ViewBag.TongNoCuoiKy = list.Sum(x => x.LuyKe);
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
                ViewBag.TongTonDauKy = list.Sum(x => x.TonDauKy);
                ViewBag.TongNoCuoiKy = list.Sum(x => x.LuyKe);
                ViewBag.KhachTraTruoc = dbModel.KhachTraTruoc;
                ViewBag.CongNoQuaHan = list.Sum(x => x.TienQuaHan);

                return PartialView("_CongNoKhachHangList", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>");
            }
        }

        [HttpGet]
        public ActionResult ExportExcel(string tuNgay = "", string denNgay = "", int? idKhachHang = null, int? trangThaiCongNo = null)
        {
            if (!PermissionHelper.HasPermission("CongNoKhachHang", LoaiPhanQuyen.Xem))
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Bạn không có quyền thực hiện chức năng này.";
                return RedirectToAction("Index");
            }

            try
            {
                var list = _repo.GetList(tuNgay, denNgay, idKhachHang, trangThaiCongNo).ToList();

                var exportData = list.Select((item, index) => new
                {
                    STT = index + 1,
                    SoChungTu = item.SoChungTu,
                    NgayChungTu = item.NgayChungTu.ToString("dd/MM/yyyy"),
                    TenKhachHang = item.TenKhachHang,
                    TenNhanVienPhuTrach = item.TenNhanVienPhuTrach,
                    TenTinh = item.TenTinh,
                    SoDienThoai = item.DienThoai,
                    DuDauKy = item.TonDauKy,
                    DoanhThu = item.DoanhThu,
                    LuyKe = item.LuyKe,
                    DaThu = item.DaThu,
                    ConPhaiThu = item.ConPhaiThu,
                    QuaHan = item.TienQuaHan,
                    TenTrangThai = item.TrangThai
                }).ToList();

                string khachHangName = "Tất cả khách hàng";
                if (idKhachHang.HasValue)
                {
                    var kh = _khachHangRepo.GetById(idKhachHang.Value);
                    if (kh != null) khachHangName = kh.TenKhachHang;
                }

                var variables = new Dictionary<string, object>
                {
                    { "TuNgay", string.IsNullOrEmpty(tuNgay) ? "..." : DateTime.Parse(tuNgay).ToString("dd/MM/yyyy") },
                    { "DenNgay", string.IsNullOrEmpty(denNgay) ? "..." : DateTime.Parse(denNgay).ToString("dd/MM/yyyy") },
                    { "KhachHang", khachHangName },
                    { "Ngay", DateTime.Now.Day.ToString("00") },
                    { "Thang", DateTime.Now.Month.ToString("00") },
                    { "Nam", DateTime.Now.Year.ToString() }
                };

                string fileExtension;
                var fileBytes = _excelExportService.Export(BieuMauConstants.CNKH, exportData, out fileExtension, variables);

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    TempData["ToastType"] = "error";
                    TempData["ToastMessage"] = "Không tìm thấy biểu mẫu hoặc lỗi khi tạo Excel.";
                    return RedirectToAction("Index");
                }

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"CongNoKhachHang_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Lỗi khi xuất excel: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult ExportSP02(string tuNgay = "", string denNgay = "", int? idKhachHang = null)
        {
            if (!PermissionHelper.HasPermission("CongNoKhachHang", LoaiPhanQuyen.Xem))
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Bạn không có quyền thực hiện chức năng này.";
                return RedirectToAction("Index");
            }

            try
            {
                var list = _repo.GetList(tuNgay, denNgay, idKhachHang, null).ToList();

                var exportData = list.Select((item, index) => new
                {
                    STT = index + 1,
                    Tinh = item.TenTinh,
                    TinhThanh = item.TenTinh,
                    TenKhachHang = item.TenKhachHang,
                    DauKy = item.TonDauKy,
                    DuDauKy = item.TonDauKy,
                    DoanhThu = item.DoanhThu,
                    ThanhToan = item.DaThu,
                    DaThu = item.DaThu,
                    SoDuCuoiKy = item.LuyKe,
                    DuCuoiKy = item.LuyKe,
                    LuyKe = item.LuyKe,
                    KhachThanhToanTruoc = 0M,
                    HangChoGiao = 0M,
                    GhiChu = ""
                }).ToList();

                var variables = new Dictionary<string, object>
                {
                    { "TuNgay", string.IsNullOrEmpty(tuNgay) ? "..." : DateTime.Parse(tuNgay).ToString("dd/MM/yyyy") },
                    { "DenNgay", string.IsNullOrEmpty(denNgay) ? "..." : DateTime.Parse(denNgay).ToString("dd/MM/yyyy") },
                    { "Ngay", DateTime.Now.Day.ToString("00") },
                    { "Thang", DateTime.Now.Month.ToString("00") },
                    { "Nam", DateTime.Now.Year.ToString() }
                };

                string fileExtension;
                var fileBytes = _excelExportService.Export(BieuMauConstants.CNKH_SP02, exportData, out fileExtension, variables);

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    TempData["ToastType"] = "error";
                    TempData["ToastMessage"] = "Không tìm thấy biểu mẫu hoặc lỗi khi tạo Excel.";
                    return RedirectToAction("Index");
                }

                string contentType = fileExtension == "xls" 
                    ? "application/vnd.ms-excel" 
                    : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(fileBytes, contentType, $"BangCongNoPhaiThu_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Lỗi khi xuất excel: " + ex.Message;
                return RedirectToAction("Index");
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
