-- Đăng ký ACL_ManHinh và ACL_Action cho Khoản mục chi
DECLARE @ManHinhID INT;
IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'Khoản mục chi')
BEGIN
    INSERT INTO ACL_ManHinh (TenManHinh, NhomChaManHinh, IsSuDung, STT)
    VALUES (N'Khoản mục chi', N'DANH MỤC', 1, 105);
    SET @ManHinhID = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @ManHinhID = ID FROM ACL_ManHinh WHERE TenManHinh = N'Khoản mục chi';
END

-- Đăng ký các action cho màn hình Khoản mục chi (controller DmKhoanMucChi)
IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Index')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Index', 'DmKhoanMucChi', 1, N'Xem danh sách');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Create')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Create', 'DmKhoanMucChi', 2, N'Thêm mới');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Save')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Save', 'DmKhoanMucChi', 2, N'Lưu thêm mới/cập nhật');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Edit')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Edit', 'DmKhoanMucChi', 3, N'Cập nhật');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Delete')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Delete', 'DmKhoanMucChi', 4, N'Xóa');

-- Gán quyền cho Admin (IDLogin = 1) và tất cả user hiện tại
INSERT INTO ACL_PhanQuyen (IDLogin, IDAction, IsChoPhep, NgayTao)
SELECT l.ID, act.ID, 1, GETDATE()
FROM ACL_Login l
CROSS JOIN ACL_Action act
WHERE act.IDManHinh = @ManHinhID
  AND NOT EXISTS (
      SELECT 1 FROM ACL_PhanQuyen pq 
      WHERE pq.IDLogin = l.ID AND pq.IDAction = act.ID
  );
GO
