using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Helpers
{
    public static class PermissionHelper
    {
        private const string PERM_CACHE_PREFIX = "UserPermissions_";

        public static void ClearUserPermissionsCache(int idLogin)
        {
            if (HttpRuntime.Cache != null)
            {
                HttpRuntime.Cache.Remove(PERM_CACHE_PREFIX + idLogin);
            }
        }

        private static HashSet<string> GetUserPermissions(int idLogin)
        {
            string cacheKey = PERM_CACHE_PREFIX + idLogin;
            if (HttpRuntime.Cache != null)
            {
                if (HttpRuntime.Cache[cacheKey] is HashSet<string> cachedPerms)
                {
                    return cachedPerms;
                }
            }

            var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (var conn = new DbConnectionFactory().CreateConnection())
                {
                    var sql = @"
                        SELECT a.TenController, a.TenAction, a.LoaiPhanQuyen 
                        FROM ACL_PhanQuyen pq
                        INNER JOIN ACL_Action a ON pq.IDAction = a.ID
                        WHERE pq.IDLogin = @IDLogin 
                          AND pq.IsChoPhep = 1";

                    var rows = conn.Query(sql, new { IDLogin = idLogin });
                    foreach (var r in rows)
                    {
                        string controller = r.TenController as string;
                        string action = r.TenAction as string;
                        int? loai = r.LoaiPhanQuyen as int?;

                        if (!string.IsNullOrEmpty(controller))
                        {
                            if (!string.IsNullOrEmpty(action))
                            {
                                permissions.Add($"{controller}:{action}");
                            }
                            if (loai.HasValue)
                            {
                                permissions.Add($"{controller}:L_{loai.Value}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorLog(ex, HttpContext.Current);
            }

            if (HttpRuntime.Cache != null)
            {
                // Cache user permissions in memory for 5 minutes
                HttpRuntime.Cache.Insert(
                    cacheKey,
                    permissions,
                    null,
                    DateTime.Now.AddMinutes(5),
                    System.Web.Caching.Cache.NoSlidingExpiration);
            }

            return permissions;
        }

        public static bool HasPermission(string controllerName, LoaiPhanQuyen loaiPhanQuyen)
        {
            var session = HttpContext.Current?.Session?[CommonConstants.USER_SESSION] as UserLoginViewModel;
            if (session == null) return false;

            var userPerms = GetUserPermissions(session.UserID);
            string key = $"{controllerName}:L_{(int)loaiPhanQuyen}";
            return userPerms.Contains(key);
        }

        public static bool HasActionPermission(string controllerName, string actionName)
        {
            var session = HttpContext.Current?.Session?[CommonConstants.USER_SESSION] as UserLoginViewModel;
            if (session == null) return false;

            var userPerms = GetUserPermissions(session.UserID);
            string key = $"{controllerName}:{actionName}";
            return userPerms.Contains(key);
        }

        public static string GetLoaiPhanQuyenDisplayName(LoaiPhanQuyen loai)
        {
            switch(loai)
            {
                case LoaiPhanQuyen.Xem: return "Xem";
                case LoaiPhanQuyen.Them: return "Thêm";
                case LoaiPhanQuyen.CapNhat: return "Cập nhật";
                case LoaiPhanQuyen.Xoa: return "Xóa";
                case LoaiPhanQuyen.TuyChon: return "Tùy chọn";
                default: return loai.ToString();
            }
        }

        public static string GetLoaiPhanQuyenColorClass(LoaiPhanQuyen loai)
        {
            switch(loai)
            {
                case LoaiPhanQuyen.Xem: return "bg-info text-dark";
                case LoaiPhanQuyen.Them: return "bg-success";
                case LoaiPhanQuyen.CapNhat: return "bg-warning text-dark";
                case LoaiPhanQuyen.Xoa: return "bg-danger";
                case LoaiPhanQuyen.TuyChon: return "bg-secondary";
                default: return "bg-primary";
            }
        }
    }
}
