-- =========================================
-- sp_KHO_PhieuNhap_GetList
-- =========================================
CREATE   PROCEDURE dbo.sp_KHO_PhieuNhap_GetList
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKho INT = NULL,
    @IDNhaCungCap INT = NULL,
    @TrangThai INT = NULL,
    @TenNguoiNhan NVARCHAR(200) = NULL,
    @Offset INT = 0,
    @PageSize INT = 20,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Láº¥y tá»•ng sá»‘ dÃ²ng
    SELECT @TotalRecords = COUNT(*)
    FROM [dbo].[KHO_PhieuNhap] p
    WHERE p.IsDeleted = 0
      AND (@TuNgay IS NULL OR p.NgayNhap >= @TuNgay)
      AND (@DenNgay IS NULL OR p.NgayNhap <= @DenNgay)
      AND (@SoChungTu IS NULL OR p.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR p.IDKho = @IDKho)
      AND (@IDNhaCungCap IS NULL OR p.IDNhaCungCap = @IDNhaCungCap)
      AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
      AND (LEN(ISNULL(@TenNguoiNhan,'')) = 0 OR p.TenNguoiNhan = @TenNguoiNhan)

    -- Trá»Ÿ vá» danh sÃ¡ch
    SELECT 
        p.ID,
        p.SoChungTu,
        p.NgayNhap,
        p.IDKho,
        k.TenKhoHang AS TenKho,
        k.MaKhoHang AS MaKhoHang,
        p.IDNhaCungCap,
        ncc.TenNhaCungCap AS TenNhaCungCap,
        ncc.MaNhaCungCap AS MaNhaCungCap,
        p.SoHoaDon,
        p.NgayHoaDon,
        p.TenNguoiGiao,
        p.SoDienThoaiNguoiGiao,
        p.IDNhanSuNhan,
        ns.Ten AS TenNhanSuNhan,
        p.TrangThai,
        p.TongTienHang,
        p.TongTienThue,
        p.TongCong,
        p.NgayTao,
        p.NguoiTao,
        NguoiTaoText = nsTao.HoDem + ' ' + ns.Ten,
        p.TrangThaiThanhToan,
        p.DaThanhToan,
        p.ConLai,
        p.IDPhuongTien,
        pt.TenPhuongTien AS TenPhuongTien
		
    FROM [dbo].[KHO_PhieuNhap] p
    LEFT JOIN [dbo].[DM_KhoHang] k ON p.IDKho = k.ID
    LEFT JOIN [dbo].[DM_NhaCungCap] ncc ON p.IDNhaCungCap = ncc.ID
    LEFT JOIN [dbo].[NS_NhanSu] ns ON p.IDNhanSuNhan = ns.ID
    LEFT JOIN [dbo].[ACL_Login] u ON p.NguoiTao = u.ID
    LEFT JOIN [dbo].[NS_NhanSu] nsTao ON u.IDNhanSu = nsTao.ID
    LEFT JOIN [dbo].[DM_PhuongTien] pt ON p.IDPhuongTien = pt.ID
    WHERE p.IsDeleted = 0
      AND (@TuNgay IS NULL OR p.NgayNhap >= @TuNgay)
      AND (@DenNgay IS NULL OR p.NgayNhap <= @DenNgay)
      AND (@SoChungTu IS NULL OR p.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR p.IDKho = @IDKho)
      AND (@IDNhaCungCap IS NULL OR p.IDNhaCungCap = @IDNhaCungCap)
      AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
      AND (LEN(ISNULL(@TenNguoiNhan,'')) = 0 OR p.TenNguoiNhan = @TenNguoiNhan)
    ORDER BY p.NgayNhap DESC, p.ID DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- =========================================
-- sp_KHO_PhieuNhap_GetByID
-- =========================================
CREATE PROCEDURE [sp_KHO_PhieuNhap_GetByID]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM KHO_PhieuNhap WHERE ID = @ID AND IsDeleted = 0;
END

GO

