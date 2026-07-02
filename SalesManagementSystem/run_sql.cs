using System;
using System.Data.SqlClient;
using System.IO;

class Program
{
    static void Main()
    {
        string sql = File.ReadAllText(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\sp_BC_KetQuaHoatDongKinhDoanh_GetList.sql");
        string[] batches = sql.Split(new[] { "\r\nGO", "\nGO", "\rGO", "GO\r", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
        using(var conn = new SqlConnection(@"Data Source=.;Initial Catalog=SalesManagementSystem;Integrated Security=True"))
        {
            conn.Open();
            foreach(var batch in batches)
            {
                if(string.IsNullOrWhiteSpace(batch)) continue;
                try {
                    using(var cmd = new SqlCommand(batch, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                } catch(Exception ex) {
                    Console.WriteLine("Error in batch: " + ex.Message);
                }
            }
        }
    }
}
