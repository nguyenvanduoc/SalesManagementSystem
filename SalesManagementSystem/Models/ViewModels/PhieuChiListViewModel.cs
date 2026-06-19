using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuChiListViewModel
    {
        public int ID { get; set; }
        public string SoPhieuChi { get; set; }
        public DateTime NgayChi { get; set; }
        public int? IDKhoanMucChi { get; set; }
        public string TenKhoanMuc { get; set; }
        public int IDTaiKhoanThanhToan { get; set; }
        public string TenTaiKhoanThanhToan { get; set; }
        public string SoTaiKhoan { get; set; }
        public int? IDNguoiNhan { get; set; }
        public string TenNguoiNhan { get; set; }
        public string NguoiNhanTien { get; set; }
        public string SoDienThoaiNguoiNhan { get; set; }
        public int? IDNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }
        public int? IDPhieuNhap { get; set; }
        public string SoPhieuNhap { get; set; }
        public decimal SoTienChi { get; set; }
        public string DienGiai { get; set; }
        public int TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public DateTime? NgayGhi { get; set; }
        public string LyDoHuy { get; set; }

        public string TenTrangThai
        {
            get
            {
                switch (TrangThai)
                {
                    case 1: return "Đề nghị";
                    case 2: return "Đã ghi";
                    case 3: return "Đã hủy";
                    default: return "Không xác định";
                }
            }
        }

        public string CssTrangThai
        {
            get
            {
                switch (TrangThai)
                {
                    case 1: return "badge bg-warning text-dark";
                    case 2: return "badge bg-success";
                    case 3: return "badge bg-danger";
                    default: return "badge bg-secondary";
                }
            }
        }
    }
}
