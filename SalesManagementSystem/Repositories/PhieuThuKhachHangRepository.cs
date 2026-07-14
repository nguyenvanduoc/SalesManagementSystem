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
            string soPhieuThu, 
            int? idKhachHang, 
            int? trangThai,
            string nguoiNopTien,
            int? idTaiKhoanThanhToan
        )
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay",        string.IsNullOrEmpty(tuNgay)      ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay",       string.IsNullOrEmpty(denNgay)     ? (DateTime?)null : DateTime.Parse(denNgay));
                p.Add("@SoPhieuThu",    string.IsNullOrEmpty(soPhieuThu)  ? null : soPhieuThu);
                p.Add("@IDKhachHang",   idKhachHang);
                p.Add("@TrangThai",     trangThai);
                p.Add("@NguoiNopTien",  string.IsNullOrEmpty(nguoiNopTien) ? null : nguoiNopTien);
                p.Add("@IDTaiKhoanThanhToan", idTaiKhoanThanhToan);

                var list = conn.Query<PhieuThuKhachHangListViewModel>(
                    "sp_KT_PhieuThu_GetList",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return list;
            }
        }

        public dynamic GetDashboardData(
            string tuNgay, string denNgay, string soPhieuThu, 
            int? idKhachHang, int? trangThai, string nguoiNopTien, int? idTaiKhoanThanhToan)
        {
            // Similar logic to PhieuChi Dashboard, simplified.
            dynamic dashboard = new System.Dynamic.ExpandoObject();
            dashboard.TongThu = 0;
            dashboard.TongThuText = "0 d";
            dashboard.TongThuTrend = "0%";
            dashboard.TongThuTrendClass = "stable";
            dashboard.CongNoKhachHang = 0;
            dashboard.CongNoKhachHangText = "0 d";
            
            return dashboard;
        }

        public PhieuThuKhachHangViewModel GetByID(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                using (var multi = conn.QueryMultiple("sp_KT_PhieuThu_GetById", new { ID = id }, commandType: CommandType.StoredProcedure))
                {
                    var model = multi.ReadFirstOrDefault<PhieuThuKhachHangViewModel>();
                    if (model != null)
                    {
                        model.ChiTiets = multi.Read<PhieuThuKhachHangChiTietViewModel>().ToList();
                        if (model.TrangThai == 2)
                        {
                            foreach (var ct in model.ChiTiets)
                            {
                                if (ct.LoaiThu == 1)
                                {
                                    ct.DaThanhToan -= ct.SoTienPhanBo;
                                    ct.ConLai += ct.SoTienPhanBo;
                                }
                            }
                        }
                        if (model.IDKhachHang.HasValue)
                        {
                            var ptTienTraTruoc = GetTienTraTruocKhachHang(model.IDKhachHang.Value);
                            if (model.TrangThai == 2)
                            {
                                var excessCreated = model.ChiTiets.Where(x => x.LoaiThu == 2).Sum(x => x.SoTienPhanBo);
                                var prepaymentUsed = model.ChiTiets.Where(x => x.LoaiThu == 3).Sum(x => x.SoTienPhanBo);
                                model.TienTraTruocKhachHang = ptTienTraTruoc - excessCreated + prepaymentUsed;
                            }
                            else
                            {
                                model.TienTraTruocKhachHang = ptTienTraTruoc;
                            }
                        }
                    }
                    return model;
                }
            }
        }

        public int Save(PhieuThuKhachHangViewModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@SoPhieuThu",            model.SoPhieuThu);
                p.Add("@NgayThu",               model.NgayThu);
                p.Add("@IDTaiKhoanThanhToan",   model.IDTaiKhoanThanhToan);
                p.Add("@IDKhachHang",           model.IDKhachHang);
                p.Add("@NguoiNopTien",          model.NguoiNopTien);
                p.Add("@SoDienThoaiNguoiNop",   model.SoDienThoaiNguoiNop);
                p.Add("@DienGiai",              model.DienGiai);
                p.Add("@SoTienThu",             model.SoTienThu);
                
                int newId = 0;
                if (model.ID == 0) 
                {
                    p.Add("@NguoiTao", userId);
                    p.Add("@NewID", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    conn.Execute("sp_KT_PhieuThu_Insert", p, commandType: CommandType.StoredProcedure);
                    newId = p.Get<int>("@NewID");
                }
                else 
                {
                    p.Add("@ID", model.ID);
                    p.Add("@NguoiCapNhat", userId);
                    conn.Execute("sp_KT_PhieuThu_Update", p, commandType: CommandType.StoredProcedure);
                    newId = model.ID;
                }
                
                // Save details
                if (model.ChiTiets != null && model.ChiTiets.Any())
                {
                    conn.Execute("sp_KT_PhieuThuChiTiet_DeleteByPhieuThu", new { IDPhieuThu = newId }, commandType: CommandType.StoredProcedure);
                    foreach (var c in model.ChiTiets)
                    {
                        conn.Execute("sp_KT_PhieuThuChiTiet_Insert", new {
                            IDPhieuThu = newId,
                            IDChungTuBanHang = c.IDChungTu,
                            LoaiThu = c.LoaiThu,
                            SoTienPhanBo = c.SoTienPhanBo,
                            DienGiai = c.DienGiai
                        }, commandType: CommandType.StoredProcedure);
                    }
                }
                
                return newId;
            }
        }

        public void DieuChinhPhanBo(PhieuThuKhachHangViewModel model, List<PhieuThuKhachHangChiTietViewModel> newChiTiets, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", model.ID);
                p.Add("@SoPhieuThu", model.SoPhieuThu);
                p.Add("@NgayThu", model.NgayThu);
                p.Add("@IDTaiKhoanThanhToan", model.IDTaiKhoanThanhToan);
                p.Add("@IDKhachHang", model.IDKhachHang);
                p.Add("@NguoiNopTien", model.NguoiNopTien);
                p.Add("@SoDienThoaiNguoiNop", model.SoDienThoaiNguoiNop);
                p.Add("@DienGiai", model.DienGiai);
                p.Add("@SoTienThu", model.SoTienThu);
                p.Add("@NguoiCapNhat", userId);
                conn.Execute("sp_KT_PhieuThu_Update", p, commandType: CommandType.StoredProcedure);

                conn.Execute("sp_KT_PhieuThuChiTiet_DeleteByPhieuThu", new { IDPhieuThu = model.ID }, commandType: CommandType.StoredProcedure);
                foreach (var c in newChiTiets)
                {
                    conn.Execute("sp_KT_PhieuThuChiTiet_Insert", new {
                        IDPhieuThu = model.ID,
                        IDChungTuBanHang = c.IDChungTu,
                        LoaiThu = c.LoaiThu,
                        SoTienPhanBo = c.SoTienPhanBo,
                        DienGiai = c.DienGiai
                    }, commandType: CommandType.StoredProcedure);
                }
            }
        }

        public void GhiSo(int id, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(
                    "sp_KT_PhieuThu_GhiSo",
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
                    "sp_KT_PhieuThu_Huy",
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
                    "sp_KT_PhieuThu_Delete",
                    new { ID = id },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public string GenerateSoPhieuThu()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.ExecuteScalar<string>(
                    "SELECT 'PT' + RIGHT('000000' + CAST(ISNULL(MAX(ID),0)+1 AS VARCHAR(10)), 6) FROM KT_PhieuThu"
                );
            }
        }

        public IEnumerable<dynamic> GetChungTuBanHangDropdown(int? idKhachHang = null)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = idKhachHang.HasValue
                    ? "SELECT ID, SoChungTu AS TenHienThi FROM BAN_ChungTuBanHang WHERE TrangThai = 2 AND IDKhachHang = @IDKhachHang ORDER BY NgayChungTu DESC"
                    : "SELECT ID, SoChungTu AS TenHienThi FROM BAN_ChungTuBanHang WHERE TrangThai = 2 ORDER BY NgayChungTu DESC";
                return conn.Query<dynamic>(sql, new { IDKhachHang = idKhachHang }).ToList();
            }
        }

        public IEnumerable<dynamic> GetKhachHangDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "SELECT ID, TenKhachHang AS TenHienThi FROM NS_KhachHang ORDER BY TenKhachHang"
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetTaiKhoanThanhToanDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "SELECT ID, ISNULL(TenTaiKhoan, '') + CASE WHEN SoTaiKhoan IS NOT NULL THEN ' - ' + SoTaiKhoan ELSE '' END AS TenHienThi FROM DM_TaiKhoanThanhToan WHERE IsHoatDong = 1 ORDER BY TenTaiKhoan"
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetNhanSuDropdown()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "SELECT ID, HoDem + ' ' + Ten AS TenHienThi FROM NS_NhanSu ORDER BY Ten"
                ).ToList();
            }
        }

        public IEnumerable<dynamic> GetChungTuBanHangCongNo(int idKhachHang)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<dynamic>(
                    "sp_KT_PhieuThu_LoadCongNoKhachHang", 
                    new { IDKhachHang = idKhachHang },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public decimal GetTienTraTruocKhachHang(int idKhachHang)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<decimal>(
                    "sp_KT_PhieuThu_GetTienTraTruocKhachHang",
                    new { IDKhachHang = idKhachHang },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        private void EnsureFileTable(IDbConnection conn)
        {
            // Already created in SQL script
        }

        public IEnumerable<PhieuThuKhachHangFile> File_GetList(int idPhieuThu)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<PhieuThuKhachHangFile>(@"
                    SELECT f.ID,
                           f.IDPhieuThu,
                           f.TenFile,
                           f.LoaiFile,
                           f.DungLuong,
                           f.NgayTao,
                           f.NguoiTao,
                           ISNULL(LTRIM(RTRIM(ISNULL(ns.HoDem,'') + ' ' + ISNULL(ns.Ten,''))), CAST(f.NguoiTao AS NVARCHAR(50))) AS TenNguoiTao
                    FROM KT_PhieuThuFile f
                    LEFT JOIN NS_NhanSu ns ON f.NguoiTao = ns.ID
                    WHERE f.IDPhieuThu = @IDPhieuThu
                    ORDER BY f.NgayTao DESC, f.ID DESC",
                    new { IDPhieuThu = idPhieuThu }).ToList();
            }
        }

        public PhieuThuKhachHangFile File_GetByID(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<PhieuThuKhachHangFile>(@"
                    SELECT f.ID,
                           f.IDPhieuThu,
                           f.TenFile,
                           f.LoaiFile,
                           f.DungLuong,
                           f.NoiDungFile,
                           f.NgayTao,
                           f.NguoiTao
                    FROM KT_PhieuThuFile f
                    WHERE f.ID = @ID",
                    new { ID = id });
            }
        }

        public void File_Save(PhieuThuKhachHangFile model, int nguoiThaoTac)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(@"
                    INSERT INTO KT_PhieuThuFile
                        (IDPhieuThu, TenFile, LoaiFile, DungLuong, NoiDungFile, NgayTao, NguoiTao)
                    VALUES
                        (@IDPhieuThu, @TenFile, @LoaiFile, @DungLuong, @NoiDungFile, GETDATE(), @NguoiThaoTac)",
                    new
                    {
                        model.IDPhieuThu,
                        model.TenFile,
                        model.LoaiFile,
                        model.DungLuong,
                        model.NoiDungFile,
                        NguoiThaoTac = nguoiThaoTac
                    });
            }
        }

        public void File_Delete(int id, int nguoiThaoTac)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute(@"DELETE FROM KT_PhieuThuFile WHERE ID = @ID", new { ID = id });
            }
        }
    }
}
