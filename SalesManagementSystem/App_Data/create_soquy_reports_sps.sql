-- =============================================
-- Author:      Antigravity
-- Create date: 2026-06-20
-- Description: Stored Procedures for redesigned Cash Book (Sổ Quỹ) Report
-- =============================================

-- 1. sp_KT_SoQuy_GetTaiKhoanSummary
GO
CREATE OR ALTER PROCEDURE sp_KT_SoQuy_GetTaiKhoanSummary
    @TuNgay             DATETIME,
    @DenNgay            DATETIME,
    @IDTaiKhoanThanhToan INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        tk.ID,
        tk.TenTaiKhoan,
        tk.NganHang,
        tk.SoTaiKhoan,
        tk.ChuTaiKhoan,
        -- Số dư đầu kỳ = Tổng thu trước TuNgay - Tổng chi trước TuNgay
        ISNULL((
            SELECT SUM(g.SoTienThu) - SUM(g.SoTienChi)
            FROM QUY_GiaoDichTien g
            WHERE g.IDTaiKhoanThanhToan = tk.ID 
              AND g.NgayGiaoDich < @TuNgay 
              AND g.IsHuy = 0
        ), 0) AS SoDuDauKy,
        -- Thu trong kỳ
        ISNULL((
            SELECT SUM(g.SoTienThu)
            FROM QUY_GiaoDichTien g
            WHERE g.IDTaiKhoanThanhToan = tk.ID 
              AND g.NgayGiaoDich BETWEEN @TuNgay AND @DenNgay 
              AND g.IsHuy = 0
        ), 0) AS ThuTrongKy,
        -- Chi trong kỳ
        ISNULL((
            SELECT SUM(g.SoTienChi)
            FROM QUY_GiaoDichTien g
            WHERE g.IDTaiKhoanThanhToan = tk.ID 
              AND g.NgayGiaoDich BETWEEN @TuNgay AND @DenNgay 
              AND g.IsHuy = 0
        ), 0) AS ChiTrongKy
    FROM DM_TaiKhoanThanhToan tk
    WHERE tk.IsHoatDong = 1
      AND (@IDTaiKhoanThanhToan IS NULL OR tk.ID = @IDTaiKhoanThanhToan)
    ORDER BY tk.TenTaiKhoan;
END
GO

-- 2. sp_KT_SoQuy_GetOpeningBalance
CREATE OR ALTER PROCEDURE sp_KT_SoQuy_GetOpeningBalance
    @TuNgay             DATETIME,
    @IDTaiKhoanThanhToan INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ISNULL(SUM(SoTienThu), 0) - ISNULL(SUM(SoTienChi), 0) AS OpeningBalance
    FROM QUY_GiaoDichTien
    WHERE IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan 
      AND NgayGiaoDich < @TuNgay 
      AND IsHuy = 0;
END
GO

-- 3. sp_KT_SoQuy_GetGiaoDichChiTiet
CREATE OR ALTER PROCEDURE sp_KT_SoQuy_GetGiaoDichChiTiet
    @TuNgay             DATETIME,
    @DenNgay            DATETIME,
    @IDTaiKhoanThanhToan INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        NgayGiaoDich,
        SoChungTu,
        LoaiChungTu,
        DienGiai,
        SoTienThu,
        SoTienChi
    FROM QUY_GiaoDichTien
    WHERE IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan 
      AND NgayGiaoDich BETWEEN @TuNgay AND @DenNgay 
      AND IsHuy = 0
    ORDER BY NgayGiaoDich ASC, ID ASC;
END
GO
