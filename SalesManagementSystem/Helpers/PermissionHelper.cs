using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;
using System.Web;
using SalesManagementSystem.Helpers;

namespace SalesManagementSystem.Helpers
{
    public static class PermissionHelper
    {
        public static bool HasPermission(string controllerName, LoaiPhanQuyen loaiPhanQuyen)
        {
            var session = HttpContext.Current.Session[CommonConstants.USER_SESSION] as UserLogin;
            if (session == null) return false;

            using (var conn = new DbConnectionFactory().CreateConnection())
            {
                var sql = @"
                    SELECT TOP 1 1 
                    FROM ACL_PhanQuyen pq
                    INNER JOIN ACL_Action a ON pq.IDAction = a.ID
                    WHERE pq.IDLogin = @IDLogin 
                      AND pq.IsChoPhep = 1
                      AND a.TenController = @Controller
                      AND a.LoaiPhanQuyen = @LoaiPhanQuyen";

                var result = conn.QueryFirstOrDefault<int?>(sql, new 
                { 
                    IDLogin = session.UserID, 
                    Controller = controllerName, 
                    LoaiPhanQuyen = (int)loaiPhanQuyen 
                });

                return result.HasValue;
            }
        }
        
        public static bool HasActionPermission(string controllerName, string actionName)
        {
            var session = HttpContext.Current.Session[CommonConstants.USER_SESSION] as UserLogin;
            if (session == null) return false;

            using (var conn = new DbConnectionFactory().CreateConnection())
            {
                var sql = @"
                    SELECT TOP 1 1 
                    FROM ACL_PhanQuyen pq
                    INNER JOIN ACL_Action a ON pq.IDAction = a.ID
                    WHERE pq.IDLogin = @IDLogin 
                      AND pq.IsChoPhep = 1
                      AND a.TenController = @Controller
                      AND a.TenAction = @Action";

                var result = conn.QueryFirstOrDefault<int?>(sql, new 
                { 
                    IDLogin = session.UserID, 
                    Controller = controllerName, 
                    Action = actionName 
                });

                return result.HasValue;
            }
        }
    }
}
