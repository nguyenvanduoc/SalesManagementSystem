using System;
using System.Configuration;
using System.Data;
using SalesManagementSystem.Data;
using Dapper;

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

                // Search for PT00045 in all tables with a string column
                var tables = conn.Query<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'");
                foreach (var tbl in tables)
                {
                    var cols = conn.Query<string>($"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tbl}' AND DATA_TYPE LIKE '%char%'");
                    foreach (var col in cols)
                    {
                        try
                        {
                            string sql = $"SELECT COUNT(1) FROM [{tbl}] WHERE [{col}] LIKE '%PT00045%'";
                            int cnt = conn.ExecuteScalar<int>(sql);
                            if (cnt > 0)
                            {
                                Console.WriteLine($"FOUND PT00045 in Table: {tbl}, Column: {col}");
                            }
                        }
                        catch {}
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
