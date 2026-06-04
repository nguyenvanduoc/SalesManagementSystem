using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Models.ViewModels
{
    /// <summary>ViewModel cho trang tạo/chỉnh sửa đơn hàng.</summary>
    public class OrderFormVM
    {
        public Order Order { get; set; } = new Order();
        public List<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    }
}
