// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FeeSlipsController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the FeeSlipsController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers
{
    using System.Web.Mvc;

    using Eyefinity.PracticeManagement.Common.Api;

    /// <summary>
    /// The fee slips controller.
    /// </summary>
    [NoCache]
    public class FeeSlipsController : Controller
    {
        // GET: /FeeSlips/

        /// <summary>
        /// The fee slips.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult FeeSlips()
        {
            return this.View();
        }
    }
}