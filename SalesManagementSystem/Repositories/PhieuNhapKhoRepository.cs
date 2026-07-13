using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class PhieuNhapKhoRepository : IPhieuNhapKhoRepository
    {
        private readonly DbConnectionFactory _db;

        public PhieuNhapKhoRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<PhieuNhapKhoListViewModel> GetPaged(
            int page, int pageSize,
            string tuNgay, string denNgay,
            string soChungTu, int? idKho, int? idNhaCungCap, 
            int? trangThai, string tenNguoiNhan,
            string tenNguoiGiao, int? idPhuongTien, string hoTenTaiXe,
            int? idSanPham,
            out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                try {
                    string sqlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "update_sp_KHO_PhieuNhap_GetList.sql");
                    if (System.IO.File.Exists(sqlPath)) {
                        string sql = System.IO.File.ReadAllText(sqlPath);
                        var parts = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            if (!string.IsNullOrWhiteSpace(part)) conn.Execute(part);
                        }
                        System.IO.File.Delete(sqlPath);
                    }
                } catch { }

                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrWhiteSpace(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrWhiteSpace(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay).AddDays(1).AddSeconds(-1));
                p.Add("@SoChungTu", string.IsNullOrWhiteSpace(soChungTu) ? null : soChungTu.Trim());
                p.Add("@IDKho", idKho);
                p.Add("@IDNhaCungCap", idNhaCungCap);
                p.Add("@TrangThai", trangThai);
                p.Add("@TenNguoiNhan", tenNguoiNhan);
                p.Add("@TenNguoiGiao", tenNguoiGiao);
                p.Add("@IDPhuongTien", idPhuongTien);
                p.Add("@IDSanPham", idSanPham);
                p.Add("@Offset", (page - 1) * pageSize);
                p.Add("@PageSize", pageSize);
                p.Add("@TotalRecords", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var list = conn.Query<PhieuNhapKhoListViewModel>(
                    "sp_KHO_PhieuNhap_GetList", 
                    p, 
                    commandType: CommandType.StoredProcedure).ToList();
                
                totalRecords = p.Get<int>("@TotalRecords");
                return list;
            }
        }

        public KHO_PhieuNhap GetByID(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<KHO_PhieuNhap>(
                    "sp_KHO_PhieuNhap_GetByID", 
                    new { ID = id }, 
                    commandType: CommandType.StoredProcedure);
            }
        }

        public List<PhieuNhapKhoChiTietViewModel> GetChiTiet(int idPhieuNhap)
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = @"
                    SELECT 
                        c.ID, c.IDPhieuNhap, c.IDSanPham, 
                        s.MaSanPham, s.TenSanPham, s.DVT,
                        c.STT, c.SoLuong, c.DonGia, c.ThanhTien, 
                        c.ThueGTGT, c.TienThue, c.TongSauThue,
                        ISNULL(c.DonGiaVanChuyen, 0) AS DonGiaVanChuyen,
                        ISNULL(c.TienVanChuyen, 0) AS TienVanChuyen,
                        c.GhiChu,
                        c.NgaySanXuat, c.HanSuDung
                    FROM KHO_PhieuNhap_ChiTiet c
                    LEFT JOIN DM_SanPham s ON c.IDSanPham = s.ID
                    WHERE c.IDPhieuNhap = @IDPhieuNhap
                    ORDER BY c.STT";
                return conn.Query<PhieuNhapKhoChiTietViewModel>(sql, new { IDPhieuNhap = idPhieuNhap }).ToList();
            }
        }

        public int Save(PhieuNhapKhoViewModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                NormalizeVanChuyen(model);
                var chiTietJson = JsonConvert.SerializeObject(model.ChiTiets);
                chiTietJson = System.Text.RegularExpressions.Regex.Replace(chiTietJson, @"\.0+(?=[,\}])", "");

                var p = new DynamicParameters();
                p.Add("@ID", model.ID);
                p.Add("@NgayNhap", model.NgayNhap);
                p.Add("@IDKho", model.IDKho);
                p.Add("@IDNhaCungCap", model.IDNhaCungCap);
                p.Add("@SoHoaDon", model.SoHoaDon);
                p.Add("@NgayHoaDon", model.NgayHoaDon);
                p.Add("@TenNguoiGiao", model.TenNguoiGiao);
                p.Add("@SoDienThoaiNguoiGiao", model.SoDienThoaiNguoiGiao);
                p.Add("@TenNguoiNhan", model.TenNguoiNhan);
                p.Add("@GhiChu", model.GhiChu);
                p.Add("@NguoiTao", userId);
                p.Add("@IDPhuongTien", model.IDPhuongTien);
                p.Add("@NgayGiaoHang", model.NgayGiaoHang);
                p.Add("@HoTenTaiXe", model.HoTenTaiXe);
                p.Add("@SoDienThoaiTaiXe", model.SoDienThoaiTaiXe);
                
                // Các tham số mới
                p.Add("@IDLoaiNhapKho", model.IDLoaiNhapKho);
                p.Add("@IDKhoNguon", model.IDKhoNguon);
                p.Add("@IDKhachHang", model.IDKhachHang);
                
                p.Add("@ChiTietJson", chiTietJson);
                p.Add("@NewID", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@SoChungTuOut", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

                conn.Execute("sp_KHO_PhieuNhap_Save", p, commandType: CommandType.StoredProcedure);

                model.SoChungTu = p.Get<string>("@SoChungTuOut");
                int newId = p.Get<int>("@NewID");

                // Tính toán lại và cập nhật TongTienHang, TongTienThue, TongCong vào KHO_PhieuNhap dựa trên KHO_PhieuNhap_ChiTiet
                int activeId = model.ID > 0 ? model.ID : newId;

                // Cập nhật TrangThai
                conn.Execute("UPDATE KHO_PhieuNhap SET TrangThai = @TrangThai WHERE ID = @ID", new { TrangThai = model.TrangThai, ID = activeId });

                // Cập nhật NgaySanXuat, HanSuDung cho từng chi tiết
                if (model.ChiTiets != null && model.ChiTiets.Count > 0)
                {
                    foreach (var ct in model.ChiTiets)
                    {
                        string updateDateSql = @"
                            UPDATE [dbo].[KHO_PhieuNhap_ChiTiet] 
                            SET NgaySanXuat = @NgaySanXuat,
                                HanSuDung = @HanSuDung,
                                DonGiaVanChuyen = @DonGiaVanChuyen,
                                TienVanChuyen = @TienVanChuyen
                            WHERE IDPhieuNhap = @IDPhieuNhap AND IDSanPham = @IDSanPham
                        ";
                        conn.Execute(updateDateSql, new { 
                            NgaySanXuat = ct.NgaySanXuat, 
                            HanSuDung = ct.HanSuDung, 
                            DonGiaVanChuyen = ct.DonGiaVanChuyen,
                            TienVanChuyen = ct.TienVanChuyen,
                            IDPhieuNhap = activeId, 
                            IDSanPham = ct.IDSanPham 
                        });
                    }
                }

                string updateTotalsSql = @"
                    UPDATE [dbo].[KHO_PhieuNhap]
                    SET TongTienHang = ISNULL((SELECT SUM(ThanhTien) FROM [dbo].[KHO_PhieuNhap_ChiTiet] WHERE IDPhieuNhap = @ID), 0),
                        TongTienThue = ISNULL((SELECT SUM(TienThue) FROM [dbo].[KHO_PhieuNhap_ChiTiet] WHERE IDPhieuNhap = @ID), 0),
                        TongCong = ISNULL((SELECT SUM(TongSauThue) FROM [dbo].[KHO_PhieuNhap_ChiTiet] WHERE IDPhieuNhap = @ID), 0),
                        TienVanChuyen = ISNULL((SELECT SUM(TienVanChuyen) FROM [dbo].[KHO_PhieuNhap_ChiTiet] WHERE IDPhieuNhap = @ID), 0)
                    WHERE ID = @ID;
                ";
                conn.Execute(updateTotalsSql, new { ID = activeId });

                SyncGiaoDichKho(conn, activeId, model.SoChungTu, model.TrangThai, userId);

                return newId;
            }
        }

        private void SyncGiaoDichKho(System.Data.IDbConnection conn, int id, string soChungTu, int trangThai, int userId)
        {
            if (trangThai == 1 || trangThai == 2)
            {
                conn.Execute("DELETE FROM KHO_GiaoDichKho WHERE LoaiChungTu = 1 AND SoChungTu = @SoChungTu", new { SoChungTu = soChungTu });
                string sqlGiaoDich = @"
                    INSERT INTO KHO_GiaoDichKho (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao)
                    SELECT 
                        p.NgayNhap,
                        p.SoChungTu,
                        1,
                        ct.ID,
                        p.IDKho,
                        ct.IDSanPham,
                        ct.SoLuong,
                        0,
                        ct.DonGia,
                        ct.ThanhTien,
                        p.GhiChu,
                        GETDATE(),
                        @UserId
                    FROM KHO_PhieuNhap_ChiTiet ct
                    INNER JOIN KHO_PhieuNhap p ON ct.IDPhieuNhap = p.ID
                    WHERE p.ID = @ID;
                ";
                conn.Execute(sqlGiaoDich, new { ID = id, UserId = userId });
            }
            else
            {
                conn.Execute("DELETE FROM KHO_GiaoDichKho WHERE LoaiChungTu = 1 AND SoChungTu = @SoChungTu", new { SoChungTu = soChungTu });
            }
        }

        public void UpdateMaster(PhieuNhapKhoViewModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = @"
                    UPDATE [dbo].[KHO_PhieuNhap]
                    SET NgayNhap = @NgayNhap,
                        IDKho = @IDKho,
                        IDNhaCungCap = @IDNhaCungCap,
                        SoHoaDon = @SoHoaDon,
                        NgayHoaDon = @NgayHoaDon,
                        TenNguoiGiao = @TenNguoiGiao,
                        SoDienThoaiNguoiGiao = @SoDienThoaiNguoiGiao,
                        TenNguoiNhan = @TenNguoiNhan,
                        GhiChu = @GhiChu,
                        NguoiCapNhat = @NguoiTao,
                        NgayCapNhat = GETDATE(),
                        IDPhuongTien = @IDPhuongTien,
                        NgayGiaoHang = @NgayGiaoHang,
                        HoTenTaiXe = @HoTenTaiXe,
                        SoDienThoaiTaiXe = @SoDienThoaiTaiXe,
                        IDLoaiNhapKho = @IDLoaiNhapKho,
                        IDKhoNguon = @IDKhoNguon,
                        IDKhachHang = @IDKhachHang
                    WHERE ID = @ID AND TrangThai = 1; -- Chỉ cho phép sửa khi Nháp
                ";
                
                var p = new DynamicParameters();
                p.Add("@ID", model.ID);
                p.Add("@NgayNhap", model.NgayNhap);
                p.Add("@IDKho", model.IDKho);
                p.Add("@IDNhaCungCap", model.IDNhaCungCap);
                p.Add("@SoHoaDon", model.SoHoaDon);
                p.Add("@NgayHoaDon", model.NgayHoaDon);
                p.Add("@TenNguoiGiao", model.TenNguoiGiao);
                p.Add("@SoDienThoaiNguoiGiao", model.SoDienThoaiNguoiGiao);
                p.Add("@TenNguoiNhan", model.TenNguoiNhan);
                p.Add("@GhiChu", model.GhiChu);
                p.Add("@NguoiTao", userId);
                p.Add("@IDPhuongTien", model.IDPhuongTien);
                p.Add("@NgayGiaoHang", model.NgayGiaoHang);
                p.Add("@HoTenTaiXe", model.HoTenTaiXe);
                p.Add("@SoDienThoaiTaiXe", model.SoDienThoaiTaiXe);
                p.Add("@IDLoaiNhapKho", model.IDLoaiNhapKho);
                p.Add("@IDKhoNguon", model.IDKhoNguon);
                p.Add("@IDKhachHang", model.IDKhachHang);
                conn.Execute(sql, p);
                
                var trangThai = conn.ExecuteScalar<int>("SELECT TrangThai FROM KHO_PhieuNhap WHERE ID = @ID", new { ID = model.ID });
                SyncGiaoDichKho(conn, model.ID, model.SoChungTu, trangThai, userId);
            }
        }

        public void GhiSo(int id, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                try {
                    string sqlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "update_sp_KHO_PhieuNhap_GhiSo.sql");
                    if (System.IO.File.Exists(sqlPath)) {
                        string sql = System.IO.File.ReadAllText(sqlPath);
                        var parts = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            if (!string.IsNullOrWhiteSpace(part)) conn.Execute(part);
                        }
                        System.IO.File.Delete(sqlPath);
                    }
                } catch { }

                // 1. Ghi sổ phiếu nhập
                conn.Execute(
                    "sp_KHO_PhieuNhap_GhiSo", 
                    new { ID = id, NguoiGhiSo = userId }, 
                    commandType: CommandType.StoredProcedure);
                
                // 2. Tự động cấn trừ Tiền trả trước (nếu có)
                // Lấy thông tin phiếu nhập
                var phieuNhap = conn.QueryFirstOrDefault("SELECT IDNhaCungCap, TongCong, DaThanhToan, ConLai FROM KHO_PhieuNhap WHERE ID = @ID", new { ID = id });
                if (phieuNhap != null && phieuNhap.IDNhaCungCap != null && phieuNhap.ConLai > 0)
                {
                    // Lấy danh sách phiếu chi còn dư tiền của NCC này
                    string sqlTienDu = @"
                        SELECT pc.ID, pc.SoTienChi, 
                               ISNULL((SELECT SUM(SoTienPhanBo) FROM KT_PhieuChiChiTiet WHERE IDPhieuChi = pc.ID), 0) AS DaPhanBo
                        FROM KT_PhieuChi pc
                        WHERE pc.IDNhaCungCap = @IDNcc AND pc.TrangThai = 2
                    ";
                    var listPc = conn.Query(sqlTienDu, new { IDNcc = phieuNhap.IDNhaCungCap }).ToList();
                    
                    decimal conLaiPhieuNhap = phieuNhap.ConLai;
                    
                    foreach(var pc in listPc)
                    {
                        if (conLaiPhieuNhap <= 0) break;
                        
                        decimal soTienChi = pc.SoTienChi ?? 0m;
                        decimal daPhanBo = pc.DaPhanBo;
                        decimal tienDu = soTienChi - daPhanBo;
                        
                        if (tienDu > 0)
                        {
                            decimal soTienCanTru = Math.Min(tienDu, conLaiPhieuNhap);
                            
                            // Thêm chi tiết phân bổ
                            string insertCt = @"
                                INSERT INTO KT_PhieuChiChiTiet (IDPhieuChi, IDPhieuNhap, LoaiChi, SoTienPhanBo, DienGiai)
                                VALUES (@IDPhieuChi, @IDPhieuNhap, 1, @SoTienPhanBo, N'Tự động cấn trừ tiền trả trước')
                            ";
                            conn.Execute(insertCt, new { 
                                IDPhieuChi = pc.ID,
                                IDPhieuNhap = id,
                                SoTienPhanBo = soTienCanTru
                            });
                            
                            // Cập nhật lại phiếu nhập
                            conn.Execute(@"
                                UPDATE KHO_PhieuNhap 
                                SET DaThanhToan = ISNULL(DaThanhToan, 0) + @SoTien, 
                                    ConLai = TongCong - (ISNULL(DaThanhToan, 0) + @SoTien)
                                WHERE ID = @ID
                            ", new { SoTien = soTienCanTru, ID = id });
                            
                            conLaiPhieuNhap -= soTienCanTru;
                        }
                    }
                }
            }
        }

        public void BoGhiSo(int id, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                try {
                    string sqlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "update_sp_KHO_PhieuNhap_BoGhiSo.sql");
                    if (System.IO.File.Exists(sqlPath)) {
                        string sql = System.IO.File.ReadAllText(sqlPath);
                        var parts = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            if (!string.IsNullOrWhiteSpace(part)) conn.Execute(part);
                        }
                        System.IO.File.Delete(sqlPath);
                    }
                } catch { }

                conn.Execute(
                    "sp_KHO_PhieuNhap_BoGhiSo", 
                    new { ID = id, NguoiBoGhi = userId }, 
                    commandType: CommandType.StoredProcedure);
            }
        }

        public void HuyPhieu(int id, string lyDoHuy, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(
                    "sp_KHO_PhieuNhap_Huy", 
                    new { ID = id, LyDoHuy = lyDoHuy, NguoiHuy = userId }, 
                    commandType: CommandType.StoredProcedure);
            }
        }

        public void Delete(int id, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(
                    "sp_KHO_PhieuNhap_Delete", 
                    new { ID = id, NguoiXoa = userId }, 
                    commandType: CommandType.StoredProcedure);
            }
        }

        public string GenerateSoChungTu()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<string>(
                    "sp_KHO_PhieuNhap_GenerateSoChungTu", 
                    commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<dynamic> GetKhoForDropdown(string keyword)
        {
            using (var conn = _db.CreateConnection())
            {
                string kw = (keyword ?? "").Trim().ToLower();
                return conn.Query("SELECT ID, MaKhoHang, TenKhoHang FROM DM_KhoHang WHERE @KW = '' OR LOWER(TenKhoHang) LIKE '%' + @KW + '%' ORDER BY TenKhoHang", new { KW = kw });
            }
        }

        public IEnumerable<dynamic> GetNhaCungCapForDropdown(string keyword)
        {
            using (var conn = _db.CreateConnection())
            {
                string kw = (keyword ?? "").Trim().ToLower();
                return conn.Query("SELECT ID, MaNhaCungCap, TenNhaCungCap FROM DM_NhaCungCap WHERE @KW = '' OR LOWER(TenNhaCungCap) LIKE '%' + @KW + '%' ORDER BY TenNhaCungCap", new { KW = kw });
            }
        }

        public IEnumerable<dynamic> GetNhanSuForDropdown(string keyword)
        {
            using (var conn = _db.CreateConnection())
            {
                string kw = (keyword ?? "").Trim().ToLower();
                return conn.Query("SELECT ID, MaNhanSu, LTRIM(RTRIM(ISNULL(HoDem, '') + ' ' + ISNULL(Ten, ''))) AS HoTen FROM NS_NhanSu WHERE @KW = '' OR LOWER(Ten) LIKE '%' + @KW + '%' ORDER BY Ten", new { KW = kw });
            }
        }

        public IEnumerable<dynamic> GetSanPhamForDropdown(string keyword)
        {
            using (var conn = _db.CreateConnection())
            {
                string kw = (keyword ?? "").Trim().ToLower();
                return conn.Query("SELECT ID, MaSanPham, TenSanPham, DVT FROM DM_SanPham WHERE @KW = '' OR LOWER(TenSanPham) LIKE '%' + @KW + '%' ORDER BY TenSanPham", new { KW = kw });
            }
        }

        public IEnumerable<dynamic> GetPhuongTienForDropdown(string keyword)
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = @"
                    SELECT TOP 20 ID, MaPhuongTien, TenPhuongTien
                    FROM DM_PhuongTien 
                    WHERE  (MaPhuongTien LIKE '%' + @Keyword + '%' OR TenPhuongTien LIKE N'%' + @Keyword + '%')
                    ORDER BY STT, TenPhuongTien";
                return conn.Query(sql, new { Keyword = keyword ?? "" });
            }
        }

        public IEnumerable<dynamic> GetLoaiNhapKhoForDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query("sp_DM_LoaiNhapKho_GetDropdown", commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<dynamic> GetKhachHangForDropdown(string keyword)
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = @"
                    SELECT TOP 20 ID, MaKhachHang, TenKhachHang
                    FROM NS_KhachHang 
                    WHERE  (MaKhachHang LIKE '%' + @Keyword + '%' OR TenKhachHang LIKE N'%' + @Keyword + '%')
                    ORDER BY TenKhachHang";
                return conn.Query(sql, new { Keyword = keyword ?? "" });
            }
        }

        public IEnumerable<dynamic> CheckTonKhoChuyenKho(int idKhoNguon, string chiTietsJson)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDKhoNguon", idKhoNguon);
                p.Add("@ChiTietsJson", chiTietsJson);

                return conn.Query("sp_KHO_TonKho_CheckChuyenKho", p, commandType: CommandType.StoredProcedure);
            }
        }

        private void NormalizeVanChuyen(PhieuNhapKhoViewModel model)
        {
            if (model?.ChiTiets == null)
            {
                if (model != null) model.TienVanChuyen = 0;
                return;
            }

            foreach (var ct in model.ChiTiets)
            {
                if (ct.DonGiaVanChuyen < 0) ct.DonGiaVanChuyen = 0;
                ct.TienVanChuyen = ct.DonGiaVanChuyen * ct.SoLuong;
            }

            model.TienVanChuyen = model.ChiTiets.Sum(x => x.TienVanChuyen);
        }
    }
}
