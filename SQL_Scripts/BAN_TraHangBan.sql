USE [SalesWarehouseDB]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BAN_TraHangBan]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[BAN_TraHangBan](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SoChungTu] [nvarchar](50) NULL,
	[NgayChungTu] [datetime] NULL,
	[IDDonDatHang] [int] NULL,
	[IDKhachHang] [int] NULL,
	[IDKho] [int] NULL,
	[LyDoTraHang] [nvarchar](500) NULL,
	[TongSoLuong] [decimal](18, 2) NULL,
	[TongTienHang] [decimal](18, 2) NULL,
	[TongTienDaHoan] [decimal](18, 2) NULL,
	[ConPhaiHoan] [decimal](18, 2) NULL,
	[TrangThai] [int] NULL,
	[NgayTao] [datetime] NULL,
	[NguoiTao] [int] NULL,
	[NgayCapNhat] [datetime] NULL,
	[NguoiCapNhat] [int] NULL,
 CONSTRAINT [PK_BAN_TraHangBan] PRIMARY KEY CLUSTERED ([ID] ASC)
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BAN_TraHangBanChiTiet]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[BAN_TraHangBanChiTiet](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IDTraHang] [int] NULL,
	[IDSanPham] [int] NULL,
	[SoLuongBan] [decimal](18, 2) NULL,
	[SoLuongDaTra] [decimal](18, 2) NULL,
	[SoLuongConLai] [decimal](18, 2) NULL,
	[SoLuongTra] [decimal](18, 2) NULL,
	[DonGia] [decimal](18, 2) NULL,
	[ThanhTien] [decimal](18, 2) NULL,
	[GhiChu] [nvarchar](500) NULL,
	[NgayTao] [datetime] NULL,
	[NguoiTao] [int] NULL,
	[NgayCapNhat] [datetime] NULL,
	[NguoiCapNhat] [int] NULL,
 CONSTRAINT [PK_BAN_TraHangBanChiTiet] PRIMARY KEY CLUSTERED ([ID] ASC)
)
END
GO

-- 1. sp_BAN_TraHangBan_GetList
CREATE OR ALTER PROCEDURE [dbo].[sp_BAN_TraHangBan_GetList]
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKhachHang INT = NULL,
    @TrangThai INT = NULL,
    @Page INT = 1,
    @PageSize INT = 10,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    
    SELECT @TotalRecords = COUNT(*)
    FROM BAN_TraHangBan t
    WHERE (@TuNgay IS NULL OR t.NgayChungTu >= @TuNgay)
      AND (@DenNgay IS NULL OR t.NgayChungTu <= @DenNgay)
      AND (@SoChungTu IS NULL OR t.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKhachHang IS NULL OR t.IDKhachHang = @IDKhachHang)
      AND (@TrangThai IS NULL OR t.TrangThai = @TrangThai);
      
    SELECT t.*, k.TenKhachHang, k.MaKhachHang, u.Ten as NguoiTaoName, d.SoDonHang
    FROM BAN_TraHangBan t
    LEFT JOIN NS_KhachHang k ON t.IDKhachHang = k.ID
    LEFT JOIN NS_NhanSu u ON t.NguoiTao = u.ID
    LEFT JOIN NS_DonDatHang d ON t.IDDonDatHang = d.ID
    WHERE (@TuNgay IS NULL OR t.NgayChungTu >= @TuNgay)
      AND (@DenNgay IS NULL OR t.NgayChungTu <= @DenNgay)
      AND (@SoChungTu IS NULL OR t.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKhachHang IS NULL OR t.IDKhachHang = @IDKhachHang)
      AND (@TrangThai IS NULL OR t.TrangThai = @TrangThai)
    ORDER BY t.NgayChungTu DESC, t.ID DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- 2. sp_BAN_TraHangBan_GetById
