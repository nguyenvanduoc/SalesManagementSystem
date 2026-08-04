CREATE OR ALTER PROCEDURE sp_KHO_TonKho_CheckChuyenKho
    @IDKhoNguon INT,
    @ChiTietsJson NVARCHAR(MAX),
    @IDPhieuNhap INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    -- Lấy Số chứng từ của phiếu hiện tại (nếu đang chỉnh sửa phiếu cũ)
    DECLARE @SoChungTuHienTai NVARCHAR(50) = NULL;
    IF ISNULL(@IDPhieuNhap, 0) > 0
    BEGIN
        SELECT @SoChungTuHienTai = SoChungTu FROM KHO_PhieuNhap WHERE ID = @IDPhieuNhap;
    END

    -- Lấy danh sách sản phẩm và số lượng yêu cầu từ JSON
    SELECT 
        JSON_VALUE(value, '$.IDSanPham') AS IDSanPham,
        CAST(JSON_VALUE(value, '$.SoLuong') AS DECIMAL(18,2)) AS SoLuongYeuCau
    INTO #ChiTiets
    FROM OPENJSON(@ChiTietsJson);

    -- Tính tồn kho thực tế khả dụng của kho nguồn:
    -- (Tồn kho hiện tại trong KHO_GiaoDichKho) + (Cộng lại số lượng đã xuất ra của phiếu hiện tại nếu đang edit)
    ;WITH CTE_TonKho AS (
        SELECT 
            gd.IDSanPham,
            (SUM(gd.SoLuongNhap) - SUM(gd.SoLuongXuat)) 
            + ISNULL(SUM(CASE WHEN @SoChungTuHienTai IS NOT NULL AND gd.SoChungTu = @SoChungTuHienTai AND gd.IDKho = @IDKhoNguon THEN gd.SoLuongXuat ELSE 0 END), 0) AS SoLuongTonKhaDung
        FROM KHO_GiaoDichKho gd
        INNER JOIN #ChiTiets c ON gd.IDSanPham = c.IDSanPham
        WHERE gd.IsHuy = 0 AND (gd.IDKho = @IDKhoNguon OR (@SoChungTuHienTai IS NOT NULL AND gd.SoChungTu = @SoChungTuHienTai))
        GROUP BY gd.IDSanPham
    )
    -- Kiểm tra tồn kho
    SELECT 
        c.IDSanPham,
        sp.MaSanPham,
        sp.TenSanPham,
        c.SoLuongYeuCau,
        ISNULL(tk.SoLuongTonKhaDung, 0) AS SoLuongTon
    FROM #ChiTiets c
    LEFT JOIN DM_SanPham sp ON c.IDSanPham = sp.ID
    LEFT JOIN CTE_TonKho tk ON c.IDSanPham = tk.IDSanPham
    WHERE c.SoLuongYeuCau > ISNULL(tk.SoLuongTonKhaDung, 0);

    DROP TABLE #ChiTiets;
END
