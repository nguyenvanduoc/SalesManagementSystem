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
    public class AclLoginSessionRepository : IAclLoginSessionRepository
    {
        private readonly DbConnectionFactory _db;

        public AclLoginSessionRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public int LogLogin(AclLoginSession session)
        {
            using (var conn = _db.CreateConnection())
            {
                // 1. Đóng các phiên đang mở (nếu có) của tài khoản này
                string closeOldSessionsSql = @"
                    UPDATE ACL_LoginSession 
                    SET ThoiGianLogout = GETDATE(), IsDangHoatDong = 0 
                    WHERE IDLogin = @IDLogin AND IsDangHoatDong = 1";
                conn.Execute(closeOldSessionsSql, new { IDLogin = session.IDLogin });

                // 2. Tạo phiên mới
                string insertSql = @"
                    INSERT INTO ACL_LoginSession (IDLogin, HoTen, ThoiGianLogin, HostName, HostAddress, TrinhDuyet, IP, IsDangHoatDong)
                    VALUES (@IDLogin, @HoTen, @ThoiGianLogin, @HostName, @HostAddress, @TrinhDuyet, @IP, 1);
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";
                
                session.ThoiGianLogin = DateTime.Now;
                return conn.ExecuteScalar<int>(insertSql, session);
            }
        }

        public void LogLogout(int loginId)
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = @"
                    UPDATE ACL_LoginSession 
                    SET ThoiGianLogout = GETDATE(), IsDangHoatDong = 0 
                    WHERE IDLogin = @IDLogin AND IsDangHoatDong = 1";
                conn.Execute(sql, new { IDLogin = loginId });
            }
        }

        public void KickSession(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = @"
                    UPDATE ACL_LoginSession 
                    SET ThoiGianLogout = GETDATE(), IsDangHoatDong = 0 
                    WHERE ID = @ID AND IsDangHoatDong = 1";
                conn.Execute(sql, new { ID = id });
            }
        }

        public bool IsSessionActive(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = "SELECT IsDangHoatDong FROM ACL_LoginSession WHERE ID = @ID";
                return conn.ExecuteScalar<bool>(sql, new { ID = id });
            }
        }

        public IEnumerable<AclLoginSessionViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            var conditions = new List<string> { "1 = 1" };
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(keyword))
            {
                conditions.Add("(ls.HoTen LIKE @Keyword OR al.TenDangNhap LIKE @Keyword OR ls.IP LIKE @Keyword)");
                parameters.Add("Keyword", "%" + keyword.Trim() + "%");
            }

            var whereClause = "WHERE " + string.Join(" AND ", conditions);

            string countSql = $@"
                SELECT COUNT(1) 
                FROM ACL_LoginSession ls 
                LEFT JOIN ACL_Login al ON ls.IDLogin = al.ID 
                {whereClause}";
                
            string sql = $@"
                SELECT ls.*, al.TenDangNhap 
                FROM ACL_LoginSession ls 
                LEFT JOIN ACL_Login al ON ls.IDLogin = al.ID 
                {whereClause}
                ORDER BY ls.ThoiGianLogin DESC
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            using (var conn = _db.CreateConnection())
            {
                totalRecords = conn.ExecuteScalar<int>(countSql, parameters);
                return conn.Query<AclLoginSessionViewModel>(sql, parameters);
            }
        }
    }
}
