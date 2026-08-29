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

                var khos = conn.Query("SELECT * FROM DM_KhoHang").ToList();
                Console.WriteLine("--- DM_KhoHang ---");
                foreach (var k in khos)
                {
                    var dict = (System.Collections.Generic.IDictionary<string, object>)k;
                    Console.WriteLine("Kho row:");
                    foreach (var kvp in dict)
                    {
                        Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
                    }
                }

                Console.WriteLine("\n--- KHO_GiaoDichKho for PX00165 ---");
                var gdk = conn.Query("SELECT * FROM KHO_GiaoDichKho WHERE SoChungTu = 'PX00165'").ToList();
                foreach (var g in gdk)
                {
                    Console.WriteLine($"ID={g.ID}, SoChungTu={g.SoChungTu}, LoaiChungTu={g.LoaiChungTu}, IDKho={g.IDKho}, IDSanPham={g.IDSanPham}, SoLuongXuat={g.SoLuongXuat}, NgayChungTu={g.NgayChungTu}, IsHuy={g.IsHuy}");
                }
            }
        }
    }
}
