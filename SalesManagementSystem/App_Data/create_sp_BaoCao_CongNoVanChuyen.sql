-- =============================================
-- Author:      Antigravity
-- Create date: 2026-08-13
-- Description: Stored Procedures cho Báo cáo công nợ vận chuyển (Bổ sung NgayGiaoHang & HoTenTaiXe)
-- =============================================

-- 1. sp_BaoCao_CongNoVanChuyen_GetList
CREATE OR ALTER PROCEDURE dbo.sp_BaoCao_CongNoVanChuyen_GetList
    @IDPhuongTien INT = NULL,
    @HoTenTaiXe NVARCHAR(255) = NULL,
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @SoPhieuNhap NVARCHAR(50) = NULL,
    @TrangThaiThanhToan INT = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    WITH CTE AS (
        SELECT 
            pn.ID AS IDPhieuNhap,
            pn.SoChungTu AS SoPhieuNhap,
            pn.NgayNhap,
            pn.NgayGiaoHang,
            ISNULL((
                SELECT SUM(ISNULL(ct.DonGiaVanChuyen, 0) * ISNULL(ct.SoLuong, 0)) 
                FROM KHO_PhieuNhap_ChiTiet ct 
                WHERE ct.IDPhieuNhap = pn.ID
            ), ISNULL(pn.TienVanChuyen, 0)) AS TongTienVanChuyen,
            pt.TenPhuongTien AS TenPhuongTien,
            pn.TenNguoiGiao,
            pn.TenNguoiNhan,
            pn.HoTenTaiXe,
            ISNULL((
                SELECT SUM(ct.SoTienPhanBo)
                FROM KT_PhieuChiChiTiet ct
                INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                WHERE ct.IDPhieuNhap = pn.ID 
                  AND ct.LoaiChi = 1
                  AND pc.IsDeleted = 0 
                  AND pc.TrangThai = 2
                  AND pc.IDPhuongTien IS NOT NULL
            ), 0) AS DaThanhToanVanChuyen,
            (
                SELECT STRING_AGG(pc.SoPhieuChi, ', ')
                FROM KT_PhieuChiChiTiet ct
                INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                WHERE ct.IDPhieuNhap = pn.ID 
                  AND ct.LoaiChi = 1
                  AND pc.IsDeleted = 0 
                  AND pc.TrangThai = 2
                  AND pc.IDPhuongTien IS NOT NULL
            ) AS SoPhieuChiList
        FROM KHO_PhieuNhap pn
        LEFT JOIN DM_PhuongTien pt ON pn.IDPhuongTien = pt.ID
        WHERE (@IDPhuongTien IS NULL OR pn.IDPhuongTien = @IDPhuongTien)
          AND (@HoTenTaiXe IS NULL OR pn.HoTenTaiXe LIKE '%' + @HoTenTaiXe + '%' OR pn.TenNguoiGiao LIKE '%' + @HoTenTaiXe + '%' OR pn.TenNguoiNhan LIKE '%' + @HoTenTaiXe + '%')
          AND pn.IsDeleted = 0
          AND pn.TrangThai IN (1, 2)
          AND (@SoPhieuNhap IS NULL OR pn.SoChungTu LIKE '%' + @SoPhieuNhap + '%')
          AND (@TuNgay IS NULL OR CAST(pn.NgayNhap AS DATE) >= CAST(@TuNgay AS DATE))
          AND (@DenNgay IS NULL OR CAST(pn.NgayNhap AS DATE) <= CAST(@DenNgay AS DATE))
    )
    SELECT *, 
           (TongTienVanChuyen - DaThanhToanVanChuyen) AS ConLaiVanChuyen, 
           COUNT(1) OVER() AS TotalRow
    FROM CTE
    WHERE TongTienVanChuyen > 0 
      AND (@TrangThaiThanhToan IS NULL OR 
           (@TrangThaiThanhToan = 0 AND DaThanhToanVanChuyen = 0) OR
           (@TrangThaiThanhToan = 1 AND DaThanhToanVanChuyen > 0 AND (TongTienVanChuyen - DaThanhToanVanChuyen) > 0) OR
           (@TrangThaiThanhToan = 2 AND (TongTienVanChuyen - DaThanhToanVanChuyen) <= 0))
    ORDER BY NgayNhap ASC, SoPhieuNhap ASC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- 2. sp_BaoCao_CongNoVanChuyen_GetDetails
CREATE OR ALTER PROCEDURE dbo.sp_BaoCao_CongNoVanChuyen_GetDetails
    @IDPhieuNhap INT
AS
BEGIN
    SET NOCOUNT ON;

    -- ResultSet 1: Thông tin phiếu nhập
    SELECT pn.ID, pn.SoChungTu, pn.NgayNhap, pn.NgayGiaoHang, ncc.TenNhaCungCap,
           pn.GhiChu, pn.TongTienHang, pn.TongTienThue, pn.TongCong,
           ISNULL((SELECT SUM(ISNULL(ct.DonGiaVanChuyen, 0) * ISNULL(ct.SoLuong, 0)) FROM KHO_PhieuNhap_ChiTiet ct WHERE ct.IDPhieuNhap = pn.ID), ISNULL(pn.TienVanChuyen, 0)) AS TienVanChuyen,
           pt.TenPhuongTien AS TenPhuongTien,
           pn.HoTenTaiXe,
           pn.TenNguoiGiao,
           pn.TenNguoiNhan,
           LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))) AS NguoiTaoTen
    FROM KHO_PhieuNhap pn
    LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN DM_PhuongTien pt ON pn.IDPhuongTien = pt.ID
    LEFT JOIN NS_NhanSu ns ON pn.NguoiTao = ns.ID
    WHERE pn.ID = @IDPhieuNhap AND pn.IsDeleted = 0;

    -- ResultSet 2: Danh sách chi tiết từng sản phẩm
    SELECT ct.ID, ct.IDSanPham, sp.MaSanPham, sp.TenSanPham, sp.DVT,
           ct.SoLuong, ct.DonGia, ct.ThanhTien,
           ISNULL(ct.DonGiaVanChuyen, 0) AS DonGiaVanChuyen,
           (ISNULL(ct.DonGiaVanChuyen, 0) * ISNULL(ct.SoLuong, 0)) AS TienVanChuyen,
           ISNULL(ct.TongSauThue, ct.ThanhTien) AS TongSauThue,
           ct.GhiChu
    FROM KHO_PhieuNhap_ChiTiet ct
    LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
    WHERE ct.IDPhieuNhap = @IDPhieuNhap
    ORDER BY ct.ID;
END
GO
