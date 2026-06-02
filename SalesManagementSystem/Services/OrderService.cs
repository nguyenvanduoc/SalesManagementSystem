using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories;

namespace SalesManagementSystem.Services
{
    /// <summary>
    /// Service nghiệp vụ bán hàng: validate tồn kho, tạo đơn, tính báo cáo doanh thu.
    /// </summary>
    public class OrderService
    {
        private readonly OrderRepository _orderRepo;
        private readonly DbConnectionFactory _db;

        public OrderService(OrderRepository orderRepo, DbConnectionFactory db)
        {
            _orderRepo = orderRepo;
            _db = db;
        }

        /// <summary>
        /// Kiểm tra tồn kho trước, nếu hợp lệ mới tạo đơn.
        /// Throw Exception nếu sản phẩm nào không đủ hàng.
        /// </summary>
        public int PlaceOrder(Order order, List<OrderDetail> details, int userId)
        {
            // Validate tồn kho
            foreach (var d in details)
            {
                const string checkStock = "SELECT ISNULL(Quantity, 0) FROM Inventory WHERE ProductId = @Id";
                int available;
                using (var conn = _db.CreateConnection())
                    available = conn.ExecuteScalar<int>(checkStock, new { Id = d.ProductId });

                if (available < d.Quantity)
                    throw new InvalidOperationException(
                        $"Sản phẩm ID {d.ProductId} chỉ còn {available} trong kho, yêu cầu {d.Quantity}.");
            }

            // Tính TotalAmount nếu chưa có
            order.TotalAmount = details.Sum(d => d.Quantity * d.UnitPrice);
            order.OrderDate = DateTime.Now;
            order.Status = "Completed";

            return _orderRepo.CreateOrder(order, details, userId);
        }

        /// <summary>Tạo báo cáo doanh thu theo khoảng ngày.</summary>
        public RevenueReportVM GetRevenueReport(DateTime from, DateTime to)
        {
            var orders = _orderRepo.GetByDateRange(from, to).ToList();

            // Tính tổng giá vốn từ OrderDetails JOIN Products
            const string costSql = @"
                SELECT ISNULL(SUM(od.Quantity * p.CostPrice), 0)
                FROM OrderDetails od
                INNER JOIN Products p ON p.Id = od.ProductId
                INNER JOIN Orders o ON o.Id = od.OrderId
                WHERE o.OrderDate BETWEEN @From AND @To
                  AND o.Status = 'Completed'";
            decimal totalCost;
            using (var conn = _db.CreateConnection())
                totalCost = conn.ExecuteScalar<decimal>(costSql, new { From = from, To = to });

            return new RevenueReportVM
            {
                FromDate = from,
                ToDate = to,
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                TotalCost = totalCost,
                TotalOrders = orders.Count,
                Orders = orders
            };
        }
    }
}
