using System.Web.Mvc;
using SalesManagementSystem.Data;

namespace SalesManagementSystem.Controllers
{
    [AllowAnonymous]
    public class SetupController : Controller
    {
        private readonly DbConnectionFactory _db;
        
        public SetupController(DbConnectionFactory db)
        {
            _db = db;
        }

        public ActionResult Run()
        {
            var sqlPath = Server.MapPath("~/App_Data/create_dieu_chinh_phieu_nhap.sql");
            var sqlContent = System.IO.File.ReadAllText(sqlPath);
            var commands = sqlContent.Split(new[] { "GO\r\n", "GO\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                foreach (var cmdText in commands)
                {
                    if (string.IsNullOrWhiteSpace(cmdText)) continue;
                    using (var cmd = new System.Data.SqlClient.SqlCommand(cmdText, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return Content("OK");
        }
    }
}
