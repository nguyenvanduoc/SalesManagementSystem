using System;
using SalesManagementSystem.Repositories;
using SalesManagementSystem.Data;

namespace SalesManagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var db = new DbConnectionFactory();
                var repo = new DashboardRepository(db);
                
                DateTime tuNgay = new DateTime(2026, 5, 1);
                DateTime denNgay = new DateTime(2026, 6, 28);
                
                Console.WriteLine("Invoking GetDashboardData for 2026-05-01 to 2026-06-28...");
                var data = repo.GetDashboardData(tuNgay, denNgay);
                
                Console.WriteLine("SUCCESS: Retrieve Dashboard data successfully.");
                Console.WriteLine("Total Orders (in period): " + data.TongSoDonHang);
                Console.WriteLine("Doanh thu: " + data.Summary.DoanhThu);
                Console.WriteLine("Loi nhuan: " + data.Summary.LoiNhuan);
                Console.WriteLine("Cong no NCC: " + data.Summary.CongNoNhaCungCap);
                Console.WriteLine("Recent Orders Count: " + (data.DonHangGanDay != null ? data.DonHangGanDay.Count : 0));
                
                if (data.DonHangGanDay != null && data.DonHangGanDay.Count > 0)
                {
                    Console.WriteLine("Recent Orders:");
                    foreach (var o in data.DonHangGanDay)
                    {
                        Console.WriteLine("- " + o.SoDonHang + " | " + o.NgayTaoDon + " | " + o.TongTien);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
            }
        }
    }
}
