using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class DmKhoHangRepository : IDmKhoHangRepository
    {
        private readonly DbConnectionFactory _db;

        public DmKhoHangRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<DM_KhoHang> GetAll()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<DM_KhoHang>("SELECT * FROM DM_KhoHang ORDER BY ISNULL(STT, 9999), TenKhoHang").ToList();
            }
        }

        public IEnumerable<DmKhoHangViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var sqlCount = @"
                    SELECT COUNT(*) 
                    FROM DM_KhoHang 
                    WHERE @Keyword = '' OR TenKhoHang LIKE N'%' + @Keyword + '%' OR MaKhoHang LIKE N'%' + @Keyword + '%'";
                
                totalRecords = conn.ExecuteScalar<int>(sqlCount, new { Keyword = keyword ?? "" });

                var sqlList = @"
                    SELECT 
                        kh.*,
                        nt.HoTen AS TenNguoiTao,
                        nc.HoTen AS TenNguoiCapNhat
                    FROM DM_KhoHang kh
                    LEFT JOIN NS_NhanVien nt ON kh.NguoiTao = nt.ID
                    LEFT JOIN NS_NhanVien nc ON kh.NguoiCapNhat = nc.ID
                    WHERE @Keyword = '' OR kh.TenKhoHang LIKE N'%' + @Keyword + '%' OR kh.MaKhoHang LIKE N'%' + @Keyword + '%'
                    ORDER BY ISNULL(kh.STT, 9999), kh.NgayTao DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                return conn.Query<DmKhoHangViewModel>(sqlList, new 
                { 
                    Keyword = keyword ?? "",
                    Offset = (page - 1) * pageSize,
                    PageSize = pageSize
                }).ToList();
            }
        }

        public DM_KhoHang GetById(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<DM_KhoHang>("SELECT * FROM DM_KhoHang WHERE ID = @Id", new { Id = id });
            }
        }

        public int Insert(DM_KhoHang kh)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = @"
                    INSERT INTO DM_KhoHang (TenKhoHang, MaKhoHang, DiaChi, NguoiDaiDien, STT, IsKhoChinh, NgayTao, NguoiTao) 
                    VALUES (@TenKhoHang, @MaKhoHang, @DiaChi, @NguoiDaiDien, @STT, @IsKhoChinh, @NgayTao, @NguoiTao);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
                return conn.ExecuteScalar<int>(sql, kh);
            }
        }

        public void Update(DM_KhoHang kh)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = @"
                    UPDATE DM_KhoHang 
                    SET TenKhoHang = @TenKhoHang, 
                        MaKhoHang = @MaKhoHang, 
                        DiaChi = @DiaChi,
                        NguoiDaiDien = @NguoiDaiDien,
                        STT = @STT,
                        IsKhoChinh = @IsKhoChinh,
                        NgayCapNhat = @NgayCapNhat, 
                        NguoiCapNhat = @NguoiCapNhat
                    WHERE ID = @ID";
                conn.Execute(sql, kh);
            }
        }

        public void Delete(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute("DELETE FROM DM_KhoHang WHERE ID = @Id", new { Id = id });
            }
        }

        public bool CheckDuplicateCode(string maKhoHang, int excludeId = 0)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = "SELECT COUNT(1) FROM DM_KhoHang WHERE MaKhoHang = @MaKhoHang AND ID != @ExcludeId";
                return conn.ExecuteScalar<int>(sql, new { MaKhoHang = maKhoHang, ExcludeId = excludeId }) > 0;
            }
        }
    }
}
