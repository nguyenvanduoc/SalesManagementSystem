IF OBJECT_ID('sp_BaoCao_DoiChieuNhapNhaCungCap', 'P') IS NOT NULL
    DROP PROCEDURE sp_BaoCao_DoiChieuNhapNhaCungCap
GO

CREATE PROCEDURE sp_BaoCao_DoiChieuNhapNhaCungCap
    @IDNhaCungCap INT = NULL,
    @TuNgay DATETIME,
    @DenNgay DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Tính nợ đầu kỳ
    DECLARE @NoDauKy DECIMAL(18,2) = 0;
    
    DECLARE @TongNhapDauKy DECIMAL(18,2) = 0;
    SELECT @TongNhapDauKy = ISNULL(SUM(ct.TongSauThue), 0)
    FROM KHO_PhieuNhap pn
    INNER JOIN KHO_PhieuNhap_ChiTiet ct ON pn.ID = ct.IDPhieuNhap
    WHERE pn.IsDeleted = 0 
      AND pn.TrangThai IN (1, 2)
      AND (@IDNhaCungCap IS NULL OR pn.IDNhaCungCap = @IDNhaCungCap)
      AND CAST(pn.NgayNhap AS DATE) < CAST(@TuNgay AS DATE);

    DECLARE @TongChiDauKy DECIMAL(18,2) = 0;
    SELECT @TongChiDauKy = ISNULL(SUM(SoTienChi), 0)
    FROM KT_PhieuChi
    WHERE IsDeleted = 0 
      AND TrangThai = 2
      AND (@IDNhaCungCap IS NULL OR IDNhaCungCap = @IDNhaCungCap)
      AND CAST(NgayChi AS DATE) < CAST(@TuNgay AS DATE);
      
    SET @NoDauKy = @TongNhapDauKy - @TongChiDauKy;

    -- 2. Gom dữ liệu phát sinh trong kỳ
    CREATE TABLE #PhatSinh (
        STT INT IDENTITY(1,1),
        NgayPhatSinh DATETIME,
        SoChungTu NVARCHAR(50),
        TenNhaCungCap NVARCHAR(MAX),
        LoaiPhatSinh NVARCHAR(100),
        MaSanPham NVARCHAR(50),
        TenSanPham NVARCHAR(MAX),
        DienGiai NVARCHAR(MAX),
        SoLuongNhap DECIMAL(18,2),
        DonGiaNhap DECIMAL(18,2),
        PhaiTra DECIMAL(18,2),
        DaThanhToan DECIMAL(18,2),
        ConNoLuyKe DECIMAL(18,2),
        GhiChu NVARCHAR(MAX),
        LoaiDong INT, -- 0: Nợ đầu kỳ, 1: Nhập, 2: Chi
        ThuTuSapXep INT, -- 0: Đầu kỳ, 1: Phát sinh
        IDPhatSinh INT
    );

    -- Insert Nợ đầu kỳ
    INSERT INTO #PhatSinh (
        NgayPhatSinh, SoChungTu, TenNhaCungCap, LoaiPhatSinh, MaSanPham, TenSanPham, DienGiai, 
        SoLuongNhap, DonGiaNhap, PhaiTra, DaThanhToan, ConNoLuyKe, GhiChu, 
        LoaiDong, ThuTuSapXep, IDPhatSinh
    )
    VALUES (
        DATEADD(day, -1, @TuNgay), '', '', N'Nợ đầu kỳ', '', N'Nợ đầu kỳ', '', 
        0, 0, 0, 0, @NoDauKy, '', 
        0, 0, 0
    );

    -- Insert Phiếu Nhập
    INSERT INTO #PhatSinh (
        NgayPhatSinh, SoChungTu, TenNhaCungCap, LoaiPhatSinh, MaSanPham, TenSanPham, DienGiai, 
        SoLuongNhap, DonGiaNhap, PhaiTra, DaThanhToan, ConNoLuyKe, GhiChu, 
        LoaiDong, ThuTuSapXep, IDPhatSinh
    )
    SELECT 
        pn.NgayNhap,
        pn.SoChungTu,
        ncc.TenNhaCungCap,
        CASE WHEN pn.TrangThai = 1 THEN N'Nhập hàng (Đề nghị ghi)' ELSE N'Nhập hàng' END,
        sp.MaSanPham,
        sp.TenSanPham,
        ct.GhiChu,
        ct.SoLuong,
        ct.DonGia,
        ct.TongSauThue,
        0,
        0,
        ct.GhiChu,
        1,
        1,
        pn.ID
    FROM KHO_PhieuNhap pn
    INNER JOIN KHO_PhieuNhap_ChiTiet ct ON pn.ID = ct.IDPhieuNhap
    LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
    WHERE pn.IsDeleted = 0 
      AND pn.TrangThai IN (1, 2)
      AND (@IDNhaCungCap IS NULL OR pn.IDNhaCungCap = @IDNhaCungCap)
      AND CAST(pn.NgayNhap AS DATE) >= CAST(@TuNgay AS DATE)
      AND CAST(pn.NgayNhap AS DATE) <= CAST(@DenNgay AS DATE);

    -- Insert Phiếu Chi
    INSERT INTO #PhatSinh (
        NgayPhatSinh, SoChungTu, TenNhaCungCap, LoaiPhatSinh, MaSanPham, TenSanPham, DienGiai, 
        SoLuongNhap, DonGiaNhap, PhaiTra, DaThanhToan, ConNoLuyKe, GhiChu, 
        LoaiDong, ThuTuSapXep, IDPhatSinh
    )
    SELECT 
        pc.NgayChi,
        pc.SoPhieuChi,
        ncc.TenNhaCungCap,
        N'Thanh toán',
        '',
        ISNULL(pc.DienGiai, N'Thanh toán cho nhà cung cấp'),
        pc.DienGiai,
        0,
        0,
        0,
        pc.SoTienChi,
        0,
        pc.DienGiai,
        2,
        1,
        pc.ID
    FROM KT_PhieuChi pc
    LEFT JOIN DM_NhaCungCap ncc ON pc.IDNhaCungCap = ncc.ID
    WHERE pc.IsDeleted = 0 
      AND pc.TrangThai = 2
      AND (@IDNhaCungCap IS NULL OR pc.IDNhaCungCap = @IDNhaCungCap)
      AND CAST(pc.NgayChi AS DATE) >= CAST(@TuNgay AS DATE)
      AND CAST(pc.NgayChi AS DATE) <= CAST(@DenNgay AS DATE);

    -- 3. Tính lũy kế và trả về
    SELECT 
        ROW_NUMBER() OVER(ORDER BY ThuTuSapXep ASC, CAST(NgayPhatSinh AS DATE) ASC, LoaiDong ASC, SoChungTu ASC, IDPhatSinh ASC, STT ASC) AS STT,
        NgayPhatSinh,
        SoChungTu,
        TenNhaCungCap,
        LoaiPhatSinh,
        MaSanPham,
        TenSanPham,
        DienGiai,
        SoLuongNhap,
        DonGiaNhap,
        PhaiTra,
        DaThanhToan,
        @NoDauKy + SUM(PhaiTra - DaThanhToan) OVER(
            ORDER BY ThuTuSapXep ASC, CAST(NgayPhatSinh AS DATE) ASC, LoaiDong ASC, SoChungTu ASC, IDPhatSinh ASC, STT ASC
            ROWS UNBOUNDED PRECEDING
        ) AS ConNoLuyKe,
        GhiChu,
        LoaiDong,
        ThuTuSapXep,
        IDPhatSinh
    FROM #PhatSinh
    ORDER BY STT ASC;

    DROP TABLE #PhatSinh;
END
GO
