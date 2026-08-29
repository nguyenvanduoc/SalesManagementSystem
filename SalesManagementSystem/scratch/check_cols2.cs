using System;
using System.Linq;
using SalesManagementSystem.Data;
using Dapper;

namespace CheckCols2
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
                    var cols = conn.Query<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NS_DonDatHangChiTiet'").ToList();
                    Console.WriteLine("NS_DonDatHangChiTiet Columns: " + string.Join(", ", cols));

                    var cols3 = conn.Query<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DON_DieuChinhDonHang_ChiTiet'").ToList();
                    Console.WriteLine("DON_DieuChinhDonHang_ChiTiet Columns: " + string.Join(", ", cols3));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
            }
        }
    }
}
