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

        public int LogLogin(AclLoginSession session, bool forceNew = false)
        {
            using (var conn = _db.CreateConnection())
            {
                if (!forceNew)
                {
                    string checkSql = @"
                        SELECT TOP 1 ID 
                        FROM ACL_LoginSession 
                        WHERE IDLogin = @IDLogin AND IsDangHoatDong = 1 
                        ORDER BY ID DESC";
                    int? existingId = conn.QueryFirstOrDefault<int?>(checkSql, new { IDLogin = session.IDLogin });
                    if (existingId.HasValue && existingId.Value > 0)
                    {
                        string updateSql = "UPDATE ACL_LoginSession SET LastActiveTime = GETDATE() WHERE ID = @ID";
                        conn.Execute(updateSql, new { ID = existingId.Value });

                        if (System.Web.HttpRuntime.Cache != null)
                        {
                            string cacheKey = "SessionActive_" + existingId.Value;
                            System.Web.HttpRuntime.Cache.Insert(cacheKey, true, null, DateTime.Now.AddSeconds(10), System.Web.Caching.Cache.NoSlidingExpiration);
                        }
                        return existingId.Value;
                    }
                }

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
                int newId = conn.ExecuteScalar<int>(insertSql, session);

                if (System.Web.HttpRuntime.Cache != null)
                {
                    string cacheKey = "SessionActive_" + newId;
                    System.Web.HttpRuntime.Cache.Insert(cacheKey, true, null, DateTime.Now.AddSeconds(10), System.Web.Caching.Cache.NoSlidingExpiration);
                }

                return newId;
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

            // Invalidate cache immediately so kicked user is logged out on next request
            string cacheKey = "SessionActive_" + id;
            if (System.Web.HttpRuntime.Cache != null)
            {
                System.Web.HttpRuntime.Cache.Insert(cacheKey, false, null, DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);
            }
        }

        public bool IsSessionActive(int id)
        {
            string cacheKey = "SessionActive_" + id;
            if (System.Web.HttpRuntime.Cache != null)
            {
                object cached = System.Web.HttpRuntime.Cache[cacheKey];
                if (cached is bool isActiveCached)
                {
                    return isActiveCached;
                }
            }

            using (var conn = _db.CreateConnection())
            {
                string sql = "SELECT IsDangHoatDong FROM ACL_LoginSession WHERE ID = @ID";
                bool isActive = conn.ExecuteScalar<bool>(sql, new { ID = id });

                if (System.Web.HttpRuntime.Cache != null)
                {
                    System.Web.HttpRuntime.Cache.Insert(cacheKey, isActive, null, DateTime.Now.AddSeconds(5), System.Web.Caching.Cache.NoSlidingExpiration);
                }

                return isActive;
            }
        }

        public void UpdateLastActive(int sessionId)
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = "UPDATE ACL_LoginSession SET LastActiveTime = GETDATE() WHERE ID = @ID AND IsDangHoatDong = 1";
                conn.Execute(sql, new { ID = sessionId });
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
                ORDER BY ls.IsDangHoatDong DESC, ls.ThoiGianLogin DESC
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
