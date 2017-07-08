// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppointmentReportsController.cs" company="Eyefinity, Inc.">
//  Eyefinity, Inc. - 2013  
// </copyright>
// <summary>
//   Defines the AppointmentReportsController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Appointment;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Reports;

    /// <summary>
    /// The appointment reports controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class AppointmentReportsController : ApiController
    {
        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly AppointmentReportsIt2Manager it2Business;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppointmentReportsController"/> class.
        /// </summary>
        public AppointmentReportsController()
        {
            this.it2Business = new AppointmentReportsIt2Manager();
        }

        // GET api/appointmentreports

        /// <summary>The get.</summary>
        /// <param name="officeNumber">The office Number.</param>
        /// <param name="report">The report.</param>
        /// <returns>The <see cref="AppointmentReportCriteria"/>.</returns>
        public HttpResponseMessage Get(string officeNumber, string report)
        {
            try
            {
                AccessControl.VerifyUserAccessToOffice(officeNumber);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                const string ValidationString = "You do not have security permission to access this area.<br/><br/> " +
                                                "Please contact your Office Manager or Office Administrator if you believe this is an error.";
                return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { validationmessage = ValidationString });
            }

            var vm = this.it2Business.GetReportCriteria(officeNumber, report);
            return Request.CreateResponse(HttpStatusCode.OK, vm);
        }
    }
}
