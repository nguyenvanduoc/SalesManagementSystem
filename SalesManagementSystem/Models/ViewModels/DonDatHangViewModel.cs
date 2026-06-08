using System;

namespace SalesManagementSystem.Models.ViewModels
{
    /// <summary>ViewModel hiển thị danh sách đơn đặt hàng (join KH, NV).</summary>
    public class DonDatHangViewModel
    {
        public int ID { get; set; }
        public string SoDonHang { get; set; }
        public DateTime? NgayTaoDon { get; set; }
        public DateTime? ThoiHanGiaoHang { get; set; }
        public int TrangThaiDon { get; set; }
        public string TenTrangThai { get; set; }
        public decimal TongTien { get; set; }
        public string GhiChu { get; set; }

        // Từ KhachHang
        public int? IDKhachHang { get; set; }
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }

        // Từ NhanVien
        public int? IDNhanVien { get; set; }
        public string TenNhanVien { get; set; }

        // Audit
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
    }
}
