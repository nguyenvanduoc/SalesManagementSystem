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
            int? trangThai)
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

        public IEnumerable<dynamic> GetPhieuNhapDropdown(int? idNhaCungCap)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = idNhaCungCap.HasValue
                    ? "SELECT ID, SoChungTu AS TenHienThi FROM KHO_PhieuNhap WHERE IsDeleted = 0 AND TrangThai = 2 AND ISNULL(TrangThaiThanhToan, 0) < 2 AND IDNhaCungCap = @IDNhaCungCap ORDER BY NgayNhap DESC"
                    : "SELECT ID, SoChungTu AS TenHienThi FROM KHO_PhieuNhap WHERE IsDeleted = 0 AND TrangThai = 2 AND ISNULL(TrangThaiThanhToan, 0) < 2 ORDER BY NgayNhap DESC";
                return conn.Query<dynamic>(sql, new { IDNhaCungCap = idNhaCungCap }).ToList();
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
    }
}
