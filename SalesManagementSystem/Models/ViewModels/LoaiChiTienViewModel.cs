using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class LoaiChiTienViewModel
    {
        public int ID { get; set; }
        public string MaLoaiChiTien { get; set; }
        public string TenLoaiChiTien { get; set; }
        public int? STT { get; set; }
        public bool IsHoatDong { get; set; }
        public string GhiChu { get; set; }
    }
}
