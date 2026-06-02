using System;

namespace SalesManagementSystem.Models.Entities
{
    /// <summary>Lịch sử nhập/xuất kho. TransactionType: "IN" | "OUT"</summary>
    public class InventoryTransaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string TransactionType { get; set; } // "IN" hoặc "OUT"
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }

        // Bind sau JOIN
        public string ProductName { get; set; }
        public string UserFullName { get; set; }
    }
}
