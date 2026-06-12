using System.Collections.Generic;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class TaiKhoanKeToanRepository : ITaiKhoanKeToanRepository
    {
        private readonly DbConnectionFactory _db;

        public TaiKhoanKeToanRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<KT_TaiKhoanKeToan> GetActive()
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.Query<KT_TaiKhoanKeToan>("sp_KT_TaiKhoanKeToan_GetActive", commandType: System.Data.CommandType.StoredProcedure);
            }
        }
    }
}
