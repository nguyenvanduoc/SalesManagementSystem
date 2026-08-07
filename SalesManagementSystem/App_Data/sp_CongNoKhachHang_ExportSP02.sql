CREATE OR ALTER PROCEDURE [dbo].[sp_CongNoKhachHang_ExportSP02]
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @IDKhachHang INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        kh.ID AS IDKhachHang,
        ISNULL(tt.TenTinhThanh, kh.DiaChi) AS TinhThanh,
        kh.TenKhachHang,
        -- Đầu kỳ: Doanh thu trước TuNgay - Thu trước TuNgay
        ISNULL((
            SELECT SUM(ct.TongCong)
            FROM BAN_ChungTuBanHang ct
            WHERE ct.IDKhachHang = kh.ID AND ct.IsDeleted = 0 AND ct.TrangThai = 2
              AND (@TuNgay IS NULL OR ct.NgayChungTu < @TuNgay)
        ), 0) 
        - 
        ISNULL((
            SELECT SUM(pt.SoTienThu)
            FROM BAN_PhieuThuKhachHang pt
            WHERE pt.IDKhachHang = kh.ID AND pt.IsDeleted = 0 AND pt.TrangThai = 2
              AND (@TuNgay IS NULL OR pt.NgayThu < @TuNgay)
        ), 0) AS DuDauKy,
        
        -- Doanh thu trong kỳ
        ISNULL((
            SELECT SUM(ct.TongCong)
            FROM BAN_ChungTuBanHang ct
            WHERE ct.IDKhachHang = kh.ID AND ct.IsDeleted = 0 AND ct.TrangThai = 2
              AND (@TuNgay IS NULL OR ct.NgayChungTu >= @TuNgay)
              AND (@DenNgay IS NULL OR ct.NgayChungTu <= @DenNgay)
        ), 0) AS DoanhThu,
        
        -- Thanh toán trong kỳ
        ISNULL((
            SELECT SUM(pt.SoTienThu)
            FROM BAN_PhieuThuKhachHang pt
            WHERE pt.IDKhachHang = kh.ID AND pt.IsDeleted = 0 AND pt.TrangThai = 2
              AND (@TuNgay IS NULL OR pt.NgayThu >= @TuNgay)
              AND (@DenNgay IS NULL OR pt.NgayThu <= @DenNgay)
        ), 0) AS ThanhToan,

        -- Khách thanh toán trước (Khách trả trước lũy kế tính đến DenNgay)
        ISNULL((
            SELECT SUM(CASE WHEN ct_pt.LoaiThu = 2 THEN ct_pt.SoTienPhanBo WHEN ct_pt.LoaiThu = 3 THEN -ct_pt.SoTienPhanBo ELSE 0 END)
            FROM KT_PhieuThuChiTiet ct_pt
            INNER JOIN KT_PhieuThu pt ON ct_pt.IDPhieuThu = pt.ID
            WHERE pt.TrangThai = 2 AND pt.IDKhachHang = kh.ID
              AND (@DenNgay IS NULL OR pt.NgayThu <= @DenNgay)
        ), 0) AS KhachThanhToanTruoc,
        
        -- Hàng chờ giao (Phiếu bán hàng ở trạng thái Đề nghị ghi / đang đi đường)
        ISNULL((
            SELECT SUM(ct.TongCong)
            FROM BAN_ChungTuBanHang ct
            WHERE ct.IDKhachHang = kh.ID AND ct.IsDeleted = 0 AND ct.TrangThai = 1
              AND (@TuNgay IS NULL OR ct.NgayChungTu >= @TuNgay)
              AND (@DenNgay IS NULL OR ct.NgayChungTu <= @DenNgay)
        ), 0) AS HangChoGiao,
        
        '' AS GhiChu
    FROM NS_KhachHang kh
    LEFT JOIN DM_TinhThanh tt ON kh.IDTinhThanh = tt.ID
    WHERE (@IDKhachHang IS NULL OR kh.ID = @IDKhachHang)
    ORDER BY kh.TenKhachHang;
END
