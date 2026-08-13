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
                Console.WriteLine($"DB Connection: {conn.ConnectionString}");

                // Find supplier Vinaken
                var nccs = conn.Query(@"
                    SELECT ID, MaNhaCungCap, TenNhaCungCap 
                    FROM DM_NhaCungCap 
                    WHERE TenNhaCungCap LIKE N'%Vinaken%' OR MaNhaCungCap LIKE N'%Vinaken%'").ToList();

                foreach (var ncc in nccs)
                {
                    int nccId = (int)ncc.ID;
                    Console.WriteLine($"\n==================================================");
                    Console.WriteLine($"NCC ID: {ncc.ID} | Ma: {ncc.MaNhaCungCap} | Ten: {ncc.TenNhaCungCap}");
                    Console.WriteLine($"==================================================");

                    // 1. Total Imports (KHO_PhieuNhap)
                    var importStats = conn.QueryFirstOrDefault(@"
                        SELECT 
                            COUNT(1) AS SoPhieuNhap,
                            ISNULL(SUM(TongCong), 0) AS TongTienNhap
                        FROM KHO_PhieuNhap
                        WHERE IDNhaCungCap = @NCCID 
                          AND IsDeleted = 0 
                          AND TrangThai IN (1, 2)", new { NCCID = nccId });

                    // 2. Total Payments (KT_PhieuChi)
                    var paymentStats = conn.QueryFirstOrDefault(@"
                        SELECT 
                            COUNT(1) AS SoPhieuChi,
                            ISNULL(SUM(SoTienChi), 0) AS TongTienChi
                        FROM KT_PhieuChi
                        WHERE IDNhaCungCap = @NCCID 
                          AND IsDeleted = 0 
                          AND TrangThai = 2", new { NCCID = nccId });

                    decimal tongNhap = (decimal)importStats.TongTienNhap;
                    decimal tongChi = (decimal)paymentStats.TongTienChi;
                    decimal diff = tongNhap - tongChi;

                    Console.WriteLine($"Tong So Phieu Nhap (TrangThai 1,2): {importStats.SoPhieuNhap}");
                    Console.WriteLine($"Tong Tien Hang Nhap:               {tongNhap:N0} đ");
                    Console.WriteLine($"Tong So Phieu Chi (TrangThai 2):   {paymentStats.SoPhieuChi}");
                    Console.WriteLine($"Tong Tien Da Chi:                  {tongChi:N0} đ");
                    Console.WriteLine($"--------------------------------------------------");
                    Console.WriteLine($"CHENH LECH (Tong Nhap - Tong Chi): {diff:N0} đ");

                    if (diff == 0)
                    {
                        Console.WriteLine("-> KET LUAN: Tong Nhap phai tra va Tong Chi da bang nhau 100% (Khong chi thieu tong the).");
                    }
                    else if (diff > 0)
                    {
                        Console.WriteLine($"-> KET LUAN: Tong Nhap LON HON Tong Chi {diff:N0} đ (Chi thieu tong the {diff:N0} đ).");
                    }
                    else
                    {
                        Console.WriteLine($"-> KET LUAN: Tong Chi LON HON Tong Nhap {Math.Abs(diff):N0} đ (Tra du / Tra truo'c {Math.Abs(diff):N0} đ).");
                    }

                    // 3. Check specific bills if exists
                    Console.WriteLine($"\n--- CHECKS CHO CAC PHIEU NHAP GANCHUYEN ---");
                    var searchBills = conn.Query(@"
                        SELECT ID, SoChungTu, NgayNhap, TongCong, TrangThai, TrangThaiThanhToan
                        FROM KHO_PhieuNhap
                        WHERE IDNhaCungCap = @NCCID AND IsDeleted = 0
                        ORDER BY ID DESC", new { NCCID = nccId }).Take(20);

                    foreach (var pn in searchBills)
                    {
                        decimal daTra = conn.QueryFirstOrDefault<decimal>(@"
                            SELECT ISNULL(SUM(ct.SoTienPhanBo), 0)
                            FROM KT_PhieuChiChiTiet ct
                            INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                            WHERE ct.IDPhieuNhap = @ID AND ct.LoaiChi = 1 AND pc.IsDeleted = 0 AND pc.TrangThai = 2", new { ID = (int)pn.ID });

                        decimal conNo = (decimal)pn.TongCong - daTra;
                        Console.WriteLine($"PN: {pn.SoChungTu} (ID={pn.ID}) | Ngay: {pn.NgayNhap:yyyy-MM-dd} | PhaiTra: {pn.TongCong:N0} | DaTra: {daTra:N0} | ConNo: {conNo:N0} | TrangThaiTT: {pn.TrangThaiThanhToan}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message));
        }
    }
}
