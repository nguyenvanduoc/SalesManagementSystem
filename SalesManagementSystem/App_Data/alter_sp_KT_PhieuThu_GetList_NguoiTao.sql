-- ==============================================================================
-- Script: alter_sp_KT_PhieuThu_GetList_NguoiTao.sql
-- Description: Cập nhật sp_KT_PhieuThu_GetList để lấy TenNguoiTao từ bảng NS_NhanSu
-- ==============================================================================

IF OBJECT_ID('sp_KT_PhieuThu_GetList', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThu_GetList;
GO

CREATE PROCEDURE sp_KT_PhieuThu_GetList
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
        -- Đoạn thêm mới: Lấy và ghép Họ đệm + Tên từ bảng NS_NhanSu
        LTRIM(RTRIM(ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, ''))) AS TenNguoiTao,
        pt.NgayCapNhat,
        
        (SELECT ISNULL(SUM(SoTienPhanBo), 0) FROM KT_PhieuThuChiTiet WHERE IDPhieuThu = pt.ID AND LoaiThu = 1) AS SoTienPhanBo
        
    FROM KT_PhieuThu pt
    LEFT JOIN DM_TaiKhoanThanhToan  tk  ON pt.IDTaiKhoanThanhToan = tk.ID
    LEFT JOIN NS_KhachHang          kh  ON pt.IDKhachHang         = kh.ID
    -- Đoạn thêm mới: JOIN với NS_NhanSu để lấy thông tin người tạo
    LEFT JOIN NS_NhanSu             ns  ON pt.NguoiTao            = ns.ID
    WHERE 1=1
      AND (@TuNgay IS NULL OR CAST(pt.NgayThu AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay IS NULL OR CAST(pt.NgayThu AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@SoPhieuThu IS NULL OR pt.SoPhieuThu LIKE '%' + @SoPhieuThu + '%')
      AND (@IDKhachHang IS NULL OR pt.IDKhachHang = @IDKhachHang)
      AND (@TrangThai IS NULL OR pt.TrangThai = @TrangThai)
      AND (@NguoiNopTien IS NULL OR pt.NguoiNopTien LIKE '%' + @NguoiNopTien + '%')
      AND (@IDTaiKhoanThanhToan IS NULL OR pt.IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan)
    ORDER BY pt.NgayThu DESC, pt.ID DESC
END
GO
