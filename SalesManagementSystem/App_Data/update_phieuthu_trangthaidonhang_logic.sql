-- ==============================================================================
-- Script: update_phieuthu_trangthaidonhang_logic.sql
-- Description: 
-- 1. Tạo sp_BAN_CapNhatTrangThaiThanhToanDonHang để tính lại công nợ chứng từ và cập nhật trạng thái đơn hàng.
-- 2. Cập nhật sp_BAN_PhieuThuKhachHang_Ghi gọi SP chung.
-- 3. Cập nhật sp_BAN_PhieuThuKhachHang_Huy gọi SP chung.
-- ==============================================================================

-- 1. sp_BAN_CapNhatTrangThaiThanhToanDonHang
CREATE OR ALTER PROCEDURE sp_BAN_CapNhatTrangThaiThanhToanDonHang
    @IDChungTuBanHang INT,
    @NguoiCapNhat INT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Tính tổng đã thanh toán
    DECLARE @DaThanhToan DECIMAL(18,2) = 0;
    SELECT @DaThanhToan = ISNULL(SUM(SoTienThu), 0)
    FROM BAN_PhieuThuKhachHang
    WHERE IDChungTuBanHang = @IDChungTuBanHang 
      AND TrangThai = 2 
      AND IsDeleted = 0;

    -- 2. Tính còn lại
    DECLARE @TongCong DECIMAL(18,2) = 0;
    DECLARE @IDDonDatHang INT;
    
    SELECT @TongCong = TongCong, @IDDonDatHang = IDDonDatHang
    FROM BAN_ChungTuBanHang
    WHERE ID = @IDChungTuBanHang;

    DECLARE @ConLai DECIMAL(18,2) = @TongCong - @DaThanhToan;

    IF @ConLai < 0
    BEGIN
        THROW 50000, N'Số tiền thu vượt quá số tiền còn phải thu của chứng từ.', 1;
    END

    -- 3 & 4. Cập nhật chứng từ bán hàng
    UPDATE BAN_ChungTuBanHang
    SET DaThanhToan = @DaThanhToan,
        ConLai = @ConLai
    WHERE ID = @IDChungTuBanHang;

    -- 5 & 6. Cập nhật trạng thái đơn hàng
    IF @IDDonDatHang IS NOT NULL
    BEGIN
        DECLARE @TrangThaiHienTai INT;
        SELECT @TrangThaiHienTai = TrangThaiDon FROM NS_DonDatHang WHERE ID = @IDDonDatHang;

        DECLARE @TrangThaiMoi INT = @TrangThaiHienTai;

        IF @ConLai <= 0
        BEGIN
            SET @TrangThaiMoi = 7; -- Đã thanh toán
        END
        ELSE IF @DaThanhToan > 0 AND @ConLai > 0
        BEGIN
            SET @TrangThaiMoi = 8; -- Thanh toán một phần
        END
        ELSE IF @DaThanhToan = 0
        BEGIN
            -- Fallback
            -- Chỉ đổi nếu đang là 7 hoặc 8 (tức là vừa hủy hết thanh toán)
            IF @TrangThaiHienTai IN (7, 8)
            BEGIN
                -- Thử tìm trạng thái gần nhất
                IF EXISTS (SELECT 1 FROM KHO_PhieuXuat WHERE IDDonDatHang = @IDDonDatHang AND TrangThai = 2 AND IsDeleted = 0)
                BEGIN
                    SET @TrangThaiMoi = 6; -- Đã giao
                END
                ELSE IF EXISTS (SELECT 1 FROM KHO_PhieuXuat WHERE IDDonDatHang = @IDDonDatHang AND TrangThai = 1 AND IsDeleted = 0)
                BEGIN
                    SET @TrangThaiMoi = 5; -- Đang đi đường
                END
                ELSE
                BEGIN
                    SET @TrangThaiMoi = 3; -- Đã lập chứng từ
                END
            END
        END

        IF @TrangThaiHienTai <> @TrangThaiMoi
        BEGIN
            UPDATE NS_DonDatHang
            SET TrangThaiDon = @TrangThaiMoi,
                NgayCapNhat = GETDATE(),
                NguoiCapNhat = @NguoiCapNhat
            WHERE ID = @IDDonDatHang;
        END
    END
END
GO

