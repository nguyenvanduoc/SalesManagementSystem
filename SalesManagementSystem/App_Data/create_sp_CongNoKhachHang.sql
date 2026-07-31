-- 1. sp_CongNoKhachHang_GetList
IF OBJECT_ID('sp_CongNoKhachHang_GetList', 'P') IS NOT NULL DROP PROC sp_CongNoKhachHang_GetList;
GO
CREATE PROCEDURE sp_CongNoKhachHang_GetList
    @TuNgay         DATETIME        = NULL,
    @DenNgay        DATETIME        = NULL,
    @IDKhachHang    INT             = NULL,
    @TrangThaiCongNo INT            = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TuNgayDate DATE = CAST(@TuNgay AS DATE);
    DECLARE @DenNgayDate DATE = CAST(@DenNgay AS DATE);

    ;WITH InvoicePaid AS (
        SELECT 
            bh.ID AS IDChungTuBanHang,
            bh.IDKhachHang,
            bh.NgayChungTu,
            bh.TongCong,
            ISNULL((
                SELECT SUM(ct.SoTienPhanBo)
                FROM KT_PhieuThuChiTiet ct
                INNER JOIN KT_PhieuThu pt ON ct.IDPhieuThu = pt.ID
                WHERE ct.IDChungTuBanHang = bh.ID AND pt.TrangThai = 2 AND ct.LoaiThu IN (1, 3)
            ), 0) AS DaThuInvoice
        FROM BAN_ChungTuBanHang bh
        WHERE bh.IsDeleted = 0 AND bh.TrangThai IN (1, 2)
    ),
    OverduePerCustomer AS (
        SELECT 
            IDKhachHang,
            SUM(CASE WHEN DATEDIFF(day, NgayChungTu, GETDATE()) > 30 AND (TongCong - DaThuInvoice) > 0 THEN (TongCong - DaThuInvoice) ELSE 0 END) AS TienQuaHan
        FROM InvoicePaid
        GROUP BY IDKhachHang
    ),
    CustomerSummary AS (
        SELECT 
            kh.ID AS IDKhachHang,
            kh.TenKhachHang,
            kh.SoDienThoai AS DienThoai,

            -- 1. Tồn đầu kỳ: (Tổng nợ bán hàng trước @TuNgay) - (Tổng đã thu trước @TuNgay)
            ISNULL((
                SELECT SUM(bh.TongCong) 
                FROM BAN_ChungTuBanHang bh 
                WHERE bh.IDKhachHang = kh.ID 
                  AND bh.IsDeleted = 0 
                  AND bh.TrangThai IN (1, 2) 
                  AND (@TuNgayDate IS NOT NULL AND CAST(bh.NgayChungTu AS DATE) < @TuNgayDate)
            ), 0)
            -
            ISNULL((
                SELECT SUM(pt.SoTienThu) 
                FROM KT_PhieuThu pt 
                WHERE pt.IDKhachHang = kh.ID 
                  AND pt.TrangThai = 2 
                  AND (@TuNgayDate IS NOT NULL AND CAST(pt.NgayThu AS DATE) < @TuNgayDate)
            ), 0) AS TonDauKy,

            -- 2. Doanh thu trong kỳ: Phát sinh nợ bán hàng từ @TuNgay đến @DenNgay
            ISNULL((
                SELECT SUM(bh.TongCong) 
                FROM BAN_ChungTuBanHang bh 
                WHERE bh.IDKhachHang = kh.ID 
                  AND bh.IsDeleted = 0 
                  AND bh.TrangThai IN (1, 2) 
                  AND (@TuNgayDate IS NULL OR CAST(bh.NgayChungTu AS DATE) >= @TuNgayDate)
                  AND (@DenNgayDate IS NULL OR CAST(bh.NgayChungTu AS DATE) <= @DenNgayDate)
            ), 0) AS DoanhThu,

            -- 3. Đã thu trong kỳ: Phát sinh có từ @TuNgay đến @DenNgay
            ISNULL((
                SELECT SUM(pt.SoTienThu) 
                FROM KT_PhieuThu pt 
                WHERE pt.IDKhachHang = kh.ID 
                  AND pt.TrangThai = 2 
                  AND (@TuNgayDate IS NULL OR CAST(pt.NgayThu AS DATE) >= @TuNgayDate)
                  AND (@DenNgayDate IS NULL OR CAST(pt.NgayThu AS DATE) <= @DenNgayDate)
            ), 0) AS DaThu,

            -- 4. Tiền quá hạn (> 30 ngày)
            ISNULL(od.TienQuaHan, 0) AS TienQuaHan

        FROM NS_KhachHang kh
        LEFT JOIN OverduePerCustomer od ON kh.ID = od.IDKhachHang
        WHERE (@IDKhachHang IS NULL OR kh.ID = @IDKhachHang)
    ),
    Calculated AS (
        SELECT 
            0 AS IDChungTuBanHang,
            CAST(NULL AS INT) AS IDDonDatHang,
            '' AS SoChungTu,
            CAST('1900-01-01' AS DATETIME) AS NgayChungTu,
            IDKhachHang,
            TenKhachHang,
            DienThoai,
            DoanhThu,
            DaThu,
            (TonDauKy + DoanhThu - DaThu) AS ConPhaiThu,
            TonDauKy,
            TienQuaHan
        FROM CustomerSummary
        WHERE TonDauKy <> 0 OR DoanhThu <> 0 OR DaThu <> 0 OR (TonDauKy + DoanhThu - DaThu) <> 0
    )
    SELECT *
    FROM Calculated
    WHERE (
        @TrangThaiCongNo IS NULL
        OR (@TrangThaiCongNo = 1 AND ConPhaiThu > 0 AND TienQuaHan = 0)
        OR (@TrangThaiCongNo = 2 AND DaThu > 0 AND ConPhaiThu > 0)
        OR (@TrangThaiCongNo = 3 AND ConPhaiThu <= 0)
        OR (@TrangThaiCongNo = 4 AND TienQuaHan > 0)
    )
    ORDER BY ConPhaiThu DESC, TenKhachHang ASC;
