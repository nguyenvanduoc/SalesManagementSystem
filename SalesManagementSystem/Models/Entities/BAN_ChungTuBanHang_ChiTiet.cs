using System;

namespace SalesManagementSystem.Models.Entities
{
    public class BAN_ChungTuBanHang_ChiTiet
    {
        public int ID { get; set; }
        public int IDChungTuBanHang { get; set; }
        public int IDSanPham { get; set; }
        public int? STT { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal? DonGiaBocXep { get; set; }
        public decimal? ThanhTienBocXep { get; set; }
        public decimal? ThanhTienHang { get; set; }
        public decimal ThanhTien { get; set; }
        public decimal ThueGTGT { get; set; }
        public decimal TienThue { get; set; }
        public decimal TongSauThue { get; set; }
        public string GhiChu { get; set; }
    }
}
