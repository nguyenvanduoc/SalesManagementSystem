using System.Collections.Generic;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Services
{
    /// <summary>
    /// Service xử lý nghiệp vụ kho: nhập hàng, kiểm tra tồn kho, lịch sử giao dịch.
    /// Gọi Dapper trực tiếp qua DbConnectionFactory (không qua Repository riêng vì đây là logic nghiệp vụ).
    /// </summary>
    public class InventoryService
    {
        private readonly DbConnectionFactory _db;

        public InventoryService(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<Inventory> GetStockList()
        {
            const string sql = @"
                SELECT i.*, p.Name AS ProductName, p.Sku, p.Unit
                FROM Inventory i
                INNER JOIN Products p ON p.Id = i.ProductId
                ORDER BY p.Name";
            using (var conn = _db.CreateConnection())
                return conn.Query<Inventory>(sql);
        }

        public int GetStockQuantity(int productId)
        {
            const string sql = "SELECT ISNULL(Quantity, 0) FROM Inventory WHERE ProductId = @Id";
            using (var conn = _db.CreateConnection())
                return conn.ExecuteScalar<int>(sql, new { Id = productId });
        }

        /// <summary>Nhập hàng vào kho: tăng Inventory + ghi lịch sử IN.</summary>
        public void StockIn(int productId, int quantity, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // Upsert tồn kho
                        const string upsert = @"
                            IF EXISTS (SELECT 1 FROM Inventory WHERE ProductId = @ProductId)
                                UPDATE Inventory
                                SET Quantity = Quantity + @Quantity, LastUpdated = GETDATE()
                                WHERE ProductId = @ProductId
                            ELSE
                                INSERT INTO Inventory (ProductId, Quantity, LastUpdated)
                                VALUES (@ProductId, @Quantity, GETDATE())";
                        conn.Execute(upsert, new { ProductId = productId, Quantity = quantity }, tx);

                        // Ghi lịch sử nhập kho
                        const string logTx = @"
                            INSERT INTO InventoryTransactions (ProductId, TransactionType, Quantity, Date, UserId)
                            VALUES (@ProductId, 'IN', @Quantity, GETDATE(), @UserId)";
                        conn.Execute(logTx, new { ProductId = productId, Quantity = quantity, UserId = userId }, tx);

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public IEnumerable<InventoryTransaction> GetTransactionHistory(int? productId = null)
        {
            var sql = @"
                SELECT t.*, p.Name AS ProductName, u.FullName AS UserFullName
                FROM InventoryTransactions t
                INNER JOIN Products p ON p.Id = t.ProductId
                INNER JOIN Users u ON u.Id = t.UserId";

            if (productId.HasValue)
                sql += " WHERE t.ProductId = @ProductId";

            sql += " ORDER BY t.Date DESC";

            using (var conn = _db.CreateConnection())
                return conn.Query<InventoryTransaction>(sql, new { ProductId = productId });
        }
    }
}
