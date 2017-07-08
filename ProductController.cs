// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductController.cs" company="Eyefinity, Inc">
//   2013 Eyefinity, Inc
// </copyright>
// <summary>
//   Defines the ProductController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers
{
    using System.Web.Mvc;

    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;

    /// <summary>
    /// The work in progress controller.
    /// </summary>
    [NoCache]
    public class ProductController : Controller
    {
        // GET: /WorkInProgress/

        /// <summary>
        /// The lookup.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Lookup()
        {
            return this.View();
        }

        /// <summary>
        /// The inventory.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Inventory()
        {
            var authorizationTicketHelper = new AuthorizationTicketHelper();
            var user = authorizationTicketHelper.GetUserInfo();
            return this.View(user);
        }

        /// <summary>
        /// The work in progress.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult WorkInProgress()
        {
            return this.View();
        }

        /// <summary>
        /// The print labels.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult PrintLabels()
        {
            return this.View();
        }
    }
}