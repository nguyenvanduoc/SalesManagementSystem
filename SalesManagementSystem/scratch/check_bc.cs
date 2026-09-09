using System;
using System.Collections.Generic;
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
            Assembly asm = Assembly.LoadFrom(@"SalesManagementSystem.dll");
            Type factoryType = asm.GetType("SalesManagementSystem.Data.DbConnectionFactory");
            object factory = Activator.CreateInstance(factoryType);
            MethodInfo createConnMethod = factoryType.GetMethod("CreateConnection");

            using (IDbConnection conn = (IDbConnection)createConnMethod.Invoke(factory, null))
            {
                conn.Open();

                Console.WriteLine("==========================================================");
                Console.WriteLine("TEST BC CALCULATION USING DASHBOARD GIA VON FORMULA");
                Console.WriteLine("==========================================================");

                string testSql = @"
                SELECT 
                    sp.MaSanPham,
                    sp.TenSanPham,
                    sp.DVT AS DonViTinh,
                    ISNULL(dt.SoLuongDoanhThu, 0) AS SoLuongDoanhThu,
                    ISNULL(dt.ThanhTienDoanhThu, 0) AS ThanhTienDoanhThu,
                    ISNULL(dt.SoLuongGiaVon, 0) AS SoLuongGiaVon,
                    ISNULL(dt.ThanhTienGiaVon, 0) AS ThanhTienGiaVon,
                    ISNULL(dt.ChiPhiVanChuyen, 0) AS ChiPhiVanChuyen,
                    (ISNULL(dt.ThanhTienDoanhThu, 0) - ISNULL(dt.ThanhTienGiaVon, 0)) AS LoiNhuanGop,
                    (ISNULL(dt.ThanhTienDoanhThu, 0) - ISNULL(dt.ThanhTienGiaVon, 0) - ISNULL(dt.ChiPhiVanChuyen, 0)) AS LoiNhuanThuan
                FROM DM_SanPham sp
                LEFT JOIN (
                    SELECT 
                        ct.IDSanPham,
                        SUM(ct.SoLuong) AS SoLuongDoanhThu,
                        SUM(ct.ThanhTien) AS ThanhTienDoanhThu,
                        SUM(ct.SoLuong) AS SoLuongGiaVon,
                        SUM(
                            CASE 
                                WHEN ISNULL(ct.ThanhTienVon, 0) > 0 AND (ct.DonGia = 0 OR ct.DonGiaVon <= ct.DonGia * 2.5 OR ap.AvgDonGia IS NULL) THEN ct.ThanhTienVon
                                WHEN ISNULL(ct.DonGiaVon, 0) > 0 AND (ct.DonGia = 0 OR ct.DonGiaVon <= ct.DonGia * 2.5 OR ap.AvgDonGia IS NULL) THEN ct.SoLuong * ct.DonGiaVon
                                ELSE ct.SoLuong * ISNULL(ap.AvgDonGia, 0)
                            END
                        ) AS ThanhTienGiaVon,
                        SUM(ct.SoLuong * ISNULL(ap.AvgDonGiaVanChuyen, 0)) AS ChiPhiVanChuyen
                    FROM BAN_ChungTuBanHang_ChiTiet ct
                    INNER JOIN BAN_ChungTuBanHang hd ON ct.IDChungTuBanHang = hd.ID
                    OUTER APPLY (
                        SELECT 
                            SUM(pn_ct.SoLuong * pn_ct.DonGia) / NULLIF(SUM(pn_ct.SoLuong), 0) AS AvgDonGia,
                            SUM(pn_ct.SoLuong * ISNULL(pn_ct.DonGiaVanChuyen, 0)) / NULLIF(SUM(pn_ct.SoLuong), 0) AS AvgDonGiaVanChuyen
                        FROM KHO_PhieuNhap_ChiTiet pn_ct
                        INNER JOIN KHO_PhieuNhap pn ON pn_ct.IDPhieuNhap = pn.ID
                        INNER JOIN DM_KhoHang kh_pn ON pn.IDKho = kh_pn.ID AND kh_pn.IsKhoChinh = 1
                        WHERE pn_ct.IDSanPham = ct.IDSanPham 
                          AND pn.TrangThai = 2 AND pn.IsDeleted = 0
                          AND pn.NgayNhap <= '2026-09-03 23:59:59'
                    ) ap
                    WHERE hd.IsDeleted = 0 
                      AND hd.TrangThai = 2
                      AND hd.NgayChungTu >= '2026-01-01' AND hd.NgayChungTu <= '2026-09-03 23:59:59'
                    GROUP BY ct.IDSanPham
                ) dt ON sp.ID = dt.IDSanPham
                WHERE dt.IDSanPham IS NOT NULL
                  AND (sp.MaSanPham IS NULL OR (sp.MaSanPham NOT IN ('NODAUKY', 'NoDauKy')))
                ORDER BY sp.TenSanPham";

                var data = conn.Query<dynamic>(testSql).ToList();

                decimal totDT = 0, totGV = 0, totLNGop = 0, totVC = 0, totLNThuan = 0;

                foreach (var row in data)
                {
                    decimal dt = (decimal)row.ThanhTienDoanhThu;
                    decimal gv = (decimal)row.ThanhTienGiaVon;
                    decimal gop = (decimal)row.LoiNhuanGop;
                    decimal vc = (decimal)row.ChiPhiVanChuyen;
                    decimal thuan = (decimal)row.LoiNhuanThuan;

                    totDT += dt;
                    totGV += gv;
                    totLNGop += gop;
                    totVC += vc;
                    totLNThuan += thuan;

                    Console.WriteLine(string.Format("{0} ({1}): DT={2:N0} | GV={3:N0} | LNGop={4:N0} | ChiPhiVC={5:N0} | LNThuan={6:N0}",
                        row.MaSanPham, row.TenSanPham, dt, gv, gop, vc, thuan));
                }

                Console.WriteLine("\n==========================================================");
                Console.WriteLine("SUMMARY CARDS (MATCHING DASHBOARD LOGIC)");
                Console.WriteLine("==========================================================");
                Console.WriteLine(string.Format("DOANH THU BÁN HÀNG:   {0:N0}", totDT));
                Console.WriteLine(string.Format("GIÁ VỐN HÀNG BÁN:    {0:N0}", totGV));
                Console.WriteLine(string.Format("LỢI NHUẬN GỘP:       {0:N0}", totLNGop));
                Console.WriteLine(string.Format("CHI PHÍ VẬN CHUYỂN:  {0:N0}", totVC));
                Console.WriteLine(string.Format("LỢI NHUẬN THUẦN:     {0:N0}", totLNThuan));
                Console.WriteLine(string.Format("TỶ SUẤT LN (%):      {0:N2}%", totDT > 0 ? (totLNThuan / totDT) * 100 : 0));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex);
        }
    }
}
