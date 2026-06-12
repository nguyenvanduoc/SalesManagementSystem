using System;

namespace SalesManagementSystem.Models.Entities
{
    public class KT_NhatKyChung
    {
        public int ID { get; set; }
        public DateTime NgayChungTu { get; set; }
        public string SoChungTu { get; set; }
        public string LoaiChungTu { get; set; }
        public int IDChungTu { get; set; }
        public string TaiKhoanNo { get; set; }
        public string TaiKhoanCo { get; set; }
        public decimal SoTien { get; set; }
        public string DienGiai { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public bool IsHuy { get; set; }
    }
}
