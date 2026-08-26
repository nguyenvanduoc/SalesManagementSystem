using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connStr = "Data Source=DESKTOP-PC;Initial Catalog=SalesWarehouseDB;User ID=sa;Password=VanDuoc@123";
        using (var conn = new SqlConnection(connStr))
        {
            conn.Open();
            string sql = @"
CREATE OR ALTER PROCEDURE sp_KHO_PhieuXuat_GetList
    @Page INT = 1,
    @PageSize INT = 20,
    @TuNgay NVARCHAR(10) = NULL,
    @DenNgay NVARCHAR(10) = NULL,
    @SoChungTu NVARCHAR(50) = NULL,
    @IDKho INT = NULL,
    @TrangThai INT = NULL,
    @IDNhanSuNhan INT = NULL,
    @IDSanPham INT = NULL,
    @IDNhaCungCap INT = NULL,
    @TenNguoiGiao NVARCHAR(100) = NULL,
    @IDPhuongTien INT = NULL,
    @TenNguoiNhan NVARCHAR(100) = NULL,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT DISTINCT
        px.ID,
        px.SoChungTu,
        px.NgayXuat,
        px.IDDonDatHang,
        px.IDChungTuBanHang,
        px.IDKho,
        k.TenKhoHang AS TenKhoHang,
        px.IDNhanSuNhan,
        px.TenNguoiNhan,
        px.GhiChu,
        px.TongTienHang,
        px.TongTienThue,
        px.TongCong,
        px.TrangThai,
        
        -- Thông tin Đơn hàng / Khách hàng
        dh.SoDonHang,
        dh.NgayTaoDon AS NgayDonHang,
        dh.TrangThaiDon AS TrangThaiDonHang,
        kh.TenKhachHang
    INTO #TempList
    FROM KHO_PhieuXuat px
    LEFT JOIN BAN_ChungTuBanHang ctbh ON px.IDChungTuBanHang = ctbh.ID AND ctbh.IsDeleted = 0
    LEFT JOIN DM_KhoHang k ON px.IDKho = k.ID
    LEFT JOIN NS_DonDatHang dh ON px.IDDonDatHang = dh.ID
    LEFT JOIN NS_KhachHang kh ON dh.IDKhachHang = kh.ID OR ctbh.IDKhachHang = kh.ID
    LEFT JOIN KHO_PhieuXuat_ChiTiet ct ON px.ID = ct.IDPhieuXuat
    WHERE px.IsDeleted = 0
      AND (@TuNgay IS NULL OR @TuNgay = '' OR px.NgayXuat >= @TuNgay)
      AND (@DenNgay IS NULL OR @DenNgay = '' OR px.NgayXuat <= @DenNgay)
      AND (@SoChungTu IS NULL OR @SoChungTu = '' OR px.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@IDKho IS NULL OR @IDKho = 0 OR px.IDKho = @IDKho)
      AND (@TrangThai IS NULL OR px.TrangThai = @TrangThai)
      AND (@IDNhanSuNhan IS NULL OR @IDNhanSuNhan = 0 OR px.IDNhanSuNhan = @IDNhanSuNhan)
      AND (@IDSanPham IS NULL OR @IDSanPham = 0 OR ct.IDSanPham = @IDSanPham)
      AND (@IDNhaCungCap IS NULL OR @IDNhaCungCap = 0 OR kh.ID = @IDNhaCungCap OR dh.IDKhachHang = @IDNhaCungCap OR ctbh.IDKhachHang = @IDNhaCungCap)
      AND (@TenNguoiNhan IS NULL OR @TenNguoiNhan = '' OR px.TenNguoiNhan LIKE N'%' + @TenNguoiNhan + '%');

    -- Lấy tổng số dòng
    SELECT @TotalRecords = COUNT(*) FROM #TempList;

    -- Lấy dữ liệu phân trang
    SELECT * 
    FROM #TempList
    ORDER BY NgayXuat DESC, ID DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    DROP TABLE #TempList;
END";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("sp_KHO_PhieuXuat_GetList updated successfully!");
            }
        }
    }
}
