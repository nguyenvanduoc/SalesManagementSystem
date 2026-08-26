using System;
using System.IO;
using System.Data.SqlClient;
using SalesManagementSystem.Data;

namespace ApplySP
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
                    string sqlPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\sp_Dashboard_GetData.sql";
                    string sql = File.ReadAllText(sqlPath);

                    var parts = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (!string.IsNullOrWhiteSpace(part))
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = part;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    Console.WriteLine("sp_Dashboard_GetData executed and updated successfully in Database!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX: " + ex);
            }
        }
    }
}
