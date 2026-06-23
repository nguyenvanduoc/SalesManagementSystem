-- =============================================
-- MODULE: HỢP ĐỒNG KHÁCH HÀNG
-- =============================================

-- 1. BẢNG DỮ LIỆU
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BAN_HopDongKhachHang]') AND type in (N'U'))
BEGIN
CREATE TABLE BAN_HopDongKhachHang
(
    ID INT IDENTITY PRIMARY KEY,

    SoHopDong NVARCHAR(50) NOT NULL,
    TenHopDong NVARCHAR(500) NULL,

    IDKhachHang INT NOT NULL,

    NgayKy DATE NULL,

    TuNgay DATE NULL,
    DenNgay DATE NULL,

    GiaTriHopDong DECIMAL(18,2) NOT NULL DEFAULT 0,

    NguoiDaiDien NVARCHAR(255) NULL,
    SoDienThoai NVARCHAR(50) NULL,
    Email NVARCHAR(255) NULL,

    NoiDung NVARCHAR(MAX) NULL,
    GhiChu NVARCHAR(1000) NULL,

    TrangThai INT NOT NULL DEFAULT 1,
    -- 1 Đang hiệu lực
    -- 2 Thanh lý
    -- 3 Hủy

    NgayTao DATETIME NULL,
    NguoiTao INT NULL,
    NgayCapNhat DATETIME NULL,
    NguoiCapNhat INT NULL,

    IsDeleted BIT NOT NULL DEFAULT 0
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BAN_HopDongKhachHang_File]') AND type in (N'U'))
BEGIN
CREATE TABLE BAN_HopDongKhachHang_File
(
    ID INT IDENTITY PRIMARY KEY,

    IDHopDong INT NOT NULL,

    TenFile NVARCHAR(255) NOT NULL,
    LoaiFile NVARCHAR(50) NULL,
    DungLuong BIGINT NULL,

    NoiDungFile VARBINARY(MAX) NOT NULL,

    GhiChu NVARCHAR(500) NULL,

    NgayTao DATETIME NULL,
    NguoiTao INT NULL,

    NgayCapNhat DATETIME NULL,
    NguoiCapNhat INT NULL,

    IsDeleted BIT NOT NULL DEFAULT 0
)
END
GO

-- 2. STORED PROCEDURES

-- GET LIST
IF OBJECT_ID('sp_BAN_HopDongKhachHang_GetList', 'P') IS NOT NULL DROP PROCEDURE sp_BAN_HopDongKhachHang_GetList
GO
CREATE PROCEDURE sp_BAN_HopDongKhachHang_GetList
    @TuNgay DATE = NULL,
    @DenNgay DATE = NULL,
    @SoHopDong NVARCHAR(50) = NULL,
    @TenHopDong NVARCHAR(500) = NULL,
    @IDKhachHang INT = NULL,
    @TrangThai INT = NULL,
    @ChiHienThiSapHetHan BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalRecords INT = 0;
    
    -- Xử lý tham số tìm kiếm
    SET @SoHopDong = LTRIM(RTRIM(ISNULL(@SoHopDong, '')));
    SET @TenHopDong = LTRIM(RTRIM(ISNULL(@TenHopDong, '')));

    -- Lọc dữ liệu vào bảng tạm
    SELECT 
        hd.ID, hd.SoHopDong, hd.TenHopDong, hd.IDKhachHang, 
        kh.MaKhachHang + ' - ' + kh.TenKhachHang AS TenKhachHang,
        hd.NgayKy, hd.TuNgay, hd.DenNgay, hd.GiaTriHopDong, 
        hd.TrangThai, hd.NgayTao,
        nd.HoDem + ' ' + nd.Ten AS TenNguoiTao,
        CASE 
            WHEN hd.TrangThai IN (2, 3) THEN NULL
            WHEN hd.DenNgay IS NULL THEN NULL
            ELSE DATEDIFF(DAY, GETDATE(), hd.DenNgay)
        END AS SoNgayConLai
    INTO #TempData
    FROM BAN_HopDongKhachHang hd
    LEFT JOIN NS_KhachHang kh ON hd.IDKhachHang = kh.ID
    LEFT JOIN AclLogin nd ON hd.NguoiTao = nd.ID
    WHERE hd.IsDeleted = 0
        AND (@TuNgay IS NULL OR hd.NgayKy >= @TuNgay)
        AND (@DenNgay IS NULL OR hd.NgayKy <= @DenNgay)
        AND (@SoHopDong = '' OR hd.SoHopDong LIKE '%' + @SoHopDong + '%')
        AND (@TenHopDong = '' OR hd.TenHopDong LIKE '%' + @TenHopDong + '%')
        AND (@IDKhachHang IS NULL OR hd.IDKhachHang = @IDKhachHang)
        AND (@TrangThai IS NULL OR hd.TrangThai = @TrangThai)
        AND (
            @ChiHienThiSapHetHan = 0 
            OR 
            (@ChiHienThiSapHetHan = 1 AND hd.TrangThai = 1 AND hd.DenNgay IS NOT NULL AND DATEDIFF(DAY, GETDATE(), hd.DenNgay) <= 30 AND DATEDIFF(DAY, GETDATE(), hd.DenNgay) >= 0)
        );

    -- Lấy tổng số dòng
    SELECT @TotalRecords = COUNT(*) FROM #TempData;

    -- Lấy dữ liệu phân trang
    SELECT *, @TotalRecords AS TotalRecords
    FROM #TempData
    ORDER BY NgayKy DESC, ID DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Lấy số liệu Dashboard
    SELECT 
        (SELECT COUNT(*) FROM BAN_HopDongKhachHang WHERE IsDeleted = 0) AS TongHopDong,
        (SELECT COUNT(*) FROM BAN_HopDongKhachHang WHERE IsDeleted = 0 AND TrangThai = 1) AS DangHieuLuc,
        (SELECT COUNT(*) FROM BAN_HopDongKhachHang WHERE IsDeleted = 0 AND TrangThai = 1 AND DenNgay IS NOT NULL AND DATEDIFF(DAY, GETDATE(), DenNgay) <= 30 AND DATEDIFF(DAY, GETDATE(), DenNgay) >= 0) AS SapHetHan,
        (SELECT COUNT(*) FROM BAN_HopDongKhachHang WHERE IsDeleted = 0 AND TrangThai = 2) AS DaThanhLy;

    DROP TABLE #TempData;
