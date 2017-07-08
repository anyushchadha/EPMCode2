// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserController.cs" company="Eyefinity, Inc">
//   2013 Eyefinity, Inc
// </copyright>
// <summary>
//   Defines the UserController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers
{
    using System.Web.Mvc;

    using Eyefinity.PracticeManagement.Common.Api;

    /// <summary>
    /// The user controller.
    /// </summary>
    [NoCache]
    public class UserController : Controller
    {
        // GET: /User/

        /// <summary>
        /// The my schedule.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult MySchedule()
        {
            return this.View();
        }

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
    }
}