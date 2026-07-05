using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connStr = "Server=localhost;Database=SalesManagementSystem;Integrated Security=True;";
        string sql = @"
CREATE OR ALTER PROCEDURE dbo.sp_KHO_PhieuNhap_GetList
    @TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKho INT = NULL,
    @IDNhaCungCap INT = NULL,
    @TrangThai INT = NULL,
    @TenNguoiNhan NVARCHAR(200) = NULL,
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
      AND (LEN(ISNULL(@TenNguoiNhan,'')) = 0 OR p.TenNguoiNhan = @TenNguoiNhan)

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
        p.TenNguoiNhan,
        p.IDNhanSuNhan,
        ns.Ten AS TenNhanSuNhan,
        p.TrangThai,
        p.TongTienHang,
        p.TongTienThue,
        p.TongCong,
        p.NgayTao,
        p.NguoiTao,
        NguoiTaoText = nsTao.HoDem + ' ' + ns.Ten,
        p.IDPhuongTien,
        pt.TenPhuongTien AS TenPhuongTien,
        DaThanhToan = ISNULL(pay.DaThanhToan, 0),
        ConLai = p.TongCong - ISNULL(pay.DaThanhToan, 0),
        TrangThaiThanhToan = CASE 
            WHEN p.TongCong - ISNULL(pay.DaThanhToan, 0) <= 0 THEN 2
            WHEN ISNULL(pay.DaThanhToan, 0) > 0 THEN 1
            ELSE 0
        END
    FROM [dbo].[KHO_PhieuNhap] p
    LEFT JOIN [dbo].[DM_KhoHang] k ON p.IDKho = k.ID
    LEFT JOIN [dbo].[DM_NhaCungCap] ncc ON p.IDNhaCungCap = ncc.ID
    LEFT JOIN [dbo].[NS_NhanSu] ns ON p.IDNhanSuNhan = ns.ID
    LEFT JOIN [dbo].[ACL_Login] u ON p.NguoiTao = u.ID
    LEFT JOIN [dbo].[NS_NhanSu] nsTao ON u.IDNhanSu = nsTao.ID
    LEFT JOIN [dbo].[DM_PhuongTien] pt ON p.IDPhuongTien = pt.ID
    LEFT JOIN (
        SELECT ct.IDPhieuNhap, SUM(ct.SoTienPhanBo) AS DaThanhToan
        FROM KT_PhieuChiChiTiet ct
        JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
        WHERE pc.TrangThai = 2 AND pc.IsDeleted = 0 AND ct.LoaiChi = 1
        GROUP BY ct.IDPhieuNhap
    ) pay ON pay.IDPhieuNhap = p.ID
    WHERE p.IsDeleted = 0
      AND (@TuNgay IS NULL OR p.NgayNhap >= @TuNgay)
      AND (@DenNgay IS NULL OR p.NgayNhap <= @DenNgay)
      AND (@SoChungTu IS NULL OR p.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR p.IDKho = @IDKho)
      AND (@IDNhaCungCap IS NULL OR p.IDNhaCungCap = @IDNhaCungCap)
      AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
      AND (LEN(ISNULL(@TenNguoiNhan,'')) = 0 OR p.TenNguoiNhan = @TenNguoiNhan)
    ORDER BY p.NgayNhap DESC, p.ID DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
";
        try {
            using (var conn = new SqlConnection(connStr)) {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn)) {
                    cmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Success: Altered sp_KHO_PhieuNhap_GetList");
        } catch (Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
