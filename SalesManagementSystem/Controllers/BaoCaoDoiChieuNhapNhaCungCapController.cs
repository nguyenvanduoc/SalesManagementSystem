using System;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Services.Interfaces;
using System.Collections.Generic;

namespace SalesManagementSystem.Controllers
{
    public class BaoCaoDoiChieuNhapNhaCungCapController : BaseController
    {
        private readonly IBaoCaoDoiChieuNhapNhaCungCapRepository _repo;
        private readonly IExcelExportService _excelExportService;

        public BaoCaoDoiChieuNhapNhaCungCapController(IBaoCaoDoiChieuNhapNhaCungCapRepository repo, IExcelExportService excelExportService)
        {
            _repo = repo;
            _excelExportService = excelExportService;
        }

        public ActionResult Index()
        {
            // Tự do tuỳ chỉnh quyền nếu cần, hiện tại không ràng buộc quyền cụ thể hoặc bạn có thể tự thêm
            // if (!PermissionHelper.HasPermission("BaoCao", LoaiPhanQuyen.Xem)) return View("AccessDenied");

            ViewBag.Title = "BÁO CÁO ĐỐI CHIẾU NHẬP NCC";
            
            var nccs = _repo.GetNhaCungCapDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.NhaCungCapList = new SelectList(nccs.ToList(), "Value", "Text");

            // Khởi tạo ngày mặc định (đầu tháng đến hiện tại)
            ViewBag.TuNgay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
            ViewBag.DenNgay = DateTime.Now.ToString("yyyy-MM-dd");

            return View();
        }

        [HttpGet]
        public ActionResult GetList(int? idNhaCungCap, DateTime? tuNgay, DateTime? denNgay, int page = 1, int pageSize = 20)
        {
            if (!tuNgay.HasValue)
                return Content("<div class='alert alert-warning text-center mt-3'>Vui lòng chọn từ ngày</div>");

            if (!denNgay.HasValue)
                return Content("<div class='alert alert-warning text-center mt-3'>Vui lòng chọn đến ngày</div>");

            if (tuNgay.Value > denNgay.Value)
                return Content("<div class='alert alert-danger text-center mt-3'>Từ ngày không được lớn hơn đến ngày</div>");

            try
            {
                var data = _repo.GetList(idNhaCungCap, tuNgay.Value, denNgay.Value).ToList();
                var totalRecords = data.Count;
                var pagedItems = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                
                decimal tongSoLuongNhap = 0;
                decimal tongPhaiTra = 0;
                decimal tongDaThanhToan = 0;
                decimal conNoCuoiKy = 0;

                if (data.Any())
                {
                    tongSoLuongNhap = data.Sum(x => x.SoLuongNhap);
                    tongPhaiTra = data.Sum(x => x.PhaiTra);
                    tongDaThanhToan = data.Sum(x => x.DaThanhToan);
                    conNoCuoiKy = data.Last().ConNoLuyKe;
                }

                ViewBag.TongSoLuongNhap = tongSoLuongNhap;
                ViewBag.TongPhaiTra = tongPhaiTra;
                ViewBag.TongDaThanhToan = tongDaThanhToan;
                ViewBag.ConNoCuoiKy = conNoCuoiKy;

                var model = new SalesManagementSystem.Models.ViewModels.PagedListViewModel<SalesManagementSystem.Models.ViewModels.BaoCaoDoiChieuNhapNhaCungCapViewModel>
                {
                    Items = pagedItems,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    ActionName = "GetList",
                    Keyword = ""
                };

                return PartialView("_DanhSach", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger text-center mt-3'>Lỗi tải dữ liệu: {ex.Message}</div>");
            }
        }

        [HttpGet]
        public ActionResult ExportExcel(int? idNhaCungCap, DateTime? tuNgay, DateTime? denNgay)
        {
            if (!tuNgay.HasValue || !denNgay.HasValue) return Content("Vui lòng chọn từ ngày và đến ngày");

            try
            {
                var data = _repo.GetList(idNhaCungCap, tuNgay.Value, denNgay.Value).ToList();
                
                string nhaCungCapName = "Tất cả";
                if (idNhaCungCap.HasValue)
                {
                    var provider = _repo.GetNhaCungCapDropdown().FirstOrDefault(x => (int)x.ID == idNhaCungCap.Value);
                    if (provider != null) nhaCungCapName = provider.TenHienThi.ToString();
                }

                var variables = new Dictionary<string, object>
                {
                    { "TuNgay", tuNgay.Value.ToString("dd/MM/yyyy") },
                    { "DenNgay", denNgay.Value.ToString("dd/MM/yyyy") },
                    { "NhaCungCap", nhaCungCapName },
                    { "Ngay", DateTime.Now.Day.ToString("00") },
                    { "Thang", DateTime.Now.Month.ToString("00") },
                    { "Nam", DateTime.Now.Year.ToString() }
                };

                var exportData = data.Select(x => new {
                    STT = x.STT,
                    TenNhaCungCap = x.TenNhaCungCap,
                    NgayPhatSinh = x.NgayPhatSinh.HasValue && x.LoaiDong != 0 ? x.NgayPhatSinh.Value.ToString("dd/MM/yyyy") : "",
                    SoChungTu = x.SoChungTu,
                    LoaiPhatSinh = x.LoaiPhatSinh,
                    MaSanPham = x.MaSanPham,
                    TenSanPham = x.TenSanPham,
                    SoLuongNhap = x.SoLuongNhap > 0 ? x.SoLuongNhap.ToString("N0") : "-",
                    DonGiaNhap = x.DonGiaNhap > 0 ? x.DonGiaNhap.ToString("N0") : "-",
                    PhaiTra = x.PhaiTra > 0 ? x.PhaiTra.ToString("N0") : "-",
                    DaThanhToan = x.DaThanhToan > 0 ? x.DaThanhToan.ToString("N0") : "-",
                    ConNoLuyKe = x.ConNoLuyKe.ToString("N0")
                }).ToList();

                string fileExtension;
                var bytes = _excelExportService.Export("BCNCC", exportData, out fileExtension, variables);
                string contentType = fileExtension == "xls" ? "application/vnd.ms-excel" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, $"BaoCaoDoiChieuNhapNCC_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi xuất Excel: {ex.Message}");
            }
        }
    }
}
