using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace TestCheck
{
    class Program
    {
        static void Main()
        {
            var db = new DbConnectionFactory();
            using (var conn = db.CreateConnection())
            {
                conn.Open();

                var dh = conn.QueryFirstOrDefault("SELECT * FROM NS_DonDatHang WHERE SoDonHang = 'DH26000169'");
                if (dh != null)
                {
                    Console.WriteLine("--- NS_DonDatHang DH26000169 ---");
                    var dict = (System.Collections.Generic.IDictionary<string, object>)dh;
                    foreach (var kvp in dict)
                    {
                        if (kvp.Value != null) Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
                    }
                }

                var ct = conn.QueryFirstOrDefault("SELECT * FROM BAN_ChungTuBanHang WHERE IDDonDatHang = @ID", new { ID = (int)dh.ID });
                if (ct != null)
                {
                    Console.WriteLine("\n--- BAN_ChungTuBanHang ---");
                    var dict = (System.Collections.Generic.IDictionary<string, object>)ct;
                    foreach (var kvp in dict)
                    {
                        if (kvp.Value != null) Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
                    }
                }
            }
        }
    }
}
