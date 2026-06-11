CREATE OR ALTER PROCEDURE [dbo].[sp_KHO_PhieuNhap_GetList]
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKho INT = NULL,
    @IDNhaCungCap INT = NULL,
    @TrangThai INT = NULL,
    @IDNhanSuNhan INT = NULL,
    @Offset INT = 0,
    @PageSize INT = 20,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Lấy tổng số dòng
    SELECT @TotalRecords = COUNT(*)
    FROM [dbo].[KHO_PhieuNhap] p
    WHERE p.IsDeleted = 0
      AND (@TuNgay IS NULL OR p.NgayNhap >= @TuNgay)
      AND (@DenNgay IS NULL OR p.NgayNhap <= @DenNgay)
      AND (@SoChungTu IS NULL OR p.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR p.IDKho = @IDKho)
      AND (@IDNhaCungCap IS NULL OR p.IDNhaCungCap = @IDNhaCungCap)
      AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
      AND (@IDNhanSuNhan IS NULL OR p.IDNhanSuNhan = @IDNhanSuNhan);

    -- Trả về danh sách
    SELECT 
        p.ID,
        p.SoChungTu,
        p.NgayNhap,
        p.IDKho,
        k.TenKho AS TenKho,
        k.MaKho AS MaKhoHang,
        p.IDNhaCungCap,
        ncc.TenNhaCungCap AS TenNhaCungCap,
        ncc.MaNhaCungCap AS MaNhaCungCap,
        p.SoHoaDon,
        p.NgayHoaDon,
        p.TenNguoiGiao,
        p.SoDienThoaiNguoiGiao,
        p.IDNhanSuNhan,
        ns.TenNhanSu AS TenNhanSuNhan,
        p.TrangThai,
        p.TongTienHang,
        p.TongTienThue,
        p.TongCong,
        p.NgayTao,
        p.NguoiTao,
        u.FullName AS NguoiTaoText
    FROM [dbo].[KHO_PhieuNhap] p
    LEFT JOIN [dbo].[KHO_DanhSach] k ON p.IDKho = k.ID
    LEFT JOIN [dbo].[DM_NhaCungCap] ncc ON p.IDNhaCungCap = ncc.ID
    LEFT JOIN [dbo].[NS_NhanSu] ns ON p.IDNhanSuNhan = ns.ID
    LEFT JOIN [dbo].[ACL_Login] u ON p.NguoiTao = u.ID
    WHERE p.IsDeleted = 0
      AND (@TuNgay IS NULL OR p.NgayNhap >= @TuNgay)
      AND (@DenNgay IS NULL OR p.NgayNhap <= @DenNgay)
      AND (@SoChungTu IS NULL OR p.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR p.IDKho = @IDKho)
      AND (@IDNhaCungCap IS NULL OR p.IDNhaCungCap = @IDNhaCungCap)
      AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
      AND (@IDNhanSuNhan IS NULL OR p.IDNhanSuNhan = @IDNhanSuNhan)
    ORDER BY p.NgayNhap DESC, p.ID DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
