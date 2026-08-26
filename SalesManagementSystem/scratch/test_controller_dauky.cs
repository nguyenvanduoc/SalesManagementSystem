using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace TestControllerDauKy
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

                    DateTime? tuNgay = new DateTime(2026, 8, 1);
                    DateTime? denNgay = new DateTime(2026, 8, 24);
                    int? idNhaCungCap = null;

                    Console.WriteLine("=================================================");
                    Console.WriteLine($"TEST SEARCH (TuNgay: {tuNgay:dd/MM/yyyy}, DenNgay: {denNgay:dd/MM/yyyy})");
                    Console.WriteLine("=================================================");

                    // Calculate TongDauKy for filter
                    decimal tongDauKy = 0;
                    if (tuNgay.HasValue)
                    {
                        tongDauKy = conn.QueryFirstOrDefault<decimal>(@"
                            SELECT ISNULL(SUM(pn.TongCong - pd.DaThanhToan), 0)
                            FROM KHO_PhieuNhap pn
                            INNER JOIN (
                                SELECT 
                                    pn.ID,
                                    ISNULL(
                                        (SELECT SUM(ct.SoTienPhanBo)
                                         FROM KT_PhieuChiChiTiet ct
                                         INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                                         WHERE ct.IDPhieuNhap = pn.ID 
                                           AND ct.LoaiChi = 1
                                           AND pc.TrangThai = 2
                                           AND pc.IsDeleted = 0),
                                        0
                                    ) + ISNULL(
                                        (SELECT SUM(pc2.SoTienChi)
                                         FROM KT_PhieuChi pc2
                                         WHERE pc2.IDPhieuNhap = pn.ID
                                           AND pc2.TrangThai = 2
                                           AND pc2.IsDeleted = 0
                                           AND NOT EXISTS (SELECT 1 FROM KT_PhieuChiChiTiet ct WHERE ct.IDPhieuChi = pc2.ID)
                                        ),
                                        0
                                    ) AS DaThanhToan
                                FROM KHO_PhieuNhap pn
                                WHERE pn.IsDeleted = 0
                            ) pd ON pn.ID = pd.ID
                            WHERE pn.IsDeleted = 0 
                              AND pn.NgayNhap < @TuNgay
                              AND (@IDNhaCungCap IS NULL OR pn.IDNhaCungCap = @IDNhaCungCap)
                        ", new { TuNgay = tuNgay.Value, IDNhaCungCap = idNhaCungCap });
                    }

                    // Get list in period
                    var list = conn.Query<dynamic>("sp_CongNo_PhaseTra_NCC_GetList", new {
                        TuNgay = tuNgay,
                        DenNgay = denNgay,
                        IDNhaCungCap = idNhaCungCap,
                        TrangThaiCongNo = (int?)null
                    }, commandType: CommandType.StoredProcedure).ToList();

                    decimal tongPhaiTra = list.Sum(x => (decimal)(x.TongTienHang ?? 0m));
                    decimal tongDaTra   = list.Sum(x => (decimal)(x.DaThanhToan ?? 0m));
                    decimal tongConLai  = tongDauKy + tongPhaiTra - tongDaTra;

                    Console.WriteLine($"Nợ đầu kỳ (tính đến {tuNgay:dd/MM/yyyy})  : {tongDauKy:N0} VND");
                    Console.WriteLine($"Mua trong kỳ ({tuNgay:dd/MM} - {denNgay:dd/MM})  : {tongPhaiTra:N0} VND");
                    Console.WriteLine($"Đã thanh toán trong kỳ              : {tongDaTra:N0} VND");
                    Console.WriteLine($"Nợ cuối kỳ (Tổng nợ thực tế)        : {tongConLai:N0} VND");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX: " + ex);
            }
        }
    }
}
