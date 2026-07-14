using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class BaoCaoCongNoVanChuyenController : BaseController
    {
        private readonly IPhieuChiRepository _phieuChiRepo;

        public BaoCaoCongNoVanChuyenController(IPhieuChiRepository phieuChiRepo)
        {
            _phieuChiRepo = phieuChiRepo;
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
                        var phieuNhap = conn.QueryFirstOrDefault(@"
                            SELECT pn.ID, pn.SoChungTu, pn.NgayNhap, ncc.TenNhaCungCap,
                                   pn.GhiChu, pn.TongTienHang, pn.TongTienThue, pn.TongCong,
                                   ISNULL(pn.TienVanChuyen, 0) AS TienVanChuyen,
                                   pt.TenPhuongTien AS TenPhuongTien,
                                   LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))) AS NguoiTaoTen
                            FROM KHO_PhieuNhap pn
                            LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
                            LEFT JOIN DM_PhuongTien pt ON pn.IDPhuongTien = pt.ID
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

                        if (phieuNhap != null)
                        {
                            ViewBag.PN_SoChungTu = (string)(phieuNhap.SoChungTu ?? "");
                            ViewBag.PN_TenNCC = (string)(phieuNhap.TenNhaCungCap ?? "");
                            ViewBag.PN_TenPhuongTien = (string)(phieuNhap.TenPhuongTien ?? "");
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

        public ActionResult GetList(int? idPhuongTien = null, string tuNgay = "", string denNgay = "", string soPhieuNhap = "", int? trangThaiThanhToan = null, int page = 1, int pageSize = 20)
        {
            if (!PermissionHelper.HasPermission("BaoCaoCongNoVanChuyen", LoaiPhanQuyen.Xem))
                return HttpNotFound();

            var data = _phieuChiRepo.GetPhieuNhapThanhToanVanChuyen(idPhuongTien, tuNgay, denNgay, soPhieuNhap, trangThaiThanhToan, page, pageSize).ToList();
            
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
    }
}
