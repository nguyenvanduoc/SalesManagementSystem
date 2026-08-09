using System;
using System.Collections.Generic;
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

                var rows = conn.Query("SELECT ID, SoPhieuThu, NgayThu, IDKhachHang, SoTienThu, TrangThai FROM KT_PhieuThu WHERE IDKhachHang = 8");
                Console.WriteLine("\n--- KT_PhieuThu rows for IDKhachHang = 8 ---");
                decimal sum = 0;
                foreach (var r in rows)
                {
                    Console.WriteLine($"ID={r.ID}, SoPhieuThu={r.SoPhieuThu}, NgayThu={r.NgayThu:yyyy-MM-dd}, IDKhachHang={r.IDKhachHang}, SoTien={r.SoTienThu:#,##0}, TrangThai={r.TrangThai}");
                    sum += (decimal)r.SoTienThu;
                }
                Console.WriteLine($"TOTAL KT_PhieuThu: {sum:#,##0}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
