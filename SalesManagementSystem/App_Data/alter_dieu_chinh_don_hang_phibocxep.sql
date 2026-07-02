-- =============================================
-- Author:      Antigravity
-- Create date: 2026-06-24
-- Description: Tạo bảng lưu lịch sử điều chỉnh và đăng ký quyền cho module Điều chỉnh đơn hàng
-- =============================================

-- 1. Tạo bảng DON_DieuChinhDonHang
IF OBJECT_ID('DON_DieuChinhDonHang') IS NULL
BEGIN
    CREATE TABLE DON_DieuChinhDonHang
    (
        ID INT IDENTITY PRIMARY KEY,
        IDDonHang INT NOT NULL,
        SoDieuChinh NVARCHAR(50) NOT NULL,
        NgayDieuChinh DATETIME NOT NULL,
        LyDoDieuChinh NVARCHAR(1000) NULL,
        TongTienCu DECIMAL(18,2) NOT NULL DEFAULT 0,
        TongTienMoi DECIMAL(18,2) NOT NULL DEFAULT 0,
        NguoiTao INT NULL,
        NgayTao DATETIME NULL,
        TrangThaiDon INT NULL
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DON_DieuChinhDonHang') AND name = 'TrangThaiDon')
    BEGIN
        ALTER TABLE DON_DieuChinhDonHang ADD TrangThaiDon INT NULL;
    END
END
GO

-- 2. Tạo bảng DON_DieuChinhDonHang_ChiTiet
IF OBJECT_ID('DON_DieuChinhDonHang_ChiTiet') IS NULL
BEGIN
    CREATE TABLE DON_DieuChinhDonHang_ChiTiet
    (
        ID INT IDENTITY PRIMARY KEY,
        IDDieuChinh INT NOT NULL,
        IDSanPham INT NOT NULL,
        SoLuongCu DECIMAL(18,2) NULL,
        SoLuongMoi DECIMAL(18,2) NULL,
        DonGiaCu DECIMAL(18,2) NULL,
        DonGiaMoi DECIMAL(18,2) NULL,
        ThanhTienCu DECIMAL(18,2) NULL,
        ThanhTienMoi DECIMAL(18,2) NULL,
        GhiChu NVARCHAR(500) NULL
    );
END
GO

-- 3. Đăng ký màn hình trong ACL_ManHinh
DECLARE @ManHinhID INT;
IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'Điều chỉnh đơn hàng')
BEGIN
    INSERT INTO ACL_ManHinh (TenManHinh, NhomChaManHinh, IsSuDung, STT)
    VALUES (N'Điều chỉnh đơn hàng', N'BAN HANG', 1, 1028);
    SET @ManHinhID = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @ManHinhID = ID FROM ACL_ManHinh WHERE TenManHinh = N'Điều chỉnh đơn hàng';
END

-- Đăng ký các action cho màn hình Điều chỉnh đơn hàng
IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Index')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Index', 'DonDieuChinhDonHang', 1, N'Xem danh sách điều chỉnh đơn hàng');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Adjust')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'Adjust', 'DonDieuChinhDonHang', 3, N'Thực hiện điều chỉnh đơn hàng');

IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'History')
    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@ManHinhID, 'History', 'DonDieuChinhDonHang', 1, N'Xem lịch sử điều chỉnh');

-- Cấp quyền cho toàn bộ các tài khoản hiện có
INSERT INTO ACL_PhanQuyen (IDLogin, IDAction, IsChoPhep, NgayTao)
SELECT l.ID, act.ID, 1, GETDATE()
FROM ACL_Login l
CROSS JOIN ACL_Action act
WHERE act.IDManHinh = @ManHinhID
  AND NOT EXISTS (
      SELECT 1 FROM ACL_PhanQuyen pq 
      WHERE pq.IDLogin = l.ID AND pq.IDAction = act.ID
  );
GO

