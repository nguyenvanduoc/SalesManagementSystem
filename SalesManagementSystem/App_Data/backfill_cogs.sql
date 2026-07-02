DECLARE @IDSanPham INT;
DECLARE @IDGiaoDich INT;
DECLARE @NgayChungTu DATETIME;
DECLARE @LoaiChungTu INT;
DECLARE @SoLuongNhap DECIMAL(18,2);
DECLARE @SoLuongXuat DECIMAL(18,2);
DECLARE @DonGia DECIMAL(18,2);
DECLARE @IDChiTietKho INT;
DECLARE @SoChungTu NVARCHAR(50);

DECLARE curSanPham CURSOR FOR 
SELECT ID FROM DM_SanPham;

OPEN curSanPham;
FETCH NEXT FROM curSanPham INTO @IDSanPham;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @TongSoLuong DECIMAL(18,2) = 0;
    DECLARE @TongGiaTri DECIMAL(18,2) = 0;

    DECLARE curGiaoDich CURSOR FOR 
    SELECT ID, NgayChungTu, LoaiChungTu, SoLuongNhap, SoLuongXuat, DonGia, IDChiTietKho, SoChungTu
    FROM KHO_GiaoDichKho
    WHERE IDSanPham = @IDSanPham
    ORDER BY NgayChungTu ASC, ID ASC;

    OPEN curGiaoDich;
    FETCH NEXT FROM curGiaoDich INTO @IDGiaoDich, @NgayChungTu, @LoaiChungTu, @SoLuongNhap, @SoLuongXuat, @DonGia, @IDChiTietKho, @SoChungTu;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @SoLuongNhap > 0
        BEGIN
            SET @TongSoLuong = @TongSoLuong + @SoLuongNhap;
            SET @TongGiaTri = @TongGiaTri + (@SoLuongNhap * @DonGia);
        END
        ELSE IF @SoLuongXuat > 0
        BEGIN
            DECLARE @DonGiaVon DECIMAL(18,2) = 0;
            DECLARE @ThanhTienVon DECIMAL(18,2) = 0;

            IF @TongSoLuong > 0
            BEGIN
                SET @DonGiaVon = CAST(@TongGiaTri / @TongSoLuong AS DECIMAL(18,2));
            END

            SET @ThanhTienVon = @DonGiaVon * @SoLuongXuat;

            -- Update KHO_GiaoDichKho
            UPDATE KHO_GiaoDichKho 
            SET DonGiaVon = @DonGiaVon, ThanhTienVon = @ThanhTienVon 
            WHERE ID = @IDGiaoDich;

            -- Update running totals
            SET @TongSoLuong = @TongSoLuong - @SoLuongXuat;
            SET @TongGiaTri = @TongGiaTri - @ThanhTienVon;

            -- Update KHO_PhieuXuat_ChiTiet
            IF @IDChiTietKho IS NOT NULL AND @IDChiTietKho > 0
            BEGIN
                UPDATE KHO_PhieuXuat_ChiTiet 
                SET DonGiaVon = @DonGiaVon, ThanhTienVon = @ThanhTienVon 
                WHERE ID = @IDChiTietKho;
            END

            -- Update BAN_ChungTuBanHang_ChiTiet if this is a sales delivery
            IF @LoaiChungTu = 2 -- 2 is Xuất Bán in this system (based on ChungTuBanHangRepository)
            BEGIN
                -- Find the invoice ID via KHO_PhieuXuat
                DECLARE @IDChungTuBanHang INT;
                SELECT @IDChungTuBanHang = IDChungTuBanHang FROM KHO_PhieuXuat WHERE SoChungTu = @SoChungTu;
                
                IF @IDChungTuBanHang IS NOT NULL
                BEGIN
                    UPDATE BAN_ChungTuBanHang_ChiTiet
                    SET DonGiaVon = @DonGiaVon, ThanhTienVon = @ThanhTienVon
                    WHERE IDChungTuBanHang = @IDChungTuBanHang AND IDSanPham = @IDSanPham;
                END
            END
        END

        FETCH NEXT FROM curGiaoDich INTO @IDGiaoDich, @NgayChungTu, @LoaiChungTu, @SoLuongNhap, @SoLuongXuat, @DonGia, @IDChiTietKho, @SoChungTu;
    END

    CLOSE curGiaoDich;
    DEALLOCATE curGiaoDich;

    FETCH NEXT FROM curSanPham INTO @IDSanPham;
END

CLOSE curSanPham;
DEALLOCATE curSanPham;

-- Also update KT_NhatKyChung to reflect the exact COGS sum for each invoice
DECLARE @IDInvoice INT;
DECLARE @SoCT NVARCHAR(50);
DECLARE @NgayCT DATETIME;
DECLARE @TotalCOGS DECIMAL(18,2);

DECLARE curInvoices CURSOR FOR
SELECT ID, SoChungTu, NgayChungTu
FROM BAN_ChungTuBanHang
WHERE TrangThai = 2; -- Đã ghi sổ

OPEN curInvoices;
FETCH NEXT FROM curInvoices INTO @IDInvoice, @SoCT, @NgayCT;

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @TotalCOGS = SUM(ISNULL(ThanhTienVon, 0)) 
    FROM BAN_ChungTuBanHang_ChiTiet 
    WHERE IDChungTuBanHang = @IDInvoice;

    IF @TotalCOGS > 0
    BEGIN
        -- Xóa bút toán giá vốn cũ
        DELETE FROM KT_NhatKyChung 
        WHERE IDChungTu = @IDInvoice AND LoaiChungTu = 'BAN' AND TaiKhoanNo = '632';

        -- Thêm lại bút toán giá vốn mới
        INSERT INTO KT_NhatKyChung (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy)
        VALUES (@NgayCT, @SoCT, 'BAN', @IDInvoice, '632', '156', @TotalCOGS, N'Giá vốn hàng bán CT ' + @SoCT, GETDATE(), 1, 0);
    END

    FETCH NEXT FROM curInvoices INTO @IDInvoice, @SoCT, @NgayCT;
END

CLOSE curInvoices;
DEALLOCATE curInvoices;
