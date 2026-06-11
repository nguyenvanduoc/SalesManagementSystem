using System;

namespace SalesManagementSystem.Models.Entities
{
    public class NS_DonDatHangChiTiet
    {
        public int ID { get; set; }
        public int IDDonDatHang { get; set; }
        public int? IDSanPham { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public decimal ThanhTienSauThue { get; set; }
        public decimal? ThanhTienThue { get; set; }
        public decimal ThueGTGT { get; set; }
        public bool IsHangKhuyenMai { get; set; }
        public string GhiChu { get; set; }

        // Denorm từ header
        public DateTime? NgayTaoDon { get; set; }
        public string SoDonHang { get; set; }
        public int? IDNhanVien { get; set; }
        public DateTime? ThoiHanGiaoHang { get; set; }
        public int? TrangThaiDon { get; set; }

        // Audit
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
    }
}
