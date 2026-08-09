CREATE OR ALTER PROCEDURE sp_KHO_TonKho_CheckAllKho
    @ListSanPham NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Lấy danh sách sản phẩm cần xuất từ JSON
    SELECT 
        JSON_VALUE(value, '$.IDSanPham') AS IDSanPham,
        CAST(JSON_VALUE(value, '$.SoLuongCanXuat') AS DECIMAL(18,2)) AS SoLuongCanXuat
    INTO #TempSanPham
    FROM OPENJSON(@ListSanPham);

    -- 2. Lấy danh sách tất cả các kho
    SELECT ID AS IDKho, TenKhoHang INTO #TempKho FROM DM_KhoHang;

    -- 3. Cross Join Kho x Sản phẩm để lấy ma trận Kho - Sản phẩm
    SELECT 
        k.IDKho,
        k.TenKhoHang,
        sp.IDSanPham,
        sp.SoLuongCanXuat
    INTO #TempKhoSanPham
    FROM #TempKho k
    CROSS JOIN #TempSanPham sp;

    -- 4. Tính tồn kho cho từng cặp Kho - Sản phẩm
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

    -- 5. Trả về kết quả (Miễn trừ kiểm tra tồn kho nếu là sản phẩm Nợ đầu kỳ / Dịch vụ)
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

    -- Dọn dẹp
    DROP TABLE #TempSanPham;
    DROP TABLE #TempKho;
    DROP TABLE #TempKhoSanPham;
    DROP TABLE #TempKetQua;
END
GO
