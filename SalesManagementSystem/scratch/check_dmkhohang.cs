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

                var khos = conn.Query("SELECT ID, MaKhoHang, TenKhoHang FROM DM_KhoHang ORDER BY ID");
                Console.WriteLine("--- DM_KhoHang ---");
                foreach (var k in khos)
                {
                    Console.WriteLine($"ID={k.ID}, MaKho={k.MaKhoHang}, TenKho={k.TenKhoHang}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
