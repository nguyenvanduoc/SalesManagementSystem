using System;
using System.IO;
using System.Configuration;
using SalesManagementSystem.Data;

class Program
{
    static void Main()
    {
        try
        {
            ConfigurationManager.AppSettings["ConfigFile"] = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\systemPublic.dat";
            ConfigurationManager.AppSettings["KeyPart1"] = "VanDuoc@123123!";
            AppDomain.CurrentDomain.SetData("DataDirectory", @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data");
            
            var db = new DbConnectionFactory();
            string sql = File.ReadAllText(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\sp_Dashboard_GetData_ALTER.sql");
            using (var conn = db.CreateConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    string sqlScript = File.ReadAllText(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\fix_check_chuyenkho.sql");
                    cmd.CommandText = sqlScript;
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("UPDATED sp_KHO_TonKho_CheckChuyenKho SUCCESSFUL");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
        }
    }
}
