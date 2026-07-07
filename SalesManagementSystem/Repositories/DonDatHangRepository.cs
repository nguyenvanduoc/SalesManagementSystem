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
            try
            {
                using (var conn = _db.CreateConnection())
                {
                    conn.Execute("IF COL_LENGTH('NS_DonDatHangChiTiet', 'ThanhTienSauThue') IS NULL ALTER TABLE NS_DonDatHangChiTiet ADD ThanhTienSauThue DECIMAL(18,2) NULL");
                    conn.Execute("IF COL_LENGTH('NS_DonDatHang', 'PhiBocXep') IS NULL ALTER TABLE NS_DonDatHang ADD PhiBocXep DECIMAL(18,2) NULL");
                    conn.Execute("IF COL_LENGTH('NS_DonDatHangChiTiet', 'DonGiaBocXep') IS NULL ALTER TABLE NS_DonDatHangChiTiet ADD DonGiaBocXep DECIMAL(18,2) NULL");
                    conn.Execute("IF COL_LENGTH('NS_DonDatHangChiTiet', 'ThanhTienBocXep') IS NULL ALTER TABLE NS_DonDatHangChiTiet ADD ThanhTienBocXep DECIMAL(18,2) NULL");
                    conn.Execute("IF COL_LENGTH('NS_DonDatHangChiTiet', 'ThanhTienHang') IS NULL ALTER TABLE NS_DonDatHangChiTiet ADD ThanhTienHang DECIMAL(18,2) NULL");
                    
                    string initTrangThaiSql = @"
                        IF OBJECT_ID('DM_TrangThaiDonHang') IS NULL
                        BEGIN
                            CREATE TABLE DM_TrangThaiDonHang (
                                ID INT PRIMARY KEY,
                                TenTrangThai NVARCHAR(100) NOT NULL,
                                ThuTuHienThi INT NOT NULL,
                                KichHoat BIT NOT NULL DEFAULT 1
                            );
                            INSERT INTO DM_TrangThaiDonHang (ID, TenTrangThai, ThuTuHienThi, KichHoat) VALUES
                            (0, N'Lưu nháp', 0, 1),
                            (1, N'Chưa giao', 1, 1),
                            (2, N'Đang đi đường', 2, 1),
                            (3, N'Đã giao', 3, 1),
                            (4, N'Đã hủy', 4, 1);
                        END
                        ELSE
                        BEGIN
                            IF NOT EXISTS(SELECT 1 FROM DM_TrangThaiDonHang WHERE ID = 0)
                                INSERT INTO DM_TrangThaiDonHang (ID, TenTrangThai, ThuTuHienThi, KichHoat) VALUES (0, N'Lưu nháp', 0, 1);
                        END
                    ";
                    conn.Execute(initTrangThaiSql);
                }
            }
            catch { }
        }

        // ── GetPaged ────────────────────────────────────────────────────────
        public IEnumerable<DonDatHangViewModel> GetPaged(
            int page, int pageSize,
            string tuNgay, string denNgay,
            int? idKhachHang, int? idNhanVien,
            int? trangThai, string soDonHang,
            int? idPhuongTien, string hoTenTaiXe,
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
                p.Add("@IDPhuongTien", idPhuongTien);
                p.Add("@HoTenTaiXe",  string.IsNullOrWhiteSpace(hoTenTaiXe) ? null : hoTenTaiXe.Trim());
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
                      AND (@IDPhuongTien IS NULL OR d.IDPhuongTien = @IDPhuongTien)
                      AND (@HoTenTaiXe  IS NULL OR d.HoTenTaiXe LIKE '%' + @HoTenTaiXe + '%')
                      AND (@SoDonHang   IS NULL OR d.SoDonHang LIKE '%' + @SoDonHang + '%')";

                totalRecords = conn.ExecuteScalar<int>(countSql, p);

                string sql = @"
                    SELECT
                        d.ID, d.SoDonHang, d.NgayTaoDon, d.ThoiHanGiaoHang,
                        d.TrangThaiDon,
                        ISNULL(tt.TenTrangThai, N'Không xác định') AS TenTrangThai,
                        d.TongTien, d.GhiChu,
                        d.IDKhachHang,
                        k.MaKhachHang,
                        k.TenKhachHang,
                        d.IDNhanVien,
                        nv.HoDem + ' ' + nv.Ten AS TenNhanVien,
                        d.NgayTao, d.NguoiTao,
                        creator.HoDem + ' ' + creator.Ten AS TenNguoiTao,
                        d.SoDienThoaiTaiXe, d.HoTenTaiXe, d.IDPhuongTien,
                        pt.TenPhuongTien
                    FROM NS_DonDatHang d
                    LEFT JOIN NS_KhachHang k  ON d.IDKhachHang = k.ID
                    LEFT JOIN NS_NhanSu  nv ON d.IDNhanVien  = nv.ID
                    LEFT JOIN DM_TrangThaiDonHang tt ON d.TrangThaiDon = tt.ID
                    LEFT JOIN NS_NhanSu creator ON d.NguoiTao = creator.ID
                    LEFT JOIN DM_PhuongTien pt ON d.IDPhuongTien = pt.ID
                    WHERE (@TuNgay      IS NULL OR d.NgayTaoDon  >= @TuNgay)
                      AND (@DenNgay     IS NULL OR d.NgayTaoDon  <= @DenNgay)
                      AND (@IDKhachHang IS NULL OR d.IDKhachHang  = @IDKhachHang)
                      AND (@IDNhanVien  IS NULL OR d.IDNhanVien   = @IDNhanVien)
                      AND (@TrangThai   IS NULL OR d.TrangThaiDon = @TrangThai)
                      AND (@IDPhuongTien IS NULL OR d.IDPhuongTien = @IDPhuongTien)
                      AND (@HoTenTaiXe  IS NULL OR d.HoTenTaiXe LIKE '%' + @HoTenTaiXe + '%')
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
                        ct.SoLuong, ct.DonGia, ct.ThueGTGT, ct.ThanhTien, ct.ThanhTienThue, ct.ThanhTienSauThue,
                        ct.IsHangKhuyenMai, ct.GhiChu,
                        ct.DonGiaBocXep, ct.ThanhTienBocXep, ct.ThanhTienHang
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
                        if (string.IsNullOrEmpty(header.SoDonHang) || header.SoDonHang == "AUTO")
                        {
                            string genSoDonSql = @"
                                DECLARE @YearSuffix VARCHAR(2) = RIGHT(CAST(YEAR(GETDATE()) AS VARCHAR(4)), 2);
                                DECLARE @Prefix VARCHAR(4) = 'DH' + @YearSuffix;
                                DECLARE @MaxSoDon VARCHAR(20) = (
                                    SELECT TOP 1 SoDonHang 
                                    FROM NS_DonDatHang WITH (UPDLOCK) 
                                    WHERE SoDonHang LIKE @Prefix + '%' 
                                    ORDER BY SoDonHang DESC
                                );
                                DECLARE @NextNum INT = 1;
                                IF @MaxSoDon IS NOT NULL
                                BEGIN
                                    SET @NextNum = CAST(SUBSTRING(@MaxSoDon, 5, 6) AS INT) + 1;
                                END
                                SELECT @Prefix + RIGHT('000000' + CAST(@NextNum AS VARCHAR(6)), 6);";
                            
                            header.SoDonHang = conn.ExecuteScalar<string>(genSoDonSql, null, tran);
                        }

                        string headerSql = @"
                            INSERT INTO NS_DonDatHang
                                (IDKhachHang, NgayTaoDon, SoDonHang, IDNhanVien, ThoiHanGiaoHang,
                                 TrangThaiDon, TongTien, PhiBocXep, ThanhTienHang, ThanhTienThue, GhiChu, 
                                 SoDienThoaiTaiXe, HoTenTaiXe, IDPhuongTien, NgayTao, NguoiTao)
                            VALUES
                                (@IDKhachHang, @NgayTaoDon, @SoDonHang, @IDNhanVien, @ThoiHanGiaoHang,
                                 @TrangThaiDon, @TongTien, @PhiBocXep, @ThanhTienHang, @ThanhTienThue, @GhiChu, 
                                 @SoDienThoaiTaiXe, @HoTenTaiXe, @IDPhuongTien, @NgayTao, @NguoiTao);
                            SELECT CAST(SCOPE_IDENTITY() as int);";

                        int newId = conn.QuerySingle<int>(headerSql, header, tran);

                        string detailSql = @"
                            INSERT INTO NS_DonDatHangChiTiet
                                (IDDonDatHang, IDSanPham, SoLuong, DonGia, ThanhTien, ThanhTienSauThue, ThanhTienThue,
                                 ThueGTGT, IsHangKhuyenMai, GhiChu, DonGiaBocXep, ThanhTienBocXep, ThanhTienHang,
                                 NgayTaoDon, SoDonHang, IDNhanVien, ThoiHanGiaoHang, TrangThaiDon,
                                 NgayTao, NguoiTao)
                            VALUES
                                (@IDDonDatHang, @IDSanPham, @SoLuong, @DonGia, @ThanhTien, @ThanhTienSauThue, @ThanhTienThue,
                                 @ThueGTGT, @IsHangKhuyenMai, @GhiChu, @DonGiaBocXep, @ThanhTienBocXep, @ThanhTienHang,
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
                        string updateHeaderSql = @"
                            UPDATE NS_DonDatHang SET
                                IDKhachHang     = @IDKhachHang,
                                NgayTaoDon      = @NgayTaoDon,
                                SoDonHang       = @SoDonHang,
                                IDNhanVien      = @IDNhanVien,
                                ThoiHanGiaoHang = @ThoiHanGiaoHang,
                                TrangThaiDon    = @TrangThaiDon,
                                TongTien        = @TongTien,
                                PhiBocXep       = @PhiBocXep,
                                ThanhTienHang   = @ThanhTienHang,
                                ThanhTienThue   = @ThanhTienThue,
                                GhiChu          = @GhiChu,
                                SoDienThoaiTaiXe = @SoDienThoaiTaiXe,
                                HoTenTaiXe      = @HoTenTaiXe,
                                IDPhuongTien    = @IDPhuongTien,
                                NgayCapNhat     = @NgayCapNhat,
                                NguoiCapNhat    = @NguoiCapNhat
                            WHERE ID = @ID";

                        conn.Execute(updateHeaderSql, header, tran);

                        // Dong bo chi tiet: xoa dong bi bo, update dong cu, insert dong moi.
                        var existingIds = conn.Query<int>(
                            "SELECT ID FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @ID",
                            new { ID = header.ID }, tran).ToList();
                        var postedIds = chiTiets.Where(x => x.ID > 0).Select(x => x.ID).ToList();
                        var deleteIds = existingIds.Except(postedIds).ToList();

                        if (deleteIds.Any())
                        {
                            conn.Execute(
                                "DELETE FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = @IDDonDatHang AND ID IN @IDs",
                                new { IDDonDatHang = header.ID, IDs = deleteIds }, tran);
                        }

                        string insertDetailSql = @"
                            INSERT INTO NS_DonDatHangChiTiet
                                (IDDonDatHang, IDSanPham, SoLuong, DonGia, ThanhTien, ThanhTienSauThue, ThanhTienThue,
                                 ThueGTGT, IsHangKhuyenMai, GhiChu, DonGiaBocXep, ThanhTienBocXep, ThanhTienHang,
                                 NgayTaoDon, SoDonHang, IDNhanVien, ThoiHanGiaoHang, TrangThaiDon,
                                 NgayTao, NguoiTao)
                            VALUES
                                (@IDDonDatHang, @IDSanPham, @SoLuong, @DonGia, @ThanhTien, @ThanhTienSauThue, @ThanhTienThue,
                                 @ThueGTGT, @IsHangKhuyenMai, @GhiChu, @DonGiaBocXep, @ThanhTienBocXep, @ThanhTienHang,
                                 @NgayTaoDon, @SoDonHang, @IDNhanVien, @ThoiHanGiaoHang, @TrangThaiDon,
                                 @NgayTao, @NguoiTao)";

                        string updateDetailSql = @"
                            UPDATE NS_DonDatHangChiTiet SET
                                IDSanPham       = @IDSanPham,
                                SoLuong         = @SoLuong,
                                DonGia          = @DonGia,
                                ThanhTien       = @ThanhTien,
                                ThanhTienSauThue= @ThanhTienSauThue,
                                ThanhTienThue   = @ThanhTienThue,
                                ThueGTGT        = @ThueGTGT,
                                IsHangKhuyenMai = @IsHangKhuyenMai,
                                GhiChu          = @GhiChu,
                                DonGiaBocXep    = @DonGiaBocXep,
                                ThanhTienBocXep = @ThanhTienBocXep,
                                ThanhTienHang   = @ThanhTienHang,
                                NgayTaoDon      = @NgayTaoDon,
                                SoDonHang       = @SoDonHang,
                                IDNhanVien      = @IDNhanVien,
                                ThoiHanGiaoHang = @ThoiHanGiaoHang,
                                TrangThaiDon    = @TrangThaiDon,
                                NgayCapNhat     = @NgayCapNhat,
                                NguoiCapNhat    = @NguoiCapNhat
                            WHERE ID = @ID AND IDDonDatHang = @IDDonDatHang";

                        foreach (var ct in chiTiets)
                        {
                            ct.IDDonDatHang    = header.ID;
                            ct.NgayTaoDon      = header.NgayTaoDon;
                            ct.SoDonHang       = header.SoDonHang;
                            ct.IDNhanVien      = header.IDNhanVien;
                            ct.ThoiHanGiaoHang = header.ThoiHanGiaoHang;
                            ct.TrangThaiDon    = header.TrangThaiDon;
                            ct.NgayCapNhat     = header.NgayCapNhat;
                            ct.NguoiCapNhat    = header.NguoiCapNhat;

                            if (ct.ID > 0 && existingIds.Contains(ct.ID))
                            {
                                conn.Execute(updateDetailSql, ct, tran);
                            }
                            else
                            {
                                ct.NgayTao  = DateTime.Now;
                                ct.NguoiTao = header.NguoiCapNhat;
                                conn.Execute(insertDetailSql, ct, tran);
                            }
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

        // ── GetTrangThaiList ──────────────────────────────────────────────────
        public IEnumerable<DM_TrangThaiDonHang> GetTrangThaiList()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<DM_TrangThaiDonHang>("SELECT * FROM DM_TrangThaiDonHang WHERE KichHoat = 1 ORDER BY ThuTuHienThi");
            }
        }

        public bool UpdateStatus(int id, int newStatus, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        string sql = "UPDATE NS_DonDatHang SET TrangThaiDon = @NewStatus, NgayCapNhat = GETDATE(), NguoiCapNhat = @UserId WHERE ID = @ID AND TrangThaiDon != 3 AND TrangThaiDon != 4";
                        int rows = conn.Execute(sql, new { ID = id, NewStatus = newStatus, UserId = userId }, transaction: tr);
                        if (rows > 0)
                        {
                            conn.Execute("UPDATE NS_DonDatHangChiTiet SET TrangThaiDon = @NewStatus, NgayCapNhat = GETDATE(), NguoiCapNhat = @UserId WHERE IDDonDatHang = @ID", new { ID = id, NewStatus = newStatus, UserId = userId }, transaction: tr);
                            
                            // Nếu hủy đơn đặt hàng, hủy luôn chứng từ và phiếu xuất kho liên quan
                            if (newStatus == 4)
                            {
                                // 1. Hủy các chứng từ bán hàng liên quan
                                var listChungTu = conn.Query<int>("SELECT ID FROM BAN_ChungTuBanHang WHERE IDDonDatHang = @ID AND TrangThai != 3", new { ID = id }, transaction: tr).ToList();
                                foreach (var idChungTu in listChungTu)
                                {
                                    var p = new DynamicParameters();
                                    p.Add("@ID", idChungTu);
                                    p.Add("@NguoiHuy", userId);
                                    p.Add("@LyDoHuy", "Hủy theo đơn đặt hàng");
                                    conn.Execute("UPDATE BAN_ChungTuBanHang SET TrangThai = 3, NguoiCapNhat = @NguoiHuy, NgayCapNhat = GETDATE() WHERE ID = @ID", new { NguoiHuy = userId, ID = idChungTu }, transaction: tr);
                                    
                                    conn.Execute("UPDATE KT_NhatKyChung SET IsHuy = 1 WHERE LoaiChungTu = 'BAN' AND IDChungTu = @ID", new { ID = idChungTu }, transaction: tr);
                                }

                                // 2. Hủy các phiếu xuất kho liên quan (trực tiếp từ IDDonDatHang hoặc qua IDChungTuBanHang)
                                var listPhieuXuat = conn.Query<int>(@"
                                    SELECT ID FROM KHO_PhieuXuat 
                                    WHERE (IDDonDatHang = @ID OR IDChungTuBanHang IN (SELECT ID FROM BAN_ChungTuBanHang WHERE IDDonDatHang = @ID)) 
                                    AND TrangThai != 3", new { ID = id }, transaction: tr).ToList();

                                foreach (var idPhieuXuat in listPhieuXuat)
                                {
                                    conn.Execute("UPDATE KHO_PhieuXuat SET TrangThai = 3, NguoiCapNhat = @NguoiHuy, NgayCapNhat = GETDATE() WHERE ID = @IDPhieuXuat", new { NguoiHuy = userId, IDPhieuXuat = idPhieuXuat }, transaction: tr);
                                    conn.Execute("DELETE FROM KHO_GiaoDichKho WHERE LoaiChungTu = 2 AND SoChungTu = (SELECT SoChungTu FROM KHO_PhieuXuat WHERE ID = @IDPhieuXuat)", new { IDPhieuXuat = idPhieuXuat }, transaction: tr);
                                }
                            }
                            
                            tr.Commit();
                            return true;
                        }
                        return false;
                    }
                    catch
                    {
                        tr.Rollback();
                        return false;
                    }
                }
            }
        }

        // ── GenerateSoDonHang ────────────────────────────────────────────────
        public string GenerateSoDonHang()
        {
            using (var conn = _db.CreateConnection())
            {
                string genSoDonSql = @"
                    DECLARE @YearSuffix VARCHAR(2) = RIGHT(CAST(YEAR(GETDATE()) AS VARCHAR(4)), 2);
                    DECLARE @Prefix VARCHAR(4) = 'DH' + @YearSuffix;
                    DECLARE @MaxSoDon VARCHAR(20) = (
                        SELECT TOP 1 SoDonHang 
                        FROM NS_DonDatHang WITH (UPDLOCK) 
                        WHERE SoDonHang LIKE @Prefix + '%' 
                        ORDER BY SoDonHang DESC
                    );
                    DECLARE @NextNum INT = 1;
                    IF @MaxSoDon IS NOT NULL
                    BEGIN
                        SET @NextNum = CAST(SUBSTRING(@MaxSoDon, 5, 6) AS INT) + 1;
                    END
                    SELECT @Prefix + RIGHT('000000' + CAST(@NextNum AS VARCHAR(6)), 6);";
                return conn.ExecuteScalar<string>(genSoDonSql);
            }
        }
    }
}
