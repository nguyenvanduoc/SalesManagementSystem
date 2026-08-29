using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace TestCheck
{
    class Program
    {
        static void Main()
        {
            var db = new DbConnectionFactory();
            using (var conn = db.CreateConnection())
            {
                conn.Open();

                var sps = new[] { "sp_KHO_NhapXuatTon_GetList", "sp_KHO_TonKho_GetList" };
                foreach (var sp in sps)
                {
                    var def = conn.ExecuteScalar<string>("SELECT OBJECT_DEFINITION(OBJECT_ID(@sp))", new { sp });
                    Console.WriteLine($"==================== {sp} ====================");
                    Console.WriteLine(def);
                }
            }
        }
    }
}
