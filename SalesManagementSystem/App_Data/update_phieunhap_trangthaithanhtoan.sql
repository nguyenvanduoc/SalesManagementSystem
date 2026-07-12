-- ==============================================================================
-- Script: update_phieuchi_payment_status_logic.sql
-- Description:
-- 1. Thêm các cột DaThanhToan, ConLai, TrangThaiThanhToan vào KHO_PhieuNhap.
-- 2. Khởi tạo dữ liệu thanh toán cho các phiếu nhập hiện tại.
-- 3. Tạo thủ tục sp_KHO_CapNhatTrangThaiThanhToanPhieuNhap.
-- 4. Cập nhật sp_KT_PhieuChi_GhiSo và sp_KT_PhieuChi_Huy để gọi SP cập nhật.
-- ==============================================================================

-- 1. Thêm các cột vào KHO_PhieuNhap nếu chưa có
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KHO_PhieuNhap' AND COLUMN_NAME = 'DaThanhToan')
BEGIN
    ALTER TABLE KHO_PhieuNhap ADD DaThanhToan DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT N'Đã thêm cột DaThanhToan vào KHO_PhieuNhap';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KHO_PhieuNhap' AND COLUMN_NAME = 'ConLai')
BEGIN
    ALTER TABLE KHO_PhieuNhap ADD ConLai DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT N'Đã thêm cột ConLai vào KHO_PhieuNhap';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KHO_PhieuNhap' AND COLUMN_NAME = 'TrangThaiThanhToan')
BEGIN
    ALTER TABLE KHO_PhieuNhap ADD TrangThaiThanhToan INT NOT NULL DEFAULT 0;
    PRINT N'Đã thêm cột TrangThaiThanhToan vào KHO_PhieuNhap';
END
GO

-- 2. Tạo thủ tục cập nhật trạng thái thanh toán của Phiếu Nhập Kho
CREATE OR ALTER PROCEDURE sp_KHO_CapNhatTrangThaiThanhToanPhieuNhap
    @IDPhieuNhap INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @IDPhieuNhap IS NOT NULL AND @IDPhieuNhap > 0
    BEGIN
        DECLARE @DaThanhToan DECIMAL(18,2) = 0;
        DECLARE @TongCong DECIMAL(18,2) = 0;

        -- Tính tổng số tiền chi của các phiếu chi đã ghi sổ (TrangThai = 2) và không bị xóa
        SELECT @DaThanhToan = ISNULL(SUM(ct.SoTienPhanBo), 0)
        FROM KT_PhieuChiChiTiet ct
        INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
        WHERE ct.IDPhieuNhap = @IDPhieuNhap 
          AND ct.LoaiChi = 1
          AND pc.TrangThai = 2 
          AND pc.IsDeleted = 0;

        -- Lấy tổng cộng của phiếu nhập kho
        SELECT @TongCong = ISNULL(TongCong, 0)
        FROM KHO_PhieuNhap
        WHERE ID = @IDPhieuNhap;

        -- Tính số tiền còn lại
        DECLARE @ConLai DECIMAL(18,2) = @TongCong - @DaThanhToan;

        -- Xác định trạng thái thanh toán: 0: Chưa thanh toán, 1: Thanh toán một phần, 2: Đã thanh toán
        DECLARE @TrangThaiThanhToan INT = 0;
        IF @DaThanhToan > 0
        BEGIN
            IF @ConLai <= 0
                SET @TrangThaiThanhToan = 2;
            ELSE
                SET @TrangThaiThanhToan = 1;
        END

        -- Cập nhật vào bảng KHO_PhieuNhap
        UPDATE KHO_PhieuNhap
        SET DaThanhToan = @DaThanhToan,
            ConLai = @ConLai,
            TrangThaiThanhToan = @TrangThaiThanhToan
        WHERE ID = @IDPhieuNhap;
        
        PRINT N'Đã cập nhật trạng thái thanh toán cho Phiếu nhập ID ' + CAST(@IDPhieuNhap AS VARCHAR(10)) + N': DaThanhToan=' + CAST(@DaThanhToan AS VARCHAR(20)) + N', ConLai=' + CAST(@ConLai AS VARCHAR(20)) + N', TrangThaiThanhToan=' + CAST(@TrangThaiThanhToan AS VARCHAR(5));
    END
END
GO

-- 3. Khởi tạo dữ liệu thanh toán ban đầu cho các phiếu nhập kho hiện tại
DECLARE @ID INT;
DECLARE cur CURSOR FOR SELECT ID FROM KHO_PhieuNhap WHERE IsDeleted = 0;
OPEN cur;
FETCH NEXT FROM cur INTO @ID;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC sp_KHO_CapNhatTrangThaiThanhToanPhieuNhap @IDPhieuNhap = @ID;
    FETCH NEXT FROM cur INTO @ID;
END
CLOSE cur;
DEALLOCATE cur;
GO

