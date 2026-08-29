using System;
using System.IO;
using SalesManagementSystem.Data;
using Dapper;

namespace ApplySpFix
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
                    string sqlPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\Fix_Sp_DonDieuChinhDonHang_Save_TrietDe.sql";
                    string sql = File.ReadAllText(sqlPath);

                    // Remove USE database and GO statements
                    string[] batches = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var batch in batches)
                    {
                        string trimmed = batch.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("USE ", StringComparison.OrdinalIgnoreCase))
                        {
                            conn.Execute(trimmed);
                        }
                    }

                    Console.WriteLine("SUCCESS: Stored procedure sp_DON_DieuChinhDonHang_Save updated successfully without any errors!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
            }
        }
    }
}
