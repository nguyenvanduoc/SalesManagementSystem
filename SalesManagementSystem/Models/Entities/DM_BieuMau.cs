using System;

namespace SalesManagementSystem.Models.Entities
{
    public class DM_BieuMau
    {
        public int ID { get; set; }
        public string MaBieuMau { get; set; }
        public string TenBieuMau { get; set; }
        public string TenFile { get; set; }
        public string DuoiFile { get; set; }
        public byte[] NoiDung { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
    }
}