-- 4. Cập nhật sp_KT_PhieuChi_GhiSo
CREATE OR ALTER PROCEDURE sp_KT_PhieuChi_GhiSo
    @ID         INT,
    @NguoiGhi   INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @IDPhieuNhap INT;
        SELECT @IDPhieuNhap = IDPhieuNhap FROM KT_PhieuChi WHERE ID = @ID AND IsDeleted = 0;

        -- Cập nhật trạng thái phiếu chi thành Đã ghi sổ
        UPDATE KT_PhieuChi
        SET TrangThai    = 2,
            NgayGhi      = GETDATE(),
            NguoiGhi     = @NguoiGhi,
            NgayCapNhat  = GETDATE(),
            NguoiCapNhat = @NguoiGhi
        WHERE ID = @ID AND IsDeleted = 0 AND TrangThai = 1;

        -- Ghi vào Nhật Ký Chung
        DECLARE @SoPhieuChi         NVARCHAR(50);
        DECLARE @NgayChi            DATE;
        DECLARE @SoTienChi          DECIMAL(18,2);
        DECLARE @DienGiai           NVARCHAR(1000);
        DECLARE @SoTKThanhToan      NVARCHAR(20);
        DECLARE @IDTKKeToan         INT;

        SELECT
            @SoPhieuChi     = pc.SoPhieuChi,
            @NgayChi        = pc.NgayChi,
            @SoTienChi      = pc.SoTienChi,
            @DienGiai       = pc.DienGiai,
            @IDTKKeToan     = tk.IDTaiKhoanKeToan
        FROM KT_PhieuChi pc
        LEFT JOIN DM_TaiKhoanThanhToan tk ON pc.IDTaiKhoanThanhToan = tk.ID
        WHERE pc.ID = @ID;

        SELECT @SoTKThanhToan = SoTaiKhoan
        FROM KT_TaiKhoanKeToan
        WHERE ID = @IDTKKeToan;

        IF NOT EXISTS (
            SELECT 1 FROM KT_NhatKyChung
            WHERE LoaiChungTu = N'PHIEUCHI' AND IDChungTu = @ID AND IsHuy = 0
        )
        BEGIN
            INSERT INTO KT_NhatKyChung
                (NgayChungTu, SoChungTu, LoaiChungTu, IDChungTu,
                 TaiKhoanNo, TaiKhoanCo, SoTien, DienGiai, NgayTao, NguoiTao)
            VALUES
                (@NgayChi, @SoPhieuChi, N'PHIEUCHI', @ID,
                 N'6418',
                 ISNULL(@SoTKThanhToan, N'1111'),
                 @SoTienChi,
                 ISNULL(@DienGiai, N'Phiếu chi ' + @SoPhieuChi),
                 GETDATE(), @NguoiGhi);
        END

        -- Cập nhật trạng thái thanh toán và số tiền còn lại bên Phiếu Nhập Kho
        IF @IDPhieuNhap IS NOT NULL AND @IDPhieuNhap > 0
        BEGIN
            EXEC sp_KHO_CapNhatTrangThaiThanhToanPhieuNhap @IDPhieuNhap = @IDPhieuNhap;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- 5. Cập nhật sp_KT_PhieuChi_Huy
CREATE OR ALTER PROCEDURE sp_KT_PhieuChi_Huy
    @ID         INT,
    @NguoiHuy   INT,
    @LyDoHuy    NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @IDPhieuNhap INT;
        DECLARE @TrangThaiHienTai INT;

        SELECT @IDPhieuNhap = IDPhieuNhap, @TrangThaiHienTai = TrangThai
        FROM KT_PhieuChi
        WHERE ID = @ID AND IsDeleted = 0;

        IF @TrangThaiHienTai IS NULL THROW 50001, N'Phiếu chi không tồn tại.', 1;
        IF @TrangThaiHienTai = 3 THROW 50002, N'Phiếu chi đã được hủy trước đó.', 1;

        -- Cập nhật trạng thái phiếu chi thành Đã hủy
        UPDATE KT_PhieuChi
        SET TrangThai    = 3,
            LyDoHuy      = @LyDoHuy,
            NgayHuy      = GETDATE(),
            NguoiHuy     = @NguoiHuy,
            NgayCapNhat  = GETDATE(),
            NguoiCapNhat = @NguoiHuy
        WHERE ID = @ID AND IsDeleted = 0;

        -- Đánh dấu hủy bút toán NKC nếu đã ghi sổ
        IF @TrangThaiHienTai = 2
        BEGIN
            UPDATE KT_NhatKyChung
            SET IsHuy = 1,
                NgayHuy = GETDATE(),
                NguoiHuy = @NguoiHuy,
                LyDoHuy = @LyDoHuy
            WHERE LoaiChungTu = N'PHIEUCHI' AND IDChungTu = @ID;
        END

        -- Cập nhật lại trạng thái thanh toán bên Phiếu Nhập Kho (Rollback dữ liệu)
        IF @IDPhieuNhap IS NOT NULL AND @IDPhieuNhap > 0
        BEGIN
            EXEC sp_KHO_CapNhatTrangThaiThanhToanPhieuNhap @IDPhieuNhap = @IDPhieuNhap;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
