using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class TaiKhoanThanhToanRepository : ITaiKhoanThanhToanRepository
    {
        private readonly DbConnectionFactory _db;

        public TaiKhoanThanhToanRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<TaiKhoanThanhToanListViewModel> GetList(int page, int pageSize, string keyword, int? isHoatDong, out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@Page", page);
                p.Add("@PageSize", pageSize);
                p.Add("@Keyword", string.IsNullOrEmpty(keyword) ? null : keyword);
                p.Add("@IsHoatDong", isHoatDong);
                p.Add("@TotalRecords", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var list = conn.Query<TaiKhoanThanhToanListViewModel>(
                    "sp_DM_TaiKhoanThanhToan_GetList", 
                    p, 
                    commandType: CommandType.StoredProcedure
                ).ToList();

                totalRecords = p.Get<int>("@TotalRecords");
                return list;
            }
        }

        public TaiKhoanThanhToanViewModel GetByID(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<TaiKhoanThanhToanViewModel>(
                    "sp_DM_TaiKhoanThanhToan_GetByID", 
                    new { ID = id }, 
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public int Save(TaiKhoanThanhToanViewModel model, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", model.ID);
                p.Add("@MaTaiKhoan", model.MaTaiKhoan);
                p.Add("@TenTaiKhoan", model.TenTaiKhoan);
                p.Add("@NganHang", model.NganHang);
                p.Add("@SoTaiKhoan", model.SoTaiKhoan);
                p.Add("@ChuTaiKhoan", model.ChuTaiKhoan);
                p.Add("@IsHoatDong", model.IsHoatDong);
                p.Add("@IDTaiKhoanKeToan", model.IDTaiKhoanKeToan);
                p.Add("@UserId", userId);
                p.Add("@NewID", dbType: DbType.Int32, direction: ParameterDirection.Output);

                conn.Execute("sp_DM_TaiKhoanThanhToan_Save", p, commandType: CommandType.StoredProcedure);
                return p.Get<int>("@NewID");
            }
        }

        public void Delete(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute("sp_DM_TaiKhoanThanhToan_Delete", new { ID = id }, commandType: CommandType.StoredProcedure);
            }
        }

        public bool IsDuplicateCode(string code, int currentId = 0)
        {
            using (var conn = _db.CreateConnection())
            {
                var result = conn.ExecuteScalar<int>(
                    "sp_DM_TaiKhoanThanhToan_CheckDuplicateCode",
                    new { MaTaiKhoan = code, CurrentID = currentId },
                    commandType: CommandType.StoredProcedure
                );
                return result > 0;
            }
        }
    }
}
