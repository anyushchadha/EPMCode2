namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Insurance;
    using Eyefinity.PracticeManagement.Model.Patient;
    using Eyefinity.PracticeManagement.Model.Patient.Orders;

    /// <summary>
    ///     The patient controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class ContactLensOrderController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The practice Id.
        /// </summary>
        private readonly string companyId = string.Empty;

        /// <summary>
        /// The office number.
        /// </summary>
        private readonly string officeNumber = string.Empty;

        /// <summary>
        /// The User Id.
        /// </summary>
        private readonly int userId;

        /// <summary>
        /// The patient insurance manager.
        /// </summary>
        private readonly PatientInsuranceManager patientInsuranceManager;

        /// <summary>
        /// The patient ledger manager.
        /// </summary>
        private readonly ContactLensOrderIt2Manager contactLensOrderIt2Manager;

        public ContactLensOrderController(string practiceId, string officeNumber)
        {
            this.companyId = practiceId;
            this.officeNumber = officeNumber;
            this.contactLensOrderIt2Manager = new ContactLensOrderIt2Manager();
            this.patientInsuranceManager = new PatientInsuranceManager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactLensOrderController"/> class.
        /// </summary>
        public ContactLensOrderController()
        {
            this.contactLensOrderIt2Manager = new ContactLensOrderIt2Manager();
            this.patientInsuranceManager = new PatientInsuranceManager();
            if (!this.User.Identity.IsAuthenticated)
            {
                return;
            }

            var authorizationTicketHelper = new AuthorizationTicketHelper();
            this.companyId = authorizationTicketHelper.GetCompanyId();
            this.officeNumber = authorizationTicketHelper.GetPracticeLocationId();
            this.userId = authorizationTicketHelper.GetUserInfo().Id;
        }

        /// <summary>Get contact lens VM.</summary>
        /// <returns>The contactLensVm</returns>
        public HttpResponseMessage GetContactLensVm(int examId)
        {
            var contactLensVm = this.contactLensOrderIt2Manager.GetPatientContactLensVm(examId, this.companyId, this.officeNumber);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { contactLensVm });
        }

        public HttpResponseMessage GetAllContactLensEligibilitiesByPatientId(int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);

            var insurances = new List<PatientInsurance>();
            var patientInsurances = this.patientInsuranceManager.GetValidPatientInsurancesEligibilities(this.officeNumber, patientId, true);
            foreach (var ins in patientInsurances)
            {
                ins.Eligibilities = this.GetEligibilities(ins.Id);
                if (ins.Eligibilities.Eligibilities != null && ins.Eligibilities.Eligibilities.Any())
                {
                    insurances.Add(ins);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { insurances });
        }

        [HttpGet]
        public HttpResponseMessage ValidateContactLensOrderRx(int patientExamId)
        {
            string serverErrorMessage;
            var isValid = this.contactLensOrderIt2Manager.ValidateContactLensOrderRx(patientExamId, this.companyId, this.officeNumber, out serverErrorMessage);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { serverErrorMessage, isValid });
        }

        public EligibilitiesAndConfiguration GetEligibilities(int insuranceId)
        {
            var result = this.patientInsuranceManager.GetEligibilitiesAndConfigurations(insuranceId);

            var eligibilities = result.Eligibilities.ToList();
            eligibilities.RemoveAll(e => !e.IsClElig || e.AuthExpireDate < DateTime.Today || e.HasClFitOrder || e.HasClOrder || e.HasEgLensOrder || e.HasExamOrder || e.HasFrameOrder);
            result.Eligibilities = eligibilities;

            return result;
        }

        public HttpResponseMessage GetAllContactLensRxByPatientId(int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            var patientExams = this.contactLensOrderIt2Manager.GetAllContactLensRxByPatientId(
                patientId,
                this.companyId,
                this.officeNumber);
            return this.Request.CreateResponse(HttpStatusCode.OK, patientExams);
        }

        public HttpResponseMessage GetIncompleteRxByPatientId(int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            return this.Request.CreateResponse(HttpStatusCode.OK, this.contactLensOrderIt2Manager.GetIncompleteRxByPatientId(patientId, this.companyId, this.officeNumber));
        }

        public HttpResponseMessage GetBesResponseForContactLensOrder(
            int patientId,
            int leftLensId,
            int leftLensQuantity,
            int rightLensId,
            int rightLensQuantity,
            int eligibilityId,
            int doctorId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            string serverErrorMessage;
            var besResponse = this.contactLensOrderIt2Manager.GetBesResponseForContactLensOrder(
                patientId,
                leftLensId,
                leftLensQuantity,
                rightLensId,
                rightLensQuantity,
                eligibilityId,
                doctorId,
                this.companyId,
                this.officeNumber,
                out serverErrorMessage);
            if (!string.IsNullOrEmpty(serverErrorMessage))
            {
                Logger.Error(string.Format("GetBesResponseForContactLensOrder(patientId = {0}, {1}", patientId, serverErrorMessage));
            }
            
            return this.Request.CreateResponse(HttpStatusCode.OK, new { serverErrorMessage, besResponse });
        }

        public HttpResponseMessage GetAllAddressList(int shipToType, int patientId)
        {
            var addresses = this.contactLensOrderIt2Manager.GetAllAddressList(this.companyId, this.officeNumber, shipToType, patientId);
            return this.Request.CreateResponse(HttpStatusCode.OK, addresses);
        }

        public HttpResponseMessage GetAllSuppliedByList(string orderType)
        {
            var suppliedByList = this.contactLensOrderIt2Manager.GetAllSuppliedByList(orderType);
            return this.Request.CreateResponse(HttpStatusCode.OK, suppliedByList);
        }

        public HttpResponseMessage GetContactLensOrderDetail(int patientId, int orderNumber, string orderType)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            AccessControl.VerifyPatientDataAccess(patientId, orderNumber, 0);
            var contactLensOrderDetail = this.contactLensOrderIt2Manager.GetContactLensOrderDetail(orderNumber, orderType, this.companyId, this.officeNumber, this.userId);
            return this.Request.CreateResponse(HttpStatusCode.OK, contactLensOrderDetail);
        }

        [HttpPut]
        public HttpResponseMessage SaveContactLensOrder(ContactLensOrderDetail contactLenslOrder, int orderNumber)
        {
            try
            {
                var savedOrderNumber = this.contactLensOrderIt2Manager.SaveContactLensOrder(
                    contactLenslOrder,
                    orderNumber,
                    this.companyId,
                    this.userId);
                return this.Request.CreateResponse(HttpStatusCode.OK, savedOrderNumber);
            }
            catch (Exception ex)
            {
                var error = "SaveContactLensOrder(" + contactLenslOrder.PatientId + ")\n" + ex;
                HandleExceptions.LogExceptions(error, Logger, ex);
                return this.Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }

        [HttpPut]
        public HttpResponseMessage SaveInProgressContactLensOrder(ContactLensOrderDetail detail)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(detail.PatientId);
                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    this.contactLensOrderIt2Manager.SaveInProgressContactLensOrder(detail, this.companyId));
            }
            catch (Exception ex)
            {
                var error = "SaveInProgressContactLensOrder(" + string.Format("{0}", detail.PatientId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage GetInProgressContactLensOrder(int patientId, string awsResourceId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.contactLensOrderIt2Manager.GetInProgressContactLensOrder(this.companyId, patientId, awsResourceId, this.officeNumber));
            }
            catch (Exception ex)
            {
                var error = "GetInProgressContactLensOrder(" + string.Format("{0}", patientId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage GetInProgressOrderSummary(int patientId, string awsResourceId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.contactLensOrderIt2Manager.GetIpOrderSummary(this.companyId, patientId, awsResourceId));
            }
            catch (Exception ex)
            {
                var error = "GetInProgressOrderSummary(" + string.Format("{0}", patientId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage GetContactLensPrice(int patientId, int itemIdLeft, int itemIdRight)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.contactLensOrderIt2Manager.GetContactLensPrice(this.companyId, itemIdLeft, itemIdRight));
            }
            catch (Exception ex)
            {
                var error = "GetContactLensPrice(" + string.Format("{0}", patientId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        [HttpPut]
        public HttpResponseMessage SaveContactLensPrice(ContactLensPrice vm)
        {
            try
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, this.contactLensOrderIt2Manager.SaveContactLensPrice(vm, this.companyId));
            }
            catch (Exception ex)
            {
                var error = "SaveContactLensPrice" + string.Format("{0}", "SaveContactLensPrice") + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }
    }
}