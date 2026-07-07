-- =======================================================
-- Phieu Thu Khach Hang Stored Procedures
-- =======================================================

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
        
        (SELECT ISNULL(SUM(SoTienPhanBo), 0) FROM KT_PhieuThuChiTiet WHERE IDPhieuThu = pt.ID AND LoaiThu = 1) AS SoTienPhanBo
        
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

IF OBJECT_ID('sp_KT_PhieuThu_GetById', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThu_GetById;
GO
CREATE OR ALTER PROCEDURE sp_KT_PhieuThu_GetById
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pt.*,
        kh.TenKhachHang,
        tk.TenTaiKhoan AS TenTaiKhoanThanhToan
    FROM KT_PhieuThu pt
    LEFT JOIN NS_KhachHang kh ON pt.IDKhachHang = kh.ID
    LEFT JOIN DM_TaiKhoanThanhToan tk ON pt.IDTaiKhoanThanhToan = tk.ID
    WHERE pt.ID = @ID;

    SELECT 
        ct.*,
        bh.SoChungTu,
        bh.NgayChungTu,
        bh.TongCong,
        bh.DaThanhToan,
        bh.ConLai
    FROM KT_PhieuThuChiTiet ct
    LEFT JOIN BAN_ChungTuBanHang bh ON ct.IDChungTuBanHang = bh.ID
    WHERE ct.IDPhieuThu = @ID;
END
GO

IF OBJECT_ID('sp_KT_PhieuThu_LoadCongNoKhachHang', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThu_LoadCongNoKhachHang;
GO
CREATE OR ALTER PROCEDURE sp_KT_PhieuThu_LoadCongNoKhachHang
    @IDKhachHang INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Lấy các chứng từ bán hàng đã ghi sổ (TrangThai = 2) và chưa thanh toán hết
    SELECT 
        bh.ID,
        bh.SoChungTu,
        bh.NgayChungTu AS NgayNhap,
        bh.TongCong AS TongTien,
        bh.DaThanhToan,
        bh.ConLai
    FROM BAN_ChungTuBanHang bh
    WHERE bh.IDKhachHang = @IDKhachHang
      AND bh.TrangThai = 2
      AND bh.ConLai > 0
    ORDER BY bh.NgayChungTu ASC, bh.ID ASC;
END
GO

IF OBJECT_ID('sp_KT_PhieuThu_GetTienTraTruocKhachHang', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThu_GetTienTraTruocKhachHang;
GO
CREATE OR ALTER PROCEDURE sp_KT_PhieuThu_GetTienTraTruocKhachHang
    @IDKhachHang INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Tổng dư trả trước (LoaiThu = 2) trừ đi tổng đã dùng (LoaiThu = 3)
    DECLARE @TraTruoc DECIMAL(18,0) = 0;
    
    SELECT @TraTruoc = ISNULL(SUM(CASE WHEN ct.LoaiThu = 2 THEN ct.SoTienPhanBo WHEN ct.LoaiThu = 3 THEN -ct.SoTienPhanBo ELSE 0 END), 0)
    FROM KT_PhieuThuChiTiet ct
    INNER JOIN KT_PhieuThu pt ON ct.IDPhieuThu = pt.ID
    WHERE pt.IDKhachHang = @IDKhachHang AND pt.TrangThai = 2; -- Chỉ tính phiếu đã ghi

    SELECT @TraTruoc;
END
GO

IF OBJECT_ID('sp_KT_PhieuThu_Delete', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThu_Delete;
GO
CREATE OR ALTER PROCEDURE sp_KT_PhieuThu_Delete
    @ID INT
AS
BEGIN
    DELETE FROM KT_PhieuThuChiTiet WHERE IDPhieuThu = @ID;
    DELETE FROM KT_PhieuThu WHERE ID = @ID;
END
GO
