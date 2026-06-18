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
    public class TonKhoRepository : ITonKhoRepository
    {
        private readonly DbConnectionFactory _db;

        public TonKhoRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<TonKhoListViewModel> GetList(
            int? idKho, 
            int? idSanPham, 
            string tuNgay, 
            string denNgay, 
            bool chiConTon)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDKho", idKho);
                p.Add("@IDSanPham", idSanPham);
                p.Add("@TuNgay", string.IsNullOrWhiteSpace(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrWhiteSpace(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay).AddDays(1).AddSeconds(-1));
                p.Add("@ChiConTon", chiConTon);

                return conn.Query<TonKhoListViewModel>(
                    "sp_KHO_TonKho_GetList", 
                    p, 
                    commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public IEnumerable<TheKhoListViewModel> GetTheKho(
            int idKho, 
            int idSanPham, 
            string tuNgay, 
            string denNgay)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDKho", idKho);
                p.Add("@IDSanPham", idSanPham);
                p.Add("@TuNgay", string.IsNullOrWhiteSpace(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrWhiteSpace(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay).AddDays(1).AddSeconds(-1));

                return conn.Query<TheKhoListViewModel>(
                    "sp_KHO_TheKho_GetList", 
                    p, 
                    commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public TonKhoDashboardViewModel GetDashboard(
            int? idKho, 
            int? idSanPham,
            string tuNgay, 
            string denNgay,
            bool chiConTon)
        {
            var list = GetList(idKho, idSanPham, tuNgay, denNgay, chiConTon);
            
            var dashboard = new TonKhoDashboardViewModel
            {
                TongSoSanPham = list.Count(),
                TongSoLuongTon = list.Sum(x => x.TonKho),
                TongGiaTriTon = list.Sum(x => x.GiaTriTon),
                SoSanPhamAmKho = list.Count(x => x.TonKho < 0),
                SoSanPhamSapHetHang = list.Count(x => x.TonKho > 0 && x.TonKho <= 10)
            };

            return dashboard;
        }
    }
}
