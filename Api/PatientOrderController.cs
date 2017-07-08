namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;

    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientOrderController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The practice Id.
        /// </summary>
        private readonly string companyId = string.Empty;
        private readonly string officeNum = string.Empty;

        private readonly int userId;

        /// <summary>
        /// The patient ledger manager.
        /// </summary>
        private readonly PatientOrderWorkInProgressManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientController"/> class.
        /// </summary>
        public PatientOrderController()
        {
            this.manager = new PatientOrderWorkInProgressManager();
            if (!this.User.Identity.IsAuthenticated)
            {
                return;
            }

            var authorizationTicketHelper = new AuthorizationTicketHelper();
            this.companyId = authorizationTicketHelper.GetCompanyId();
            this.officeNum = authorizationTicketHelper.GetPracticeLocationId();
            this.userId = authorizationTicketHelper.GetUserInfo().Id;
        }

        public HttpResponseMessage GetPatientWipOrdersList(int id)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(id);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.manager.GetPatientInProgressOrderList(id, this.companyId, this.officeNum));
            }
            catch (Exception ex)
            {
                var error = "GetPatientWipOrders(" + string.Format("{0}", id) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage DeletePatientIpOrder(string resourceId, int patientId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.manager.DeleteIpOrder(patientId, this.companyId, resourceId));
            }
            catch (Exception ex)
            {
                var error = "DeleteIpOrder(" + string.Format("{0}", resourceId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }
    }
}
