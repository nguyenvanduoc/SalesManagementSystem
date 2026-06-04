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
    public class NKTongHopRepository : INKTongHopRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public NKTongHopRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<NKTongHopViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            using (var conn = _connectionFactory.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@Keyword", keyword ?? "");
                p.Add("@Page", page);
                p.Add("@PageSize", pageSize);

                string sql = @"
                    SELECT count(*) FROM NK_TongHop nk
                    LEFT JOIN Acl_Login al ON nk.IDLogin = al.ID
                    WHERE (@Keyword = '' OR nk.TenManHinh LIKE N'%' + @Keyword + '%' OR al.TenDangNhap LIKE N'%' + @Keyword + '%');

                    SELECT nk.ID, nk.IDLogin, nk.TenController, nk.TenAction, nk.NgayThucThi, nk.NoiDung,
                           ISNULL((SELECT TOP 1 am.TenManHinh FROM ACL_Action aa JOIN ACL_ManHinh am ON aa.IDManHinh = am.ID WHERE aa.TenController = nk.TenController), nk.TenManHinh) as TenManHinh,
                           al.TenDangNhap, al.HoDem + ' ' + al.Ten as TenNhanVien
                    FROM NK_TongHop nk
                    LEFT JOIN Acl_Login al ON nk.IDLogin = al.ID
                    WHERE (@Keyword = '' OR nk.TenManHinh LIKE N'%' + @Keyword + '%' OR al.TenDangNhap LIKE N'%' + @Keyword + '%')
                    ORDER BY nk.NgayThucThi DESC
                    OFFSET (@Page - 1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY;";

                using (var multi = conn.QueryMultiple(sql, p))
                {
                    totalRecords = multi.Read<int>().Single();
                    var result = multi.Read<NKTongHopViewModel>().ToList();
                    return result;
                }
            }
        }

        public NKTongHopViewModel GetById(int id)
        {
            using (var conn = _connectionFactory.CreateConnection())
            {
                string sql = @"
                    SELECT nk.*, al.TenDangNhap, al.HoDem + ' ' + al.Ten as TenNhanVien
                    FROM NK_TongHop nk
                    LEFT JOIN Acl_Login al ON nk.IDLogin = al.ID
                    WHERE nk.ID = @Id";
                return conn.QueryFirstOrDefault<NKTongHopViewModel>(sql, new { Id = id });
            }
        }
    }
}
