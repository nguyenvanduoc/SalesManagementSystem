using System;

namespace SalesManagementSystem.Models.ViewModels
{
    public class DMBieuMauViewModel
    {
        public int ID { get; set; }
        public string MaBieuMau { get; set; }
        public string TenBieuMau { get; set; }
        public string TenFile { get; set; }
        public string DuoiFile { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public string TenNguoiTao { get; set; }
    }
}