-- =========================================
-- sp_KHO_PhieuNhap_Save
-- =========================================
-- 2. Cập nhật sp_KHO_PhieuNhap_Save
CREATE   PROCEDURE [dbo].[sp_KHO_PhieuNhap_Save]
    @ID INT,
    @NgayNhap DATETIME,
    @IDKho INT,
    @IDNhaCungCap INT,
    @SoHoaDon NVARCHAR(50),
    @NgayHoaDon DATETIME,
    @TenNguoiGiao NVARCHAR(150),
    @SoDienThoaiNguoiGiao NVARCHAR(50),
    @TenNguoiNhan NVARCHAR(150),
    @GhiChu NVARCHAR(500),
    @NguoiTao INT,
    @IDPhuongTien INT = NULL,
    @NgayGiaoHang DATETIME = NULL,
    @HoTenTaiXe NVARCHAR(200) = NULL,
    @SoDienThoaiTaiXe NVARCHAR(50) = NULL,
    @ChiTietJson NVARCHAR(MAX),
    @NewID INT OUTPUT,
    @SoChungTuOut NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Xử lý lưu thông tin Master (bảng KHO_PhieuNhap)
    IF ISNULL(@ID, 0) = 0
    BEGIN
        -- Thêm mới
        -- Tạo số chứng từ tự động (ví dụ: PN26000001)
        DECLARE @Prefix NVARCHAR(10) = 'PN' + RIGHT(CAST(YEAR(GETDATE()) AS NVARCHAR(4)), 2)
        DECLARE @MaxSeq INT = ISNULL((SELECT MAX(CAST(RIGHT(SoChungTu, 6) AS INT)) FROM KHO_PhieuNhap WHERE SoChungTu LIKE @Prefix + '%' AND LEN(SoChungTu) = 10), 0)
        SET @SoChungTuOut = @Prefix + RIGHT('000000' + CAST(@MaxSeq + 1 AS NVARCHAR), 6)

        INSERT INTO [dbo].[KHO_PhieuNhap] (
            [SoChungTu], [NgayNhap], [IDKho], [IDNhaCungCap], 
            [SoHoaDon], [NgayHoaDon], [TenNguoiGiao], [SoDienThoaiNguoiGiao], 
            [TenNguoiNhan], [GhiChu], [TrangThai], 
            [NgayTao], [NguoiTao], [IsDeleted],
            [IDPhuongTien], [NgayGiaoHang], [HoTenTaiXe], [SoDienThoaiTaiXe]
        )
        VALUES (
            @SoChungTuOut, @NgayNhap, @IDKho, @IDNhaCungCap, 
            @SoHoaDon, @NgayHoaDon, @TenNguoiGiao, @SoDienThoaiNguoiGiao, 
            @TenNguoiNhan, @GhiChu, 1, -- 1: Nhập
            GETDATE(), @NguoiTao, 0,
            @IDPhuongTien, @NgayGiaoHang, @HoTenTaiXe, @SoDienThoaiTaiXe
        )
        
        SET @NewID = SCOPE_IDENTITY()
    END
    ELSE
    BEGIN
        -- Cập nhật
        SET @NewID = @ID
        SELECT @SoChungTuOut = SoChungTu FROM [dbo].[KHO_PhieuNhap] WHERE ID = @ID

        UPDATE [dbo].[KHO_PhieuNhap]
        SET [NgayNhap] = @NgayNhap,
            [IDKho] = @IDKho,
            [IDNhaCungCap] = @IDNhaCungCap,
            [SoHoaDon] = @SoHoaDon,
            [NgayHoaDon] = @NgayHoaDon,
            [TenNguoiGiao] = @TenNguoiGiao,
            [SoDienThoaiNguoiGiao] = @SoDienThoaiNguoiGiao,
            [TenNguoiNhan] = @TenNguoiNhan,
            [GhiChu] = @GhiChu,
            [NgayCapNhat] = GETDATE(),
            [NguoiCapNhat] = @NguoiTao,
            [IDPhuongTien] = @IDPhuongTien,
            [NgayGiaoHang] = @NgayGiaoHang,
            [HoTenTaiXe] = @HoTenTaiXe,
            [SoDienThoaiTaiXe] = @SoDienThoaiTaiXe
        WHERE ID = @ID
    END

    -- 2. Xử lý chi tiết (bảng KHO_PhieuNhap_ChiTiet) bằng JSON
    DELETE FROM [dbo].[KHO_PhieuNhap_ChiTiet] WHERE IDPhieuNhap = @NewID
    
    INSERT INTO [dbo].[KHO_PhieuNhap_ChiTiet] (
        [IDPhieuNhap], [IDSanPham], [STT], [SoLuong], [DonGia], 
        [ThanhTien], [ThueGTGT], [TienThue], [TongSauThue], 
        [NgaySanXuat], [HanSuDung], [GhiChu]
    )
    SELECT 
        @NewID,
        JSON_VALUE(value, '$.IDSanPham'),
        ROW_NUMBER() OVER(ORDER BY (SELECT NULL)),
        CAST(JSON_VALUE(value, '$.SoLuong') AS FLOAT),
        CAST(JSON_VALUE(value, '$.DonGia') AS FLOAT),
        CAST(JSON_VALUE(value, '$.ThanhTien') AS FLOAT),
        CAST(JSON_VALUE(value, '$.ThueGTGT') AS FLOAT),
        CAST(JSON_VALUE(value, '$.TienThue') AS FLOAT),
        CAST(JSON_VALUE(value, '$.TongSauThue') AS FLOAT),
        CASE WHEN JSON_VALUE(value, '$.NgaySanXuat') = '' THEN NULL ELSE JSON_VALUE(value, '$.NgaySanXuat') END,
        CASE WHEN JSON_VALUE(value, '$.HanSuDung') = '' THEN NULL ELSE JSON_VALUE(value, '$.HanSuDung') END,
        JSON_VALUE(value, '$.GhiChu')
    FROM OPENJSON(@ChiTietJson)

