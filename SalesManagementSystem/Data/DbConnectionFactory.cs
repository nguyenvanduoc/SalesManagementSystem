using System.Configuration;
using System.Data.SqlClient;

namespace SalesManagementSystem.Data
{
    /// <summary>
    /// Factory duy nhất chịu trách nhiệm tạo SqlConnection từ Web.config.
    /// Inject trực tiếp concrete class này — không dùng interface.
    /// </summary>
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory()
        {
            _connectionString = SalesManagementSystem.Helpers.Security.ConfigManager.GetConnectionString("DefaultConnection");
        }

        /// <summary>Tạo và trả về SqlConnection mới (chưa mở).</summary>
        public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
