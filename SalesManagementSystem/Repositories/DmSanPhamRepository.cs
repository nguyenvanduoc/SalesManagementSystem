using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class DmSanPhamRepository : IDmSanPhamRepository
    {
        private readonly DbConnectionFactory _dbFactory;

        public DmSanPhamRepository(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public List<DmSanPhamViewModel> GetPaged(int pageIndex, int pageSize, string keyword, out int totalRecords)
        {
            totalRecords = 0;
            using (var conn = _dbFactory.CreateConnection())
            {
                string countSql = "SELECT COUNT(1) FROM DM_SanPham WHERE @Keyword = '' OR TenSanPham LIKE @LikeKeyword OR MaSanPham LIKE @LikeKeyword";
                totalRecords = conn.ExecuteScalar<int>(countSql, new { Keyword = keyword ?? "", LikeKeyword = $"%{(keyword ?? "").Trim()}%" });

                string sql = @"
                    SELECT sp.*, ISNULL(nv.HoDem, '') + ' ' + ISNULL(nv.Ten, '') as TenNguoiTao
                    FROM DM_SanPham sp
                    LEFT JOIN NS_NhanSu nv ON sp.NguoiTao = nv.ID
                    WHERE @Keyword = '' OR sp.TenSanPham LIKE @LikeKeyword OR sp.MaSanPham LIKE @LikeKeyword
                    ORDER BY ISNULL(sp.STT, 999999), sp.ID DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                ";

                var list = conn.Query<DmSanPhamViewModel>(sql, new
                {
                    Keyword = keyword ?? "",
                    LikeKeyword = $"%{(keyword ?? "").Trim()}%",
                    Offset = (pageIndex - 1) * pageSize,
                    PageSize = pageSize
                }).ToList();

                return list;
            }
        }

        public DM_SanPham GetById(int id)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                return conn.QueryFirstOrDefault<DM_SanPham>("SELECT * FROM DM_SanPham WHERE ID = @Id", new { Id = id });
            }
        }

        public bool CheckDuplicateCode(string maSanPham, int id = 0)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                string sql = "SELECT COUNT(1) FROM DM_SanPham WHERE MaSanPham = @MaSanPham AND ID != @Id";
                int count = conn.ExecuteScalar<int>(sql, new { MaSanPham = maSanPham, Id = id });
                return count > 0;
            }
        }

        public int Insert(DM_SanPham entity)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                string sql = @"
                    INSERT INTO DM_SanPham (TenSanPham, MaSanPham, DVT, STT, NgayTao, NguoiTao, NgayCapNhat, NguoiCapNhat)
                    VALUES (@TenSanPham, @MaSanPham, @DVT, @STT, @NgayTao, @NguoiTao, @NgayCapNhat, @NguoiCapNhat);
                    SELECT CAST(SCOPE_IDENTITY() as int);
                ";
                return conn.QuerySingle<int>(sql, entity);
            }
        }

        public bool Update(DM_SanPham entity)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                string sql = @"
                    UPDATE DM_SanPham
                    SET TenSanPham = @TenSanPham,
                        MaSanPham = @MaSanPham,
                        DVT = @DVT,
                        STT = @STT,
                        NgayCapNhat = @NgayCapNhat,
                        NguoiCapNhat = @NguoiCapNhat
                    WHERE ID = @ID
                ";
                return conn.Execute(sql, entity) > 0;
            }
        }

        public bool Delete(int id)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                return conn.Execute("DELETE FROM DM_SanPham WHERE ID = @Id", new { Id = id }) > 0;
            }
        }
    }
}
