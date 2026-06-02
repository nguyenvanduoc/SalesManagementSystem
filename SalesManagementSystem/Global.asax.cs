using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SalesManagementSystem.App_Start;

namespace SalesManagementSystem
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // 1. Khởi tạo Unity DI Container (PHẢI đặt đầu tiên)
            UnityConfig.RegisterComponents();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
