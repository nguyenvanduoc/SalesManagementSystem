using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class CongNoKhachHangRepository : ICongNoKhachHangRepository
    {
        private readonly DbConnectionFactory _db;

        public CongNoKhachHangRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<CongNoKhachHangViewModel> GetList(
            string tuNgay,
            string denNgay,
            int? idKhachHang,
            int? trangThaiCongNo)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay));
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@TrangThaiCongNo", trangThaiCongNo);

                return conn.Query<CongNoKhachHangViewModel>(
                    "sp_CongNoKhachHang_GetList",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public IEnumerable<CongNoKhachHangSP02ViewModel> GetExportSP02(
            string tuNgay,
            string denNgay)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay));

                var result = conn.Query<CongNoKhachHangSP02ViewModel>(
                    "sp_CongNoKhachHang_ExportSP02",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
                
                return result;
            }
        }

        public CongNoKhachHangDashboardViewModel GetDashboard(
            string tuNgay,
            string denNgay,
            int? idKhachHang)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay));
                p.Add("@IDKhachHang", idKhachHang);

                return conn.Query<CongNoKhachHangDashboardViewModel>(
                    "sp_CongNoKhachHang_GetDashboard",
                    p,
                    commandType: CommandType.StoredProcedure
                ).FirstOrDefault() ?? new CongNoKhachHangDashboardViewModel();
            }
        }

        public IEnumerable<CongNoKhachHangDetailViewModel> GetDetail(
            int idKhachHang,
            string tuNgay,
            string denNgay)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay));

                return conn.Query<CongNoKhachHangDetailViewModel>(
                    "sp_CongNoKhachHang_GetDetail",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetHistory(int idChungTuBanHang)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDChungTuBanHang", idChungTuBanHang);

                return conn.Query(
                    "sp_CongNoKhachHang_GetHistory",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetKhachHangDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "SELECT ID, TenKhachHang AS TenHienThi FROM NS_KhachHang ORDER BY TenKhachHang"
                ).ToList();
            }
        }
    }
}
