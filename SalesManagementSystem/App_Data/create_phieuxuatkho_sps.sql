-- 1. sp_KHO_PhieuXuat_GetList
CREATE OR ALTER PROCEDURE sp_KHO_PhieuXuat_GetList
    @Page INT = 1,
    @PageSize INT = 20,
    @TuNgay NVARCHAR(10) = NULL,
    @DenNgay NVARCHAR(10) = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKho INT = NULL,
    @TrangThai INT = NULL,
    @IDNhanSuNhan INT = NULL,
    @IDSanPham INT = NULL,
    @IDNhaCungCap INT = NULL,
    @TenNguoiGiao NVARCHAR(100) = NULL,
    @IDPhuongTien INT = NULL,
    @TenNguoiNhan NVARCHAR(100) = NULL,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT DISTINCT
        px.ID,
        px.SoChungTu,
        px.NgayXuat,
        px.IDDonDatHang,
        px.IDChungTuBanHang,
        px.IDKho,
        k.TenKhoHang AS TenKhoHang,
        px.IDNhanSuNhan,
        px.TenNguoiNhan,
        px.GhiChu,
        px.TongTienHang,
        px.TongTienThue,
        px.TongCong,
        px.TrangThai,
        
        -- Thông tin Đơn hàng / Khách hàng
        dh.SoDonHang,
        dh.NgayTaoDon AS NgayDonHang,
        dh.TrangThaiDon AS TrangThaiDonHang,
        kh.TenKhachHang
    INTO #TempList
    FROM KHO_PhieuXuat px
    LEFT JOIN BAN_ChungTuBanHang ctbh ON px.IDChungTuBanHang = ctbh.ID AND ctbh.IsDeleted = 0
    LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
    LEFT JOIN NS_DonDatHang dh ON px.IDDonDatHang = dh.ID
    LEFT JOIN NS_KhachHang kh ON dh.IDKhachHang = kh.ID OR ctbh.IDKhachHang = kh.ID
    LEFT JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
    WHERE px.IsDeleted = 0
      AND (@TuNgay IS NULL OR @TuNgay = '' OR px.NgayXuat >= @TuNgay)
      AND (@DenNgay IS NULL OR @DenNgay = '' OR px.NgayXuat <= @DenNgay)
      AND (@SoChungTu IS NULL OR @SoChungTu = '' OR px.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR @IDKho = 0 OR px.IDKho = @IDKho)
      AND (@TrangThai IS NULL OR px.TrangThai = @TrangThai)
      AND (@IDNhanSuNhan IS NULL OR @IDNhanSuNhan = 0 OR px.IDNhanSuNhan = @IDNhanSuNhan)
      AND (@IDSanPham IS NULL OR @IDSanPham = 0 OR ct.IDSanPham = @IDSanPham)
      AND (@IDNhaCungCap IS NULL OR @IDNhaCungCap = 0 OR kh.ID = @IDNhaCungCap OR dh.IDKhachHang = @IDNhaCungCap OR ctbh.IDKhachHang = @IDNhaCungCap)
      AND (@TenNguoiNhan IS NULL OR @TenNguoiNhan = '' OR px.TenNguoiNhan LIKE N'%' + @TenNguoiNhan + '%');

    -- Lấy tổng số dòng
    SELECT @TotalRecords = COUNT(*) FROM #TempList;

    -- Lấy dữ liệu phân trang
    SELECT * 
    FROM #TempList
    ORDER BY NgayXuat DESC, ID DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    DROP TABLE #TempList;
END
GO

-- 2. sp_KHO_PhieuXuat_GetById
CREATE OR ALTER PROCEDURE sp_KHO_PhieuXuat_GetById
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        px.*,
        dh.SoDonHang,
        kh.TenKhachHang,
        k.TenKhoHang AS TenKhoHang
    FROM KHO_PhieuXuat px
    LEFT JOIN NS_DonDatHang dh ON px.IDDonDatHang = dh.ID
    LEFT JOIN NS_KhachHang kh ON dh.IDKhachHang = kh.ID
    LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
    WHERE px.ID = @ID AND px.IsDeleted = 0;
END
GO

