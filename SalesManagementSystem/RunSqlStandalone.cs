using System;
using System.IO;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        try {
            string sqlPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\create_dieu_chinh_phieu_nhap.sql";
            string sql = File.ReadAllText(sqlPath);
            
            // Need to get connection string from system.dat
            string systemDatPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\system.dat";
            // Wait, system.dat might be encrypted. We need the same decryption logic.
            // Actually, we can just compile this by referencing the project's DLL!
        }
        catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
