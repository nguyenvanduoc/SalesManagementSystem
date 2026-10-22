CREATE   PROCEDURE dbo.sp_Dashboard_GetData
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

    SELECT @GiaVon = ISNULL(SUM(
        CASE 
            WHEN ISNULL(ct.ThanhTienVon, 0) > 0 THEN ct.ThanhTienVon
            WHEN ISNULL(ct.DonGiaVon, 0) > 0 THEN ct.SoLuong * ct.DonGiaVon
            ELSE ct.SoLuong * ISNULL(ap.AvgDonGia, 0)
        END
    ), 0) 
    FROM BAN_ChungTuBanHang_ChiTiet ct
    INNER JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
    OUTER APPLY (
        SELECT 
            SUM(pn_ct.SoLuong * pn_ct.DonGia) / NULLIF(SUM(pn_ct.SoLuong), 0) AS AvgDonGia
        FROM KHO_PhieuNhap_ChiTiet pn_ct
        INNER JOIN KHO_PhieuNhap pn ON pn_ct.IDPhieuNhap = pn.ID
        INNER JOIN DM_KhoHang kh_pn ON pn.IDKho = kh_pn.ID AND ISNULL(kh_pn.IsKhoChinh, 0) = 1
        WHERE pn_ct.IDSanPham = ct.IDSanPham 
          AND pn.TrangThai = 2 AND pn.IsDeleted = 0
          AND pn.NgayNhap <= @DenNgay
    ) ap
    WHERE bh.TrangThai = 2 AND bh.IsDeleted = 0 AND bh.NgayChungTu >= @TuNgay AND bh.NgayChungTu <= @DenNgay;

    SELECT @GiaVonKyTruoc = ISNULL(SUM(
        CASE 
            WHEN ISNULL(ct.ThanhTienVon, 0) > 0 THEN ct.ThanhTienVon
            WHEN ISNULL(ct.DonGiaVon, 0) > 0 THEN ct.SoLuong * ct.DonGiaVon
            ELSE ct.SoLuong * ISNULL(ap.AvgDonGia, 0)
        END
    ), 0) 
    FROM BAN_ChungTuBanHang_ChiTiet ct
    INNER JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
    OUTER APPLY (
        SELECT 
            SUM(pn_ct.SoLuong * pn_ct.DonGia) / NULLIF(SUM(pn_ct.SoLuong), 0) AS AvgDonGia
        FROM KHO_PhieuNhap_ChiTiet pn_ct
        INNER JOIN KHO_PhieuNhap pn ON pn_ct.IDPhieuNhap = pn.ID
        INNER JOIN DM_KhoHang kh_pn ON pn.IDKho = kh_pn.ID AND ISNULL(kh_pn.IsKhoChinh, 0) = 1
        WHERE pn_ct.IDSanPham = ct.IDSanPham 
          AND pn.TrangThai = 2 AND pn.IsDeleted = 0
          AND pn.NgayNhap <= @DenNgayKyTruoc
    ) ap
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
    SELECT FORMAT(bh.NgayChungTu, 'dd/MM/yyyy') AS Label,
           SUM(
               CASE 
                   WHEN ISNULL(ct.ThanhTienVon, 0) > 0 THEN ct.ThanhTienVon
                   WHEN ISNULL(ct.DonGiaVon, 0) > 0 THEN ct.SoLuong * ct.DonGiaVon
                   ELSE ct.SoLuong * ISNULL(ap.AvgDonGia, 0)
               END
           ) AS Value 
    FROM BAN_ChungTuBanHang_ChiTiet ct
    INNER JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
    OUTER APPLY (
        SELECT 
            SUM(pn_ct.SoLuong * pn_ct.DonGia) / NULLIF(SUM(pn_ct.SoLuong), 0) AS AvgDonGia
        FROM KHO_PhieuNhap_ChiTiet pn_ct
        INNER JOIN KHO_PhieuNhap pn ON pn_ct.IDPhieuNhap = pn.ID
        INNER JOIN DM_KhoHang kh_pn ON pn.IDKho = kh_pn.ID AND ISNULL(kh_pn.IsKhoChinh, 0) = 1
        WHERE pn_ct.IDSanPham = ct.IDSanPham 
          AND pn.TrangThai = 2 AND pn.IsDeleted = 0
          AND pn.NgayNhap <= @DenNgay
    ) ap
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

    SELECT TOP 10
        sp.TenSanPham AS Label,
        ts.TotalQty AS Value,
        kh.TenKhachHang AS ExtraText
    FROM #TotalSales ts
    JOIN DM_SanPham sp ON ts.IDSanPham = sp.ID
    OUTER APPLY (
        SELECT TOP 1 IDKhachHang
        FROM #CustSales cs
        WHERE cs.IDSanPham = ts.IDSanPham
        ORDER BY CustQty DESC
    ) topCust
    LEFT JOIN DM_KhachHang kh ON topCust.IDKhachHang = kh.ID
    ORDER BY ts.TotalQty DESC;

    DROP TABLE #TotalSales;
    DROP TABLE #CustSales;

    -- 7. ThuChiTheoNgay
    SELECT FORMAT(Ngay, 'dd/MM/yyyy') AS Label, SUM(Thu) AS Value, SUM(Chi) AS SubValue
    FROM (
        SELECT NgayThu AS Ngay, SoTienThu AS Thu, 0 AS Chi FROM BAN_PhieuThuKhachHang WHERE TrangThai = 2 AND IsDeleted = 0 AND NgayThu >= @TuNgay AND NgayThu <= @DenNgay
        UNION ALL
        SELECT NgayChi AS Ngay, 0 AS Thu, SoTienChi AS Chi FROM KT_PhieuChi WHERE TrangThai = 2 AND IsDeleted = 0 AND NgayChi >= @TuNgay AND NgayChi <= @DenNgay
    ) t
    GROUP BY FORMAT(Ngay, 'dd/MM/yyyy'), CAST(Ngay AS DATE)
    ORDER BY CAST(Ngay AS DATE);

    -- 8. TaiKhoanThanhToan
    SELECT 
        tk.ID,
        tk.TenTaiKhoan,
        tk.SoTaiKhoan,
        tk.NganHang,
        tk.LoaiTaiKhoan,
        ISNULL(SUM(g.SoTienThu) - SUM(g.SoTienChi), 0) AS SoDuHienTai
    FROM DM_TaiKhoanThanhToan tk
    LEFT JOIN QUY_GiaoDichTien g ON tk.ID = g.IDTaiKhoanThanhToan AND g.IsHuy = 0
    WHERE tk.IsHoatDong = 1
    GROUP BY tk.ID, tk.TenTaiKhoan, tk.SoTaiKhoan, tk.NganHang, tk.LoaiTaiKhoan
    ORDER BY tk.LoaiTaiKhoan, tk.TenTaiKhoan;

    -- 9. CongNoKhachHangQuaHan Summary
    SELECT 
        ISNULL(SUM(TongCong - DaThanhToan), 0) AS TongNoQuaHan,
        COUNT(DISTINCT IDKhachHang) AS SoDoiTuongQuaHan,
        (SELECT TOP 1 kh.TenKhachHang FROM BAN_ChungTuBanHang bh2 INNER JOIN DM_KhachHang kh ON bh2.IDKhachHang = kh.ID WHERE bh2.IsDeleted = 0 AND bh2.TrangThai = 2 AND (bh2.TongCong - bh2.DaThanhToan) > 0 ORDER BY (bh2.TongCong - bh2.DaThanhToan) DESC) AS TenDoiTuongNoLonNhat,
        ISNULL((SELECT TOP 1 (bh3.TongCong - bh3.DaThanhToan) FROM BAN_ChungTuBanHang bh3 WHERE bh3.IsDeleted = 0 AND bh3.TrangThai = 2 AND (bh3.TongCong - bh3.DaThanhToan) > 0 ORDER BY (bh3.TongCong - bh3.DaThanhToan) DESC), 0) AS NoLonNhat
    FROM BAN_ChungTuBanHang
    WHERE IsDeleted = 0 AND TrangThai = 2 
      AND (TongCong - DaThanhToan) > 0;

    -- 10. TopKhachHangQuaHan
    SELECT TOP 5
        kh.TenKhachHang AS TenKhachHang,
        SUM(bh.TongCong - bh.DaThanhToan) AS SoTienNo,
        MAX(DATEDIFF(DAY, bh.NgayChungTu, GETDATE())) AS SoNgayQuaHan
    FROM BAN_ChungTuBanHang bh
    INNER JOIN DM_KhachHang kh ON bh.IDKhachHang = kh.ID
    WHERE bh.IsDeleted = 0 AND bh.TrangThai = 2 
      AND (bh.TongCong - bh.DaThanhToan) > 0
    GROUP BY kh.ID, kh.TenKhachHang
    ORDER BY SoTienNo DESC;

    -- 11. CongNoNccQuaHan Summary
    SELECT 
        ISNULL(SUM(pn.TongCong - pd.DaThanhToan), 0) AS TongNoQuaHan,
        COUNT(DISTINCT pn.IDNhaCungCap) AS SoDoiTuongQuaHan,
        (SELECT TOP 1 ncc.TenNhaCungCap FROM KHO_PhieuNhap pn2 INNER JOIN #PaidNCC pd2 ON pn2.ID = pd2.IDPhieuNhap INNER JOIN DM_NhaCungCap ncc ON pn2.IDNhaCungCap = ncc.ID WHERE pn2.IsDeleted = 0 AND pn2.TrangThai = 2 AND (pn2.TongCong - pd2.DaThanhToan) > 0 ORDER BY (pn2.TongCong - pd2.DaThanhToan) DESC) AS TenDoiTuongNoLonNhat,
        ISNULL((SELECT TOP 1 (pn3.TongCong - pd3.DaThanhToan) FROM KHO_PhieuNhap pn3 INNER JOIN #PaidNCC pd3 ON pn3.ID = pd3.IDPhieuNhap WHERE pn3.IsDeleted = 0 AND pn3.TrangThai = 2 AND (pn3.TongCong - pd3.DaThanhToan) > 0 ORDER BY (pn3.TongCong - pd3.DaThanhToan) DESC), 0) AS NoLonNhat
    FROM KHO_PhieuNhap pn
    INNER JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
    WHERE pn.IsDeleted = 0 AND pn.TrangThai = 2 
      AND (pn.TongCong - pd.DaThanhToan) > 0;

    -- 12. TopNccQuaHan
    SELECT TOP 5
        ncc.TenNhaCungCap AS TenNhaCungCap,
        SUM(pn.TongCong - pd.DaThanhToan) AS SoTienNo,
        MAX(DATEDIFF(DAY, pn.NgayNhap, GETDATE())) AS SoNgayQuaHan
    FROM KHO_PhieuNhap pn
    INNER JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
    INNER JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    WHERE pn.IsDeleted = 0 AND pn.TrangThai = 2 
      AND (pn.TongCong - pd.DaThanhToan) > 0
    GROUP BY ncc.ID, ncc.TenNhaCungCap
    ORDER BY SoTienNo DESC;

    -- 13. HoatDongGanDay (Top 10)
    SELECT TOP 10 *
    FROM (
        SELECT N'Bán hàng' AS LoaiHoatDong, SoChungTu AS MaChungTu, NgayChungTu AS ThoiGian, TongCong AS SoTien, N'Đã tạo chứng từ bán hàng' AS MoTa
        FROM BAN_ChungTuBanHang WHERE IsDeleted = 0
        UNION ALL
        SELECT N'Thu tiền' AS LoaiHoatDong, SoPhieuThu AS MaChungTu, NgayThu AS ThoiGian, SoTienThu AS SoTien, N'Thu tiền khách hàng' AS MoTa
        FROM BAN_PhieuThuKhachHang WHERE IsDeleted = 0
        UNION ALL
        SELECT N'Chi tiền' AS LoaiHoatDong, SoPhieuChi AS MaChungTu, NgayChi AS ThoiGian, SoTienChi AS SoTien, N'Chi tiền thanh toán' AS MoTa
        FROM KT_PhieuChi WHERE IsDeleted = 0
        UNION ALL
        SELECT N'Nhập kho' AS LoaiHoatDong, SoChungTu AS MaChungTu, NgayNhap AS ThoiGian, TongCong AS SoTien, N'Tạo phiếu nhập kho' AS MoTa
        FROM KHO_PhieuNhap WHERE IsDeleted = 0
    ) h
    ORDER BY h.ThoiGian DESC;

    -- 14. TopKhachHang (Top 5 theo Doanh Thu)
    SELECT TOP 5
        kh.TenKhachHang AS TenDoiTuong,
        SUM(bh.TongCong) AS TongGiaTri
    FROM BAN_ChungTuBanHang bh
    INNER JOIN DM_KhachHang kh ON bh.IDKhachHang = kh.ID
    WHERE bh.IsDeleted = 0 AND bh.TrangThai = 2
      AND bh.NgayChungTu >= @TuNgay AND bh.NgayChungTu <= @DenNgay
    GROUP BY kh.ID, kh.TenKhachHang
    ORDER BY TongGiaTri DESC;

    -- 15. TopNhaCungCap (Top 5 theo Giá Trị Nhập)
    SELECT TOP 5
        ncc.TenNhaCungCap AS TenDoiTuong,
        SUM(pn.TongCong) AS TongGiaTri
    FROM KHO_PhieuNhap pn
    INNER JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    WHERE pn.IsDeleted = 0 AND pn.TrangThai = 2
      AND pn.NgayNhap >= @TuNgay AND pn.NgayNhap <= @DenNgay
    GROUP BY ncc.ID, ncc.TenNhaCungCap
    ORDER BY TongGiaTri DESC;

    -- 16. DonHangGanDay (Recent Orders)
    SELECT TOP 5
        d.ID, d.MaDonHang, d.NgayTaoDon, d.TongTien, d.TrangThaiDon,
        kh.TenKhachHang
    FROM NS_DonDatHang d
    LEFT JOIN DM_KhachHang kh ON d.IDKhachHang = kh.ID
    ORDER BY d.NgayTaoDon DESC, d.ID DESC;

    -- 17. DonHangDangDiDuong (Pending/In Transit Orders)
    SELECT TOP 5
        d.ID, d.MaDonHang, d.NgayTaoDon, d.TongTien, d.TrangThaiDon,
        kh.TenKhachHang
    FROM NS_DonDatHang d
    LEFT JOIN DM_KhachHang kh ON d.IDKhachHang = kh.ID
    WHERE d.TrangThaiDon IN (1, 2)
    ORDER BY d.NgayTaoDon DESC, d.ID DESC;

    -- 18. PhieuNhapDangDiDuong (Trạng thái = 1: Nháp / Đề nghị ghi / Đang đi đường)
    SELECT TOP 5
        pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TongCong, pn.TrangThai,
        ncc.TenNhaCungCap
    FROM KHO_PhieuNhap pn
    LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    WHERE pn.IsDeleted = 0 AND pn.TrangThai = 1
    ORDER BY pn.NgayNhap DESC, pn.ID DESC;

    -- 19. PhieuNhapGanDay (Đã ghi)
    SELECT TOP 5
        pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TongCong, pn.TrangThai,
        ncc.TenNhaCungCap
    FROM KHO_PhieuNhap pn
    LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    WHERE pn.IsDeleted = 0 AND pn.TrangThai = 2
    ORDER BY pn.NgayNhap DESC, pn.ID DESC;

    DROP TABLE #PaidNCC;
END