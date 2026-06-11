-- =============================================
-- Author:      System
-- Create date: 
-- Description: Store Procedures for KHO_PhieuXuat
-- =============================================

-- 1. GetList
CREATE OR ALTER PROCEDURE [dbo].[sp_KHO_PhieuXuat_GetList]
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKho INT = NULL,
    @TrangThai INT = NULL,
    @IDNhanSuNhan INT = NULL,
    @Offset INT = 0,
    @PageSize INT = 20,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @TotalRecords = COUNT(*)
    FROM [dbo].[KHO_PhieuXuat] p
    WHERE p.IsDeleted = 0
      AND (@TuNgay IS NULL OR p.NgayXuat >= @TuNgay)
      AND (@DenNgay IS NULL OR p.NgayXuat <= @DenNgay)
      AND (@SoChungTu IS NULL OR p.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR p.IDKho = @IDKho)
      AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
      AND (@IDNhanSuNhan IS NULL OR p.IDNhanSuNhan = @IDNhanSuNhan);

    SELECT 
        p.IDPhieuXuat AS ID,
        p.SoChungTu,
        p.NgayXuat,
        p.IDKho,
        k.TenKhoHang AS TenKho,
        k.MaKhoHang AS MaKhoHang,
        p.IDNhanSuNhan,
        ns.Ten AS TenNhanSuNhan,
        p.TenNguoiNhan,
        p.SoDienThoaiNguoiNhan,
        p.TrangThai,
        p.TongTienHang,
        p.TongTienThue,
        p.TongCong,
        p.NgayTao,
        p.NguoiTao,
        COALESCE(
            NULLIF(LTRIM(RTRIM(ISNULL(nsTaoDirect.HoDem, '') + ' ' + ISNULL(nsTaoDirect.Ten, ''))), ''),
            NULLIF(LTRIM(RTRIM(ISNULL(nsTaoViaUser.HoDem, '') + ' ' + ISNULL(nsTaoViaUser.Ten, ''))), ''),
            u.TenDangNhap,
            ''
        ) AS NguoiTaoText
    FROM [dbo].[KHO_PhieuXuat] p
    LEFT JOIN [dbo].[DM_KhoHang] k ON p.IDKho = k.ID
    LEFT JOIN [dbo].[NS_NhanSu] ns ON p.IDNhanSuNhan = ns.ID
    LEFT JOIN [dbo].[NS_NhanSu] nsTaoDirect ON p.NguoiTao = nsTaoDirect.ID
    LEFT JOIN [dbo].[ACL_Login] u ON p.NguoiTao = u.ID
    LEFT JOIN [dbo].[NS_NhanSu] nsTaoViaUser ON u.IDNhanSu = nsTaoViaUser.ID
    WHERE p.IsDeleted = 0
      AND (@TuNgay IS NULL OR p.NgayXuat >= @TuNgay)
      AND (@DenNgay IS NULL OR p.NgayXuat <= @DenNgay)
      AND (@SoChungTu IS NULL OR p.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR p.IDKho = @IDKho)
      AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
      AND (@IDNhanSuNhan IS NULL OR p.IDNhanSuNhan = @IDNhanSuNhan)
    ORDER BY p.NgayXuat DESC, p.IDPhieuXuat DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- 2. GetByID
CREATE OR ALTER PROCEDURE [dbo].[sp_KHO_PhieuXuat_GetByID]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        IDPhieuXuat AS ID,
        SoChungTu,
        NgayXuat,
        IDKho,
        IDNhanSuNhan,
        TenNguoiNhan,
        SoDienThoaiNguoiNhan,
        GhiChu,
        TongTienHang,
        TongTienThue,
        TongCong,
        TrangThai
    FROM [dbo].[KHO_PhieuXuat]
    WHERE IDPhieuXuat = @ID AND IsDeleted = 0;
END;
GO

-- 3. GetChiTiet
CREATE OR ALTER PROCEDURE [dbo].[sp_KHO_PhieuXuat_GetChiTiet]
    @IDPhieuXuat INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        c.ID,
        c.IDPhieuXuat,
        c.IDSanPham,
        sp.MaSanPham,
        sp.TenSanPham,
        sp.DVT,
        c.SoLuong,
        c.DonGia,
        c.ThanhTien,
        c.ThueGTGT,
        c.TienThue,
        c.TongSauThue,
        c.GhiChu
    FROM [dbo].[KHO_PhieuXuat_ChiTiet] c
    LEFT JOIN [dbo].[DM_SanPham] sp ON c.IDSanPham = sp.ID
    WHERE c.IDPhieuXuat = @IDPhieuXuat
    ORDER BY c.STT ASC;
END;
GO

-- 4. GenerateSoChungTu
CREATE OR ALTER PROCEDURE [dbo].[sp_KHO_PhieuXuat_GenerateSoChungTu]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @MaxId INT;
    DECLARE @SoChungTu NVARCHAR(50);
    
    SELECT @MaxId = ISNULL(MAX(IDPhieuXuat), 0) + 1 FROM [dbo].[KHO_PhieuXuat];
    
    SET @SoChungTu = 'PX' + RIGHT('000000' + CAST(@MaxId AS NVARCHAR), 6);
    
    SELECT @SoChungTu AS SoChungTu;
END;
GO