CREATE OR ALTER PROCEDURE [dbo].[sp_BAN_TraHangBan_GetById]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT t.*, k.TenKhachHang, k.MaKhachHang, d.SoDonHang, kh.TenKhoHang as TenKho,
           ISNULL((SELECT SUM(pt.SoTienThu) 
                   FROM BAN_PhieuThuKhachHang pt 
                   JOIN BAN_ChungTuBanHang ct ON pt.IDChungTuBanHang = ct.ID 
                   WHERE ct.IDDonDatHang = t.IDDonDatHang AND pt.TrangThai = 2), 0) as DaThanhToan,
           ISNULL((SELECT SUM(TongTien) FROM NS_DonDatHang WHERE ID = t.IDDonDatHang), 0) as TongTienDonHang
    FROM BAN_TraHangBan t
    LEFT JOIN NS_KhachHang k ON t.IDKhachHang = k.ID
    LEFT JOIN NS_DonDatHang d ON t.IDDonDatHang = d.ID
    LEFT JOIN DM_KhoHang kh ON t.IDKho = kh.ID
    WHERE t.ID = @ID;
    
    SELECT c.*, s.TenSanPham, s.MaSanPham, s.DVT as DonViTinh
    FROM BAN_TraHangBanChiTiet c
    LEFT JOIN DM_SanPham s ON c.IDSanPham = s.ID
    WHERE c.IDTraHang = @ID;
END
GO

-- 3. sp_BAN_TraHangBan_LoadDonHangTra
CREATE OR ALTER PROCEDURE [dbo].[sp_BAN_TraHangBan_LoadDonHangTra]
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @SoDonHang NVARCHAR(50) = NULL,
    @Page INT = 1,
    @PageSize INT = 10,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    
    SELECT @TotalRecords = COUNT(*)
    FROM NS_DonDatHang d
    WHERE ISNULL(d.TrangThaiDon, 0) NOT IN (0, 4)
      AND (@TuNgay IS NULL OR d.NgayTaoDon >= @TuNgay)
      AND (@DenNgay IS NULL OR d.NgayTaoDon <= @DenNgay)
      AND (@SoDonHang IS NULL OR d.SoDonHang LIKE '%' + @SoDonHang + '%');
    
    SELECT d.ID, d.SoDonHang, d.NgayTaoDon as NgayTao, d.IDKhachHang, k.TenKhachHang, k.MaKhachHang,
           d.TongTien as TongTien, 
           ISNULL((SELECT SUM(pt.SoTienThu) 
                   FROM BAN_PhieuThuKhachHang pt 
                   JOIN BAN_ChungTuBanHang ct ON pt.IDChungTuBanHang = ct.ID 
                   WHERE ct.IDDonDatHang = d.ID AND pt.TrangThai = 2), 0) as DaThanhToan, 
           d.TrangThaiDon as TrangThai,
           ISNULL((SELECT SUM(SoLuongTra) FROM BAN_TraHangBan t JOIN BAN_TraHangBanChiTiet tc ON t.ID = tc.IDTraHang WHERE t.IDDonDatHang = d.ID AND t.TrangThai = 2), 0) as DaTraHang,
           ISNULL((SELECT SUM(SoLuong) FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = d.ID), 0) as TongSoLuong,
           (SELECT TOP 1 IDKho FROM BAN_ChungTuBanHang WHERE IDDonDatHang = d.ID ORDER BY ID DESC) as IDKho
    FROM NS_DonDatHang d
    LEFT JOIN NS_KhachHang k ON d.IDKhachHang = k.ID
    WHERE ISNULL(d.TrangThaiDon, 0) NOT IN (0, 4) -- Bo qua luu nhap (0) va da huy (4)
      AND (@TuNgay IS NULL OR d.NgayTaoDon >= @TuNgay)
      AND (@DenNgay IS NULL OR d.NgayTaoDon <= @DenNgay)
      AND (@SoDonHang IS NULL OR d.SoDonHang LIKE '%' + @SoDonHang + '%')
    ORDER BY d.NgayTaoDon DESC, d.ID DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- 4. sp_BAN_TraHangBan_LoadChiTietDonHang
