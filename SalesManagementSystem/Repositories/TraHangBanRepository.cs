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
    public class TraHangBanRepository : ITraHangBanRepository
    {
        private readonly DbConnectionFactory _db;

        public TraHangBanRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<TraHangBanViewModel> GetPaged(int page, int pageSize, string tuNgay, string denNgay, int? idKhachHang, int? trangThai, string soChungTu, out int totalRecords)
        {
            DateTime? tn = null;
            if (!string.IsNullOrEmpty(tuNgay))
            {
                if (DateTime.TryParseExact(tuNgay, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var d1)) tn = d1;
                else if (DateTime.TryParseExact(tuNgay, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d2)) tn = d2;
                else tn = DateTime.Parse(tuNgay);
            }

            DateTime? dn = null;
            if (!string.IsNullOrEmpty(denNgay))
            {
                if (DateTime.TryParseExact(denNgay, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var d1)) dn = d1;
                else if (DateTime.TryParseExact(denNgay, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d2)) dn = d2;
                else dn = DateTime.Parse(denNgay);
            }

            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", tn);
                p.Add("@DenNgay", dn);
                p.Add("@SoChungTu", soChungTu);
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@TrangThai", trangThai);
                p.Add("@Page", page);
                p.Add("@PageSize", pageSize);
                p.Add("@TotalRecords", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var list = conn.Query<TraHangBanViewModel>("sp_BAN_TraHangBan_GetList", p, commandType: CommandType.StoredProcedure).ToList();
                totalRecords = p.Get<int>("@TotalRecords");
                return list;
            }
        }

        public TraHangBanViewModel GetById(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<TraHangBanViewModel>("sp_BAN_TraHangBan_GetById", new { ID = id }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<TraHangBanChiTietViewModel> GetChiTietByTraHangId(int id)
        {
            // Note: In sp_BAN_TraHangBan_GetById we could return multiple result sets, 
            // but Dapper's QueryMultiple is usually better for that. Here we can just call it again, 
            // or write a dedicated SP. Actually, sp_BAN_TraHangBan_GetById returns 2 sets. Let's use QueryMultiple.
            using (var conn = _db.CreateConnection())
            {
                using (var multi = conn.QueryMultiple("sp_BAN_TraHangBan_GetById", new { ID = id }, commandType: CommandType.StoredProcedure))
                {
                    var header = multi.Read<TraHangBanViewModel>().FirstOrDefault();
                    if (header != null)
                    {
                        var details = multi.Read<TraHangBanChiTietViewModel>().ToList();
                        return details;
                    }
                    return new List<TraHangBanChiTietViewModel>();
                }
            }
        }
        
        public string GenerateSoChungTu()
        {
            using (var conn = _db.CreateConnection())
            {
                string sql = @"
                    DECLARE @YearSuffix VARCHAR(2) = RIGHT(CAST(YEAR(GETDATE()) AS VARCHAR(4)), 2);
                    DECLARE @Prefix VARCHAR(4) = 'TH' + @YearSuffix;
                    DECLARE @MaxSo VARCHAR(20) = (
                        SELECT TOP 1 SoChungTu 
                        FROM BAN_TraHangBan 
                        WHERE SoChungTu LIKE @Prefix + '%' 
                        ORDER BY SoChungTu DESC
                    );
                    DECLARE @NextNum INT = 1;
                    IF @MaxSo IS NOT NULL
                    BEGIN
                        SET @NextNum = CAST(SUBSTRING(@MaxSo, 5, 6) AS INT) + 1;
                    END
                    SELECT @Prefix + RIGHT('000000' + CAST(@NextNum AS VARCHAR(6)), 6);";
                return conn.ExecuteScalar<string>(sql);
            }
        }

        public int Insert(TraHangBan traHang, List<TraHangBanChiTiet> chiTiets)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        var p = new DynamicParameters();
                        p.Add("@SoChungTu", traHang.SoChungTu);
                        p.Add("@NgayChungTu", traHang.NgayChungTu);
                        p.Add("@IDDonDatHang", traHang.IDDonDatHang);
                        p.Add("@IDKhachHang", traHang.IDKhachHang);
                        p.Add("@IDKho", traHang.IDKho);
                        p.Add("@LyDoTraHang", traHang.LyDoTraHang);
                        p.Add("@TongSoLuong", traHang.TongSoLuong);
                        p.Add("@TongTienHang", traHang.TongTienHang);
                        p.Add("@TongTienDaHoan", traHang.TongTienDaHoan);
                        p.Add("@ConPhaiHoan", traHang.ConPhaiHoan);
                        p.Add("@TrangThai", traHang.TrangThai);
                        p.Add("@NguoiTao", traHang.NguoiTao);
                        p.Add("@PhiBocXep", traHang.PhiBocXep);
                        p.Add("@NewID", dbType: DbType.Int32, direction: ParameterDirection.Output);

                        conn.Execute("sp_BAN_TraHangBan_Insert", p, transaction: tr, commandType: CommandType.StoredProcedure);
                        
                        int newId = p.Get<int>("@NewID");

                        if (chiTiets != null && chiTiets.Any())
                        {
                            foreach (var item in chiTiets)
                            {
                                item.IDTraHang = newId;
                                item.NguoiTao = traHang.NguoiTao;
                                conn.Execute("sp_BAN_TraHangBanChiTiet_Insert", new {
                                    item.IDTraHang,
                                    item.IDSanPham,
                                    item.SoLuongBan,
                                    item.SoLuongDaTra,
                                    item.SoLuongConLai,
                                    item.SoLuongTra,
                                    item.DonGia,
                                    item.ThanhTien,
                                    item.GhiChu,
                                    item.NguoiTao
                                }, transaction: tr, commandType: CommandType.StoredProcedure);
                            }
                        }

                        tr.Commit();
                        return newId;
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Update(TraHangBan traHang, List<TraHangBanChiTiet> chiTiets)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    try
                    {
                        int currentStatus = conn.ExecuteScalar<int>("SELECT TrangThai FROM BAN_TraHangBan WHERE ID = @ID", new { ID = traHang.ID }, transaction: tr);

                        if (currentStatus == 2)
                        {
                            // Revert old inventory
                            string reverseInvSql = @"
                                DECLARE @SoChungTu NVARCHAR(50), @NgayChungTu DATETIME, @IDKho INT, @LyDo NVARCHAR(500);
                                SELECT @SoChungTu = SoChungTu, @NgayChungTu = GETDATE(), @IDKho = IDKho, @LyDo = N'Sửa ' + LyDoTraHang 
                                FROM BAN_TraHangBan WHERE ID = @ID;

                                INSERT INTO KHO_GiaoDichKho (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao)
                                SELECT @NgayChungTu, @SoChungTu, 6, 0, @IDKho, c.IDSanPham, 0, c.SoLuongTra, c.DonGia, c.ThanhTien, @LyDo, GETDATE(), @NguoiTao
                                FROM BAN_TraHangBanChiTiet c
                                WHERE c.IDTraHang = @ID;";
                            conn.Execute(reverseInvSql, new { ID = traHang.ID, NguoiTao = traHang.NguoiCapNhat }, transaction: tr);
                        }

                        conn.Execute("sp_BAN_TraHangBan_Update", new {
                            traHang.ID,
                            traHang.NgayChungTu,
                            traHang.IDDonDatHang,
                            traHang.IDKhachHang,
                            traHang.IDKho,
                            traHang.LyDoTraHang,
                            traHang.TongSoLuong,
                            traHang.TongTienHang,
                            traHang.TongTienDaHoan,
                            traHang.ConPhaiHoan,
                            traHang.NguoiCapNhat,
                            traHang.PhiBocXep
                        }, transaction: tr, commandType: CommandType.StoredProcedure);
                        
                        conn.Execute("sp_BAN_TraHangBanChiTiet_DeleteByTraHangId", new { IDTraHang = traHang.ID }, transaction: tr, commandType: CommandType.StoredProcedure);

                        if (chiTiets != null && chiTiets.Any())
                        {
                            foreach (var item in chiTiets)
                            {
                                item.IDTraHang = traHang.ID;
                                item.NguoiTao = traHang.NguoiCapNhat;
                                conn.Execute("sp_BAN_TraHangBanChiTiet_Insert", new {
                                    item.IDTraHang,
                                    item.IDSanPham,
                                    item.SoLuongBan,
                                    item.SoLuongDaTra,
                                    item.SoLuongConLai,
                                    item.SoLuongTra,
                                    item.DonGia,
                                    item.ThanhTien,
                                    item.GhiChu,
                                    item.NguoiTao
                                }, transaction: tr, commandType: CommandType.StoredProcedure);
                            }
                        }

                        if (currentStatus == 2)
                        {
                            // Add new inventory
                            string addInvSql = @"
                                DECLARE @SoChungTu NVARCHAR(50), @NgayChungTu DATETIME, @IDKho INT, @LyDo NVARCHAR(500);
                                SELECT @SoChungTu = SoChungTu, @NgayChungTu = NgayChungTu, @IDKho = IDKho, @LyDo = LyDoTraHang 
                                FROM BAN_TraHangBan WHERE ID = @ID;

                                INSERT INTO KHO_GiaoDichKho (NgayChungTu, SoChungTu, LoaiChungTu, IDChiTietKho, IDKho, IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DienGiai, NgayTao, NguoiTao)
                                SELECT @NgayChungTu, @SoChungTu, 5, 0, @IDKho, c.IDSanPham, c.SoLuongTra, 0, c.DonGia, c.ThanhTien, @LyDo, GETDATE(), @NguoiTao
                                FROM BAN_TraHangBanChiTiet c
                                WHERE c.IDTraHang = @ID;";
                            conn.Execute(addInvSql, new { ID = traHang.ID, NguoiTao = traHang.NguoiCapNhat }, transaction: tr);
                        }

                        tr.Commit();
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute("sp_BAN_TraHangBan_Delete", new { ID = id }, commandType: CommandType.StoredProcedure);
            }
        }

        public void GhiSo(int id, int nguoiThucHien)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute("sp_BAN_TraHangBan_GhiSo", new { ID = id, NguoiThucHien = nguoiThucHien }, commandType: CommandType.StoredProcedure);
            }
        }

        public void Huy(int id, int nguoiThucHien)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute("sp_BAN_TraHangBan_Huy", new { ID = id, NguoiThucHien = nguoiThucHien }, commandType: CommandType.StoredProcedure);
            }
        }
        
        public IEnumerable<TraHangBanViewModel> LoadDonHangTra(string tuNgay, string denNgay, string soDonHang, int page, int pageSize, out int totalRecords)
        {
            DateTime? tn = null;
            if (!string.IsNullOrEmpty(tuNgay))
            {
                if (DateTime.TryParseExact(tuNgay, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var d1)) tn = d1;
                else if (DateTime.TryParseExact(tuNgay, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d2)) tn = d2;
                else tn = DateTime.Parse(tuNgay);
            }

            DateTime? dn = null;
            if (!string.IsNullOrEmpty(denNgay))
            {
                if (DateTime.TryParseExact(denNgay, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var d1)) dn = d1;
                else if (DateTime.TryParseExact(denNgay, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d2)) dn = d2;
                else dn = DateTime.Parse(denNgay);
            }

            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", tn);
                p.Add("@DenNgay", dn);
                p.Add("@SoDonHang", soDonHang);
                p.Add("@Page", page);
                p.Add("@PageSize", pageSize);
                p.Add("@TotalRecords", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var list = conn.Query<TraHangBanViewModel>("sp_BAN_TraHangBan_LoadDonHangTra", p, commandType: CommandType.StoredProcedure).ToList();
                totalRecords = p.Get<int>("@TotalRecords");
                return list;
            }
        }
        
        public IEnumerable<TraHangBanChiTietViewModel> LoadChiTietDonHang(int idDonDatHang)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<TraHangBanChiTietViewModel>("sp_BAN_TraHangBan_LoadChiTietDonHang", new { IDDonDatHang = idDonDatHang }, commandType: CommandType.StoredProcedure).ToList();
            }
        }
    }
}
