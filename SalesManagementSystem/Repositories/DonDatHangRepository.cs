using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class DonDatHangRepository : IDonDatHangRepository
    {
        private readonly DbConnectionFactory _db;

        public DonDatHangRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        // ── GetPaged ────────────────────────────────────────────────────────
        public IEnumerable<DonDatHangViewModel> GetPaged(
            int page, int pageSize,
            string tuNgay, string denNgay,
            int? idKhachHang, int? idNhanVien,
            int? trangThai, string soDonHang,
            out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay",      string.IsNullOrWhiteSpace(tuNgay)    ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay",     string.IsNullOrWhiteSpace(denNgay)   ? (DateTime?)null : DateTime.Parse(denNgay).AddDays(1).AddSeconds(-1));
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@IDNhanVien",  idNhanVien);
                p.Add("@TrangThai",   trangThai);
                p.Add("@SoDonHang",   string.IsNullOrWhiteSpace(soDonHang) ? null : soDonHang.Trim());
                p.Add("@Offset",      (page - 1) * pageSize);
                p.Add("@PageSize",    pageSize);

                string countSql = @"
                    SELECT COUNT(1)
                    FROM NS_DonDatHang d
                    WHERE (@TuNgay      IS NULL OR d.NgayTaoDon  >= @TuNgay)
                      AND (@DenNgay     IS NULL OR d.NgayTaoDon  <= @DenNgay)
                      AND (@IDKhachHang IS NULL OR d.IDKhachHang  = @IDKhachHang)
                      AND (@IDNhanVien  IS NULL OR d.IDNhanVien   = @IDNhanVien)
                      AND (@TrangThai   IS NULL OR d.TrangThaiDon = @TrangThai)
                      AND (@SoDonHang   IS NULL OR d.SoDonHang LIKE '%' + @SoDonHang + '%')";

                totalRecords = conn.ExecuteScalar<int>(countSql, p);

                string sql = @"
                    SELECT
                        d.ID, d.SoDonHang, d.NgayTaoDon, d.ThoiHanGiaoHang,
                        d.TrangThaiDon,
                        CASE d.TrangThaiDon
                            WHEN 1 THEN N'Chưa giao'
                            WHEN 2 THEN N'Đang đi đường'
                            WHEN 3 THEN N'Đã giao'
                            ELSE N'Không xác định'
                        END AS TenTrangThai,
                        d.TongTien, d.GhiChu,
                        d.IDKhachHang,
                        k.MaKhachHang,
                        ISNULL(k.HoDem,'') + ' ' + ISNULL(k.Ten,'') AS TenKhachHang,
                        d.IDNhanVien,
                        nv.HoTen AS TenNhanVien,
                        d.NgayTao, d.NguoiTao
                    FROM NS_DonDatHang d
                    LEFT JOIN NS_KhachHang k  ON d.IDKhachHang = k.ID
                    LEFT JOIN NS_NhanVien  nv ON d.IDNhanVien  = nv.ID
                    WHERE (@TuNgay      IS NULL OR d.NgayTaoDon  >= @TuNgay)
                      AND (@DenNgay     IS NULL OR d.NgayTaoDon  <= @DenNgay)
                      AND (@IDKhachHang IS NULL OR d.IDKhachHang  = @IDKhachHang)
                      AND (@IDNhanVien  IS NULL OR d.IDNhanVien   = @IDNhanVien)
                      AND (@TrangThai   IS NULL OR d.TrangThaiDon = @TrangThai)
                      AND (@SoDonHang   IS NULL OR d.SoDonHang LIKE '%' + @SoDonHang + '%')
                    ORDER BY d.ID DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                return conn.Query<DonDatHangViewModel>(sql, p).ToList();
            }
        }

        // ── GetById ─────────────────────────────────────────────────────────
        public NS_DonDatHang GetById(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<NS_DonDatHang>(
                    "SELECT * FROM NS_DonDatHang WHERE ID = @ID", new { ID = id });
            }
        }

        // ── GetChiTietByDonId ────────────────────────────────────────────────
        public List<DonDatHangChiTietViewModel> GetChiTietByDonId(int idDon)
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = @"
                    SELECT
                        ct.ID, ct.IDDonDatHang, ct.IDSanPham,
                        sp.MaSanPham, sp.TenSanPham, sp.DVT,
                        ct.SoLuong, ct.DonGia, ct.ThueGTGT, ct.ThanhTien,
                        ct.IsHangKhuyenMai, ct.GhiChu
                    FROM NS_DonDatHangChiTiet ct
                    LEFT JOIN DM_SanPham sp ON ct.IDSanPham = sp.ID
                    WHERE ct.IDDonDatHang = @IDDon
                    ORDER BY ct.ID";

                return conn.Query<DonDatHangChiTietViewModel>(sql, new { IDDon = idDon }).ToList();
            }
        }

        // ── CheckDuplicate ───────────────────────────────────────────────────
        public bool CheckDuplicateSoDon(string soDonHang, int excludeId = 0)
        {
            using (var conn = _db.CreateConnection())
            {
                int count = conn.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM NS_DonDatHang WHERE SoDonHang = @SoDon AND ID != @ExcludeId",
                    new { SoDon = soDonHang, ExcludeId = excludeId });
                return count > 0;
            }
        }

        // ── Insert (Transaction) ─────────────────────────────────────────────
        public int Insert(NS_DonDatHang header, List<NS_DonDatHangChiTiet> chiTiets)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        string headerSql = @"
                            INSERT INTO NS_DonDatHang
                                (IDKhachHang, NgayTaoDon, SoDonHang, IDNhanVien,
                                 ThoiHanGiaoHang, TrangThaiDon, TongTien, GhiChu,
                                 NgayTao, NguoiTao)
                            VALUES
                                (@IDKhachHang, @NgayTaoDon, @SoDonHang, @IDNhanVien,
                                 @ThoiHanGiaoHang, @TrangThaiDon, @TongTien, @GhiChu,
                                 @NgayTao, @NguoiTao);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int newId = conn.QuerySingle<int>(headerSql, header, tran);

                        string detailSql = @"
                            INSERT INTO NS_DonDatHangChiTiet
                                (IDDonDatHang, IDSanPham, SoLuong, DonGia, ThanhTien,
                                 ThueGTGT, IsHangKhuyenMai, GhiChu,
                                 NgayTaoDon, SoDonHang, IDNhanVien, ThoiHanGiaoHang, TrangThaiDon,
                                 NgayTao, NguoiTao)
                            VALUES
                                (@IDDonDatHang, @IDSanPham, @SoLuong, @DonGia, @ThanhTien,
                                 @ThueGTGT, @IsHangKhuyenMai, @GhiChu,
                                 @NgayTaoDon, @SoDonHang, @IDNhanVien, @ThoiHanGiaoHang, @TrangThaiDon,
                                 @NgayTao, @NguoiTao)";

                        foreach (var ct in chiTiets)
                        {
                            ct.IDDonDatHang    = newId;
                            ct.NgayTaoDon      = header.NgayTaoDon;
                            ct.SoDonHang       = header.SoDonHang;
                            ct.IDNhanVien      = header.IDNhanVien;
                            ct.ThoiHanGiaoHang = header.ThoiHanGiaoHang;
                            ct.TrangThaiDon    = header.TrangThaiDon;
                            ct.NgayTao         = header.NgayTao;
                            ct.NguoiTao        = header.NguoiTao;
                            conn.Execute(detailSql, ct, tran);
                        }

                        tran.Commit();
                        return newId;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        // ── Update (Transaction) ─────────────────────────────────────────────
        public bool Update(NS_DonDatHang header, List<NS_DonDatHangChiTiet> chiTiets)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        string headerSql = @"
                            UPDATE NS_DonDatHang SET
                                IDKhachHang     = @IDKhachHang,
                                NgayTaoDon      = @NgayTaoDon,
                                SoDonHang       = @SoDonHang,
                                IDNhanVien      = @IDNhanVien,
                                ThoiHanGiaoHang = @ThoiHanGiaoHang,
                                TrangThaiDon    = @TrangThaiDon,
                                TongTien        = @TongTien,
                                GhiChu          = @GhiChu,
                                NgayCapNhat     = @NgayCapNhat,
                                NguoiCapNhat    = @NguoiCapNhat
                            WHERE ID = @ID";

                        conn.Execute(headerSql, header, tran);

                        // Xóa chi tiết cũ, chèn lại
                        conn.Execute("DELETE FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @ID",
                                     new { ID = header.ID }, tran);

                        string detailSql = @"
                            INSERT INTO NS_DonDatHangChiTiet
                                (IDDonDatHang, IDSanPham, SoLuong, DonGia, ThanhTien,
                                 ThueGTGT, IsHangKhuyenMai, GhiChu,
                                 NgayTaoDon, SoDonHang, IDNhanVien, ThoiHanGiaoHang, TrangThaiDon,
                                 NgayTao, NguoiTao)
                            VALUES
                                (@IDDonDatHang, @IDSanPham, @SoLuong, @DonGia, @ThanhTien,
                                 @ThueGTGT, @IsHangKhuyenMai, @GhiChu,
                                 @NgayTaoDon, @SoDonHang, @IDNhanVien, @ThoiHanGiaoHang, @TrangThaiDon,
                                 @NgayTao, @NguoiTao)";

                        foreach (var ct in chiTiets)
                        {
                            ct.IDDonDatHang    = header.ID;
                            ct.NgayTaoDon      = header.NgayTaoDon;
                            ct.SoDonHang       = header.SoDonHang;
                            ct.IDNhanVien      = header.IDNhanVien;
                            ct.ThoiHanGiaoHang = header.ThoiHanGiaoHang;
                            ct.TrangThaiDon    = header.TrangThaiDon;
                            ct.NgayTao         = header.NgayTao;
                            ct.NguoiTao        = header.NguoiTao;
                            conn.Execute(detailSql, ct, tran);
                        }

                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        // ── Delete ───────────────────────────────────────────────────────────
        public bool Delete(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        conn.Execute("DELETE FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @ID",
                                     new { ID = id }, tran);
                        conn.Execute("DELETE FROM NS_DonDatHang WHERE ID = @ID",
                                     new { ID = id }, tran);
                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