CREATE OR ALTER PROCEDURE [dbo].[sp_BAN_TraHangBan_LoadChiTietDonHang]
    @IDDonDatHang INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.IDSanPham,
        s.MaSanPham,
        s.TenSanPham,
        s.DVT as DonViTinh,
        c.SoLuong as SoLuongBan,
        c.DonGia as DonGia,
        ISNULL((
            SELECT SUM(tc.SoLuongTra) 
            FROM BAN_TraHangBanChiTiet tc 
            JOIN BAN_TraHangBan t ON tc.IDTraHang = t.ID 
            WHERE t.IDDonDatHang = @IDDonDatHang AND tc.IDSanPham = c.IDSanPham AND t.TrangThai IN (1, 2)
        ), 0) as SoLuongDaTra,
        c.SoLuong - ISNULL((
            SELECT SUM(tc.SoLuongTra) 
            FROM BAN_TraHangBanChiTiet tc 
            JOIN BAN_TraHangBan t ON tc.IDTraHang = t.ID 
            WHERE t.IDDonDatHang = @IDDonDatHang AND tc.IDSanPham = c.IDSanPham AND t.TrangThai IN (1, 2)
        ), 0) as SoLuongConLai
    FROM NS_DonDatHangChiTiet c
    LEFT JOIN DM_SanPham s ON c.IDSanPham = s.ID
    WHERE c.IDDonDatHang = @IDDonDatHang
END
GO

-- 5. sp_BAN_TraHangBan_GhiSo
CREATE OR ALTER PROCEDURE [dbo].[sp_BAN_TraHangBan_GhiSo]
    @ID INT,
    @NguoiThucHien INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        UPDATE BAN_TraHangBan 
        SET TrangThai = 2, NgayCapNhat = GETDATE(), NguoiCapNhat = @NguoiThucHien
        WHERE ID = @ID;
        
        DECLARE @SoChungTu NVARCHAR(50), @NgayChungTu DATETIME, @IDKho INT, @LyDo NVARCHAR(500);
        SELECT @SoChungTu = SoChungTu, @NgayChungTu = NgayChungTu, @IDKho = IDKho, @LyDo = LyDoTraHang 
        FROM BAN_TraHangBan WHERE ID = @ID;

        INSERT INTO KHO_GiaoDichKho (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao)
        SELECT @NgayChungTu, @SoChungTu, 5, 0, @IDKho, c.IDSanPham, c.SoLuongTra, 0, c.DonGia, c.ThanhTien, @LyDo, GETDATE(), @NguoiThucHien
        FROM BAN_TraHangBanChiTiet c
        WHERE c.IDTraHang = @ID;

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- 6. sp_BAN_TraHangBan_Huy
CREATE OR ALTER PROCEDURE [dbo].[sp_BAN_TraHangBan_Huy]
    @ID INT,
    @NguoiThucHien INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DECLARE @TrangThaiHienTai INT;
        SELECT @TrangThaiHienTai = TrangThai FROM BAN_TraHangBan WHERE ID = @ID;
        
        IF @TrangThaiHienTai = 2
        BEGIN
            DECLARE @SoChungTu NVARCHAR(50), @NgayChungTu DATETIME, @IDKho INT, @LyDo NVARCHAR(500);
            SELECT @SoChungTu = SoChungTu, @NgayChungTu = GETDATE(), @IDKho = IDKho, @LyDo = N'Hủy ' + LyDoTraHang 
            FROM BAN_TraHangBan WHERE ID = @ID;

            INSERT INTO KHO_GiaoDichKho (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao)
            SELECT @NgayChungTu, @SoChungTu, 6, 0, @IDKho, c.IDSanPham, 0, c.SoLuongTra, c.DonGia, c.ThanhTien, @LyDo, GETDATE(), @NguoiThucHien
            FROM BAN_TraHangBanChiTiet c
            WHERE c.IDTraHang = @ID;
        END

        UPDATE BAN_TraHangBan 
        SET TrangThai = 3, NgayCapNhat = GETDATE(), NguoiCapNhat = @NguoiThucHien
        WHERE ID = @ID;

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO
