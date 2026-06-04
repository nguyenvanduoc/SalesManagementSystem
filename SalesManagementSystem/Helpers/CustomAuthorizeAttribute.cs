using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Helpers
{
    public class CustomAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly AuthorizeTypes _authType;

        public CustomAuthorizeAttribute(AuthorizeTypes authType)
        {
            _authType = authType;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (_authType == AuthorizeTypes.Everyone)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var session = HttpContext.Current.Session[CommonConstants.USER_SESSION] as UserLogin;

            if (session == null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Login", action = "Index" }));
                }
                return;
            }

            if (_authType == AuthorizeTypes.MustHavePermission)
            {
                string actionName = filterContext.ActionDescriptor.ActionName;
                string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

                bool hasPermission = PermissionHelper.HasActionPermission(controllerName, actionName);

                if (!hasPermission)
                {
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        filterContext.Result = new JsonResult
                        {
                            Data = new { success = false, message = "Bạn không có quyền thực hiện chức năng này." },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                    else
                    {
                        filterContext.Result = new ContentResult
                        {
                            Content = "<h3>Access Denied</h3><p>Bạn không có quyền thực hiện chức năng này.</p>"
                        };
                    }
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
