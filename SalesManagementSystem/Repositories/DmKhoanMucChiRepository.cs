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
    public class DmKhoanMucChiRepository : IDmKhoanMucChiRepository
    {
        private readonly DbConnectionFactory _db;

        public DmKhoanMucChiRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<DmKhoanMucChiViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var sqlCount = @"
                    SELECT COUNT(*) 
                    FROM DM_KhoanMucChi 
                    WHERE @Keyword = '' OR TenKhoanMuc LIKE N'%' + @Keyword + '%' OR MaKhoanMuc LIKE N'%' + @Keyword + '%'";
                
                totalRecords = conn.ExecuteScalar<int>(sqlCount, new { Keyword = keyword ?? "" });

                var sqlList = @"
                    SELECT 
                        km.*,
                        nt.HoTen AS TenNguoiTao,
                        nc.HoTen AS TenNguoiCapNhat
                    FROM DM_KhoanMucChi km
                    LEFT JOIN NS_NhanVien nt ON km.NguoiTao = nt.ID
                    LEFT JOIN NS_NhanVien nc ON km.NguoiCapNhat = nc.ID
                    WHERE @Keyword = '' OR km.TenKhoanMuc LIKE N'%' + @Keyword + '%' OR km.MaKhoanMuc LIKE N'%' + @Keyword + '%'
                    ORDER BY km.MaKhoanMuc
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                return conn.Query<DmKhoanMucChiViewModel>(sqlList, new 
                { 
                    Keyword = keyword ?? "",
                    Offset = (page - 1) * pageSize,
                    PageSize = pageSize
                }).ToList();
            }
        }

        public DM_KhoanMucChi GetById(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<DM_KhoanMucChi>("SELECT * FROM DM_KhoanMucChi WHERE ID = @Id", new { Id = id });
            }
        }

        public int Insert(DM_KhoanMucChi entity)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = @"
                    INSERT INTO DM_KhoanMucChi (MaKhoanMuc, TenKhoanMuc, IsHoatDong, NgayTao, NguoiTao) 
                    VALUES (@MaKhoanMuc, @TenKhoanMuc, @IsHoatDong, @NgayTao, @NguoiTao);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
                return conn.ExecuteScalar<int>(sql, entity);
            }
        }

        public void Update(DM_KhoanMucChi entity)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = @"
                    UPDATE DM_KhoanMucChi 
                    SET MaKhoanMuc = @MaKhoanMuc, 
                        TenKhoanMuc = @TenKhoanMuc, 
                        IsHoatDong = @IsHoatDong, 
                        NgayCapNhat = @NgayCapNhat, 
                        NguoiCapNhat = @NguoiCapNhat
                    WHERE ID = @ID";
                conn.Execute(sql, entity);
            }
        }

        public void Delete(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute("DELETE FROM DM_KhoanMucChi WHERE ID = @Id", new { Id = id });
            }
        }

        public bool CheckDuplicateCode(string code, int excludeId = 0)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = "SELECT COUNT(1) FROM DM_KhoanMucChi WHERE MaKhoanMuc = @MaKhoanMuc AND ID != @ExcludeId";
                return conn.ExecuteScalar<int>(sql, new { MaKhoanMuc = code, ExcludeId = excludeId }) > 0;
            }
        }
    }
}
