CREATE OR ALTER PROCEDURE sp_KHO_DieuChinhPhieuNhap_Save
    @IDPhieuNhap INT,
    @LyDoDieuChinh NVARCHAR(1000),
    @ChiTietsJson NVARCHAR(MAX),
    @IDLoaiNhapKho INT,
    @IDKho INT,
    @IDKhoNguon INT = NULL,
    @IDNhaCungCap INT = NULL,
    @IDKhachHang INT = NULL,
    @IDPhuongTien INT = NULL,
    @NgayNhap DATETIME,
    @NgayGiaoHang DATETIME = NULL,
    @HoTenTaiXe NVARCHAR(255) = NULL,
    @SoDienThoaiTaiXe NVARCHAR(50) = NULL,
    @SoHoaDon NVARCHAR(50) = NULL,
    @NgayHoaDon DATETIME = NULL,
    @GhiChu NVARCHAR(500) = NULL,
    @NguoiTao INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- 1. L?y thông tin phi?u nh?p hi?n t?i
    DECLARE @SoChungTu NVARCHAR(50);
    DECLARE @TrangThai INT;
    DECLARE @TongTienCu DECIMAL(18,2);
    DECLARE @OldIDLoaiNhapKho INT;
    DECLARE @OldIDKho INT;
    DECLARE @OldIDKhoNguon INT;

    SELECT 
        @SoChungTu = SoChungTu,
        @TrangThai = TrangThai,
        @OldIDLoaiNhapKho = IDLoaiNhapKho,
        @OldIDKho = IDKho,
        @OldIDKhoNguon = IDKhoNguon
    FROM KHO_PhieuNhap
    WHERE ID = @IDPhieuNhap AND IsDeleted = 0;

    IF @SoChungTu IS NULL
    BEGIN
        THROW 50001, N'Không tìm th?y phi?u nh?p g?c ho?c phi?u dã b? xóa.', 1;
    END

    IF @TrangThai NOT IN (2) 
    BEGIN
        THROW 50002, N'Ch? phi?u nh?p dã ghi s? m?i du?c di?u ch?nh qua module này.', 1;
    END

    -- Get Old Total
    SELECT @TongTienCu = ISNULL(SUM(TongSauThue), 0)
    FROM KHO_PhieuNhap_ChiTiet
    WHERE IDPhieuNhap = @IDPhieuNhap;

    -- 2. Parse chi ti?t m?i t? JSON
    DECLARE @ChiTietMoi TABLE (
        IDSanPham INT,
        SoLuong DECIMAL(18,2),
        DonGia DECIMAL(18,2),
        ThueGTGT DECIMAL(18,2),
        ThanhTien DECIMAL(18,2),
        TienThue DECIMAL(18,2),
        TongSauThue DECIMAL(18,2),
        GhiChu NVARCHAR(500),
        NgaySanXuat DATETIME NULL,
        HanSuDung DATETIME NULL
    );

    INSERT INTO @ChiTietMoi (IDSanPham, SoLuong, DonGia, ThueGTGT, ThanhTien, TienThue, TongSauThue, GhiChu, NgaySanXuat, HanSuDung)
    SELECT 
        ISNULL(IDSanPham, 0),
        ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 0 END, 2),
        ROUND(CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 2),
        ROUND(CASE WHEN ThueGTGT >= 0 THEN ThueGTGT ELSE 0 END, 2),
        ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 0 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) AS ThanhTien,
        ROUND(ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 0 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) * CASE WHEN ThueGTGT >= 0 THEN ThueGTGT ELSE 0 END / 100, 0) AS TienThue,
        ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 0 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) + 
        ROUND(ROUND(CASE WHEN SoLuong >= 0 THEN SoLuong ELSE 0 END * CASE WHEN DonGia >= 0 THEN DonGia ELSE 0 END, 0) * CASE WHEN ThueGTGT >= 0 THEN ThueGTGT ELSE 0 END / 100, 0) AS TongSauThue,
        GhiChu,
        NgaySanXuat,
        HanSuDung
    FROM OPENJSON(@ChiTietsJson)
    WITH (
        IDSanPham INT '$.IDSanPham',
        SoLuong DECIMAL(18,2) '$.SoLuong',
        DonGia DECIMAL(18,2) '$.DonGia',
        ThueGTGT DECIMAL(18,2) '$.ThueGTGT',
        GhiChu NVARCHAR(500) '$.GhiChu',
        NgaySanXuat DATETIME '$.NgaySanXuat',
        HanSuDung DATETIME '$.HanSuDung'
    );

    -- Tính toán t?ng ti?n m?i
    DECLARE @newTongTien DECIMAL(18,2);
    SELECT @newTongTien = ISNULL(SUM(TongSauThue), 0) FROM @ChiTietMoi;
    
    DECLARE @newTienHang DECIMAL(18,2);
    SELECT @newTienHang = ISNULL(SUM(ThanhTien), 0) FROM @ChiTietMoi;
    
    DECLARE @newTienThue DECIMAL(18,2);
    SELECT @newTienThue = ISNULL(SUM(TienThue), 0) FROM @ChiTietMoi;

    DECLARE @ChenhLech DECIMAL(18,2) = @newTongTien - @TongTienCu;

    -- 3. Sinh s? di?u ch?nh
    DECLARE @adjCount INT;
    SELECT @adjCount = COUNT(1) FROM KHO_DieuChinhPhieuNhap WHERE IDPhieuNhap = @IDPhieuNhap;
    DECLARE @soDieuChinh NVARCHAR(50) = N'DC-' + @SoChungTu + N'-' + RIGHT('00' + CAST(@adjCount + 1 AS NVARCHAR(10)), 2);

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 4. Luu header Ði?u ch?nh
        DECLARE @idDieuChinh INT;
        INSERT INTO KHO_DieuChinhPhieuNhap 
            (IDPhieuNhap, SoDieuChinh, NgayDieuChinh, LyDoDieuChinh, TongTienCu, TongTienMoi, ChenhLech, NgayTao, NguoiTao)
        VALUES 
            (@IDPhieuNhap, @soDieuChinh, GETDATE(), @LyDoDieuChinh, @TongTienCu, @newTongTien, @ChenhLech, GETDATE(), @NguoiTao);
        SET @idDieuChinh = SCOPE_IDENTITY();

        -- 5. L?y t?p h?p t?t c? các ID s?n ph?m tham gia (tru?c và sau)
        DECLARE @allSpIds TABLE (IDSanPham INT PRIMARY KEY);
        INSERT INTO @allSpIds (IDSanPham)
        SELECT DISTINCT IDSanPham FROM KHO_PhieuNhap_ChiTiet WHERE IDPhieuNhap = @IDPhieuNhap AND IDSanPham IS NOT NULL
        UNION
        SELECT DISTINCT IDSanPham FROM @ChiTietMoi WHERE IDSanPham IS NOT NULL;

        -- Duy?t qua t?ng s?n ph?m d? so sánh và ghi nh?n di?u ch?nh
        DECLARE @spId INT;
        DECLARE db_cursor CURSOR LOCAL FOR SELECT IDSanPham FROM @allSpIds;
        OPEN db_cursor;
        FETCH NEXT FROM db_cursor INTO @spId;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @slCu DECIMAL(18,2) = NULL, @slMoi DECIMAL(18,2) = NULL;
            DECLARE @dgCu DECIMAL(18,2) = NULL, @dgMoi DECIMAL(18,2) = NULL;
            DECLARE @ttCu DECIMAL(18,2) = NULL, @ttMoi DECIMAL(18,2) = NULL;
            DECLARE @itemLoaiThayDoi NVARCHAR(20) = N'KhongDoi';

            SELECT @slCu = SUM(SoLuong), @dgCu = MAX(DonGia), @ttCu = SUM(TongSauThue)
            FROM KHO_PhieuNhap_ChiTiet WHERE IDPhieuNhap = @IDPhieuNhap AND IDSanPham = @spId;

            SELECT @slMoi = SUM(SoLuong), @dgMoi = MAX(DonGia), @ttMoi = SUM(TongSauThue)
            FROM @ChiTietMoi WHERE IDSanPham = @spId;

            IF @slCu IS NOT NULL AND @slMoi IS NULL SET @itemLoaiThayDoi = N'Xoa';
            ELSE IF @slCu IS NULL AND @slMoi IS NOT NULL SET @itemLoaiThayDoi = N'ThemMoi';
            ELSE IF ISNULL(@slCu, 0) <> ISNULL(@slMoi, 0) OR ISNULL(@dgCu, 0) <> ISNULL(@dgMoi, 0) OR ISNULL(@ttCu, 0) <> ISNULL(@ttMoi, 0)
                SET @itemLoaiThayDoi = N'CapNhat';

            DECLARE @isKhoChanged BIT = 0;
            IF ISNULL(@OldIDKho, 0) <> ISNULL(@IDKho, 0) OR ISNULL(@OldIDKhoNguon, 0) <> ISNULL(@IDKhoNguon, 0)
                SET @isKhoChanged = 1;

            IF @itemLoaiThayDoi <> N'KhongDoi' OR @isKhoChanged = 1
            BEGIN
                IF @isKhoChanged = 1 AND @itemLoaiThayDoi = N'KhongDoi' SET @itemLoaiThayDoi = N'DoiKho';

                INSERT INTO KHO_DieuChinhPhieuNhapChiTiet
                    (IDDieuChinh, IDPhieuNhapChiTiet, IDSanPhamCu, IDSanPhamMoi, SoLuongCu, SoLuongMoi, DonGiaCu, DonGiaMoi, ThanhTienCu, ThanhTienMoi, LoaiThayDoi, NgayTao, NguoiTao)
                VALUES
                    (@idDieuChinh, 0, CASE WHEN @slCu IS NOT NULL THEN @spId ELSE NULL END, CASE WHEN @slMoi IS NOT NULL THEN @spId ELSE NULL END, @slCu, @slMoi, @dgCu, @dgMoi, @ttCu, @ttMoi, @itemLoaiThayDoi, GETDATE(), @NguoiTao);
            END

            FETCH NEXT FROM db_cursor INTO @spId;
        END

        CLOSE db_cursor;
        DEALLOCATE db_cursor;

        -- 6. C?p nh?t b?ng g?c KHO_PhieuNhap & KHO_PhieuNhap_ChiTiet
        UPDATE KHO_PhieuNhap SET
            IDLoaiNhapKho = @IDLoaiNhapKho,
            IDKho = @IDKho,
            IDKhoNguon = @IDKhoNguon,
            IDNhaCungCap = @IDNhaCungCap,
            IDKhachHang = @IDKhachHang,
            IDPhuongTien = @IDPhuongTien,
            NgayNhap = @NgayNhap,
            NgayGiaoHang = @NgayGiaoHang,
            HoTenTaiXe = @HoTenTaiXe,
            SoDienThoaiTaiXe = @SoDienThoaiTaiXe,
            SoHoaDon = @SoHoaDon,
            NgayHoaDon = @NgayHoaDon,
            GhiChu = @GhiChu,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @NguoiTao,
            TongTien = @newTongTien,
            TienHang = @newTienHang,
            TienThue = @newTienThue,
            ConLai = ISNULL(ConLai, 0) + @ChenhLech
        WHERE ID = @IDPhieuNhap;

        DELETE FROM KHO_PhieuNhap_ChiTiet WHERE IDPhieuNhap = @IDPhieuNhap;

        INSERT INTO KHO_PhieuNhap_ChiTiet
            (IDPhieuNhap, IDSanPham, SoLuong, DonGia, ThanhTien, ThueGTGT, TienThue, TongSauThue, GhiChu, NgaySanXuat, HanSuDung)
        SELECT 
            @IDPhieuNhap, IDSanPham, SoLuong, DonGia, ThanhTien, ThueGTGT, TienThue, TongSauThue, GhiChu, NgaySanXuat, HanSuDung
        FROM @ChiTietMoi;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