-- =============================================
-- 4. STORED PROCEDURE: sp_DON_DieuChinhDonHang_Save
-- =============================================
ALTER PROCEDURE [dbo].[sp_DON_DieuChinhDonHang_Save]
    @IDDonHang INT,
    @LyDoDieuChinh NVARCHAR(1000),
    @ChiTietsJson NVARCHAR(MAX),
    @PhiBocXep DECIMAL(18,2),
    @IDKho INT,
    @NguoiTao INT,
    @IDKhachHang INT = NULL,
    @IDNhanVien INT = NULL,
    @NgayTaoDon DATETIME = NULL,
    @NgayGiaoHang DATETIME = NULL,
    @ThoiHanGiaoHang DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- 1. Lấy thông tin đơn hàng hiện tại
    DECLARE @SoDonHang NVARCHAR(50);
    DECLARE @TrangThaiDon INT;
    DECLARE @TongTienCu DECIMAL(18,2);

    DECLARE @currentNgayTaoDon DATETIME;
    DECLARE @currentIDNhanVien INT;
    DECLARE @currentThoiHanGiaoHang DATETIME;
    DECLARE @currentIDKhachHang INT;

    SELECT 
        @SoDonHang = SoDonHang,
        @currentNgayTaoDon = NgayTaoDon,
        @currentIDNhanVien = IDNhanVien,
        @currentThoiHanGiaoHang = ThoiHanGiaoHang,
        @currentIDKhachHang = IDKhachHang,
        @TrangThaiDon = TrangThaiDon,
        @TongTienCu = TongTien
    FROM NS_DonDatHang
    WHERE ID = @IDDonHang;

    IF @SoDonHang IS NULL
    BEGIN
        THROW 50001, N'Không tìm thấy đơn hàng gốc.', 1;
    END

    -- Fallback to current values if inputs are null
    SET @IDKhachHang = ISNULL(@IDKhachHang, @currentIDKhachHang);
    SET @IDNhanVien = ISNULL(@IDNhanVien, @currentIDNhanVien);
    SET @NgayTaoDon = ISNULL(@NgayTaoDon, @currentNgayTaoDon);
    SET @ThoiHanGiaoHang = ISNULL(@ThoiHanGiaoHang, @currentThoiHanGiaoHang);

    -- 2. Parse chi tiết mới từ JSON
    DECLARE @ChiTietMoi TABLE (
        IDSanPham INT,
        SoLuong DECIMAL(18,2),
        DonGia DECIMAL(18,2),
        DonGiaBocXep DECIMAL(18,2),
        ThanhTienBocXep DECIMAL(18,2),
        ThanhTienHang DECIMAL(18,2),
        ThueGTGT DECIMAL(18,2),
        ThanhTien DECIMAL(18,2),
        ThanhTienThue DECIMAL(18,2),
        ThanhTienSauThue DECIMAL(18,2),
        IsHangKhuyenMai BIT,
        GhiChu NVARCHAR(500)
    );

    INSERT INTO @ChiTietMoi (IDSanPham, SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep, ThanhTienHang, ThueGTGT, ThanhTien, ThanhTienThue, ThanhTienSauThue, IsHangKhuyenMai, GhiChu)
    SELECT 
        ISNULL(IDSanPham, 0),
        ISNULL(SoLuong, 1),
        ISNULL(DonGia, 0),
        ISNULL(DonGiaBocXep, 0),
        ISNULL(ThanhTienBocXep, 0),
        ISNULL(ThanhTienHang, 0),
        ISNULL(ThueGTGT, 0),
        ISNULL(ThanhTien, 0),
        ISNULL(ThanhTienThue, 0),
        ISNULL(ThanhTienSauThue, 0),
        ISNULL(IsHangKhuyenMai, 0),
        GhiChu
    FROM OPENJSON(@ChiTietsJson)
    WITH (
        IDSanPham INT '$.idSanPham',
        SoLuong DECIMAL(18,2) '$.soLuong',
        DonGia DECIMAL(18,2) '$.donGia',
        DonGiaBocXep DECIMAL(18,2) '$.donGiaBocXep',
        ThanhTienBocXep DECIMAL(18,2) '$.thanhTienBocXep',
        ThanhTienHang DECIMAL(18,2) '$.thanhTienHang',
        ThanhTien DECIMAL(18,2) '$.thanhTien',
        ThanhTienThue DECIMAL(18,2) '$.thanhTienThue',
        ThanhTienSauThue DECIMAL(18,2) '$.thanhTienSauThue',
        ThueGTGT DECIMAL(18,2) '$.thueGTGT',
        IsHangKhuyenMai BIT '$.isHangKhuyenMai',
        GhiChu NVARCHAR(500) '$.ghiChu'
    );

    -- Tính toán tổng tiền mới
    DECLARE @newThanhTienHang DECIMAL(18,2);
    DECLARE @newThanhTienThue DECIMAL(18,2);
    DECLARE @newTongTien DECIMAL(18,2);

    SELECT 
        @newThanhTienHang = SUM(ThanhTienHang),
        @newThanhTienThue = SUM(ThanhTienThue),
        @newTongTien = SUM(ThanhTienSauThue)
    FROM @ChiTietMoi;

    SET @newThanhTienHang = ISNULL(@newThanhTienHang, 0);
    SET @newThanhTienThue = ISNULL(@newThanhTienThue, 0);
    SET @newTongTien = ISNULL(@newTongTien, 0);

    -- 3. Sinh số điều chỉnh
    DECLARE @adjCount INT;
    SELECT @adjCount = COUNT(1) FROM DON_DieuChinhDonHang WHERE IDDonHang = @IDDonHang;
    DECLARE @soDieuChinh NVARCHAR(50) = N'DC-' + @SoDonHang + N'-' + RIGHT('00' + CAST(@adjCount + 1 AS NVARCHAR(10)), 2);

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 4. Lưu header Điều chỉnh
        DECLARE @idDieuChinh INT;
        INSERT INTO DON_DieuChinhDonHang 
            (IDDonHang, SoDieuChinh, NgayDieuChinh, LyDoDieuChinh, TongTienCu, TongTienMoi, NguoiTao, NgayTao, TrangThaiDon)
        VALUES 
            (@IDDonHang, @soDieuChinh, GETDATE(), @LyDoDieuChinh, @TongTienCu, @newTongTien, @NguoiTao, GETDATE(), @TrangThaiDon);
        SET @idDieuChinh = SCOPE_IDENTITY();

        -- 5. Lấy tập hợp tất cả các ID sản phẩm tham gia (trước và sau)
        DECLARE @allSpIds TABLE (IDSanPham INT PRIMARY KEY);
        INSERT INTO @allSpIds (IDSanPham)
        SELECT DISTINCT IDSanPham FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @IDDonHang AND IDSanPham IS NOT NULL
        UNION
        SELECT DISTINCT IDSanPham FROM @ChiTietMoi WHERE IDSanPham IS NOT NULL;

        -- Kiểm tra xem đơn hàng đã xuất kho chưa
        DECLARE @isDaXuatKho BIT = 0;
        IF EXISTS (SELECT 1 FROM KHO_PhieuXuat WHERE IDDonDatHang = @IDDonHang AND TrangThai = 2 AND IsDeleted = 0)
        BEGIN
            SET @isDaXuatKho = 1;
        END

        -- Duyệt qua từng sản phẩm để so sánh và ghi nhận điều chỉnh
        DECLARE @spId INT;
        DECLARE db_cursor CURSOR LOCAL FOR SELECT IDSanPham FROM @allSpIds;
        OPEN db_cursor;
        FETCH NEXT FROM db_cursor INTO @spId;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @slCu DECIMAL(18,2) = NULL, @slMoi DECIMAL(18,2) = NULL;
            DECLARE @dgCu DECIMAL(18,2) = NULL, @dgMoi DECIMAL(18,2) = NULL;
            DECLARE @ttCu DECIMAL(18,2) = NULL, @ttMoi DECIMAL(18,2) = NULL;
            DECLARE @itemGhiChu NVARCHAR(500) = N'';

            -- Lấy thông tin cũ
            SELECT 
                @slCu = SoLuong,
                @dgCu = DonGia,
                @ttCu = CASE WHEN ThanhTienSauThue <> 0 THEN ThanhTienSauThue ELSE (ThanhTien + ISNULL(ThanhTienThue, 0)) END
            FROM NS_DonDatHangChiTiet
            WHERE IDDonDatHang = @IDDonHang AND IDSanPham = @spId;

            -- Lấy thông tin mới
            SELECT 
                @slMoi = SoLuong,
                @dgMoi = DonGia,
                @ttMoi = ThanhTienSauThue,
                @itemGhiChu = GhiChu
            FROM @ChiTietMoi
            WHERE IDSanPham = @spId;

            -- Chỉ ghi nhận dòng có thay đổi
            IF ISNULL(@slCu, 0) <> ISNULL(@slMoi, 0) OR ISNULL(@dgCu, 0) <> ISNULL(@dgMoi, 0) OR ISNULL(@ttCu, 0) <> ISNULL(@ttMoi, 0)
            BEGIN
                INSERT INTO DON_DieuChinhDonHang_ChiTiet
                    (IDDieuChinh, IDSanPham, SoLuongCu, SoLuongMoi, DonGiaCu, DonGiaMoi, ThanhTienCu, ThanhTienMoi, GhiChu)
                VALUES
                    (@idDieuChinh, @spId, @slCu, @slMoi, @dgCu, @dgMoi, @ttCu, @ttMoi, @itemGhiChu);

                -- Xử lý chênh lệch tồn kho nếu đã xuất kho
                IF @isDaXuatKho = 1 AND @IDKho IS NOT NULL AND @IDKho > 0
                BEGIN
                    DECLARE @qCu DECIMAL(18,2) = ISNULL(@slCu, 0);
                    DECLARE @qMoi DECIMAL(18,2) = ISNULL(@slMoi, 0);
                    DECLARE @delta DECIMAL(18,2) = @qMoi - @qCu;

                    IF @delta > 0
                    BEGIN
                        -- Xuất thêm (LoaiChungTu = 2)
                        DECLARE @dgMoiOrCu DECIMAL(18,2) = COALESCE(@dgMoi, @dgCu, 0);
                        INSERT INTO KHO_GiaoDichKho 
                            (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao) 
                        VALUES 
                            (CAST(GETDATE() AS DATE), @soDieuChinh, 2, 0, @IDKho, @spId, 0, @delta, @dgMoiOrCu, @dgMoiOrCu * @delta, N'Xuất điều chỉnh tăng bán hàng theo phiếu ' + @soDieuChinh, GETDATE(), @NguoiTao);
                    END
                    ELSE IF @delta < 0
                    BEGIN
                        -- Nhập lại (LoaiChungTu = 1)
                        DECLARE @actualDelta DECIMAL(18,2) = ABS(@delta);
                        DECLARE @dgCuOrMoi DECIMAL(18,2) = COALESCE(@dgCu, @dgMoi, 0);
                        INSERT INTO KHO_GiaoDichKho 
                            (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao) 
                        VALUES 
                            (CAST(GETDATE() AS DATE), @soDieuChinh, 1, 0, @IDKho, @spId, @actualDelta, 0, @dgCuOrMoi, @dgCuOrMoi * @actualDelta, N'Nhập điều chỉnh giảm bán hàng theo phiếu ' + @soDieuChinh, GETDATE(), @NguoiTao);
                    END
                END
            END

            FETCH NEXT FROM db_cursor INTO @spId;
        END

        CLOSE db_cursor;
        DEALLOCATE db_cursor;

        -- 6. Cập nhật bảng gốc NS_DonDatHang & NS_DonDatHangChiTiet
        UPDATE NS_DonDatHang SET
            TongTien = @newTongTien,
            PhiBocXep = @PhiBocXep,
            ThanhTienHang = @newThanhTienHang,
            ThanhTienThue = @newThanhTienThue,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @NguoiTao,
            IDKhachHang = @IDKhachHang,
            IDNhanVien = @IDNhanVien,
            NgayTaoDon = @NgayTaoDon,
            ThoiHanGiaoHang = @ThoiHanGiaoHang
        WHERE ID = @IDDonHang;

        DELETE FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @IDDonHang;

        INSERT INTO NS_DonDatHangChiTiet
            (IDDonDatHang, IDSanPham, SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep, ThanhTienHang, ThanhTien, ThanhTienSauThue, ThanhTienThue,
             ThueGTGT, IsHangKhuyenMai, GhiChu,
             NgayTaoDon, SoDonHang, IDNhanVien, ThoiHanGiaoHang, TrangThaiDon, NgayTao, NguoiTao)
        SELECT 
            @IDDonHang, IDSanPham, SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep, ThanhTienHang, ThanhTien, ThanhTienSauThue, ThanhTienThue,
            ThueGTGT, IsHangKhuyenMai, GhiChu,
            @NgayTaoDon, @SoDonHang, @IDNhanVien, @ThoiHanGiaoHang, @TrangThaiDon, GETDATE(), @NguoiTao
        FROM @ChiTietMoi;

        -- 7. Cập nhật bảng liên đới BAN_ChungTuBanHang & BAN_ChungTuBanHang_ChiTiet (nếu có)
        DECLARE @invoiceId INT;
        DECLARE @currentDaThanhToan DECIMAL(18,2);
        
        SELECT @invoiceId = ID, @currentDaThanhToan = DaThanhToan
        FROM BAN_ChungTuBanHang
        WHERE IDDonDatHang = @IDDonHang AND IsDeleted = 0;

        IF @invoiceId IS NOT NULL
        BEGIN
            DECLARE @newConLai DECIMAL(18,2) = @newTongTien - @currentDaThanhToan;
            
            UPDATE BAN_ChungTuBanHang SET
                IDKho = @IDKho,
                IDKhachHang = @IDKhachHang,
                NgayChungTu = ISNULL(@NgayGiaoHang, NgayChungTu),
                TongTienHang = @newThanhTienHang,
                TongTienThue = @newThanhTienThue,
                PhiBocXep = @PhiBocXep,
                TongCong = @newTongTien,
                ConLai = @newConLai,
                NgayCapNhat = GETDATE(),
                NguoiCapNhat = @NguoiTao
            WHERE ID = @invoiceId;

            DELETE FROM BAN_ChungTuBanHang_ChiTiet WHERE IDChungTuBanHang = @invoiceId;

            INSERT INTO BAN_ChungTuBanHang_ChiTiet
                (IDChungTuBanHang, IDSanPham, STT, SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep, ThanhTienHang, ThanhTien, ThueGTGT, TienThue, TongSauThue, GhiChu)
            SELECT
                @invoiceId, IDSanPham, ROW_NUMBER() OVER(ORDER BY IDSanPham), SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep, ThanhTienHang, ThanhTien, ThueGTGT, ThanhTienThue, ThanhTienSauThue, GhiChu
            FROM @ChiTietMoi;

            -- Cập nhật ngày chứng từ trong KT_NhatKyChung nếu có
            IF @NgayGiaoHang IS NOT NULL
            BEGIN
                UPDATE KT_NhatKyChung
                SET NgayChungTu = CAST(@NgayGiaoHang AS DATE)
                WHERE LoaiChungTu = 'BAN' AND IDChungTu = @invoiceId;
            END
        END

        -- 8. Cập nhật bảng KHO_PhieuXuat & KHO_PhieuXuat_ChiTiet (nếu có)
        DECLARE @shipId INT;
        SELECT @shipId = ID FROM KHO_PhieuXuat WHERE IDDonDatHang = @IDDonHang AND IsDeleted = 0;

        IF @shipId IS NOT NULL
        BEGIN
            UPDATE KHO_PhieuXuat SET
                IDKho = @IDKho,
                NgayXuat = ISNULL(@NgayGiaoHang, NgayXuat),
                TongTienHang = @newThanhTienHang,
                TongTienThue = @newThanhTienThue,
                TongCong = @newTongTien,
                NgayCapNhat = GETDATE(),
                NguoiCapNhat = @NguoiTao
            WHERE ID = @shipId;

            DELETE FROM KHO_PhieuXuat_ChiTiet WHERE IDPhieuXuat = @shipId;

            INSERT INTO KHO_PhieuXuat_ChiTiet
                (IDPhieuXuat, IDSanPham, STT, SoLuong, DonGia, ThanhTien, ThueGTGT, TienThue, TongSauThue)
            SELECT
                @shipId, IDSanPham, ROW_NUMBER() OVER(ORDER BY IDSanPham), SoLuong, DonGia, ThanhTien, ThueGTGT, ThanhTienThue, ThanhTienSauThue
            FROM @ChiTietMoi;

            -- Cập nhật IDKho và NgayChungTu của các giao dịch kho cũ
            UPDATE KHO_GiaoDichKho
            SET IDKho = @IDKho,
                NgayChungTu = ISNULL(CAST(@NgayGiaoHang AS DATE), NgayChungTu)
            WHERE IDChiTietKho IN (
                SELECT pxct.ID
                FROM KHO_PhieuXuat_ChiTiet pxct
                WHERE pxct.IDPhieuXuat = @shipId
            ) AND LoaiChungTu = 2; -- Xuất kho
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

