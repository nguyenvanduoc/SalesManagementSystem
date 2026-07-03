IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_KT_PhieuChi_GetList')
    DROP PROCEDURE sp_KT_PhieuChi_GetList;
GO

CREATE PROCEDURE sp_KT_PhieuChi_GetList
    @TuNgay         DATETIME        = NULL,
    @DenNgay        DATETIME        = NULL,
    @SoPhieuChi     NVARCHAR(50)    = NULL,
    @IDNhaCungCap   INT             = NULL,
    @IDKhoanMucChi  INT             = NULL,
    @TrangThai      INT             = NULL,
    @NguoiNhanTien  NVARCHAR(250)   = NULL,
    @IDTaiKhoanThanhToan INT        = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        pc.ID,
        pc.SoPhieuChi,
        pc.NgayChi,
        pc.IDKhoanMucChi,
        km.TenKhoanMuc,
        pc.IDTaiKhoanThanhToan,
        tk.TenTaiKhoan  AS TenTaiKhoanThanhToan,
        tk.SoTaiKhoan,
        pc.IDNguoiNhan,
        ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, '') AS TenNguoiNhan,
        pc.NguoiNhanTien,
        pc.SoDienThoaiNguoiNhan,
        pc.IDNhaCungCap,
        ncc.TenNhaCungCap,
        pc.IDPhieuNhap,
        
        -- Lấy chuỗi các số phiếu nhập từ bảng KT_PhieuChiChiTiet
        STUFF((
            SELECT ', ' + pn2.SoChungTu
            FROM KT_PhieuChiChiTiet ct
            INNER JOIN KHO_PhieuNhap pn2 ON ct.IDPhieuNhap = pn2.ID
            WHERE ct.IDPhieuChi = pc.ID
            FOR XML PATH('')
        ), 1, 2, '') AS SoPhieuNhap,

        pc.SoTienChi,
        pc.DienGiai,
        pc.TrangThai,
        pc.NgayTao,
        pc.NguoiTao,
        pc.NgayGhi,
        pc.LyDoHuy,
        
        -- Cột tự sinh SoTienPhanBo để Grid hiển thị
        (SELECT ISNULL(SUM(SoTienPhanBo), 0) FROM KT_PhieuChiChiTiet WHERE IDPhieuChi = pc.ID) AS SoTienPhanBo
        
    FROM KT_PhieuChi pc
    LEFT JOIN DM_KhoanMucChi        km  ON pc.IDKhoanMucChi       = km.ID
    LEFT JOIN DM_TaiKhoanThanhToan  tk  ON pc.IDTaiKhoanThanhToan = tk.ID
    LEFT JOIN NS_NhanSu             ns  ON pc.IDNguoiNhan          = ns.ID
    LEFT JOIN DM_NhaCungCap         ncc ON pc.IDNhaCungCap         = ncc.ID
    WHERE pc.IsDeleted = 0
      AND (@TuNgay        IS NULL OR CAST(pc.NgayChi AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay       IS NULL OR CAST(pc.NgayChi AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@SoPhieuChi    IS NULL OR (
          pc.SoPhieuChi LIKE '%' + @SoPhieuChi + '%'
          OR EXISTS (
              SELECT 1 
              FROM KT_PhieuChiChiTiet ct
              INNER JOIN KHO_PhieuNhap pn2 ON ct.IDPhieuNhap = pn2.ID
              WHERE ct.IDPhieuChi = pc.ID AND pn2.SoChungTu LIKE '%' + @SoPhieuChi + '%'
          )
      ))
      AND (@IDNhaCungCap  IS NULL OR pc.IDNhaCungCap = @IDNhaCungCap)
      AND (@IDKhoanMucChi IS NULL OR pc.IDKhoanMucChi = @IDKhoanM ucChi)
      AND (@TrangThai     IS NULL OR pc.TrangThai = @TrangThai)
      AND (@NguoiNhanTien IS NULL OR pc.NguoiNhanTien LIKE '%' + @NguoiNhanTien + '%' OR (ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, '')) LIKE '%' + @NguoiNhanTien + '%')
      AND (@IDTaiKhoanThanhToan IS NULL OR pc.IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan)
    ORDER BY pc.NgayChi DESC, pc.ID DESC;
END
GO
