using System;
using System.Web.Mvc;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class DashboardController : BaseController
    {
        private readonly IDashboardRepository _repo;

        public DashboardController(IDashboardRepository repo)
        {
            _repo = repo;
        }

        public ActionResult Index()
        {
            // Kiểm tra quyền truy cập Dashboard
            bool hasPermission = PermissionHelper.HasPermission("Dashboard", LoaiPhanQuyen.Xem);
            if (!hasPermission)
            {
                var user = GetCurrentUser();
                ViewBag.HoTen = user?.HoDem + " " + user?.Ten ?? "Người dùng";
                return View("Blank");
            }

            // Có quyền -> Render UI Dashboard
            return View();
        }

        [HttpPost]
        public JsonResult GetData(string tuNgay, string denNgay)
        {
            try
            {
                bool hasPermission = PermissionHelper.HasPermission("Dashboard", LoaiPhanQuyen.Xem);
                if (!hasPermission)
                {
                    return Json(new { success = false, message = "Không có quyền truy cập" });
                }

                DateTime? dtTu = ParseDate(tuNgay);
                DateTime? dtDen = ParseDate(denNgay);

                var data = _repo.GetDashboardData(dtTu, dtDen);
                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorLog(ex, System.Web.HttpContext.Current);
                return Json(new { success = false, message = ex.Message });
            }
        }

        private DateTime? ParseDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return null;

            string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "dd-MM-yyyy" };
            if (DateTime.TryParseExact(dateStr.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            if (DateTime.TryParse(dateStr, out result))
            {
                return result;
            }
            return null;
        }
    }
}
