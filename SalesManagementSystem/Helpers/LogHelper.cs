using System;
using System.IO;
using System.Text;
using System.Web;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Helpers
{
    public static class LogHelper
    {
        private static readonly object _lock = new object();

        public static void WriteErrorLog(Exception ex, HttpContext context)
        {
            try
            {
                // Thư mục Logs nằm ở thư mục gốc của project
                string logDirectory = HttpContext.Current.Server.MapPath("~/Logs");
                
                // Nếu chưa có thư mục thì tạo
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // Tên file yyyy-MM-dd.log
                string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                string filePath = Path.Combine(logDirectory, fileName);

                // Thu thập thông tin
                string errorTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                string username = "Unknown";
                string controller = "Unknown";
                string action = "Unknown";
                string url = context?.Request?.Url?.ToString() ?? "Unknown";
                string message = ex.Message;
                string stackTrace = ex.StackTrace;
                string innerException = ex.InnerException != null ? ex.InnerException.Message : "None";

                // Lấy thông tin Username từ Session (nếu có)
                if (context?.Session != null && context.Session[CommonConstants.USER_SESSION] != null)
                {
                    var userSession = (UserLoginViewModel)context.Session[CommonConstants.USER_SESSION];
                    username = userSession.UserName;
                }

                // Lấy thông tin Controller & Action từ RouteData
                if (context?.Request?.RequestContext?.RouteData?.Values != null)
                {
                    var routeValues = context.Request.RequestContext.RouteData.Values;
                    if (routeValues.ContainsKey("controller"))
                        controller = routeValues["controller"].ToString();
                    if (routeValues.ContainsKey("action"))
                        action = routeValues["action"].ToString();
                }

                // Build nội dung log
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"[Thời gian]       : {errorTime}");
                sb.AppendLine($"[Username]        : {username}");
                sb.AppendLine($"[Controller]      : {controller}");
                sb.AppendLine($"[Action]          : {action}");
                sb.AppendLine($"[URL]             : {url}");
                sb.AppendLine($"[Message]         : {message}");
                sb.AppendLine($"[InnerException]  : {innerException}");
                sb.AppendLine($"[StackTrace]      : ");
                sb.AppendLine(stackTrace);
                sb.AppendLine("--------------------------------------------------\n");

                // Thread-safe ghi file
                lock (_lock)
                {
                    File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
                }
            }
            catch
            {
                // Fallback nếu việc ghi log cũng sinh lỗi (không throw để tránh chết app)
            }
        }
    }
}
