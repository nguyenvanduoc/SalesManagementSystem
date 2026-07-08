CREATE OR ALTER PROCEDURE dbo.sp_KHO_PhieuNhap_GetList
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKho INT = NULL,
    @IDNhaCungCap INT = NULL,
    @TrangThai INT = NULL,
    @TenNguoiNhan NVARCHAR(200) = NULL,
    @TenNguoiGiao NVARCHAR(200) = NULL,
    @IDPhuongTien INT = NULL,
    @Offset INT = 0,
    @PageSize INT = 20,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Lấy tổng số dòng
    SELECT @TotalRecords = COUNT(*)
    FROM [dbo].[KHO_PhieuNhap] p
    LEFT JOIN [dbo].[NS_NhanSu] ns ON p.IDNhanSuNhan = ns.ID
    WHERE p.IsDeleted = 0
      AND (@TuNgay IS NULL OR p.NgayNhap >= @TuNgay)
      AND (@DenNgay IS NULL OR p.NgayNhap <= @DenNgay)
      AND (@SoChungTu IS NULL OR p.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR p.IDKho = @IDKho)
      AND (@IDNhaCungCap IS NULL OR p.IDNhaCungCap = @IDNhaCungCap)
      AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
      AND (LEN(ISNULL(@TenNguoiNhan,'')) = 0 OR ISNULL(NULLIF(p.TenNguoiNhan, ''), ns.Ten) LIKE N'%' + @TenNguoiNhan + N'%')
      AND (LEN(ISNULL(@TenNguoiGiao,'')) = 0 OR p.TenNguoiGiao LIKE N'%' + @TenNguoiGiao + N'%')
      AND (@IDPhuongTien IS NULL OR p.IDPhuongTien = @IDPhuongTien)

    -- Trở về danh sách
    SELECT 
        p.ID,
        p.SoChungTu,
        p.NgayNhap,
        p.IDKho,
        k.TenKhoHang AS TenKho,
        k.MaKhoHang AS MaKhoHang,
        p.IDNhaCungCap,
        ncc.TenNhaCungCap AS TenNhaCungCap,
        ncc.MaNhaCungCap AS MaNhaCungCap,
        p.SoHoaDon,
        p.NgayHoaDon,
        p.TenNguoiGiao,
        p.SoDienThoaiNguoiGiao,
        ISNULL(NULLIF(p.TenNguoiNhan, ''), ns.Ten) AS TenNguoiNhan,
        p.IDNhanSuNhan,
        ns.Ten AS TenNhanSuNhan,
        p.TrangThai,
        p.TongTienHang,
        p.TongTienThue,
        p.TongCong,
        p.NgayTao,
        p.NguoiTao,
        NguoiTaoText = nsTao.HoDem + ' ' + ns.Ten,
        TrangThaiThanhToan = CASE 
            WHEN p.TongCong - ISNULL(pay.DaThanhToan, 0) <= 0 THEN 2
            WHEN ISNULL(pay.DaThanhToan, 0) > 0 THEN 1
            ELSE 0
        END,
        DaThanhToan = ISNULL(pay.DaThanhToan, 0),
        ConLai = p.TongCong - ISNULL(pay.DaThanhToan, 0),
        p.IDPhuongTien,
        pt.TenPhuongTien AS TenPhuongTien,
        ISNULL(sl.TongSoLuong, 0) AS TongSoLuong,
        ISNULL(p.TienVanChuyen, 0) AS TienVanChuyen,
        p.HoTenTaiXe,
        p.SoDienThoaiTaiXe,
        p.NgayGiaoHang
		
    FROM [dbo].[KHO_PhieuNhap] p
    LEFT JOIN (
        SELECT ct.IDPhieuNhap, SUM(ct.SoTienPhanBo) AS DaThanhToan
        FROM [dbo].[KT_PhieuChiChiTiet] ct
        JOIN [dbo].[KT_PhieuChi] pc ON ct.IDPhieuChi = pc.ID
        WHERE pc.TrangThai = 2 AND pc.IsDeleted = 0 AND ct.LoaiChi = 1
        GROUP BY ct.IDPhieuNhap
    ) pay ON pay.IDPhieuNhap = p.ID
    LEFT JOIN (
        SELECT IDPhieuNhap, SUM(SoLuong) AS TongSoLuong
        FROM [dbo].[KHO_PhieuNhap_ChiTiet]
        GROUP BY IDPhieuNhap
    ) sl ON sl.IDPhieuNhap = p.ID
    LEFT JOIN [dbo].[DM_KhoHang] k ON p.IDKho = k.ID
    LEFT JOIN [dbo].[DM_NhaCungCap] ncc ON p.IDNhaCungCap = ncc.ID
    LEFT JOIN [dbo].[NS_NhanSu] ns ON p.IDNhanSuNhan = ns.ID
    LEFT JOIN [dbo].[NS_NhanSu] nsTao ON p.NguoiTao = nsTao.ID
    LEFT JOIN [dbo].[DM_PhuongTien] pt ON p.IDPhuongTien = pt.ID
    WHERE p.IsDeleted = 0
      AND (@TuNgay IS NULL OR p.NgayNhap >= @TuNgay)
      AND (@DenNgay IS NULL OR p.NgayNhap <= @DenNgay)
      AND (@SoChungTu IS NULL OR p.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR p.IDKho = @IDKho)
      AND (@IDNhaCungCap IS NULL OR p.IDNhaCungCap = @IDNhaCungCap)
      AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
      AND (LEN(ISNULL(@TenNguoiNhan,'')) = 0 OR ISNULL(NULLIF(p.TenNguoiNhan, ''), ns.Ten) LIKE N'%' + @TenNguoiNhan + N'%')
      AND (LEN(ISNULL(@TenNguoiGiao,'')) = 0 OR p.TenNguoiGiao LIKE N'%' + @TenNguoiGiao + N'%')
      AND (@IDPhuongTien IS NULL OR p.IDPhuongTien = @IDPhuongTien)
    ORDER BY p.NgayNhap DESC, p.ID DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO
