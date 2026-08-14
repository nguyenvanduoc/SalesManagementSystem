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
        ISNULL(c.TongTienHang, d.ThanhTienHang) AS ThanhTienHang,
        c.ID AS IDChungTuBanHang,
        c.SoChungTu,
        c.NgayChungTu,
        ISNULL(c.PhiBocXep, d.PhiBocXep) AS PhiBocXep,
        d.HoTenTaiXe,
        d.SoDienThoaiTaiXe,
        CASE WHEN d.TrangThaiDon = 4 THEN 3 ELSE c.TrangThai END AS TrangThaiChungTu
    FROM NS_DonDatHang d
    LEFT JOIN NS_KhachHang k ON d.IDKhachHang = k.ID
    LEFT JOIN BAN_ChungTuBanHang c ON c.IDDonDatHang = d.ID
    WHERE (@TuNgay IS NULL OR d.NgayTaoDon >= @TuNgay)
      AND (@DenNgay IS NULL OR d.NgayTaoDon <= @DenNgay)
      AND (@SoDonHang IS NULL OR d.SoDonHang LIKE '%' + @SoDonHang + '%' OR c.SoChungTu LIKE '%' + @SoDonHang + '%')
      AND (@IDKhachHang IS NULL OR d.IDKhachHang = @IDKhachHang)
      AND (@TrangThaiChungTu IS NULL OR (CASE WHEN d.TrangThaiDon = 4 THEN 3 ELSE ISNULL(c.TrangThai, 0) END) = @TrangThaiChungTu)
      AND (@IDSanPham IS NULL OR EXISTS (SELECT 1 FROM NS_DonDatHangChiTiet dt WHERE dt.IDDonDatHang = d.ID AND dt.IDSanPham = @IDSanPham) OR EXISTS (SELECT 1 FROM BAN_ChungTuBanHang_ChiTiet ct WHERE ct.IDChungTuBanHang = c.ID AND ct.IDSanPham = @IDSanPham))
      AND (@IDPhuongTien IS NULL OR d.IDPhuongTien = @IDPhuongTien)
      AND (@HoTenTaiXe IS NULL OR d.HoTenTaiXe LIKE '%' + @HoTenTaiXe + '%' OR d.SoDienThoaiTaiXe LIKE '%' + @HoTenTaiXe + '%')
    ORDER BY 
        CASE 
            WHEN (CASE WHEN d.TrangThaiDon = 4 THEN 3 ELSE ISNULL(c.TrangThai, 0) END) = 0 THEN 1
            WHEN (CASE WHEN d.TrangThaiDon = 4 THEN 3 ELSE ISNULL(c.TrangThai, 0) END) = 3 THEN 3
            ELSE 2
        END ASC,
        d.SoDonHang DESC,
        d.ID DESC;
END;
