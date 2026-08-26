using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace CheckDiff
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

                    DateTime denNgay = new DateTime(2026, 8, 31);
                    DateTime tuNgay = new DateTime(2026, 8, 1);

                    Console.WriteLine("=================================================");
                    Console.WriteLine("ANALYSIS OF CÔNG NỢ NCC");
                    Console.WriteLine("=================================================");

                    // 1. All PhieuNhap with TrangThai=2 vs TrangThai IN (1,2)
                    var allPn = conn.Query<dynamic>(@"
                        SELECT 
                            pn.ID, pn.SoChungTu, pn.NgayNhap, pn.TrangThai, pn.TongCong,
                            -- DaThanhToan all time
                            (
                                ISNULL((SELECT SUM(ct.SoTienPhanBo) FROM KT_PhieuChiChiTiet ct INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID WHERE ct.IDPhieuNhap = pn.ID AND ct.LoaiChi = 1 AND pc.TrangThai = 2 AND pc.IsDeleted = 0), 0) +
                                ISNULL((SELECT SUM(pc2.SoTienChi) FROM KT_PhieuChi pc2 WHERE pc2.IDPhieuNhap = pn.ID AND pc2.TrangThai = 2 AND pc2.IsDeleted = 0 AND NOT EXISTS (SELECT 1 FROM KT_PhieuChiChiTiet ct WHERE ct.IDPhieuChi = pc2.ID)), 0)
                            ) AS DaThanhToan_AllTime,
                            -- DaThanhToan up to DenNgay (31/08/2026)
                            (
                                ISNULL((SELECT SUM(ct.SoTienPhanBo) FROM KT_PhieuChiChiTiet ct INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID WHERE ct.IDPhieuNhap = pn.ID AND ct.LoaiChi = 1 AND pc.TrangThai = 2 AND pc.IsDeleted = 0 AND pc.NgayChi <= @DenNgay), 0) +
                                ISNULL((SELECT SUM(pc2.SoTienChi) FROM KT_PhieuChi pc2 WHERE pc2.IDPhieuNhap = pn.ID AND pc2.TrangThai = 2 AND pc2.IsDeleted = 0 AND pc2.NgayChi <= @DenNgay AND NOT EXISTS (SELECT 1 FROM KT_PhieuChiChiTiet ct WHERE ct.IDPhieuChi = pc2.ID)), 0)
                            ) AS DaThanhToan_UpToDenNgay
                        FROM KHO_PhieuNhap pn
                        WHERE pn.IsDeleted = 0
                    ", new { DenNgay = denNgay }).ToList();

                    Console.WriteLine($"\n--- A. TOÀN BỘ THỜI GIAN (All Time) ---");
                    var all_TrangThai12 = allPn;
                    var all_TrangThai2Only = allPn.Where(x => x.TrangThai == 2).ToList();

                    decimal sumTong_All_12 = all_TrangThai12.Sum(x => (decimal)x.TongCong);
                    decimal sumPaid_All_12 = all_TrangThai12.Sum(x => (decimal)x.DaThanhToan_AllTime);
                    decimal sumCon_All_12 = sumTong_All_12 - sumPaid_All_12;

                    decimal sumTong_All_2 = all_TrangThai2Only.Sum(x => (decimal)x.TongCong);
                    decimal sumPaid_All_2 = all_TrangThai2Only.Sum(x => (decimal)x.DaThanhToan_AllTime);
                    decimal sumCon_All_2 = sumTong_All_2 - sumPaid_All_2;

                    Console.WriteLine($"Gồm TrangThai 1 & 2 (184 phiếu) -> Tong: {sumTong_All_12:N0} | DaTra: {sumPaid_All_12:N0} | ConNo: {sumCon_All_12:N0}");
                    Console.WriteLine($"Chỉ TrangThai 2 (177 phiếu)      -> Tong: {sumTong_All_2:N0} | DaTra: {sumPaid_All_2:N0} | ConNo: {sumCon_All_2:N0}");

                    Console.WriteLine($"\n--- B. LŨY KẾ ĐẾN 31/08/2026 (NgayNhap <= 31/08/2026) ---");
                    var upToAug_12 = allPn.Where(x => x.NgayNhap <= denNgay).ToList();
                    var upToAug_2 = allPn.Where(x => x.TrangThai == 2 && x.NgayNhap <= denNgay).ToList();

                    decimal sumTong_Aug_12 = upToAug_12.Sum(x => (decimal)x.TongCong);
                    decimal sumPaid_Aug_12 = upToAug_12.Sum(x => (decimal)x.DaThanhToan_UpToDenNgay);
                    decimal sumCon_Aug_12 = sumTong_Aug_12 - sumPaid_Aug_12;

                    decimal sumTong_Aug_2 = upToAug_2.Sum(x => (decimal)x.TongCong);
                    decimal sumPaid_Aug_2 = upToAug_2.Sum(x => (decimal)x.DaThanhToan_UpToDenNgay);
                    decimal sumCon_Aug_2 = sumTong_Aug_2 - sumPaid_Aug_2;

                    Console.WriteLine($"Gồm TrangThai 1 & 2 -> Tong: {sumTong_Aug_12:N0} | DaTra: {sumPaid_Aug_12:N0} | ConNo: {sumCon_Aug_12:N0}");
                    Console.WriteLine($"Chỉ TrangThai 2      -> Tong: {sumTong_Aug_2:N0} | DaTra: {sumPaid_Aug_2:N0} | ConNo: {sumCon_Aug_2:N0}");

                    Console.WriteLine($"\n--- C. PHÁT SINH TRONG THÁNG 8 (01/08/2026 <= NgayNhap <= 31/08/2026) ---");
                    var inAug_12 = allPn.Where(x => x.NgayNhap >= tuNgay && x.NgayNhap <= denNgay).ToList();
                    var inAug_2 = allPn.Where(x => x.TrangThai == 2 && x.NgayNhap >= tuNgay && x.NgayNhap <= denNgay).ToList();

                    decimal sumTong_InAug_12 = inAug_12.Sum(x => (decimal)x.TongCong);
                    decimal sumPaid_InAug_12 = inAug_12.Sum(x => (decimal)x.DaThanhToan_AllTime);
                    decimal sumCon_InAug_12 = sumTong_InAug_12 - sumPaid_InAug_12;

                    decimal sumTong_InAug_2 = inAug_2.Sum(x => (decimal)x.TongCong);
                    decimal sumPaid_InAug_2 = inAug_2.Sum(x => (decimal)x.DaThanhToan_AllTime);
                    decimal sumCon_InAug_2 = sumTong_InAug_2 - sumPaid_InAug_2;

                    Console.WriteLine($"Gồm TrangThai 1 & 2 -> Tong: {sumTong_InAug_12:N0} | DaTra: {sumPaid_InAug_12:N0} | ConNo: {sumCon_InAug_12:N0}");
                    Console.WriteLine($"Chỉ TrangThai 2      -> Tong: {sumTong_InAug_2:N0} | DaTra: {sumPaid_InAug_2:N0} | ConNo: {sumCon_InAug_2:N0}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX: " + ex);
            }
        }
    }
}
