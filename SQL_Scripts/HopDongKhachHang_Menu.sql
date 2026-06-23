-- =============================================
-- THÊM MENU VÀO HỆ THỐNG PHÂN QUYỀN (NẾU CẦN)
-- =============================================

-- Lấy ID của Module 'Bán hàng' (Giả sử ParentID = x hoặc Tên là 'Bán hàng')
DECLARE @ParentID INT;
SELECT TOP 1 @ParentID = ID FROM ACL_ManHinh WHERE TenManHinh LIKE N'%Bán hàng%' AND ParentID IS NULL;

-- Thêm vào ACL_ManHinh nếu chưa có
IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'Hợp đồng khách hàng')
BEGIN
    INSERT INTO ACL_ManHinh (TenManHinh, Icon, TieuDe, ParentID, STT, IsSuDung)
    VALUES (N'Hợp đồng khách hàng', 'bi-file-earmark-text', N'Quản lý hợp đồng khách hàng', @ParentID, 99, 1);
    
    DECLARE @ManHinhID INT = SCOPE_IDENTITY();
    
    -- Thêm vào ACL_Action
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, STT, IsSuDung)
    VALUES (@ManHinhID, 'Index', 'HopDongKhachHang', 1, 1);
END
GO
