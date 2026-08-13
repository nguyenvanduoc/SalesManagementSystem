-- ============================================================================
-- CẬP NHẬT STORED PROCEDURE: sp_KHO_PhieuNhap_Huy
-- Tự động hoàn lại tiền phiếu chi (LoaiChi = 2: Tiền dư trả trước) 
-- và bổ sung chuỗi "(Đã hủy)" vào Diễn giải khi Hủy phiếu nhập kho.
-- ============================================================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_KHO_PhieuNhap_Huy]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_KHO_PhieuNhap_Huy];
GO

CREATE PROCEDURE [dbo].[sp_KHO_PhieuNhap_Huy]
    @ID INT,
    @LyDoHuy NVARCHAR(MAX),
    @NguoiHuy INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @TrangThai INT, @SoChungTu NVARCHAR(50);
        SELECT @TrangThai = TrangThai, @SoChungTu = SoChungTu
        FROM KHO_PhieuNhap WHERE ID = @ID AND IsDeleted = 0;

        IF @TrangThai IS NULL
        BEGIN
            THROW 50003, N'Không tìm thấy phiếu nhập kho.', 1; 
        END

        IF @TrangThai IN (1, 2)
        BEGIN
            -- 1. Nếu phiếu nhập ở trạng thái Đã ghi sổ (TrangThai = 2), hủy tồn kho
            IF @TrangThai = 2
            BEGIN
                UPDATE KHO_GiaoDichKho
                SET IsHuy = 1,
                    NgayHuy = GETDATE(),
                    NguoiHuy = @NguoiHuy,
                    LyDoHuy = @LyDoHuy
                WHERE SoChungTu = @SoChungTu AND LoaiChungTu = 1; -- 1: Phiếu nhập
            END

            -- 2. TỰ ĐỘNG HOÀN TIỀN PHIẾU CHI:
            -- Chuyển các dòng phân bổ KT_PhieuChiChiTiet đang gắn với Phiếu Nhập này sang LoaiChi = 2 (Tiền dư trả trước)
            -- và bổ sung chuỗi "(Đã hủy)" vào Diễn giải
            UPDATE ct
            SET ct.LoaiChi = 2, -- 2: Tiền dư chuyển thành trả trước cho nhà cung cấp
                ct.DienGiai = CASE 
                    WHEN ct.DienGiai LIKE N'%\(Đã hủy\)%' THEN ct.DienGiai
                    WHEN ct.DienGiai IS NULL OR ct.DienGiai = '' THEN N'Hoàn tiền phiếu nhập ' + ISNULL(@SoChungTu, N'') + N' (Đã hủy)'
                    ELSE ct.DienGiai + N' (Đã hủy)'
                END,
                ct.IDPhieuNhap = NULL
            FROM KT_PhieuChiChiTiet ct
            INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
            WHERE ct.IDPhieuNhap = @ID 
              AND ct.LoaiChi = 1
              AND pc.IsDeleted = 0 
              AND pc.TrangThai = 2;

            -- 3. Cập nhật trạng thái phiếu nhập kho thành 3 (Hủy) và reset số tiền đã thanh toán về 0
            UPDATE KHO_PhieuNhap 
            SET TrangThai = 3, 
                NgayHuy = GETDATE(), 
                NguoiHuy = @NguoiHuy, 
                LyDoHuy = @LyDoHuy,
                DaThanhToan = 0,
                ConLai = TongCong,
                TrangThaiThanhToan = 0
            WHERE ID = @ID;
        END
        ELSE
        BEGIN
            THROW 50003, N'Phiếu không ở trạng thái hợp lệ để hủy.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- ============================================================================
-- CẬP NHẬT DỮ LIỆU TỒN ĐỌNG: Xử lý phiếu nhập PN26000127 đã bị hủy trước đó
-- ============================================================================
UPDATE ct
SET ct.LoaiChi = 2,
    ct.DienGiai = CASE 
        WHEN ct.DienGiai LIKE N'%\(Đã hủy\)%' THEN ct.DienGiai
        WHEN ct.DienGiai IS NULL OR ct.DienGiai = '' THEN N'Hoàn tiền phiếu nhập PN26000127 (Đã hủy)'
        ELSE ct.DienGiai + N' (Đã hủy)'
    END,
    ct.IDPhieuNhap = NULL
FROM KT_PhieuChiChiTiet ct
INNER JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
WHERE pn.SoChungTu = 'PN26000127' AND pn.TrangThai = 3 AND ct.LoaiChi = 1;
GO
