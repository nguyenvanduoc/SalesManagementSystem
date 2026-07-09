using System;
using System.Data;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connStr = "Data Source=localhost;Initial Catalog=SalesWarehouseDB;Integrated Security=True";
        using (var conn = new SqlConnection(connStr))
        {
            conn.Open();
            // Tim cac phieu chuyen kho noi bo da dc ghi so (TrangThai=2) nhung chua co GiaoDichKho
            string sql = @"
SELECT p.ID, p.SoChungTu, p.TrangThai, p.IDKhoNguon, p.IDKho,
       ln.MaLoaiNhap,
       (SELECT COUNT(*) FROM KHO_GiaoDichKho gd WHERE gd.SoChungTu = p.SoChungTu AND gd.LoaiChungTu = 1) AS SoGiaoDich
FROM KHO_PhieuNhap p
LEFT JOIN DM_LoaiNhapKho ln ON p.IDLoaiNhapKho = ln.ID
WHERE ln.MaLoaiNhap = 'CHUYEN_KHO'
  AND p.IsDeleted = 0
ORDER BY p.TrangThai, p.ID";
            using (var cmd = new SqlCommand(sql, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine("ID:{0} SoChungTu:{1} TrangThai:{2} IDKhoNguon:{3} IDKhoDich:{4} MaLoai:{5} SoGiaoDich:{6}",
                            reader["ID"], reader["SoChungTu"], reader["TrangThai"], 
                            reader["IDKhoNguon"], reader["IDKho"], reader["MaLoaiNhap"],
                            reader["SoGiaoDich"]);
                    }
                }
            }
        }
    }
}
