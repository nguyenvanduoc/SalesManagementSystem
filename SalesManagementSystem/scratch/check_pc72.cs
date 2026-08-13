using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper;

class Program
{
    static void Main()
    {
        try
        {
            Assembly asm = Assembly.LoadFrom(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll");
            Type factoryType = asm.GetType("SalesManagementSystem.Data.DbConnectionFactory");
            object factory = Activator.CreateInstance(factoryType);
            MethodInfo createConnMethod = factoryType.GetMethod("CreateConnection");

            using (IDbConnection conn = (IDbConnection)createConnMethod.Invoke(factory, null))
            {
                conn.Open();

                // Get PC26000072 or latest PhieuChi for Vinaken
                var pc = conn.QueryFirstOrDefault(@"
                    SELECT pc.ID, pc.SoPhieuChi, pc.NgayChi, pc.SoTienChi, pc.IDNhaCungCap, pc.TrangThai, pc.IsDeleted
                    FROM KT_PhieuChi pc
                    WHERE pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0
                    ORDER BY pc.ID DESC");

                if (pc != null)
                {
                    Console.WriteLine($"Latest PC: ID={pc.ID}, So={pc.SoPhieuChi}, Ngay={pc.NgayChi:yyyy-MM-dd}, SoTienChi={pc.SoTienChi:N0}, TrangThai={pc.TrangThai}");

                    var chiTiets = conn.Query(@"
                        SELECT ct.ID, ct.IDPhieuNhap, pn.SoChungTu, ct.LoaiChi, ct.SoTienPhanBo, ct.DienGiai
                        FROM KT_PhieuChiChiTiet ct
                        LEFT JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
                        WHERE ct.IDPhieuChi = @ID", new { ID = (int)pc.ID }).ToList();

                    Console.WriteLine($"Chi tiêt PC {pc.SoPhieuChi}: ({chiTiets.Count} dòng)");
                    foreach (var ct in chiTiets)
                    {
                        Console.WriteLine($"   ctID={ct.ID}, PN={ct.SoChungTu} (ID={ct.IDPhieuNhap}), LoaiChi={ct.LoaiChi}, SoTienPhanBo={ct.SoTienPhanBo:N0}");
                    }
                }

                // Check all LoaiChi=2 or LoaiChi=3 for Vinaken across all PhieuChi
                var prepayments = conn.Query(@"
                    SELECT ct.ID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, pc.TrangThai, ct.LoaiChi, ct.SoTienPhanBo, ct.IDPhieuNhap, pn.SoChungTu
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    LEFT JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
                    WHERE pc.IDNhaCungCap = 1 AND pc.IsDeleted = 0 AND ct.LoaiChi IN (2, 3)
                    ORDER BY pc.ID, ct.ID");

                Console.WriteLine($"\nTat ca cac dong LoaiChi=2 hoac 3 cua Vinaken ({prepayments.Count()} dong):");
                foreach (var p in prepayments)
                {
                    Console.WriteLine($"   PC={p.SoPhieuChi} (ID={p.IDPhieuChi}), Ngay={p.NgayChi:yyyy-MM-dd}, TrangThai={p.TrangThai}, LoaiChi={p.LoaiChi}, SoTienPhanBo={p.SoTienPhanBo:N0}, PN={p.SoChungTu}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message));
        }
    }
}
