USE [SalesWarehouseDB]
GO

-- 7. sp_BAN_TraHangBan_Insert
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
    @NewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO BAN_TraHangBan (
        SoChungTu, NgayChungTu, IDDonDatHang, IDKhachHang, IDKho, LyDoTraHang,
        TongSoLuong, TongTienHang, TongTienDaHoan, ConPhaiHoan, TrangThai, NgayTao, NguoiTao
    ) VALUES (
        @SoChungTu, @NgayChungTu, @IDDonDatHang, @IDKhachHang, @IDKho, @LyDoTraHang,
        @TongSoLuong, @TongTienHang, @TongTienDaHoan, @ConPhaiHoan, @TrangThai, GETDATE(), @NguoiTao
    );
    
    SET @NewID = SCOPE_IDENTITY();
END
GO

-- 8. sp_BAN_TraHangBanChiTiet_Insert
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

-- 9. sp_BAN_TraHangBan_Update
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
    @NguoiCapNhat INT
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
        NguoiCapNhat = @NguoiCapNhat
    WHERE ID = @ID AND TrangThai IN (1, 2); -- Cho phep update khi con nhap hoac da ghi
END
GO

-- 10. sp_BAN_TraHangBanChiTiet_DeleteByTraHangId
CREATE OR ALTER PROCEDURE [dbo].[sp_BAN_TraHangBanChiTiet_DeleteByTraHangId]
    @IDTraHang INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM BAN_TraHangBanChiTiet
    WHERE IDTraHang = @IDTraHang;
END
GO

-- 11. sp_BAN_TraHangBan_Delete
CREATE OR ALTER PROCEDURE [dbo].[sp_BAN_TraHangBan_Delete]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        -- Chi xoa khi trang thai la 1 (Luu nhap) hoac 3 (Da huy) (Tuy nghiep vu, thuong chi xoa draft)
        DELETE FROM BAN_TraHangBanChiTiet WHERE IDTraHang = @ID AND EXISTS (SELECT 1 FROM BAN_TraHangBan WHERE ID = @ID AND TrangThai IN (1, 3));
        DELETE FROM BAN_TraHangBan WHERE ID = @ID AND TrangThai IN (1, 3);

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO
