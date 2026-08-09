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

                string sql = @"
CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_GetDonHangList
    @TuNgay DATE = NULL,
    @DenNgay DATE = NULL,
    @SoDonHang NVARCHAR(50) = NULL,
    @IDKhachHang INT = NULL,
    @TrangThaiChungTu INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        d.ID AS IDDonDatHang,
        d.SoDonHang,
        d.NgayTaoDon,
        k.TenKhachHang,
        d.TongTien,
        c.ID AS IDChungTuBanHang,
        c.SoChungTu,
        c.NgayChungTu,
        c.TrangThai AS TrangThaiChungTu,
        d.PhiBocXep,
        d.SoDienThoaiTaiXe,
        d.HoTenTaiXe
    FROM NS_DonDatHang d
    LEFT JOIN NS_KhachHang k ON d.IDKhachHang = k.ID
    LEFT JOIN BAN_ChungTuBanHang c ON c.IDDonDatHang = d.ID
    WHERE (@TuNgay IS NULL OR d.NgayTaoDon >= @TuNgay)
      AND (@DenNgay IS NULL OR d.NgayTaoDon <= @DenNgay)
      AND (@SoDonHang IS NULL OR d.SoDonHang LIKE '%' + @SoDonHang + '%')
      AND (@IDKhachHang IS NULL OR d.IDKhachHang = @IDKhachHang)
      AND (@TrangThaiChungTu IS NULL OR ISNULL(c.TrangThai, 0) = @TrangThaiChungTu)
    ORDER BY CASE WHEN c.ID IS NULL THEN 0 ELSE 1 END ASC, d.SoDonHang DESC, d.ID DESC;
END
";
                conn.Execute(sql);
                Console.WriteLine("UPDATED sp_BAN_ChungTuBanHang_GetDonHangList SUCCESSFULLY!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