END
GO

-- =========================================
-- sp_KHO_PhieuNhap_GhiSo
-- =========================================
CREATE   PROCEDURE [sp_KHO_PhieuNhap_GhiSo]
    @ID INT,
    @NguoiGhiSo INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (SELECT 1 FROM KHO_PhieuNhap WHERE ID = @ID AND TrangThai = 1 AND IsDeleted = 0)
        BEGIN
            IF EXISTS (SELECT 1 FROM KHO_GiaoDichKho WHERE LoaiChungTu = 1 AND SoChungTu = (SELECT SoChungTu FROM KHO_PhieuNhap WHERE ID = @ID))
            BEGIN
                THROW 50001, N'Đã tồn tại giao dịch kho cho phiếu nhập này.', 1;
            END

            UPDATE KHO_PhieuNhap 
            SET TrangThai = 2, NgayGhiSo = GETDATE(), NguoiGhiSo = @NguoiGhiSo 
            WHERE ID = @ID;

            -- Cập nhật trạng thái thanh toán và số tiền còn lại của Phiếu Nhập Kho
            EXEC sp_KHO_CapNhatTrangThaiThanhToanPhieuNhap @IDPhieuNhap = @ID;

            INSERT INTO KHO_GiaoDichKho (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao)
            SELECT 
                p.NgayNhap,
                p.SoChungTu,
                1,
                ct.ID,
                p.IDKho,
                ct.IDSanPham,
                ct.SoLuong,
                0,
                ct.DonGia,
                ct.ThanhTien,
                p.GhiChu,
                GETDATE(),
                @NguoiGhiSo
            FROM KHO_PhieuNhap_ChiTiet ct
            INNER JOIN KHO_PhieuNhap p ON ct.IDPhieuNhap = p.ID
            WHERE p.ID = @ID;
        END
        ELSE
        BEGIN
            THROW 50002, N'Phiếu không hợp lệ hoặc không ở trạng thái lưu nháp.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO

-- =========================================
-- sp_KHO_PhieuNhap_Huy
-- =========================================
-- ==========================================
-- 1. STORED PROCEDURE: sp_KHO_PhieuNhap_Huy
-- ==========================================
CREATE   PROCEDURE [dbo].[sp_KHO_PhieuNhap_Huy]
    @ID INT,
    @LyDoHuy NVARCHAR(MAX),
    @NguoiHuy INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @TrangThai INT, @SoChungTu NVARCHAR(50);
        SELECT @TrangThai = TrangThai, @SoChungTu = SoChungTu
        FROM KHO_PhieuNhap WHERE ID = @ID AND IsDeleted = 0;

        IF @TrangThai IS NULL
        BEGIN
            THROW 50003, N'Không tìm thấy phiếu nhập kho.', 1;
        END

        IF @TrangThai = 1
        BEGIN
            UPDATE KHO_PhieuNhap 
            SET TrangThai = 3, NgayHuy = GETDATE(), NguoiHuy = @NguoiHuy, LyDoHuy = @LyDoHuy 
            WHERE ID = @ID;
        END
        ELSE IF @TrangThai = 2
        BEGIN
            -- KHÔNG sinh dòng mới, chỉ cập nhật trực tiếp các dòng KHO_GiaoDichKho của phiếu đó
            UPDATE KHO_GiaoDichKho
            SET IsHuy = 1,
                NgayHuy = GETDATE(),
                NguoiHuy = @NguoiHuy,
                LyDoHuy = @LyDoHuy
            WHERE SoChungTu = @SoChungTu AND LoaiChungTu = 1; -- 1: Phiếu nhập

            UPDATE KHO_PhieuNhap 
            SET TrangThai = 3, NgayHuy = GETDATE(), NguoiHuy = @NguoiHuy, LyDoHuy = @LyDoHuy 
            WHERE ID = @ID;
        END
        ELSE
        BEGIN
            THROW 50003, N'Phiếu không ở trạng thái hợp lệ để hủy.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

GO

-- =========================================
-- sp_KHO_TonKho_GetByKhoSanPham
-- =========================================

GO

-- =========================================
-- sp_KHO_TonKho_CheckChuyenKho
-- =========================================

GO

