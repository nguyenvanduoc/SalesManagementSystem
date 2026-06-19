using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuChiDashboardViewModel
    {
        public decimal TongChi { get; set; }
        public string TongChiText { get; set; }
        public string TongChiTrend { get; set; }
        public string TongChiTrendClass { get; set; }

        public decimal QuyTienMat { get; set; }
        public string QuyTienMatText { get; set; }
        public string QuyTienMatStatus { get; set; }

        public decimal DuNganHang { get; set; }
        public string DuNganHangText { get; set; }
        public int DuNganHangCount { get; set; }

        public decimal CongNoNcc { get; set; }
        public string CongNoNccText { get; set; }
        public string CongNoNccLabel { get; set; }
    }
}
