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
    public class BaoCaoDoiChieuCongNoKhachHangRepository : IBaoCaoDoiChieuCongNoKhachHangRepository
    {
        private readonly DbConnectionFactory _db;

        public BaoCaoDoiChieuCongNoKhachHangRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<BaoCaoDoiChieuCongNoKhachHangViewModel> GetList(int? idKhachHang, DateTime tuNgay, DateTime denNgay, string soChungTu = null)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@TuNgay", tuNgay);
                p.Add("@DenNgay", denNgay);
                p.Add("@SoChungTu", string.IsNullOrEmpty(soChungTu) ? null : soChungTu);

                return conn.Query<BaoCaoDoiChieuCongNoKhachHangViewModel>(
                    "sp_BaoCao_DoiChieuCongNoKhachHang",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetKhachHangDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = @"
                    SELECT ID, 
                           ISNULL(MaKhachHang, '') + ' - ' + ISNULL(TenKhachHang, '') AS TenHienThi
                    FROM NS_KhachHang
                    ORDER BY TenKhachHang";
                return conn.Query(sql).ToList();
            }
        }
    }
}
