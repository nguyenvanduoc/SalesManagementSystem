using System;
using System.Data.SqlClient;

public class CheckDB
{
    public static void Main()
    {
        string connStr = "Data Source=.;Initial Catalog=QuanLyBanHang;Integrated Security=True";
        using (var conn = new SqlConnection(connStr))
        {
            conn.Open();
            using (var cmd = new SqlCommand("SELECT ID, SoChungTu, TrangThai, TongCong, TongTienHang, DaThanhToan, ConLai, IDNhaCungCap FROM KHO_PhieuNhap WHERE SoChungTu = 'PN26000009'", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine("ID: " + reader["ID"] + 
                                          ", SoChungTu: " + reader["SoChungTu"] + 
                                          ", TrangThai: " + reader["TrangThai"] + 
                                          ", TongCong: " + reader["TongCong"] + 
                                          ", TongTienHang: " + reader["TongTienHang"] + 
                                          ", DaThanhToan: " + reader["DaThanhToan"] + 
                                          ", ConLai: " + reader["ConLai"] + 
                                          ", IDNhaCungCap: " + reader["IDNhaCungCap"]);
                    }
                    else
                    {
                        Console.WriteLine("Not found!");
                    }
                }
            }
        }
    }
}
