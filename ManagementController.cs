// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManagementController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the ManagementController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers
{
    using System.Web.Mvc;

    using Eyefinity.PracticeManagement.Common.Api;

    /// <summary>
    /// The management controller.
    /// </summary>
    [NoCache]
    public class ManagementController : Controller
    {
        // GET: /Management/

        /// <summary>
        /// The e time.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult ETime()
        {
            return this.View();
        }

        /// <summary>
        /// The change payment type.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult ChangePaymentType()
        {
            return this.View();
        }

        /// <summary>
        /// The change payment type log.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult ChangePaymentTypeLog()
        {
            return this.View();
        }

        /// <summary>
        /// The reset password.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult ResetPassword()
        {
            return this.View();
        }
    }
}