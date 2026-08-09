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

                // 1. Find customer KHTongthauvu
                var kh = conn.QueryFirstOrDefault("SELECT ID, MaKhachHang, TenKhachHang FROM NS_KhachHang WHERE MaKhachHang LIKE '%Tongthauvu%' OR TenKhachHang LIKE N'%Vũ Tổng Thầu%'");
                if (kh != null)
                {
                    Console.WriteLine($"Found Customer: ID={kh.ID}, Ma={kh.MaKhachHang}, Ten={kh.TenKhachHang}");
                    int idKh = kh.ID;

                    // Check BAN_PhieuThuKhachHang
                    var listPT = conn.Query(@"
                        SELECT ID, SoPhieuThu, NgayThu, IDKhachHang, SoTienThu, TrangThai, IsDeleted 
                        FROM BAN_PhieuThuKhachHang 
                        WHERE IDKhachHang = @IDKh", new { IDKh = idKh });

                    Console.WriteLine("\n--- BAN_PhieuThuKhachHang ---");
                    foreach (var pt in listPT)
                    {
                        Console.WriteLine($"ID={pt.ID}, SoPhieuThu={pt.SoPhieuThu}, NgayThu={pt.NgayThu:yyyy-MM-dd}, SoTien={pt.SoTienThu:#,##0}, TrangThai={pt.TrangThai}, IsDeleted={pt.IsDeleted}");
                    }

                    // Check BAN_ChungTuBanHang
                    var listBH = conn.Query(@"
                        SELECT ID, SoChungTu, NgayChungTu, IDKhachHang, TongCong, TrangThai, IsDeleted 
                        FROM BAN_ChungTuBanHang 
                        WHERE IDKhachHang = @IDKh", new { IDKh = idKh });

                    Console.WriteLine("\n--- BAN_ChungTuBanHang ---");
                    foreach (var bh in listBH)
                    {
                        Console.WriteLine($"ID={bh.ID}, SoChungTu={bh.SoChungTu}, NgayChungTu={bh.NgayChungTu:yyyy-MM-dd}, TongCong={bh.TongCong:#,##0}, TrangThai={bh.TrangThai}, IsDeleted={bh.IsDeleted}");
                    }
                }
                else
                {
                    Console.WriteLine("Customer KHTongthauvu not found!");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
