using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DonDieuChinhListViewModel
    {
        public int ID { get; set; }
        public string SoDonHang { get; set; }
        public DateTime? NgayTaoDon { get; set; }
        public string TenKhachHang { get; set; }
        public decimal TongTien { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal CongNo => TongTien - DaThanhToan;
        public int TrangThaiDon { get; set; }
        public string TenTrangThai { get; set; }

        // Các cột bổ sung
        public bool DaDieuChinh { get; set; }
        public int SoLanDieuChinh { get; set; }
        public DateTime? NgayDieuChinh { get; set; }
        public string NguoiDieuChinh { get; set; }
    }

    public class DonDieuChinhHistoryViewModel
    {
        public int ID { get; set; }
        public string SoDieuChinh { get; set; }
        public DateTime NgayDieuChinh { get; set; }
        public string LyDoDieuChinh { get; set; }
        public decimal TongTienCu { get; set; }
        public decimal TongTienMoi { get; set; }
        public string TenNguoiTao { get; set; }
        public int? TrangThaiDon { get; set; }
        public string TenTrangThai { get; set; }
        public List<DonDieuChinhHistoryDetailViewModel> ChiTiets { get; set; } = new List<DonDieuChinhHistoryDetailViewModel>();
    }

    public class DonDieuChinhHistoryDetailViewModel
    {
        public string TenSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string DVT { get; set; }
        public decimal? SoLuongCu { get; set; }
        public decimal? SoLuongMoi { get; set; }
        public decimal? DonGiaCu { get; set; }
        public decimal? DonGiaMoi { get; set; }
        public decimal? ThanhTienCu { get; set; }
        public decimal? ThanhTienMoi { get; set; }
        public string GhiChu { get; set; }
    }

    public class DonDieuChinhPostModel
    {
        public int IDDonHang { get; set; }
        public string LyDoDieuChinh { get; set; }
        public string ChiTietsJson { get; set; }
        public decimal PhiBocXep { get; set; }
    }
}
