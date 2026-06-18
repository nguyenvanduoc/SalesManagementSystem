namespace SalesManagementSystem.Models.Entities
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }

        // Bind sau JOIN với Products
        public string ProductName { get; set; }
        public string Sku { get; set; }
    }
}
