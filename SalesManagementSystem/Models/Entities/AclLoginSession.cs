using System;

namespace SalesManagementSystem.Models.Entities
{
    public class AclLoginSession
    {
        public int ID { get; set; }
        public int IDLogin { get; set; }
        public string HoTen { get; set; }
        public DateTime ThoiGianLogin { get; set; }
        public DateTime? ThoiGianLogout { get; set; }
        public string HostName { get; set; }
        public string HostAddress { get; set; }
        public string TrinhDuyet { get; set; }
        public string IP { get; set; }
        public bool IsDangHoatDong { get; set; }
        public DateTime? LastActiveTime { get; set; }
    }
}
