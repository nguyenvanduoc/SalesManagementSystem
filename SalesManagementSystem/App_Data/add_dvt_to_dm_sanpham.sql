-- Bổ sung cột DVT (Đơn Vị Tính) vào bảng DM_SanPham
-- Chạy script này một lần trên database

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'DM_SanPham' AND COLUMN_NAME = 'DVT'
)
BEGIN
    ALTER TABLE DM_SanPham
    ADD DVT NVARCHAR(100) NULL;

    PRINT 'Đã thêm cột DVT vào bảng DM_SanPham thành công.';
END
ELSE
BEGIN
    PRINT 'Cột DVT đã tồn tại trong bảng DM_SanPham.';
END
