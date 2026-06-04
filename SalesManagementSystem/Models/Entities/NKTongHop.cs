using System;

namespace SalesManagementSystem.Models.Entities
{
    public class NKTongHop
    {
        public int ID { get; set; }
        public int IDLogin { get; set; }
        public string TenManHinh { get; set; }
        public string TenController { get; set; }
        public string TenAction { get; set; }
        public DateTime? NgayThucThi { get; set; }
        public string NoiDung { get; set; }
    }
}
