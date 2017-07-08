// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the Global.asax type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement
{
    using System;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    using Eyefinity.PracticeManagement.App_Start;
    using Eyefinity.PracticeManagement.Business.Ioc;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    /// <summary>
    /// The MVC application.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// The application_ start.
        /// </summary>
        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            ConfigureViewEngines();
            ConfigureAntiForgeryTokens();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.Filters.Add(new NoCacheAttribute());
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterScriptBundles(BundleTable.Bundles);
            MvcHandler.DisableMvcResponseHeader = true;
            AdditionalIoc.RegisterAll();
            ////JsonConfig.Configure();
            ////NHibernate Profiler - UnComment Following line to work with Profiler
            ////HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
        }

        /// <summary>
        /// The session_ end.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Session_End(object sender, EventArgs e)
        {
            ////HACK: session state is required for Crystal Reports Viewer so that paging works!!! Session state needs to be eliminated.
            ////When session dies, so should kill any report that the report viewer has a hold of.
            ReportFactory.KillSession(this.Session.SessionID);
        }

        /// <summary>
        /// Configures the view engines. By default, Asp.Net MVC includes the Web Forms (WebFormsViewEngine) and 
        /// Razor (RazorViewEngine) view engines that supports both C# (.cshtml) and VB (.vbhtml). You can remove view 
        /// engines you are not using here for better performance and include a custom Razor view engine that only 
        /// supports C#.
        /// </summary>
        private static void ConfigureViewEngines()
        {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new CSharpRazorViewEngine());
        }

        /// <summary>
        /// Configures the anti-forgery tokens. See 
        /// http://www.asp.net/mvc/overview/security/xsrfcsrf-prevention-in-aspnet-mvc-and-web-pages
        /// </summary>
        private static void ConfigureAntiForgeryTokens()
        {
            //// Rename the Anti-Forgery cookie from "__RequestVerificationToken" to "f". This adds a little security 
            //// through obscurity and also saves sending a few characters over the wire. Sadly there is no way to change 
            //// the form input name which is hard coded in the @Html.AntiForgeryToken helper and the 
            //// ValidationAntiforgeryTokenAttribute to  __RequestVerificationToken.
            //// <input name="__RequestVerificationToken" type="hidden" value="..." />
            AntiForgeryConfig.CookieName = "f";

            //// If you have enabled SSL. Uncomment this line to ensure that the Anti-Forgery 
            //// cookie requires SSL to be sent across the wire. 
            AntiForgeryConfig.RequireSsl = true;
        }

        ////public static class JsonConfig
        ////{
        ////    public static void Configure()
        ////    {
        ////        var formatters = GlobalConfiguration.Configuration.Formatters;
        ////        var jsonFormatter = formatters.JsonFormatter;
        ////        var settings = jsonFormatter.SerializerSettings;
        ////        settings.NullValueHandling = NullValueHandling.Include;
        ////        settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
        ////    }
        ////}
    }
}