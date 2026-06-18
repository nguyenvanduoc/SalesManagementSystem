using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuNhapKhoViewModel
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayNhap { get; set; }
        public int? IDKho { get; set; }
        public string TenKho { get; set; }
        public int? IDNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }
        public string SoHoaDon { get; set; }
        public DateTime? NgayHoaDon { get; set; }
        public string TenNguoiGiao { get; set; }
        public string SoDienThoaiNguoiGiao { get; set; }
        public string TenNguoiNhan { get; set; }
        public string GhiChu { get; set; }
        public int TrangThai { get; set; }
        public bool IsReadOnly { get; set; } // Set to true if TrangThai == 2 or 3

        public List<PhieuNhapKhoChiTietViewModel> ChiTiets { get; set; }

        public PhieuNhapKhoViewModel()
        {
            ChiTiets = new List<PhieuNhapKhoChiTietViewModel>();
            NgayNhap = DateTime.Now;
            TrangThai = 1; // Mặc định là Nháp
            IsReadOnly = false;
        }
    }
}
