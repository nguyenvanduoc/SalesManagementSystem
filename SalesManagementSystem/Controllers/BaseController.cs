using System.Web.Mvc;
using System.Web.Routing;
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
            if (sessionId != null)
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
    }
}