END
GO

-- 2. sp_CongNoKhachHang_GetDashboard
IF OBJECT_ID('sp_CongNoKhachHang_GetDashboard', 'P') IS NOT NULL DROP PROC sp_CongNoKhachHang_GetDashboard;
GO
CREATE PROCEDURE sp_CongNoKhachHang_GetDashboard
    @TuNgay         DATETIME        = NULL,
    @DenNgay        DATETIME        = NULL,
    @IDKhachHang    INT             = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Calculate total sales invoices and collections matching the date filter
    DECLARE @TongPhaiThu DECIMAL(18,0) = 0;
    DECLARE @DaThu DECIMAL(18,0) = 0;
    DECLARE @ConPhaiThu DECIMAL(18,0) = 0;
    DECLARE @KhachTraTruoc DECIMAL(18,0) = 0;
    DECLARE @CongNoQuaHan DECIMAL(18,0) = 0;

    -- 1. Tổng phải thu (Doanh thu bán hàng trong kỳ)
    SELECT @TongPhaiThu = ISNULL(SUM(bh.TongCong), 0)
    FROM BAN_ChungTuBanHang bh
    WHERE bh.IsDeleted = 0 
      AND bh.TrangThai IN (1, 2)
      AND (@TuNgay IS NULL OR CAST(bh.NgayChungTu AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay IS NULL OR CAST(bh.NgayChungTu AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@IDKhachHang IS NULL OR bh.IDKhachHang = @IDKhachHang);

    -- 2. Đã thu (Số tiền đã phân bổ/thu trong kỳ)
    SELECT @DaThu = ISNULL(SUM(ct.SoTienPhanBo), 0)
    FROM KT_PhieuThuChiTiet ct
    INNER JOIN KT_PhieuThu pt ON ct.IDPhieuThu = pt.ID
    INNER JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
    WHERE pt.TrangThai = 2
      AND bh.IsDeleted = 0 
      AND bh.TrangThai IN (1, 2)
      AND (@TuNgay IS NULL OR CAST(bh.NgayChungTu AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay IS NULL OR CAST(bh.NgayChungTu AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@IDKhachHang IS NULL OR bh.IDKhachHang = @IDKhachHang)
      AND ct.LoaiThu IN (1, 3);

    -- 3. Còn phải thu
    SET @ConPhaiThu = @TongPhaiThu - @DaThu;

    -- 4. Khách trả trước (Số dư trả trước lũy kế của khách hàng còn lại > 0)
    ;WITH Prepayment AS (
        SELECT 
            pt.IDKhachHang,
            SUM(CASE WHEN ct.LoaiThu = 2 THEN ct.SoTienPhanBo WHEN ct.LoaiThu = 3 THEN -ct.SoTienPhanBo ELSE 0 END) AS Balance
        FROM KT_PhieuThuChiTiet ct
        INNER JOIN KT_PhieuThu pt ON ct.IDPhieuThu = pt.ID
        WHERE pt.TrangThai = 2
          AND (@IDKhachHang IS NULL OR pt.IDKhachHang = @IDKhachHang)
        GROUP BY pt.IDKhachHang
    )
    SELECT @KhachTraTruoc = ISNULL(SUM(Balance), 0)
    FROM Prepayment
    WHERE Balance > 0;

    -- 5. Công nợ quá hạn
    ;WITH Overdue AS (
        SELECT 
            bh.ID,
            bh.TongCong - ISNULL((
                SELECT SUM(ct.SoTienPhanBo)
                FROM KT_PhieuThuChiTiet ct
                INNER JOIN KT_PhieuThu pt ON ct.IDPhieuThu = pt.ID
                WHERE ct.IDChungTuBanHang = bh.ID AND pt.TrangThai = 2 AND ct.LoaiThu IN (1, 3)
            ), 0) AS ConLai
        FROM BAN_ChungTuBanHang bh
        WHERE bh.IsDeleted = 0
          AND bh.TrangThai IN (1, 2)
          AND DATEDIFF(day, bh.NgayChungTu, GETDATE()) > 30
          AND (@IDKhachHang IS NULL OR bh.IDKhachHang = @IDKhachHang)
    )
    SELECT @CongNoQuaHan = ISNULL(SUM(ConLai), 0)
    FROM Overdue
    WHERE ConLai > 0;

    SELECT 
        @TongPhaiThu AS TongPhaiThu,
        @DaThu AS DaThu,
        @ConPhaiThu AS ConPhaiThu,
        @KhachTraTruoc AS KhachTraTruoc,
        @CongNoQuaHan AS CongNoQuaHan;
END
GO

-- 3. sp_CongNoKhachHang_GetDetail
IF OBJECT_ID('sp_CongNoKhachHang_GetDetail', 'P') IS NOT NULL DROP PROC sp_CongNoKhachHang_GetDetail;
GO
CREATE PROCEDURE sp_CongNoKhachHang_GetDetail
    @IDKhachHang    INT,
    @TuNgay         DATETIME        = NULL,
    @DenNgay        DATETIME        = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Calculate starting balance before @TuNgay
    DECLARE @StartBalance DECIMAL(18,0) = 0;

    SELECT @StartBalance = 
        ISNULL((SELECT SUM(bh.TongCong) FROM BAN_ChungTuBanHang bh WHERE bh.IDKhachHang = @IDKhachHang AND bh.IsDeleted = 0 AND bh.TrangThai IN (1, 2) AND (@TuNgay IS NULL OR CAST(bh.NgayChungTu AS DATE) < CAST(@TuNgay AS DATE))), 0)
        -
        ISNULL((SELECT SUM(pt.SoTienThu) FROM KT_PhieuThu pt WHERE pt.IDKhachHang = @IDKhachHang AND pt.TrangThai = 2 AND (@TuNgay IS NULL OR CAST(pt.NgayThu AS DATE) < CAST(@TuNgay AS DATE))), 0);

    -- Temp table to collect transactions
    CREATE TABLE #Ledger (
        SortDate DATETIME,
        Ngay VARCHAR(10),
        SoChungTu NVARCHAR(50),
        LoaiChungTu NVARCHAR(50),
        DienGiai NVARCHAR(500),
        PhaiThu DECIMAL(18,0),
        ThanhToan DECIMAL(18,0),
        SortOrder INT
    );

    -- 1. Insert opening balance row
    INSERT INTO #Ledger (SortDate, Ngay, SoChungTu, LoaiChungTu, DienGiai, PhaiThu, ThanhToan, SortOrder)
    VALUES (
        ISNULL(@TuNgay, '1900-01-01'),
        COALESCE(CONVERT(VARCHAR(10), @TuNgay, 103), '01/01/1900'),
        '-',
        N'Số dư đầu kỳ',
        N'Chuyển sang từ kỳ trước',
        0,
        0,
        1
    );

    -- 2. Invoices (Phải thu)
    INSERT INTO #Ledger (SortDate, Ngay, SoChungTu, LoaiChungTu, DienGiai, PhaiThu, ThanhToan, SortOrder)
    SELECT 
        CAST(bh.NgayChungTu AS DATETIME) + CAST(CAST(bh.NgayTao AS TIME) AS DATETIME),
        CONVERT(VARCHAR(10), bh.NgayChungTu, 103),
        bh.SoChungTu,
        N'BÁN HÀNG',
        N'Doanh thu bán hàng',
        bh.TongCong,
        0,
        2
    FROM BAN_ChungTuBanHang bh
    WHERE bh.IDKhachHang = @IDKhachHang
      AND bh.IsDeleted = 0
      AND bh.TrangThai IN (1, 2)
      AND (@TuNgay IS NULL OR CAST(bh.NgayChungTu AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay IS NULL OR CAST(bh.NgayChungTu AS DATE) <= CAST(@DenNgay AS DATE));

    -- 3. Receipts (Thanh toán)
    INSERT INTO #Ledger (SortDate, Ngay, SoChungTu, LoaiChungTu, DienGiai, PhaiThu, ThanhToan, SortOrder)
    SELECT 
        CAST(pt.NgayThu AS DATETIME) + CAST(CAST(pt.NgayTao AS TIME) AS DATETIME),
        CONVERT(VARCHAR(10), pt.NgayThu, 103),
        pt.SoPhieuThu,
        N'PHIẾU THU',
        pt.DienGiai,
        0,
        pt.SoTienThu,
        2
    FROM KT_PhieuThu pt
    WHERE pt.IDKhachHang = @IDKhachHang
      AND pt.TrangThai = 2
      AND (@TuNgay IS NULL OR CAST(pt.NgayThu AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay IS NULL OR CAST(pt.NgayThu AS DATE) <= CAST(@DenNgay AS DATE));

    -- Select output with running balance
    ;WITH Sorted AS (
        SELECT 
            ROW_NUMBER() OVER (ORDER BY SortOrder ASC, SortDate ASC, SoChungTu ASC) AS RowNum,
            SortDate, Ngay, SoChungTu, LoaiChungTu, DienGiai, PhaiThu, ThanhToan
        FROM #Ledger
    )
    SELECT 
        Ngay,
        SoChungTu,
        LoaiChungTu,
        DienGiai,
        PhaiThu,
        ThanhToan,
        -- Running balance calculation using window function for O(N) performance
        @StartBalance + SUM(PhaiThu - ThanhToan) OVER (ORDER BY RowNum ROWS UNBOUNDED PRECEDING) AS ConLai
    FROM Sorted
    ORDER BY RowNum ASC;

    DROP TABLE #Ledger;
END
GO

-- 4. sp_CongNoKhachHang_GetHistory
IF OBJECT_ID('sp_CongNoKhachHang_GetHistory', 'P') IS NOT NULL DROP PROC sp_CongNoKhachHang_GetHistory;
GO
CREATE PROCEDURE sp_CongNoKhachHang_GetHistory
    @IDChungTuBanHang INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        pt.ID               AS IDPhieuThu,
        pt.SoPhieuThu,
        pt.NgayThu,
        ct.SoTienPhanBo     AS SoTienThu,
        pt.DienGiai,
        pt.TrangThai
    FROM KT_PhieuThuChiTiet ct
    INNER JOIN KT_PhieuThu pt ON ct.IDPhieuThu = pt.ID
    WHERE ct.IDChungTuBanHang = @IDChungTuBanHang
      AND pt.TrangThai = 2
    ORDER BY pt.NgayThu DESC, pt.ID DESC;
END
GO
