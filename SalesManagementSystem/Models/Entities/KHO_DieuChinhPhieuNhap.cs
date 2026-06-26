using System;

namespace SalesManagementSystem.Models.Entities
{
    public class KHO_DieuChinhPhieuNhap
    {
        public int ID { get; set; }
        public int IDPhieuNhap { get; set; }
        public string SoDieuChinh { get; set; }
        public DateTime NgayDieuChinh { get; set; }
        public string LyDoDieuChinh { get; set; }
        public decimal TongTienCu { get; set; }
        public decimal TongTienMoi { get; set; }
        public decimal ChenhLech { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
    }
}
