<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="SalesManagementSystem.Helpers.Security" %>
<%@ Import Namespace="System.IO" %>
<%
    try {
        string connStr = ConfigManager.GetConnectionString("DefaultConnection");
        string sql = @"
ALTER PROCEDURE sp_KT_PhieuChi_GetList
    @TuNgay         DATETIME        = NULL,
    @DenNgay        DATETIME        = NULL,
    @SoPhieuChi     NVARCHAR(50)    = NULL,
    @IDNhaCungCap   INT             = NULL,
    @IDKhoanMucChi  INT             = NULL,
    @TrangThai      INT             = NULL,
    @NguoiNhanTien  NVARCHAR(250)   = NULL,
    @IDTaiKhoanThanhToan INT        = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        pc.ID,
        pc.SoPhieuChi,
        pc.NgayChi,
        pc.IDKhoanMucChi,
        km.TenKhoanMuc,
        pc.IDTaiKhoanThanhToan,
        tk.TenTaiKhoan  AS TenTaiKhoanThanhToan,
        tk.SoTaiKhoan,
        pc.IDNguoiNhan,
        ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, '') AS TenNguoiNhan,
        pc.NguoiNhanTien,
        pc.SoDienThoaiNguoiNhan,
        pc.IDNhaCungCap,
        ncc.TenNhaCungCap,
        pc.IDPhieuNhap,
        
        -- L?y chu?i các s? phi?u nh?p t? b?ng KT_PhieuChiChiTiet
        STUFF((
            SELECT ', ' + pn2.SoChungTu
            FROM KT_PhieuChiChiTiet ct
            INNER JOIN KHO_PhieuNhap pn2 ON ct.IDPhieuNhap = pn2.ID
            WHERE ct.IDPhieuChi = pc.ID
            FOR XML PATH('')
        ), 1, 2, '') AS SoPhieuNhap,

        pc.SoTienChi,
        pc.DienGiai,
        pc.TrangThai,
        pc.NgayTao,
        pc.NguoiTao,
        pc.NgayGhi,
        pc.LyDoHuy,
        
        -- C?t t? sinh SoTienPhanBo d? Grid hi?n th?
        (SELECT ISNULL(SUM(SoTienPhanBo), 0) FROM KT_PhieuChiChiTiet WHERE IDPhieuChi = pc.ID) AS SoTienPhanBo
        
    FROM KT_PhieuChi pc
    LEFT JOIN DM_KhoanMucChi        km  ON pc.IDKhoanMucChi       = km.ID
    LEFT JOIN DM_TaiKhoanThanhToan  tk  ON pc.IDTaiKhoanThanhToan = tk.ID
    LEFT JOIN NS_NhanSu             ns  ON pc.IDNguoiNhan          = ns.ID
    LEFT JOIN DM_NhaCungCap         ncc ON pc.IDNhaCungCap         = ncc.ID
    WHERE pc.IsDeleted = 0
      AND (@TuNgay        IS NULL OR CAST(pc.NgayChi AS DATE) >= CAST(@TuNgay AS DATE))
      AND (@DenNgay       IS NULL OR CAST(pc.NgayChi AS DATE) <= CAST(@DenNgay AS DATE))
      AND (@SoPhieuChi    IS NULL OR (
          pc.SoPhieuChi LIKE '%' + @SoPhieuChi + '%'
          OR EXISTS (
              SELECT 1 
              FROM KT_PhieuChiChiTiet ct
              INNER JOIN KHO_PhieuNhap pn2 ON ct.IDPhieuNhap = pn2.ID
              WHERE ct.IDPhieuChi = pc.ID AND pn2.SoChungTu LIKE '%' + @SoPhieuChi + '%'
          )
      ))
      AND (@IDNhaCungCap  IS NULL OR pc.IDNhaCungCap = @IDNhaCungCap)
      AND (@IDKhoanMucChi IS NULL OR pc.IDKhoanMucChi = @IDKhoanMucChi)
      AND (@TrangThai     IS NULL OR pc.TrangThai = @TrangThai)
      AND (@NguoiNhanTien IS NULL OR pc.NguoiNhanTien LIKE '%' + @NguoiNhanTien + '%' OR (ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, '')) LIKE '%' + @NguoiNhanTien + '%')
      AND (@IDTaiKhoanThanhToan IS NULL OR pc.IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan)
    ORDER BY pc.NgayChi DESC, pc.ID DESC;
END
";
        using (var conn = new SqlConnection(connStr)) {
            conn.Open();
            using (var cmd = new SqlCommand(sql, conn)) {
                cmd.ExecuteNonQuery();
            }
        }
        Response.Write("SUCCESS");
    } catch (Exception ex) {
        Response.Write("ERROR: " + ex.Message);
    }
%>
