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
        k.TenKhoHang AS TenKho,
        k.MaKhoHang AS MaKhoHang,
        p.IDNhaCungCap,
        ncc.TenNhaCungCap AS TenNhaCungCap,
        ncc.MaNhaCungCap AS MaNhaCungCap,
        p.SoHoaDon,
        p.NgayHoaDon,
        p.TenNguoiGiao,
        p.SoDienThoaiNguoiGiao,
        p.IDNhanSuNhan,
        ns.Ten AS TenNhanSuNhan,
        p.TrangThai,
        p.TongTienHang,
        p.TongTienThue,
        p.TongCong,
        p.NgayTao,
        p.NguoiTao,
        COALESCE(
            NULLIF(LTRIM(RTRIM(ISNULL(nsTaoDirect.HoDem, '') + ' ' + ISNULL(nsTaoDirect.Ten, ''))), ''),
            NULLIF(LTRIM(RTRIM(ISNULL(nsTaoViaUser.HoDem, '') + ' ' + ISNULL(nsTaoViaUser.Ten, ''))), ''),
            u.TenDangNhap,
            ''
        ) AS NguoiTaoText
    FROM [dbo].[KHO_PhieuNhap] p
    LEFT JOIN [dbo].[DM_KhoHang] k ON p.IDKho = k.ID
    LEFT JOIN [dbo].[DM_NhaCungCap] ncc ON p.IDNhaCungCap = ncc.ID
    LEFT JOIN [dbo].[NS_NhanSu] ns ON p.IDNhanSuNhan = ns.ID
    -- Lấy thông tin nhân sự trực tiếp nếu p.NguoiTao lưu IDNhanSu
    LEFT JOIN [dbo].[NS_NhanSu] nsTaoDirect ON p.NguoiTao = nsTaoDirect.ID
    -- Lấy thông tin nhân sự thông qua bảng login nếu p.NguoiTao lưu UserID
    LEFT JOIN [dbo].[ACL_Login] u ON p.NguoiTao = u.ID
    LEFT JOIN [dbo].[NS_NhanSu] nsTaoViaUser ON u.IDNhanSu = nsTaoViaUser.ID
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
GO
