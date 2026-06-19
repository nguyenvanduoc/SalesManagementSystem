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
    public class CongNoNCCRepository : ICongNoNCCRepository
    {
        private readonly DbConnectionFactory _db;

        public CongNoNCCRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<CongNoNCCViewModel> GetList(
            string tuNgay,
            string denNgay,
            int? idNhaCungCap,
            int? trangThaiCongNo)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay",        string.IsNullOrEmpty(tuNgay)  ? (DateTime?)null : DateTime.Parse(tuNgay));
                p.Add("@DenNgay",       string.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.Parse(denNgay));
                p.Add("@IDNhaCungCap",  idNhaCungCap);
                p.Add("@TrangThaiCongNo", trangThaiCongNo);

                return conn.Query<CongNoNCCViewModel>(
                    "sp_CongNo_PhaseTra_NCC_GetList",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }
    }
}
