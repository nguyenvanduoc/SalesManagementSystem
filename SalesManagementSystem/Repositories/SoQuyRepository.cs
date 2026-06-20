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
    public class SoQuyRepository : ISoQuyRepository
    {
        private readonly DbConnectionFactory _db;

        public SoQuyRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<SoQuyViewModel> GetList(
            string tuNgay,
            string denNgay,
            int? idTaiKhoanThanhToan)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay",               string.IsNullOrEmpty(tuNgay)  ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay",              string.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay));
                p.Add("@IDTaiKhoanThanhToan",  idTaiKhoanThanhToan);

                return conn.Query<SoQuyViewModel>(
                    "sp_KT_SoQuy_GetList",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public IEnumerable<TaiKhoanSummaryViewModel> GetTaiKhoanSummary(
            string tuNgay,
            string denNgay,
            int? idTaiKhoanThanhToan)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay));
                p.Add("@IDTaiKhoanThanhToan", idTaiKhoanThanhToan);

                return conn.Query<TaiKhoanSummaryViewModel>(
                    "sp_KT_SoQuy_GetTaiKhoanSummary",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public decimal GetOpeningBalance(string tuNgay, int idTaiKhoanThanhToan)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@IDTaiKhoanThanhToan", idTaiKhoanThanhToan);

                return conn.ExecuteScalar<decimal>(
                    "sp_KT_SoQuy_GetOpeningBalance",
                    p,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public IEnumerable<GiaoDichChiTietViewModel> GetGiaoDichChiTiet(
            string tuNgay,
            string denNgay,
            int idTaiKhoanThanhToan)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay));
                p.Add("@IDTaiKhoanThanhToan", idTaiKhoanThanhToan);

                return conn.Query<GiaoDichChiTietViewModel>(
                    "sp_KT_SoQuy_GetGiaoDichChiTiet",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }
    }
}
