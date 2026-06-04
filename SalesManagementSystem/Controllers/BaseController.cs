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
    }
}