-- 2. Cập nhật sp_BAN_PhieuThuKhachHang_Ghi
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_Ghi
    @ID INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Lấy thông tin phiếu thu
        DECLARE @IDChungTuBanHang INT, @IDTaiKhoanThanhToan INT, @SoTienThu DECIMAL(18,2), @SoPhieuThu NVARCHAR(50), @NgayThu DATE;
        DECLARE @TrangThai INT;

        SELECT 
            @IDChungTuBanHang = IDChungTuBanHang,
            @IDTaiKhoanThanhToan = IDTaiKhoanThanhToan,
            @SoTienThu = SoTienThu,
            @SoPhieuThu = SoPhieuThu,
            @NgayThu = NgayThu,
            @TrangThai = TrangThai
        FROM BAN_PhieuThuKhachHang
        WHERE ID = @ID AND IsDeleted = 0;

        IF @IDChungTuBanHang IS NULL THROW 50001, N'Phiếu thu không tồn tại.', 1;
        IF @TrangThai <> 1 THROW 50002, N'Trạng thái phiếu thu không hợp lệ (phải là Đề nghị ghi).', 1;

        -- Lấy số tài khoản thanh toán
        DECLARE @TaiKhoanNo NVARCHAR(50);
        SELECT @TaiKhoanNo = SoTaiKhoan FROM DM_TaiKhoanThanhToan WHERE ID = @IDTaiKhoanThanhToan;
        IF @TaiKhoanNo IS NULL THROW 50003, N'Tài khoản thanh toán không tồn tại.', 1;

        -- Kiểm tra chứng từ bán hàng
        DECLARE @TrangThaiCTBH INT, @SoChungTuBanHang NVARCHAR(50);
        SELECT @TrangThaiCTBH = TrangThai, @SoChungTuBanHang = SoChungTu
        FROM BAN_ChungTuBanHang 
        WHERE ID = @IDChungTuBanHang AND IsDeleted = 0;

        IF @TrangThaiCTBH IS NULL THROW 50004, N'Chứng từ bán hàng không tồn tại.', 1;
        IF @TrangThaiCTBH <> 2 THROW 50005, N'Chứng từ bán hàng chưa được ghi sổ.', 1;

        -- Cập nhật trạng thái phiếu thu
        UPDATE BAN_PhieuThuKhachHang
        SET TrangThai = 2,
            NgayGhi = GETDATE(),
            NguoiGhi = @UserId,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @UserId
        WHERE ID = @ID;

        -- Tính lại công nợ và trạng thái đơn hàng (sẽ chặn nếu ConLai < 0 trong store)
        EXEC sp_BAN_CapNhatTrangThaiThanhToanDonHang @IDChungTuBanHang = @IDChungTuBanHang, @NguoiCapNhat = @UserId;

        -- Ghi sổ kế toán KT_NhatKyChung
        IF NOT EXISTS (SELECT 1 FROM KT_NhatKyChung WHERE LoaiChungTu = 'PHIEU_THU' AND IDChungTu = @ID AND IsHuy = 0)
        BEGIN
            INSERT INTO KT_NhatKyChung (
                NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu, 
                TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao, IsHuy
            )
            VALUES (
                @NgayThu, @SoPhieuThu, 'PHIEU_THU', @ID,
                @TaiKhoanNo, '131', @SoTienThu, N'Thu tiền khách hàng theo chứng từ ' + @SoChungTuBanHang, 
                GETDATE(), @UserId, 0
            );
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- 3. Cập nhật sp_BAN_PhieuThuKhachHang_Huy
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_Huy
    @ID INT,
    @UserId INT,
    @LyDoHuy NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Lấy thông tin phiếu thu
        DECLARE @IDChungTuBanHang INT, @TrangThai INT;
        SELECT 
            @IDChungTuBanHang = IDChungTuBanHang,
            @TrangThai = TrangThai
        FROM BAN_PhieuThuKhachHang
        WHERE ID = @ID AND IsDeleted = 0;

        IF @TrangThai IS NULL THROW 50001, N'Phiếu thu không tồn tại.', 1;
        IF @TrangThai = 3 THROW 50002, N'Phiếu thu đã được hủy trước đó.', 1;

        -- Cập nhật trạng thái phiếu thu sang 3 (Đã hủy)
        UPDATE BAN_PhieuThuKhachHang
        SET TrangThai = 3,
            NgayHuy = GETDATE(),
            NguoiHuy = @UserId,
            LyDoHuy = @LyDoHuy,
            NgayCapNhat = GETDATE(),
            NguoiCapNhat = @UserId
        WHERE ID = @ID;

        IF @TrangThai = 2 -- Nếu đã ghi sổ thì hoàn sổ
        BEGIN
            -- Đánh dấu hủy sổ nhật ký chung
            UPDATE KT_NhatKyChung
            SET IsHuy = 1,
                NgayHuy = GETDATE(),
                NguoiHuy = @UserId,
                LyDoHuy = @LyDoHuy
            WHERE LoaiChungTu = 'PHIEU_THU' AND IDChungTu = @ID;

            -- Tính lại công nợ và cập nhật trạng thái đơn hàng
            EXEC sp_BAN_CapNhatTrangThaiThanhToanDonHang @IDChungTuBanHang = @IDChungTuBanHang, @NguoiCapNhat = @UserId;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