END
GO

-- GET BY ID
IF OBJECT_ID('sp_BAN_HopDongKhachHang_GetByID', 'P') IS NOT NULL DROP PROCEDURE sp_BAN_HopDongKhachHang_GetByID
GO
CREATE PROCEDURE sp_BAN_HopDongKhachHang_GetByID
    @ID INT
AS
BEGIN
    SELECT * FROM BAN_HopDongKhachHang WHERE ID = @ID AND IsDeleted = 0;
END
GO

-- CHECK DUPLICATE
IF OBJECT_ID('sp_BAN_HopDongKhachHang_CheckDuplicate', 'P') IS NOT NULL DROP PROCEDURE sp_BAN_HopDongKhachHang_CheckDuplicate
GO
CREATE PROCEDURE sp_BAN_HopDongKhachHang_CheckDuplicate
    @ID INT,
    @SoHopDong NVARCHAR(50)
AS
BEGIN
    IF EXISTS(SELECT 1 FROM BAN_HopDongKhachHang WHERE SoHopDong = @SoHopDong AND ID <> @ID AND IsDeleted = 0)
        SELECT 1 AS IsDuplicate;
    ELSE
        SELECT 0 AS IsDuplicate;
END
GO

-- SAVE
IF OBJECT_ID('sp_BAN_HopDongKhachHang_Save', 'P') IS NOT NULL DROP PROCEDURE sp_BAN_HopDongKhachHang_Save
GO
CREATE PROCEDURE sp_BAN_HopDongKhachHang_Save
    @ID INT OUT,
    @SoHopDong NVARCHAR(50),
    @TenHopDong NVARCHAR(500),
    @IDKhachHang INT,
    @NgayKy DATE,
    @TuNgay DATE,
    @DenNgay DATE,
    @GiaTriHopDong DECIMAL(18,2),
    @NguoiDaiDien NVARCHAR(255),
    @SoDienThoai NVARCHAR(50),
    @Email NVARCHAR(255),
    @NoiDung NVARCHAR(MAX),
    @GhiChu NVARCHAR(1000),
    @NguoiThaoTac INT
AS
BEGIN
    IF @ID = 0
    BEGIN
        INSERT INTO BAN_HopDongKhachHang (
            SoHopDong, TenHopDong, IDKhachHang, NgayKy, TuNgay, DenNgay, 
            GiaTriHopDong, NguoiDaiDien, SoDienThoai, Email, NoiDung, 
            GhiChu, TrangThai, NgayTao, NguoiTao, IsDeleted
        )
        VALUES (
            @SoHopDong, @TenHopDong, @IDKhachHang, @NgayKy, @TuNgay, @DenNgay, 
            @GiaTriHopDong, @NguoiDaiDien, @SoDienThoai, @Email, @NoiDung, 
            @GhiChu, 1, GETDATE(), @NguoiThaoTac, 0
        );
        SET @ID = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE BAN_HopDongKhachHang
        SET SoHopDong = @SoHopDong,
            TenHopDong = @TenHopDong,
            IDKhachHang = @IDKhachHang,
            NgayKy = @NgayKy,
            TuNgay = @TuNgay,
            DenNgay = @DenNgay,
            GiaTriHopDong = @GiaTriHopDong,
            NguoiDaiDien = @NguoiDaiDien,
            SoDienThoai = @SoDienThoai,
            Email = @Email,
            NoiDung = @NoiDung,
            GhiChu = @GhiChu,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @NguoiThaoTac
        WHERE ID = @ID AND IsDeleted = 0;
    END
