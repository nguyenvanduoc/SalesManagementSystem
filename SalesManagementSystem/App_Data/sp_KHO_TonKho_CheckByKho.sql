CREATE OR ALTER PROCEDURE sp_KHO_TonKho_CheckByKho
    @IDKho INT,
    @ListSanPham NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    -- Lấy danh sách sản phẩm cần xuất từ JSON
    SELECT 
        JSON_VALUE(value, '$.IDSanPham') AS IDSanPham,
        CAST(JSON_VALUE(value, '$.SoLuongCanXuat') AS DECIMAL(18,2)) AS SoLuongCanXuat
    INTO #TempSanPham
    FROM OPENJSON(@ListSanPham);

    -- Lấy tồn kho hiện tại cho các sản phẩm trong danh sách tại kho được chọn
    -- (Yêu cầu: Không lấy tồn kho từ bảng sản phẩm, tính tổng nhập trừ tổng xuất từ KHO_GiaoDichKho)
    SELECT 
        sp.IDSanPham,
        SUM(ISNULL(k.SoLuongNhap, 0)) - SUM(ISNULL(k.SoLuongXuat, 0)) AS SoLuongTon
    INTO #TempTonKho
    FROM #TempSanPham sp
    LEFT JOIN KHO_GiaoDichKho k ON sp.IDSanPham = k.IDSanPham AND k.IDKho = @IDKho
    GROUP BY sp.IDSanPham;

    -- Trả kết quả
    SELECT 
        @IDKho AS IDKho,
        kho.TenKhoHang,
        sp.IDSanPham,
        dmsp.MaSanPham,
        dmsp.TenSanPham,
        sp.SoLuongCanXuat,
        ISNULL(tk.SoLuongTon, 0) AS SoLuongTon,
        ISNULL(tk.SoLuongTon, 0) - sp.SoLuongCanXuat AS ChenhLech,
        CAST(CASE WHEN ISNULL(tk.SoLuongTon, 0) >= sp.SoLuongCanXuat THEN 1 ELSE 0 END AS BIT) AS IsDuTon
    FROM #TempSanPham sp
    LEFT JOIN #TempTonKho tk ON sp.IDSanPham = tk.IDSanPham
    LEFT JOIN DM_SanPham dmsp ON sp.IDSanPham = dmsp.ID
    LEFT JOIN DM_KhoHang kho ON kho.ID = @IDKho;

    DROP TABLE #TempSanPham;
    DROP TABLE #TempTonKho;
END
GO
