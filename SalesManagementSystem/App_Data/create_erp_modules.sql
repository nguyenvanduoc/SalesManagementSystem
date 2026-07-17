-- =============================================
-- Author:      Antigravity
-- Create date: 2026-06-12
-- Description: Khởi tạo các Stored Procedures cho phân hệ Chứng Từ Bán Hàng & Nhật Ký Kế Toán
-- =============================================

-- =============================================
-- BAN_ChungTuBanHang
-- =============================================
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
      AND (@SoChungTu IS NULL OR b.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKhachHang IS NULL OR b.IDKhachHang = @IDKhachHang)
      AND (@IDKho IS NULL OR b.IDKho = @IDKho)
      AND (@TrangThai IS NULL OR b.TrangThai = @TrangThai)
    ORDER BY b.NgayChungTu DESC, b.ID DESC
END
GO

CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_GetById
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT c.*, d.SoDienThoaiTaiXe, d.HoTenTaiXe 
    FROM BAN_ChungTuBanHang c
    LEFT JOIN NS_DonDatHang d ON c.IDDonDatHang = d.ID
    WHERE c.ID = @ID AND c.IsDeleted = 0;
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
        TongTienHang, TongTienThue, TongCong, DaThanhToan, ConLai, TrangThai, NgayTao, NguoiTao, IsDeleted)
    VALUES (@SoChungTu, @NgayChungTu, @IDDonDatHang, @IDKhachHang, @IDKho, @IDTaiKhoanThanhToan,
        @TongTienHang, @TongTienThue, @TongCong, @DaThanhToan, @ConLai, @TrangThai, GETDATE(), @NguoiTao, 0);
        
    SET @NewID = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_UpdateStatus
    @ID INT,
    @TrangThai INT,
    @NguoiCapNhat INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @TrangThai = 2 -- Đã ghi
    BEGIN
        UPDATE BAN_ChungTuBanHang 
        SET TrangThai = @TrangThai, NgayGhi = GETDATE(), NguoiGhi = @NguoiCapNhat, NgayCapNhat = GETDATE(), NguoiCapNhat = @NguoiCapNhat
        WHERE ID = @ID AND IsDeleted = 0;
    END
    ELSE
    BEGIN
        UPDATE BAN_ChungTuBanHang 
        SET TrangThai = @TrangThai, NgayCapNhat = GETDATE(), NguoiCapNhat = @NguoiCapNhat
        WHERE ID = @ID AND IsDeleted = 0;
    END
END
GO

CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_Cancel
    @ID INT,
    @NguoiHuy INT,
    @LyDoHuy NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE BAN_ChungTuBanHang 
    SET TrangThai = 3, NgayHuy = GETDATE(), NguoiHuy = @NguoiHuy, LyDoHuy = @LyDoHuy
    WHERE ID = @ID AND IsDeleted = 0;
END
GO

-- =============================================
-- BAN_ChungTuBanHang_ChiTiet
-- =============================================
CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_ChiTiet_GetList
    @IDChungTuBanHang INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.ID,
        c.IDChungTuBanHang,
        c.IDSanPham,
        s.MaSanPham,
        s.TenSanPham,
        s.DVT,
        c.STT,
        c.SoLuong,
        c.DonGia,
        c.ThanhTien,
        c.ThueGTGT,
        c.TienThue,
        c.TongSauThue,
        c.GhiChu
    FROM BAN_ChungTuBanHang_ChiTiet c
    JOIN DM_SanPham s ON c.IDSanPham = s.ID
    WHERE c.IDChungTuBanHang = @IDChungTuBanHang
    ORDER BY c.STT;
END
GO

CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_ChiTiet_Insert
    @IDChungTuBanHang INT,
    @IDSanPham INT,
    @STT INT,
    @SoLuong DECIMAL(18,2),
    @DonGia DECIMAL(18,2),
    @ThanhTien DECIMAL(18,2),
    @ThueGTGT DECIMAL(18,2),
    @TienThue DECIMAL(18,2),
    @TongSauThue DECIMAL(18,2),
    @GhiChu NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO BAN_ChungTuBanHang_ChiTiet (IDChungTuBanHang, IDSanPham, STT, SoLuong, DonGia, ThanhTien, ThueGTGT, TienThue, TongSauThue, GhiChu)
    VALUES (@IDChungTuBanHang, @IDSanPham, @STT, @SoLuong, @DonGia, @ThanhTien, @ThueGTGT, @TienThue, @TongSauThue, @GhiChu);
