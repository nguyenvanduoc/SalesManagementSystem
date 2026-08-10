using System;
using System.Configuration;
using System.Data;
using Dapper;

class Program
{
    static void Main()
    {
        ConfigurationManager.AppSettings["ConfigFile"] = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\systemPublic.dat";
        ConfigurationManager.AppSettings["KeyPart1"] = "VanDuoc@123123!";

        var factory = new SalesManagementSystem.Data.DbConnectionFactory();
        using (var conn = factory.CreateConnection())
        {
            conn.Open();

            Console.WriteLine("=== TEST TP3 (ID=3) VOI FILTER TON KHO <> 0 ===");
            var p = new DynamicParameters();
            p.Add("@IDKho", 3);
            p.Add("@TuNgay", new DateTime(2026, 8, 1));
            p.Add("@DenNgay", new DateTime(2026, 8, 10));
            var list = conn.Query("sp_KHO_TonKho_GetList", p, commandType: CommandType.StoredProcedure);
            foreach (var item in list)
            {
                Console.WriteLine(string.Format("Kho: {0}, MaSP: {1}, TenSP: {2}, TonKho: {3}", item.MaKho, item.MaSanPham, item.TenSanPham, item.TonKho));
            }
        }
    }
}
