using System.Web.Mvc;
using System.Web.Routing;
using System;
using System.Linq;
using SalesManagementSystem.Helpers;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Controllers
{
    [CustomAuthorize(AuthorizeTypes.AuthorizedUsers)]
    public class BaseController : Controller
    {
        protected AuditHelper AuditLog { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var sessionId = Session["LoginSessionID"];
            if (sessionId != null && (int)sessionId > 0)
            {
                var sessionRepo = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(SalesManagementSystem.Repositories.Interfaces.IAclLoginSessionRepository)) as SalesManagementSystem.Repositories.Interfaces.IAclLoginSessionRepository;
                if (sessionRepo != null)
                {
                    bool isActive = sessionRepo.IsSessionActive((int)sessionId);
                    if (!isActive)
                    {
                        Session.Clear();
                        if (filterContext.HttpContext.Request.IsAjaxRequest())
                        {
                            filterContext.Result = new JsonResult { Data = new { success = false, message = "Phiên làm việc của bạn đã bị ngắt bởi quản trị viên." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        }
                        else
                        {
                            filterContext.Result = new RedirectResult("/Login/Index");
                        }
                        return;
                    }
                }
            }

            AuditLog = new AuditHelper();
            
            // Auto-default tuNgay and denNgay to current month on initial screen load
            if (filterContext.ActionParameters.ContainsKey("tuNgay"))
            {
                var tuNgay = filterContext.ActionParameters["tuNgay"] as string;
                bool hasParam = (filterContext.HttpContext.Request.QueryString.AllKeys.Contains("tuNgay") || 
                                 filterContext.HttpContext.Request.Form.AllKeys.Contains("tuNgay"));
                                
                if (!hasParam && string.IsNullOrEmpty(tuNgay))
                {
                    filterContext.ActionParameters["tuNgay"] = new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyy-MM-dd");
                }
            }

            if (filterContext.ActionParameters.ContainsKey("denNgay"))
            {
                var denNgay = filterContext.ActionParameters["denNgay"] as string;
                bool hasParam = (filterContext.HttpContext.Request.QueryString.AllKeys.Contains("denNgay") || 
                                 filterContext.HttpContext.Request.Form.AllKeys.Contains("denNgay"));
                                
                if (!hasParam && string.IsNullOrEmpty(denNgay))
                {
                    filterContext.ActionParameters["denNgay"] = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }

            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Exception == null && AuditLog != null && AuditLog.HasChanges())
            {
                bool isSuccess = false;
                if (filterContext.Result is JsonResult || filterContext.Result is RedirectToRouteResult || filterContext.Result is RedirectResult)
                {
                    isSuccess = true;
                }
                else if (ModelState.IsValid)
                {
                    isSuccess = true;
                }

                if (isSuccess)
                {
                    var session = (UserLoginViewModel)Session[CommonConstants.USER_SESSION];
                    int loginId = session?.UserID ?? 0;

                    string controller = filterContext.RouteData.Values["controller"]?.ToString();
                    string action = filterContext.RouteData.Values["action"]?.ToString();
                    string manHinh = ViewBag.Title as string;

                    try
                    {
                        AuditLog.SaveAudit(loginId, manHinh, controller, action);
                    }
                    catch (System.Exception ex)
                    {
                        LogHelper.WriteErrorLog(ex, System.Web.HttpContext.Current);
                    }
                }
            }

            base.OnActionExecuted(filterContext);
        }

        protected void ForceSaveAudit()
        {
            if (AuditLog != null && AuditLog.HasChanges())
            {
                var session = (UserLoginViewModel)Session[CommonConstants.USER_SESSION];
                int loginId = session?.UserID ?? 0;
                string controller = RouteData.Values["controller"]?.ToString();
                string action = RouteData.Values["action"]?.ToString();
                string manHinh = ViewBag.Title as string;

                try
                {
                    AuditLog.SaveAudit(loginId, manHinh, controller, action);
                }
                catch (System.Exception ex)
                {
                    LogHelper.WriteErrorLog(ex, System.Web.HttpContext.Current);
                }
            }
        }

        protected UserLoginViewModel GetCurrentUser()
            => (UserLoginViewModel)Session[CommonConstants.USER_SESSION];

        protected ActionResult ExportDanhMucToExcel<T>(
            string maBieuMau,
            System.Collections.Generic.IEnumerable<T> data,
            string tenManHinh,
            string fileNamePrefix)
        {
            var excelExportService = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(SalesManagementSystem.Services.Interfaces.IExcelExportService)) as SalesManagementSystem.Services.Interfaces.IExcelExportService;
            if (excelExportService == null)
            {
                throw new Exception("Không thể resolve IExcelExportService.");
            }

            var session = GetCurrentUser();
            string nguoiLapBieu = session != null ? (session.HoDem + " " + session.Ten).Trim() : "";
            if (string.IsNullOrEmpty(nguoiLapBieu)) nguoiLapBieu = session?.UserName ?? "";

            var variables = new System.Collections.Generic.Dictionary<string, object>
            {
                { "TenManHinh", tenManHinh },
                { "Ngay", DateTime.Now.ToString("dd") },
                { "Thang", DateTime.Now.ToString("MM") },
                { "Nam", DateTime.Now.ToString("yyyy") },
                { "NguoiLapBieu", nguoiLapBieu }
            };

            string fileExtension;
            var fileBytes = excelExportService.Export(maBieuMau, data, out fileExtension, variables);

            // Ghi cookie để thông báo cho Client tắt spinner loading
            var downloadToken = Request["downloadToken"];
            if (!string.IsNullOrEmpty(downloadToken))
            {
                var cookie = new System.Web.HttpCookie("downloadToken", downloadToken)
                {
                    Path = "/"
                };
                Response.Cookies.Add(cookie);
            }

            string contentType = fileExtension == "xls" 
                ? "application/vnd.ms-excel" 
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(fileBytes, contentType, fileNamePrefix + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + fileExtension);
        }
    }
}
