// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClaimManagementController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the ClaimManagementController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers
{
    using System.Web.Mvc;

    using Eyefinity.PracticeManagement.Common.Api;

    /// <summary>
    /// The claim management controller.
    /// </summary>
    [NoCache]
    public class ClaimManagementController : Controller
    {
        // GET: /ClaimManagement/

        /// <summary>
        /// The claim management.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult ClaimManagement()
        {
            return this.View();
        }
    }
}