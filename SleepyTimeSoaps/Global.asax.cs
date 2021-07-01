using Newtonsoft.Json;
using SleepyTimeSoaps.CustomAuthentication;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace SleepyTimeSoaps
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected FormsAuthenticationTicket GetAuthTicket()
        {
            HttpCookie authCookie = Request.Cookies["stsauth"];
            if (authCookie == null) return null;
            try
            {
                return FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch (CryptographicException exception)
            {
                return null;
            }
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var authTicket = GetAuthTicket();
            if (authTicket != null)
            {
                var serializeModel = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);

                CustomPrincipal principal = new CustomPrincipal(authTicket.Name);

                principal.UserId = serializeModel.UserId;
                principal.FirstName = serializeModel.FirstName;
                principal.LastName = serializeModel.LastName;
                principal.Roles = serializeModel.RoleName.ToArray<string>();

                HttpContext.Current.User = principal;
            }
            else
            {
                FormsAuthentication.SignOut();
                HttpContext context = HttpContext.Current;
                if (context != null && context.Session != null)
                    Session.Abandon();
                if (Request.Cookies["stsauth"] != null)
                {
                    var c = new HttpCookie("stsauth");
                    c.Expires = DateTime.Now.AddDays(-30);
                    Response.Cookies.Add(c);
                }
                Server.ClearError();
            }
        }

        protected void Application_Error(object sender_, CommandEventArgs e_)
        {
            var error = Server.GetLastError();
            var cryptoEx = error as CryptographicException;
            if (cryptoEx != null)
            {
                FormsAuthentication.SignOut();
                Session.Abandon();
                if (Request.Cookies["stsauth"] != null)
                {
                    var c = new HttpCookie("stsauth");
                    c.Expires = DateTime.Now.AddDays(-30);
                    Response.Cookies.Add(c);
                }
                Server.ClearError();
            }
        }
    }
}
