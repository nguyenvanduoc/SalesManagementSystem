using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using System;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class NhanSuRepository : INhanSuRepository
    {
        private readonly DbConnectionFactory _db;

        public NhanSuRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<NhanSu> GetAll()
        {
            const string sql = "SELECT * FROM NS_NhanSu ORDER BY NgayTao DESC";
            using (var conn = _db.CreateConnection())
                return conn.Query<NhanSu>(sql);
        }

        public IEnumerable<NhanSu> GetAllWithChucVu()
        {
            const string sql = @"
                SELECT n.*, c.TenChucVu 
                FROM NS_NhanSu n
                LEFT JOIN DM_ChucVu c ON n.IDChucVu = c.ID
                ORDER BY c.STT ASC, n.NgayTao DESC";
            using (var conn = _db.CreateConnection())
                return conn.Query<NhanSu>(sql);
        }

        public IEnumerable<NhanSu> GetPaged(int page, int pageSize, string keyword, bool? gender, out int totalRecords)
        {
            var conditions = new List<string> { "1 = 1" };
            var parameters = new DynamicParameters();
            
            if (!string.IsNullOrEmpty(keyword))
            {
                conditions.Add("(MaNhanSu LIKE @Keyword OR Ten LIKE @Keyword OR SoDienThoai LIKE @Keyword)");
                parameters.Add("Keyword", "%" + keyword.Trim() + "%");
            }
            if (gender.HasValue)
            {
                conditions.Add("GioiTinh = @Gender");
                parameters.Add("Gender", gender.Value);
            }
            
            var whereClause = "WHERE " + string.Join(" AND ", conditions);
            
            string countSql = $"SELECT COUNT(1) FROM NS_NhanSu {whereClause}";
            string sql = $@"
                SELECT * FROM NS_NhanSu 
                {whereClause}
                ORDER BY NgayTao DESC
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";
            
            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);
            
            using (var conn = _db.CreateConnection())
            {
                totalRecords = conn.ExecuteScalar<int>(countSql, parameters);
                return conn.Query<NhanSu>(sql, parameters);
            }
        }

        public NhanSu GetById(int id)
        {
            const string sql = "SELECT * FROM NS_NhanSu WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<NhanSu>(sql, new { ID = id });
        }

        public bool IsDuplicateCode(string code, int id = 0)
        {
            const string sql = "SELECT COUNT(1) FROM NS_NhanSu WHERE MaNhanSu = @MaNhanSu AND ID != @ID";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, new { MaNhanSu = code, ID = id }) > 0;
        }

        public int Insert(NhanSu employee)
        {
            employee.NgayTao = DateTime.Now;
            const string sql = @"
                INSERT INTO NS_NhanSu (MaNhanSu, Ten, HoDem, NgaySinh, GioiTinh, SoCMND, NgayCap, DiaChi, Email, SoDienThoai, SoDienThoai2, NgayTao, NguoiTao, NgayCapNhat, NguoiCapNhat, IDChucVu, IDPhongBan, LuongCoBan, HinhAnh)
                VALUES (@MaNhanSu, @Ten, @HoDem, @NgaySinh, @GioiTinh, @SoCMND, @NgayCap, @DiaChi, @Email, @SoDienThoai, @SoDienThoai2, @NgayTao, @NguoiTao, @NgayCapNhat, @NguoiCapNhat, @IDChucVu, @IDPhongBan, @LuongCoBan, @HinhAnh);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, employee);
        }

        public void Update(NhanSu employee)
        {
            employee.NgayCapNhat = DateTime.Now;
            const string sql = @"
                UPDATE NS_NhanSu
                SET MaNhanSu = @MaNhanSu, Ten = @Ten, HoDem = @HoDem,
                    NgaySinh = @NgaySinh, GioiTinh = @GioiTinh, SoCMND = @SoCMND,
                    NgayCap = @NgayCap, DiaChi = @DiaChi, Email = @Email,
                    SoDienThoai = @SoDienThoai, SoDienThoai2 = @SoDienThoai2,
                    NgayCapNhat = @NgayCapNhat, NguoiCapNhat = @NguoiCapNhat,
                    IDChucVu = @IDChucVu, IDPhongBan = @IDPhongBan, LuongCoBan = @LuongCoBan,
                    HinhAnh = @HinhAnh
                WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, employee);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM NS_NhanSu WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, new { ID = id });
        }
    }
}
