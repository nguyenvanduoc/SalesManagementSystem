using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class PhieuThuKhachHangListViewModel
    {
        public int ID { get; set; }
        public string SoPhieuThu { get; set; }
        public DateTime NgayThu { get; set; }
        public int IDTaiKhoanThanhToan { get; set; }
        public string TenTaiKhoanThanhToan { get; set; }
        public string NguoiNopTien { get; set; }
        public string SoDienThoaiNguoiNop { get; set; }
        public int? IDKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string SoChungTuBanHang { get; set; }
        public decimal TonDauKy { get; set; }
        public decimal SoTienThu { get; set; }
        public decimal SoTienPhanBo { get; set; }
        public decimal TienTraTruoc { get; set; }
        public decimal LuyKe { get; set; }
        public string DienGiai { get; set; }
        public int TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public string TenNguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public bool HasDinhKem { get; set; }
        
        public string TenTrangThai
        {
            get
            {
                switch (TrangThai)
                {
                    case 1: return "Đề nghị ghi";
                    case 2: return "Đã ghi sổ";
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
