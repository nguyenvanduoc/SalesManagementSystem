CREATE PROCEDURE sp_KHO_HaoHutHangHoa_GhiNhan
    @ID INT,
    @NguoiCapNhat INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;

        DECLARE @TrangThai INT, @LoaiHaoHut INT, @IDKho INT, @IDDonHang INT, @IDChungTuBanHang INT, @NgayHaoHut DATETIME, @SoChungTu NVARCHAR(50);
        SELECT @TrangThai = TrangThai, @LoaiHaoHut = LoaiHaoHut, @IDKho = IDKho, 
               @IDDonHang = IDDonHang, @IDChungTuBanHang = IDChungTuBanHang, 
               @NgayHaoHut = NgayHaoHut, @SoChungTu = SoChungTu
        FROM KHO_HaoHutHangHoa WHERE ID = @ID;

        IF @TrangThai <> 1
        BEGIN
            RAISERROR(N'Phiếu hao hụt đã được ghi nhận hoặc đã hủy.', 16, 1);
            RETURN;
        END

        -- Cập nhật tổng số lượng và tiền vào Header
        UPDATE KHO_HaoHutHangHoa
        SET TongSoLuong = (SELECT ISNULL(SUM(SoLuongHaoHut), 0) FROM KHO_HaoHutHangHoa_ChiTiet WHERE IDHaoHut = @ID),
            TongTienHaoHut = (SELECT ISNULL(SUM(TienHaoHut), 0) FROM KHO_HaoHutHangHoa_ChiTiet WHERE IDHaoHut = @ID),
            TrangThai = 2,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @NguoiCapNhat
        WHERE ID = @ID;

        IF @LoaiHaoHut = 1 -- Hao hụt bán hàng
        BEGIN
            IF @IDDonHang IS NULL OR @IDChungTuBanHang IS NULL
            BEGIN
                RAISERROR(N'Thiếu thông tin đơn hàng hoặc chứng từ bán hàng.', 16, 1);
                RETURN;
            END

            -- Cập nhật NS_DonDatHangChiTiet
            UPDATE dt
            SET 
                dt.SoLuong = dt.SoLuong - ht.SoLuongHaoHut,
                dt.ThanhTien = (dt.SoLuong - ht.SoLuongHaoHut) * dt.DonGia,
                dt.ThanhTienBocXep = (dt.SoLuong - ht.SoLuongHaoHut) * dt.DonGiaBocXep,
                dt.ThanhTienHang = ((dt.SoLuong - ht.SoLuongHaoHut) * dt.DonGia) + ((dt.SoLuong - ht.SoLuongHaoHut) * dt.DonGiaBocXep),
                dt.ThanhTienThue = (((dt.SoLuong - ht.SoLuongHaoHut) * dt.DonGia) + ((dt.SoLuong - ht.SoLuongHaoHut) * dt.DonGiaBocXep)) * ISNULL(dt.ThueGTGT, 0) / 100,
                dt.ThanhTienSauThue = (((dt.SoLuong - ht.SoLuongHaoHut) * dt.DonGia) + ((dt.SoLuong - ht.SoLuongHaoHut) * dt.DonGiaBocXep)) * (1 + ISNULL(dt.ThueGTGT, 0) / 100)
            FROM NS_DonDatHangChiTiet dt
            INNER JOIN KHO_HaoHutHangHoa_ChiTiet ht ON dt.IDSanPham = ht.IDSanPham AND ht.IDHaoHut = @ID
            WHERE dt.IDDonDatHang = @IDDonHang;

            -- Cập nhật NS_DonDatHang Header
            UPDATE NS_DonDatHang
            SET 
                PhiBocXep = (SELECT ISNULL(SUM(ThanhTienBocXep), 0) FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @IDDonHang),
                ThanhTienHang = (SELECT ISNULL(SUM(ThanhTienHang), 0) FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @IDDonHang),
                ThanhTienThue = (SELECT ISNULL(SUM(ThanhTienThue), 0) FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @IDDonHang),
                TongTien = (SELECT ISNULL(SUM(ThanhTienHang + ThanhTienThue), 0) FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @IDDonHang) + ISNULL(TongTienVanChuyen, 0),
                IsHaoHut = 1
            WHERE ID = @IDDonHang;

            -- Cập nhật BAN_ChungTuBanHang_ChiTiet
            UPDATE ct
            SET 
                ct.SoLuong = ct.SoLuong - ht.SoLuongHaoHut,
                ct.ThanhTien = (ct.SoLuong - ht.SoLuongHaoHut) * ct.DonGia,
                ct.ThanhTienVon = (ct.SoLuong - ht.SoLuongHaoHut) * ISNULL(ct.DonGiaVon, 0),
                ct.ThanhTienBocXep = (ct.SoLuong - ht.SoLuongHaoHut) * ISNULL(ct.DonGiaBocXep, 0),
                ct.ThanhTienHang = ((ct.SoLuong - ht.SoLuongHaoHut) * ct.DonGia) + ((ct.SoLuong - ht.SoLuongHaoHut) * ISNULL(ct.DonGiaBocXep, 0)),
                ct.TienThue = (((ct.SoLuong - ht.SoLuongHaoHut) * ct.DonGia) + ((ct.SoLuong - ht.SoLuongHaoHut) * ISNULL(ct.DonGiaBocXep, 0))) * ISNULL(ct.ThueGTGT, 0) / 100,
                ct.TongSauThue = (((ct.SoLuong - ht.SoLuongHaoHut) * ct.DonGia) + ((ct.SoLuong - ht.SoLuongHaoHut) * ISNULL(ct.DonGiaBocXep, 0))) * (1 + ISNULL(ct.ThueGTGT, 0) / 100)
            FROM BAN_ChungTuBanHang_ChiTiet ct
            INNER JOIN KHO_HaoHutHangHoa_ChiTiet ht ON ct.IDSanPham = ht.IDSanPham AND ht.IDHaoHut = @ID
            WHERE ct.IDChungTuBanHang = @IDChungTuBanHang;

            -- Cập nhật BAN_ChungTuBanHang Header
            UPDATE BAN_ChungTuBanHang
            SET 
                PhiBocXep = (SELECT ISNULL(SUM(ThanhTienBocXep), 0) FROM BAN_ChungTuBanHang_ChiTiet WHERE IDChungTuBanHang = @IDChungTuBanHang),
                TongTienHang = (SELECT ISNULL(SUM(ThanhTienHang), 0) FROM BAN_ChungTuBanHang_ChiTiet WHERE IDChungTuBanHang = @IDChungTuBanHang),
                TongTienThue = (SELECT ISNULL(SUM(TienThue), 0) FROM BAN_ChungTuBanHang_ChiTiet WHERE IDChungTuBanHang = @IDChungTuBanHang),
                TongCong = (SELECT ISNULL(SUM(ThanhTienHang + TienThue), 0) FROM BAN_ChungTuBanHang_ChiTiet WHERE IDChungTuBanHang = @IDChungTuBanHang),
                ConLai = (SELECT ISNULL(SUM(ThanhTienHang + TienThue), 0) FROM BAN_ChungTuBanHang_ChiTiet WHERE IDChungTuBanHang = @IDChungTuBanHang) - ISNULL(DaThanhToan, 0)
            WHERE ID = @IDChungTuBanHang;

        END
        ELSE IF @LoaiHaoHut = 2 -- Hao hụt tồn kho
        BEGIN
            IF @IDKho IS NULL
            BEGIN
                RAISERROR(N'Thiếu thông tin kho.', 16, 1);
                RETURN;
            END

            -- Insert vào KHO_GiaoDichKho
            INSERT INTO KHO_GiaoDichKho (NgayChungTu, SoChungTu, LoaiChungTu, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao, IsHuy, IDHaoHut)
            SELECT @NgayHaoHut, @SoChungTu, 4, @IDKho, ht.IDSanPham, 0, ht.SoLuongHaoHut, ht.DonGiaHaoHut, ht.TienHaoHut, N'Ghi nhận xuất hao hụt', GETDATE(), @NguoiCapNhat, 0, @ID
            FROM KHO_HaoHutHangHoa_ChiTiet ht
            WHERE ht.IDHaoHut = @ID;
        END

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
    END CATCH
END
GO
