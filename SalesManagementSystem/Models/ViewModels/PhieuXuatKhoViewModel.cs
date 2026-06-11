using System;
using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuXuatKhoViewModel
    {
        public int ID { get; set; }
        public string SoChungTu { get; set; }
        public DateTime NgayXuat { get; set; }
        public int? IDKho { get; set; }
        public string TenKho { get; set; }
        public int? IDNhanSuNhan { get; set; }
        public string TenNhanSuNhan { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SoDienThoaiNguoiNhan { get; set; }
        public string GhiChu { get; set; }
        public int TrangThai { get; set; }
        public bool IsReadOnly { get; set; }

        public List<PhieuXuatKhoChiTietViewModel> ChiTiets { get; set; }

        public PhieuXuatKhoViewModel()
        {
            ChiTiets = new List<PhieuXuatKhoChiTietViewModel>();
            NgayXuat = DateTime.Now;
            TrangThai = 1; // Nháp
            IsReadOnly = false;
        }
    }
}
