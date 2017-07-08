// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonController.cs" company="Eyefinity, Inc">
//  2013 Eyefinity, Inc  
// </copyright>
// <summary>
//   Defines the CommonController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers
{
    using System.Web.Mvc;

    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Controllers.Api;

    /// <summary>
    /// The common controller.
    /// </summary>
    [NoCache]
    public class CommonController : Controller
    {
        // GET: /Common/

        /// <summary>
        /// The daily closing.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Daily Closing")]
        public ActionResult DailyClosing()
        {
            return this.View();
        }

        /// <summary>
        /// The generate PDF.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <param name="dayCloseId">
        /// The day Close Id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Daily Closing")]
        public ActionResult GeneratePdf(string officeNumber, int dayCloseId)
        {
            var ms = new DailyClosingController().GetReportContent(officeNumber, dayCloseId);
            if (ms != null)
            {
                Response.Clear();
                Response.ClearHeaders();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "inline; filename= filename=DailyClosingReport.pdf");
                Response.BinaryWrite(ms.ToByteArrayBytes());
                Response.End();
            }

            return null;
        }

        /// <summary>
        /// The e time.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult ChangePassword()
        {
            return this.View();
        }

        public ActionResult SecurityQuestion()
        {
            return this.View();
        }
    }
}
