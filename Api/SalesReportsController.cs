﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SalesReportsController.cs" company="Eyefinity, Inc.">
//  Eyefinity, Inc. - 2013    
// </copyright>
// <summary>
//   Defines the SalesReportsController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Reports;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Reports;
    using IT2.Ioc;
    using IT2.Services;

    /// <summary>
    /// The sales reports controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class SalesReportsController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The it 2 manager.
        /// </summary>
        private readonly SalesReportsIt2Manager it2Manager;

        /// <summary>The employee services.</summary>
        private readonly IEmployeeServices employeeServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="SalesReportsController"/> class.
        /// </summary>
        public SalesReportsController()
        {
            this.it2Manager = new SalesReportsIt2Manager();
            this.employeeServices = Container.Resolve<IEmployeeServices>();
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="report">
        /// The report.
        /// </param>
        /// <returns>
        /// The <see cref="SalesReportCriteria"/>.
        /// </returns>
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

            try
            {
                var user = new AuthorizationTicketHelper().GetUserInfo();
                var officeEmployee = this.employeeServices.GetOfficeEmployeeByUserId(user.Id);
                return Request.CreateResponse(HttpStatusCode.OK, this.it2Manager.GetReportCriteria(officeNumber, report, officeEmployee));
            }
            catch (Exception ex)
            {
                var msg = string.Format("Get(officeNumber = {0}, {1}, {2}", officeNumber, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }
    }
}
