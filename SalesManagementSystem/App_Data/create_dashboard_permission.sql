-- =============================================
-- Author:      System
-- Create date: 2026-06-20
-- Description: Thêm màn hình Dashboard vào hệ thống phân quyền
-- =============================================

IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenController = 'Dashboard')
BEGIN
    DECLARE @IDNhomManHinh INT;
    
    -- Lấy ID nhóm màn hình 'Hệ thống' hoặc 'Tổng quan'
    SELECT TOP 1 @IDNhomManHinh = ID FROM ACL_NhomManHinh WHERE TenNhom LIKE N'%Hệ thống%' OR TenNhom LIKE N'%Danh mục%';
    
    -- Nếu không có thì lấy nhóm đầu tiên
    IF @IDNhomManHinh IS NULL
        SELECT TOP 1 @IDNhomManHinh = ID FROM ACL_NhomManHinh;
        
    INSERT INTO ACL_ManHinh (IDNhomManHinh, TenManHinh, TenController, TenAction, Icon, ThuTu, IsSuDung)
    VALUES (@IDNhomManHinh, N'Dashboard', 'Dashboard', 'Index', 'bi-speedometer2', 1, 1);
    
    DECLARE @IDManHinh INT = SCOPE_IDENTITY();
    
    -- Thêm các Action (Chỉ có quyền Xem)
    INSERT INTO ACL_Action (IDManHinh, TenAction, MoTa, TenController, TenActionMethod, IsSuDung)
    VALUES (@IDManHinh, 'View', N'Xem Dashboard', 'Dashboard', 'Index', 1);
    
    PRINT N'Đã thêm màn hình Dashboard thành công!';
END
ELSE
BEGIN
    PRINT N'Màn hình Dashboard đã tồn tại!';
END
