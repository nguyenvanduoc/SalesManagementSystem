-- Cập nhật bảng KHO_PhieuNhap
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IDLoaiNhapKho' AND Object_ID = Object_ID(N'KHO_PhieuNhap'))
BEGIN
    ALTER TABLE KHO_PhieuNhap
    ADD IDLoaiNhapKho INT NULL;
END
GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IDKhoNguon' AND Object_ID = Object_ID(N'KHO_PhieuNhap'))
BEGIN
    ALTER TABLE KHO_PhieuNhap
    ADD IDKhoNguon INT NULL;
END
GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IDKhachHang' AND Object_ID = Object_ID(N'KHO_PhieuNhap'))
BEGIN
    ALTER TABLE KHO_PhieuNhap
    ADD IDKhachHang INT NULL;
END
GO

-- Cập nhật SP sp_KHO_TonKho_CheckChuyenKho
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

    -- Kiểm tra tồn kho
    SELECT 
        c.IDSanPham,
        sp.MaSanPham,
        sp.TenSanPham,
        c.SoLuongYeuCau,
        ISNULL(tk.SoLuongTon, 0) AS SoLuongTon
    FROM #ChiTiets c
    LEFT JOIN DM_SanPham sp ON c.IDSanPham = sp.ID
    LEFT JOIN KHO_TonKho tk ON c.IDSanPham = tk.IDSanPham AND tk.IDKho = @IDKhoNguon
    WHERE c.SoLuongYeuCau > ISNULL(tk.SoLuongTon, 0);

    DROP TABLE #ChiTiets;
END
GO
