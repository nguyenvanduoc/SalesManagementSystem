-- =========================================================================================
-- Script Name: fix_hienthi_phieuchuyenkho.sql
-- Description: Cập nhật Stored Procedures để hiển thị Phiếu chuyển kho nội bộ khi lọc theo Kho Xuất.
--              Cung cấp câu truy vấn nhận diện các phiếu có khả năng bị nhập ngược thông tin kho.
-- =========================================================================================

-- =========================================================================================
-- 1. Cập nhật sp_KHO_PhieuNhap_GetList (Dùng cho trang Quản lý Phiếu Nhập)
-- =========================================================================================
GO
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
    @IDSanPham INT = NULL,
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
      AND (@IDKho IS NULL OR p.IDKho = @IDKho OR p.IDKhoNguon = @IDKho)
      AND (@IDNhaCungCap IS NULL OR p.IDNhaCungCap = @IDNhaCungCap)
      AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
      AND (LEN(ISNULL(@TenNguoiNhan,'')) = 0 OR ISNULL(NULLIF(p.TenNguoiNhan, ''), ns.Ten) LIKE N'%' + @TenNguoiNhan + N'%')
      AND (LEN(ISNULL(@TenNguoiGiao,'')) = 0 OR p.TenNguoiGiao LIKE N'%' + @TenNguoiGiao + N'%')
      AND (@IDPhuongTien IS NULL OR p.IDPhuongTien = @IDPhuongTien)
      AND (@IDSanPham IS NULL OR EXISTS (SELECT 1 FROM [dbo].[KHO_PhieuNhap_ChiTiet] ct WHERE ct.IDPhieuNhap = p.ID AND ct.IDSanPham = @IDSanPham))

    -- Trở về danh sách
    SELECT 
        p.ID,
        p.SoChungTu,
        p.NgayNhap,
        p.IDKho,
        k.TenKhoHang AS TenKho,
        k.MaKhoHang AS MaKhoHang,
        p.IDKhoNguon,
        kng.TenKhoHang AS TenKhoNguon,
        p.IDLoaiNhapKho,
        ln.TenLoaiNhap AS TenLoaiNhap,
        ln.MaLoaiNhap AS MaLoaiNhap,
        p.IDNhaCungCap,
        ncc.TenNhaCungCap AS TenNhaCungCap,
        ncc.MaNhaCungCap AS MaNhaCungCap,
        p.IDKhachHang,
        kh.TenKhachHang AS TenKhachHang,
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
        NguoiTaoText = ISNULL(nsTao.HoDem + ' ' + nsTao.Ten, ''),
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
        ISNULL(vc.TongTienVanChuyen, ISNULL(p.TienVanChuyen, 0)) AS TienVanChuyen,
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
    LEFT JOIN (
        SELECT IDPhieuNhap, SUM(ISNULL(TienVanChuyen, ISNULL(DonGiaVanChuyen, 0) * ISNULL(SoLuong, 0))) AS TongTienVanChuyen
        FROM [dbo].[KHO_PhieuNhap_ChiTiet]
        GROUP BY IDPhieuNhap
    ) vc ON vc.IDPhieuNhap = p.ID
    LEFT JOIN [dbo].[DM_KhoHang] k ON p.IDKho = k.ID
    LEFT JOIN [dbo].[DM_KhoHang] kng ON p.IDKhoNguon = kng.ID
    LEFT JOIN [dbo].[DM_LoaiNhapKho] ln ON p.IDLoaiNhapKho = ln.ID
    LEFT JOIN [dbo].[NS_KhachHang] kh ON p.IDKhachHang = kh.ID
    LEFT JOIN [dbo].[DM_NhaCungCap] ncc ON p.IDNhaCungCap = ncc.ID
    LEFT JOIN [dbo].[NS_NhanSu] ns ON p.IDNhanSuNhan = ns.ID
    LEFT JOIN [dbo].[NS_NhanSu] nsTao ON p.NguoiTao = nsTao.ID
    LEFT JOIN [dbo].[DM_PhuongTien] pt ON p.IDPhuongTien = pt.ID
    WHERE p.IsDeleted = 0
      AND (@TuNgay IS NULL OR p.NgayNhap >= @TuNgay)
      AND (@DenNgay IS NULL OR p.NgayNhap <= @DenNgay)
      AND (@SoChungTu IS NULL OR p.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR p.IDKho = @IDKho OR p.IDKhoNguon = @IDKho)
      AND (@IDNhaCungCap IS NULL OR p.IDNhaCungCap = @IDNhaCungCap)
      AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
      AND (LEN(ISNULL(@TenNguoiNhan,'')) = 0 OR ISNULL(NULLIF(p.TenNguoiNhan, ''), ns.Ten) LIKE N'%' + @TenNguoiNhan + N'%')
      AND (LEN(ISNULL(@TenNguoiGiao,'')) = 0 OR p.TenNguoiGiao LIKE N'%' + @TenNguoiGiao + N'%')
      AND (@IDPhuongTien IS NULL OR p.IDPhuongTien = @IDPhuongTien)
      AND (@IDSanPham IS NULL OR EXISTS (SELECT 1 FROM [dbo].[KHO_PhieuNhap_ChiTiet] ct WHERE ct.IDPhieuNhap = p.ID AND ct.IDSanPham = @IDSanPham))
    ORDER BY p.NgayNhap DESC, p.ID DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- =========================================================================================
