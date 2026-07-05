using System;
using System.IO;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connStr = "Server=localhost;Database=SalesManagementSystem;Integrated Security=True;";
        string sqlPath = @"C:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\create_dieu_chinh_phieu_nhap.sql";
        try
        {
            string sqlContent = File.ReadAllText(sqlPath);
            string[] commands = sqlContent.Split(new string[] { "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                foreach (string commandText in commands)
                {
                    if (string.IsNullOrWhiteSpace(commandText)) continue;
                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            Console.WriteLine("Success");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
