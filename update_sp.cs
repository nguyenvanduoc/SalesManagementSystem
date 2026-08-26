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
CREATE OR ALTER PROCEDURE sp_KHO_HaoHutHangHoa_GetDonHang
    @Keyword NVARCHAR(100)
AS
BEGIN
    SELECT TOP 50 
           d.ID, d.SoDonHang, d.NgayTaoDon, d.IDKhachHang, k.TenKhachHang, 
           c.ID AS IDChungTuBanHang, c.SoChungTu AS SoChungTuBanHang
    FROM NS_DonDatHang d
    INNER JOIN NS_KhachHang k ON d.IDKhachHang = k.ID
    LEFT JOIN BAN_ChungTuBanHang c ON d.ID = c.IDDonDatHang AND c.IsDeleted = 0 AND c.TrangThai IN (1, 2)
    WHERE d.TrangThaiDon NOT IN (0, 4)
      AND (@Keyword IS NULL OR @Keyword = '' OR d.SoDonHang LIKE '%' + @Keyword + '%' OR k.TenKhachHang LIKE N'%' + @Keyword + '%')
    ORDER BY d.NgayTaoDon DESC, d.ID DESC;
END";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("sp_KHO_HaoHutHangHoa_GetDonHang created/updated successfully!");
            }
        }
    }
}
