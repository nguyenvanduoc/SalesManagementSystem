CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_GetDonHangList
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @SoDonHang NVARCHAR(50) = NULL,
    @IDKhachHang INT = NULL,
    @TrangThaiChungTu INT = NULL,
    @IDSanPham INT = NULL,
    @IDPhuongTien INT = NULL,
    @HoTenTaiXe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        d.ID AS IDDonDatHang,
        d.SoDonHang,
        d.NgayTaoDon,
        k.TenKhachHang,
        ISNULL(c.TongCong, d.TongTien) AS TongTien,
        ISNULL(ct.ThanhTienHang, ISNULL(dt.ThanhTienHang, d.ThanhTienHang)) AS ThanhTienHang,
        c.ID AS IDChungTuBanHang,
        c.SoChungTu,
        c.NgayChungTu,
        ISNULL(ct.ThanhTienBocXep, ISNULL(dt.ThanhTienBocXep, ISNULL(c.PhiBocXep, d.PhiBocXep))) AS PhiBocXep,
        d.HoTenTaiXe,
        d.SoDienThoaiTaiXe,
        CASE WHEN d.TrangThaiDon = 4 THEN 3 ELSE c.TrangThai END AS TrangThaiChungTu,
        ISNULL(s_ct.MaSanPham, s_dt.MaSanPham) AS MaSanPham,
        ISNULL(s_ct.TenSanPham, s_dt.TenSanPham) AS TenSanPham,
        ISNULL(ct.SoLuong, ISNULL(dt.SoLuong, 0)) AS SoLuong
    FROM NS_DonDatHang d
    LEFT JOIN NS_KhachHang k ON d.IDKhachHang = k.ID
    LEFT JOIN BAN_ChungTuBanHang c ON c.IDDonDatHang = d.ID
    LEFT JOIN BAN_ChungTuBanHang_ChiTiet ct ON c.ID IS NOT NULL AND ct.IDChungTuBanHang = c.ID
    LEFT JOIN DM_SanPham s_ct ON ct.IDSanPham = s_ct.ID
    LEFT JOIN NS_DonDatHangChiTiet dt ON c.ID IS NULL AND dt.IDDonDatHang = d.ID
    LEFT JOIN DM_SanPham s_dt ON dt.IDSanPham = s_dt.ID
    WHERE (@TuNgay IS NULL OR d.NgayTaoDon >= @TuNgay)
      AND (@DenNgay IS NULL OR d.NgayTaoDon <= @DenNgay)
      AND (@SoDonHang IS NULL OR d.SoDonHang LIKE '%' + @SoDonHang + '%' OR c.SoChungTu LIKE '%' + @SoDonHang + '%')
      AND (@IDKhachHang IS NULL OR d.IDKhachHang = @IDKhachHang)
      AND (@TrangThaiChungTu IS NULL OR (CASE WHEN d.TrangThaiDon = 4 THEN 3 ELSE ISNULL(c.TrangThai, 0) END) = @TrangThaiChungTu)
      AND (@IDSanPham IS NULL OR ISNULL(s_ct.ID, s_dt.ID) = @IDSanPham OR EXISTS (SELECT 1 FROM NS_DonDatHangChiTiet dt_check WHERE dt_check.IDDonDatHang = d.ID AND dt_check.IDSanPham = @IDSanPham) OR EXISTS (SELECT 1 FROM BAN_ChungTuBanHang_ChiTiet ct_check WHERE ct_check.IDChungTuBanHang = c.ID AND ct_check.IDSanPham = @IDSanPham))
      AND (@IDPhuongTien IS NULL OR d.IDPhuongTien = @IDPhuongTien)
      AND (@HoTenTaiXe IS NULL OR d.HoTenTaiXe LIKE '%' + @HoTenTaiXe + '%' OR d.SoDienThoaiTaiXe LIKE '%' + @HoTenTaiXe + '%')
    ORDER BY 
        CASE 
            WHEN (CASE WHEN d.TrangThaiDon = 4 THEN 3 ELSE ISNULL(c.TrangThai, 0) END) = 0 THEN 1
            WHEN (CASE WHEN d.TrangThaiDon = 4 THEN 3 ELSE ISNULL(c.TrangThai, 0) END) = 4 THEN 2
            WHEN (CASE WHEN d.TrangThaiDon = 4 THEN 3 ELSE ISNULL(c.TrangThai, 0) END) = 1 THEN 3
            WHEN (CASE WHEN d.TrangThaiDon = 4 THEN 3 ELSE ISNULL(c.TrangThai, 0) END) = 2 THEN 4
            ELSE 5
        END ASC,
        d.SoDonHang DESC,
        d.ID DESC,
        ISNULL(ct.ID, dt.ID) ASC;
END;
