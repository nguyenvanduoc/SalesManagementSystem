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
                return conn.QueryFirstOrDefault<PhieuChiViewModel>(
                    "sp_KT_PhieuChi_GetByID",
                    new { ID = id },
                    commandType: CommandType.StoredProcedure
                );
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
                return p.Get<int>("@NewID");
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
    }
}
