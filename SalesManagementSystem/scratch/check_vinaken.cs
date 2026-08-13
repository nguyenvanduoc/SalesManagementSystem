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

                // 1. Find PN26000147
                var pn147 = conn.QueryFirstOrDefault(@"
                    SELECT ID, SoChungTu, NgayNhap, TongCong, IDNhaCungCap, TrangThai, TrangThaiThanhToan
                    FROM KHO_PhieuNhap
                    WHERE SoChungTu LIKE '%147%'");

                if (pn147 != null)
                {
                    Console.WriteLine($"PN147: ID={pn147.ID}, So={pn147.SoChungTu}, Ngay={pn147.NgayNhap}, TongCong={pn147.TongCong:N0}, NCC_ID={pn147.IDNhaCungCap}, TrangThai={pn147.TrangThai}, TrangThaiThanhToan={pn147.TrangThaiThanhToan}");

                    // Allocations for PN26000147
                    var alloc = conn.Query(@"
                        SELECT ct.ID, ct.IDPhieuChi, pc.SoPhieuChi, pc.NgayChi, pc.TrangThai AS TrangThaiPC, pc.IsDeleted, ct.LoaiChi, ct.SoTienPhanBo, ct.DienGiai
                        FROM KT_PhieuChiChiTiet ct
                        INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                        WHERE ct.IDPhieuNhap = @ID", new { ID = (int)pn147.ID });

                    Console.WriteLine("Allocations for PN26000147:");
                    decimal sumAlloc = 0;
                    foreach (var a in alloc)
                    {
                        if (a.TrangThaiPC == 2 && !a.IsDeleted && a.LoaiChi == 1) sumAlloc += (decimal)a.SoTienPhanBo;
                        Console.WriteLine($"   ctID={a.ID}, PC={a.SoPhieuChi}, NgayChi={a.NgayChi}, TrangThaiPC={a.TrangThaiPC}, IsDeleted={a.IsDeleted}, LoaiChi={a.LoaiChi}, SoTienPhanBo={a.SoTienPhanBo:N0}");
                    }
                    Console.WriteLine($"   TOTAL ALLOCATED (DaTra) TO PN147: {sumAlloc:N0}");
                    Console.WriteLine($"   REMAINING (ConNo) FOR PN147: {(decimal)pn147.TongCong - sumAlloc:N0}");
                }
                else
                {
                    Console.WriteLine("PN26000147 NOT FOUND!");
                }

                // 2. Check PC26000072 (from screenshot 2)
                var pc72 = conn.QueryFirstOrDefault(@"
                    SELECT pc.ID, pc.SoPhieuChi, pc.NgayChi, pc.SoTienChi, pc.IDNhaCungCap, pc.TrangThai, pc.IsDeleted
                    FROM KT_PhieuChi pc
                    WHERE pc.SoPhieuChi LIKE '%72%'");

                if (pc72 != null)
                {
                    Console.WriteLine($"\nPC26000072: ID={pc72.ID}, So={pc72.SoPhieuChi}, Ngay={pc72.NgayChi}, SoTienChi={pc72.SoTienChi:N0}, NCC_ID={pc72.IDNhaCungCap}, TrangThai={pc72.TrangThai}");

                    var details72 = conn.Query(@"
                        SELECT ct.ID, ct.IDPhieuNhap, pn.SoChungTu, ct.LoaiChi, ct.SoTienPhanBo, ct.DienGiai
                        FROM KT_PhieuChiChiTiet ct
                        LEFT JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID
                        WHERE ct.IDPhieuChi = @ID", new { ID = (int)pc72.ID });

                    Console.WriteLine("Details of PC26000072:");
                    foreach (var d in details72)
                    {
                        Console.WriteLine($"   ctID={d.ID}, PN={d.SoChungTu} (ID={d.IDPhieuNhap}), LoaiChi={d.LoaiChi}, SoTienPhanBo={d.SoTienPhanBo:N0}");
                    }
                }

                // 3. Check all PhieuNhap for Vinaken (ID=1) sorted by NgayNhap, ID
                int nccId = 1;
                if (pn147 != null) nccId = (int)pn147.IDNhaCungCap;

                var allPns = conn.Query(@"
                    SELECT pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TongCong, pn.TrangThai, pn.TrangThaiThanhToan
                    FROM KHO_PhieuNhap pn
                    WHERE pn.IDNhaCungCap = @NCCID AND pn.IsDeleted = 0
                    ORDER BY pn.ID DESC", new { NCCID = nccId });

                Console.WriteLine($"\nLAST 15 PHIEU NHAP FOR VINAKEN (NCC_ID={nccId}):");
                foreach (var pn in allPns.Take(15))
                {
                    decimal daTra = conn.QueryFirstOrDefault<decimal>(@"
                        SELECT ISNULL(SUM(ct.SoTienPhanBo), 0)
                        FROM KT_PhieuChiChiTiet ct
                        INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                        WHERE ct.IDPhieuNhap = @ID AND ct.LoaiChi = 1 AND pc.IsDeleted = 0 AND pc.TrangThai = 2", new { ID = (int)pn.ID });

                    decimal conNo = (decimal)pn.TongCong - daTra;
                    Console.WriteLine($"   PN={pn.SoChungTu} (ID={pn.ID}), Ngay={pn.NgayNhap:yyyy-MM-dd}, TongCong={pn.TongCong:N0}, DaTra={daTra:N0}, ConNo={conNo:N0}, TrangThaiTT_DB={pn.TrangThaiThanhToan}");
                }

                // 4. Check all PhieuChi for Vinaken sorted by ID DESC
                var allPcs = conn.Query(@"
                    SELECT pc.ID, pc.SoPhieuChi, pc.NgayChi, pc.SoTienChi, pc.TrangThai, pc.IsDeleted
                    FROM KT_PhieuChi pc
                    WHERE pc.IDNhaCungCap = @NCCID AND pc.IsDeleted = 0 AND pc.TrangThai = 2
                    ORDER BY pc.ID DESC", new { NCCID = nccId });

                Console.WriteLine($"\nLAST 15 PHIEU CHI (TRANG THAI=2) FOR VINAKEN (NCC_ID={nccId}):");
                foreach (var pc in allPcs.Take(15))
                {
                    decimal tongPhanBo = conn.QueryFirstOrDefault<decimal>(@"
                        SELECT ISNULL(SUM(SoTienPhanBo), 0)
                        FROM KT_PhieuChiChiTiet
                        WHERE IDPhieuChi = @ID", new { ID = (int)pc.ID });

                    Console.WriteLine($"   PC={pc.SoPhieuChi} (ID={pc.ID}), Ngay={pc.NgayChi:yyyy-MM-dd}, SoTienChi={pc.SoTienChi:N0}, TongPhanBo={tongPhanBo:N0}");
                }

                // 5. Total Sums for this NCC
                decimal sumAllNhap = conn.QueryFirstOrDefault<decimal>(@"
                    SELECT ISNULL(SUM(TongCong), 0)
                    FROM KHO_PhieuNhap
                    WHERE IDNhaCungCap = @NCCID AND IsDeleted = 0 AND TrangThai IN (1, 2)", new { NCCID = nccId });

                decimal sumAllChi = conn.QueryFirstOrDefault<decimal>(@"
                    SELECT ISNULL(SUM(SoTienChi), 0)
                    FROM KT_PhieuChi
                    WHERE IDNhaCungCap = @NCCID AND IsDeleted = 0 AND TrangThai = 2", new { NCCID = nccId });

                Console.WriteLine($"\nTOTAL SUMMARY FOR NCC_ID={nccId}:");
                Console.WriteLine($"   SUM ALL PHIEU NHAP (TrangThai IN (1,2)): {sumAllNhap:N0}");
                Console.WriteLine($"   SUM ALL PHIEU CHI  (TrangThai = 2):      {sumAllChi:N0}");
                Console.WriteLine($"   TOTAL DEBT BALANCE (Nhap - Chi):         {sumAllNhap - sumAllChi:N0}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message));
        }
    }
}
