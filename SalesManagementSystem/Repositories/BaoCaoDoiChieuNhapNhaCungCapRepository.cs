using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class BaoCaoDoiChieuNhapNhaCungCapRepository : IBaoCaoDoiChieuNhapNhaCungCapRepository
    {
        private readonly DbConnectionFactory _db;

        public BaoCaoDoiChieuNhapNhaCungCapRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<BaoCaoDoiChieuNhapNhaCungCapViewModel> GetList(int? idNhaCungCap, DateTime tuNgay, DateTime denNgay, string soChungTu = null)
        {
            using (var conn = _db.CreateConnection())
            {
                // Tự động tạo SP nếu tồn tại file script (mô phỏng migration)
                try {
                    string sqlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "sp_BaoCao_DoiChieuNhapNhaCungCap.sql");
                    if (System.IO.File.Exists(sqlPath)) {
                        string sql = System.IO.File.ReadAllText(sqlPath);
                        var parts = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach(var part in parts) {
                            if (!string.IsNullOrWhiteSpace(part)) {
                                conn.Execute(part);
                            }
                        }
                        // Sau khi chạy xong đổi tên file hoặc xoá để tránh chạy lại lần sau
                        if (System.IO.File.Exists(sqlPath + ".executed")) {
                            System.IO.File.Delete(sqlPath + ".executed");
                        }
                        System.IO.File.Move(sqlPath, sqlPath + ".executed");
                    }
                    
                    // Script insert menu (ACL)
                    string aclPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "insert_acl_BaoCaoDoiChieuNhapNhaCungCap.sql");
                    if (System.IO.File.Exists(aclPath)) {
                        string sql = System.IO.File.ReadAllText(aclPath);
                        var parts = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach(var part in parts) {
                            if (!string.IsNullOrWhiteSpace(part)) {
                                conn.Execute(part);
                            }
                        }
                        System.IO.File.Move(aclPath, aclPath + ".executed");
                    }
                } catch { }

                var p = new DynamicParameters();
                p.Add("@IDNhaCungCap", idNhaCungCap);
                p.Add("@TuNgay", tuNgay);
                p.Add("@DenNgay", denNgay);
                p.Add("@SoChungTu", soChungTu);

                return conn.Query<BaoCaoDoiChieuNhapNhaCungCapViewModel>(
                    "sp_BaoCao_DoiChieuNhapNhaCungCap",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetNhaCungCapDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "SELECT ID, ISNULL(MaNhaCungCap, '') + ' - ' + ISNULL(TenNhaCungCap, '') AS TenHienThi FROM DM_NhaCungCap ORDER BY TenNhaCungCap"
                ).ToList();
            }
        }

        public IEnumerable<BaoCaoTongHopCongNoPhaiTraViewModel> GetTongHopCongNoPhaiTra(int? idNhaCungCap, DateTime tuNgay, DateTime denNgay)
        {
            // Cùng quy tắc với sp_BaoCao_DoiChieuNhapNhaCungCap để số liệu 2 báo cáo khớp nhau:
            //  - Phải trả (Nợ): SUM(KHO_PhieuNhap_ChiTiet.TongSauThue), phiếu nhập TrangThai IN (1,2), chưa xóa
            //  - Thanh toán (Có): SUM(KT_PhieuChi.SoTienChi), phiếu chi TrangThai = 2, chưa xóa
            const string sql = @"
                ;WITH Nhap AS (
                    SELECT pn.IDNhaCungCap,
                           SUM(CASE WHEN CAST(pn.NgayNhap AS DATE) < @TuNgay THEN ISNULL(ct.TongSauThue, 0) ELSE 0 END) AS TruocKy,
                           SUM(CASE WHEN CAST(pn.NgayNhap AS DATE) >= @TuNgay THEN ISNULL(ct.TongSauThue, 0) ELSE 0 END) AS TrongKy
                    FROM KHO_PhieuNhap pn
                    INNER JOIN KHO_PhieuNhap_ChiTiet ct ON pn.ID = ct.IDPhieuNhap
                    WHERE pn.IsDeleted = 0
                      AND pn.TrangThai IN (1, 2)
                      AND pn.IDNhaCungCap IS NOT NULL
                      AND (@IDNhaCungCap IS NULL OR pn.IDNhaCungCap = @IDNhaCungCap)
                      AND CAST(pn.NgayNhap AS DATE) <= @DenNgay
                    GROUP BY pn.IDNhaCungCap
                ),
                Chi AS (
                    SELECT pc.IDNhaCungCap,
                           SUM(CASE WHEN CAST(pc.NgayChi AS DATE) < @TuNgay THEN pc.SoTienChi ELSE 0 END) AS TruocKy,
                           SUM(CASE WHEN CAST(pc.NgayChi AS DATE) >= @TuNgay THEN pc.SoTienChi ELSE 0 END) AS TrongKy
                    FROM KT_PhieuChi pc
                    WHERE pc.IsDeleted = 0
                      AND pc.TrangThai = 2
                      AND pc.IDNhaCungCap IS NOT NULL
                      AND (@IDNhaCungCap IS NULL OR pc.IDNhaCungCap = @IDNhaCungCap)
                      AND CAST(pc.NgayChi AS DATE) <= @DenNgay
                    GROUP BY pc.IDNhaCungCap
                ),
                TongHop AS (
                    SELECT ncc.ID AS IDNhaCungCap,
                           ncc.TenNhaCungCap,
                           ISNULL(n.TruocKy, 0) - ISNULL(c.TruocKy, 0) AS SoDuDauKy,
                           ISNULL(n.TrongKy, 0) AS PhatSinhNo,
                           ISNULL(c.TrongKy, 0) AS PhatSinhCo
                    FROM DM_NhaCungCap ncc
                    LEFT JOIN Nhap n ON n.IDNhaCungCap = ncc.ID
                    LEFT JOIN Chi c ON c.IDNhaCungCap = ncc.ID
                    WHERE n.IDNhaCungCap IS NOT NULL OR c.IDNhaCungCap IS NOT NULL
                )
                SELECT IDNhaCungCap, TenNhaCungCap, SoDuDauKy, PhatSinhNo, PhatSinhCo
                FROM TongHop
                WHERE SoDuDauKy <> 0 OR PhatSinhNo <> 0 OR PhatSinhCo <> 0
                ORDER BY TenNhaCungCap";

            using (var conn = _db.CreateConnection())
            {
                return conn.Query<BaoCaoTongHopCongNoPhaiTraViewModel>(sql, new
                {
                    IDNhaCungCap = idNhaCungCap,
                    TuNgay = tuNgay.Date,
                    DenNgay = denNgay.Date
                }).ToList();
            }
        }
    }
}
