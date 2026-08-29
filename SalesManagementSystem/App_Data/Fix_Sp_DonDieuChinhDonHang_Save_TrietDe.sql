-- =======================================================
-- SCRIPT SỬA LỖI TRIỆT ĐỂ: TỰ ĐỘNG ĐỒNG BỘ KHO_GIAODICHKHO KHI ĐIỀU CHỈNH ĐƠN HÀNG
-- Ngày cập nhật: 27/08/2026
-- =======================================================

CREATE OR ALTER PROCEDURE sp_DON_DieuChinhDonHang_Save
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
        @SoDonHang              = SoDonHang,
        @currentNgayTaoDon      = NgayTaoDon,
        @currentIDNhanVien      = IDNhanVien,
        @currentThoiHanGiaoHang = ThoiHanGiaoHang,
        @currentIDKhachHang     = IDKhachHang,
        @TrangThaiDon           = TrangThaiDon,
        @TongTienCu             = TongTien
    FROM NS_DonDatHang
    WHERE ID = @IDDonHang;

    IF @SoDonHang IS NULL
    BEGIN
        THROW 50001, N'Không tìm thấy đơn hàng gốc.', 1;
    END

    SET @IDKhachHang    = ISNULL(@IDKhachHang,    @currentIDKhachHang);
    SET @IDNhanVien     = ISNULL(@IDNhanVien,     @currentIDNhanVien);
    SET @NgayTaoDon     = ISNULL(@NgayTaoDon,     @currentNgayTaoDon);
    SET @ThoiHanGiaoHang= ISNULL(@ThoiHanGiaoHang,@currentThoiHanGiaoHang);

    -- 2. Parse chi tiết mới từ JSON
    DECLARE @ChiTietMoi TABLE (
        IDSanPham              INT,
        SoLuong                DECIMAL(18,2),
        DonGia                 DECIMAL(18,2),
        DonGiaBocXep           DECIMAL(18,2),
        ThanhTienBocXep        DECIMAL(18,2),
        SoTienChietKhau        DECIMAL(18,2),
        ChuongTrinhTichLuySale DECIMAL(18,2),
        ThanhTienHang          DECIMAL(18,2),
        ThueGTGT               DECIMAL(18,2),
        ThanhTien              DECIMAL(18,2),
        ThanhTienThue          DECIMAL(18,2),
        ThanhTienSauThue       DECIMAL(18,2),
        IsHangKhuyenMai        BIT,
        GhiChu                 NVARCHAR(500)
    );

    INSERT INTO @ChiTietMoi
        (IDSanPham, SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep,
         SoTienChietKhau, ChuongTrinhTichLuySale,
         ThanhTienHang, ThueGTGT, ThanhTien, ThanhTienThue, ThanhTienSauThue,
         IsHangKhuyenMai, GhiChu)
    SELECT
        ISNULL(IDSanPham, 0),
        ISNULL(SoLuong, 1),
        ISNULL(DonGia, 0),
        ISNULL(DonGiaBocXep, 0),
        ISNULL(ThanhTienBocXep, 0),
        ISNULL(SoTienChietKhau, 0),
        ISNULL(ChuongTrinhTichLuySale, 0),
        ISNULL(ThanhTienHang, 0),
        ISNULL(ThueGTGT, 0),
        ISNULL(ThanhTien, 0),
        ISNULL(ThanhTienThue, 0),
        ISNULL(ThanhTienSauThue, 0),
        ISNULL(IsHangKhuyenMai, 0),
        GhiChu
    FROM OPENJSON(@ChiTietsJson)
    WITH (
        IDSanPham              INT           '$.idSanPham',
        SoLuong                DECIMAL(18,2) '$.soLuong',
        DonGia                 DECIMAL(18,2) '$.donGia',
        DonGiaBocXep           DECIMAL(18,2) '$.donGiaBocXep',
        ThanhTienBocXep        DECIMAL(18,2) '$.thanhTienBocXep',
        SoTienChietKhau        DECIMAL(18,2) '$.soTienChietKhau',
        ChuongTrinhTichLuySale DECIMAL(18,2) '$.chuongTrinhTichLuySale',
        ThanhTienHang          DECIMAL(18,2) '$.thanhTienHang',
        ThanhTien              DECIMAL(18,2) '$.thanhTien',
        ThanhTienThue          DECIMAL(18,2) '$.thanhTienThue',
        ThanhTienSauThue       DECIMAL(18,2) '$.thanhTienSauThue',
        ThueGTGT               DECIMAL(18,2) '$.thueGTGT',
        IsHangKhuyenMai        BIT           '$.isHangKhuyenMai',
        GhiChu                 NVARCHAR(500) '$.ghiChu'
    );

    -- 3. Tính toán tổng tiền mới
    DECLARE @newThanhTienHang      DECIMAL(18,2);
    DECLARE @newThanhTienThue      DECIMAL(18,2);
    DECLARE @newTongChietKhau      DECIMAL(18,2);
    DECLARE @newTongTichLuySale    DECIMAL(18,2);
    DECLARE @newTongTien           DECIMAL(18,2);

    SELECT
        @newThanhTienHang    = SUM(ThanhTienHang),
        @newThanhTienThue    = SUM(ThanhTienThue),
        @newTongChietKhau    = SUM(SoTienChietKhau),
        @newTongTichLuySale  = SUM(ChuongTrinhTichLuySale),
        @newTongTien         = SUM(ThanhTienSauThue)
    FROM @ChiTietMoi;

    SET @newThanhTienHang    = ISNULL(@newThanhTienHang, 0);
    SET @newThanhTienThue    = ISNULL(@newThanhTienThue, 0);
    SET @newTongChietKhau    = ISNULL(@newTongChietKhau, 0);
    SET @newTongTichLuySale  = ISNULL(@newTongTichLuySale, 0);
    SET @newTongTien         = ISNULL(@newTongTien, 0);

    -- 4. Sinh số điều chỉnh
    DECLARE @adjCount INT;
    SELECT @adjCount = COUNT(1) FROM DON_DieuChinhDonHang WHERE IDDonHang = @IDDonHang;
    DECLARE @soDieuChinh NVARCHAR(50) = N'DC-' + @SoDonHang + N'-' + RIGHT('00' + CAST(@adjCount + 1 AS NVARCHAR(10)), 2);

    BEGIN TRANSACTION;
    BEGIN TRY

        -- 5. Lưu header lịch sử điều chỉnh
        DECLARE @idDieuChinh INT;
        INSERT INTO DON_DieuChinhDonHang
            (IDDonHang, SoDieuChinh, NgayDieuChinh, LyDoDieuChinh, TongTienCu, TongTienMoi, NguoiTao, NgayTao, TrangThaiDon)
        VALUES
            (@IDDonHang, @soDieuChinh, GETDATE(), @LyDoDieuChinh, @TongTienCu, @newTongTien, @NguoiTao, GETDATE(), @TrangThaiDon);
        SET @idDieuChinh = SCOPE_IDENTITY();

        -- 6. Tập hợp tất cả ID sản phẩm (cũ + mới)
        DECLARE @allSpIds TABLE (IDSanPham INT PRIMARY KEY);
        INSERT INTO @allSpIds (IDSanPham)
        SELECT DISTINCT IDSanPham FROM NS_DonDatHangChiTiet
        WHERE IDDonDatHang = @IDDonHang AND IDSanPham IS NOT NULL
        UNION
        SELECT DISTINCT IDSanPham FROM @ChiTietMoi WHERE IDSanPham IS NOT NULL;

        DECLARE @isDaXuatKho BIT = 0;
        IF EXISTS (SELECT 1 FROM KHO_PhieuXuat WHERE IDDonDatHang = @IDDonHang AND TrangThai = 2 AND IsDeleted = 0)
            SET @isDaXuatKho = 1;

        -- 7. Ghi lịch sử thay đổi & điều chỉnh tồn kho
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

            SELECT
                @slCu = SoLuong,
                @dgCu = DonGia,
                @ttCu = CASE WHEN ThanhTienSauThue <> 0 THEN ThanhTienSauThue ELSE (ThanhTien + ISNULL(ThanhTienThue, 0)) END
            FROM NS_DonDatHangChiTiet
            WHERE IDDonDatHang = @IDDonHang AND IDSanPham = @spId;

            SELECT
                @slMoi = SoLuong,
                @dgMoi = DonGia,
                @ttMoi = ThanhTienSauThue,
                @itemGhiChu = GhiChu
            FROM @ChiTietMoi
            WHERE IDSanPham = @spId;

            IF ISNULL(@slCu, 0) <> ISNULL(@slMoi, 0)
               OR ISNULL(@dgCu, 0) <> ISNULL(@dgMoi, 0)
               OR ISNULL(@ttCu, 0) <> ISNULL(@ttMoi, 0)
            BEGIN
                INSERT INTO DON_DieuChinhDonHang_ChiTiet
                    (IDDieuChinh, IDSanPham, SoLuongCu, SoLuongMoi, DonGiaCu, DonGiaMoi, ThanhTienCu, ThanhTienMoi, GhiChu)
                VALUES
                    (@idDieuChinh, @spId, @slCu, @slMoi, @dgCu, @dgMoi, @ttCu, @ttMoi, @itemGhiChu);
            END

            FETCH NEXT FROM db_cursor INTO @spId;
        END

        CLOSE db_cursor;
        DEALLOCATE db_cursor;

        -- 8. Cập nhật NS_DonDatHang
        UPDATE NS_DonDatHang SET
            TongTien                   = @newTongTien,
            PhiBocXep                  = @PhiBocXep,
            TongTienChietKhau          = @newTongChietKhau,
            TongChuongTrinhTichLuySale = @newTongTichLuySale,
            ThanhTienHang              = @newThanhTienHang,
            ThanhTienThue              = @newThanhTienThue,
            NgayCapNhat                = GETDATE(),
            NguoiCapNhat               = @NguoiTao,
            IDKhachHang                = @IDKhachHang,
            IDNhanVien                 = @IDNhanVien,
            NgayTaoDon                 = @NgayTaoDon,
            ThoiHanGiaoHang            = @ThoiHanGiaoHang
        WHERE ID = @IDDonHang;

        -- 9. Xóa & thêm lại NS_DonDatHangChiTiet
        DELETE FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @IDDonHang;

        INSERT INTO NS_DonDatHangChiTiet
            (IDDonDatHang, IDSanPham, SoLuong, DonGia,
             DonGiaBocXep, ThanhTienBocXep,
             SoTienChietKhau, ChuongTrinhTichLuySale,
             ThanhTienHang, ThanhTien, ThanhTienSauThue, ThanhTienThue,
             ThueGTGT, IsHangKhuyenMai, GhiChu,
             NgayTaoDon, SoDonHang, IDNhanVien, ThoiHanGiaoHang, TrangThaiDon, NgayTao, NguoiTao)
        SELECT
            @IDDonHang, IDSanPham, SoLuong, DonGia,
            DonGiaBocXep, ThanhTienBocXep,
            SoTienChietKhau, ChuongTrinhTichLuySale,
            ThanhTienHang, ThanhTien, ThanhTienSauThue, ThanhTienThue,
            ThueGTGT, IsHangKhuyenMai, GhiChu,
            @NgayTaoDon, @SoDonHang, @IDNhanVien, @ThoiHanGiaoHang, @TrangThaiDon, GETDATE(), @NguoiTao
        FROM @ChiTietMoi;

        -- 10. Cập nhật BAN_ChungTuBanHang & BAN_ChungTuBanHang_ChiTiet (nếu có)
        DECLARE @invoiceId INT;
        DECLARE @currentDaThanhToan DECIMAL(18,2);

        SELECT @invoiceId = ID, @currentDaThanhToan = DaThanhToan
        FROM BAN_ChungTuBanHang
        WHERE IDDonDatHang = @IDDonHang AND IsDeleted = 0;

        IF @invoiceId IS NOT NULL
        BEGIN
            DECLARE @newConLai DECIMAL(18,2) = @newTongTien - ISNULL(@currentDaThanhToan, 0);

            UPDATE BAN_ChungTuBanHang SET
                IDKho                      = @IDKho,
                IDKhachHang                = @IDKhachHang,
                NgayChungTu                = ISNULL(@NgayGiaoHang, NgayChungTu),
                TongTienHang               = @newThanhTienHang,
                TongTienThue               = @newThanhTienThue,
                PhiBocXep                  = @PhiBocXep,
                TongTienChietKhau          = @newTongChietKhau,
                TongChuongTrinhTichLuySale = @newTongTichLuySale,
                TongCong                   = @newTongTien,
                ConLai                     = @newConLai,
                NgayCapNhat                = GETDATE(),
                NguoiCapNhat               = @NguoiTao
            WHERE ID = @invoiceId;

            DELETE FROM BAN_ChungTuBanHang_ChiTiet WHERE IDChungTuBanHang = @invoiceId;

            INSERT INTO BAN_ChungTuBanHang_ChiTiet
                (IDChungTuBanHang, IDSanPham, STT, SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep, ThanhTienHang, ThanhTien, ThueGTGT, TienThue, TongSauThue, GhiChu)
            SELECT
                @invoiceId, IDSanPham, ROW_NUMBER() OVER(ORDER BY IDSanPham), SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep, ThanhTienHang, ThanhTien, ThueGTGT, ThanhTienThue, ThanhTienSauThue, GhiChu
            FROM @ChiTietMoi;

            IF @NgayGiaoHang IS NOT NULL
            BEGIN
                UPDATE KT_NhatKyChung
                SET NgayChungTu = CAST(@NgayGiaoHang AS DATE)
                WHERE LoaiChungTu = 'BAN' AND IDChungTu = @invoiceId;
            END
        END

        -- 11. Cập nhật KHO_PhieuXuat & KHO_PhieuXuat_ChiTiet & TÁI TẠO KHO_GiaoDichKho
        DECLARE @shipId INT;
        DECLARE @shipSoChungTu NVARCHAR(50);
        SELECT @shipId = ID, @shipSoChungTu = SoChungTu FROM KHO_PhieuXuat WHERE IDDonDatHang = @IDDonHang AND IsDeleted = 0;

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

            -- Xóa các dòng giao dịch kho cũ thuộc phiếu xuất này
            DELETE FROM KHO_GiaoDichKho 
            WHERE LoaiChungTu = 2 
              AND (SoChungTu = @shipSoChungTu OR IDChiTietKho IN (SELECT ID FROM KHO_PhieuXuat_ChiTiet WHERE IDPhieuXuat = @shipId));

            -- Xóa các chi tiết phiếu xuất cũ
            DELETE FROM KHO_PhieuXuat_ChiTiet WHERE IDPhieuXuat = @shipId;

            -- Chèn lại chi tiết phiếu xuất mới
            INSERT INTO KHO_PhieuXuat_ChiTiet
                (IDPhieuXuat, IDSanPham, STT, SoLuong, DonGia, ThanhTien, ThueGTGT, TienThue, TongSauThue)
            SELECT
                @shipId, IDSanPham, ROW_NUMBER() OVER(ORDER BY IDSanPham), SoLuong, DonGia, ThanhTien, ThueGTGT, ThanhTienThue, ThanhTienSauThue
            FROM @ChiTietMoi;

            -- Tái tạo giao dịch kho mới chuẩn xác 100% nếu phiếu xuất ở trạng thái Đã ghi = 2
            IF EXISTS (SELECT 1 FROM KHO_PhieuXuat WHERE ID = @shipId AND TrangThai = 2)
            BEGIN
                INSERT INTO KHO_GiaoDichKho (
                    NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, 
                    SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao, IsHuy
                )
                SELECT 
                    px.NgayXuat, 
                    px.SoChungTu, 
                    2 AS LoaiChungTu, 
                    pxct.ID AS IDChiTietKho, 
                    px.IDKho, 
                    pxct.IDSanPham, 
                    0 AS SoLuongNhap, 
                    pxct.SoLuong AS SoLuongXuat, 
                    pxct.DonGia, 
                    pxct.ThanhTien, 
                    ISNULL(px.GhiChu, N'Xuất kho tự động (Điều chỉnh đơn hàng)'), 
                    GETDATE(), 
                    @NguoiTao, 
                    0 AS IsHuy
                FROM KHO_PhieuXuat_ChiTiet pxct
                INNER JOIN KHO_PhieuXuat px ON pxct.IDPhieuXuat = px.ID
                WHERE px.ID = @shipId;
            END
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
