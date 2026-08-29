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

                var sps = new[] { "sp_KHO_BaoCaoTonKho", "sp_KHO_TonKho", "sp_KHO_TheKho", "sp_KHO_GiaoDichKho_GetList", "sp_KHO_PhieuXuat_GetList" };
                foreach (var sp in sps)
                {
                    try
                    {
                        var def = conn.ExecuteScalar<string>("SELECT OBJECT_DEFINITION(OBJECT_ID(@sp))", new { sp });
                        Console.WriteLine($"==================== {sp} ====================");
                        Console.WriteLine(def);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting {sp}: {ex.Message}");
                    }
                }
            }
        }
    }
}
