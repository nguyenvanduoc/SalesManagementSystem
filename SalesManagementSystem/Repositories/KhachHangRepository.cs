using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class KhachHangRepository : IKhachHangRepository
    {
        private readonly DbConnectionFactory _db;

        public KhachHangRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<KhachHangViewModel> GetAll()
        {
            var sql = @"
                SELECT k.*, 
                       nh.TenNhomKhachHang,
                       nv.HoTen as TenNhanVien,
                       t.TenTinhThanh
                FROM NS_KhachHang k
                LEFT JOIN NS_NhomKhachHang nh ON k.IDNhomKhachHang = nh.ID
                LEFT JOIN NS_NhanVien nv ON k.IDNhanVien = nv.ID
                LEFT JOIN DM_TinhThanh t ON k.IDTinhThanh = t.ID
                ORDER BY k.TenKhachHang";

            using (var conn = _db.CreateConnection())
            {
                return conn.Query<KhachHangViewModel>(sql);
            }
        }

        public IEnumerable<KhachHangViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Keyword", string.IsNullOrEmpty(keyword) ? "" : keyword.ToLower());
                parameters.Add("@Offset", (page - 1) * pageSize);
                parameters.Add("@PageSize", pageSize);

                var countSql = @"
                    SELECT COUNT(*) 
                    FROM NS_KhachHang
                    WHERE (@Keyword = '' 
                           OR LOWER(MaKhachHang) LIKE '%' + @Keyword + '%'
                           OR LOWER(TenKhachHang) LIKE '%' + @Keyword + '%'
                           OR LOWER(SoDienThoai) LIKE '%' + @Keyword + '%'
                           OR LOWER(TenKhuVuc) LIKE '%' + @Keyword + '%')";

                totalRecords = conn.ExecuteScalar<int>(countSql, parameters);

                var sql = @"
                    SELECT k.*, 
                           nh.TenNhomKhachHang,
                           nv.HoTen as TenNhanVien,
                           t.TenTinhThanh
                    FROM NS_KhachHang k
                    LEFT JOIN NS_NhomKhachHang nh ON k.IDNhomKhachHang = nh.ID
                    LEFT JOIN NS_NhanVien nv ON k.IDNhanVien = nv.ID
                    LEFT JOIN DM_TinhThanh t ON k.IDTinhThanh = t.ID
                    WHERE (@Keyword = '' 
                           OR LOWER(k.MaKhachHang) LIKE '%' + @Keyword + '%'
                           OR LOWER(k.TenKhachHang) LIKE '%' + @Keyword + '%'
                           OR LOWER(k.SoDienThoai) LIKE '%' + @Keyword + '%'
                           OR LOWER(k.TenKhuVuc) LIKE '%' + @Keyword + '%')
                    ORDER BY k.NgayTao DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                return conn.Query<KhachHangViewModel>(sql, parameters);
            }
        }

        public NS_KhachHang GetById(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<NS_KhachHang>("SELECT * FROM NS_KhachHang WHERE ID = @ID", new { ID = id });
            }
        }

        public bool IsDuplicateCode(string maKhachHang, int currentId = 0)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = "SELECT COUNT(1) FROM NS_KhachHang WHERE MaKhachHang = @MaKhachHang AND ID != @CurrentId";
                return conn.ExecuteScalar<int>(sql, new { MaKhachHang = maKhachHang, CurrentId = currentId }) > 0;
            }
        }

        public int Insert(NS_KhachHang entity)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = @"
                    INSERT INTO NS_KhachHang (MaSoThue, TenKhachHang, MaKhachHang, IDNhomKhachHang, DiaChi, SoDienThoai, Email, IDNhanVien, IDTinhThanh, TenKhuVuc, NguoiTao, NgayTao)
                    VALUES (@MaSoThue, @TenKhachHang, @MaKhachHang, @IDNhomKhachHang, @DiaChi, @SoDienThoai, @Email, @IDNhanVien, @IDTinhThanh, @TenKhuVuc, @NguoiTao, @NgayTao);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                entity.NgayTao = DateTime.Now;
                return conn.ExecuteScalar<int>(sql, entity);
            }
        }

        public int Update(NS_KhachHang entity)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = @"
                    UPDATE NS_KhachHang 
                    SET MaSoThue = @MaSoThue, 
                        TenKhachHang = @TenKhachHang, 
                        MaKhachHang = @MaKhachHang,
                        IDNhomKhachHang = @IDNhomKhachHang,
                        DiaChi = @DiaChi,
                        SoDienThoai = @SoDienThoai,
                        Email = @Email,
                        IDNhanVien = @IDNhanVien,
                        IDTinhThanh = @IDTinhThanh,
                        TenKhuVuc = @TenKhuVuc,
                        NgayCapNhat = @NgayCapNhat, 
                        NguoiCapNhat = @NguoiCapNhat
                    WHERE ID = @ID";

                entity.NgayCapNhat = DateTime.Now;
                return conn.Execute(sql, entity);
            }
        }

        public int Delete(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Execute("DELETE FROM NS_KhachHang WHERE ID = @ID", new { ID = id });
            }
        }
    }
}
