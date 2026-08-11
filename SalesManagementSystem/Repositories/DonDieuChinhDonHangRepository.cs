    using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Dapper;
using Newtonsoft.Json;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class DonDieuChinhDonHangRepository : IDonDieuChinhDonHangRepository
    {
        private readonly DbConnectionFactory _db;

        public DonDieuChinhDonHangRepository(DbConnectionFactory db)
        {
            _db = db;
            try
            {
                using (var conn = _db.CreateConnection())
                {
                    string sql = @"
                        IF OBJECT_ID('DON_DieuChinhDonHang') IS NULL
                        BEGIN
                            CREATE TABLE DON_DieuChinhDonHang
                            (
                                ID INT IDENTITY PRIMARY KEY,
                                IDDonHang INT NOT NULL,
                                SoDieuChinh NVARCHAR(50) NOT NULL,
                                NgayDieuChinh DATETIME NOT NULL,
                                LyDoDieuChinh NVARCHAR(1000) NULL,
                                TongTienCu DECIMAL(18,2) NOT NULL DEFAULT 0,
                                TongTienMoi DECIMAL(18,2) NOT NULL DEFAULT 0,
                                NguoiTao INT NULL,
                                NgayTao DATETIME NULL
                            );
                        END

                        IF OBJECT_ID('DON_DieuChinhDonHang_ChiTiet') IS NULL
                        BEGIN
                            CREATE TABLE DON_DieuChinhDonHang_ChiTiet
                            (
                                ID INT IDENTITY PRIMARY KEY,
                                IDDieuChinh INT NOT NULL,
                                IDSanPham INT NOT NULL,
                                SoLuongCu DECIMAL(18,2) NULL,
                                SoLuongMoi DECIMAL(18,2) NULL,
                                DonGiaCu DECIMAL(18,2) NULL,
                                DonGiaMoi DECIMAL(18,2) NULL,
                                ThanhTienCu DECIMAL(18,2) NULL,
                                ThanhTienMoi DECIMAL(18,2) NULL,
                                GhiChu NVARCHAR(500) NULL
                            );
                        END

                        DECLARE @ManHinhID INT;
                        IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'Điều chỉnh đơn hàng')
                        BEGIN
                            INSERT INTO ACL_ManHinh (TenManHinh, NhomChaManHinh, IsSuDung, STT)
                            VALUES (N'Điều chỉnh đơn hàng', N'BAN HANG', 1, 1028);
                            SET @ManHinhID = SCOPE_IDENTITY();
                        END
                        ELSE
                        BEGIN
                            SELECT @ManHinhID = ID FROM ACL_ManHinh WHERE TenManHinh = N'Điều chỉnh đơn hàng';
                        END

                        IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Index')
                            INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                            VALUES (@ManHinhID, 'Index', 'DonDieuChinhDonHang', 1, N'Xem danh sách điều chỉnh đơn hàng');

                        IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Adjust')
                            INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                            VALUES (@ManHinhID, 'Adjust', 'DonDieuChinhDonHang', 3, N'Thực hiện điều chỉnh đơn hàng');

                        IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'History')
                            INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                            VALUES (@ManHinhID, 'History', 'DonDieuChinhDonHang', 1, N'Xem lịch sử điều chỉnh');

                        INSERT INTO ACL_PhanQuyen (IDLogin, IDAction, IsChoPhep, NgayTao)
                        SELECT l.ID, act.ID, 1, GETDATE()
                        FROM ACL_Login l
                        CROSS JOIN ACL_Action act
                        WHERE act.IDManHinh = @ManHinhID
                          AND NOT EXISTS (
                              SELECT 1 FROM ACL_PhanQuyen pq 
                              WHERE pq.IDLogin = l.ID AND pq.IDAction = act.ID
                          );
                    ";
                    conn.Execute(sql);

                    // Tạo Stored Procedure
                    // Tạo/cập nhật Stored Procedure từ file SQL
                    string spFile = HostingEnvironment.MapPath("~/App_Data/sp_DON_DieuChinhDonHang_Save.sql");
                    if (spFile != null && File.Exists(spFile))
                    {
                        string spSql = File.ReadAllText(spFile, System.Text.Encoding.UTF8);
                        conn.Execute(spSql);
                    }
                }
            }
            catch { }
        }

        public IEnumerable<DonDieuChinhListViewModel> GetPaged(
            int page, int pageSize,
            string tuNgay, string denNgay,
            int? idKhachHang, string soDonHang,
            bool chiDonDieuChinh,
            out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrWhiteSpace(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrWhiteSpace(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay).AddDays(1).AddSeconds(-1));
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@SoDonHang", string.IsNullOrWhiteSpace(soDonHang) ? null : soDonHang.Trim());
                p.Add("@ChiDonDieuChinh", chiDonDieuChinh ? 1 : 0);
                p.Add("@Offset", (page - 1) * pageSize);
                p.Add("@PageSize", pageSize);

                // Chỉ hiển thị các đơn: Đã lập chứng từ OR Đã xuất kho OR Đã phát sinh phiếu thu
                string filterSql = @"
                    d.TrangThaiDon != 4
                    AND (
                        EXISTS (SELECT 1 FROM BAN_ChungTuBanHang c WHERE c.IDDonDatHang = d.ID AND c.IsDeleted = 0)
                        OR EXISTS (SELECT 1 FROM KHO_PhieuXuat px WHERE px.IDDonDatHang = d.ID AND px.TrangThai = 2 AND px.IsDeleted = 0)
                        OR EXISTS (
                            SELECT 1 
                            FROM BAN_PhieuThuKhachHang pt 
                            INNER JOIN BAN_ChungTuBanHang c2 ON pt.IDChungTuBanHang = c2.ID 
                            WHERE c2.IDDonDatHang = d.ID AND pt.TrangThai = 2 AND pt.IsDeleted = 0 AND c2.IsDeleted = 0
                        )
                    )
                    AND (@TuNgay IS NULL OR d.NgayTaoDon >= @TuNgay)
                    AND (@DenNgay IS NULL OR d.NgayTaoDon <= @DenNgay)
                    AND (@IDKhachHang IS NULL OR d.IDKhachHang = @IDKhachHang)
                    AND (@SoDonHang IS NULL OR d.SoDonHang LIKE '%' + @SoDonHang + '%')
                    AND (@ChiDonDieuChinh = 0 OR EXISTS (SELECT 1 FROM DON_DieuChinhDonHang dc WHERE dc.IDDonHang = d.ID))";

                string countSql = "SELECT COUNT(1) FROM NS_DonDatHang d WHERE " + filterSql;
                totalRecords = conn.ExecuteScalar<int>(countSql, p);

                string sql = $@"
                    SELECT
                        d.ID, d.SoDonHang, d.NgayTaoDon, d.TrangThaiDon, d.HoTenTaiXe,
                        k.TenKhachHang,
                        d.TongTien,
                        ISNULL((
                            SELECT SUM(pt.SoTienThu) 
                            FROM BAN_PhieuThuKhachHang pt 
                            INNER JOIN BAN_ChungTuBanHang c ON pt.IDChungTuBanHang = c.ID 
                            WHERE c.IDDonDatHang = d.ID AND pt.TrangThai = 2 AND pt.IsDeleted = 0 AND c.IsDeleted = 0
                        ), 0) AS DaThanhToan,
                        ISNULL(tt.TenTrangThai, N'Không xác định') AS TenTrangThai,
                        CAST(CASE WHEN EXISTS (SELECT 1 FROM DON_DieuChinhDonHang dc WHERE dc.IDDonHang = d.ID) THEN 1 ELSE 0 END AS BIT) AS DaDieuChinh,
                        ISNULL((SELECT COUNT(1) FROM DON_DieuChinhDonHang dc WHERE dc.IDDonHang = d.ID), 0) AS SoLanDieuChinh,
                        (SELECT MAX(dc.NgayDieuChinh) FROM DON_DieuChinhDonHang dc WHERE dc.IDDonHang = d.ID) AS NgayDieuChinh,
                        (
                            SELECT TOP 1 ns.HoDem + ' ' + ns.Ten 
                            FROM DON_DieuChinhDonHang dc 
                            LEFT JOIN NS_NhanSu ns ON dc.NguoiTao = ns.ID 
                            WHERE dc.IDDonHang = d.ID 
                            ORDER BY dc.NgayDieuChinh DESC, dc.ID DESC
                        ) AS NguoiDieuChinh
                    FROM NS_DonDatHang d
                    LEFT JOIN NS_KhachHang k ON d.IDKhachHang = k.ID
                    LEFT JOIN DM_TrangThaiDonHang tt ON d.TrangThaiDon = tt.ID
                    WHERE {filterSql}
                    ORDER BY d.ID DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                return conn.Query<DonDieuChinhListViewModel>(sql, p).ToList();
            }
        }

        public IEnumerable<DonDieuChinhHistoryViewModel> GetAdjustHistory(int idDonHang)
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = @"
                    SELECT 
                        dc.ID,
                        dc.SoDieuChinh,
                        dc.NgayDieuChinh,
                        dc.LyDoDieuChinh,
                        dc.TongTienCu,
                        dc.TongTienMoi,
                        dc.TrangThaiDon,
                        tt.TenTrangThai,
                        ns.HoDem + ' ' + ns.Ten AS TenNguoiTao
                    FROM DON_DieuChinhDonHang dc
                    LEFT JOIN NS_NhanSu ns ON dc.NguoiTao = ns.ID
                    LEFT JOIN DM_TrangThaiDonHang tt ON dc.TrangThaiDon = tt.ID
                    WHERE dc.IDDonHang = @IDDonHang
                    ORDER BY dc.NgayDieuChinh DESC, dc.ID DESC";

                var histories = conn.Query<DonDieuChinhHistoryViewModel>(sql, new { IDDonHang = idDonHang }).ToList();

                string detailSql = @"
                    SELECT 
                        sp.TenSanPham,
                        sp.MaSanPham,
                        sp.DVT,
                        ct.SoLuongCu,
                        ct.SoLuongMoi,
                        ct.DonGiaCu,
                        ct.DonGiaMoi,
                        ct.ThanhTienCu,
                        ct.ThanhTienMoi,
                        ct.GhiChu
                    FROM DON_DieuChinhDonHang_ChiTiet ct
                    LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                    WHERE ct.IDDieuChinh = @IDDieuChinh
                    ORDER BY ct.ID";

                foreach (var h in histories)
                {
                    h.ChiTiets = conn.Query<DonDieuChinhHistoryDetailViewModel>(detailSql, new { IDDieuChinh = h.ID }).ToList();
                }

                return histories;
            }
        }

        public void SaveAdjustment(DonDieuChinhPostModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDDonHang", model.IDDonHang);
                p.Add("@LyDoDieuChinh", model.LyDoDieuChinh);
                p.Add("@ChiTietsJson", model.ChiTietsJson);
                p.Add("@PhiBocXep", model.PhiBocXep);
                p.Add("@IDKho", model.IDKho);
                p.Add("@NguoiTao", userId);
                p.Add("@IDKhachHang", model.IDKhachHang);
                p.Add("@IDNhanVien", model.IDNhanVien);
                p.Add("@NgayTaoDon", model.NgayTaoDon);
                p.Add("@NgayGiaoHang", model.NgayGiaoHang);
                p.Add("@ThoiHanGiaoHang", model.ThoiHanGiaoHang);

                conn.Execute("sp_DON_DieuChinhDonHang_Save", p, commandType: CommandType.StoredProcedure);

                // Update additional info
                conn.Execute("UPDATE NS_DonDatHang SET GhiChu = @GhiChu, HoTenTaiXe = @HoTenTaiXe, IDPhuongTien = @IDPhuongTien WHERE ID = @ID", 
                    new { GhiChu = model.GhiChu, HoTenTaiXe = model.HoTenTaiXe, IDPhuongTien = model.IDPhuongTien, ID = model.IDDonHang });
            }
        }
    }
}
