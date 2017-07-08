// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RouteConfig.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the RouteConfig type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.App_Start
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// The route config.
    /// </summary>
    public static class RouteConfig
    {
        /// <summary>
        /// The register routes.
        /// </summary>
        /// <param name="routes">
        /// The routes.
        /// </param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.AppendTrailingSlash = true;
            routes.LowercaseUrls = true;

            //// IgnoreRoute - Tell the routing system to ignore certain routes for better performance.
            //// Ignore .axd files.
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //// Ignore everything in the Content folder.
            routes.IgnoreRoute("Content/{*pathInfo}");
            //// Ignore everything in the Scripts folder.
            routes.IgnoreRoute("Scripts/{*pathInfo}");
           
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "Eyefinity.PracticeManagement.Controllers" });
        }
    }
}