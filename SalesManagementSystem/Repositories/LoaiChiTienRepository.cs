using System.Collections.Generic;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class LoaiChiTienRepository : ILoaiChiTienRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public LoaiChiTienRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<LoaiChiTienViewModel> GetAllActive()
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                return db.Query<LoaiChiTienViewModel>("sp_DM_LoaiChiTien_GetAllActive", commandType: System.Data.CommandType.StoredProcedure);
            }
        }
    }
}
