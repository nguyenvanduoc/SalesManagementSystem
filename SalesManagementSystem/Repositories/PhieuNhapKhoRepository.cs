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
            int? trangThai, int? idNhanSuNhan,
            out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrWhiteSpace(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrWhiteSpace(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay).AddDays(1).AddSeconds(-1));
                p.Add("@SoChungTu", string.IsNullOrWhiteSpace(soChungTu) ? null : soChungTu.Trim());
                p.Add("@IDKho", idKho);
                p.Add("@IDNhaCungCap", idNhaCungCap);
                p.Add("@TrangThai", trangThai);
                p.Add("@IDNhanSuNhan", idNhanSuNhan);
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
                return conn.Query<PhieuNhapKhoChiTietViewModel>(
                    "sp_KHO_PhieuNhap_GetChiTiet", 
                    new { IDPhieuNhap = idPhieuNhap }, 
                    commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public int Save(PhieuNhapKhoViewModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
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
                p.Add("@IDNhanSuNhan", model.IDNhanSuNhan);
                p.Add("@GhiChu", model.GhiChu);
                p.Add("@NguoiTao", userId);
                p.Add("@ChiTietJson", chiTietJson);
                p.Add("@NewID", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@SoChungTuOut", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

                conn.Execute("sp_KHO_PhieuNhap_Save", p, commandType: CommandType.StoredProcedure);

                model.SoChungTu = p.Get<string>("@SoChungTuOut");
                int newId = p.Get<int>("@NewID");

                // Tính toán lại và cập nhật TongTienHang, TongTienThue, TongCong vào KHO_PhieuNhap dựa trên KHO_PhieuNhap_ChiTiet
                int activeId = model.ID > 0 ? model.ID : newId;
                string updateTotalsSql = @"
                    UPDATE [dbo].[KHO_PhieuNhap]
                    SET TongTienHang = ISNULL((SELECT SUM(ThanhTien) FROM [dbo].[KHO_PhieuNhap_ChiTiet] WHERE IDPhieuNhap = @ID), 0),
                        TongTienThue = ISNULL((SELECT SUM(TienThue) FROM [dbo].[KHO_PhieuNhap_ChiTiet] WHERE IDPhieuNhap = @ID), 0),
                        TongCong = ISNULL((SELECT SUM(TongSauThue) FROM [dbo].[KHO_PhieuNhap_ChiTiet] WHERE IDPhieuNhap = @ID), 0)
                    WHERE ID = @ID;
                ";
                conn.Execute(updateTotalsSql, new { ID = activeId });

                return newId;
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
                        IDNhanSuNhan = @IDNhanSuNhan,
                        GhiChu = @GhiChu,
                        NguoiCapNhat = @NguoiTao,
                        NgayCapNhat = GETDATE()
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
                p.Add("@IDNhanSuNhan", model.IDNhanSuNhan);
                p.Add("@GhiChu", model.GhiChu);
                p.Add("@NguoiTao", userId);

                conn.Execute(sql, p);
            }
        }

        public void GhiSo(int id, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(
                    "sp_KHO_PhieuNhap_GhiSo", 
                    new { ID = id, NguoiGhiSo = userId }, 
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
                return conn.Query("sp_DM_KhoHang_GetForDropdown", new { Keyword = keyword }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<dynamic> GetNhaCungCapForDropdown(string keyword)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query("sp_DM_NhaCungCap_GetForDropdown", new { Keyword = keyword }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<dynamic> GetNhanSuForDropdown(string keyword)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query("sp_NS_NhanSu_GetForDropdown", new { Keyword = keyword }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<dynamic> GetSanPhamForDropdown(string keyword)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query("sp_DM_SanPham_GetForDropdown", new { Keyword = keyword }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
