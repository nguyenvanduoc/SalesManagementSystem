-- ==================================================
-- sp_BAN_PhieuThuKhachHang_GetList
-- ==================================================
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_GetList
    @TuNgay DATE = NULL,
    @DenNgay DATE = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKhachHang INT = NULL,
    @TrangThaiCongNo INT = NULL -- 1: Chưa thanh toán, 2: Thanh toán một phần, 3: Đã thanh toán
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.ID,
        c.SoChungTu,
        c.NgayChungTu,
        c.IDKhachHang,
        kh.TenKhachHang,
        c.TongCong,
        ISNULL(pt.DaThanhToan, 0) AS DaThanhToan,
        (c.TongCong - ISNULL(pt.DaThanhToan, 0)) AS ConLai,
        CASE 
            WHEN ISNULL(pt.DaThanhToan, 0) = 0 THEN 1 -- Chưa thanh toán
            WHEN c.TongCong - ISNULL(pt.DaThanhToan, 0) <= 0 THEN 3 -- Đã thanh toán
            ELSE 2 -- Thanh toán một phần
        END AS TrangThaiCongNo,
        ns.HoDem + ' ' + ns.Ten AS TenNguoiTao,
        c.NgayTao
    FROM BAN_ChungTuBanHang c
    JOIN NS_KhachHang kh ON c.IDKhachHang = kh.ID
    LEFT JOIN NS_NhanSu ns ON c.NguoiTao = ns.ID
    OUTER APPLY (
        SELECT SUM(p.SoTienThu) AS DaThanhToan
        FROM BAN_PhieuThuKhachHang p
        WHERE p.IDChungTuBanHang = c.ID 
          AND p.TrangThai = 2 -- Đã ghi
          AND p.IsDeleted = 0
    ) pt
    WHERE c.IsDeleted = 0
      AND c.TrangThai = 2 -- Chỉ hiển thị chứng từ bán hàng đã ghi sổ
      AND (@TuNgay IS NULL OR c.NgayChungTu >= @TuNgay)
      AND (@DenNgay IS NULL OR c.NgayChungTu <= @DenNgay)
      AND (@SoChungTu IS NULL OR c.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKhachHang IS NULL OR c.IDKhachHang = @IDKhachHang)
      AND (@TrangThaiCongNo IS NULL OR @TrangThaiCongNo = 0 OR (
            CASE 
                WHEN ISNULL(pt.DaThanhToan, 0) = 0 THEN 1
                WHEN c.TongCong - ISNULL(pt.DaThanhToan, 0) <= 0 THEN 3
                ELSE 2
            END = @TrangThaiCongNo
      ))
    ORDER BY c.NgayChungTu DESC, c.ID DESC;
END
GO

-- ==================================================
-- sp_BAN_ChungTuBanHang_GetCongNoByID
-- ==================================================
CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_GetCongNoByID
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.ID, 
        c.SoChungTu, 
        c.NgayChungTu, 
        c.IDKhachHang, 
        kh.TenKhachHang, 
        c.TongCong, 
        ISNULL(pt.DaThanhToan, 0) AS DaThanhToan, 
        (c.TongCong - ISNULL(pt.DaThanhToan, 0)) AS ConLai
    FROM BAN_ChungTuBanHang c
    JOIN NS_KhachHang kh ON c.IDKhachHang = kh.ID
    OUTER APPLY (
        SELECT SUM(p.SoTienThu) AS DaThanhToan
        FROM BAN_PhieuThuKhachHang p
        WHERE p.IDChungTuBanHang = c.ID 
          AND p.TrangThai = 2 -- Đã ghi
          AND p.IsDeleted = 0
    ) pt
    WHERE c.ID = @ID 
      AND c.IsDeleted = 0;
END
GO

-- ==================================================
-- sp_BAN_PhieuThuKhachHang_GetHistoryByChungTuID
-- ==================================================
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_GetHistoryByChungTuID
    @IDChungTuBanHang INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        pt.ID,
        pt.SoPhieuThu,
        pt.NgayThu,
        pt.IDChungTuBanHang,
        pt.IDKhachHang,
        pt.IDTaiKhoanThanhToan,
        tk.SoTaiKhoan + ' - ' + tk.TenTaiKhoan AS TenTaiKhoan,
        pt.SoTienThu,
        pt.GhiChu,
        pt.TrangThai,
        pt.NgayTao,
        ns.HoDem + ' ' + ns.Ten AS TenNguoiTao
    FROM BAN_PhieuThuKhachHang pt
    JOIN KT_TaiKhoanKeToan tk ON pt.IDTaiKhoanThanhToan = tk.ID
    LEFT JOIN NS_NhanSu ns ON pt.NguoiTao = ns.ID
    WHERE pt.IDChungTuBanHang = @IDChungTuBanHang 
      AND pt.IsDeleted = 0
    ORDER BY pt.NgayThu DESC, pt.ID DESC;
END
GO

-- ==================================================
-- sp_BAN_PhieuThuKhachHang_GetCreditInfo
-- ==================================================
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_GetCreditInfo
    @IDKhachHang INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ISNULL(SUM(c.TongCong - ISNULL(pt.DaThanhToan, 0)), 0) AS TongCongNo
    FROM BAN_ChungTuBanHang c
    OUTER APPLY (
        SELECT SUM(p.SoTienThu) AS DaThanhToan
        FROM BAN_PhieuThuKhachHang p
        WHERE p.IDChungTuBanHang = c.ID 
          AND p.TrangThai = 2 -- Đã ghi
          AND p.IsDeleted = 0
    ) pt
    WHERE c.IDKhachHang = @IDKhachHang
      AND c.TrangThai = 2 -- Đã ghi
      AND c.IsDeleted = 0;
END
GO

-- ==================================================
-- sp_BAN_PhieuThuKhachHang_GetRecentActivities
-- ==================================================
CREATE OR ALTER PROCEDURE sp_BAN_PhieuThuKhachHang_GetRecentActivities
    @IDChungTuBanHang INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 5
        pt.ID,
        pt.SoPhieuThu,
        pt.NgayThu,
        pt.SoTienThu,
        pt.TrangThai,
        ns.HoDem + ' ' + ns.Ten AS NguoiGhiSo,
        pt.NgayGhi AS ThoiGianGhiSo,
        pt.NgayCapNhat,
        ns2.HoDem + ' ' + ns2.Ten AS NguoiCapNhat
    FROM BAN_PhieuThuKhachHang pt
    LEFT JOIN NS_NhanSu ns ON pt.NguoiGhi = ns.ID
    LEFT JOIN NS_NhanSu ns2 ON pt.NguoiCapNhat = ns2.ID
    WHERE pt.IDChungTuBanHang = @IDChungTuBanHang
      AND pt.IsDeleted = 0
    ORDER BY pt.ID DESC;
END
GO
