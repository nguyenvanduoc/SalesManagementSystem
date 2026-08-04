using System;
using System.Data.SqlClient;
using Dapper;

class Program
{
    static void Main()
    {
        var connStr = "Server=localhost;Database=SalesManagementSystem;Trusted_Connection=True;";
        using (var conn = new SqlConnection(connStr))
        {
            var data = conn.Query("SELECT ID, TenKhachHang FROM NS_KhachHang");
            foreach(var row in data)
            {
                Console.WriteLine(row.ID + " - " + row.TenKhachHang);
            }
        }
    }
}
