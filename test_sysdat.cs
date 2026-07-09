using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        // Kiem tra system.dat
        string datPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\system.dat";
        if (File.Exists(datPath))
        {
            Console.WriteLine("system.dat contents (raw bytes):");
            byte[] bytes = File.ReadAllBytes(datPath);
            Console.WriteLine("Length: " + bytes.Length);
            Console.WriteLine("As text: " + Encoding.UTF8.GetString(bytes));
        }
        
        // Tim DB QuanLyBanHang
        string connStr = "Data Source=localhost;Initial Catalog=QuanLyBanHang;Integrated Security=True";
        try {
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM KHO_PhieuNhap", conn))
                {
                    Console.WriteLine("QuanLyBanHang KHO_PhieuNhap count: " + cmd.ExecuteScalar());
                }
            }
        } catch(Exception ex) {
            Console.WriteLine("QuanLyBanHang: " + ex.Message.Substring(0, Math.Min(100, ex.Message.Length)));
        }
    }
}
