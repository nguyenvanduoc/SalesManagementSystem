CREATE OR ALTER PROCEDURE sp_BAN_ChungTuBanHang_GetDonHangList
    @TuNgay DATE = NULL,
    @DenNgay DATE = NULL,
    @SoDonHang NVARCHAR(50) = NULL,
    @IDKhachHang INT = NULL,
    @TrangThaiChungTu INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        d.ID AS IDDonDatHang,
        d.SoDonHang,
        d.NgayTaoDon,
        k.TenKhachHang,
        d.TongTien,
        c.ID AS IDChungTuBanHang,
        c.SoChungTu,
        c.NgayChungTu,
        c.TrangThai AS TrangThaiChungTu,
        d.PhiBocXep,
        d.SoDienThoaiTaiXe,
        d.HoTenTaiXe
    FROM NS_DonDatHang d
    LEFT JOIN NS_KhachHang k ON d.IDKhachHang = k.ID
    LEFT JOIN BAN_ChungTuBanHang c ON c.IDDonDatHang = d.ID
    WHERE (@TuNgay IS NULL OR d.NgayTaoDon >= @TuNgay)
      AND (@DenNgay IS NULL OR d.NgayTaoDon <= @DenNgay)
      AND (@SoDonHang IS NULL OR d.SoDonHang LIKE '%' + @SoDonHang + '%')
      AND (@IDKhachHang IS NULL OR d.IDKhachHang = @IDKhachHang)
      AND (@TrangThaiChungTu IS NULL OR ISNULL(c.TrangThai, 0) = @TrangThaiChungTu)
    ORDER BY d.NgayTaoDon DESC, d.ID DESC;
END
GO
