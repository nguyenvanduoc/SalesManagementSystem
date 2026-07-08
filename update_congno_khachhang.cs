using System;
using System.IO;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\create_sp_CongNoKhachHang.sql";
        string content = File.ReadAllText(path);
        
        content = content.Replace("bh.TrangThai = 2", "bh.TrangThai IN (1, 2)");
        content = content.Replace("bh2.TrangThai = 2", "bh2.TrangThai IN (1, 2)");
        content = content.Replace("pt.TrangThai = 2", "pt.TrangThai IN (1, 2)");
        
        File.WriteAllText(path, content);
        Console.WriteLine("Done replacing TrangThai in create_sp_CongNoKhachHang.sql");
    }
}
