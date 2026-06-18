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
    public class PhieuThuKhachHangRepository : IPhieuThuKhachHangRepository
    {
        private readonly DbConnectionFactory _db;

        public PhieuThuKhachHangRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<PhieuThuKhachHangListViewModel> GetList(
            string tuNgay, 
            string denNgay, 
            string soChungTu, 
            int? idKhachHang, 
            int? trangThaiCongNo
        )
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay));
                p.Add("@SoChungTu", string.IsNullOrEmpty(soChungTu) ? null : soChungTu);
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@TrangThaiCongNo", trangThaiCongNo);

                return conn.Query<PhieuThuKhachHangListViewModel>(
                    "sp_BAN_PhieuThuKhachHang_GetList", 
                    p, 
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public PhieuThuKhachHangViewModel GetByID(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<PhieuThuKhachHangViewModel>(
                    "sp_BAN_PhieuThuKhachHang_GetByID", 
                    new { ID = id }, 
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public int Save(PhieuThuKhachHangViewModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", model.ID == 0 ? (int?)null : model.ID);
                p.Add("@SoPhieuThu", model.SoPhieuThu);
                p.Add("@NgayThu", model.NgayThu);
                p.Add("@IDChungTuBanHang", model.IDChungTuBanHang);
                p.Add("@IDKhachHang", model.IDKhachHang);
                p.Add("@IDTaiKhoanThanhToan", model.IDTaiKhoanThanhToan);
                p.Add("@IDNguoiThu", model.IDNguoiThu);
                p.Add("@SoTienThu", model.SoTienThu);
                p.Add("@GhiChu", model.GhiChu);
                p.Add("@TrangThai", model.TrangThai);
                p.Add("@UserId", userId);
                p.Add("@NewID", dbType: DbType.Int32, direction: ParameterDirection.Output);

                conn.Execute("sp_BAN_PhieuThuKhachHang_Save", p, commandType: CommandType.StoredProcedure);
                return p.Get<int>("@NewID");
            }
        }

        public void GhiSo(int id, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(
                    "sp_BAN_PhieuThuKhachHang_Ghi", 
                    new { ID = id, UserId = userId }, 
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public void Huy(int id, int userId, string lyDo)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(
                    "sp_BAN_PhieuThuKhachHang_Huy", 
                    new { ID = id, UserId = userId, LyDoHuy = lyDo }, 
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public void Delete(int id, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(
                    "sp_BAN_PhieuThuKhachHang_Delete", 
                    new { ID = id, UserId = userId }, 
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public string GenerateSoPhieuThu()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.ExecuteScalar<string>(
                    "sp_BAN_PhieuThuKhachHang_GenerateSoPhieuThu", 
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public IEnumerable<dynamic> GetChungTuBanHangDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "sp_BAN_ChungTuBanHang_GetCongNoForDropdown", 
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public dynamic GetCongNoChungTuByID(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<dynamic>(
                    "sp_BAN_ChungTuBanHang_GetCongNoByID", 
                    new { ID = id }, 
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public IEnumerable<dynamic> GetTaiKhoanThanhToanDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "sp_DM_TaiKhoanThanhToan_GetForDropdown", 
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetNhanSuDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "sp_NS_NhanSu_GetForDropdown", 
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetHistoryByChungTuID(int idChungTuBanHang)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "sp_BAN_PhieuThuKhachHang_GetHistoryByChungTuID",
                    new { IDChungTuBanHang = idChungTuBanHang },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public decimal GetCreditInfo(int idKhachHang)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.ExecuteScalar<decimal>(
                    "sp_BAN_PhieuThuKhachHang_GetCreditInfo",
                    new { IDKhachHang = idKhachHang },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public IEnumerable<dynamic> GetRecentActivities(int idChungTuBanHang)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "sp_BAN_PhieuThuKhachHang_GetRecentActivities",
                    new { IDChungTuBanHang = idChungTuBanHang },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }
    }
}
