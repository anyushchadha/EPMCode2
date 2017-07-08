namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Insurance;
    using Eyefinity.PracticeManagement.Model.Patient;
    using Eyefinity.PracticeManagement.Model.Patient.Orders;

    /// <summary>
    ///     The Eyeglass Order controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class EyeglassOrderController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The company Id.
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
        private readonly EyeglassOrderIt2Manager eyeglassOrderIt2Manager;

        /// <summary>
        /// The patient ledger manager.
        /// </summary>
        private readonly EyeglassLensIt2Manager eyeglassLensIt2Manager;

        public EyeglassOrderController(string companyId, string officeNumber)
        {
            this.companyId = companyId;
            this.officeNumber = officeNumber;
            this.eyeglassOrderIt2Manager = new EyeglassOrderIt2Manager();
            this.patientInsuranceManager = new PatientInsuranceManager();
            this.eyeglassLensIt2Manager = new EyeglassLensIt2Manager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeglassOrderController"/> class.
        /// </summary>
        public EyeglassOrderController()
        {
            this.eyeglassOrderIt2Manager = new EyeglassOrderIt2Manager();
            this.patientInsuranceManager = new PatientInsuranceManager();
            this.eyeglassLensIt2Manager = new EyeglassLensIt2Manager();
            if (!this.User.Identity.IsAuthenticated)
            {
                return;
            }

            var authorizationTicketHelper = new AuthorizationTicketHelper();
            this.companyId = authorizationTicketHelper.GetCompanyId();
            this.officeNumber = authorizationTicketHelper.GetPracticeLocationId();
            this.userId = authorizationTicketHelper.GetUserInfo().Id;
        }

        #region Public Methods

        [HttpPut]
        public HttpResponseMessage SaveInProgressEyeglassOrder(EyeglassOrderDetail detail)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(detail.PatientId);
                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    this.eyeglassOrderIt2Manager.SaveInProgressEyeglassOrder(detail, this.companyId));
            }
            catch (Exception ex)
            {
                var error = "SaveInProgressContactLensOrder(" + string.Format("{0}", detail.PatientId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage GetInProgressOrderSummary(int patientId, string awsResourceId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.eyeglassOrderIt2Manager.GetIpOrderSummary(this.companyId, patientId, awsResourceId));
            }
            catch (Exception ex)
            {
                var error = "GetInProgressOrderSummary(" + string.Format("{0}", patientId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage GetInProgressEyeglassOrder(int patientId, string awsResourceId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.eyeglassOrderIt2Manager.GetInProgressEyeglassOrder(this.companyId, patientId, awsResourceId, this.officeNumber, this.userId));
            }
            catch (Exception ex)
            {
                var error = "GetInProgressContactLensOrder(" + string.Format("{0}", patientId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage GetAllEyeglassRxByPatientId(int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            var patientExams = this.eyeglassOrderIt2Manager.GetAllEyeglassRxByPatientId(
                patientId,
                this.companyId,
                this.officeNumber);
            return this.Request.CreateResponse(HttpStatusCode.OK, patientExams);
        }

        public HttpResponseMessage GetAllEyeglassRxDetailsByPatientId(int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            var patientExams = this.eyeglassOrderIt2Manager.GetAllEyeglassRxDetailsByPatientId(
                patientId,
                this.companyId,
                this.officeNumber);
            return this.Request.CreateResponse(HttpStatusCode.OK, patientExams);
        }

        public HttpResponseMessage GetConvertedEyeglassRxDetailsById(int patientId, int examId, int conversionType)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            var patientExam = this.eyeglassOrderIt2Manager.GetConvertedEyeglassRxDetailsById(
                patientId,
                examId,
                conversionType,
                this.companyId,
                this.officeNumber);
            return this.Request.CreateResponse(HttpStatusCode.OK, patientExam);
        }

        public HttpResponseMessage GetIncompleteRxByPatientId(int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            return this.Request.CreateResponse(HttpStatusCode.OK, this.eyeglassOrderIt2Manager.GetIncompleteRxByPatientId(patientId, this.companyId, this.officeNumber));
        }

        public HttpResponseMessage GetValidEyeglassEligibilitiesByPatientId(int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);

            var insurances = this.patientInsuranceManager.GetValidPatientInsurancesEligibilities(this.officeNumber, patientId, true);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { insurances });
        }

        public HttpResponseMessage GetEgExtrasByCompany(int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            var extras = this.eyeglassOrderIt2Manager.GetEgExtrasByCompany(this.companyId);
            return this.Request.CreateResponse(HttpStatusCode.OK, extras);
        }

        public HttpResponseMessage GetFrameAndLensExtrasByCompany(int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            var extras = this.eyeglassOrderIt2Manager.GetFrameAndLensExtrasByCompany(this.companyId);
            return this.Request.CreateResponse(HttpStatusCode.OK, extras);
        }

        public HttpResponseMessage GetEgLabsByCompany(int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            var extras = this.eyeglassOrderIt2Manager.GetLabsByOffice(this.officeNumber);
            return this.Request.CreateResponse(HttpStatusCode.OK, extras);
        }

        public HttpResponseMessage GetAllAddressList(int shipToType, int patientId)
        {
            var addresses = this.eyeglassOrderIt2Manager.GetAllAddressList(this.companyId, this.officeNumber, shipToType, patientId);
            return this.Request.CreateResponse(HttpStatusCode.OK, addresses);
        }

        public HttpResponseMessage GetCopyEyeglassOrderDetail(int patientId, int copyOrderNumber)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            AccessControl.VerifyPatientDataAccess(patientId, copyOrderNumber, 0);
            var eyeglassOrderDetail = this.eyeglassOrderIt2Manager.GetCopyEyeglassOrderDetail(copyOrderNumber, this.companyId, this.officeNumber, this.userId);
            return this.Request.CreateResponse(HttpStatusCode.OK, eyeglassOrderDetail);
        }

        public HttpResponseMessage GetEyeglassOrderDetail(int patientId, int orderNumber)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            AccessControl.VerifyPatientDataAccess(patientId, orderNumber, 0);
            var eyeglassOrderDetail = this.eyeglassOrderIt2Manager.GetEyeglassOrderDetail(orderNumber, this.companyId, this.officeNumber, this.userId);
            return this.Request.CreateResponse(HttpStatusCode.OK, eyeglassOrderDetail);
        }

        [HttpPut]
        public HttpResponseMessage SaveEyeglassOrder(EyeglassOrderDetail eyeglassOrder, int orderNumber)
        {
            try
            {
                int remakeMessageType;
                var savedOrderNumber = this.eyeglassOrderIt2Manager.SaveEyeglassOrder(
                    eyeglassOrder,
                    orderNumber,
                    this.companyId,
                    this.userId,
                    out remakeMessageType);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { savedOrderNumber, remakeMessageType });
            }
            catch (Exception ex)
            {
                var error = "SaveEyeglassOrder(" + eyeglassOrder.PatientId + ")\n" + ex;
                HandleExceptions.LogExceptions(error, Logger, ex);
                return this.Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }

        public HttpResponseMessage GetEyeglassOrderLensDetail(int patientId, int examId, int orderId, bool isMultiFocal, bool isVsp)
        {
            try
            {
                AccessControl.VerifyPatientDataAccess(patientId, orderId, examId);
                AccessControl.VerifyUserAccessToPatient(patientId);

                var eyeglassOrderLenses = this.eyeglassOrderIt2Manager.GetEyeglassOrderLensDetail(this.companyId, isMultiFocal, isVsp);

                return this.Request.CreateResponse(HttpStatusCode.OK, eyeglassOrderLenses);
            }
            catch (Exception ex)
            {
                var error = "GetEyeglassOrderLensDetail(" + patientId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage GetAdditionalCoatingItem(int coatingId)
        {
            var additionalCoating = this.eyeglassOrderIt2Manager.GetAdditionalCoatingItem(coatingId, this.companyId);
            if (additionalCoating != null)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { additionalCoating.ID, additionalCoating.CompanyItemName });
            }

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage GetLensMaterials(int lensType)
        {
            var lensMaterials = this.eyeglassOrderIt2Manager.GetLensMaterials(lensType);
            return this.Request.CreateResponse(HttpStatusCode.OK, lensMaterials);
        }

        public HttpResponseMessage GetLensStyles(int lensTypeId, int lensMaterialId, bool isVsp, bool iofOnly)
        {
            var lensStyles = this.eyeglassOrderIt2Manager.GetLensStyles(this.companyId, lensTypeId, lensMaterialId, isVsp, iofOnly);
            return this.Request.CreateResponse(HttpStatusCode.OK, lensStyles);
        }

        public HttpResponseMessage GetAllLensStyles(int lensTypeId, int lensMaterialId, bool isVsp, bool isIof)
        {
            var lensStyles = this.eyeglassOrderIt2Manager.GetAllLensStyles(lensTypeId, lensMaterialId, isVsp, isIof);
            return this.Request.CreateResponse(HttpStatusCode.OK, lensStyles);
        }

        public HttpResponseMessage GetLensColors(int lensTypeId, int lensMaterialId, int lensStyleId, bool isVsp)
        {
            var lensColors = this.eyeglassOrderIt2Manager.GetLensColors(this.companyId, lensTypeId, lensMaterialId, lensStyleId, isVsp);
            return this.Request.CreateResponse(HttpStatusCode.OK, lensColors);
        }

        public HttpResponseMessage GetEyeglassLensAttributes(int itemId)
        {
            try
            {
                var lensAttributes = this.eyeglassLensIt2Manager.GetLensAttributesToPrice(itemId, this.companyId);
                return this.Request.CreateResponse(HttpStatusCode.OK, lensAttributes);
            }
            catch (Exception ex)
            {
                var error = "GetEyeglassLensAttributes(" + itemId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage GetMfgCoatings(int lensTypeId, int lensMaterialId, int lensStyleId, int lensColorId, bool isVsp)
        {
            string additionalColor = string.Empty;
            var lensMfgCoatings = this.eyeglassOrderIt2Manager.GetMfgCoatings(this.companyId, lensTypeId, lensMaterialId, lensStyleId, lensColorId, isVsp, out additionalColor);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { lensMfgCoatings, additionalColor });
        }

        public HttpResponseMessage GetTintColors(string tintGroup, bool isVsp)
        {
            var tintColors = this.eyeglassOrderIt2Manager.GetTintColors(this.companyId, tintGroup, isVsp);
            return this.Request.CreateResponse(HttpStatusCode.OK, tintColors);
        }

        public HttpResponseMessage GetLensEdging(bool isVsp)
        {
            var edgings = this.eyeglassOrderIt2Manager.GetLensEdging(this.companyId, isVsp);
            return this.Request.CreateResponse(HttpStatusCode.OK, edgings);
        }

        public HttpResponseMessage GetLensByAttributes(int lensTypeId, int lensMaterialId, int lensStyleId, int lensColorId, int lensMfgCoating)
        { 
            var lens = this.eyeglassOrderIt2Manager.GetLensByAttributes(this.companyId, lensTypeId, lensMaterialId, lensStyleId, lensColorId, lensMfgCoating);
            return this.Request.CreateResponse(HttpStatusCode.OK, lens);
        }

        [HttpGet]
        public HttpResponseMessage GetPriceByItemId([FromUri]EyeglassOrderEntityIds entityIds)
        {
            var entities = this.eyeglassOrderIt2Manager.GetPriceByItemId(this.officeNumber, entityIds);
            return this.Request.CreateResponse(HttpStatusCode.OK, entities);
        }

        [HttpGet]
        public HttpResponseMessage GetNonInsurancePriceByItemIds([FromUri]EyeglassOrderEntityIds entityIds)
        {
            var charges = this.eyeglassOrderIt2Manager.GetNonInsurancePriceByItemIds(this.officeNumber, this.companyId, entityIds);
            return this.Request.CreateResponse(HttpStatusCode.OK, charges);
        }

        [HttpGet]
        public HttpResponseMessage GetBesResponseForEyeglassOrder(int patientId, int eligibilityId, int doctorId, string labNum, int patientExamId, int rightLensId, int leftLensId, [FromUri]EyeglassOrderEntityIds entityIds)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            string serverErrorMessage;
            var besResponse = this.eyeglassOrderIt2Manager.GetBesResponseForEyeglassOrder(
                patientId, 
                eligibilityId,
                doctorId,
                labNum,
                patientExamId,
                rightLensId,
                leftLensId,
                entityIds,
                this.companyId,
                this.officeNumber,
                out serverErrorMessage);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { serverErrorMessage, besResponse });
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage SaveLensAttributePricingAndMapping(int lensId, List<EyeglassLensAttribute> itemsToPrice)
        {
            if (itemsToPrice != null)
            {
                foreach (var eyeglassLensAttribute in itemsToPrice)
                {
                    if (eyeglassLensAttribute.Price.Equals(0.0))
                    {
                        eyeglassLensAttribute.AllowZeroPrice = true;
                    }
                }
            }

            var it2Lens = this.eyeglassLensIt2Manager.SaveLensPricingAndMapping(lensId, itemsToPrice, this.companyId);
            var lens = this.eyeglassOrderIt2Manager.ConvertToEyeglassOrderLens(it2Lens);
            return Request.CreateResponse(HttpStatusCode.OK, lens);
        }

        #endregion Public Methods

        #region Private Methods

        #endregion Private Methods
    }
}