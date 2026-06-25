CREATE OR ALTER PROCEDURE sp_KHO_TonKho_CheckChuyenKho
    @IDKhoNguon INT,
    @ChiTietsJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    -- Lấy danh sách sản phẩm và số lượng yêu cầu từ JSON
    SELECT 
        JSON_VALUE(value, '$.IDSanPham') AS IDSanPham,
        CAST(JSON_VALUE(value, '$.SoLuong') AS DECIMAL(18,2)) AS SoLuongYeuCau
    INTO #ChiTiets
    FROM OPENJSON(@ChiTietsJson);

    -- Tính tồn kho hiện tại từ KHO_GiaoDichKho
    ;WITH CTE_TonKho AS (
        SELECT 
            gd.IDSanPham,
            SUM(gd.SoLuongNhap) - SUM(gd.SoLuongXuat) AS SoLuongTon
        FROM KHO_GiaoDichKho gd
        INNER JOIN #ChiTiets c ON gd.IDSanPham = c.IDSanPham
        WHERE gd.IsHuy = 0 AND gd.IDKho = @IDKhoNguon
        GROUP BY gd.IDSanPham
    )
    -- Kiểm tra tồn kho
    SELECT 
        c.IDSanPham,
        sp.MaSanPham,
        sp.TenSanPham,
        c.SoLuongYeuCau,
        ISNULL(tk.SoLuongTon, 0) AS SoLuongTon
    FROM #ChiTiets c
    LEFT JOIN DM_SanPham sp ON c.IDSanPham = sp.ID
    LEFT JOIN CTE_TonKho tk ON c.IDSanPham = tk.IDSanPham
    WHERE c.SoLuongYeuCau > ISNULL(tk.SoLuongTon, 0);

    DROP TABLE #ChiTiets;
END
GO
