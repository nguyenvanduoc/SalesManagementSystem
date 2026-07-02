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
            
            // Mặc định từ ngày đầu tháng đến cuối tháng
            var now = DateTime.Now;
            model.Filter.TuNgay = new DateTime(now.Year, now.Month, 1);
            model.Filter.DenNgay = model.Filter.TuNgay.Value.AddMonths(1).AddDays(-1);

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
                parameters.Add("@TuNgay", filter.TuNgay ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
                parameters.Add("@DenNgay", filter.DenNgay ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1));
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
