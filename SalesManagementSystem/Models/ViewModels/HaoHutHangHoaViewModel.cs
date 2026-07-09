using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class HaoHutHangHoaViewModel
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayHaoHut { get; set; }
        public int LoaiHaoHut { get; set; } // 1: Hao hụt bán hàng, 2: Hao hụt tồn kho
        public int? IDKho { get; set; }
        public string TenKho { get; set; }
        public int? IDDonHang { get; set; }
        public string SoDonHang { get; set; }
        public int? IDChungTuBanHang { get; set; }
        public string SoChungTuBanHang { get; set; }
        public int? IDKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string LyDo { get; set; }
        public string GhiChu { get; set; }
        public decimal? TongSoLuong { get; set; }
        public decimal? TongTienHaoHut { get; set; }
        public int TrangThai { get; set; } // 1: Draft, 2: Ghi Nhan, 3: Huy
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }

        public int TotalRecords { get; set; }
        
        public List<HaoHutHangHoaChiTietViewModel> Details { get; set; }

        public HaoHutHangHoaViewModel()
        {
            Details = new List<HaoHutHangHoaChiTietViewModel>();
        }
    }

    public class HaoHutHangHoaChiTietViewModel
    {
        public int ID { get; set; }
        public int IDHaoHut { get; set; }
        public int IDSanPham { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public decimal SoLuongHaoHut { get; set; }
        public decimal SLHienTai { get; set; }
        public decimal DonGiaHaoHut { get; set; }
        public decimal TienHaoHut { get; set; }
        public decimal? DonGiaBan { get; set; }
        public decimal? DoanhThuGiam { get; set; }
        public string GhiChu { get; set; }
    }

    public class HaoHutHangHoaFilter
    {
        public string TuNgay { get; set; }
        public string DenNgay { get; set; }
        public int LoaiHaoHut { get; set; }
        public int IDKho { get; set; }
        public int IDKhachHang { get; set; }
        public string SoChungTu { get; set; }
        public int TrangThai { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
