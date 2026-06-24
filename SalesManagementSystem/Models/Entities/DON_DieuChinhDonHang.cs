using System;

namespace SalesManagementSystem.Models.Entities
{
    public class DON_DieuChinhDonHang
    {
        public int ID { get; set; }
        public int IDDonHang { get; set; }
        public string SoDieuChinh { get; set; }
        public DateTime NgayDieuChinh { get; set; }
        public string LyDoDieuChinh { get; set; }
        public decimal TongTienCu { get; set; }
        public decimal TongTienMoi { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? TrangThaiDon { get; set; }
    }
}
