using System;
using System.IO;
using SalesManagementSystem.Data;
using Dapper;

namespace ExtractSP
{
    class Program
    {
        static void Main()
        {
            var db = new DbConnectionFactory();
            using (var conn = db.CreateConnection())
            {
                var definition = conn.QueryFirstOrDefault<string>("SELECT OBJECT_DEFINITION(OBJECT_ID('sp_Dashboard_GetData'))");
                File.WriteAllText(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\sp_Dashboard_GetData_def.txt", definition ?? "NOT_FOUND");
            }
        }
    }
}
