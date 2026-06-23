using System;
using System.Collections.Generic;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Models.ViewModels
{
    public class HopDongKhachHangListVM
    {
        public IEnumerable<HopDongKhachHang> DanhSachHopDong { get; set; }
        public int TongSoTrang { get; set; }
        public int TrangHienTai { get; set; }
        public int TongSoBanGhi { get; set; }
    }

    public class HopDongDashboardVM
    {
        public int TongHopDong { get; set; }
        public int DangHieuLuc { get; set; }
        public int SapHetHan { get; set; }
        public int DaThanhLy { get; set; }
    }
}
