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

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            if (ex != null)
            {
                if (ex is System.Web.Mvc.HttpAntiForgeryException)
                {
                    Server.ClearError();
                    Response.Redirect("~/Login");
                    return;
                }
                
                SalesManagementSystem.Helpers.LogHelper.WriteErrorLog(ex, HttpContext.Current);
            }
        }

        protected void Session_End(object sender, System.EventArgs e)
        {
            var userSession = Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION] as SalesManagementSystem.Models.ViewModels.UserLoginViewModel;
            if (userSession != null)
            {
                var sessionRepo = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(SalesManagementSystem.Repositories.Interfaces.IAclLoginSessionRepository)) as SalesManagementSystem.Repositories.Interfaces.IAclLoginSessionRepository;
                if (sessionRepo != null)
                {
                    sessionRepo.LogLogout(userSession.UserID);
                }
            }
        }

        protected void Application_AcquireRequestState(object sender, System.EventArgs e)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                if (HttpContext.Current.Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION] == null)
                {
                    var authCookie = HttpContext.Current.Request.Cookies["SMS_AutoLogin"];
                    if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
                    {
                        try
                        {
                            var ticket = System.Web.Security.FormsAuthentication.Decrypt(authCookie.Value);
                            if (ticket != null && !ticket.Expired)
                            {
                                var userSession = Newtonsoft.Json.JsonConvert.DeserializeObject<SalesManagementSystem.Models.ViewModels.UserLoginViewModel>(ticket.UserData);
                                if (userSession != null)
                                {
                                    HttpContext.Current.Session[SalesManagementSystem.Helpers.CommonConstants.USER_SESSION] = userSession;
                                    
                                    var sessionRepo = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(SalesManagementSystem.Repositories.Interfaces.IAclLoginSessionRepository)) as SalesManagementSystem.Repositories.Interfaces.IAclLoginSessionRepository;
                                    if (sessionRepo != null)
                                    {
                                        var req = HttpContext.Current.Request;
                                        int sessionId = sessionRepo.LogLogin(new SalesManagementSystem.Models.Entities.AclLoginSession
                                        {
                                            IDLogin = userSession.UserID,
                                            HoTen = userSession.HoDem + " " + userSession.Ten,
                                            HostName = req.UserHostName,
                                            HostAddress = req.UserHostAddress,
                                            TrinhDuyet = req.Browser != null ? req.Browser.Browser + " " + req.Browser.Version : "Unknown",
                                            IP = req.ServerVariables["REMOTE_ADDR"] ?? req.UserHostAddress
                                        });
                                        HttpContext.Current.Session["LoginSessionID"] = sessionId;
                                    }
                                    
                                    // Renew auto-login cookie for 7 days
                                    var newTicket = new System.Web.Security.FormsAuthenticationTicket(1, ticket.Name, System.DateTime.Now, System.DateTime.Now.AddDays(7), true, ticket.UserData);
                                    var encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(newTicket);
                                    var newCookie = new System.Web.HttpCookie("SMS_AutoLogin", encryptedTicket) { HttpOnly = true, Expires = newTicket.Expiration };
                                    HttpContext.Current.Response.Cookies.Add(newCookie);
                                }
                            }
                        }
                        catch
                        {
                            // Ignore decryption/deserialization errors, just let them be logged out
                        }
                    }
                }
            }
        }
    }
}
