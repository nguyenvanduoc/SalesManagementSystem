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

            Console.WriteLine("=== FIXING DM_TRANGTHAIDONHANG IN DATABASE ===");

            string sql = @"
                IF NOT EXISTS (SELECT 1 FROM DM_TrangThaiDonHang WHERE ID = 5)
                BEGIN
                    INSERT INTO DM_TrangThaiDonHang (ID, TenTrangThai, ThuTuHienThi, KichHoat) VALUES (5, N'Đang giao hàng', 5, 1);
                END

                IF NOT EXISTS (SELECT 1 FROM DM_TrangThaiDonHang WHERE ID = 8)
                BEGIN
                    INSERT INTO DM_TrangThaiDonHang (ID, TenTrangThai, ThuTuHienThi, KichHoat) VALUES (8, N'Thanh toán một phần', 8, 1);
                END
                ELSE
                BEGIN
                    UPDATE DM_TrangThaiDonHang SET TenTrangThai = N'Thanh toán một phần' WHERE ID = 8;
                END
            ";

            conn.Execute(sql);
            Console.WriteLine("Update executed successfully!");

            Console.WriteLine("\n=== VERIFYING DM_TRANGTHAIDONHANG ===");
            var listStatus = conn.Query("SELECT ID, TenTrangThai FROM DM_TrangThaiDonHang ORDER BY ID");
            foreach (var st in listStatus)
            {
                Console.WriteLine("ID = {0}: {1}", st.ID, st.TenTrangThai);
            }
        }
    }
}
