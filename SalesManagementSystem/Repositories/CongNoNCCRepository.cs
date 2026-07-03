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
                try {
                    string sqlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "update_sp_CongNo_PhaseTra_NCC_GetList.sql");
                    if (System.IO.File.Exists(sqlPath)) {
                        string sql = System.IO.File.ReadAllText(sqlPath);
                        var parts = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach(var part in parts) {
                            if (!string.IsNullOrWhiteSpace(part)) {
                                conn.Execute(part);
                            }
                        }
                        System.IO.File.Delete(sqlPath);
                    }
                } catch { }

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
