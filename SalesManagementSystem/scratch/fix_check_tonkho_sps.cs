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

                string sqlAll = @"
CREATE OR ALTER PROCEDURE sp_KHO_TonKho_CheckAllKho
    @ListSanPham NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        JSON_VALUE(value, '$.IDSanPham') AS IDSanPham,
        CAST(JSON_VALUE(value, '$.SoLuongCanXuat') AS DECIMAL(18,2)) AS SoLuongCanXuat
    INTO #TempSanPham
    FROM OPENJSON(@ListSanPham);

    SELECT ID AS IDKho, TenKhoHang INTO #TempKho FROM DM_KhoHang;

    SELECT 
        k.IDKho,
        k.TenKhoHang,
        sp.IDSanPham,
        sp.SoLuongCanXuat
    INTO #TempKhoSanPham
    FROM #TempKho k
    CROSS JOIN #TempSanPham sp;

    SELECT 
        ksp.IDKho,
        ksp.TenKhoHang,
        ksp.IDSanPham,
        dmsp.MaSanPham,
        dmsp.TenSanPham,
        dmsp.DVT,
        ksp.SoLuongCanXuat,
        ISNULL(SUM(ISNULL(g.SoLuongNhap, 0)) - SUM(ISNULL(g.SoLuongXuat, 0)), 0) AS SoLuongTon
    INTO #TempKetQua
    FROM #TempKhoSanPham ksp
    LEFT JOIN DM_SanPham dmsp ON ksp.IDSanPham = dmsp.ID
    LEFT JOIN KHO_GiaoDichKho g ON ksp.IDKho = g.IDKho AND ksp.IDSanPham = g.IDSanPham
    GROUP BY 
        ksp.IDKho,
        ksp.TenKhoHang,
        ksp.IDSanPham,
        dmsp.MaSanPham,
        dmsp.TenSanPham,
        dmsp.DVT,
        ksp.SoLuongCanXuat;

    SELECT 
        IDKho,
        TenKhoHang,
        IDSanPham,
        MaSanPham,
        TenSanPham,
        SoLuongCanXuat,
        SoLuongTon,
        SoLuongTon - SoLuongCanXuat AS ChenhLech,
        CAST(CASE 
            WHEN UPPER(ISNULL(MaSanPham,'')) LIKE '%NODAU%' 
              OR UPPER(ISNULL(TenSanPham,'')) LIKE N'%NỢ ĐẦU KỲ%' 
              OR ISNULL(DVT,'') IN ('', '-', 'DichVu', 'N/A') 
              OR SoLuongTon >= SoLuongCanXuat 
            THEN 1 ELSE 0 
        END AS BIT) AS IsDuTon
    FROM #TempKetQua
    ORDER BY TenKhoHang, TenSanPham;

    DROP TABLE #TempSanPham;
    DROP TABLE #TempKho;
    DROP TABLE #TempKhoSanPham;
    DROP TABLE #TempKetQua;
END
";

                string sqlBy = @"
CREATE OR ALTER PROCEDURE sp_KHO_TonKho_CheckByKho
    @IDKho INT,
    @ListSanPham NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        JSON_VALUE(value, '$.IDSanPham') AS IDSanPham,
        CAST(JSON_VALUE(value, '$.SoLuongCanXuat') AS DECIMAL(18,2)) AS SoLuongCanXuat
    INTO #TempSanPham
    FROM OPENJSON(@ListSanPham);

    SELECT 
        sp.IDSanPham,
        SUM(ISNULL(k.SoLuongNhap, 0)) - SUM(ISNULL(k.SoLuongXuat, 0)) AS SoLuongTon
    INTO #TempTonKho
    FROM #TempSanPham sp
    LEFT JOIN KHO_GiaoDichKho k ON sp.IDSanPham = k.IDSanPham AND k.IDKho = @IDKho
    GROUP BY sp.IDSanPham;

    SELECT 
        @IDKho AS IDKho,
        kho.TenKhoHang,
        sp.IDSanPham,
        dmsp.MaSanPham,
        dmsp.TenSanPham,
        sp.SoLuongCanXuat,
        ISNULL(tk.SoLuongTon, 0) AS SoLuongTon,
        ISNULL(tk.SoLuongTon, 0) - sp.SoLuongCanXuat AS ChenhLech,
        CAST(CASE 
            WHEN UPPER(ISNULL(dmsp.MaSanPham,'')) LIKE '%NODAU%' 
              OR UPPER(ISNULL(dmsp.TenSanPham,'')) LIKE N'%NỢ ĐẦU KỲ%' 
              OR ISNULL(dmsp.DVT,'') IN ('', '-', 'DichVu', 'N/A') 
              OR ISNULL(tk.SoLuongTon, 0) >= sp.SoLuongCanXuat 
            THEN 1 ELSE 0 
        END AS BIT) AS IsDuTon
    FROM #TempSanPham sp
    LEFT JOIN #TempTonKho tk ON sp.IDSanPham = tk.IDSanPham
    LEFT JOIN DM_SanPham dmsp ON sp.IDSanPham = dmsp.ID
    LEFT JOIN DM_KhoHang kho ON kho.ID = @IDKho;

    DROP TABLE #TempSanPham;
    DROP TABLE #TempTonKho;
END
";

                conn.Execute(sqlAll);
                conn.Execute(sqlBy);
                Console.WriteLine("UPDATED sp_KHO_TonKho_CheckAllKho AND sp_KHO_TonKho_CheckByKho SUCCESSFULLY!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
