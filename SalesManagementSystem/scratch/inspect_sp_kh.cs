using System;
using System.Data;
using Dapper;
using SalesManagementSystem.Data;

namespace InspectSpKh
{
    class Program
    {
        static void Main()
        {
            try
            {
                var db = new DbConnectionFactory();
                using (var conn = db.CreateConnection())
                {
                    conn.Open();

                    Console.WriteLine("=== SP: sp_CongNoKhachHang_GetList ===");
                    string text1 = conn.QueryFirstOrDefault<string>(
                        "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_CongNoKhachHang_GetList'))"
                    );
                    Console.WriteLine(text1);

                    Console.WriteLine("\n=== SP: sp_CongNoKhachHang_GetDashboard ===");
                    string text2 = conn.QueryFirstOrDefault<string>(
                        "SELECT OBJECT_DEFINITION(OBJECT_ID('sp_CongNoKhachHang_GetDashboard'))"
                    );
                    Console.WriteLine(text2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX: " + ex);
            }
        }
    }
}
