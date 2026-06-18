using System;

namespace SalesManagementSystem.Models.Entities
{
    public class KHO_GiaoDichKho
    {
        public int ID { get; set; }
        public DateTime NgayChungTu { get; set; }
        public string SoChungTu { get; set; }
        public int LoaiChungTu { get; set; } // 1: Phiếu nhập, 3: Hủy phiếu nhập
        public int IDChiTietKho { get; set; }
        public int IDKho { get; set; }
        public int IDSanPham { get; set; }
        public decimal SoLuongNhap { get; set; }
        public decimal SoLuongXuat { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string DienGiai { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
    }
}
