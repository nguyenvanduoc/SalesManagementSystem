namespace SalesManagementSystem.Models.ViewModels
{
    /// <summary>ViewModel cho từng dòng chi tiết trong form đơn hàng.</summary>
    public class DonDatHangChiTietViewModel
    {
        public int ID { get; set; }
        public int IDDonDatHang { get; set; }

        // Sản phẩm
        public int? IDSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string DVT { get; set; }

        // Số liệu
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThueGTGT { get; set; }
        public decimal ThanhTien { get; set; }
        public decimal ThanhTienThue { get; set; }
        public decimal ThanhTienSauThue { get; set; }

        public decimal? DonGiaBocXep { get; set; }
        public decimal? ThanhTienBocXep { get; set; }
        public decimal? ThanhTienHang { get; set; }

        public bool IsHangKhuyenMai { get; set; }
        public string GhiChu { get; set; }
    }
}
