using System;
using System.IO;
using SalesManagementSystem.Data;
using Dapper;

namespace TestAlterSp
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
                    string sqlPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\alter_dieu_chinh_don_hang_phibocxep.sql";
                    string sql = File.ReadAllText(sqlPath);

                    string[] batches = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var batch in batches)
                    {
                        string trimmed = batch.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("USE ", StringComparison.OrdinalIgnoreCase))
                        {
                            conn.Execute(trimmed);
                        }
                    }

                    Console.WriteLine("SUCCESS: alter_dieu_chinh_don_hang_phibocxep.sql executed cleanly!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
            }
        }
    }
}
