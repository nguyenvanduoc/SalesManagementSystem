-- =========================================================================
-- Script sửa lỗi Card Số Dư và đồng bộ sổ quỹ cho hệ thống
-- =========================================================================

-- 1. Cập nhật sp_KT_PhieuChi_GhiSo để tự động đồng bộ vào QUY_GiaoDichTien
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

        -- Lấy thông tin phiếu chi
        DECLARE @SoPhieuChi         NVARCHAR(50);
        DECLARE @NgayChi            DATE;
        DECLARE @SoTienChi          DECIMAL(18,2);
        DECLARE @DienGiai           NVARCHAR(1000);
        DECLARE @IDTaiKhoanThanhToan INT;
        DECLARE @SoTKThanhToan      NVARCHAR(20);
        DECLARE @IDTKKeToan         INT;

        SELECT
            @SoPhieuChi          = pc.SoPhieuChi,
            @NgayChi             = pc.NgayChi,
            @SoTienChi           = pc.SoTienChi,
            @DienGiai            = pc.DienGiai,
            @IDTaiKhoanThanhToan = pc.IDTaiKhoanThanhToan,
            @IDTKKeToan          = tk.IDTaiKhoanKeToan
        FROM KT_PhieuChi pc
        LEFT JOIN DM_TaiKhoanThanhToan tk ON pc.IDTaiKhoanThanhToan = tk.ID
        WHERE pc.ID = @ID;

        SELECT @SoTKThanhToan = SoTaiKhoan
        FROM KT_TaiKhoanKeToan
        WHERE ID = @IDTKKeToan;

        -- Ghi vào Nhật Ký Chung
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

        -- Ghi vào Sổ Quỹ QUY_GiaoDichTien
        IF NOT EXISTS (
            SELECT 1 FROM QUY_GiaoDichTien
            WHERE LoaiChungTu = 'PHIEU_CHI' AND IDChungTu = @ID AND IsHuy = 0
        )
        BEGIN
            INSERT INTO QUY_GiaoDichTien (
                NgayGiaoDich, SoChungTu, LoaiChungTu, IDChungTu,
                IDTaiKhoanThanhToan, SoTienThu, SoTienChi, DienGiai, NguoiTao
            )
            VALUES (
                @NgayChi, @SoPhieuChi, 'PHIEU_CHI', @ID,
                @IDTaiKhoanThanhToan, 0, @SoTienChi,
                ISNULL(@DienGiai, N'Phiếu chi ' + @SoPhieuChi), @NguoiGhi
            );
        END

        -- Cập nhật trạng thái thanh toán bên Phiếu Nhập Kho nếu có
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

-- 2. Cập nhật sp_KT_PhieuChi_Huy để hủy sổ quỹ QUY_GiaoDichTien
CREATE OR ALTER PROCEDURE sp_KT_PhieuChi_Huy
    @ID         INT,
    @NguoiHuy   INT,
    @LyDoHuy    NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE KT_PhieuChi
    SET TrangThai    = 3,
        LyDoHuy      = @LyDoHuy,
        NgayHuy      = GETDATE(),
        NguoiHuy     = @NguoiHuy,
        NgayCapNhat  = GETDATE(),
        NguoiCapNhat = @NguoiHuy
    WHERE ID = @ID AND IsDeleted = 0;

    -- Đánh dấu hủy bút toán NKC nếu đã ghi
    UPDATE KT_NhatKyChung
    SET IsHuy = 1
    WHERE LoaiChungTu = N'PHIEUCHI' AND IDChungTu = @ID;

    -- Đánh dấu hủy sổ quỹ QUY_GiaoDichTien
    UPDATE QUY_GiaoDichTien
    SET IsHuy = 1
    WHERE LoaiChungTu = 'PHIEU_CHI' AND IDChungTu = @ID;
END
GO

-- 3. Đồng bộ các phiếu chi KT_PhieuChi đã ghi sổ (TrangThai = 2) vào QUY_GiaoDichTien
INSERT INTO QUY_GiaoDichTien (
    NgayGiaoDich, SoChungTu, LoaiChungTu, IDChungTu,
    IDTaiKhoanThanhToan, SoTienThu, SoTienChi, DienGiai, NguoiTao, NgayTao, IsHuy
)
SELECT 
    pc.NgayChi,
    pc.SoPhieuChi,
    'PHIEU_CHI',
    pc.ID,
    pc.IDTaiKhoanThanhToan,
    0,
    pc.SoTienChi,
    ISNULL(pc.DienGiai, N'Phiếu chi ' + pc.SoPhieuChi),
    ISNULL(pc.NguoiGhi, pc.NguoiTao),
    ISNULL(pc.NgayGhi, pc.NgayTao),
    0
FROM KT_PhieuChi pc
WHERE pc.TrangThai = 2 AND pc.IsDeleted = 0
  AND NOT EXISTS (
      SELECT 1 FROM QUY_GiaoDichTien g 
      WHERE g.LoaiChungTu = 'PHIEU_CHI' AND g.IDChungTu = pc.ID
  );
GO
