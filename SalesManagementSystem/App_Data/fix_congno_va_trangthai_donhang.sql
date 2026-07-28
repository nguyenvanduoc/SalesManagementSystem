-- ==============================================================================
-- Script: fix_congno_va_trangthai_donhang.sql
-- Description:
-- 1. Cập nhật lại sai lệch dữ liệu trường ConLai của 2 chứng từ bán hàng
-- 2. Cập nhật logic đồng bộ trạng thái đơn hàng (NS_DonDatHang) vào sp_KT_PhieuThu_GhiSo
-- 3. Cập nhật logic đồng bộ trạng thái đơn hàng (NS_DonDatHang) vào sp_KT_PhieuThu_Huy
-- ==============================================================================

-- ==============================================================================
-- PHẦN 1: SỬA LỖI LỆCH DỮ LIỆU
-- ==============================================================================

-- Sửa chứng từ BH00003 (Đơn hàng DH26000001) bị mất giá trị ConLai
UPDATE BAN_ChungTuBanHang
SET ConLai = 565800000
WHERE SoChungTu = 'BH00003' AND ConLai = 0 AND DaThanhToan = 0;

-- Sửa chứng từ BH00025 (Đơn hàng DH26000023) bị đánh dấu đã thanh toán nhầm (nếu có)
-- Lưu ý: Bạn chỉ chạy đoạn này nếu chứng từ BH00025 thực sự chưa thanh toán.
UPDATE BAN_ChungTuBanHang
SET DaThanhToan = 0,
    ConLai = TongCong
WHERE SoChungTu = 'BH00025' AND DaThanhToan = 533025000;

GO

-- ==============================================================================
-- PHẦN 2: THÊM LOGIC CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG VÀO SP GHI SỔ PHIẾU THU
-- ==============================================================================
CREATE OR ALTER PROCEDURE sp_KT_PhieuThu_GhiSo
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

    -- Cập nhật BAN_ChungTuBanHang và NS_DonDatHang
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
        -- Cập nhật công nợ chứng từ bán hàng
        UPDATE BAN_ChungTuBanHang
        SET DaThanhToan = ISNULL(DaThanhToan, 0) + @SoTienPhanBo,
            ConLai = TongCong - (ISNULL(DaThanhToan, 0) + @SoTienPhanBo)
        WHERE ID = @IDChungTuBanHang;
        
        -- Cập nhật trạng thái Đơn đặt hàng tương ứng
        DECLARE @NewConLai DECIMAL(18,0), @NewDaThanhToan DECIMAL(18,0), @IDDonDatHang INT;
        SELECT @NewConLai = ConLai, @NewDaThanhToan = DaThanhToan, @IDDonDatHang = IDDonDatHang 
        FROM BAN_ChungTuBanHang WHERE ID = @IDChungTuBanHang;

        IF ISNULL(@IDDonDatHang, 0) > 0
        BEGIN
            DECLARE @TrangThaiDon INT;
            IF @NewConLai <= 0
                SET @TrangThaiDon = 7; -- Đã thanh toán
            ELSE IF @NewDaThanhToan > 0
                SET @TrangThaiDon = 8; -- Thanh toán một phần
            ELSE
                SET @TrangThaiDon = 3; -- Vẫn là Đã lập chứng từ

            UPDATE NS_DonDatHang
            SET TrangThaiDon = @TrangThaiDon,
                NguoiCapNhat = @NguoiGhi,
                NgayCapNhat = GETDATE()
            WHERE ID = @IDDonDatHang;
        END

        FETCH NEXT FROM cur INTO @IDChungTuBanHang, @SoTienPhanBo;
    END
    
    CLOSE cur;
    DEALLOCATE cur;
END
GO

-- ==============================================================================
-- PHẦN 3: THÊM LOGIC CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG VÀO SP HỦY GHI SỔ PHIẾU THU
-- ==============================================================================
CREATE OR ALTER PROCEDURE sp_KT_PhieuThu_Huy
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

    -- Hoàn lại BAN_ChungTuBanHang và xử lý trạng thái Đơn đặt hàng
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
        
        DECLARE @NewConLai DECIMAL(18,0), @NewDaThanhToan DECIMAL(18,0), @IDDonDatHang INT;
        SELECT @NewConLai = ConLai, @NewDaThanhToan = DaThanhToan, @IDDonDatHang = IDDonDatHang 
        FROM BAN_ChungTuBanHang WHERE ID = @IDChungTuBanHang;

        IF ISNULL(@IDDonDatHang, 0) > 0
        BEGIN
            DECLARE @TrangThaiDon INT;
            IF @NewConLai <= 0
                SET @TrangThaiDon = 7; -- Đã thanh toán
            ELSE IF @NewDaThanhToan > 0
                SET @TrangThaiDon = 8; -- Thanh toán một phần
            ELSE
            BEGIN
                -- Kiểm tra trạng thái hiện tại của đơn hàng (nếu đang ở các trạng thái giao hàng)
                IF EXISTS (SELECT 1 FROM KHO_PhieuXuat WHERE IDDonDatHang = @IDDonDatHang AND TrangThai = 2 AND IsDeleted = 0)
                    SET @TrangThaiDon = 6; -- Đã giao
                ELSE IF EXISTS (SELECT 1 FROM KHO_PhieuXuat WHERE IDDonDatHang = @IDDonDatHang AND TrangThai = 1 AND IsDeleted = 0)
                    SET @TrangThaiDon = 5; -- Đang đi đường
                ELSE
                    SET @TrangThaiDon = 3; -- Trở về Đã lập chứng từ
            END

            UPDATE NS_DonDatHang
            SET TrangThaiDon = @TrangThaiDon,
                NguoiCapNhat = @NguoiHuy,
                NgayCapNhat = GETDATE()
            WHERE ID = @IDDonDatHang;
        END

        FETCH NEXT FROM cur INTO @IDChungTuBanHang, @SoTienPhanBo;
    END
    
    CLOSE cur;
    DEALLOCATE cur;
END
GO
