-- Cập nhật sp_KT_PhieuThu_GetList để bổ sung cột TienTraTruoc, LuyKe, HasDinhKem

IF OBJECT_ID('sp_KT_PhieuThu_GetList', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThu_GetList;
GO
CREATE OR ALTER PROCEDURE sp_KT_PhieuThu_GetList
    @TuNgay         DATETIME        = NULL,
    @DenNgay        DATETIME        = NULL,
    @SoPhieuThu     NVARCHAR(50)    = NULL,
    @IDKhachHang    INT             = NULL,
    @TrangThai      INT             = NULL,
    @NguoiNopTien   NVARCHAR(250)   = NULL,
    @IDTaiKhoanThanhToan INT        = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        pt.ID,
        pt.SoPhieuThu,
        pt.NgayThu,
        pt.IDTaiKhoanThanhToan,
        tk.TenTaiKhoan AS TenTaiKhoanThanhToan,
        pt.NguoiNopTien,
        pt.SoDienThoaiNguoiNop,
        pt.IDKhachHang,
        kh.TenKhachHang,
        
        STUFF((
            SELECT ', ' + ct2.SoChungTu
            FROM KT_PhieuThuChiTiet ct
            INNER JOIN BAN_ChungTuBanHang ct2 ON ct.IDChungTuBanHang = ct2.ID
            WHERE ct.IDPhieuThu = pt.ID
            FOR XML PATH('')
        ), 1, 2, '') AS SoChungTuBanHang,

        pt.SoTienThu,
        pt.DienGiai,
        pt.TrangThai,
        pt.NgayTao,
        pt.NguoiTao,
        pt.NgayCapNhat,
        
        -- Tổng tiền phân bổ cho hóa đơn (LoaiThu = 1)
        ISNULL((SELECT SUM(SoTienPhanBo) FROM KT_PhieuThuChiTiet WHERE IDPhieuThu = pt.ID AND LoaiThu = 1), 0) AS SoTienPhanBo,

        -- Tiền trả trước còn lại của khách hàng (LoaiThu=2 đặt cọc, LoaiThu=3 đã dùng)
        ISNULL((
            SELECT SUM(CASE WHEN ct2.LoaiThu = 2 THEN ct2.SoTienPhanBo WHEN ct2.LoaiThu = 3 THEN -ct2.SoTienPhanBo ELSE 0 END)
            FROM KT_PhieuThuChiTiet ct2
            INNER JOIN KT_PhieuThu pt2 ON ct2.IDPhieuThu = pt2.ID
            WHERE pt2.IDKhachHang = pt.IDKhachHang 
              AND pt2.TrangThai = 2
              AND pt2.ID <= pt.ID  -- Lũy kế đến thời điểm phiếu này
        ), 0) AS TienTraTruoc,

        -- Lũy kế tổng thu từ đầu đến phiếu này của khách hàng
        ISNULL((
            SELECT SUM(pt2.SoTienThu)
            FROM KT_PhieuThu pt2
            WHERE pt2.IDKhachHang = pt.IDKhachHang
              AND pt2.TrangThai = 2
              AND pt2.ID <= pt.ID
        ), 0) AS LuyKe,

        -- Có đính kèm file không
        CASE WHEN EXISTS (
            SELECT 1 FROM KT_PhieuThuFile WHERE IDPhieuThu = pt.ID
        ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS HasDinhKem,

        -- Tồn đầu kỳ: tổng công nợ của khách hàng trước ngày TuNgay (tổng hóa đơn - tổng đã thu trước kỳ)
        ISNULL((
            SELECT SUM(bh.TongCong)
            FROM BAN_ChungTuBanHang bh
            WHERE bh.IDKhachHang = pt.IDKhachHang
              AND bh.TrangThai = 2
              AND (@TuNgay IS NULL OR CAST(bh.NgayChungTu AS DATE) < CAST(@TuNgay AS DATE))
        ), 0)
        - ISNULL((
            SELECT SUM(pt2.SoTienThu)
            FROM KT_PhieuThu pt2
            WHERE pt2.IDKhachHang = pt.IDKhachHang
              AND pt2.TrangThai = 2
              AND (@TuNgay IS NULL OR CAST(pt2.NgayThu AS DATE) < CAST(@TuNgay AS DATE))
        ), 0) AS TonDauKy
        
    FROM KT_PhieuThu pt
    LEFT JOIN DM_TaiKhoanThanhToan  tk  ON pt.IDTaiKhoanThanhToan = tk.ID
    LEFT JOIN NS_KhachHang          kh  ON pt.IDKhachHang         = kh.ID
    WHERE 1=1
      AND (@TuNgay        IS NULL OR CAST(pt.NgayThu AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay       IS NULL OR CAST(pt.NgayThu AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@SoPhieuThu    IS NULL OR (
          pt.SoPhieuThu LIKE '%' + @SoPhieuThu + '%'
          OR EXISTS (
              SELECT 1 
              FROM KT_PhieuThuChiTiet ct
              INNER JOIN BAN_ChungTuBanHang ct2 ON ct.IDChungTuBanHang = ct2.ID
              WHERE ct.IDPhieuThu = pt.ID AND ct2.SoChungTu LIKE '%' + @SoPhieuThu + '%'
          )
      ))
      AND (@IDKhachHang   IS NULL OR pt.IDKhachHang = @IDKhachHang)
      AND (@TrangThai     IS NULL OR pt.TrangThai = @TrangThai)
      AND (@NguoiNopTien  IS NULL OR pt.NguoiNopTien LIKE '%' + @NguoiNopTien + '%')
      AND (@IDTaiKhoanThanhToan IS NULL OR pt.IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan)
    ORDER BY pt.NgayThu DESC, pt.ID DESC;
END
GO
