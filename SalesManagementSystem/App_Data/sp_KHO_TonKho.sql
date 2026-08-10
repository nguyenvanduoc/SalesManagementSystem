-- =======================================================
-- Author:      Antigravity
-- Create date: 2026-06-12
-- Update date: 2026-08-10 (Lọc kho VT/TP bằng LIKE & Không hiển thị SP có tồn hiện tại = 0)
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

    -- Lấy Mã Kho của kho hàng được chọn
    DECLARE @MaKhoSelected NVARCHAR(50) = NULL;
    IF @IDKho IS NOT NULL
    BEGIN
        SELECT TOP 1 @MaKhoSelected = UPPER(TRIM(MaKhoHang)) FROM DM_KhoHang WHERE ID = @IDKho;
    END

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
      -- 1. Nếu không chọn checkbox "Chỉ hiển thị SP còn tồn" (@ChiConTon = 0) -> Không hiển thị SP có tồn hiện tại = 0
      -- 2. Nếu tích chọn checkbox (@ChiConTon = 1) -> Chỉ hiển thị SP có tồn hiện tại > 0
      AND (
          (@ChiConTon = 0 AND ISNULL(gd.TonKho, 0) <> 0)
          OR (@ChiConTon = 1 AND ISNULL(gd.TonKho, 0) > 0)
      )
      AND UPPER(ISNULL(sp.MaSanPham, '')) NOT LIKE '%NODAU%'
      AND UPPER(ISNULL(sp.TenSanPham, '')) NOT LIKE N'%NỢ ĐẦU KỲ%'
      -- Logic lọc mã sản phẩm VT theo mã Kho bằng LIKE:
      AND (
          @MaKhoSelected IS NULL 
          OR (@MaKhoSelected LIKE 'VT%' AND UPPER(LTRIM(sp.MaSanPham)) LIKE 'VT%')
          OR (@MaKhoSelected NOT LIKE 'VT%' AND UPPER(LTRIM(sp.MaSanPham)) NOT LIKE 'VT%')
      )
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
        CASE 
            WHEN EXISTS (SELECT 1 FROM KHO_PhieuNhap WHERE SoChungTu = gd.SoChungTu) THEN 
                ISNULL(
                    (SELECT TOP 1 IDPhieuNhap FROM KHO_PhieuNhap_ChiTiet WHERE ID = gd.IDChiTietKho),
                    (SELECT TOP 1 ID FROM KHO_PhieuNhap WHERE SoChungTu = gd.SoChungTu)
                )
            WHEN EXISTS (SELECT 1 FROM KHO_PhieuXuat WHERE SoChungTu = gd.SoChungTu) THEN 
                ISNULL(
                    (SELECT TOP 1 IDPhieuXuat FROM KHO_PhieuXuat_ChiTiet WHERE ID = gd.IDChiTietKho),
                    (SELECT TOP 1 ID FROM KHO_PhieuXuat WHERE SoChungTu = gd.SoChungTu)
                )
            ELSE gd.ID
        END AS ID,
        gd.NgayChungTu,
        gd.SoChungTu,
        CASE 
            WHEN EXISTS (SELECT 1 FROM KHO_PhieuNhap WHERE SoChungTu = gd.SoChungTu) THEN 1
            WHEN EXISTS (SELECT 1 FROM KHO_PhieuXuat WHERE SoChungTu = gd.SoChungTu) THEN 2
            ELSE gd.LoaiChungTu
        END AS LoaiChungTu,
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
