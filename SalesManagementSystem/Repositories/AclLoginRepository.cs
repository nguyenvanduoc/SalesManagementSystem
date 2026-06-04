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
    public class AclLoginRepository : IAclLoginRepository
    {
        private readonly DbConnectionFactory _db;

        public AclLoginRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<AclLoginViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            var conditions = new List<string> { "L.NgayXoa IS NULL" };
            var parameters = new DynamicParameters();
            
            if (!string.IsNullOrEmpty(keyword))
            {
                conditions.Add("(L.TenDangNhap LIKE @Keyword OR N.TenNhanVien LIKE @Keyword)");
                parameters.Add("Keyword", "%" + keyword.Trim() + "%");
            }
            
            var whereClause = "WHERE " + string.Join(" AND ", conditions);
            
            string countSql = $@"
                SELECT COUNT(1) 
                FROM ACL_Login L
                LEFT JOIN NS_NhanVien N ON L.IDNhanVien = N.ID
                {whereClause}";
                
            string sql = $@"
                SELECT L.*, N.HoDem, N.TenNhanVien as Ten 
                FROM ACL_Login L
                LEFT JOIN NS_NhanVien N ON L.IDNhanVien = N.ID
                {whereClause}
                ORDER BY L.ID DESC
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";
            
            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);
            
            using (var conn = _db.CreateConnection())
            {
                totalRecords = conn.ExecuteScalar<int>(countSql, parameters);
                return conn.Query<AclLoginViewModel>(sql, parameters);
            }
        }

        public AclLogin GetById(int id)
        {
            const string sql = "SELECT * FROM ACL_Login WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<AclLogin>(sql, new { ID = id });
        }

        public bool IsDuplicateUsername(string username, int id = 0)
        {
            const string sql = "SELECT COUNT(1) FROM ACL_Login WHERE TenDangNhap = @TenDangNhap AND ID != @ID";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, new { TenDangNhap = username, ID = id }) > 0;
        }

        public int Insert(AclLogin login)
        {
            login.NgayTao = DateTime.Now;
            const string sql = @"
                INSERT INTO ACL_Login (IDNhanVien, TenDangNhap, MatKhau, HoDem, Ten, IsActive, IDThamChieu, NgayTao, NguoiTao)
                VALUES (@IDNhanVien, @TenDangNhap, @MatKhau, @HoDem, @Ten, @IsActive, @IDThamChieu, @NgayTao, @NguoiTao);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, login);
        }

        public void Update(AclLogin login)
        {
            login.NgayCapNhat = DateTime.Now;
            const string sql = @"
                UPDATE ACL_Login
                SET IDNhanVien = @IDNhanVien, TenDangNhap = @TenDangNhap, MatKhau = @MatKhau, 
                    HoDem = @HoDem, Ten = @Ten, IsActive = @IsActive, IDThamChieu = @IDThamChieu,
                    NgayCapNhat = @NgayCapNhat, NguoiCapNhat = @NguoiCapNhat,
                    NgayXoa = @NgayXoa, NguoiXoa = @NguoiXoa
                WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, login);
        }

        public void Delete(int id)
        {
            const string sql = "UPDATE ACL_Login SET NgayXoa = @NgayXoa, NguoiXoa = 0 WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, new { ID = id, NgayXoa = DateTime.Now });
        }

        public IEnumerable<NhanVien> GetEmployeesWithoutAccount()
        {
            const string sql = @"
                SELECT N.* 
                FROM NS_NhanVien N
                LEFT JOIN ACL_Login L ON N.ID = L.IDNhanVien AND L.NgayXoa IS NULL
                WHERE L.ID IS NULL
                ORDER BY N.TenNhanVien";
            using (var conn = _db.CreateConnection())
                return conn.Query<NhanVien>(sql);
        }

        public NhanVien GetEmployeeById(int id)
        {
            const string sql = "SELECT * FROM NS_NhanVien WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<NhanVien>(sql, new { ID = id });
        }

        public AclLogin GetByEmployeeId(int empId)
        {
            const string sql = "SELECT * FROM ACL_Login WHERE IDNhanVien = @EmpId";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<AclLogin>(sql, new { EmpId = empId });
        }

        public IEnumerable<AclLoginViewModel> GetManagers()
        {
            const string sql = @"
                SELECT L.*, N.HoDem, N.TenNhanVien as Ten 
                FROM ACL_Login L
                LEFT JOIN NS_NhanVien N ON L.IDNhanVien = N.ID
                WHERE (L.IDThamChieu IS NULL OR L.IDThamChieu = 0) AND L.IsActive = 1 AND L.NgayXoa IS NULL
                ORDER BY N.TenNhanVien";
            using (var conn = _db.CreateConnection())
                return conn.Query<AclLoginViewModel>(sql);
        }

        public AclLogin Login(string userName, string passWord)
        {
            const string sql = "SELECT * FROM ACL_Login WHERE TenDangNhap = @userName AND MatKhau = @passWord AND IsActive = 1 AND NgayXoa IS NULL";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<AclLogin>(sql, new { userName, passWord });
        }
    }
}
