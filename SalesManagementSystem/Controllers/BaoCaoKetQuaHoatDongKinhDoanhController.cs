using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Services;
using SalesManagementSystem.Services.Interfaces;

namespace SalesManagementSystem.Controllers
{
    public class BaoCaoKetQuaHoatDongKinhDoanhController : BaseController
    {
        private readonly IExcelExportService _excelExportService;

        public BaoCaoKetQuaHoatDongKinhDoanhController(IExcelExportService excelExportService)
        {
            _excelExportService = excelExportService;
        }

        public ActionResult Index()
        {
            if (!PermissionHelper.HasPermission("BaoCaoKetQuaHoatDongKinhDoanh", LoaiPhanQuyen.Xem))
                return RedirectToAction("UnAuthorized", "Home");

            var model = new BaoCaoKetQuaHoatDongKinhDoanhViewModel();
            
            // Mac dinh tu ngay dau thang den hien tai, giong form nhap hang.
            var now = DateTime.Now;
            model.Filter.TuNgay = new DateTime(now.Year, 1, 1);
            model.Filter.DenNgay = now.Date;

            ViewBag.KhoHangList = GetKhoHangList();

            return View(model);
        }

        [HttpPost]
        public ActionResult GetList(BaoCaoKetQuaHoatDongKinhDoanhFilterModel filter)
        {
            if (!PermissionHelper.HasPermission("BaoCaoKetQuaHoatDongKinhDoanh", LoaiPhanQuyen.Xem))
                return Json(new { success = false, message = "Bạn không có quyền xem báo cáo này" });

            try
            {
                var data = GetDataFromStoredProcedure(filter);
                var model = BuildViewModel(filter, data);
                return PartialView("_List", model);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private List<BaoCaoKetQuaHoatDongKinhDoanhRowModel> GetDataFromStoredProcedure(BaoCaoKetQuaHoatDongKinhDoanhFilterModel filter)
        {
            using (var conn = new DbConnectionFactory().CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TuNgay", filter.TuNgay ?? new DateTime(DateTime.Now.Year, 1, 1));
                parameters.Add("@DenNgay", filter.DenNgay ?? DateTime.Now.Date);
                parameters.Add("@IDKho", filter.IDKho);
                parameters.Add("@IDSanPham", filter.IDSanPham);
                parameters.Add("@DonViTinh", filter.DonViTinh);
                parameters.Add("@MaSanPham", filter.MaSanPham);
                parameters.Add("@TenSanPham", filter.TenSanPham);

                var data = conn.Query<BaoCaoKetQuaHoatDongKinhDoanhRowModel>(
                    "sp_BC_KetQuaHoatDongKinhDoanh_GetList",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return data;
            }
        }

        private BaoCaoKetQuaHoatDongKinhDoanhViewModel BuildViewModel(BaoCaoKetQuaHoatDongKinhDoanhFilterModel filter, List<BaoCaoKetQuaHoatDongKinhDoanhRowModel> data)
        {
            var model = new BaoCaoKetQuaHoatDongKinhDoanhViewModel
            {
                Filter = filter
            };

            var resultData = new List<BaoCaoKetQuaHoatDongKinhDoanhRowModel>();
            
            int stt = 1;
            foreach (var c in data)
            {
                c.STT = stt++;
                c.IsGroup = false;
                resultData.Add(c);
            }
            
            model.Data = resultData;

            // Compute totals for cards
            model.TotalDoanhThu = data.Sum(x => x.ThanhTienDoanhThu);
            model.TotalGiaVon = data.Sum(x => x.ThanhTienGiaVon);
            model.TotalLoiNhuanGop = data.Sum(x => x.LoiNhuanGop);
            model.TotalChiPhiVanChuyen = data.Sum(x => x.ChiPhiVanChuyen);
            model.TotalChiPhiBaoBi = data.Sum(x => x.ChiPhiBaoBi);
            model.TotalLoiNhuanThuan = data.Sum(x => x.LoiNhuanThuan);
            model.TotalTySuatLN = model.TotalDoanhThu > 0 ? (model.TotalLoiNhuanThuan / model.TotalDoanhThu) * 100 : 0;

            return model;
        }

        public ActionResult ExportExcel(BaoCaoKetQuaHoatDongKinhDoanhFilterModel filter)
        {
            if (!PermissionHelper.HasPermission("BaoCaoKetQuaHoatDongKinhDoanh", LoaiPhanQuyen.Xem))
                return Content("Bạn không có quyền xuất dữ liệu này");

            var data = GetDataFromStoredProcedure(filter);
            var model = BuildViewModel(filter, data);
            
            var exportData = model.Data.Select((x, index) => new
            {
                STT = x.STT,
                MaSanPham = x.MaSanPham,
                TenSanPham = x.TenSanPham,
                DonViTinh = x.DonViTinh,
                SoLuongDoanhThu = x.SoLuongDoanhThu,
                ThanhTienDoanhThu = x.ThanhTienDoanhThu,
                SoLuongGiaVon = x.SoLuongGiaVon,
                ThanhTienGiaVon = x.ThanhTienGiaVon,
                ChiPhiVanChuyen = x.ChiPhiVanChuyen,
                ChiPhiBaoBi = x.ChiPhiBaoBi,
                LoiNhuanGop = x.LoiNhuanGop,
                LoiNhuanThuan = x.LoiNhuanThuan,
                TySuatLoiNhuan = x.TySuatLoiNhuan / 100 // Excel % format
            });

            var variables = new Dictionary<string, object>
            {
                { "TuNgay", filter.TuNgay.HasValue ? filter.TuNgay.Value.ToString("dd/MM/yyyy") : "" },
                { "DenNgay", filter.DenNgay.HasValue ? filter.DenNgay.Value.ToString("dd/MM/yyyy") : "" },
                { "TotalDoanhThu", model.TotalDoanhThu },
                { "TotalGiaVon", model.TotalGiaVon },
                { "TotalChiPhiVanChuyen", model.TotalChiPhiVanChuyen },
                { "TotalChiPhiBaoBi", model.TotalChiPhiBaoBi },
                { "TotalLoiNhuanGop", model.TotalLoiNhuanGop },
                { "TotalLoiNhuanThuan", model.TotalLoiNhuanThuan },
                { "TotalTySuatLN", model.TotalTySuatLN / 100 }
            };

            var fileBytes = _excelExportService.Export("KQHDKD_BaoCaoKetQuaKinhDoanh", exportData, out string ext, variables);
            
            string fileName = $"BaoCaoKetQuaHDKD_{DateTime.Now:yyyyMMddHHmmss}.{ext}";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public ActionResult GetDetails(int idSanPham, DateTime? tuNgay, DateTime? denNgay, int? idKho)
        {
            if (!PermissionHelper.HasPermission("BaoCaoKetQuaHoatDongKinhDoanh", LoaiPhanQuyen.Xem))
                return Json(new { success = false, message = "Bạn không có quyền xem chi tiết báo cáo này" });

            try
            {
                using (var conn = new DbConnectionFactory().CreateConnection())
                {
                    var sanPham = conn.QueryFirstOrDefault("SELECT MaSanPham, TenSanPham, DVT FROM DM_SanPham WHERE ID = @ID", new { ID = idSanPham });
                    ViewBag.MaSanPham = sanPham?.MaSanPham ?? "";
                    ViewBag.TenSanPham = sanPham?.TenSanPham ?? "";
                    ViewBag.DVT = sanPham?.DVT ?? "";
                    
                    string sqlSales = @"
                        SELECT 
                            hd.SoChungTu,
                            hd.NgayChungTu,
                            ct.SoLuong,
                            ct.DonGia,
                            ct.ThanhTien,
                            kh.TenKhachHang
                        FROM BAN_ChungTuBanHang_ChiTiet ct
                        INNER JOIN BAN_ChungTuBanHang hd ON ct.IDChungTuBanHang = hd.ID
                        LEFT JOIN NS_KhachHang kh ON hd.IDKhachHang = kh.ID
                        WHERE hd.IsDeleted = 0 
                          AND hd.TrangThai = 2
                          AND ct.IDSanPham = @IDSanPham
                          AND hd.NgayChungTu >= @TuNgay AND hd.NgayChungTu <= @DenNgay
                          AND (@IDKho IS NULL OR hd.IDKho = @IDKho)
                        ORDER BY hd.NgayChungTu DESC, hd.ID DESC";

                    var salesList = conn.Query(sqlSales, new { 
                        IDSanPham = idSanPham, 
                        TuNgay = tuNgay ?? new DateTime(DateTime.Now.Year, 1, 1),
                        DenNgay = denNgay ?? DateTime.Now.Date,
                        IDKho = idKho
                    }).ToList();

                    string sqlPurchases = @"
                        SELECT 
                            p.SoChungTu,
                            p.NgayNhap,
                            ct.SoLuong,
                            ct.DonGia,
                            ct.SoLuong * ct.DonGia AS ThanhTienHang,
                            ISNULL(ct.DonGiaVanChuyen, 0) AS DonGiaVanChuyen,
                            ISNULL(ct.TienVanChuyen, 0) AS TienVanChuyen
                        FROM KHO_PhieuNhap_ChiTiet ct
                        INNER JOIN KHO_PhieuNhap p ON ct.IDPhieuNhap = p.ID
                        WHERE p.IsDeleted = 0 
                          AND p.TrangThai = 2
                          AND ct.IDSanPham = @IDSanPham
                          AND p.NgayNhap <= @DenNgay
                        ORDER BY p.NgayNhap DESC, p.ID DESC";

                    var purchaseList = conn.Query(sqlPurchases, new { 
                        IDSanPham = idSanPham,
                        DenNgay = denNgay ?? DateTime.Now.Date
                    }).ToList();

                    ViewBag.SalesDetails = salesList;
                    ViewBag.PurchaseDetails = purchaseList;

                    return PartialView("_DetailsModal");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private SelectList GetKhoHangList()
        {
            using (var conn = new DbConnectionFactory().CreateConnection())
            {
                var list = conn.Query("SELECT ID, TenKhoHang FROM DM_KhoHang ORDER BY TenKhoHang")
                               .Select(x => new { ID = (int)x.ID, TenKhoHang = (string)x.TenKhoHang })
                               .ToList();
                return new SelectList(list, "ID", "TenKhoHang");
            }
        }
    }
}
