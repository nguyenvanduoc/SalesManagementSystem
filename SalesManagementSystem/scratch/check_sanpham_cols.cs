using System;
using System.Configuration;
using System.Data;
using SalesManagementSystem.Data;
using Dapper;

class Program
{
    static void Main()
    {
        try
        {
            ConfigurationManager.AppSettings["ConfigFile"] = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\systemPublic.dat";
            ConfigurationManager.AppSettings["KeyPart1"] = "VanDuoc@123123!";
            AppDomain.CurrentDomain.SetData("DataDirectory", @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data");

            var db = new DbConnectionFactory();
            using (var conn = db.CreateConnection())
            {
                conn.Open();

                var cols = conn.Query("SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DM_SanPham'");
                Console.WriteLine("--- DM_SanPham Columns ---");
                foreach (var c in cols)
                {
                    Console.WriteLine($"{c.COLUMN_NAME} ({c.DATA_TYPE})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
