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
                conditions.Add("(L.TenDangNhap LIKE @Keyword OR N.Ten LIKE @Keyword)");
                parameters.Add("Keyword", "%" + keyword.Trim() + "%");
            }
            
            var whereClause = "WHERE " + string.Join(" AND ", conditions);
            
            string countSql = $@"
                SELECT COUNT(1) 
                FROM ACL_Login L
                LEFT JOIN NS_NhanSu N ON L.IDNhanSu = N.ID
                {whereClause}";
                
            string sql = $@"
                SELECT L.*, N.HoDem, N.Ten as Ten 
                FROM ACL_Login L
                LEFT JOIN NS_NhanSu N ON L.IDNhanSu = N.ID
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
                INSERT INTO ACL_Login (IDNhanSu, TenDangNhap, MatKhau, HoDem, Ten, IsActive, IDThamChieu, NgayTao, NguoiTao)
                VALUES (@IDNhanSu, @TenDangNhap, @MatKhau, @HoDem, @Ten, @IsActive, @IDThamChieu, @NgayTao, @NguoiTao);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, login);
        }

        public void Update(AclLogin login)
        {
            login.NgayCapNhat = DateTime.Now;
            const string sql = @"
                UPDATE ACL_Login
                SET IDNhanSu = @IDNhanSu, TenDangNhap = @TenDangNhap, MatKhau = @MatKhau, 
                    HoDem = @HoDem, Ten = @Ten, IsActive = @IsActive, IDThamChieu = @IDThamChieu,
                    NgayCapNhat = @NgayCapNhat, NguoiCapNhat = @NguoiCapNhat,
                    NgayXoa = @NgayXoa, NguoiXoa = @NguoiXoa
                WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, login);
        }

        public void Delete(int id, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                // Delete permissions for both parent and its children
                const string sqlDeletePhanQuyen = @"
                    DELETE FROM ACL_PhanQuyen 
                    WHERE IDLogin = @ID OR IDLogin IN (SELECT ID FROM ACL_Login WHERE IDThamChieu = @ID)";
                conn.Execute(sqlDeletePhanQuyen, new { ID = id });

                // Soft delete and clear IDThamChieu for all its children
                const string sqlChildren = "UPDATE ACL_Login SET NgayXoa = @NgayXoa, NguoiXoa = @NguoiXoa, IDThamChieu = NULL WHERE IDThamChieu = @ID AND NgayXoa IS NULL";
                conn.Execute(sqlChildren, new { ID = id, NgayXoa = DateTime.Now, NguoiXoa = userId });

                // Soft delete and clear IDThamChieu for the target account
                const string sqlParent = "UPDATE ACL_Login SET NgayXoa = @NgayXoa, NguoiXoa = @NguoiXoa, IDThamChieu = NULL WHERE ID = @ID";
                conn.Execute(sqlParent, new { ID = id, NgayXoa = DateTime.Now, NguoiXoa = userId });
            }
        }

        public void TransferManager(int newManagerId, int updateBy)
        {
            using (var conn = _db.CreateConnection())
            {
                // Find the old manager ID
                var oldManagerId = conn.QueryFirstOrDefault<int?>("SELECT IDThamChieu FROM ACL_Login WHERE ID = @ID", new { ID = newManagerId });
                
                if (oldManagerId != null)
                {
                    // Update all users who have the same manager, but skip the new manager
                    const string sqlUpdateGroup = @"
                        UPDATE ACL_Login 
                        SET IDThamChieu = @NewManagerId, NgayCapNhat = @NgayCapNhat, NguoiCapNhat = @NguoiCapNhat 
                        WHERE IDThamChieu = @OldManagerId AND ID != @NewManagerId AND NgayXoa IS NULL";
                    conn.Execute(sqlUpdateGroup, new { NewManagerId = newManagerId, OldManagerId = oldManagerId, NgayCapNhat = DateTime.Now, NguoiCapNhat = updateBy });

                    // We also need to set the old manager to become a child of the new manager?
                    // The user said: "nhân sự được chuyển quyền sẽ thay thế cấp trên hiện tại". 
                    // Let's assume the old manager becomes a child of the new manager.
                    const string sqlUpdateOldManager = @"
                        UPDATE ACL_Login 
                        SET IDThamChieu = @NewManagerId, NgayCapNhat = @NgayCapNhat, NguoiCapNhat = @NguoiCapNhat 
                        WHERE ID = @OldManagerId AND NgayXoa IS NULL";
                    conn.Execute(sqlUpdateOldManager, new { NewManagerId = newManagerId, OldManagerId = oldManagerId, NgayCapNhat = DateTime.Now, NguoiCapNhat = updateBy });
                }

                // Make the new manager a top-level manager
                const string sqlMakeManager = "UPDATE ACL_Login SET IDThamChieu = NULL, NgayCapNhat = @NgayCapNhat, NguoiCapNhat = @NguoiCapNhat WHERE ID = @ID";
                conn.Execute(sqlMakeManager, new { ID = newManagerId, NgayCapNhat = DateTime.Now, NguoiCapNhat = updateBy });
            }
        }

        public IEnumerable<NhanSu> GetEmployeesWithoutAccount()
        {
            const string sql = @"
                SELECT N.* 
                FROM NS_NhanSu N
                LEFT JOIN ACL_Login L ON N.ID = L.IDNhanSu AND L.NgayXoa IS NULL
                WHERE L.ID IS NULL
                ORDER BY N.Ten";
            using (var conn = _db.CreateConnection())
                return conn.Query<NhanSu>(sql);
        }

        public NhanSu GetEmployeeById(int id)
        {
            const string sql = "SELECT * FROM NS_NhanSu WHERE ID = @ID";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<NhanSu>(sql, new { ID = id });
        }

        public AclLogin GetByEmployeeId(int empId)
        {
            const string sql = "SELECT * FROM ACL_Login WHERE IDNhanSu = @EmpId";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<AclLogin>(sql, new { EmpId = empId });
        }

        public IEnumerable<AclLoginViewModel> GetManagers()
        {
            const string sql = @"
                SELECT L.*, N.HoDem, N.Ten as Ten 
                FROM ACL_Login L
                LEFT JOIN NS_NhanSu N ON L.IDNhanSu = N.ID
                WHERE (L.IDThamChieu IS NULL OR L.IDThamChieu = 0) AND L.IsActive = 1 AND L.NgayXoa IS NULL
                ORDER BY N.Ten";
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
