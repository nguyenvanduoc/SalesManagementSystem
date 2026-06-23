using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class HopDongDashboardCount
    {
        public int TongHopDong { get; set; }
        public int DangHieuLuc { get; set; }
        public int SapHetHan { get; set; }
        public int DaThanhLy { get; set; }
    }

    public class HopDongKhachHangRepository : IHopDongKhachHangRepository
    {
        private readonly DbConnectionFactory _db;

        public HopDongKhachHangRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<HopDongKhachHang> GetList(
            System.DateTime? tuNgay, 
            System.DateTime? denNgay, 
            string soHopDong, 
            string tenHopDong, 
            int? idKhachHang, 
            int? trangThai, 
            bool chiHienThiSapHetHan, 
            int pageNumber, 
            int pageSize,
            out int totalRecords,
            out int tongHopDong,
            out int dangHieuLuc,
            out int sapHetHan,
            out int daThanhLy)
        {
            totalRecords = 0;
            tongHopDong = 0;
            dangHieuLuc = 0;
            sapHetHan = 0;
            daThanhLy = 0;

            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", tuNgay);
                p.Add("@DenNgay", denNgay);
                p.Add("@SoHopDong", soHopDong);
                p.Add("@TenHopDong", tenHopDong);
                p.Add("@IDKhachHang", idKhachHang);
                p.Add("@TrangThai", trangThai);
                p.Add("@ChiHienThiSapHetHan", chiHienThiSapHetHan);
                p.Add("@PageNumber", pageNumber);
                p.Add("@PageSize", pageSize);

                using (var multi = conn.QueryMultiple("sp_BAN_HopDongKhachHang_GetList", p, commandType: CommandType.StoredProcedure))
                {
                    var items = multi.Read<HopDongKhachHang>().ToList();
                    
                    if (items.Any())
                    {
                        totalRecords = items.First().TotalRecords;
                    }

                    var dashboardCounts = multi.ReadFirstOrDefault<HopDongDashboardCount>();
                    if (dashboardCounts != null)
                    {
                        tongHopDong = dashboardCounts.TongHopDong;
                        dangHieuLuc = dashboardCounts.DangHieuLuc;
                        sapHetHan = dashboardCounts.SapHetHan;
                        daThanhLy = dashboardCounts.DaThanhLy;
                    }

                    return items;
                }
            }
        }

        public HopDongKhachHang GetByID(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                return conn.QueryFirstOrDefault<HopDongKhachHang>("sp_BAN_HopDongKhachHang_GetByID", p, commandType: CommandType.StoredProcedure);
            }
        }

        public bool CheckDuplicate(int id, string soHopDong)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@SoHopDong", soHopDong);
                
                var isDup = conn.ExecuteScalar<int>("sp_BAN_HopDongKhachHang_CheckDuplicate", p, commandType: CommandType.StoredProcedure);
                return isDup == 1;
            }
        }

        public int Save(HopDongKhachHang model, int nguoiThaoTac)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", model.ID, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                p.Add("@SoHopDong", model.SoHopDong);
                p.Add("@TenHopDong", model.TenHopDong);
                p.Add("@IDKhachHang", model.IDKhachHang);
                p.Add("@NgayKy", model.NgayKy);
                p.Add("@TuNgay", model.TuNgay);
                p.Add("@DenNgay", model.DenNgay);
                p.Add("@GiaTriHopDong", model.GiaTriHopDong);
                p.Add("@NguoiDaiDien", model.NguoiDaiDien);
                p.Add("@SoDienThoai", model.SoDienThoai);
                p.Add("@Email", model.Email);
                p.Add("@NoiDung", model.NoiDung);
                p.Add("@GhiChu", model.GhiChu);
                p.Add("@NguoiThaoTac", nguoiThaoTac);

                conn.Execute("sp_BAN_HopDongKhachHang_Save", p, commandType: CommandType.StoredProcedure);
                return p.Get<int>("@ID");
            }
        }

        public void Delete(int id, int nguoiThaoTac)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@NguoiThaoTac", nguoiThaoTac);
                conn.Execute("sp_BAN_HopDongKhachHang_Delete", p, commandType: CommandType.StoredProcedure);
            }
        }

        public void ThanhLy(int id, int nguoiThaoTac)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@NguoiThaoTac", nguoiThaoTac);
                conn.Execute("sp_BAN_HopDongKhachHang_ThanhLy", p, commandType: CommandType.StoredProcedure);
            }
        }

        public void Huy(int id, int nguoiThaoTac)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@NguoiThaoTac", nguoiThaoTac);
                conn.Execute("sp_BAN_HopDongKhachHang_Huy", p, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<HopDongKhachHangFile> File_GetList(int idHopDong)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDHopDong", idHopDong);
                return conn.Query<HopDongKhachHangFile>("sp_BAN_HopDongKhachHang_File_GetList", p, commandType: CommandType.StoredProcedure);
            }
        }

        public HopDongKhachHangFile File_GetByID(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                return conn.QueryFirstOrDefault<HopDongKhachHangFile>("sp_BAN_HopDongKhachHang_File_GetByID", p, commandType: CommandType.StoredProcedure);
            }
        }

        public void File_Save(HopDongKhachHangFile model, int nguoiThaoTac)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@IDHopDong", model.IDHopDong);
                p.Add("@TenFile", model.TenFile);
                p.Add("@LoaiFile", model.LoaiFile);
                p.Add("@DungLuong", model.DungLuong);
                p.Add("@NoiDungFile", model.NoiDungFile);
                p.Add("@GhiChu", model.GhiChu);
                p.Add("@NguoiThaoTac", nguoiThaoTac);

                conn.Execute("sp_BAN_HopDongKhachHang_File_Save", p, commandType: CommandType.StoredProcedure);
            }
        }

        public void File_Delete(int id, int nguoiThaoTac)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@NguoiThaoTac", nguoiThaoTac);
                conn.Execute("sp_BAN_HopDongKhachHang_File_Delete", p, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
