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
    public class PhieuChiRepository : IPhieuChiRepository
    {
        private readonly DbConnectionFactory _db;

        public PhieuChiRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<PhieuChiListViewModel> GetList(
            string tuNgay,
            string denNgay,
            string soPhieuChi,
            int? idNhaCungCap,
            int? idKhoanMucChi,
            int? trangThai,
            string nguoiNhanTien = null,
            int? idTaiKhoanThanhToan = null)
        {
            using (var conn = _db.CreateConnection())
            {
                try {
                    string sqlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "update_sp_KT_PhieuChi_GetList.sql");
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
                p.Add("@TuNgay",        string.IsNullOrEmpty(tuNgay)      ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay",       string.IsNullOrEmpty(denNgay)     ? (DateTime?)null : DateTime.Parse(denNgay));
                p.Add("@SoPhieuChi",    string.IsNullOrEmpty(soPhieuChi)  ? null : soPhieuChi);
                p.Add("@IDNhaCungCap",  idNhaCungCap);
                p.Add("@IDKhoanMucChi", idKhoanMucChi);
                p.Add("@TrangThai",     trangThai);
                p.Add("@NguoiNhanTien",  string.IsNullOrEmpty(nguoiNhanTien) ? null : nguoiNhanTien);
                p.Add("@IDTaiKhoanThanhToan", idTaiKhoanThanhToan);

                return conn.Query<PhieuChiListViewModel>(
                    "sp_KT_PhieuChi_GetList",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public PhieuChiViewModel GetByID(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                var model = conn.QueryFirstOrDefault<PhieuChiViewModel>(
                    "sp_KT_PhieuChi_GetByID",
                    new { ID = id },
                    commandType: CommandType.StoredProcedure
                );
                if (model != null)
                {
                    model.ChiTiets = GetChiTiet(id).ToList();
                    if (model.IDNhaCungCap.HasValue)
                    {
                        model.TienTraTruocNCC = GetTienTraTruocNhaCungCap(model.IDNhaCungCap.Value);
                    }
                }
                return model;
            }
        }

        public int Save(PhieuChiViewModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID",                    model.ID == 0 ? (int?)null : model.ID);
                p.Add("@SoPhieuChi",            model.SoPhieuChi);
                p.Add("@NgayChi",               model.NgayChi);
                p.Add("@IDKhoanMucChi",         model.IDKhoanMucChi);
                p.Add("@IDTaiKhoanThanhToan",   model.IDTaiKhoanThanhToan);
                p.Add("@IDNguoiNhan",           model.IDNguoiNhan);
                p.Add("@NguoiNhanTien",         model.NguoiNhanTien);
                p.Add("@SoDienThoaiNguoiNhan",  model.SoDienThoaiNguoiNhan);
                p.Add("@IDNhaCungCap",          model.IDNhaCungCap);
                p.Add("@IDPhieuNhap",           model.IDPhieuNhap);
                p.Add("@SoTienChi",             model.SoTienChi);
                p.Add("@DienGiai",              model.DienGiai);
                p.Add("@NguoiTao",              userId);
                p.Add("@NewID",                 dbType: DbType.Int32, direction: ParameterDirection.Output);

                conn.Execute("sp_KT_PhieuChi_Save", p, commandType: CommandType.StoredProcedure);
                int newId = p.Get<int>("@NewID");
                
                // Save details
                if (model.ChiTiets != null && model.ChiTiets.Any())
                {
                    // For update, delete existing details (assuming sp_KT_PhieuChi_Huy would rollback, but for edit we might need to rollback old allocations first? 
                    // Note: User spec says "Khi hủy phiếu chi: Rollback toàn bộ...". But if they are just editing a PhieuChi? 
                    // If it is in draft (TrangThai = 1), DaThanhToan is not updated yet, it's only updated when GhiSo (TrangThai = 2). 
                    // Actually, sp_KT_PhieuChi_Save might not do the allocation. Wait, my sp_KT_PhieuChi_Save does NOT allocate KHO_PhieuNhap. 
                    // KHO_PhieuNhap is only allocated on GhiSo, OR maybe sp_KT_PhieuChi_Save does it?
                    // The old sp_KT_PhieuChi_Save does not update PhieuNhap directly, maybe sp_KT_PhieuChi_GhiSo does?
                    
                    conn.Execute("DELETE FROM KT_PhieuChiChiTiet WHERE IDPhieuChi = @ID", new { ID = newId });
                    
                    foreach (var c in model.ChiTiets)
                    {
                        conn.Execute(@"
                            INSERT INTO KT_PhieuChiChiTiet (IDPhieuChi, IDPhieuNhap, LoaiChi, SoTienPhanBo, DienGiai, NgayTao, NguoiTao)
                            VALUES (@IDPhieuChi, @IDPhieuNhap, @LoaiChi, @SoTienPhanBo, @DienGiai, GETDATE(), @NguoiTao)
                        ", new {
                            IDPhieuChi = newId,
                            IDPhieuNhap = c.IDPhieuNhap,
                            LoaiChi = c.LoaiChi,
                            SoTienPhanBo = c.SoTienPhanBo,
                            DienGiai = c.DienGiai,
                            NguoiTao = userId
                        });
                    }
                }
                
                return newId;
            }
        }

        public void GhiSo(int id, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(
                    "sp_KT_PhieuChi_GhiSo",
                    new { ID = id, NguoiGhi = userId },
                    commandType: CommandType.StoredProcedure
                );
                
                // Apply allocation to PhieuNhap here
                var chiTiets = conn.Query<PhieuChiChiTietViewModel>("SELECT * FROM KT_PhieuChiChiTiet WHERE IDPhieuChi = @ID", new { ID = id }).ToList();
                foreach(var ct in chiTiets)
                {
                    if (ct.LoaiChi == 1 && ct.IDPhieuNhap.HasValue)
                    {
                        conn.Execute(@"
                            UPDATE KHO_PhieuNhap
                            SET DaThanhToan = ISNULL(DaThanhToan, 0) + @SoTienPhanBo,
                                ConLai = ISNULL(ConLai, 0) - @SoTienPhanBo,
                                TrangThaiThanhToan = CASE WHEN ISNULL(ConLai, 0) - @SoTienPhanBo <= 0 THEN 2 ELSE 1 END
                            WHERE ID = @IDPhieuNhap
                        ", new { SoTienPhanBo = ct.SoTienPhanBo, IDPhieuNhap = ct.IDPhieuNhap });
                    }
                }
            }
        }

        public void Huy(int id, int userId, string lyDo)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(
                    "sp_KT_PhieuChi_Huy",
                    new { ID = id, NguoiHuy = userId, LyDoHuy = lyDo },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public void Delete(int id, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(
                    "sp_KT_PhieuChi_Delete",
                    new { ID = id, NguoiXoa = userId },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public string GenerateSoPhieuChi()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.ExecuteScalar<string>(
                    "sp_KT_PhieuChi_GenerateSo",
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public IEnumerable<dynamic> GetKhoanMucDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "SELECT ID, TenKhoanMuc AS TenHienThi FROM DM_KhoanMucChi WHERE IsHoatDong = 1 ORDER BY TenKhoanMuc"
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetTaiKhoanDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "SELECT ID, ISNULL(TenTaiKhoan, '') + CASE WHEN SoTaiKhoan IS NOT NULL THEN ' - ' + SoTaiKhoan ELSE '' END AS TenHienThi FROM DM_TaiKhoanThanhToan WHERE IsHoatDong = 1 ORDER BY TenTaiKhoan"
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetNhaCungCapDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "SELECT ID, TenNhaCungCap AS TenHienThi FROM DM_NhaCungCap ORDER BY TenNhaCungCap"
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetPhieuNhapDropdown(int? idNhaCungCap, int? currentPhieuNhapId = null)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = idNhaCungCap.HasValue
                    ? "SELECT ID, SoChungTu AS TenHienThi FROM KHO_PhieuNhap WHERE IsDeleted = 0 AND TrangThai = 2 AND (ISNULL(TrangThaiThanhToan, 0) < 2 OR ID = @CurrentID) AND IDNhaCungCap = @IDNhaCungCap ORDER BY NgayNhap DESC"
                    : "SELECT ID, SoChungTu AS TenHienThi FROM KHO_PhieuNhap WHERE IsDeleted = 0 AND TrangThai = 2 AND (ISNULL(TrangThaiThanhToan, 0) < 2 OR ID = @CurrentID) ORDER BY NgayNhap DESC";
                return conn.Query<dynamic>(sql, new { IDNhaCungCap = idNhaCungCap, CurrentID = currentPhieuNhapId }).ToList();
            }
        }

        public IEnumerable<dynamic> GetNhanSuDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "SELECT ID, HoDem + ' ' + Ten AS TenHienThi FROM NS_NhanSu  ORDER BY Ten"
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetPhieuNhapCongNo(int idNhaCungCap)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    @"SELECT ID, SoChungTu AS SoPhieuNhap, NgayNhap, TongCong AS TongTien, DaThanhToan, ConLai 
                      FROM KHO_PhieuNhap 
                      WHERE IsDeleted = 0 AND TrangThai = 2 AND ISNULL(ConLai, 0) > 0 AND IDNhaCungCap = @IDNhaCungCap 
                      ORDER BY NgayNhap ASC, ID ASC", 
                    new { IDNhaCungCap = idNhaCungCap }
                ).ToList();
            }
        }

        public decimal GetTienTraTruocNhaCungCap(int idNhaCungCap)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<decimal>(
                    @"SELECT ISNULL(SUM(CASE WHEN LoaiChi = 2 THEN SoTienPhanBo ELSE -SoTienPhanBo END), 0)
                      FROM KT_PhieuChiChiTiet ct
                      INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID OR (ct.IDPhieuChi IS NULL)
                      WHERE ((pc.IsDeleted = 0 AND pc.TrangThai = 2) OR ct.IDPhieuChi IS NULL)
                        AND (pc.IDNhaCungCap = @IDNhaCungCap OR (ct.IDPhieuChi IS NULL AND EXISTS(SELECT 1 FROM KHO_PhieuNhap pn WHERE pn.ID = ct.IDPhieuNhap AND pn.IDNhaCungCap = @IDNhaCungCap)))
                        AND ct.LoaiChi IN (2, 3)",
                    new { IDNhaCungCap = idNhaCungCap }
                );
            }
        }

        public IEnumerable<PhieuChiChiTietViewModel> GetChiTiet(int idPhieuChi)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<PhieuChiChiTietViewModel>(
                    @"SELECT ct.*, pn.SoChungTu AS SoPhieuNhap, pn.NgayNhap, pn.TongCong AS TongTien, pn.DaThanhToan, pn.ConLai 
                      FROM KT_PhieuChiChiTiet ct 
                      LEFT JOIN KHO_PhieuNhap pn ON ct.IDPhieuNhap = pn.ID 
                      WHERE ct.IDPhieuChi = @IDPhieuChi 
                      ORDER BY ct.ID ASC", 
                    new { IDPhieuChi = idPhieuChi }
                ).ToList();
            }
        }

        public dynamic GetPhieuNhapDetail(int idPhieuNhap)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<dynamic>(
                    "SELECT TongCong, DaThanhToan, ConLai FROM KHO_PhieuNhap WHERE ID = @ID",
                    new { ID = idPhieuNhap }
                );
            }
        }

        public IEnumerable<dynamic> GetLichSuChiTienPhieuNhap(int idPhieuNhap)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "SELECT SoPhieuChi, NgayChi, SoTienChi, TrangThai FROM KT_PhieuChi WHERE IDPhieuNhap = @IDPhieuNhap AND IsDeleted = 0 ORDER BY NgayChi DESC, ID DESC",
                    new { IDPhieuNhap = idPhieuNhap }
                ).ToList();
            }
        }

        public PhieuChiDashboardViewModel GetDashboardData(
            string tuNgay,
            string denNgay,
            string soPhieuChi,
            int? idNhaCungCap,
            int? idKhoanMucChi,
            int? trangThai,
            string nguoiNhanTien = null,
            int? idTaiKhoanThanhToan = null)
        {
            var cultureVi = new System.Globalization.CultureInfo("vi-VN");
            
            DateTime? startCurr = string.IsNullOrEmpty(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay);
            DateTime? endCurr = string.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay);
            DateTime? startPrev = null;
            DateTime? endPrev = null;
            string periodLabel = "tháng trước";

            if (startCurr.HasValue && endCurr.HasValue)
            {
                int days = (endCurr.Value - startCurr.Value).Days + 1;
                startPrev = startCurr.Value.AddDays(-days);
                endPrev = startCurr.Value.AddDays(-1);
                periodLabel = "kỳ trước";
            }
            else if (startCurr.HasValue)
            {
                startPrev = startCurr.Value.AddDays(-30);
                endPrev = startCurr.Value.AddDays(-1);
                periodLabel = "kỳ trước";
            }
            else if (endCurr.HasValue)
            {
                startPrev = endCurr.Value.AddDays(-30);
                endPrev = endCurr.Value.AddDays(-1);
                periodLabel = "kỳ trước";
            }
            else
            {
                var today = DateTime.Today;
                startCurr = new DateTime(today.Year, today.Month, 1);
                endCurr = today;
                startPrev = startCurr.Value.AddMonths(-1);
                endPrev = new DateTime(startPrev.Value.Year, startPrev.Value.Month, DateTime.DaysInMonth(startPrev.Value.Year, startPrev.Value.Month));
                periodLabel = "tháng trước";
            }

            using (var conn = _db.CreateConnection())
            {
                // 1. Calculate Balances
                string balanceSql = @"
                    WITH Balances AS (
                        SELECT t.ID, k.SoTaiKhoan, t.IsHoatDong,
                          (SELECT ISNULL(SUM(pth.SoTienThu), 0) FROM BAN_PhieuThuKhachHang pth WHERE pth.IDTaiKhoanThanhToan = t.ID AND pth.TrangThai = 2 AND pth.IsDeleted = 0) AS Thu,
                          (SELECT ISNULL(SUM(pc.SoTienChi), 0) FROM KT_PhieuChi pc WHERE pc.IDTaiKhoanThanhToan = t.ID AND pc.TrangThai = 2 AND pc.IsDeleted = 0) AS Chi
                        FROM DM_TaiKhoanThanhToan t
                        LEFT JOIN KT_TaiKhoanKeToan k ON t.IDTaiKhoanKeToan = k.ID
                    )
                    SELECT
                        SUM(CASE WHEN SoTaiKhoan LIKE '111%' THEN Thu - Chi ELSE 0 END) AS CashBalance,
                        SUM(CASE WHEN SoTaiKhoan LIKE '112%' THEN Thu - Chi ELSE 0 END) AS BankBalance,
                        COUNT(CASE WHEN SoTaiKhoan LIKE '112%' AND IsHoatDong = 1 THEN 1 END) AS BankAccountCount
                    FROM Balances";
                
                var balance = conn.QueryFirstOrDefault<dynamic>(balanceSql);
                decimal cashVal = balance?.CashBalance ?? 0;
                decimal bankVal = balance?.BankBalance ?? 0;
                int bankCount = balance?.BankAccountCount ?? 0;

                // 2. Accounts Payable
                string congNoSql = @"
                    SELECT SUM(ConLai) AS CongNoNCC
                    FROM (
                        SELECT pn.TongTienHang - ISNULL((SELECT SUM(pc2.SoTienChi) FROM KT_PhieuChi pc2 WHERE pc2.IDPhieuNhap = pn.ID AND pc2.TrangThai = 2 AND pc2.IsDeleted = 0), 0) AS ConLai
                        FROM KHO_PhieuNhap pn
                        WHERE pn.IsDeleted = 0
                          AND (@IDNhaCungCap IS NULL OR pn.IDNhaCungCap = @IDNhaCungCap)
                    ) t";
                decimal congNoVal = conn.QueryFirstOrDefault<decimal?>(congNoSql, new { IDNhaCungCap = idNhaCungCap }) ?? 0;

                string nccLabel = "Hạn chót: Cuối kỳ";
                if (idNhaCungCap.HasValue)
                {
                    string nccName = conn.QueryFirstOrDefault<string>("SELECT TenNhaCungCap FROM DM_NhaCungCap WHERE ID = @IDNhaCungCap", new { IDNhaCungCap = idNhaCungCap });
                    if (!string.IsNullOrEmpty(nccName))
                    {
                        nccLabel = "NCC: " + (nccName.Length > 20 ? nccName.Substring(0, 18) + "..." : nccName);
                    }
                }

                // 3. Current Period Chi
                int checkTrangThai = trangThai ?? 2;
                string currChiSql = @"
                    SELECT ISNULL(SUM(pc.SoTienChi), 0)
                    FROM KT_PhieuChi pc
                    LEFT JOIN NS_NhanSu ns ON pc.IDNguoiNhan = ns.ID
                    WHERE pc.IsDeleted = 0
                      AND pc.NgayChi >= @StartCurr AND pc.NgayChi <= @EndCurr
                      AND (@SoPhieuChi IS NULL OR pc.SoPhieuChi LIKE '%' + @SoPhieuChi + '%')
                      AND (@IDNhaCungCap IS NULL OR pc.IDNhaCungCap = @IDNhaCungCap)
                      AND (@IDKhoanMucChi IS NULL OR pc.IDKhoanMucChi = @IDKhoanMucChi)
                      AND (pc.TrangThai = @TrangThai)
                      AND (@NguoiNhanTien IS NULL OR pc.NguoiNhanTien LIKE '%' + @NguoiNhanTien + '%' OR (ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, '')) LIKE '%' + @NguoiNhanTien + '%')
                      AND (@IDTaiKhoanThanhToan IS NULL OR pc.IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan)";
                decimal currChi = conn.QueryFirstOrDefault<decimal>(currChiSql, new {
                    StartCurr = startCurr,
                    EndCurr = endCurr,
                    SoPhieuChi = string.IsNullOrEmpty(soPhieuChi) ? null : soPhieuChi,
                    IDNhaCungCap = idNhaCungCap,
                    IDKhoanMucChi = idKhoanMucChi,
                    TrangThai = checkTrangThai,
                    NguoiNhanTien = string.IsNullOrEmpty(nguoiNhanTien) ? null : nguoiNhanTien,
                    IDTaiKhoanThanhToan = idTaiKhoanThanhToan
                });

                // 4. Previous Period Chi
                string prevChiSql = @"
                    SELECT ISNULL(SUM(pc.SoTienChi), 0)
                    FROM KT_PhieuChi pc
                    LEFT JOIN NS_NhanSu ns ON pc.IDNguoiNhan = ns.ID
                    WHERE pc.IsDeleted = 0
                      AND pc.NgayChi >= @StartPrev AND pc.NgayChi <= @EndPrev
                      AND (@SoPhieuChi IS NULL OR pc.SoPhieuChi LIKE '%' + @SoPhieuChi + '%')
                      AND (@IDNhaCungCap IS NULL OR pc.IDNhaCungCap = @IDNhaCungCap)
                      AND (@IDKhoanMucChi IS NULL OR pc.IDKhoanMucChi = @IDKhoanMucChi)
                      AND (pc.TrangThai = @TrangThai)
                      AND (@NguoiNhanTien IS NULL OR pc.NguoiNhanTien LIKE '%' + @NguoiNhanTien + '%' OR (ISNULL(ns.HoDem, '') + ' ' + ISNULL(ns.Ten, '')) LIKE '%' + @NguoiNhanTien + '%')
                      AND (@IDTaiKhoanThanhToan IS NULL OR pc.IDTaiKhoanThanhToan = @IDTaiKhoanThanhToan)";
                decimal prevChi = conn.QueryFirstOrDefault<decimal>(prevChiSql, new {
                    StartPrev = startPrev,
                    EndPrev = endPrev,
                    SoPhieuChi = string.IsNullOrEmpty(soPhieuChi) ? null : soPhieuChi,
                    IDNhaCungCap = idNhaCungCap,
                    IDKhoanMucChi = idKhoanMucChi,
                    TrangThai = checkTrangThai,
                    NguoiNhanTien = string.IsNullOrEmpty(nguoiNhanTien) ? null : nguoiNhanTien,
                    IDTaiKhoanThanhToan = idTaiKhoanThanhToan
                });

                // Trend formatting
                decimal trendPct = 0;
                if (prevChi > 0)
                    trendPct = Math.Round(((currChi - prevChi) / prevChi) * 100, 1);
                else if (currChi > 0)
                    trendPct = 100;

                string trendText;
                string trendClass;
                if (trendPct > 0)
                {
                    trendText = $"~{trendPct}% so với {periodLabel}";
                    trendClass = "up";
                }
                else if (trendPct < 0)
                {
                    trendText = $"~{Math.Abs(trendPct)}% so với {periodLabel}";
                    trendClass = "down";
                }
                else
                {
                    trendText = $"0% so với {periodLabel}";
                    trendClass = "stable";
                }

                return new PhieuChiDashboardViewModel
                {
                    TongChi = currChi,
                    TongChiText = currChi.ToString("N0", cultureVi) + " đ",
                    TongChiTrend = trendText,
                    TongChiTrendClass = trendClass,

                    QuyTienMat = cashVal,
                    QuyTienMatText = cashVal.ToString("N0", cultureVi) + " đ",
                    QuyTienMatStatus = "Trạng thái: Ổn định",

                    DuNganHang = bankVal,
                    DuNganHangText = bankVal.ToString("N0", cultureVi) + " đ",
                    DuNganHangCount = bankCount,

                    CongNoNcc = congNoVal,
                    CongNoNccText = congNoVal.ToString("N0", cultureVi) + " đ",
                    CongNoNccLabel = nccLabel
                };
            }
        }

        public void DieuChinhPhanBo(int idPhieuChi, List<PhieuChiChiTietViewModel> newChiTiets, int userId, decimal soTienChiMoi)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Get existing allocations (All types)
                        var oldAllocations = conn.Query<PhieuChiChiTietViewModel>(
                            "SELECT * FROM KT_PhieuChiChiTiet WHERE IDPhieuChi = @IDPhieuChi",
                            new { IDPhieuChi = idPhieuChi },
                            transaction).ToList();

                        // Serialize for history
                        string oldJson = Newtonsoft.Json.JsonConvert.SerializeObject(oldAllocations);
                        string newJson = Newtonsoft.Json.JsonConvert.SerializeObject(newChiTiets);

                        // 2. Rollback old allocations (Only LoaiChi = 1 affects KHO_PhieuNhap)
                        foreach (var old in oldAllocations.Where(c => c.LoaiChi == 1))
                        {
                            if (old.IDPhieuNhap.HasValue)
                            {
                                conn.Execute(@"
                                    UPDATE KHO_PhieuNhap 
                                    SET DaThanhToan = ISNULL(DaThanhToan, 0) - @SoTienPhanBo,
                                        ConLai = ConLai + @SoTienPhanBo
                                    WHERE ID = @IDPhieuNhap
                                ", new { old.SoTienPhanBo, old.IDPhieuNhap }, transaction);
                            }
                        }

                        // 3. Delete old allocations
                        conn.Execute(
                            "DELETE FROM KT_PhieuChiChiTiet WHERE IDPhieuChi = @IDPhieuChi",
                            new { IDPhieuChi = idPhieuChi },
                            transaction);

                        // 3.5 Update SoTienChi
                        conn.Execute(
                            "UPDATE KT_PhieuChi SET SoTienChi = @SoTienChiMoi WHERE ID = @IDPhieuChi",
                            new { IDPhieuChi = idPhieuChi, SoTienChiMoi = soTienChiMoi },
                            transaction);

                        // 4. Insert new allocations and update Phiếu Nhập
                        foreach (var nc in newChiTiets)
                        {
                            conn.Execute(@"
                                INSERT INTO KT_PhieuChiChiTiet (IDPhieuChi, IDPhieuNhap, LoaiChi, SoTienPhanBo, DienGiai, NgayTao, NguoiTao)
                                VALUES (@IDPhieuChi, @IDPhieuNhap, @LoaiChi, @SoTienPhanBo, @DienGiai, GETDATE(), @NguoiTao)
                            ", new { 
                                IDPhieuChi = idPhieuChi, 
                                nc.IDPhieuNhap, 
                                nc.LoaiChi, 
                                nc.SoTienPhanBo, 
                                nc.DienGiai, 
                                NguoiTao = userId 
                            }, transaction);

                            if (nc.LoaiChi == 1 && nc.IDPhieuNhap.HasValue)
                            {
                                conn.Execute(@"
                                    UPDATE KHO_PhieuNhap 
                                    SET DaThanhToan = ISNULL(DaThanhToan, 0) + @SoTienPhanBo,
                                        ConLai = ConLai - @SoTienPhanBo
                                    WHERE ID = @IDPhieuNhap
                                ", new { nc.SoTienPhanBo, nc.IDPhieuNhap }, transaction);
                            }
                        }

                        // 5. Save history
                        conn.Execute(@"
                            INSERT INTO KT_PhieuChiLichSu (IDPhieuChi, NoiDungCu, NoiDungMoi, NguoiTao, NgayTao)
                            VALUES (@IDPhieuChi, @NoiDungCu, @NoiDungMoi, @NguoiTao, GETDATE())
                        ", new {
                            IDPhieuChi = idPhieuChi,
                            NoiDungCu = oldJson,
                            NoiDungMoi = newJson,
                            NguoiTao = userId
                        }, transaction);

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
