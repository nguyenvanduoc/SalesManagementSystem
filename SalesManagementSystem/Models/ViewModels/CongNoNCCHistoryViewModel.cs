using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class CongNoNCCHistoryViewModel
    {
        public int IDPhieuChi { get; set; }
        public string SoPhieuChi { get; set; }
        public DateTime NgayChi { get; set; }
        public decimal SoTienChi { get; set; }
        public string DienGiai { get; set; }
        public int TrangThai { get; set; }
    }
}
