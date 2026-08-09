using System;
using System.Configuration;
using System.Data;
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
            using (var conn = db.CreateConnection())
            {
                conn.Open();
                
                // Count junk rows (duration <= 2s and closed)
                string countSql = @"
                    SELECT COUNT(1) 
                    FROM ACL_LoginSession 
                    WHERE IsDangHoatDong = 0 
                      AND ThoiGianLogout IS NOT NULL 
                      AND DATEDIFF(second, ThoiGianLogin, ThoiGianLogout) <= 3";
                
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = countSql;
                    int junkCount = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine("Junk session rows count (duration <= 3s): " + junkCount);
                }

                // Delete junk rows (closed sessions with duration <= 3 seconds created by auto-login spam)
                string deleteSql = @"
                    DELETE FROM ACL_LoginSession 
                    WHERE IsDangHoatDong = 0 
                      AND ThoiGianLogout IS NOT NULL 
                      AND DATEDIFF(second, ThoiGianLogin, ThoiGianLogout) <= 3";
                
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = deleteSql;
                    int deleted = cmd.ExecuteNonQuery();
                    Console.WriteLine("Deleted " + deleted + " junk login session rows.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
