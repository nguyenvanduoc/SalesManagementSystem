CREATE PROCEDURE sp_KHO_HaoHutHangHoa_Huy
    @ID INT,
    @NguoiCapNhat INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;

        DECLARE @TrangThai INT, @LoaiHaoHut INT, @IDKho INT, @IDDonHang INT, @IDChungTuBanHang INT;
        SELECT @TrangThai = TrangThai, @LoaiHaoHut = LoaiHaoHut, @IDKho = IDKho, 
               @IDDonHang = IDDonHang, @IDChungTuBanHang = IDChungTuBanHang
        FROM KHO_HaoHutHangHoa WHERE ID = @ID;

        IF @TrangThai <> 2
        BEGIN
            RAISERROR(N'Chỉ có thể hủy phiếu đã ghi nhận.', 16, 1);
            RETURN;
        END

        UPDATE KHO_HaoHutHangHoa
        SET TrangThai = 3,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @NguoiCapNhat
        WHERE ID = @ID;

        IF @LoaiHaoHut = 1 -- Hao hụt bán hàng
        BEGIN
            -- Hoàn lại số lượng cho NS_DonDatHangChiTiet
            UPDATE dt
            SET 
                dt.SoLuong = dt.SoLuong + ht.SoLuongHaoHut,
                dt.ThanhTien = (dt.SoLuong + ht.SoLuongHaoHut) * dt.DonGia,
                dt.ThanhTienBocXep = (dt.SoLuong + ht.SoLuongHaoHut) * dt.DonGiaBocXep,
                dt.ThanhTienHang = ((dt.SoLuong + ht.SoLuongHaoHut) * dt.DonGia) + ((dt.SoLuong + ht.SoLuongHaoHut) * dt.DonGiaBocXep),
                dt.ThanhTienThue = (((dt.SoLuong + ht.SoLuongHaoHut) * dt.DonGia) + ((dt.SoLuong + ht.SoLuongHaoHut) * dt.DonGiaBocXep)) * ISNULL(dt.ThueGTGT, 0) / 100,
                dt.ThanhTienSauThue = (((dt.SoLuong + ht.SoLuongHaoHut) * dt.DonGia) + ((dt.SoLuong + ht.SoLuongHaoHut) * dt.DonGiaBocXep)) * (1 + ISNULL(dt.ThueGTGT, 0) / 100)
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
                IsHaoHut = CASE WHEN (SELECT COUNT(*) FROM KHO_HaoHutHangHoa WHERE IDDonHang = @IDDonHang AND TrangThai = 2) > 0 THEN 1 ELSE 0 END
            WHERE ID = @IDDonHang;

            -- Hoàn lại BAN_ChungTuBanHang_ChiTiet
            UPDATE ct
            SET 
                ct.SoLuong = ct.SoLuong + ht.SoLuongHaoHut,
                ct.ThanhTien = (ct.SoLuong + ht.SoLuongHaoHut) * ct.DonGia,
                ct.ThanhTienVon = (ct.SoLuong + ht.SoLuongHaoHut) * ISNULL(ct.DonGiaVon, 0),
                ct.ThanhTienBocXep = (ct.SoLuong + ht.SoLuongHaoHut) * ISNULL(ct.DonGiaBocXep, 0),
                ct.ThanhTienHang = ((ct.SoLuong + ht.SoLuongHaoHut) * ct.DonGia) + ((ct.SoLuong + ht.SoLuongHaoHut) * ISNULL(ct.DonGiaBocXep, 0)),
                ct.TienThue = (((ct.SoLuong + ht.SoLuongHaoHut) * ct.DonGia) + ((ct.SoLuong + ht.SoLuongHaoHut) * ISNULL(ct.DonGiaBocXep, 0))) * ISNULL(ct.ThueGTGT, 0) / 100,
                ct.TongSauThue = (((ct.SoLuong + ht.SoLuongHaoHut) * ct.DonGia) + ((ct.SoLuong + ht.SoLuongHaoHut) * ISNULL(ct.DonGiaBocXep, 0))) * (1 + ISNULL(ct.ThueGTGT, 0) / 100)
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
            -- Hủy các giao dịch kho
            UPDATE KHO_GiaoDichKho
            SET IsHuy = 1,
                NgayHuy = GETDATE(),
                NguoiHuy = @NguoiCapNhat,
                LyDoHuy = N'Hủy phiếu hao hụt'
            WHERE IDHaoHut = @ID;
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

CREATE OR ALTER PROCEDURE sp_KHO_HaoHutHangHoa_GetDonHang
    @Keyword NVARCHAR(100)
AS
BEGIN
    SELECT TOP 50 
           d.ID, d.SoDonHang, d.NgayTaoDon, d.IDKhachHang, k.TenKhachHang, 
           c.ID AS IDChungTuBanHang, c.SoChungTu AS SoChungTuBanHang
    FROM NS_DonDatHang d
    INNER JOIN NS_KhachHang k ON d.IDKhachHang = k.ID
    LEFT JOIN BAN_ChungTuBanHang c ON d.ID = c.IDDonDatHang AND c.IsDeleted = 0 AND c.TrangThai IN (1, 2)
    WHERE d.TrangThaiDon NOT IN (0, 4) -- Tất cả trạng thái trừ Lưu nháp (0) và Hủy (4)
      AND (@Keyword IS NULL OR @Keyword = '' OR d.SoDonHang LIKE '%' + @Keyword + '%' OR k.TenKhachHang LIKE N'%' + @Keyword + '%')
    ORDER BY d.NgayTaoDon DESC, d.ID DESC;
END
GO

CREATE PROCEDURE sp_KHO_HaoHutHangHoa_GetChiTietDonHang
    @IDDonHang INT
AS
BEGIN
    SELECT dt.IDSanPham, s.MaSanPham, s.TenSanPham, 
           dt.SoLuong AS SoLuongHienTai, dt.DonGia AS DonGiaBan,
           -- Lấy giá vốn nhập gần nhất
           ISNULL((SELECT TOP 1 DonGia FROM KHO_GiaoDichKho WHERE IDSanPham = dt.IDSanPham AND SoLuongNhap > 0 AND IsHuy = 0 ORDER BY NgayChungTu DESC, ID DESC), 0) AS DonGiaHaoHut
    FROM NS_DonDatHangChiTiet dt
    INNER JOIN DM_SanPham s ON dt.IDSanPham = s.ID
    WHERE dt.IDDonDatHang = @IDDonHang AND dt.SoLuong > 0;
END
GO

CREATE PROCEDURE sp_KHO_HaoHutHangHoa_GetTonKho
    @IDKho INT,
    @IDSanPham INT
AS
BEGIN
    SELECT ISNULL(SUM(SoLuongNhap - SoLuongXuat), 0) AS SoLuongTon
    FROM KHO_GiaoDichKho
    WHERE IDKho = @IDKho AND IDSanPham = @IDSanPham AND IsHuy = 0;
END
GO

CREATE PROCEDURE sp_KHO_HaoHutHangHoa_GetGiaNhapGanNhat
    @IDSanPham INT
AS
BEGIN
    SELECT TOP 1 DonGia
    FROM KHO_GiaoDichKho
    WHERE IDSanPham = @IDSanPham AND SoLuongNhap > 0 AND IsHuy = 0
    ORDER BY NgayChungTu DESC, ID DESC;
END
GO
