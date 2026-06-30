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

        private DateTime? ParseDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr)) return null;
            string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "d/M/yyyy", "yyyy-M-d" };
            if (DateTime.TryParseExact(dateStr.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
            if (DateTime.TryParse(dateStr, out result))
            {
                return result;
            }
            return null;
        }

        public IEnumerable<SoQuyViewModel> GetList(
            string tuNgay,
            string denNgay,
            int? idTaiKhoanThanhToan)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay",               ParseDate(tuNgay));
                p.Add("@DenNgay",              ParseDate(denNgay));
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
                p.Add("@TuNgay", ParseDate(tuNgay));
                p.Add("@DenNgay", ParseDate(denNgay));
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
                p.Add("@TuNgay", ParseDate(tuNgay));
                p.Add("@IDTaiKhoanThanhToan", idTaiKhoanThanhToan);

                return conn.ExecuteScalar<decimal?>(
                    "sp_KT_SoQuy_GetOpeningBalance",
                    p,
                    commandType: CommandType.StoredProcedure
                ) ?? 0M;
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
                p.Add("@TuNgay", ParseDate(tuNgay));
                p.Add("@DenNgay", ParseDate(denNgay));
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
