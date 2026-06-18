using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.Entities
{
    /// <summary>Status: "Pending" | "Completed" | "Cancelled"</summary>
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }

        // Bind sau JOIN với Users
        public string UserFullName { get; set; }

        // Danh sách chi tiết đơn hàng (populate thủ công sau khi query)
        public List<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    }
}