-- 2. Cập nhật sp_KHO_DieuChinhPhieuNhap_GetPaged (Dùng cho trang Điều chỉnh phiếu nhập)
-- =========================================================================================
GO
CREATE OR ALTER PROCEDURE sp_KHO_DieuChinhPhieuNhap_GetPaged
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @IDLoaiNhapKho INT = NULL,
    @IDKho INT = NULL,
    @IDNhaCungCap INT = NULL,
    @IDKhachHang INT = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @ChiDonDieuChinh BIT = 0,
    @Offset INT = 0,
    @PageSize INT = 10,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Loc du lieu vao bang tam
    SELECT pn.ID INTO #FilteredPhieuNhap
    FROM KHO_PhieuNhap pn
    WHERE pn.TrangThai = 2 AND pn.IsDeleted = 0
        AND (@TuNgay IS NULL OR pn.NgayNhap >= @TuNgay)
        AND (@DenNgay IS NULL OR pn.NgayNhap <= @DenNgay)
        AND (@IDLoaiNhapKho IS NULL OR pn.IDLoaiNhapKho = @IDLoaiNhapKho)
        AND (@IDKho IS NULL OR pn.IDKho = @IDKho OR pn.IDKhoNguon = @IDKho)
        AND (@IDNhaCungCap IS NULL OR pn.IDNhaCungCap = @IDNhaCungCap)
        AND (@IDKhachHang IS NULL OR pn.IDKhachHang = @IDKhachHang)
        AND (@SoChungTu IS NULL OR pn.SoChungTu LIKE '%' + @SoChungTu + '%')
        AND (@ChiDonDieuChinh = 0 OR EXISTS (SELECT 1 FROM KHO_DieuChinhPhieuNhap dc WHERE dc.IDPhieuNhap = pn.ID));

    -- Dem tong so ban ghi
    SELECT @TotalRecords = COUNT(1) FROM #FilteredPhieuNhap;

    -- Lay du lieu phan trang
    SELECT
        pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TrangThai,
        pn.IDLoaiNhapKho,
        ln.TenLoaiNhap,
        pn.IDKho,
        k.TenKhoHang AS TenKhoNhap,
        pn.IDKhoNguon,
        kng.TenKhoHang AS TenKhoNguon,
        CASE 
            WHEN ln.MaLoaiNhap = 'NHAP_MUA' THEN ncc.TenNhaCungCap
            WHEN ln.MaLoaiNhap = 'TRA_HANG' THEN kh.TenKhachHang
            ELSE ''
        END AS DoiTuong,
        ISNULL((SELECT SUM(TongSauThue) FROM KHO_PhieuNhap_ChiTiet ct WHERE ct.IDPhieuNhap = pn.ID), 0) AS TongTien,
        ISNULL((
            SELECT SUM(pc.SoTienChi) 
            FROM KT_PhieuChi pc 
            WHERE pc.IDPhieuNhap = pn.ID AND pc.TrangThai = 2 AND pc.IsDeleted = 0
        ), 0) AS DaThanhToan,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM KHO_DieuChinhPhieuNhap dc WHERE dc.IDPhieuNhap = pn.ID) THEN 1 ELSE 0 END AS BIT) AS DaDieuChinh,
        ISNULL((SELECT COUNT(1) FROM KHO_DieuChinhPhieuNhap dc WHERE dc.IDPhieuNhap = pn.ID), 0) AS SoLanDieuChinh,
        (SELECT MAX(dc.NgayDieuChinh) FROM KHO_DieuChinhPhieuNhap dc WHERE dc.IDPhieuNhap = pn.ID) AS NgayDieuChinhCuoi,
        CASE pn.TrangThai WHEN 1 THEN N'Mới tạo' WHEN 2 THEN N'Đã ghi sổ' WHEN 3 THEN N'Đã thanh toán' ELSE N'' END AS TenTrangThai
    FROM KHO_PhieuNhap pn
    INNER JOIN #FilteredPhieuNhap f ON pn.ID = f.ID
    LEFT JOIN DM_LoaiNhapKho ln ON pn.IDLoaiNhapKho = ln.ID
    LEFT JOIN DM_KhoHang k ON pn.IDKho = k.ID
    LEFT JOIN DM_KhoHang kng ON pn.IDKhoNguon = kng.ID
    LEFT JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN NS_KhachHang kh ON pn.IDKhachHang = kh.ID
    ORDER BY pn.ID DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    DROP TABLE #FilteredPhieuNhap;
