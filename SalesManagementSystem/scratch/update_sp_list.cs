using System;
using System.IO;
using SalesManagementSystem.Data;
using Dapper;

namespace UpdateGetDonHangList
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
                    string sqlPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\create_sp_BAN_ChungTuBanHang_GetDonHangList.sql";
                    string sql = File.ReadAllText(sqlPath);
                    conn.Execute(sql);
                    Console.WriteLine("SUCCESS: sp_BAN_ChungTuBanHang_GetDonHangList updated!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
            }
        }
    }
}
