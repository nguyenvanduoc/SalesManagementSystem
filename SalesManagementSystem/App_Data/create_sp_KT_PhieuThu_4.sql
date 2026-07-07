IF OBJECT_ID('sp_KT_PhieuThu_GhiSo', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThu_GhiSo;
GO
CREATE PROCEDURE sp_KT_PhieuThu_GhiSo
    @ID INT,
    @NguoiGhi INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TrangThai INT, @SoPhieu VARCHAR(50), @NgayThu DATE, @IDTaiKhoanThanhToan INT;
    SELECT @TrangThai = TrangThai, @SoPhieu = SoPhieuThu, @NgayThu = NgayThu, @IDTaiKhoanThanhToan = IDTaiKhoanThanhToan FROM KT_PhieuThu WHERE ID = @ID;
    
    IF @TrangThai != 1
    BEGIN
        RAISERROR(N'Chỉ có thể ghi sổ phiếu thu ở trạng thái mới.', 16, 1);
        RETURN;
    END
    
    -- Cập nhật trạng thái phiếu thu
    UPDATE KT_PhieuThu 
    SET TrangThai = 2, NguoiCapNhat = @NguoiGhi, NgayCapNhat = GETDATE()
    WHERE ID = @ID;
    
    -- Ghi sổ quỹ
    INSERT INTO QUY_GiaoDichTien (NgayGiaoDich, SoChungTu, LoaiChungTu, IDChungTu, IDTaiKhoanThanhToan, SoTienThu, SoTienChi, DienGiai, NguoiTao)
    SELECT @NgayThu, @SoPhieu, 'PHIEU_THU_KHACH_HANG', @ID, @IDTaiKhoanThanhToan, SoTienThu, 0, DienGiai, @NguoiGhi
    FROM KT_PhieuThu
    WHERE ID = @ID;

    -- Cập nhật BAN_ChungTuBanHang
    -- Đối với mỗi chi tiết phiếu thu, cập nhật DaThanhToan và ConLai của BAN_ChungTuBanHang
    DECLARE @IDChungTuBanHang INT, @SoTienPhanBo DECIMAL(18,0);
    
    DECLARE cur CURSOR FOR 
    SELECT IDChungTuBanHang, SUM(SoTienPhanBo)
    FROM KT_PhieuThuChiTiet
    WHERE IDPhieuThu = @ID AND LoaiThu = 1 AND IDChungTuBanHang IS NOT NULL
    GROUP BY IDChungTuBanHang;
    
    OPEN cur;
    FETCH NEXT FROM cur INTO @IDChungTuBanHang, @SoTienPhanBo;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        UPDATE BAN_ChungTuBanHang
        SET DaThanhToan = ISNULL(DaThanhToan, 0) + @SoTienPhanBo,
            ConLai = TongCong - (ISNULL(DaThanhToan, 0) + @SoTienPhanBo)
        WHERE ID = @IDChungTuBanHang;
        
        FETCH NEXT FROM cur INTO @IDChungTuBanHang, @SoTienPhanBo;
    END
    
    CLOSE cur;
    DEALLOCATE cur;
END
GO
