namespace SalesManagementSystem.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string Unit { get; set; }

        // Bind sau JOIN với Categories — không phải EF navigation property
        public string CategoryName { get; set; }
    }
}
