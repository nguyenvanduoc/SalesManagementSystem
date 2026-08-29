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

                var ctbhList = conn.Query("SELECT ID FROM BAN_ChungTuBanHang WHERE TrangThai IN (1, 2) AND IsDeleted = 0").ToList();
                var repo = new SalesManagementSystem.Repositories.ChungTuBanHangRepository(db);

                int syncedCount = 0;
                foreach (var c in ctbhList)
                {
                    int id = (int)c.ID;
                    var model = repo.GetById(id);
                    if (model != null && model.ChiTiets.Any())
                    {
                        repo.Update(model, 1, false, model.TrangThai);
                        syncedCount++;
                    }
                }

                Console.WriteLine($"Successfully synchronized {syncedCount} sales invoices with inventory transactions!");
            }
        }
    }
}
