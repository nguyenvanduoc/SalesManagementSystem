using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DieuChinhNhapKhoListViewModel
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayNhap { get; set; }
        public int? IDLoaiNhapKho { get; set; }
        public string TenLoaiNhap { get; set; }
        public int? IDKho { get; set; }
        public string TenKhoNhap { get; set; }
        public int? IDKhoNguon { get; set; }
        public string TenKhoNguon { get; set; }
        public string DoiTuong { get; set; }
        public decimal TongTien { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal CongNo { get; set; }
        public bool DaDieuChinh { get; set; }
        public int SoLanDieuChinh { get; set; }
        public DateTime? NgayDieuChinhCuoi { get; set; }
        public int TrangThai { get; set; }
        public string TenTrangThai { get; set; }
    }

    public class DieuChinhNhapKhoHistoryViewModel
    {
        public int ID { get; set; }
        public string SoDieuChinh { get; set; }
        public DateTime NgayDieuChinh { get; set; }
        public string LyDoDieuChinh { get; set; }
        public decimal TongTienCu { get; set; }
        public decimal TongTienMoi { get; set; }
        public decimal ChenhLech { get; set; }
        public string TenNguoiTao { get; set; }
        public List<DieuChinhNhapKhoHistoryDetailViewModel> ChiTiets { get; set; }
    }

    public class DieuChinhNhapKhoHistoryDetailViewModel
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
        public decimal? DonGiaVanChuyenCu { get; set; }
        public decimal? DonGiaVanChuyenMoi { get; set; }
        public decimal? TienVanChuyenCu { get; set; }
        public decimal? TienVanChuyenMoi { get; set; }
        public string LoaiThayDoi { get; set; }
    }

    public class DieuChinhNhapKhoPostModel
    {
        [Required]
        public int IDPhieuNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do điều chỉnh")]
        public string LyDoDieuChinh { get; set; }
        
        [Required]
        public string ChiTietsJson { get; set; }

        public int IDLoaiNhapKho { get; set; }
        public int IDKho { get; set; }
        public int? IDKhoNguon { get; set; }
        public int? IDNhaCungCap { get; set; }
        public int? IDKhachHang { get; set; }
        public int? IDPhuongTien { get; set; }
        public DateTime NgayNhap { get; set; }
        public DateTime? NgayGiaoHang { get; set; }
        public string HoTenTaiXe { get; set; }
        public string SoDienThoaiTaiXe { get; set; }
        public string SoHoaDon { get; set; }
        public DateTime? NgayHoaDon { get; set; }
        public string GhiChu { get; set; }
    }
}
