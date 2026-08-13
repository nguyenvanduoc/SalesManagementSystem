using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Repositories.Interfaces;
using SalesManagementSystem.Services.Interfaces;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Controllers
{
    public class BaoCaoCongNoVanChuyenController : BaseController
    {
        private readonly IPhieuChiRepository _phieuChiRepo;
        private readonly IExcelExportService _excelExportService;

        public BaoCaoCongNoVanChuyenController(IPhieuChiRepository phieuChiRepo, IExcelExportService excelExportService)
        {
            _phieuChiRepo = phieuChiRepo;
            _excelExportService = excelExportService;
        }

        public ActionResult Index()
        {
            if (!PermissionHelper.HasPermission("BaoCaoCongNoVanChuyen", LoaiPhanQuyen.Xem))
                return View("AccessDenied");

            PopulateFilterDropdowns();
            return View();
        }

        [HttpGet]
        public ActionResult GetDetails(int idPhatSinh, int loaiDong)
        {
            if (!PermissionHelper.HasPermission("BaoCaoCongNoVanChuyen", LoaiPhanQuyen.Xem))
                return HttpNotFound();

            try
            {
                using (var conn = new DbConnectionFactory().CreateConnection())
                {
                    if (loaiDong == 1) // Phiếu nhập kho
                    {
                        dynamic phieuNhap = null;
                        IEnumerable<dynamic> chiTiets = null;
                        using (var multi = conn.QueryMultiple("sp_BaoCao_CongNoVanChuyen_GetDetails", new { IDPhieuNhap = idPhatSinh }, commandType: System.Data.CommandType.StoredProcedure))
                        {
                            phieuNhap = multi.ReadFirstOrDefault();
                            chiTiets = multi.Read().ToList();
                        }

                        if (phieuNhap != null)
                        {
                            ViewBag.PN_SoChungTu = (string)(phieuNhap.SoChungTu ?? "");
                            ViewBag.PN_TenNCC = (string)(phieuNhap.TenNhaCungCap ?? "");
                            ViewBag.PN_TenPhuongTien = (string)(phieuNhap.TenPhuongTien ?? "");
                            ViewBag.PN_NgayNhap = (DateTime?)phieuNhap.NgayNhap;
                            ViewBag.PN_NgayGiaoHang = (DateTime?)phieuNhap.NgayGiaoHang;
                            ViewBag.PN_TongTienHang = (decimal)(phieuNhap.TongTienHang ?? 0m);
                            ViewBag.PN_TongCong = (decimal)(phieuNhap.TongCong ?? 0m);
                            ViewBag.PN_TienVanChuyen = (decimal)(phieuNhap.TienVanChuyen ?? 0m);
                            ViewBag.PN_GhiChu = (string)(phieuNhap.GhiChu ?? "");
                            ViewBag.PN_NguoiTao = (string)(phieuNhap.NguoiTaoTen ?? "");
                        }
                        else
                        {
                            ViewBag.PN_SoChungTu = ""; ViewBag.PN_TenNCC = "";
                            ViewBag.PN_TenPhuongTien = "";
                            ViewBag.PN_NgayNhap = null;
                            ViewBag.PN_TongTienHang = 0m; ViewBag.PN_TongCong = 0m;
                            ViewBag.PN_TienVanChuyen = 0m; ViewBag.PN_GhiChu = "";
                            ViewBag.PN_NguoiTao = "";
                        }

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

                    return PartialView("~/Views/BaoCaoCongNoVanChuyen/_DetailsModal.cshtml");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetList(int? idPhuongTien = null, string hoTenTaiXe = "", string tuNgay = "", string denNgay = "", string soPhieuNhap = "", int? trangThaiThanhToan = null, int page = 1, int pageSize = 20)
        {
            if (!PermissionHelper.HasPermission("BaoCaoCongNoVanChuyen", LoaiPhanQuyen.Xem))
                return HttpNotFound();

            var data = _phieuChiRepo.GetPhieuNhapThanhToanVanChuyen(idPhuongTien, hoTenTaiXe, tuNgay, denNgay, soPhieuNhap, trangThaiThanhToan, page, pageSize).ToList();
            
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            
            int totalRecords = data.Any() ? (int)((dynamic)data.First()).TotalRow : 0;
            ViewBag.TotalRecords = totalRecords;
            
            return PartialView("_List", data);
        }

        private void PopulateFilterDropdowns()
        {
            var phuongTiens = _phieuChiRepo.GetPhuongTienDropdown()
                .Select(x => new SelectListItem { Value = ((int)x.Value).ToString(), Text = (string)x.Text });
            ViewBag.PhuongTienList = new SelectList(phuongTiens.ToList(), "Value", "Text");
        }

        [HttpGet]
        public ActionResult ExportExcel(int? idPhuongTien = null, string hoTenTaiXe = "", string tuNgay = "", string denNgay = "", string soPhieuNhap = "", int? trangThaiThanhToan = null)
        {
            if (!PermissionHelper.HasPermission("BaoCaoCongNoVanChuyen", LoaiPhanQuyen.Xem))
                return Content("Không có quyền xuất Excel");

            try
            {
                // Get all rows by specifying a large page size (1,000,000)
                var data = _phieuChiRepo.GetPhieuNhapThanhToanVanChuyen(idPhuongTien, hoTenTaiXe, tuNgay, denNgay, soPhieuNhap, trangThaiThanhToan, 1, 1000000).ToList();

                string phuongTienName = "Tất cả";
                if (idPhuongTien.HasValue)
                {
                    using (var conn = new DbConnectionFactory().CreateConnection())
                    {
                        phuongTienName = conn.ExecuteScalar<string>(
                            "SELECT TenPhuongTien FROM DM_PhuongTien WHERE ID = @ID",
                            new { ID = idPhuongTien.Value }
                        ) ?? "Tất cả";
                    }
                }

                string strTuNgay = "";
                string strDenNgay = "";
                if (DateTime.TryParse(tuNgay, out DateTime dTu)) strTuNgay = dTu.ToString("dd/MM/yyyy");
                if (DateTime.TryParse(denNgay, out DateTime dDen)) strDenNgay = dDen.ToString("dd/MM/yyyy");

                var session = (UserLoginViewModel)Session[CommonConstants.USER_SESSION];
                string nguoiLapBieu = session != null ? (session.HoDem + " " + session.Ten).Trim() : "Hệ thống";
                if (string.IsNullOrEmpty(nguoiLapBieu)) nguoiLapBieu = session?.UserName ?? "Hệ thống";

                var variables = new Dictionary<string, object>
                {
                    { "TuNgay", strTuNgay },
                    { "DenNgay", strDenNgay },
                    { "PhuongTien", phuongTienName },
                    { "TenPhuongTien", phuongTienName },
                    { "KhachHang", phuongTienName },
                    { "Ngay", DateTime.Now.ToString("dd") },
                    { "Thang", DateTime.Now.ToString("MM") },
                    { "Nam", DateTime.Now.ToString("yyyy") },
                    { "NguoiLapBieu", nguoiLapBieu }
                };

                int stt = 1;
                var exportData = data.Select(x => {
                    decimal tongTien = Convert.ToDecimal(x.TongTienVanChuyen);
                    decimal daThanhToan = Convert.ToDecimal(x.DaThanhToanVanChuyen);
                    decimal conLai = Convert.ToDecimal(x.ConLaiVanChuyen);

                    string trangThaiText = "Chưa thanh toán";
                    if (daThanhToan >= tongTien && tongTien > 0)
                    {
                        trangThaiText = "Đã thanh toán đủ";
                    }
                    else if (daThanhToan > 0)
                    {
                        trangThaiText = "Thanh toán một phần";
                    }

                    return new {
                        STT = stt++,
                        SoPhieu = (string)(x.SoPhieuNhap ?? ""),
                        SoPhieuNhap = (string)(x.SoPhieuNhap ?? ""),
                        NgayNhap = x.NgayNhap != null ? ((DateTime)x.NgayNhap).ToString("dd/MM/yyyy") : "",
                        NgayGiao = x.NgayGiaoHang != null ? ((DateTime)x.NgayGiaoHang).ToString("dd/MM/yyyy") : "",
                        NgayGiaoHang = x.NgayGiaoHang != null ? ((DateTime)x.NgayGiaoHang).ToString("dd/MM/yyyy") : "",
                        NguoiGiaoHang = (string)(x.TenNguoiGiao ?? ""),
                        TenNguoiGiao = (string)(x.TenNguoiGiao ?? ""),
                        TenPhuongTien = (string)(x.TenPhuongTien ?? ""),
                        PhuongTien = (string)(x.TenPhuongTien ?? ""),
                        SoPhieuChi = (string)(x.SoPhieuChiList ?? ""),
                        SoPhieuChiList = (string)(x.SoPhieuChiList ?? ""),
                        TongTienVanChuyen = tongTien,
                        TongTien = tongTien,
                        DaThanhToan = daThanhToan,
                        DaThanhToanVanChuyen = daThanhToan,
                        ConLai = conLai,
                        ConLaiVanChuyen = conLai,
                        TenTrangThai = trangThaiText,
                        TrangThai = trangThaiText
                    };
                }).ToList();

                string fileExtension;
                var bytes = _excelExportService.Export("CNVC01", exportData, out fileExtension, variables);
                string contentType = fileExtension == "xls" ? "application/vnd.ms-excel" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, $"BaoCaoCongNoVanChuyen_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi xuất Excel: {ex.Message}");
            }
        }
    }
}
