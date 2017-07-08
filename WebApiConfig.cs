// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the WebApiConfig type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.App_Start
{
    using System.Web.Http;

    /// <summary>
    /// The web API config.
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// The register.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute("OfficeActionApi", "api/Office/{officeNumber}/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            config.Routes.MapHttpRoute("OfficeApi", "api/Office/{officeNumber}/{controller}/{id}", new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute("ActionApiById", "api/{controller}/{action}/{id}", new { }, new { id = @"^[0-9]+$" });
            config.Routes.MapHttpRoute("DefaultApiById", "api/{controller}/{id}", new { }, new { id = @"^[0-9]+$" });

            config.Routes.MapHttpRoute("ActionApi", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            //// To disable tracing in your application, please comment out or remove the following line of code
            //// For more information, refer to: http://www.asp.net/web-api
            ////config.EnableSystemDiagnosticsTracing();
        }
    }
}
