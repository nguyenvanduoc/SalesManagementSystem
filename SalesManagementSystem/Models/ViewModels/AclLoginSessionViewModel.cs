using System;
using SalesManagementSystem.Models.Entities;

namespace SalesManagementSystem.Models.ViewModels
{
    public class AclLoginSessionViewModel : AclLoginSession
    {
        public string TenDangNhap { get; set; }

        public string ThoiLuong
        {
            get
            {
                if (ThoiGianLogout.HasValue)
                {
                    var span = ThoiGianLogout.Value - ThoiGianLogin;
                    if (span.TotalHours >= 1)
                        return $"{(int)span.TotalHours}h {span.Minutes}m {span.Seconds}s";
                    if (span.TotalMinutes >= 1)
                        return $"{span.Minutes}m {span.Seconds}s";
                    return $"{span.Seconds}s";
                }
                return "Đang trực tuyến";
            }
        }
    }
}