-- 5. Save (Insert/Update)
CREATE OR ALTER PROCEDURE [dbo].[sp_KHO_PhieuXuat_Save]
    @ID INT,
    @NgayXuat DATETIME,
    @IDKho INT,
    @IDNhanSuNhan INT = NULL,
    @TenNguoiNhan NVARCHAR(250) = NULL,
    @SoDienThoaiNguoiNhan NVARCHAR(50) = NULL,
    @GhiChu NVARCHAR(MAX) = NULL,
    @NguoiTao INT,
    @ChiTietJson NVARCHAR(MAX),
    @NewID INT OUTPUT,
    @SoChungTuOut NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @ID = 0
        BEGIN
            -- Generate SoChungTu
            DECLARE @MaxId INT;
            SELECT @MaxId = ISNULL(MAX(IDPhieuXuat), 0) + 1 FROM [dbo].[KHO_PhieuXuat];
            SET @SoChungTuOut = 'PX' + RIGHT('000000' + CAST(@MaxId AS NVARCHAR), 6);

            INSERT INTO [dbo].[KHO_PhieuXuat] (
                SoChungTu, NgayXuat, IDKho, IDNhanSuNhan, TenNguoiNhan, SoDienThoaiNguoiNhan,
                GhiChu, TrangThai, NguoiTao, NgayTao, IsDeleted
            ) VALUES (
                @SoChungTuOut, @NgayXuat, @IDKho, @IDNhanSuNhan, @TenNguoiNhan, @SoDienThoaiNguoiNhan,
                @GhiChu, 1, @NguoiTao, GETDATE(), 0
            );

            SET @NewID = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [dbo].[KHO_PhieuXuat]
            SET NgayXuat = @NgayXuat,
                IDKho = @IDKho,
                IDNhanSuNhan = @IDNhanSuNhan,
                TenNguoiNhan = @TenNguoiNhan,
                SoDienThoaiNguoiNhan = @SoDienThoaiNguoiNhan,
                GhiChu = @GhiChu,
                NguoiCapNhat = @NguoiTao,
                NgayCapNhat = GETDATE()
            WHERE IDPhieuXuat = @ID AND TrangThai = 1 AND IsDeleted = 0;

            SET @NewID = @ID;
            SELECT @SoChungTuOut = SoChungTu FROM [dbo].[KHO_PhieuXuat] WHERE IDPhieuXuat = @ID;
        END

        -- Delete old chi tiet
        DELETE FROM [dbo].[KHO_PhieuXuat_ChiTiet] WHERE IDPhieuXuat = @NewID;

        -- Insert new chi tiet
        IF @ChiTietJson IS NOT NULL AND LEN(@ChiTietJson) > 0
        BEGIN
            INSERT INTO [dbo].[KHO_PhieuXuat_ChiTiet] (
                IDPhieuXuat, IDSanPham, STT, SoLuong, DonGia, ThanhTien,
                ThueGTGT, TienThue, TongSauThue, GhiChu, NgayTao, NguoiTao
            )
            SELECT 
                @NewID,
                JSON_VALUE(value, '$.IDSanPham'),
                CAST(JSON_VALUE(value, '$.STT') AS INT),
                CAST(JSON_VALUE(value, '$.SoLuong') AS DECIMAL(18,2)),
                CAST(JSON_VALUE(value, '$.DonGia') AS DECIMAL(18,2)),
                CAST(JSON_VALUE(value, '$.ThanhTien') AS DECIMAL(18,2)),
                CAST(JSON_VALUE(value, '$.ThueGTGT') AS DECIMAL(18,2)),
                CAST(JSON_VALUE(value, '$.TienThue') AS DECIMAL(18,2)),
                CAST(JSON_VALUE(value, '$.TongSauThue') AS DECIMAL(18,2)),
                JSON_VALUE(value, '$.GhiChu'),
                GETDATE(),
                @NguoiTao
            FROM OPENJSON(@ChiTietJson);
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 6. GhiSo
CREATE OR ALTER PROCEDURE [dbo].[sp_KHO_PhieuXuat_GhiSo]
    @ID INT,
    @NguoiGhiSo INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[KHO_PhieuXuat]
    SET TrangThai = 2,
        NgayGhiSo = GETDATE(),
        NguoiGhiSo = @NguoiGhiSo
    WHERE IDPhieuXuat = @ID AND TrangThai = 1 AND IsDeleted = 0;
END;
GO

-- 7. Huy
CREATE OR ALTER PROCEDURE [dbo].[sp_KHO_PhieuXuat_Huy]
    @ID INT,
    @LyDoHuy NVARCHAR(MAX),
    @NguoiHuy INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[KHO_PhieuXuat]
    SET TrangThai = 3,
        NgayHuy = GETDATE(),
        NguoiHuy = @NguoiHuy,
        LyDoHuy = @LyDoHuy
    WHERE IDPhieuXuat = @ID AND TrangThai IN (1, 2) AND IsDeleted = 0;
END;
GO

-- 8. Delete
CREATE OR ALTER PROCEDURE [dbo].[sp_KHO_PhieuXuat_Delete]
    @ID INT,
    @NguoiXoa INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[KHO_PhieuXuat]
    SET IsDeleted = 1,
        NgayCapNhat = GETDATE(),
        NguoiCapNhat = @NguoiXoa
    WHERE IDPhieuXuat = @ID AND TrangThai = 1;
END;
GO
