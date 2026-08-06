-- =============================================
-- Author:      Antigravity
-- Create date: 2026-06-21
-- Description: Retrieve all dashboard statistics, charts, warnings, and recent orders in a single database call.
-- =============================================

ALTER PROCEDURE sp_Dashboard_GetData
    @TuNgay DATETIME,
    @DenNgay DATETIME,
    @TuNgayKyTruoc DATETIME,
    @DenNgayKyTruoc DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    -- Tính DaThanhToan cho tất cả phiếu nhập
    SELECT 
        pn.ID AS IDPhieuNhap,
        ISNULL(
            (SELECT SUM(ct.SoTienPhanBo)
             FROM KT_PhieuChiChiTiet ct
             INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
             WHERE ct.IDPhieuNhap = pn.ID 
               AND ct.LoaiChi = 1
               AND pc.TrangThai = 2
               AND pc.IsDeleted = 0),
            0
        ) + ISNULL(
            (SELECT SUM(pc2.SoTienChi)
             FROM KT_PhieuChi pc2
             WHERE pc2.IDPhieuNhap = pn.ID
               AND pc2.TrangThai = 2
               AND pc2.IsDeleted = 0
               AND NOT EXISTS (SELECT 1 FROM KT_PhieuChiChiTiet ct WHERE ct.IDPhieuChi = pc2.ID)
            ),
            0
        ) AS DaThanhToan
    INTO #PaidNCC
    FROM KHO_PhieuNhap pn
    WHERE pn.IsDeleted = 0;


    -- 1. KHỐI 1: TỔNG QUAN
    -- Doanh thu hiện tại & Doanh thu kỳ trước
    DECLARE @DoanhThu DECIMAL(18, 2) = 0;
    DECLARE @DoanhThuKyTruoc DECIMAL(18, 2) = 0;

    SELECT @DoanhThu = ISNULL(SUM(TongCong), 0) 
    FROM BAN_ChungTuBanHang 
    WHERE TrangThai = 2 AND IsDeleted = 0 
      AND NgayChungTu >= @TuNgay AND NgayChungTu <= @DenNgay;
    
    SELECT @DoanhThuKyTruoc = ISNULL(SUM(TongCong), 0) 
    FROM BAN_ChungTuBanHang 
    WHERE TrangThai = 2 AND IsDeleted = 0 
      AND NgayChungTu >= @TuNgayKyTruoc AND NgayChungTu <= @DenNgayKyTruoc;

    -- Công nợ khách hàng
    DECLARE @CongNoKhachHang DECIMAL(18, 2) = 0;
    SELECT @CongNoKhachHang = ISNULL(SUM(TongCong - DaThanhToan), 0) 
    FROM BAN_ChungTuBanHang 
    WHERE IsDeleted = 0 AND TrangThai = 2 AND NgayChungTu <= @DenNgay;

    -- Công nợ nhà cung cấp
    DECLARE @TongTienHangNCC DECIMAL(18, 2) = 0;
    DECLARE @DaThanhToanNCC DECIMAL(18, 2) = 0;
    DECLARE @CongNoNhaCungCap DECIMAL(18, 2) = 0;
    
    SELECT 
        @TongTienHangNCC = ISNULL(SUM(TongTienHang), 0),
        @DaThanhToanNCC = ISNULL(SUM(DaThanhToan), 0)
    FROM (
        SELECT 
            pn.TongCong AS TongTienHang,
            ISNULL(
                (SELECT SUM(ct.SoTienPhanBo)
                 FROM KT_PhieuChiChiTiet ct
                 INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                 WHERE ct.IDPhieuNhap = pn.ID 
                   AND ct.LoaiChi = 1
                   AND pc.TrangThai = 2
                   AND pc.IsDeleted = 0),
                0
            ) + ISNULL(
                (SELECT SUM(pc2.SoTienChi)
                 FROM KT_PhieuChi pc2
                 WHERE pc2.IDPhieuNhap = pn.ID
                   AND pc2.TrangThai = 2
                   AND pc2.IsDeleted = 0
                   AND NOT EXISTS (SELECT 1 FROM KT_PhieuChiChiTiet ct WHERE ct.IDPhieuChi = pc2.ID)
                ),
                0
            ) AS DaThanhToan
        FROM KHO_PhieuNhap pn
        LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
        WHERE pn.IsDeleted = 0 AND pn.NgayNhap <= @DenNgay
    ) t;
    
    SET @CongNoNhaCungCap = @TongTienHangNCC - @DaThanhToanNCC;

    -- Tiền hiện có
    DECLARE @TienHienCo DECIMAL(18, 2) = 0;
    SELECT @TienHienCo = ISNULL((SELECT SUM(SoTienThu) FROM BAN_PhieuThuKhachHang WHERE TrangThai = 2 AND IsDeleted = 0 AND NgayThu <= @DenNgay), 0) -
                         ISNULL((SELECT SUM(SoTienChi) FROM KT_PhieuChi WHERE TrangThai = 2 AND IsDeleted = 0 AND NgayChi <= @DenNgay), 0);

    -- Số dư tiền mặt (tài khoản ket toan 1111)
    DECLARE @TienMat DECIMAL(18, 2) = 0;
    SELECT @TienMat = ISNULL(SUM(g.SoTienThu) - SUM(g.SoTienChi), 0)
    FROM QUY_GiaoDichTien g
    INNER JOIN DM_TaiKhoanThanhToan tk ON g.IDTaiKhoanThanhToan = tk.ID
    WHERE g.IsHuy = 0 AND tk.IDTaiKhoanKeToan = 2; -- 1111 Tiền mặt Việt Nam

    -- Tổng số dư tất cả tài khoản
    DECLARE @TongSoDuTaiKhoan DECIMAL(18, 2) = 0;
    SELECT @TongSoDuTaiKhoan = ISNULL(SUM(g.SoTienThu) - SUM(g.SoTienChi), 0)
    FROM QUY_GiaoDichTien g
    WHERE g.IsHuy = 0;

    -- Giá vốn & Lợi nhuận
    DECLARE @GiaVon DECIMAL(18, 2) = 0;
    DECLARE @GiaVonKyTruoc DECIMAL(18, 2) = 0;

    SELECT @GiaVon = ISNULL(SUM(ct.DonGia * ct.SoLuong), 0) 
    FROM BAN_ChungTuBanHang_ChiTiet ct
    JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
    WHERE bh.TrangThai = 2 AND bh.IsDeleted = 0 AND bh.NgayChungTu >= @TuNgay AND bh.NgayChungTu <= @DenNgay;

    SELECT @GiaVonKyTruoc = ISNULL(SUM(ct.DonGia * ct.SoLuong), 0) 
    FROM BAN_ChungTuBanHang_ChiTiet ct
    JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
    WHERE bh.TrangThai = 2 AND bh.IsDeleted = 0 AND bh.NgayChungTu >= @TuNgayKyTruoc AND bh.NgayChungTu <= @DenNgayKyTruoc;

    DECLARE @LoiNhuan DECIMAL(18, 2) = @DoanhThu - @GiaVon;
    DECLARE @LoiNhuanKyTruoc DECIMAL(18, 2) = @DoanhThuKyTruoc - @GiaVonKyTruoc;

    -- Sản phẩm trong kho summary
    DECLARE @TongGiaTriTonKho DECIMAL(18, 2) = 0;
    DECLARE @SoLuongSanPhamTon INT = 0;
    DECLARE @SoSanPhamSapHet INT = 0;
    DECLARE @TongSoLuongTon DECIMAL(18, 2) = 0;

    SELECT 
        gd.IDSanPham,
        SUM(gd.SoLuongNhap - gd.SoLuongXuat) AS SoLuongTon,
        ISNULL((SELECT TOP 1 gd2.DonGia FROM KHO_GiaoDichKho gd2 INNER JOIN DM_KhoHang kh2 ON gd2.IDKho = kh2.ID AND ISNULL(kh2.IsKhoChinh, 0) = 1 WHERE gd2.IDSanPham = gd.IDSanPham AND gd2.SoLuongNhap > 0 AND gd2.IsHuy = 0 ORDER BY gd2.NgayChungTu DESC, gd2.ID DESC), 0) AS DonGiaTon
    INTO #TempStock
    FROM KHO_GiaoDichKho gd
    INNER JOIN DM_KhoHang kh ON gd.IDKho = kh.ID AND ISNULL(kh.IsKhoChinh, 0) = 1
    WHERE gd.IsHuy = 0 AND gd.NgayChungTu <= @DenNgay
    GROUP BY gd.IDSanPham;

    SELECT 
        @TongGiaTriTonKho = ISNULL(SUM(SoLuongTon * DonGiaTon), 0),
        @SoLuongSanPhamTon = COUNT(CASE WHEN SoLuongTon > 0 THEN 1 END),
        @SoSanPhamSapHet = COUNT(CASE WHEN SoLuongTon > 0 AND SoLuongTon < 10 THEN 1 END),
        @TongSoLuongTon = ISNULL(SUM(CASE WHEN SoLuongTon > 0 THEN SoLuongTon ELSE 0 END), 0)
    FROM #TempStock;

    DROP TABLE #TempStock;

    -- Thu chi summary
    DECLARE @TongThu DECIMAL(18, 2) = 0;
    DECLARE @TongChi DECIMAL(18, 2) = 0;
    SELECT @TongThu = ISNULL((SELECT SUM(SoTienThu) FROM BAN_PhieuThuKhachHang WHERE TrangThai = 2 AND IsDeleted = 0 AND NgayThu >= @TuNgay AND NgayThu <= @DenNgay), 0);
    SELECT @TongChi = ISNULL((SELECT SUM(SoTienChi) FROM KT_PhieuChi WHERE TrangThai = 2 AND IsDeleted = 0 AND NgayChi >= @TuNgay AND NgayChi <= @DenNgay), 0);

    -- Cảnh báo
    DECLARE @DonHangQuaHanGiao INT = 0;
    DECLARE @PhieuNhapChuaThanhToan INT = 0;
    DECLARE @ChungTuChuaGhi INT = 0;
    SELECT @DonHangQuaHanGiao = COUNT(1) FROM NS_DonDatHang WHERE TrangThaiDon IN (1, 2) AND ThoiHanGiaoHang < CAST(GETDATE() AS DATE);
    SELECT @PhieuNhapChuaThanhToan = COUNT(1) FROM KHO_PhieuNhap WHERE IsDeleted = 0 AND TrangThai = 2 AND TongCong > ISNULL((SELECT SUM(SoTienChi) FROM KT_PhieuChi WHERE IDPhieuNhap = KHO_PhieuNhap.ID AND TrangThai = 2 AND IsDeleted = 0), 0);
    SELECT @ChungTuChuaGhi = COUNT(1) FROM BAN_ChungTuBanHang WHERE IsDeleted = 0 AND TrangThai = 1;

    -- Trả về SELECT Summary đầu tiên
    SELECT 
        @DoanhThu AS DoanhThu,
        @DoanhThuKyTruoc AS DoanhThuKyTruoc,
        @CongNoKhachHang AS CongNoKhachHang,
        @TongTienHangNCC AS TongTienHangNCC,
        @DaThanhToanNCC AS DaThanhToanNCC,
        @CongNoNhaCungCap AS CongNoNhaCungCap,
        @TienHienCo AS TienHienCo,
        @LoiNhuan AS LoiNhuan,
        @LoiNhuanKyTruoc AS LoiNhuanKyTruoc,
        @SoLuongSanPhamTon AS SoLuongSanPhamTon,
        @TongGiaTriTonKho AS TongGiaTriTonKho,
        @SoSanPhamSapHet AS SoSanPhamSapHet,
        @TongSoLuongTon AS TongSoLuongTon,
        @TienMat AS TienMat,
        @TongSoDuTaiKhoan AS TongSoDuTaiKhoan,
        @TongThu AS TongThu,
        @TongChi AS TongChi,
        @DonHangQuaHanGiao AS DonHangQuaHanGiao,
        @PhieuNhapChuaThanhToan AS PhieuNhapChuaThanhToan,
        @ChungTuChuaGhi AS ChungTuChuaGhi;

    -- 2. DoanhThuTheoThoiGian
    SELECT FORMAT(NgayChungTu, 'dd/MM/yyyy') AS Label, SUM(TongCong) AS Value 
    FROM BAN_ChungTuBanHang 
    WHERE TrangThai = 2 AND IsDeleted = 0 AND NgayChungTu >= @TuNgay AND NgayChungTu <= @DenNgay
    GROUP BY FORMAT(NgayChungTu, 'dd/MM/yyyy'), CAST(NgayChungTu AS DATE)
    ORDER BY CAST(NgayChungTu AS DATE);

    -- 3. GiaVonTheoThoiGian
    SELECT FORMAT(bh.NgayChungTu, 'dd/MM/yyyy') AS Label, SUM(ct.DonGia * ct.SoLuong) AS Value 
    FROM BAN_ChungTuBanHang_ChiTiet ct
    JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
    WHERE bh.TrangThai = 2 AND bh.IsDeleted = 0 AND bh.NgayChungTu >= @TuNgay AND bh.NgayChungTu <= @DenNgay
    GROUP BY FORMAT(bh.NgayChungTu, 'dd/MM/yyyy'), CAST(bh.NgayChungTu AS DATE)
    ORDER BY CAST(bh.NgayChungTu AS DATE);

    -- 4. TrangThaiDonHang
    SELECT 
        CASE TrangThaiDon
            WHEN 1 THEN N'Chưa giao'
            WHEN 2 THEN N'Đang giao'
            WHEN 3 THEN N'Đã giao'
            WHEN 4 THEN N'Đã hủy'
            ELSE N'Khác'
        END AS Label, COUNT(ID) AS Value
    FROM NS_DonDatHang
    WHERE NgayTaoDon >= @TuNgay AND NgayTaoDon <= @DenNgay
    GROUP BY TrangThaiDon;

    -- 5. TopTonKho (Top 10)
    WITH CurrentStock AS (
        SELECT 
            gd.IDSanPham,
            SUM(gd.SoLuongNhap - gd.SoLuongXuat) AS SoLuongTon
        FROM KHO_GiaoDichKho gd
        INNER JOIN DM_KhoHang kh ON gd.IDKho = kh.ID AND ISNULL(kh.IsKhoChinh, 0) = 1
        WHERE gd.IsHuy = 0 AND gd.NgayChungTu <= @DenNgay
        GROUP BY gd.IDSanPham
    )
    SELECT TOP 10
        s.TenSanPham AS Label,
        cs.SoLuongTon AS Value
    FROM CurrentStock cs
    JOIN DM_SanPham s ON cs.IDSanPham = s.ID
    WHERE cs.SoLuongTon > 0
    ORDER BY cs.SoLuongTon DESC;

    -- 6. TopBanChay (Top 10)
    SELECT ct.IDSanPham, SUM(ct.SoLuong) AS TotalQty
    INTO #TotalSales
    FROM BAN_ChungTuBanHang_ChiTiet ct
    JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
    WHERE bh.IsDeleted = 0 AND bh.TrangThai = 2
      AND bh.NgayChungTu >= @TuNgay AND bh.NgayChungTu <= @DenNgay
    GROUP BY ct.IDSanPham;

    SELECT ct.IDSanPham, bh.IDKhachHang, SUM(ct.SoLuong) AS CustQty
    INTO #CustSales
    FROM BAN_ChungTuBanHang_ChiTiet ct
    JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
    WHERE bh.IsDeleted = 0 AND bh.TrangThai = 2
      AND bh.NgayChungTu >= @TuNgay AND bh.NgayChungTu <= @DenNgay
    GROUP BY ct.IDSanPham, bh.IDKhachHang;

    WITH CustRank AS (
        SELECT 
            c.IDSanPham, 
            c.IDKhachHang, 
            c.CustQty,
            ROW_NUMBER() OVER (PARTITION BY c.IDSanPham ORDER BY c.CustQty DESC) AS Rnk
        FROM #CustSales c
    )
    SELECT TOP 10 
        sp.TenSanPham AS Label, 
        t.TotalQty AS Value,
        sp.MaSanPham,
        kh.TenKhachHang AS TopCustomerName,
        cr.CustQty AS TopCustomerQty,
        CASE WHEN t.TotalQty > 0 THEN (cr.CustQty / t.TotalQty) * 100 ELSE 0 END AS TopCustomerRatio
    FROM #TotalSales t
    JOIN DM_SanPham sp ON t.IDSanPham = sp.ID
    LEFT JOIN CustRank cr ON t.IDSanPham = cr.IDSanPham AND cr.Rnk = 1
    LEFT JOIN NS_KhachHang kh ON cr.IDKhachHang = kh.ID
    ORDER BY t.TotalQty DESC;

    DROP TABLE #TotalSales;
    DROP TABLE #CustSales;

    -- 7. ThuChiTheoNgay
    WITH DailyFlow AS (
        SELECT CAST(NgayThu AS DATE) AS Ngay, SoTienThu AS Thu, 0 AS Chi
        FROM BAN_PhieuThuKhachHang
        WHERE TrangThai = 2 AND IsDeleted = 0 AND NgayThu >= @TuNgay AND NgayThu <= @DenNgay
        UNION ALL
        SELECT CAST(NgayChi AS DATE) AS Ngay, 0 AS Thu, SoTienChi AS Chi
        FROM KT_PhieuChi
        WHERE TrangThai = 2 AND IsDeleted = 0 AND NgayChi >= @TuNgay AND NgayChi <= @DenNgay
    )
    SELECT 
        FORMAT(Ngay, 'dd/MM/yyyy') AS Label,
        SUM(Thu - Chi) AS Value
    FROM DailyFlow
    GROUP BY Ngay, CAST(Ngay AS DATE)
    ORDER BY CAST(Ngay AS DATE);

    -- 8. TaiKhoanThanhToan
    SELECT tk.ID, tk.TenTaiKhoan,
           ISNULL(tk.NganHang, '') AS NganHang,
           ISNULL(tk.SoTaiKhoan, '') AS SoTaiKhoan,
           
           -- TongThu, TongChi (Toàn thời gian đến DenNgay)
           ISNULL((SELECT SUM(g.SoTienThu) FROM QUY_GiaoDichTien g WHERE g.IDTaiKhoanThanhToan = tk.ID AND g.NgayGiaoDich <= @DenNgay AND g.IsHuy = 0), 0) AS TongThu,
           ISNULL((SELECT SUM(g.SoTienChi) FROM QUY_GiaoDichTien g WHERE g.IDTaiKhoanThanhToan = tk.ID AND g.NgayGiaoDich <= @DenNgay AND g.IsHuy = 0), 0) AS TongChi,
           
           -- Số dư đầu kỳ
           ISNULL((
               SELECT SUM(g.SoTienThu) - SUM(g.SoTienChi)
               FROM QUY_GiaoDichTien g
               WHERE g.IDTaiKhoanThanhToan = tk.ID 
                 AND g.NgayGiaoDich < @TuNgay 
                 AND g.IsHuy = 0
           ), 0) AS SoDuDauKy,
           
           -- Thu trong kỳ
           ISNULL((
               SELECT SUM(g.SoTienThu)
               FROM QUY_GiaoDichTien g
               WHERE g.IDTaiKhoanThanhToan = tk.ID 
                 AND g.NgayGiaoDich >= @TuNgay AND g.NgayGiaoDich <= @DenNgay 
                 AND g.IsHuy = 0
           ), 0) AS ThuTrongKy,
           
           -- Chi trong kỳ
           ISNULL((
               SELECT SUM(g.SoTienChi)
               FROM QUY_GiaoDichTien g
               WHERE g.IDTaiKhoanThanhToan = tk.ID 
                 AND g.NgayGiaoDich >= @TuNgay AND g.NgayGiaoDich <= @DenNgay 
                 AND g.IsHuy = 0
           ), 0) AS ChiTrongKy,
           
           -- Số dư cuối kỳ
           ISNULL((
               SELECT SUM(g.SoTienThu) - SUM(g.SoTienChi)
               FROM QUY_GiaoDichTien g
               WHERE g.IDTaiKhoanThanhToan = tk.ID 
                 AND g.NgayGiaoDich <= @DenNgay 
                 AND g.IsHuy = 0
           ), 0) AS SoDuCuoiKy,
           
           '' AS GhiChu
           
    FROM DM_TaiKhoanThanhToan tk
    WHERE tk.IsHoatDong = 1
    ORDER BY SoDuCuoiKy DESC;

    -- 9. Công nợ khách hàng quá hạn (Summary & Top 10)
    SELECT 
        SUM(TongTien - DaThanhToan) AS TongNoQuaHan,
        COUNT(DISTINCT KhachHang) AS SoDoiTuongQuaHan
    INTO #SumKH
    FROM (
        SELECT kh.TenKhachHang AS KhachHang, ct.TongCong AS TongTien, ISNULL(ct.DaThanhToan, 0) AS DaThanhToan
        FROM BAN_ChungTuBanHang ct
        JOIN NS_KhachHang kh ON ct.IDKhachHang = kh.ID
        WHERE ct.IsDeleted = 0 AND ct.TrangThai = 2 
          AND ct.TongCong > ISNULL(ct.DaThanhToan, 0)
          AND DATEADD(day, 30, ct.NgayChungTu) < CAST(GETDATE() AS DATE)
    ) t;

    SELECT TOP 1 KhachHang, SUM(TongTien - DaThanhToan) AS TongNo
    INTO #MaxKH
    FROM (
        SELECT kh.TenKhachHang AS KhachHang, ct.TongCong AS TongTien, ISNULL(ct.DaThanhToan, 0) AS DaThanhToan
        FROM BAN_ChungTuBanHang ct
        JOIN NS_KhachHang kh ON ct.IDKhachHang = kh.ID
        WHERE ct.IsDeleted = 0 AND ct.TrangThai = 2 
          AND ct.TongCong > ISNULL(ct.DaThanhToan, 0)
          AND DATEADD(day, 30, ct.NgayChungTu) < CAST(GETDATE() AS DATE)
    ) t GROUP BY KhachHang ORDER BY TongNo DESC;

    SELECT 
        ISNULL((SELECT TongNoQuaHan FROM #SumKH), 0) AS TongNoQuaHan,
        ISNULL((SELECT SoDoiTuongQuaHan FROM #SumKH), 0) AS SoDoiTuongQuaHan,
        ISNULL((SELECT KhachHang FROM #MaxKH), '') AS TenDoiTuongNoLonNhat,
        ISNULL((SELECT TongNo FROM #MaxKH), 0) AS NoLonNhat;

    DROP TABLE #SumKH;
    DROP TABLE #MaxKH;

    -- 10. List KH
    SELECT TOP 10 
        kh.TenKhachHang AS KhachHang, ct.SoChungTu, ct.NgayChungTu, 
        DATEADD(day, 30, ct.NgayChungTu) AS HanThanhToan,
        DATEDIFF(day, DATEADD(day, 30, ct.NgayChungTu), GETDATE()) AS SoNgayQuaHan,
        ct.TongCong AS TongTien, ISNULL(ct.DaThanhToan, 0) AS DaThanhToan
    FROM BAN_ChungTuBanHang ct
    JOIN NS_KhachHang kh ON ct.IDKhachHang = kh.ID
    WHERE ct.IsDeleted = 0 AND ct.TrangThai = 2 
      AND ct.TongCong > ISNULL(ct.DaThanhToan, 0)
      AND DATEADD(day, 30, ct.NgayChungTu) < CAST(GETDATE() AS DATE)
    ORDER BY SoNgayQuaHan DESC;

    -- 11. Công nợ NCC quá hạn (Summary & Top 10)
    SELECT 
        SUM(TongTien - DaThanhToan) AS TongNoQuaHan,
        COUNT(DISTINCT NhaCungCap) AS SoDoiTuongQuaHan
    INTO #SumNCC
    FROM (
        SELECT ncc.TenNhaCungCap AS NhaCungCap, pn.TongCong AS TongTien, 
               ISNULL(pd.DaThanhToan, 0) AS DaThanhToan
        FROM KHO_PhieuNhap pn
        JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
        LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
        WHERE pn.IsDeleted = 0 
          AND pn.TongCong > ISNULL(pd.DaThanhToan, 0)
          AND DATEADD(day, 30, pn.NgayNhap) < CAST(GETDATE() AS DATE)
    ) t;

    SELECT TOP 1 NhaCungCap, SUM(TongTien - DaThanhToan) AS TongNo
    INTO #MaxNCC
    FROM (
        SELECT ncc.TenNhaCungCap AS NhaCungCap, pn.TongCong AS TongTien, 
               ISNULL(pd.DaThanhToan, 0) AS DaThanhToan
        FROM KHO_PhieuNhap pn
        JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
        LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
        WHERE pn.IsDeleted = 0 
          AND pn.TongCong > ISNULL(pd.DaThanhToan, 0)
          AND DATEADD(day, 30, pn.NgayNhap) < CAST(GETDATE() AS DATE)
    ) t GROUP BY NhaCungCap ORDER BY TongNo DESC;

    SELECT 
        ISNULL((SELECT TongNoQuaHan FROM #SumNCC), 0) AS TongNoQuaHan,
        ISNULL((SELECT SoDoiTuongQuaHan FROM #SumNCC), 0) AS SoDoiTuongQuaHan,
        ISNULL((SELECT NhaCungCap FROM #MaxNCC), '') AS TenDoiTuongNoLonNhat,
        ISNULL((SELECT TongNo FROM #MaxNCC), 0) AS NoLonNhat;

    DROP TABLE #SumNCC;
    DROP TABLE #MaxNCC;

    -- 12. List NCC
    SELECT TOP 10 
        ncc.TenNhaCungCap AS NhaCungCap, pn.SoChungTu AS SoPhieuNhap, pn.NgayNhap, 
        DATEADD(day, 30, pn.NgayNhap) AS HanThanhToan,
        DATEDIFF(day, DATEADD(day, 30, pn.NgayNhap), GETDATE()) AS SoNgayQuaHan,
        pn.TongCong AS TongTien, 
        ISNULL(pd.DaThanhToan, 0) AS DaThanhToan
    FROM KHO_PhieuNhap pn
    JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
    WHERE pn.IsDeleted = 0 
      AND pn.TongCong > ISNULL(pd.DaThanhToan, 0)
      AND DATEADD(day, 30, pn.NgayNhap) < CAST(GETDATE() AS DATE)
    ORDER BY SoNgayQuaHan DESC;

    -- 13. HoatDongGanDay
    SELECT TOP 10 * FROM (
        SELECT NgayTao AS ThoiGian, 
               ISNULL((SELECT LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) FROM ACL_Login WHERE ID = NguoiTao), N'Hệ thống') AS NguoiThucHien,
               N'Tạo đơn hàng ' + SoDonHang AS NoiDung, 
               'TaoDonHang' AS LoaiHoatDong
        FROM NS_DonDatHang
        UNION ALL
        SELECT NgayGhi, 
               ISNULL((SELECT LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) FROM ACL_Login WHERE ID = NguoiGhi), N'Hệ thống') AS NguoiThucHien,
               N'Ghi chứng từ ' + SoChungTu AS NoiDung, 
               'GhiChungTu' AS LoaiHoatDong
        FROM BAN_ChungTuBanHang WHERE IsDeleted = 0 AND TrangThai = 2
        UNION ALL
        SELECT NgayGhiSo, 
               ISNULL((SELECT LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) FROM ACL_Login WHERE ID = NguoiGhiSo), N'Hệ thống') AS NguoiThucHien,
               N'Ghi sổ nhập kho ' + SoChungTu AS NoiDung, 
               'NhapKho' AS LoaiHoatDong
        FROM KHO_PhieuNhap WHERE IsDeleted = 0 AND TrangThai = 2
        UNION ALL
        SELECT NgayChi, 
               ISNULL((SELECT LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) FROM ACL_Login WHERE ID = NguoiTao), N'Hệ thống') AS NguoiThucHien,
               N'Chi tiền ' + SoPhieuChi AS NoiDung, 
               'ChiTien' AS LoaiHoatDong
        FROM KT_PhieuChi WHERE IsDeleted = 0 AND TrangThai = 2
        UNION ALL
        SELECT NgayThu, 
               ISNULL((SELECT LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) FROM ACL_Login WHERE ID = NguoiTao), N'Hệ thống') AS NguoiThucHien,
               N'Thu tiền ' + SoPhieuThu AS NoiDung, 
               'ThuTien' AS LoaiHoatDong
        FROM BAN_PhieuThuKhachHang WHERE IsDeleted = 0 AND TrangThai = 2
    ) t
    WHERE ThoiGian IS NOT NULL
    ORDER BY ThoiGian DESC;

    -- 14. TopKhachHang
    SELECT TOP 10 kh.TenKhachHang AS TenDoiTuong,
           ISNULL(SUM(ct.TongCong), 0) AS DoanhThuHoacGiaTriNhap,
           ISNULL(SUM(ct.TongCong - ISNULL(ct.DaThanhToan, 0)), 0) AS CongNo
    FROM BAN_ChungTuBanHang ct
    JOIN NS_KhachHang kh ON ct.IDKhachHang = kh.ID
    WHERE ct.IsDeleted = 0 AND ct.TrangThai = 2
    GROUP BY kh.TenKhachHang
    ORDER BY DoanhThuHoacGiaTriNhap DESC;

    -- 15. TopNhaCungCap
    SELECT TOP 10 ncc.TenNhaCungCap AS TenDoiTuong,
           SUM(pn.TongCong) AS DoanhThuHoacGiaTriNhap,
           SUM(pn.TongCong - ISNULL(pd.DaThanhToan, 0)) AS CongNo
    FROM KHO_PhieuNhap pn
    JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
    WHERE pn.IsDeleted = 0
    GROUP BY ncc.TenNhaCungCap
    ORDER BY DoanhThuHoacGiaTriNhap DESC;

    -- 16. DonHangGanDay (Recent Orders)
    SELECT TOP 5
        d.ID,
        d.SoDonHang,
        d.NgayTaoDon,
        d.TrangThaiDon,
        CASE d.TrangThaiDon
            WHEN 1 THEN N'Chưa giao'
            WHEN 2 THEN N'Đang giao'
            WHEN 3 THEN N'Đã giao'
            WHEN 4 THEN N'Đã hủy'
            ELSE N'Khác'
        END AS TenTrangThai,
        d.TongTien,
        ISNULL(l.HoDem + ' ' + l.Ten, N'Hệ thống') AS TenNguoiTao,
        kh.TenKhachHang
    FROM NS_DonDatHang d
    LEFT JOIN ACL_Login l ON d.NguoiTao = l.ID
    LEFT JOIN NS_KhachHang kh ON d.IDKhachHang = kh.ID
    ORDER BY d.NgayTaoDon DESC, d.ID DESC;

    -- 17. DonHangDangDiDuong (Orders on the way)
    SELECT
        d.ID,
        d.SoDonHang,
        d.NgayTaoDon,
        kh.TenKhachHang,
        pt.TenPhuongTien,
        ISNULL((SELECT SUM(SoLuong) FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = d.ID), 0) AS TongSoLuong,
        d.TongTien
    FROM NS_DonDatHang d
    LEFT JOIN NS_KhachHang kh ON d.IDKhachHang = kh.ID
    LEFT JOIN DM_PhuongTien pt ON d.IDPhuongTien = pt.ID
    WHERE d.TrangThaiDon = 2
    ORDER BY d.NgayTaoDon DESC, d.ID DESC;

    -- 18. PhieuNhapDangDiDuong (Phiếu nhập kho trạng thái 1 - Nháp / Đang đi đường / Đề nghị ghi)
    SELECT
        pn.ID,
        pn.SoChungTu,
        pn.NgayNhap,
        pn.TrangThai,
        ncc.TenNhaCungCap,
        pt.TenPhuongTien,
        pn.HoTenTaiXe,
        pn.TenNguoiGiao,
        ISNULL((SELECT SUM(SoLuong) FROM KHO_PhieuNhap_ChiTiet WHERE IDPhieuNhap = pn.ID), 0) AS TongSoLuong,
        pn.TongCong
    FROM KHO_PhieuNhap pn
    LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN DM_PhuongTien pt ON pn.IDPhuongTien = pt.ID
    WHERE pn.IsDeleted = 0 AND pn.TrangThai = 1
    ORDER BY pn.NgayNhap DESC, pn.ID DESC;

    -- 19. PhieuNhapGanDay (Phiếu nhập kho đã ghi gần đây)
    SELECT TOP 5
        pn.ID,
        pn.SoChungTu,
        pn.NgayNhap,
        pn.TrangThai,
        ncc.TenNhaCungCap,
        pt.TenPhuongTien,
        pn.TongCong
    FROM KHO_PhieuNhap pn
    LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN DM_PhuongTien pt ON pn.IDPhuongTien = pt.ID
    WHERE pn.IsDeleted = 0 AND pn.TrangThai = 2
    ORDER BY pn.NgayGhiSo DESC, pn.ID DESC;

END
