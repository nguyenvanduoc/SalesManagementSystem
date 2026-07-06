using System;

namespace SalesManagementSystem.Models.Entities
{
    public class PhieuChiFile
    {
        public int ID { get; set; }
        public int IDPhieuChi { get; set; }
        public string TenFile { get; set; }
        public string LoaiFile { get; set; }
        public long? DungLuong { get; set; }
        public byte[] NoiDungFile { get; set; }
        public string GhiChu { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public int? NguoiCapNhat { get; set; }
        public bool IsDeleted { get; set; }
        public string TenNguoiTao { get; set; }
    }
}
