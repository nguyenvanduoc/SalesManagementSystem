using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Repositories
{
    /// <summary>
    /// Nơi DUY NHẤT chứa SQL liên quan đến Orders, OrderDetails và InventoryTransactions.
    /// </summary>
    public class OrderRepository
    {
        private readonly DbConnectionFactory _db;

        public OrderRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<Order> GetAll()
        {
            const string sql = @"
                SELECT o.*, u.FullName AS UserFullName
                FROM Orders o
                INNER JOIN Users u ON u.Id = o.UserId
                ORDER BY o.OrderDate DESC";
            using (var conn = _db.CreateConnection())
                return conn.Query<Order>(sql);
        }

        public Order GetById(int id)
        {
            const string sql = @"
                SELECT o.*, u.FullName AS UserFullName
                FROM Orders o
                INNER JOIN Users u ON u.Id = o.UserId
                WHERE o.Id = @Id";
            using (var conn = _db.CreateConnection())
                return conn.QueryFirstOrDefault<Order>(sql, new { Id = id });
        }

        public IEnumerable<OrderDetail> GetDetailsByOrderId(int orderId)
        {
            const string sql = @"
                SELECT od.*, p.Name AS ProductName, p.Sku
                FROM OrderDetails od
                INNER JOIN Products p ON p.Id = od.ProductId
                WHERE od.OrderId = @OrderId";
            using (var conn = _db.CreateConnection())
                return conn.Query<OrderDetail>(sql, new { OrderId = orderId });
        }

        /// <summary>
        /// Tạo đơn hàng hoàn chỉnh trong một transaction:
        /// Insert Order → Insert OrderDetails → Giảm tồn kho → Ghi InventoryTransaction.
        /// </summary>
        public int CreateOrder(Order order, IEnumerable<OrderDetail> details, int userId)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Insert Order header
                        const string insertOrder = @"
                            INSERT INTO Orders (OrderDate, TotalAmount, UserId, Status)
                            VALUES (@OrderDate, @TotalAmount, @UserId, @Status);
                            SELECT CAST(SCOPE_IDENTITY() AS INT)";
                        int orderId = conn.ExecuteScalar<int>(insertOrder, order, tx);

                        foreach (var d in details)
                        {
                            d.OrderId = orderId;
                            d.SubTotal = d.Quantity * d.UnitPrice;

                            // 2. Insert từng OrderDetail
                            const string insertDetail = @"
                                INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice, SubTotal)
                                VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @SubTotal)";
                            conn.Execute(insertDetail, d, tx);

                            // 3. Giảm tồn kho
                            const string decreaseInventory = @"
                                UPDATE Inventory
                                SET Quantity = Quantity - @Qty, LastUpdated = GETDATE()
                                WHERE ProductId = @ProductId";
                            conn.Execute(decreaseInventory,
                                new { Qty = d.Quantity, ProductId = d.ProductId }, tx);

                            // 4. Ghi lịch sử xuất kho
                            const string insertTx = @"
                                INSERT INTO InventoryTransactions (ProductId, TransactionType, Quantity, Date, UserId)
                                VALUES (@ProductId, 'OUT', @Quantity, GETDATE(), @UserId)";
                            conn.Execute(insertTx,
                                new { d.ProductId, d.Quantity, UserId = userId }, tx);
                        }

                        tx.Commit();
                        return orderId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public IEnumerable<Order> GetByDateRange(DateTime from, DateTime to)
        {
            const string sql = @"
                SELECT o.*, u.FullName AS UserFullName
                FROM Orders o
                INNER JOIN Users u ON u.Id = o.UserId
                WHERE o.OrderDate BETWEEN @From AND @To
                ORDER BY o.OrderDate DESC";
            using (var conn = _db.CreateConnection())
                return conn.Query<Order>(sql, new { From = from, To = to });
        }

        public void UpdateStatus(int orderId, string status)
        {
            const string sql = "UPDATE Orders SET Status = @Status WHERE Id = @Id";
            using (var conn = _db.CreateConnection())
                conn.Execute(sql, new { Status = status, Id = orderId });
        }
    }
}