END
GO

-- =============================================
-- KT_TaiKhoanKeToan
-- =============================================
CREATE OR ALTER PROCEDURE sp_KT_TaiKhoanKeToan_GetActive
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT ID, SoTaiKhoan, TenTaiKhoan 
    FROM KT_TaiKhoanKeToan 
    WHERE IsChiTiet = 1
    ORDER BY SoTaiKhoan;
END
GO

-- =============================================
-- KT_NhatKyChung
-- =============================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KT_NhatKyChung' AND COLUMN_NAME = 'IsHuy')
BEGIN
    ALTER TABLE KT_NhatKyChung ADD IsHuy BIT NOT NULL DEFAULT 0;
END
GO

CREATE OR ALTER PROCEDURE sp_KT_NhatKyChung_GetList
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @TaiKhoanNo NVARCHAR(20) = NULL,
    @TaiKhoanCo NVARCHAR(20) = NULL,
    @LoaiChungTu NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ID,
        NgayChungTu,
        SoChungTu,
        LoaiChungTu,
        IDChungTu,
        TaiKhoanNo,
        TaiKhoanCo,
        SoTien,
        DienGiai,
        NgayTao,
        NguoiTao
    FROM KT_NhatKyChung
    WHERE 1=1
      AND (@TuNgay IS NULL OR CAST(NgayChungTu AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay IS NULL OR CAST(NgayChungTu AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@SoChungTu IS NULL OR SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@TaiKhoanNo IS NULL OR TaiKhoanNo LIKE '%' + @TaiKhoanNo + '%')
      AND (@TaiKhoanCo IS NULL OR TaiKhoanCo LIKE '%' + @TaiKhoanCo + '%')
      AND (@LoaiChungTu IS NULL OR LoaiChungTu = @LoaiChungTu)
    ORDER BY NgayChungTu DESC, ID DESC
END
GO

CREATE OR ALTER PROCEDURE sp_KT_NhatKyChung_Insert
    @NgayChungTu DATE,
    @SoChungTu NVARCHAR(50),
    @LoaiChungTu NVARCHAR(50),
    @IDChungTu INT,
    @TaiKhoanNo NVARCHAR(20),
    @TaiKhoanCo NVARCHAR(20),
    @SoTien DECIMAL(18,2),
    @DienGiai NVARCHAR(1000) = NULL,
    @NguoiTao INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check for exact duplicate to prevent double entry
    IF NOT EXISTS (SELECT 1 FROM KT_NhatKyChung 
                   WHERE LoaiChungTu = @LoaiChungTu 
                     AND IDChungTu = @IDChungTu 
                     AND TaiKhoanNo = @TaiKhoanNo 
                     AND TaiKhoanCo = @TaiKhoanCo
                     AND SoTien = @SoTien)
    BEGIN
        INSERT INTO KT_NhatKyChung (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao)
        VALUES (@NgayChungTu, @SoChungTu, @LoaiChungTu, @IDChungTu, @TaiKhoanNo, @TaiKhoanCo, @SoTien, @DienGiai, GETDATE(), @NguoiTao);
    END
END
GO

CREATE OR ALTER PROCEDURE sp_KT_NhatKyChung_Cancel
    @LoaiChungTu NVARCHAR(50),
    @IDChungTu INT,
    @NguoiHuy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Reverse the journal entry by creating a negative entry or swapping No/Co.
    -- The requirement says "Đánh dấu IsHuy = 1 trong KT_NhatKyChung". 
-- If IsHuy column doesn't exist, we will alter the table first.
    UPDATE KT_NhatKyChung
    SET IsHuy = 1
    WHERE LoaiChungTu = @LoaiChungTu AND IDChungTu = @IDChungTu;
END
GO
