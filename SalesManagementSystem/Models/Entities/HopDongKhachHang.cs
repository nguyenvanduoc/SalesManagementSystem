using System;

namespace SalesManagementSystem.Models.Entities
{
    public class HopDongKhachHang
    {
        public int ID { get; set; }
        public string SoHopDong { get; set; }
        public string TenHopDong { get; set; }
        public int IDKhachHang { get; set; }
        public DateTime? NgayKy { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public decimal GiaTriHopDong { get; set; }
        public string NguoiDaiDien { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string NoiDung { get; set; }
        public string GhiChu { get; set; }
        public int TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation properties for JOIN results
        public string TenKhachHang { get; set; }
        public string TenNguoiTao { get; set; }
        public int? SoNgayConLai { get; set; }
        public int TotalRecords { get; set; }
    }
}
