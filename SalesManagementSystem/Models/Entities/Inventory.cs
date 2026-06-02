using System;

namespace SalesManagementSystem.Models.Entities
{
    public class Inventory
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }

        // Bind sau JOIN với Products
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public string Unit { get; set; }
    }
}
