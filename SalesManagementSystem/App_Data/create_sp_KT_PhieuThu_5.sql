IF OBJECT_ID('sp_KT_PhieuThu_Huy', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThu_Huy;
GO
CREATE PROCEDURE sp_KT_PhieuThu_Huy
    @ID INT,
    @NguoiHuy INT,
    @LyDoHuy NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TrangThai INT, @SoPhieu VARCHAR(50);
    SELECT @TrangThai = TrangThai, @SoPhieu = SoPhieuThu FROM KT_PhieuThu WHERE ID = @ID;
    
    IF @TrangThai != 2
    BEGIN
        RAISERROR(N'Chỉ có thể hủy phiếu thu đã ghi sổ.', 16, 1);
        RETURN;
    END
    
    -- Cập nhật trạng thái phiếu thu
    UPDATE KT_PhieuThu 
    SET TrangThai = 3, NguoiCapNhat = @NguoiHuy, NgayCapNhat = GETDATE(), DienGiai = ISNULL(DienGiai,'') + N' [Hủy: ' + @LyDoHuy + ']'
    WHERE ID = @ID;
    
    -- Hủy sổ quỹ
    UPDATE QUY_GiaoDichTien
    SET IsHuy = 1, NgayHuy = GETDATE(), NguoiHuy = @NguoiHuy, LyDoHuy = @LyDoHuy
    WHERE LoaiChungTu = 'PHIEU_THU_KHACH_HANG' AND IDChungTu = @ID AND ISNULL(IsHuy, 0) = 0;

    -- Hoàn lại BAN_ChungTuBanHang
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
        SET DaThanhToan = ISNULL(DaThanhToan, 0) - @SoTienPhanBo,
            ConLai = TongCong - (ISNULL(DaThanhToan, 0) - @SoTienPhanBo)
        WHERE ID = @IDChungTuBanHang;
        
        FETCH NEXT FROM cur INTO @IDChungTuBanHang, @SoTienPhanBo;
    END
    
    CLOSE cur;
    DEALLOCATE cur;
END
GO
