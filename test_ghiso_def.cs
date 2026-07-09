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
            using (var cmd = new SqlCommand("sp_helptext", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@objname", "sp_KHO_PhieuNhap_GhiSo");
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.Write(reader[0]);
                    }
                }
            }
        }
    }
}
