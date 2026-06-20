using System;
using System.Data;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connStr = "Data Source=.;Initial Catalog=SalesWarehouseDB;Integrated Security=True";
        using (var conn = new SqlConnection(connStr))
        {
            conn.Open();
            using (var cmd = new SqlCommand("sp_KHO_TonKho_GetList", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TuNgay", new DateTime(2026, 6, 18));
                cmd.Parameters.AddWithValue("@DenNgay", new DateTime(2026, 6, 30, 23, 59, 59));
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(string.Format("MaSP: {0}, TonDauKy: {1}, TongNhap: {2}, TongXuat: {3}, TonKho: {4}", reader["MaSanPham"], reader["TonDauKy"], reader["TongNhap"], reader["TongXuat"], reader["TonKho"]));
                    }
                }
            }
        }
    }
}
