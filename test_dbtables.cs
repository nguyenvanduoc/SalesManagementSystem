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
            // Kiem tra cac bang trong DB
            using (var cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME LIKE '%LoaiNhap%' OR TABLE_NAME LIKE '%KHO%'", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("Tables in DB:");
                    while (reader.Read())
                    {
                        Console.WriteLine("  " + reader["TABLE_NAME"]);
                    }
                }
            }
        }
    }
}
