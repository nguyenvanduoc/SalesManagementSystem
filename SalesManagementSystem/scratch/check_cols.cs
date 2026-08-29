using System;
using System.Linq;
using SalesManagementSystem.Data;
using Dapper;

namespace CheckCols
{
    class Program
    {
        static void Main()
        {
            try
            {
                var db = new DbConnectionFactory();
                using (var conn = db.CreateConnection())
                {
                    conn.Open();
                    var cols = conn.Query<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KHO_PhieuXuat_ChiTiet'").ToList();
                    Console.WriteLine("KHO_PhieuXuat_ChiTiet Columns: " + string.Join(", ", cols));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
            }
        }
    }
}
