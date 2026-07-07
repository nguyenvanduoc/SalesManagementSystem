IF OBJECT_ID('sp_KT_PhieuThu_Insert', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThu_Insert;
GO
CREATE PROCEDURE sp_KT_PhieuThu_Insert
    @SoPhieuThu VARCHAR(50),
    @NgayThu DATE,
    @IDTaiKhoanThanhToan INT,
    @IDKhachHang INT,
    @NguoiNopTien NVARCHAR(250),
    @SoDienThoaiNguoiNop VARCHAR(50),
    @DienGiai NVARCHAR(500),
    @SoTienThu DECIMAL(18,0),
    @NguoiTao INT,
    @NewID INT OUTPUT
AS
BEGIN
    INSERT INTO KT_PhieuThu (SoPhieuThu, NgayThu, IDTaiKhoanThanhToan, IDKhachHang, NguoiNopTien, SoDienThoaiNguoiNop, DienGiai, SoTienThu, TrangThai, NguoiTao)
    VALUES (@SoPhieuThu, @NgayThu, @IDTaiKhoanThanhToan, @IDKhachHang, @NguoiNopTien, @SoDienThoaiNguoiNop, @DienGiai, @SoTienThu, 1, @NguoiTao);
    
    SET @NewID = SCOPE_IDENTITY();
END
GO

IF OBJECT_ID('sp_KT_PhieuThu_Update', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThu_Update;
GO
CREATE PROCEDURE sp_KT_PhieuThu_Update
    @ID INT,
    @SoPhieuThu VARCHAR(50),
    @NgayThu DATE,
    @IDTaiKhoanThanhToan INT,
    @IDKhachHang INT,
    @NguoiNopTien NVARCHAR(250),
    @SoDienThoaiNguoiNop VARCHAR(50),
    @DienGiai NVARCHAR(500),
    @SoTienThu DECIMAL(18,0),
    @NguoiCapNhat INT
AS
BEGIN
    UPDATE KT_PhieuThu
    SET SoPhieuThu = @SoPhieuThu,
        NgayThu = @NgayThu,
        IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan,
        IDKhachHang = @IDKhachHang,
        NguoiNopTien = @NguoiNopTien,
        SoDienThoaiNguoiNop = @SoDienThoaiNguoiNop,
        DienGiai = @DienGiai,
        SoTienThu = @SoTienThu,
        NguoiCapNhat = @NguoiCapNhat,
        NgayCapNhat = GETDATE()
    WHERE ID = @ID;
END
GO

IF OBJECT_ID('sp_KT_PhieuThuChiTiet_Insert', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThuChiTiet_Insert;
GO
CREATE PROCEDURE sp_KT_PhieuThuChiTiet_Insert
    @IDPhieuThu INT,
    @IDChungTuBanHang INT,
    @LoaiThu INT,
    @SoTienPhanBo DECIMAL(18,0),
    @DienGiai NVARCHAR(500)
AS
BEGIN
    INSERT INTO KT_PhieuThuChiTiet (IDPhieuThu, IDChungTuBanHang, LoaiThu, SoTienPhanBo, DienGiai)
    VALUES (@IDPhieuThu, @IDChungTuBanHang, @LoaiThu, @SoTienPhanBo, @DienGiai);
END
GO
