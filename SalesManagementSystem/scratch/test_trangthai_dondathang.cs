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

            Console.WriteLine("=== SP_BAOCAO_DOICHIEUCONGNOKHACHHANG DEFINITION ===");
            var spDef = conn.ExecuteScalar<string>("SELECT OBJECT_DEFINITION(OBJECT_ID('sp_BaoCao_DoiChieuCongNoKhachHang'))");
            Console.WriteLine(spDef ?? "NOT FOUND");
        }
    }
}
...