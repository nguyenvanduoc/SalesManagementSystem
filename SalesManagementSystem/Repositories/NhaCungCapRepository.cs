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
    public class NhaCungCapRepository : INhaCungCapRepository
    {
        private readonly DbConnectionFactory _db;

        public NhaCungCapRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<NhaCungCapViewModel> GetPaged(
            int page, int pageSize,
            string ma, string ten, string dt, string email,
            out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@MaNhaCungCap", string.IsNullOrWhiteSpace(ma) ? null : ma.Trim());
                p.Add("@TenNhaCungCap", string.IsNullOrWhiteSpace(ten) ? null : ten.Trim());
                p.Add("@DienThoai", string.IsNullOrWhiteSpace(dt) ? null : dt.Trim());
                p.Add("@Email", string.IsNullOrWhiteSpace(email) ? null : email.Trim());
                p.Add("@Offset", (page - 1) * pageSize);
                p.Add("@PageSize", pageSize);
                p.Add("@TotalRecords", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var list = conn.Query<NhaCungCapViewModel>(
                    "sp_DM_NhaCungCap_GetList",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                totalRecords = p.Get<int>("@TotalRecords");
                return list;
            }
        }

        public DM_NhaCungCap GetById(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                var row = conn.QueryFirstOrDefault<dynamic>(
                    "sp_DM_NhaCungCap_GetByID",
                    new { ID = id },
                    commandType: CommandType.StoredProcedure
                );

                if (row == null) return null;

                return new DM_NhaCungCap
                {
                    ID = row.ID,
                    MaNhaCungCap = row.MaNhaCungCap,
                    TenNhaCungCap = row.TenNhaCungCap,
                    DiaChi = row.DiaChi,
                    SoDienThoai = row.DienThoai,
                    Email = row.Email,
                    NgayTao = row.NgayTao,
                    NguoiTao = row.NguoiTao
                };
            }
        }

        public int Save(DM_NhaCungCap ncc)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", ncc.ID);
                p.Add("@MaNhaCungCap", ncc.MaNhaCungCap?.Trim());
                p.Add("@TenNhaCungCap", ncc.TenNhaCungCap?.Trim());
                p.Add("@DienThoai", ncc.SoDienThoai?.Trim());
                p.Add("@Email", ncc.Email?.Trim());
                p.Add("@DiaChi", ncc.DiaChi?.Trim());
                p.Add("@NguoiTao", ncc.NguoiTao);

                return conn.ExecuteScalar<int>(
                    "sp_DM_NhaCungCap_Save",
                    p,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public bool Delete(int id, out string message)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@ID", id);
                p.Add("@Success", dbType: DbType.Boolean, direction: ParameterDirection.Output);
                p.Add("@Message", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

                conn.Execute(
                    "sp_DM_NhaCungCap_Delete",
                    p,
                    commandType: CommandType.StoredProcedure
                );

                bool success = p.Get<bool>("@Success");
                message = p.Get<string>("@Message");
                return success;
            }
        }

        public bool CheckDuplicate(string code, int excludeId = 0)
        {
            using (var conn = _db.CreateConnection())
            {
                var count = conn.ExecuteScalar<int>(
                    "sp_DM_NhaCungCap_CheckDuplicate",
                    new { MaNhaCungCap = code?.Trim(), ExcludeID = excludeId },
                    commandType: CommandType.StoredProcedure
                );
                return count > 0;
            }
        }

        public IEnumerable<dynamic> GetForDropdown(string keyword)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query(
                    "sp_DM_NhaCungCap_GetForDropdown",
                    new { Keyword = keyword },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }
    }
}