END
GO


-- =========================================================================================
-- 3. Truy vấn kiểm tra dữ liệu cũ:
-- Giúp xác định các phiếu CHUYỂN KHO NỘI BỘ bị lưu ngược Kho Nhập / Kho Xuất.
-- Nếu người dùng không tìm thấy phiếu, họ có thể đã cố tình chọn "Nhà máy vinaken" 
-- vào mục "Kho Nhập" (IDKho) thay vì "Kho Xuất" (IDKhoNguon) để phiếu hiện lên.
-- =========================================================================================
/*
SELECT 
    pn.ID,
    pn.SoChungTu, 
    pn.NgayNhap, 
    ln.TenLoaiNhap,
    pn.IDKho, 
    k.TenKhoHang AS [Kho Nhập (Thực tế đang lưu)], 
    pn.IDKhoNguon, 
    kng.TenKhoHang AS [Kho Xuất (Thực tế đang lưu)],
    pn.TrangThai,
    pn.NguoiTao
FROM KHO_PhieuNhap pn
INNER JOIN DM_LoaiNhapKho ln ON pn.IDLoaiNhapKho = ln.ID
LEFT JOIN DM_KhoHang k ON pn.IDKho = k.ID
LEFT JOIN DM_KhoHang kng ON pn.IDKhoNguon = kng.ID
WHERE ln.MaLoaiNhap = 'CHUYEN_KHO' 
  AND pn.IsDeleted = 0
ORDER BY pn.NgayNhap DESC;

-- Nếu anh phát hiện IDKho và IDKhoNguon bị ngược (VD: Phiếu đáng lẽ xuất từ Vinaken 
-- sang Kho A, nhưng lại lưu là xuất từ Kho A sang Vinaken), anh có thể chạy lệnh CẬP NHẬT sau:
-- (Bỏ comment để chạy, nhớ sửa list ID ở IN (...) cho chính xác)

BEGIN TRAN;
    UPDATE KHO_PhieuNhap
    SET IDKho = IDKhoNguon,
        IDKhoNguon = IDKho
    WHERE ID IN (/* Điền danh sách ID các phiếu bị sai vào đây, ví dụ: 101, 102 */);

    -- Lưu ý: Nếu phiếu đã được 'Ghi sổ' (TrangThai = 2), việc cập nhật trên KHO_PhieuNhap
    -- cần đi kèm với cập nhật KHO_GiaoDichKho tương ứng nếu sổ kho bị ảnh hưởng.
COMMIT TRAN;
*/
