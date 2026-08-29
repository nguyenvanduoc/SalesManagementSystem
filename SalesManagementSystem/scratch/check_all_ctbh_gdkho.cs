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

                var ctbhList = conn.Query(@"
                    SELECT c.ID, c.SoChungTu, c.NgayChungTu, c.IDKho, c.TrangThai, k.TenKhoHang
                    FROM BAN_ChungTuBanHang c
                    LEFT JOIN DM_KhoHang k ON c.IDKho = k.ID
                    WHERE c.IsDeleted = 0
                    ORDER BY c.ID DESC").ToList();

                Console.WriteLine($"Total BAN_ChungTuBanHang records: {ctbhList.Count}");
                Console.WriteLine("--------------------------------------------------");

                foreach (var c in ctbhList)
                {
                    int cId = (int)c.ID;
                    int trangThai = (int)c.TrangThai;
                    int idKho = c.IDKho != null ? (int)c.IDKho : 0;
                    string soCt = c.SoChungTu;

                    var px = conn.QueryFirstOrDefault("SELECT ID, SoChungTu, IDKho, TrangThai FROM KHO_PhieuXuat WHERE IDChungTuBanHang = @ID", new { ID = cId });
                    string soPx = px != null ? (string)px.SoChungTu : null;
                    int gdkCount = 0;
                    if (soPx != null)
                    {
                        gdkCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM KHO_GiaoDichKho WHERE SoChungTu = @SoPx AND LoaiChungTu = 2", new { SoPx = soPx });
                    }

                    var chiTietsCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM BAN_ChungTuBanHang_ChiTiet WHERE IDChungTuBanHang = @ID", new { ID = cId });

                    string statusName = trangThai == 1 ? "1 (Đề nghị ghi)" : (trangThai == 2 ? "2 (Đã ghi)" : (trangThai == 4 ? "4 (Lưu nháp)" : trangThai.ToString()));

                    Console.WriteLine($"CTBH #{cId} [{soCt}] - TrangThai: {statusName} - Kho: {c.TenKhoHang} (ID={idKho}) - Details: {chiTietsCount}");
                    Console.WriteLine($"   PX: {(px != null ? px.SoChungTu + " (IDKho=" + px.IDKho + ", Status=" + px.TrangThai + ")" : "NULL")}");
                    Console.WriteLine($"   GDK Count: {gdkCount}");

                    if ((trangThai == 1 || trangThai == 2) && (gdkCount == 0 || gdkCount != chiTietsCount))
                    {
                        Console.WriteLine($"   *** WARNING: MISMATCH! Expected {chiTietsCount} GDK entries, but found {gdkCount}! ***");
                    }
                }
            }
        }
    }
}
