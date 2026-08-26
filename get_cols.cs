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
            Console.WriteLine("--- KHO_PhieuXuat Columns ---");
            using (var cmd = new SqlCommand("SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KHO_PhieuXuat'", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader[0] + " (" + reader[1] + ")");
                }
            }

            Console.WriteLine("\n--- KHO_PhieuNhap Columns ---");
            using (var cmd = new SqlCommand("SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KHO_PhieuNhap'", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader[0] + " (" + reader[1] + ")");
                }
            }
        }
    }
}
