-- Đăng ký ACL_ManHinh và ACL_Action
DECLARE @ManHinhID INT;
IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'Tài khoản thanh toán')
BEGIN
    INSERT INTO ACL_ManHinh (TenManHinh, NhomChaManHinh, IsSuDung, STT)
    VALUES (N'Tài khoản thanh toán', N'DANH MỤC', 1, 100);
END
SELECT @ManHinhID = ID FROM ACL_ManHinh WHERE TenManHinh = N'Tài khoản thanh toán';

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Index')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Index', 'TaiKhoanThanhToan', 1, N'Xem danh sách');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Create')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Create', 'TaiKhoanThanhToan', 2, N'Thêm mới');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Save')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Save', 'TaiKhoanThanhToan', 2, N'Lưu thêm mới/cập nhật');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Edit')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Edit', 'TaiKhoanThanhToan', 3, N'Cập nhật');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Delete')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Delete', 'TaiKhoanThanhToan', 4, N'Xóa');

-- Gán quyền tự động cho Admin (IDLogin = 1)
INSERT INTO ACL_PhanQuyen (IDLogin, IDAction, IsChoPhep, NgayTao)
SELECT 1, act.ID, 1, GETDATE()
FROM ACL_Action act
WHERE act.IDManHinh = @ManHinhID
  AND NOT EXISTS (
      SELECT 1 FROM ACL_PhanQuyen pq 
      WHERE pq.IDLogin = 1 AND pq.IDAction = act.ID
  );
GO

-- 1. sp_DM_TaiKhoanThanhToan_GetList
CREATE OR ALTER PROCEDURE sp_DM_TaiKhoanThanhToan_GetList
    @Page INT = 1,
    @PageSize INT = 20,
    @Keyword NVARCHAR(200) = NULL,
    @IsHoatDong INT = NULL,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT 
        t.ID,
        t.MaTaiKhoan,
        t.TenTaiKhoan,
        t.NganHang,
        t.SoTaiKhoan,
        t.ChuTaiKhoan,
        t.IsHoatDong,
        t.IDTaiKhoanKeToan,
        k.SoTaiKhoan AS SoTaiKhoanKeToan,
        k.TenTaiKhoan AS TenTaiKhoanKeToan,
        t.NgayTao,
        t.NguoiTao,
        t.NgayCapNhat,
        t.NguoiCapNhat,
        t.LoaiTaiKhoan
    INTO #TempList
    FROM DM_TaiKhoanThanhToan t
    LEFT JOIN KT_TaiKhoanKeToan k ON t.IDTaiKhoanKeToan = k.ID
    WHERE (@Keyword IS NULL 
           OR t.MaTaiKhoan LIKE N'%' + @Keyword + N'%'
           OR t.TenTaiKhoan LIKE N'%' + @Keyword + N'%'
           OR t.SoTaiKhoan LIKE N'%' + @Keyword + N'%')
      AND (@IsHoatDong IS NULL OR t.IsHoatDong = @IsHoatDong);

    SELECT @TotalRecords = COUNT(*) FROM #TempList;

    SELECT * FROM #TempList
    ORDER BY MaTaiKhoan ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    DROP TABLE #TempList;
END
GO

-- 2. sp_DM_TaiKhoanThanhToan_GetByID
CREATE OR ALTER PROCEDURE sp_DM_TaiKhoanThanhToan_GetByID
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM DM_TaiKhoanThanhToan WHERE ID = @ID;
END
GO

-- 3. sp_DM_TaiKhoanThanhToan_Save
CREATE OR ALTER PROCEDURE sp_DM_TaiKhoanThanhToan_Save
    @ID INT,
    @MaTaiKhoan NVARCHAR(50),
    @TenTaiKhoan NVARCHAR(250),
    @NganHang NVARCHAR(250) = NULL,
    @SoTaiKhoan NVARCHAR(50) = NULL,
    @ChuTaiKhoan NVARCHAR(250) = NULL,
    @IsHoatDong BIT = 1,
    @IDTaiKhoanKeToan INT = NULL,
    @UserId INT,
    @NewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @ID = 0 OR @ID IS NULL
    BEGIN
        INSERT INTO DM_TaiKhoanThanhToan (
            MaTaiKhoan, TenTaiKhoan, NganHang, SoTaiKhoan, ChuTaiKhoan, 
            IsHoatDong, IDTaiKhoanKeToan, NgayTao, NguoiTao
        )
        VALUES (
            @MaTaiKhoan, @TenTaiKhoan, @NganHang, @SoTaiKhoan, @ChuTaiKhoan,
            @IsHoatDong, @IDTaiKhoanKeToan, GETDATE(), @UserId
        );
        SET @NewID = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE DM_TaiKhoanThanhToan
        SET MaTaiKhoan = @MaTaiKhoan,
            TenTaiKhoan = @TenTaiKhoan,
            NganHang = @NganHang,
            SoTaiKhoan = @SoTaiKhoan,
            ChuTaiKhoan = @ChuTaiKhoan,
            IsHoatDong = @IsHoatDong,
            IDTaiKhoanKeToan = @IDTaiKhoanKeToan,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @UserId
        WHERE ID = @ID;
        SET @NewID = @ID;
    END
END
GO

-- 4. sp_DM_TaiKhoanThanhToan_Delete
CREATE OR ALTER PROCEDURE sp_DM_TaiKhoanThanhToan_Delete
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM DM_TaiKhoanThanhToan WHERE ID = @ID;
END
GO

-- 5. sp_DM_TaiKhoanThanhToan_CheckDuplicateCode
CREATE OR ALTER PROCEDURE sp_DM_TaiKhoanThanhToan_CheckDuplicateCode
    @MaTaiKhoan NVARCHAR(50),
    @CurrentID INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM DM_TaiKhoanThanhToan WHERE MaTaiKhoan = @MaTaiKhoan AND ID <> @CurrentID)
        SELECT 1 AS Result;
    ELSE
        SELECT 0 AS Result;
END
GO
