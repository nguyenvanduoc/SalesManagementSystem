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
    public class CongNoNCCRepository : ICongNoNCCRepository
    {
        private readonly DbConnectionFactory _db;

        public CongNoNCCRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<CongNoNCCViewModel> GetList(
            string tuNgay,
            string denNgay,
            int? idNhaCungCap,
            int? trangThaiCongNo)
        {
            using (var conn = _db.CreateConnection())
            {
                try {
                    string sqlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "update_sp_CongNo_PhaseTra_NCC_GetList.sql");
                    if (System.IO.File.Exists(sqlPath)) {
                        string sql = System.IO.File.ReadAllText(sqlPath);
                        var parts = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach(var part in parts) {
                            if (!string.IsNullOrWhiteSpace(part)) {
                                conn.Execute(part);
                            }
                        }
                        System.IO.File.Delete(sqlPath);
                    }
                } catch { }

                var p = new DynamicParameters();
                p.Add("@TuNgay",        string.IsNullOrEmpty(tuNgay)  ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay",       string.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay));
                p.Add("@IDNhaCungCap",  idNhaCungCap);
                p.Add("@TrangThaiCongNo", trangThaiCongNo);

                return conn.Query<CongNoNCCViewModel>(
                    "sp_CongNo_PhaseTra_NCC_GetList",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public decimal GetTongDauKy(string tuNgay, int? idNhaCungCap)
        {
            if (string.IsNullOrWhiteSpace(tuNgay)) return 0M;
            if (!DateTime.TryParse(tuNgay, out DateTime dtTu)) return 0M;

            using (var conn = _db.CreateConnection())
            {
                string sql = @"
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
                      AND (@IDNhaCungCap IS NULL OR pn.IDNhaCungCap = @IDNhaCungCap)";

                return conn.QueryFirstOrDefault<decimal>(sql, new { TuNgay = dtTu, IDNhaCungCap = idNhaCungCap });
            }
        }
    }
}
