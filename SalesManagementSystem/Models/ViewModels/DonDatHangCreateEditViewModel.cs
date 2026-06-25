using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models.ViewModels
{
    /// <summary>ViewModel cho form tạo mới / chỉnh sửa đơn đặt hàng.</summary>
    public class DonDatHangCreateEditViewModel
    {
        public int ID { get; set; }

        // ── Thông tin khách hàng (Select2) ──────────────────────────────
        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public int? IDKhachHang { get; set; }

        // Readonly display sau khi chọn
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string MaSoThue { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }

        // ── Thông tin đơn hàng ──────────────────────────────────────────
        [Required(ErrorMessage = "Vui lòng nhập số đơn hàng")]
        [StringLength(100, ErrorMessage = "Số đơn hàng không được vượt quá 100 ký tự")]
        public string SoDonHang { get; set; }

        public DateTime? NgayTaoDon { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhân viên phụ trách")]
        public int? IDNhanVien { get; set; }

        public DateTime? ThoiHanGiaoHang { get; set; }
        public DateTime? NgayGiaoHang { get; set; }

        public int TrangThaiDon { get; set; } = 1;

        [StringLength(3000)]
        public string GhiChu { get; set; }

        public decimal TongTien { get; set; }
        public decimal PhiBocXep { get; set; }
        public decimal ThanhTienHang { get; set; }
        public decimal ThanhTienThue { get; set; }

        // ── Chi tiết sản phẩm ───────────────────────────────────────────
        public List<DonDatHangChiTietViewModel> ChiTiets { get; set; }
            = new List<DonDatHangChiTietViewModel>();

        // ── Dropdown data (không map DB) ────────────────────────────────
        public System.Web.Mvc.SelectList NhanVienList { get; set; }
        public System.Web.Mvc.SelectList TrangThaiList { get; set; }
    }
}
