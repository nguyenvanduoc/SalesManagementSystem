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
CREATE OR ALTER PROCEDURE sp_KHO_TonKho_GetList
    @IDKho INT = NULL,
    @IDSanPham INT = NULL,
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @ChiConTon BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH CTE_GiaoDich AS (
        SELECT 
            gd.IDKho,
            gd.IDSanPham,
            SUM(CASE WHEN @TuNgay IS NOT NULL AND gd.NgayChungTu < @TuNgay THEN gd.SoLuongNhap ELSE 0 END) - 
            SUM(CASE WHEN @TuNgay IS NOT NULL AND gd.NgayChungTu < @TuNgay THEN gd.SoLuongXuat ELSE 0 END) AS TonDauKy,
            SUM(CASE WHEN @TuNgay IS NULL OR gd.NgayChungTu >= @TuNgay THEN gd.SoLuongNhap ELSE 0 END) AS TongNhap,
            SUM(CASE WHEN @TuNgay IS NULL OR gd.NgayChungTu >= @TuNgay THEN gd.SoLuongXuat ELSE 0 END) AS TongXuat,
            SUM(gd.SoLuongNhap) - SUM(gd.SoLuongXuat) AS TonKho,
            MAX(CASE WHEN gd.SoLuongNhap > 0 AND (@TuNgay IS NULL OR gd.NgayChungTu >= @TuNgay) THEN gd.NgayChungTu ELSE NULL END) AS NgayNhapCuoi,
            MAX(CASE WHEN gd.SoLuongXuat > 0 AND (@TuNgay IS NULL OR gd.NgayChungTu >= @TuNgay) THEN gd.NgayChungTu ELSE NULL END) AS NgayXuatCuoi,
            (SELECT TOP 1 DonGia FROM KHO_GiaoDichKho WHERE IDSanPham = gd.IDSanPham AND IDKho = gd.IDKho AND SoLuongNhap > 0 AND IsHuy = 0 ORDER BY NgayChungTu DESC, ID DESC) AS DonGiaCuoi
        FROM KHO_GiaoDichKho gd
        WHERE gd.IsHuy = 0
          AND (@IDKho IS NULL OR gd.IDKho = @IDKho)
          AND (@IDSanPham IS NULL OR gd.IDSanPham = @IDSanPham)
          AND (@DenNgay IS NULL OR gd.NgayChungTu <= @DenNgay)
        GROUP BY gd.IDKho, gd.IDSanPham
    )
    SELECT 
        ISNULL(gd.IDKho, @IDKho) AS IDKho,
        k.MaKhoHang AS MaKho,
        k.TenKhoHang AS TenKho,
        sp.ID AS IDSanPham,
        sp.MaSanPham,
        sp.TenSanPham,
        sp.DVT,
        ISNULL(gd.TonDauKy, 0) AS TonDauKy,
        ISNULL(gd.TongNhap, 0) AS TongNhap,
        ISNULL(gd.TongXuat, 0) AS TongXuat,
        ISNULL(gd.TonKho, 0) AS TonKho,
        ISNULL(gd.DonGiaCuoi, 0) AS DonGiaTon,
        ISNULL(gd.TonKho, 0) * ISNULL(gd.DonGiaCuoi, 0) AS GiaTriTon,
        gd.NgayNhapCuoi,
        gd.NgayXuatCuoi,
        0 AS MucTonToiThieu
    FROM DM_SanPham sp
    LEFT JOIN CTE_GiaoDich gd ON sp.ID = gd.IDSanPham
    LEFT JOIN DM_KhoHang k ON ISNULL(gd.IDKho, @IDKho) = k.ID
    WHERE (@IDSanPham IS NULL OR sp.ID = @IDSanPham)
      AND (@ChiConTon = 0 OR ISNULL(gd.TonKho, 0) > 0)
      AND UPPER(ISNULL(sp.MaSanPham, '')) NOT LIKE '%NODAU%'
      AND UPPER(ISNULL(sp.TenSanPham, '')) NOT LIKE N'%NỢ ĐẦU KỲ%'
    ORDER BY k.TenKhoHang, sp.TenSanPham;
END
";
                conn.Execute(sql);
                Console.WriteLine("UPDATED sp_KHO_TonKho_GetList TO HIDE NODAU PRODUCTS SUCCESSFULLY!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
