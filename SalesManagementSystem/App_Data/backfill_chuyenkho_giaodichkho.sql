-- =====================================================
-- BACKFILL: Ghi giao dịch kho cho phiếu CHUYEN_KHO
-- đã được ghi sổ (TrangThai=2) nhưng chưa có giao dịch kho
-- Nguyên nhân: Chức năng ghi giao dịch kho CHUYEN_KHO được bổ sung sau,
-- các phiếu cũ đã ghi sổ trước đó chưa có giao dịch kho tương ứng
-- =====================================================

SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    -- Tìm các phiếu CHUYEN_KHO đã ghi sổ nhưng chưa có giao dịch kho
    DECLARE @PhieuChuaCoGD TABLE (
        ID INT,
        SoChungTu NVARCHAR(50),
        NgayNhap DATETIME,
        IDKho INT,
        IDKhoNguon INT,
        NguoiGhiSo INT
    );

    INSERT INTO @PhieuChuaCoGD (ID, SoChungTu, NgayNhap, IDKho, IDKhoNguon, NguoiGhiSo)
    SELECT p.ID, p.SoChungTu, p.NgayNhap, p.IDKho, p.IDKhoNguon, ISNULL(p.NguoiGhiSo, 0)
    FROM KHO_PhieuNhap p
    INNER JOIN DM_LoaiNhapKho ln ON p.IDLoaiNhapKho = ln.ID
    WHERE ln.MaLoaiNhap = 'CHUYEN_KHO'
      AND p.TrangThai = 2   -- Đã ghi sổ
      AND p.IsDeleted = 0
      AND p.IDKhoNguon IS NOT NULL
      AND NOT EXISTS (
          -- Chưa có giao dịch kho nào cho phiếu này
          SELECT 1 FROM KHO_GiaoDichKho gd
          WHERE gd.SoChungTu = p.SoChungTu
            AND gd.LoaiChungTu = 1
      );

    -- Hiển thị danh sách phiếu cần backfill
    SELECT p.ID, p.SoChungTu, p.NgayNhap, p.IDKho, p.IDKhoNguon,
           kn.TenKhoHang AS TenKhoNguon, k.TenKhoHang AS TenKhoDich
    FROM @PhieuChuaCoGD p
    LEFT JOIN DM_KhoHang kn ON p.IDKhoNguon = kn.ID
    LEFT JOIN DM_KhoHang k ON p.IDKho = k.ID;

    -- Dòng 1: Xuất khỏi kho nguồn (SoLuongXuat)
    INSERT INTO KHO_GiaoDichKho (
        NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho,
        IDKho, IDSanPham, SoLuongNhap, SoLuongXuat,
        DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao
    )
    SELECT
        p.NgayNhap, p.SoChungTu, 1, ct.ID,
        pck.IDKhoNguon, ct.IDSanPham, 0, ct.SoLuong,
        ct.DonGia, ct.ThanhTien, N'[BACKFILL] Chuyển kho đi', GETDATE(), pck.NguoiGhiSo
    FROM @PhieuChuaCoGD pck
    INNER JOIN KHO_PhieuNhap p ON pck.ID = p.ID
    INNER JOIN KHO_PhieuNhap_ChiTiet ct ON ct.IDPhieuNhap = p.ID;

    DECLARE @XuatRows INT = @@ROWCOUNT;

    -- Dòng 2: Nhập vào kho đích (SoLuongNhap)
    INSERT INTO KHO_GiaoDichKho (
        NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho,
        IDKho, IDSanPham, SoLuongNhap, SoLuongXuat,
        DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao
    )
    SELECT
        p.NgayNhap, p.SoChungTu, 1, ct.ID,
        pck.IDKho, ct.IDSanPham, ct.SoLuong, 0,
        ct.DonGia, ct.ThanhTien, N'[BACKFILL] Chuyển kho đến', GETDATE(), pck.NguoiGhiSo
    FROM @PhieuChuaCoGD pck
    INNER JOIN KHO_PhieuNhap p ON pck.ID = p.ID
    INNER JOIN KHO_PhieuNhap_ChiTiet ct ON ct.IDPhieuNhap = p.ID;

    DECLARE @NhapRows INT = @@ROWCOUNT;

    COMMIT TRANSACTION;

    PRINT N'Backfill hoàn tất.';
    PRINT N'  - Số dòng xuất kho nguồn đã tạo: ' + CAST(@XuatRows AS NVARCHAR);
    PRINT N'  - Số dòng nhập kho đích đã tạo: ' + CAST(@NhapRows AS NVARCHAR);

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT N'LỖI: ' + ERROR_MESSAGE();
    THROW;
END CATCH
GO
