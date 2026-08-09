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
                string sql = @"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ACL_LoginSession_ID_IsDangHoatDong' AND object_id = OBJECT_ID('ACL_LoginSession'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ACL_LoginSession_ID_IsDangHoatDong ON ACL_LoginSession(ID, IsDangHoatDong);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ACL_PhanQuyen_IDLogin_IsChoPhep' AND object_id = OBJECT_ID('ACL_PhanQuyen'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ACL_PhanQuyen_IDLogin_IsChoPhep ON ACL_PhanQuyen(IDLogin, IsChoPhep) INCLUDE (IDAction);
END
";
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Database nonclustered indexes created successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Index creation notice: " + ex.Message);
        }
    }
}
