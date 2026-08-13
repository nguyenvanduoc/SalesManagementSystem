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

                int nccId = 1;

                // 1. Check GetTienTraTruocNhaCungCap SQL result
                decimal tienTraTruocRepo = conn.QueryFirstOrDefault<decimal>(
                    @"SELECT ISNULL(SUM(CASE WHEN LoaiChi = 2 THEN SoTienPhanBo ELSE -SoTienPhanBo END), 0)
                      FROM KT_PhieuChiChiTiet ct
                      INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                      WHERE pc.IsDeleted = 0 AND pc.TrangThai = 2
                        AND pc.IDNhaCungCap = @IDNhaCungCap
                        AND ct.LoaiChi IN (2, 3)",
                    new { IDNhaCungCap = nccId }
                );

                Console.WriteLine($"GetTienTraTruocNhaCungCap result for NCC {nccId}: {tienTraTruocRepo:N0}");

                // 2. Count details by LoaiChi for Vinaken
                var detailsByLoaiChi = conn.Query(@"
                    SELECT ct.LoaiChi, COUNT(1) AS SoLuong, ISNULL(SUM(ct.SoTienPhanBo), 0) AS TongTienPhanBo
                    FROM KT_PhieuChiChiTiet ct
                    INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                    WHERE pc.IDNhaCungCap = @IDNhaCungCap AND pc.IsDeleted = 0 AND pc.TrangThai = 2
                    GROUP BY ct.LoaiChi", new { IDNhaCungCap = nccId });

                Console.WriteLine("\nPhan bo chi tiet theo LoaiChi:");
                foreach (var d in detailsByLoaiChi)
                {
                    Console.WriteLine($"   LoaiChi={d.LoaiChi}: SoLuong={d.SoLuong}, TongPhanBo={d.TongTienPhanBo:N0}");
                }

                // 3. Check PhieuChi where SoTienChi > Sum(SoTienPhanBo)
                var unallocatedPcs = conn.Query(@"
                    SELECT pc.ID, pc.SoPhieuChi, pc.NgayChi, pc.SoTienChi,
                           ISNULL((SELECT SUM(SoTienPhanBo) FROM KT_PhieuChiChiTiet WHERE IDPhieuChi = pc.ID), 0) AS TongPhanBo
                    FROM KT_PhieuChi pc
                    WHERE pc.IDNhaCungCap = @IDNhaCungCap AND pc.IsDeleted = 0 AND pc.TrangThai = 2
                      AND pc.SoTienChi > ISNULL((SELECT SUM(SoTienPhanBo) FROM KT_PhieuChiChiTiet WHERE IDPhieuChi = pc.ID), 0)",
                    new { IDNhaCungCap = nccId });

                Console.WriteLine($"\nPhieu Chi co SoTienChi > TongPhanBo (Co phan tien du chua tao dong LoaiChi=2): {unallocatedPcs.Count()} phieu");
                foreach (var pc in unallocatedPcs)
                {
                    decimal excess = (decimal)pc.SoTienChi - (decimal)pc.TongPhanBo;
                    Console.WriteLine($"   PC={pc.SoPhieuChi}, Ngay={pc.NgayChi:yyyy-MM-dd}, SoTienChi={pc.SoTienChi:N0}, TongPhanBo={pc.TongPhanBo:N0}, TienDuChuaPhanBo={excess:N0}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message));
        }
    }
}
