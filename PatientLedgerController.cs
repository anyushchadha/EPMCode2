// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatientLedgerController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   The patient ledger controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;

    using IT2.Ioc;
    using IT2.Services;

    /// <summary>
    ///     The patient controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientLedgerController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The practice Id.
        /// </summary>
        private readonly string practiceLocationId = string.Empty;

        /// <summary>
        /// The patient ledger manager.
        /// </summary>
        private readonly PatientLedgerIt2Manager patientLedgerManager;

        /// <summary>
        /// The company Id.
        /// </summary>
        private string companyId = string.Empty;
        
        public PatientLedgerController(string officeNumber)
        {
            this.practiceLocationId = officeNumber;
            this.companyId = new AuthorizationTicketHelper().GetUserInfo().CompanyId;
            this.patientLedgerManager = new PatientLedgerIt2Manager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientController"/> class.
        /// </summary>
        public PatientLedgerController()
        {
            this.patientLedgerManager = new PatientLedgerIt2Manager();
            if (!this.User.Identity.IsAuthenticated)
            {
                return;
            }

            var authorizationTicketHelper = new AuthorizationTicketHelper();
            this.practiceLocationId = authorizationTicketHelper.GetPracticeLocationId();
        }

        public HttpResponseMessage GetPatientLedger(int patientId, int orderId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                this.companyId = new AuthorizationTicketHelper().GetUserInfo().CompanyId;
                return Request.CreateResponse(HttpStatusCode.OK, this.patientLedgerManager.GetPatientLedgerForOrder(this.companyId, orderId));
            }
            catch (Exception ex)
            {
                var error = "GetPatientLedger(" + string.Format("{0}, {1}", patientId, orderId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }
    }
}