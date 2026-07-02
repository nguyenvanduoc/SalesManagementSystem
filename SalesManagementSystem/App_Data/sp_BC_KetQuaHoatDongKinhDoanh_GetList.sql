
IF OBJECT_ID('sp_BC_KetQuaHoatDongKinhDoanh_GetList', 'P') IS NOT NULL
    DROP PROCEDURE sp_BC_KetQuaHoatDongKinhDoanh_GetList
GO

CREATE PROCEDURE [dbo].[sp_BC_KetQuaHoatDongKinhDoanh_GetList]
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @IDKho INT = NULL,
    @IDSanPham INT = NULL,
    @DonViTinh NVARCHAR(50) = NULL,
    @MaSanPham NVARCHAR(50) = NULL,
    @TenSanPham NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Đảm bảo đến hết ngày
    SET @DenNgay = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(CAST(@DenNgay AS DATE) AS DATETIME)));
    
    -- 1. Bảng tạm chứa Doanh Thu và Giá Vốn (từ chứng từ bán hàng đã ghi sổ)
    SELECT 
        ct.IDSanPham,
        SUM(ct.SoLuong) AS SoLuongDoanhThu,
        SUM(ct.ThanhTien) AS ThanhTienDoanhThu, -- Tổng tiền đã trừ phí bốc xếp
        SUM(ct.SoLuong) AS SoLuongGiaVon,
        SUM(ISNULL(ct.ThanhTienVon, 0)) AS ThanhTienGiaVon,
        -- Phân bổ phí vận chuyển: (ThanhTien của chi tiết / Tổng thành tiền của đơn) * Phí vận chuyển của đơn
        SUM(
            ISNULL(dh.TongTienVanChuyen, 0) * 
            CASE WHEN ISNULL(hd.TongTienHang, 0) = 0 THEN 0 ELSE (ct.ThanhTienHang / hd.TongTienHang) END
        ) AS ChiPhiVanChuyen
    INTO #TmpDoanhThu
    FROM BAN_ChungTuBanHang_ChiTiet ct
    INNER JOIN BAN_ChungTuBanHang hd ON ct.IDChungTuBanHang = hd.ID
    LEFT JOIN NS_DonDatHang dh ON hd.IDDonDatHang = dh.ID
    WHERE hd.IsDeleted = 0 
      AND hd.TrangThai = 2 -- Đã ghi sổ
      AND hd.NgayChungTu >= @TuNgay AND hd.NgayChungTu <= @DenNgay
      AND (@IDKho IS NULL OR hd.IDKho = @IDKho)
    GROUP BY ct.IDSanPham

    -- 3. Tổng hợp dữ liệu
    SELECT 
        sp.ID AS IDSanPham,
        sp.MaSanPham,
        sp.TenSanPham,
        sp.DVT AS DonViTinh,
        NULL AS IDSanPhamCha,
        ISNULL(dt.SoLuongDoanhThu, 0) AS SoLuongDoanhThu,
        ISNULL(dt.ThanhTienDoanhThu, 0) AS ThanhTienDoanhThu,
        ISNULL(dt.SoLuongGiaVon, 0) AS SoLuongGiaVon,
        ISNULL(dt.ThanhTienGiaVon, 0) AS ThanhTienGiaVon,
        ISNULL(dt.ChiPhiVanChuyen, 0) AS ChiPhiVanChuyen,
        CAST(0 AS DECIMAL(18,2)) AS ChiPhiBaoBi,
        
        -- Lợi nhuận gộp = Doanh thu - Giá vốn
        (ISNULL(dt.ThanhTienDoanhThu, 0) - ISNULL(dt.ThanhTienGiaVon, 0)) AS LoiNhuanGop,
        
        -- Lợi nhuận thuần = Lợi nhuận gộp - Chi phí VC - Chi phí BB
        (ISNULL(dt.ThanhTienDoanhThu, 0) - ISNULL(dt.ThanhTienGiaVon, 0) - ISNULL(dt.ChiPhiVanChuyen, 0)) AS LoiNhuanThuan,
        
        -- Tỷ suất LN = LN thuần / Doanh thu
        CASE 
            WHEN ISNULL(dt.ThanhTienDoanhThu, 0) > 0 
            THEN ((ISNULL(dt.ThanhTienDoanhThu, 0) - ISNULL(dt.ThanhTienGiaVon, 0) - ISNULL(dt.ChiPhiVanChuyen, 0)) / dt.ThanhTienDoanhThu) * 100
            ELSE 0 
        END AS TySuatLoiNhuan,
        CAST(0 AS BIT) AS IsGroup,
        NULL AS STT
    INTO #TmpResult
    FROM DM_SanPham sp
    LEFT JOIN #TmpDoanhThu dt ON sp.ID = dt.IDSanPham
    WHERE dt.IDSanPham IS NOT NULL
      AND (@IDSanPham IS NULL OR sp.ID = @IDSanPham)
      AND (@DonViTinh IS NULL OR sp.DVT = @DonViTinh)
      AND (@MaSanPham IS NULL OR sp.MaSanPham LIKE '%' + @MaSanPham + '%')
      AND (@TenSanPham IS NULL OR sp.TenSanPham LIKE '%' + @TenSanPham + '%')

    -- Trả kết quả
    SELECT * FROM #TmpResult ORDER BY TenSanPham

    DROP TABLE #TmpDoanhThu
    DROP TABLE #TmpResult
END
GO
