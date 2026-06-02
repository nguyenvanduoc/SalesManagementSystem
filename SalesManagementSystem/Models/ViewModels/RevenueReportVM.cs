using System;
using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Models.ViewModels
{
    /// <summary>ViewModel cho trang báo cáo doanh thu tổng hợp.</summary>
    public class RevenueReportVM
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit => TotalRevenue - TotalCost;
        public int TotalOrders { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