-- 3. sp_KHO_PhieuXuat_ChiTiet_GetList
CREATE OR ALTER PROCEDURE sp_KHO_PhieuXuat_ChiTiet_GetList
    @IDPhieuXuat INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        ct.*,
        sp.MaSanPham,
        sp.TenSanPham,
        sp.DVT
    FROM KHO_PhieuXuat_ChiTiet ct
    LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
    WHERE ct.IDPhieuXuat = @IDPhieuXuat;
END
GO

-- 4. sp_KHO_PhieuXuat_Insert
CREATE OR ALTER PROCEDURE sp_KHO_PhieuXuat_Insert
    @SoChungTu NVARCHAR(50),
    @NgayXuat DATE,
    @IDDonDatHang INT = NULL,
    @IDKho INT,
    @IDNhanSuNhan INT = NULL,
    @TenNguoiNhan NVARCHAR(100) = NULL,
    @SoDienThoaiNguoiNhan NVARCHAR(20) = NULL,
    @GhiChu NVARCHAR(250) = NULL,
    @TongTienHang DECIMAL(18,2) = 0,
    @TongTienThue DECIMAL(18,2) = 0,
    @TongCong DECIMAL(18,2) = 0,
    @NguoiTao INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO KHO_PhieuXuat (
        SoChungTu, NgayXuat, IDDonDatHang, IDKho, IDNhanSuNhan, TenNguoiNhan, SoDienThoaiNguoiNhan,
        GhiChu, TongTienHang, TongTienThue, TongCong, TrangThai, NgayTao, NguoiTao, IsDeleted
    )
    VALUES (
        @SoChungTu, @NgayXuat, @IDDonDatHang, @IDKho, @IDNhanSuNhan, @TenNguoiNhan, @SoDienThoaiNguoiNhan, 
        @GhiChu, @TongTienHang, @TongTienThue, @TongCong, 1, GETDATE(), @NguoiTao, 0
    );

    SELECT SCOPE_IDENTITY() AS NewID;
END
GO

-- 5. sp_KHO_PhieuXuat_ChiTiet_Insert
CREATE OR ALTER PROCEDURE sp_KHO_PhieuXuat_ChiTiet_Insert
    @IDPhieuXuat INT,
    @IDSanPham INT,
    @SoLuong DECIMAL(18,2),
    @DonGia DECIMAL(18,2),
    @ThanhTien DECIMAL(18,2) = 0,
    @ThueGTGT DECIMAL(18,2) = 0,
    @TienThue DECIMAL(18,2) = 0,
    @TongSauThue DECIMAL(18,2) = 0,
    @GhiChu NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO KHO_PhieuXuat_ChiTiet (
        IDPhieuXuat, IDSanPham, SoLuong, DonGia, ThanhTien, ThueGTGT, TienThue, TongSauThue, GhiChu
    )
    VALUES (
        @IDPhieuXuat, @IDSanPham, @SoLuong, @DonGia, @ThanhTien, @ThueGTGT, @TienThue, @TongSauThue, @GhiChu
    );
END
GO

-- 6. sp_KHO_PhieuXuat_UpdateStatus
CREATE OR ALTER PROCEDURE sp_KHO_PhieuXuat_UpdateStatus
    @ID INT,
    @TrangThai INT,
    @NguoiGhi INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE KHO_PhieuXuat
    SET TrangThai = @TrangThai,
        NgayGhi = CASE WHEN @TrangThai = 2 THEN GETDATE() ELSE NgayGhi END,
        NguoiGhi = CASE WHEN @TrangThai = 2 THEN @NguoiGhi ELSE NguoiGhi END
    WHERE ID = @ID;
END
GO

-- 7. sp_KHO_PhieuXuat_Cancel
CREATE OR ALTER PROCEDURE sp_KHO_PhieuXuat_Cancel
    @ID INT,
    @NguoiHuy INT,
    @LyDoHuy NVARCHAR(250)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE KHO_PhieuXuat
    SET TrangThai = 3,
        NgayHuy = GETDATE(),
        NguoiHuy = @NguoiHuy,
        LyDoHuy = @LyDoHuy
    WHERE ID = @ID;
END
GO
