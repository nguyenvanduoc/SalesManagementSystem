USE [SalesWarehouseDB]
GO

-- 1. Revert ChiTiet table change
IF COL_LENGTH('BAN_TraHangBanChiTiet', 'PhiBocXep') IS NOT NULL
BEGIN
    ALTER TABLE BAN_TraHangBanChiTiet DROP COLUMN PhiBocXep;
END
GO

-- 2. Add PhiBocXep to master table
IF COL_LENGTH('BAN_TraHangBan', 'PhiBocXep') IS NULL
BEGIN
    ALTER TABLE BAN_TraHangBan ADD PhiBocXep DECIMAL(18,2) NULL;
END
GO

-- 3. Revert sp_BAN_TraHangBanChiTiet_Insert
CREATE OR ALTER PROCEDURE [dbo].[sp_BAN_TraHangBanChiTiet_Insert]
    @IDTraHang INT,
    @IDSanPham INT,
    @SoLuongBan DECIMAL(18, 2),
    @SoLuongDaTra DECIMAL(18, 2),
    @SoLuongConLai DECIMAL(18, 2),
    @SoLuongTra DECIMAL(18, 2),
    @DonGia DECIMAL(18, 2),
    @ThanhTien DECIMAL(18, 2),
    @GhiChu NVARCHAR(500),
    @NguoiTao INT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO BAN_TraHangBanChiTiet (
        IDTraHang, IDSanPham, SoLuongBan, SoLuongDaTra, SoLuongConLai,
        SoLuongTra, DonGia, ThanhTien, GhiChu, NgayTao, NguoiTao
    ) VALUES (
        @IDTraHang, @IDSanPham, @SoLuongBan, @SoLuongDaTra, @SoLuongConLai,
        @SoLuongTra, @DonGia, @ThanhTien, @GhiChu, GETDATE(), @NguoiTao
    );
END
GO

-- 4. Update sp_BAN_TraHangBan_Insert
CREATE OR ALTER PROCEDURE [dbo].[sp_BAN_TraHangBan_Insert]
    @SoChungTu NVARCHAR(50),
    @NgayChungTu DATETIME,
    @IDDonDatHang INT,
    @IDKhachHang INT,
    @IDKho INT,
    @LyDoTraHang NVARCHAR(500),
    @TongSoLuong DECIMAL(18, 2),
    @TongTienHang DECIMAL(18, 2),
    @TongTienDaHoan DECIMAL(18, 2),
    @ConPhaiHoan DECIMAL(18, 2),
    @TrangThai INT,
    @NguoiTao INT,
    @PhiBocXep DECIMAL(18, 2) = NULL,
    @NewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO BAN_TraHangBan (
        SoChungTu, NgayChungTu, IDDonDatHang, IDKhachHang, IDKho, LyDoTraHang,
        TongSoLuong, TongTienHang, TongTienDaHoan, ConPhaiHoan, TrangThai, NgayTao, NguoiTao, PhiBocXep
    ) VALUES (
        @SoChungTu, @NgayChungTu, @IDDonDatHang, @IDKhachHang, @IDKho, @LyDoTraHang,
        @TongSoLuong, @TongTienHang, @TongTienDaHoan, @ConPhaiHoan, @TrangThai, GETDATE(), @NguoiTao, @PhiBocXep
    );
    
    SET @NewID = SCOPE_IDENTITY();
END
GO

-- 5. Update sp_BAN_TraHangBan_Update
CREATE OR ALTER PROCEDURE [dbo].[sp_BAN_TraHangBan_Update]
    @ID INT,
    @NgayChungTu DATETIME,
    @IDDonDatHang INT,
    @IDKhachHang INT,
    @IDKho INT,
    @LyDoTraHang NVARCHAR(500),
    @TongSoLuong DECIMAL(18, 2),
    @TongTienHang DECIMAL(18, 2),
    @TongTienDaHoan DECIMAL(18, 2),
    @ConPhaiHoan DECIMAL(18, 2),
    @NguoiCapNhat INT,
    @PhiBocXep DECIMAL(18, 2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE BAN_TraHangBan
    SET NgayChungTu = @NgayChungTu,
        IDDonDatHang = @IDDonDatHang,
        IDKhachHang = @IDKhachHang,
        IDKho = @IDKho,
        LyDoTraHang = @LyDoTraHang,
        TongSoLuong = @TongSoLuong,
        TongTienHang = @TongTienHang,
        TongTienDaHoan = @TongTienDaHoan,
        ConPhaiHoan = @ConPhaiHoan,
        NgayCapNhat = GETDATE(),
        NguoiCapNhat = @NguoiCapNhat,
        PhiBocXep = @PhiBocXep
    WHERE ID = @ID AND TrangThai IN (1, 2);
END
GO
