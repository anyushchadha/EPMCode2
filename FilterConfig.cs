// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterConfig.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the FilterConfig type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.App_Start
{
    using System.Web.Mvc;
    using Eyefinity.PracticeManagement.Common.Api;

    /// <summary>
    /// The filter config.
    /// </summary>
    public static class FilterConfig
    {
        /// <summary>
        /// The register global filters.
        /// </summary>
        /// <param name="filters">
        /// The filters.
        /// </param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new LogonAuthorize());
            filters.Add(new MvcNoCacheAttribute());
            filters.Add(new HandleErrorAttribute());
        }
    }
}