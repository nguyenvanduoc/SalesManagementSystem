IF COL_LENGTH('NS_DonDatHang', 'TongTienKhac') IS NULL 
    ALTER TABLE NS_DonDatHang ADD TongTienKhac DECIMAL(18,2) NULL;

IF COL_LENGTH('NS_DonDatHangChiTiet', 'SoTienKhac') IS NULL 
    ALTER TABLE NS_DonDatHangChiTiet ADD SoTienKhac DECIMAL(18,2) NULL;

IF COL_LENGTH('BAN_ChungTuBanHang', 'TongTienKhac') IS NULL 
    ALTER TABLE BAN_ChungTuBanHang ADD TongTienKhac DECIMAL(18,2) NULL;

IF COL_LENGTH('BAN_ChungTuBanHang_ChiTiet', 'SoTienKhac') IS NULL 
    ALTER TABLE BAN_ChungTuBanHang_ChiTiet ADD SoTienKhac DECIMAL(18,2) NULL;
GO

CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_GetList
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKhachHang INT = NULL,
    @IDKho INT = NULL,
    @TrangThai INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        b.ID,
        b.SoChungTu,
        b.NgayChungTu,
        b.IDDonDatHang,
        d.SoDonHang,
        b.IDKhachHang,
        k.TenKhachHang,
        b.IDKho,
        kho.TenKhoHang,
        b.IDTaiKhoanThanhToan,
        tk.SoTaiKhoan,
        b.TongTienHang,
        b.TongTienThue,
        b.TongCong,
        b.TongTienKhac,
        b.DaThanhToan,
        b.ConLai,
        b.TrangThai,
        b.NgayTao,
        b.NguoiTao,
        d.SoDienThoaiTaiXe,
        d.HoTenTaiXe
    FROM BAN_ChungTuBanHang b
    LEFT JOIN NS_DonDatHang d ON b.IDDonDatHang = d.ID
    LEFT JOIN NS_KhachHang k ON b.IDKhachHang = k.ID
    LEFT JOIN DM_KhoHang kho ON b.IDKho = kho.ID
    LEFT JOIN KT_TaiKhoanKeToan tk ON b.IDTaiKhoanThanhToan = tk.ID
    WHERE b.IsDeleted = 0
      AND (@TuNgay IS NULL OR CAST(b.NgayChungTu AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay IS NULL OR CAST(b.NgayChungTu AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@SoChungTu IS NULL OR b.SoChungTu LIKE '%' + @SoChungTu + '%' OR d.SoDonHang LIKE '%' + @SoChungTu + '%')
      AND (@IDKhachHang IS NULL OR b.IDKhachHang = @IDKhachHang)
      AND (@IDKho IS NULL OR b.IDKho = @IDKho)
      AND (@TrangThai IS NULL OR b.TrangThai = @TrangThai)
    ORDER BY b.NgayChungTu DESC, b.ID DESC
END
GO

CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_Insert
    @SoChungTu NVARCHAR(50),
    @NgayChungTu DATE,
    @IDDonDatHang INT = NULL,
    @IDKhachHang INT,
    @IDKho INT,
    @IDTaiKhoanThanhToan INT = NULL,
    @TongTienHang DECIMAL(18,2),
    @TongTienThue DECIMAL(18,2),
    @PhiBocXep DECIMAL(18,2) = 0,
    @TongTienKhac DECIMAL(18,2) = 0,
    @TongTienChietKhau DECIMAL(18,2) = 0,
    @TongChuongTrinhTichLuySale DECIMAL(18,2) = 0,
    @TongCong DECIMAL(18,2),
    @DaThanhToan DECIMAL(18,2),
    @ConLai DECIMAL(18,2),
    @TrangThai INT,
    @NguoiTao INT,
    @NewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO BAN_ChungTuBanHang (SoChungTu, NgayChungTu, IDDonDatHang, IDKhachHang, IDKho, IDTaiKhoanThanhToan, 
        TongTienHang, TongTienThue, PhiBocXep, TongTienKhac, TongTienChietKhau, TongChuongTrinhTichLuySale, TongCong, DaThanhToan, ConLai, TrangThai, NgayTao, NguoiTao, IsDeleted)
    VALUES (@SoChungTu, @NgayChungTu, @IDDonDatHang, @IDKhachHang, @IDKho, @IDTaiKhoanThanhToan,
        @TongTienHang, @TongTienThue, @PhiBocXep, @TongTienKhac, @TongTienChietKhau, @TongChuongTrinhTichLuySale, @TongCong, @DaThanhToan, @ConLai, @TrangThai, GETDATE(), @NguoiTao, 0);
        
    SET @NewID = SCOPE_IDENTITY();
END
GO
