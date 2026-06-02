using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using System;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class NhanVienRepository : INhanVienRepository
    {
        private readonly DbConnectionFactory _db;

        public NhanVienRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<NhanVien> GetAll()
        {
            const string sql = "SELECT * FROM NS_NhanVien ORDER BY MaNhanVien";
            using (var conn = _db.CreateConnection())
                return conn.Query<NhanVien>(sql);
        }

        public IEnumerable<NhanVien> GetPaged(int page, int pageSize, string keyword, bool? gender, out int totalRecords)
        {
            var conditions = new List<string> { "1 = 1" };
            var parameters = new DynamicParameters();
            
            if (!string.IsNullOrEmpty(keyword))
            {
                conditions.Add("(MaNhanVien LIKE @Keyword OR TenNhanVien LIKE @Keyword OR SoDienThoai LIKE @Keyword)");
                parameters.Add("Keyword", "%" + keyword.Trim() + "%");
            }
            if (gender.HasValue)
            {
                conditions.Add("GioiTinh = @Gender");
                parameters.Add("Gender", gender.Value);
            }
            
            var whereClause = "WHERE " + string.Join(" AND ", conditions);
            
            string countSql = $"SELECT COUNT(1) FROM NS_NhanVien {whereClause}";
            string sql = $@"
                SELECT * FROM NS_NhanVien 
                {whereClause}
                ORDER BY MaNhanVien
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";
            
            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);
            
            using (var conn = _db.CreateConnection())
            {
                totalRecords = conn.ExecuteScalar<int>(countSql, parameters);
                return conn.Query<NhanVien>(sql, parameters);
            }
        }

        public NhanVien GetById(int id)
        {
            const string sql = "SELECT * FROM NS_NhanVien WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<NhanVien>(sql, new { ID = id });
        }

        public bool IsDuplicateCode(string code, int id = 0)
        {
            const string sql = "SELECT COUNT(1) FROM NS_NhanVien WHERE MaNhanVien = @MaNhanVien AND ID != @ID";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, new { MaNhanVien = code, ID = id }) > 0;
        }

        public int Insert(NhanVien employee)
        {
            employee.NgayTao = DateTime.Now;
            const string sql = @"
                INSERT INTO NS_NhanVien (MaNhanVien, TenNhanVien, HoDem, NgaySinh, GioiTinh, SoCMND, NgayCap, DiaChi, Email, SoDienThoai, SoDienThoai2, NgayTao, NguoiTao, NgayCapNhat, NguoiCapNhat)
                VALUES (@MaNhanVien, @TenNhanVien, @HoDem, @NgaySinh, @GioiTinh, @SoCMND, @NgayCap, @DiaChi, @Email, @SoDienThoai, @SoDienThoai2, @NgayTao, @NguoiTao, @NgayCapNhat, @NguoiCapNhat);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, employee);
        }

        public void Update(NhanVien employee)
        {
            employee.NgayCapNhat = DateTime.Now;
            const string sql = @"
                UPDATE NS_NhanVien
                SET MaNhanVien = @MaNhanVien, TenNhanVien = @TenNhanVien, HoDem = @HoDem,
                    NgaySinh = @NgaySinh, GioiTinh = @GioiTinh, SoCMND = @SoCMND,
                    NgayCap = @NgayCap, DiaChi = @DiaChi, Email = @Email,
                    SoDienThoai = @SoDienThoai, SoDienThoai2 = @SoDienThoai2,
                    NgayCapNhat = @NgayCapNhat, NguoiCapNhat = @NguoiCapNhat
                WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, employee);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM NS_NhanVien WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, new { ID = id });
        }
    }
}
