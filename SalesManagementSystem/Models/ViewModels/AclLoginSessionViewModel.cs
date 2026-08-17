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

        public string ThietBi
        {
            get
            {
                string text = (TrinhDuyet ?? "") + " " + (HostName ?? "") + " " + (HostAddress ?? "");
                var ua = text.ToLowerInvariant();

                if (ua.Contains("iphone")) return "Mobile (iPhone)";
                if (ua.Contains("ipad")) return "Tablet (iPad)";
                if (ua.Contains("samsung") || ua.Contains("sm-") || ua.Contains("sec-")) return "Mobile (Samsung)";
                if (ua.Contains("xiaomi") || ua.Contains("redmi") || ua.Contains("mi ")) return "Mobile (Xiaomi)";
                if (ua.Contains("oppo") || ua.Contains("cph")) return "Mobile (Oppo)";
                if (ua.Contains("vivo")) return "Mobile (Vivo)";
                if (ua.Contains("huawei") || ua.Contains("honor")) return "Mobile (Huawei)";
                if (ua.Contains("android")) return ua.Contains("mobile") ? "Mobile (Android)" : "Tablet (Android)";
                if (ua.Contains("mobile")) return "Mobile";

                if (ua.Contains("macintosh") || ua.Contains("mac os") || ua.Contains("macbook")) return "PC (MacBook)";
                if (ua.Contains("linux") && !ua.Contains("android")) return "PC (Linux)";
                if (ua.Contains("windows") || ua.Contains("win32") || ua.Contains("win64")) return "PC (Windows)";

                return "PC (Desktop)";
            }
        }

        public string ThietBiBadgeHtml
        {
            get
            {
                string device = ThietBi;
                if (device.StartsWith("Mobile") || device.StartsWith("Tablet"))
                {
                    return $"<span class=\"badge bg-primary-subtle text-primary border border-primary-subtle px-2 py-1\"><i class=\"bi bi-phone-fill me-1\"></i>{device}</span>";
                }
                if (device.Contains("MacBook") || device.Contains("Mac"))
                {
                    return $"<span class=\"badge bg-dark-subtle text-dark border border-dark-subtle px-2 py-1\"><i class=\"bi bi-laptop-fill me-1\"></i>{device}</span>";
                }
                return $"<span class=\"badge bg-secondary-subtle text-secondary border border-secondary-subtle px-2 py-1\"><i class=\"bi bi-display-fill me-1\"></i>{device}</span>";
            }
        }
    }
}
