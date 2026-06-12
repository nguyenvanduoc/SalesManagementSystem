-- =======================================================
-- Author:      Antigravity
-- Create date: 2026-06-12
-- Description: Get list of inventory for Tồn Kho screen
-- =======================================================
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
            SUM(gd.SoLuongNhap) AS TongNhap,
            SUM(gd.SoLuongXuat) AS TongXuat,
            SUM(gd.SoLuongNhap) - SUM(gd.SoLuongXuat) AS TonKho,
            MAX(CASE WHEN gd.SoLuongNhap > 0 THEN gd.NgayChungTu ELSE NULL END) AS NgayNhapCuoi,
            MAX(CASE WHEN gd.SoLuongXuat > 0 THEN gd.NgayChungTu ELSE NULL END) AS NgayXuatCuoi,
            (SELECT TOP 1 DonGia FROM KHO_GiaoDichKho WHERE IDSanPham = gd.IDSanPham AND IDKho = gd.IDKho AND SoLuongNhap > 0 AND IsHuy = 0 ORDER BY NgayChungTu DESC, ID DESC) AS DonGiaCuoi
        FROM KHO_GiaoDichKho gd
        WHERE gd.IsHuy = 0
          AND (@IDKho IS NULL OR gd.IDKho = @IDKho)
          AND (@IDSanPham IS NULL OR gd.IDSanPham = @IDSanPham)
          AND (@TuNgay IS NULL OR gd.NgayChungTu >= @TuNgay)
          AND (@DenNgay IS NULL OR gd.NgayChungTu <= @DenNgay)
        GROUP BY gd.IDKho, gd.IDSanPham
    )
    SELECT 
        gd.IDKho,
        k.MaKhoHang AS MaKho,
        k.TenKhoHang AS TenKho,
        gd.IDSanPham,
        sp.MaSanPham,
        sp.TenSanPham,
        sp.DVT,
        gd.TongNhap,
        gd.TongXuat,
        gd.TonKho,
        ISNULL(gd.DonGiaCuoi, 0) AS DonGiaTon,
        gd.TonKho * ISNULL(gd.DonGiaCuoi, 0) AS GiaTriTon,
        gd.NgayNhapCuoi,
        gd.NgayXuatCuoi,
        0 AS MucTonToiThieu
    FROM CTE_GiaoDich gd
    LEFT JOIN DM_KhoHang k ON gd.IDKho = k.ID
    LEFT JOIN DM_SanPham sp ON gd.IDSanPham = sp.ID
    WHERE (@ChiConTon = 0 OR gd.TonKho > 0)
    ORDER BY k.TenKhoHang, sp.TenSanPham;
END
GO

-- =======================================================
-- Author:      Antigravity
-- Create date: 2026-06-12
-- Description: Get inventory history (Thẻ kho)
-- =======================================================
CREATE OR ALTER PROCEDURE sp_KHO_TheKho_GetList
    @IDKho INT,
    @IDSanPham INT,
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TonDau DECIMAL(18,2) = 0;
    
    IF @TuNgay IS NOT NULL
    BEGIN
        SELECT @TonDau = ISNULL(SUM(SoLuongNhap), 0) - ISNULL(SUM(SoLuongXuat), 0)
        FROM KHO_GiaoDichKho
        WHERE IsHuy = 0 
          AND IDKho = @IDKho 
          AND IDSanPham = @IDSanPham 
          AND NgayChungTu < @TuNgay;
    END

    SELECT 
        gd.ID,
        gd.NgayChungTu,
        gd.SoChungTu,
        gd.LoaiChungTu,
        gd.DienGiai,
        gd.SoLuongNhap AS Nhap,
        gd.SoLuongXuat AS Xuat,
        gd.DonGia,
        gd.ThanhTien,
        @TonDau + SUM(gd.SoLuongNhap - gd.SoLuongXuat) OVER (ORDER BY gd.NgayChungTu ASC, gd.ID ASC) AS TonLuyKe
    FROM KHO_GiaoDichKho gd
    WHERE gd.IsHuy = 0
      AND gd.IDKho = @IDKho
      AND gd.IDSanPham = @IDSanPham
      AND (@TuNgay IS NULL OR gd.NgayChungTu >= @TuNgay)
      AND (@DenNgay IS NULL OR gd.NgayChungTu <= @DenNgay)
    ORDER BY gd.NgayChungTu ASC, gd.ID ASC;
END
GO
