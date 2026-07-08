using System;
using System.Linq;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using Dapper;
using SalesManagementSystem.Data;

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
            ViewBag.Title = "BÁO CÁO ĐỐI CHIẾU NHẬP NCC";

            var nccs = _repo.GetNhaCungCapDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.ID).ToString(), Text = (string)x.TenHienThi });
            ViewBag.NhaCungCapList = new SelectList(nccs.ToList(), "Value", "Text");

            ViewBag.TuNgay = new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyy-MM-dd");
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
        public ActionResult GetDetails(int idPhatSinh, int loaiDong)
        {
            try
            {
                using (var conn = new DbConnectionFactory().CreateConnection())
                {
                    if (loaiDong == 1) // Phiếu nhập kho
                    {
                        var phieuNhap = conn.QueryFirstOrDefault(@"
                            SELECT pn.ID, pn.SoChungTu, pn.NgayNhap, ncc.TenNhaCungCap,
                                   pn.GhiChu, pn.TongTienHang, pn.TongTienThue, pn.TongCong,
                                   ISNULL(pn.TienVanChuyen, 0) AS TienVanChuyen,
                                   LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))) AS NguoiTaoTen
                            FROM KHO_PhieuNhap pn
                            LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
                            LEFT JOIN NS_NhanSu ns ON pn.NguoiTao = ns.ID
                            WHERE pn.ID = @ID AND pn.IsDeleted = 0", new { ID = idPhatSinh });

                        var chiTiets = conn.Query(@"
                            SELECT ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, sp.DVT,
                                   ct.SoLuong, ct.DonGia, ct.ThanhTien,
                                   ISNULL(ct.DonGiaVanChuyen, 0) AS DonGiaVanChuyen,
                                   ISNULL(ct.TienVanChuyen, 0) AS TienVanChuyen,
                                   ISNULL(ct.TongSauThue, ct.ThanhTien) AS TongSauThue,
                                   ct.GhiChu
                            FROM KHO_PhieuNhap_ChiTiet ct
                            LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                            WHERE ct.IDPhieuNhap = @IDPhieuNhap
                            ORDER BY ct.ID", new { IDPhieuNhap = idPhatSinh }).ToList();

                        // Pre-process sang typed scalar để Razor không cần dynamic cast
                        if (phieuNhap != null)
                        {
                            ViewBag.PN_SoChungTu = (string)(phieuNhap.SoChungTu ?? "");
                            ViewBag.PN_TenNCC = (string)(phieuNhap.TenNhaCungCap ?? "");
                            ViewBag.PN_NgayNhap = (DateTime?)phieuNhap.NgayNhap;
                            ViewBag.PN_TongTienHang = (decimal)(phieuNhap.TongTienHang ?? 0m);
                            ViewBag.PN_TongCong = (decimal)(phieuNhap.TongCong ?? 0m);
                            ViewBag.PN_TienVanChuyen = (decimal)(phieuNhap.TienVanChuyen ?? 0m);
                            ViewBag.PN_GhiChu = (string)(phieuNhap.GhiChu ?? "");
                            ViewBag.PN_NguoiTao = (string)(phieuNhap.NguoiTaoTen ?? "");
                        }
                        else
                        {
                            ViewBag.PN_SoChungTu = ""; ViewBag.PN_TenNCC = "";
                            ViewBag.PN_NgayNhap = null;
                            ViewBag.PN_TongTienHang = 0m; ViewBag.PN_TongCong = 0m;
                            ViewBag.PN_TienVanChuyen = 0m; ViewBag.PN_GhiChu = "";
                            ViewBag.PN_NguoiTao = "";
                        }

                        // Chuyển sang named ViewModel để Razor truy cập property an toàn (anonymous type bị lỗi cross-assembly)
                        var chiTietList = chiTiets.Select(ct => new SalesManagementSystem.Models.ViewModels.PhieuNhapChiTietDetailViewModel
                        {
                            MaSanPham        = (string)(ct.MaSanPham ?? ""),
                            TenSanPham       = (string)(ct.TenSanPham ?? ""),
                            DVT              = (string)(ct.DVT ?? ""),
                            SoLuong          = (decimal)(ct.SoLuong ?? 0m),
                            DonGia           = (decimal)(ct.DonGia ?? 0m),
                            ThanhTien        = (decimal)(ct.ThanhTien ?? 0m),
                            DonGiaVanChuyen  = (decimal)(ct.DonGiaVanChuyen ?? 0m),
                            TienVanChuyen    = (decimal)(ct.TienVanChuyen ?? 0m),
                            TongSauThue      = (decimal)(ct.TongSauThue ?? 0m),
                            GhiChu           = (string)(ct.GhiChu ?? "")
                        }).ToList();

                        ViewBag.LoaiDong = 1;
                        ViewBag.ChiTiets = chiTietList;
                        ViewBag.TongThanhTien = chiTietList.Sum(x => x.ThanhTien);
                        ViewBag.TongVanChuyen = chiTietList.Sum(x => x.TienVanChuyen);
                    }
                    else if (loaiDong == 2) // Phiếu chi thanh toán
                    {
                        var phieuChi = conn.QueryFirstOrDefault(@"
                            SELECT pc.ID, pc.SoPhieuChi, pc.NgayChi, ncc.TenNhaCungCap,
                                   pc.SoTienChi, pc.DienGiai,
                                   tk.TenTaiKhoan, tk.LoaiTaiKhoan,
                                   ISNULL(tk.NganHang, '') AS NganHang,
                                   pc.IDPhieuNhap,
                                   pn.SoChungTu AS SoChungTuNhap,
                                   LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))) AS NguoiTaoTen
                            FROM KT_PhieuChi pc
                            LEFT JOIN DM_NhaCungCap ncc ON pc.IDNhaCungCap = ncc.ID
                            LEFT JOIN DM_TaiKhoanThanhToan tk ON pc.IDTaiKhoanThanhToan = tk.ID
                            LEFT JOIN KHO_PhieuNhap pn ON pc.IDPhieuNhap = pn.ID
                            LEFT JOIN NS_NhanSu ns ON pc.NguoiTao = ns.ID
                            WHERE pc.ID = @ID AND pc.IsDeleted = 0", new { ID = idPhatSinh });

                        if (phieuChi != null)
                        {
                            ViewBag.PC_SoPhieuChi     = (string)(phieuChi.SoPhieuChi ?? "");
                            ViewBag.PC_TenNCC          = (string)(phieuChi.TenNhaCungCap ?? "");
                            ViewBag.PC_NgayChi         = (DateTime?)phieuChi.NgayChi;
                            ViewBag.PC_SoTienChi       = (decimal)(phieuChi.SoTienChi ?? 0m);
                            ViewBag.PC_DienGiai        = (string)(phieuChi.DienGiai ?? "");
                            
                            // Safe processing of LoaiTaiKhoan (int)
                            string hinhThuc = "";
                            var ltk = phieuChi.LoaiTaiKhoan;
                            if (ltk != null)
                            {
                                int ltkInt = 0;
                                if (int.TryParse(ltk.ToString(), out ltkInt))
                                {
                                    if (ltkInt == 1) hinhThuc = "Tiền mặt";
                                    else if (ltkInt == 2) hinhThuc = "Chuyển khoản";
                                    else hinhThuc = "Loại khác (" + ltkInt + ")";
                                }
                            }
                            ViewBag.PC_HinhThuc        = hinhThuc;
                            ViewBag.PC_TenTaiKhoan     = (string)(phieuChi.TenTaiKhoan ?? "");
                            ViewBag.PC_NganHang        = (string)(phieuChi.NganHang ?? "");
                            ViewBag.PC_SoChungTuNhap   = (string)(phieuChi.SoChungTuNhap ?? "");
                            ViewBag.PC_NguoiTao        = (string)(phieuChi.NguoiTaoTen ?? "");
                        }
                        else
                        {
                            ViewBag.PC_SoPhieuChi = ""; ViewBag.PC_TenNCC = "";
                            ViewBag.PC_NgayChi = null; ViewBag.PC_SoTienChi = 0m;
                            ViewBag.PC_DienGiai = ""; ViewBag.PC_HinhThuc = "";
                            ViewBag.PC_TenTaiKhoan = ""; ViewBag.PC_NganHang = "";
                            ViewBag.PC_SoChungTuNhap = ""; ViewBag.PC_NguoiTao = "";
                        }


                        ViewBag.LoaiDong = 2;
                    }

                    return PartialView("_DetailsModal");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
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
