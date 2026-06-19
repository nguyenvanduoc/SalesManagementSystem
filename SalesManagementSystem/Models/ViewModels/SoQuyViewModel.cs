using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class SoQuyViewModel
    {
        public DateTime NgayChungTu { get; set; }
        public string SoChungTu { get; set; }
        public string LoaiChungTu { get; set; }   // "THU" hoặc "CHI"
        public int IDTaiKhoanThanhToan { get; set; }
        public string TenTaiKhoanThanhToan { get; set; }
        public string DienGiai { get; set; }
        public decimal SoTienThu { get; set; }
        public decimal SoTienChi { get; set; }
        public int TrangThai { get; set; }
    }
}
