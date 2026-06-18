-- =====================================================
-- STEP 1: ALTER TABLE thêm cột IDNguoiThu
-- =====================================================
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'BAN_PhieuThuKhachHang'
      AND COLUMN_NAME = 'IDNguoiThu'
)
BEGIN
    ALTER TABLE BAN_PhieuThuKhachHang ADD IDNguoiThu INT NULL;
    PRINT 'Column IDNguoiThu added.';
END
ELSE
BEGIN
    PRINT 'Column IDNguoiThu already exists.';
END
GO

-- =====================================================
-- STEP 2: sp_BAN_PhieuThuKhachHang_GetByID
--   (thêm IDNguoiThu, TenNguoiThu, fix NhanSu -> NS_NhanSu)
-- =====================================================
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_GetByID
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        pt.ID,
        pt.SoPhieuThu,
        pt.NgayThu,
        pt.IDChungTuBanHang,
        ct.SoChungTu AS SoChungTuBanHang,
        pt.IDKhachHang,
        kh.TenKhachHang,
        pt.IDTaiKhoanThanhToan,
        tk.SoTaiKhoan,
        tk.TenTaiKhoan,
        pt.SoTienThu,
        pt.GhiChu,
        pt.TrangThai,
        pt.NgayTao,
        pt.NguoiTao,
        pt.NgayCapNhat,
        pt.NguoiCapNhat,
        pt.NgayGhi,
        pt.NguoiGhi,
        pt.NgayHuy,
        pt.NguoiHuy,
        pt.LyDoHuy,
        pt.IDNguoiThu,
        LTRIM(RTRIM(ISNULL(thu.HoDem, '') + ' ' + ISNULL(thu.Ten, ''))) AS TenNguoiThu,
        ct.TongCong AS TongChungTu,
        -- Đã thanh toán trước đó
        ISNULL((
            SELECT SUM(p.SoTienThu) 
            FROM BAN_PhieuThuKhachHang p 
            WHERE p.IDChungTuBanHang = pt.IDChungTuBanHang 
              AND p.TrangThai = 2 
              AND p.IsDeleted = 0 
              AND p.ID <> pt.ID
        ), 0) AS DaThanhToanTruoc,
        -- Còn lại sau thu
        (ct.TongCong - ISNULL((
            SELECT SUM(p.SoTienThu) 
            FROM BAN_PhieuThuKhachHang p 
            WHERE p.IDChungTuBanHang = pt.IDChungTuBanHang 
              AND p.TrangThai = 2 
              AND p.IsDeleted = 0 
              AND p.ID <> pt.ID
        ), 0) - pt.SoTienThu) AS ConLaiSauThu,
        LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))) AS TenNguoiTao
    FROM BAN_PhieuThuKhachHang pt
    JOIN BAN_ChungTuBanHang ct ON pt.IDChungTuBanHang = ct.ID
    JOIN NS_KhachHang kh ON pt.IDKhachHang = kh.ID
    JOIN DM_TaiKhoanThanhToan tk ON pt.IDTaiKhoanThanhToan = tk.ID
    LEFT JOIN NS_NhanSu ns ON pt.NguoiTao = ns.ID
    LEFT JOIN NS_NhanSu thu ON pt.IDNguoiThu = thu.ID
    WHERE pt.ID = @ID AND pt.IsDeleted = 0;
END
GO

-- =====================================================
-- STEP 3: sp_BAN_PhieuThuKhachHang_Save
--   (thêm @IDNguoiThu vào INSERT và UPDATE)
-- =====================================================
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_Save
    @ID INT = NULL,
    @SoPhieuThu NVARCHAR(50),
    @NgayThu DATE,
    @IDChungTuBanHang INT,
    @IDKhachHang INT,
    @IDTaiKhoanThanhToan INT,
    @SoTienThu DECIMAL(18,2),
    @GhiChu NVARCHAR(1000) = NULL,
    @TrangThai INT,
    @IDNguoiThu INT = NULL,
    @UserId INT,
    @NewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ID IS NULL OR @ID = 0
    BEGIN
        INSERT INTO BAN_PhieuThuKhachHang (
            SoPhieuThu, NgayThu, IDChungTuBanHang, IDKhachHang, IDTaiKhoanThanhToan, 
            SoTienThu, GhiChu, TrangThai, IDNguoiThu, NgayTao, NguoiTao, IsDeleted
        )
        VALUES (
            @SoPhieuThu, @NgayThu, @IDChungTuBanHang, @IDKhachHang, @IDTaiKhoanThanhToan, 
            @SoTienThu, @GhiChu, @TrangThai, @IDNguoiThu, GETDATE(), @UserId, 0
        );
        SET @NewID = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE BAN_PhieuThuKhachHang
        SET 
            NgayThu = @NgayThu,
            IDChungTuBanHang = @IDChungTuBanHang,
            IDKhachHang = @IDKhachHang,
            IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan,
            SoTienThu = @SoTienThu,
            GhiChu = @GhiChu,
            TrangThai = @TrangThai,
            IDNguoiThu = @IDNguoiThu,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @UserId
        WHERE ID = @ID AND IsDeleted = 0;
        
        SET @NewID = @ID;
    END
END
GO

PRINT 'All changes applied successfully.';