END
GO

-- DELETE
IF OBJECT_ID('sp_BAN_HopDongKhachHang_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_BAN_HopDongKhachHang_Delete
GO
CREATE PROCEDURE sp_BAN_HopDongKhachHang_Delete
    @ID INT,
    @NguoiThaoTac INT
AS
BEGIN
    UPDATE BAN_HopDongKhachHang
    SET IsDeleted = 1,
        NgayCapNhat = GETDATE(),
        NguoiCapNhat = @NguoiThaoTac
    WHERE ID = @ID;
END
GO

-- THANH LY
IF OBJECT_ID('sp_BAN_HopDongKhachHang_ThanhLy', 'P') IS NOT NULL DROP PROCEDURE sp_BAN_HopDongKhachHang_ThanhLy
GO
CREATE PROCEDURE sp_BAN_HopDongKhachHang_ThanhLy
    @ID INT,
    @NguoiThaoTac INT
AS
BEGIN
    UPDATE BAN_HopDongKhachHang
    SET TrangThai = 2,
        NgayCapNhat = GETDATE(),
        NguoiCapNhat = @NguoiThaoTac
    WHERE ID = @ID AND IsDeleted = 0;
END
GO

-- HUY
IF OBJECT_ID('sp_BAN_HopDongKhachHang_Huy', 'P') IS NOT NULL DROP PROCEDURE sp_BAN_HopDongKhachHang_Huy
GO
CREATE PROCEDURE sp_BAN_HopDongKhachHang_Huy
    @ID INT,
    @NguoiThaoTac INT
AS
BEGIN
    UPDATE BAN_HopDongKhachHang
    SET TrangThai = 3,
        NgayCapNhat = GETDATE(),
        NguoiCapNhat = @NguoiThaoTac
    WHERE ID = @ID AND IsDeleted = 0;
END
GO

-- ==========================================================
-- STORED PROCEDURES (FILE HOP DONG)
-- ==========================================================

-- GET LIST FILES
IF OBJECT_ID('sp_BAN_HopDongKhachHang_File_GetList', 'P') IS NOT NULL DROP PROCEDURE sp_BAN_HopDongKhachHang_File_GetList
GO
CREATE PROCEDURE sp_BAN_HopDongKhachHang_File_GetList
    @IDHopDong INT
AS
BEGIN
    SELECT 
        f.ID, f.IDHopDong, f.TenFile, f.LoaiFile, f.DungLuong, f.GhiChu, 
        f.NgayTao, nd.HoDem + ' ' + nd.Ten AS TenNguoiTao
    FROM BAN_HopDongKhachHang_File f
    LEFT JOIN AclLogin nd ON f.NguoiTao = nd.ID
    WHERE f.IDHopDong = @IDHopDong AND f.IsDeleted = 0
    ORDER BY f.NgayTao DESC;
END
GO

-- GET FILE BY ID
IF OBJECT_ID('sp_BAN_HopDongKhachHang_File_GetByID', 'P') IS NOT NULL DROP PROCEDURE sp_BAN_HopDongKhachHang_File_GetByID
GO
CREATE PROCEDURE sp_BAN_HopDongKhachHang_File_GetByID
    @ID INT
AS
BEGIN
    SELECT * FROM BAN_HopDongKhachHang_File WHERE ID = @ID AND IsDeleted = 0;
END
GO

-- SAVE FILE
IF OBJECT_ID('sp_BAN_HopDongKhachHang_File_Save', 'P') IS NOT NULL DROP PROCEDURE sp_BAN_HopDongKhachHang_File_Save
GO
CREATE PROCEDURE sp_BAN_HopDongKhachHang_File_Save
    @IDHopDong INT,
    @TenFile NVARCHAR(255),
    @LoaiFile NVARCHAR(50),
    @DungLuong BIGINT,
    @NoiDungFile VARBINARY(MAX),
    @GhiChu NVARCHAR(500),
    @NguoiThaoTac INT
AS
BEGIN
    INSERT INTO BAN_HopDongKhachHang_File (
        IDHopDong, TenFile, LoaiFile, DungLuong, NoiDungFile, 
        GhiChu, NgayTao, NguoiTao, IsDeleted
    )
    VALUES (
        @IDHopDong, @TenFile, @LoaiFile, @DungLuong, @NoiDungFile, 
        @GhiChu, GETDATE(), @NguoiThaoTac, 0
    );
END
GO

-- DELETE FILE
IF OBJECT_ID('sp_BAN_HopDongKhachHang_File_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_BAN_HopDongKhachHang_File_Delete
GO
CREATE PROCEDURE sp_BAN_HopDongKhachHang_File_Delete
    @ID INT,
    @NguoiThaoTac INT
AS
BEGIN
    UPDATE BAN_HopDongKhachHang_File
    SET IsDeleted = 1,
        NgayCapNhat = GETDATE(),
        NguoiCapNhat = @NguoiThaoTac
    WHERE ID = @ID;
END
GO
