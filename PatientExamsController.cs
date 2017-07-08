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
    using Eyefinity.PracticeManagement.Model.Patient.Exams;

    using IT2.Core;
    using IT2.Core.ABB;
    using IT2.Ioc;
    using IT2.Services;

    using NHibernate.Mapping;

    using PatientExamDetail = Eyefinity.PracticeManagement.Model.Patient.Exams.PatientExamDetail;

    /// <summary>
    ///     The patient controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientExamsController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

         /// <summary>
        /// The office number of the practice's location.
        /// </summary>
        private readonly string practiceLocationId = string.Empty;

        /// <summary>
        /// The company id of the practice.
        /// </summary>
        private readonly string companyId = string.Empty;

        private readonly int userId = 0;

        /// <summary>
        /// The patient ledger manager.
        /// </summary>
        private readonly PatientExamsIt2Manager patientExamsIt2Manager;

        public PatientExamsController(string officeNumber)
        {
            this.practiceLocationId = officeNumber;
            this.patientExamsIt2Manager = new PatientExamsIt2Manager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientController"/> class.
        /// </summary>
        public PatientExamsController()
        {
            this.patientExamsIt2Manager = new PatientExamsIt2Manager();
            if (!this.User.Identity.IsAuthenticated)
            {
                return;
            }

            var authorizationTicketHelper = new AuthorizationTicketHelper();
            this.practiceLocationId = authorizationTicketHelper.GetPracticeLocationId();
            this.companyId = authorizationTicketHelper.GetCompanyId();
            this.userId = authorizationTicketHelper.GetUserInfo().Id;
        }

        public HttpResponseMessage GetPatientExams(int patientId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                var patientExamVm = this.patientExamsIt2Manager.GetUnusedPatientEhrExams(patientId);

                if (patientExamVm != null && patientExamVm.PendingExams != null && patientExamVm.PendingExams.Count > 0)
                {
                    var data = patientExamVm.PendingExams.Select(x => new KeyValuePair<int, DateTime>(x.ExamId, DateTime.UtcNow)).ToList();
                    MessageTracking.SignalAlSupportTracking(data, "Listed");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, patientExamVm);
            }
            catch (Exception ex)
            {
                var error = "GetPatientExams(" + string.Format("{0}", patientId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage GetPatientEhrExams(int patientId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.patientExamsIt2Manager.GetUnusedPatientEhrExams(patientId));
            }
            catch (Exception ex)
            {
                var error = "GetPatientExams(" + string.Format("{0}", patientId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage GetPatientExamById(int examId, int patientId)
        {
            try
            {
                AccessControl.VerifyPatientDataAccess(patientId, 0, examId);
                AccessControl.VerifyUserAccessToPatient(patientId);
                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    this.patientExamsIt2Manager.GetPatientExamById(examId, this.companyId));
            }
            catch (Exception ex)
            {
                var error = "GetPatientExamById(" + string.Format("{0}", patientId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        public HttpResponseMessage GetPatientExamDetail(int patientId, int orderId, int examId)
        {
            try
            {
                AccessControl.VerifyPatientDataAccess(patientId, orderId, examId);
                AccessControl.VerifyUserAccessToPatient(patientId);

                PatientExamDetail patientExamDetail = this.patientExamsIt2Manager.GetNewPatientExamDetail(patientId, this.companyId, examId, orderId);

                if (patientExamDetail != null)
                {
                    var data = new List<KeyValuePair<int, DateTime>> { new KeyValuePair<int, DateTime>(patientExamDetail.ExamId, DateTime.UtcNow) };
                    MessageTracking.SignalAlSupportTracking(data, "Viewed");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, patientExamDetail);
            }
            catch (Exception ex)
            {
                var error = "GetPatientExamDetail(" + string.Format("{0}", patientId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetItemPrice(int itemId)
        {
            string price;
            bool allowZeroPrice;
            
            try
            {
                double priceN = 0;
                price = this.patientExamsIt2Manager.GetItemPriceByOffice(itemId, this.practiceLocationId, out priceN);
            }
            catch (Exception ex)
            {
                var error = "GetItemPriceByCompanyId(" + itemId + ")\n" + ex;
                HandleExceptions.LogExceptions(error, Logger, ex);
                return this.Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }

            try
            {
                allowZeroPrice = this.patientExamsIt2Manager.GetItemAllowZeroPrice(itemId, this.companyId);
            }
            catch (Exception ex)
            {
                var error = "GetItemAllowZeroPrice(" + itemId + ")\n" + ex;
                HandleExceptions.LogExceptions(error, Logger, ex);
                return this.Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
            
            return this.Request.CreateResponse(HttpStatusCode.OK, new { price, allowZeroPrice });
        }

        [HttpPut]
        public HttpResponseMessage SaveExamOrder(PatientExamDetail patientExamDetail, int orderNumber)
        {
            try
            {
                var savedOrderNumber = this.patientExamsIt2Manager.SaveExamOrder(patientExamDetail, orderNumber, this.practiceLocationId, this.userId);
                return this.Request.CreateResponse(HttpStatusCode.OK, savedOrderNumber);
            }
            catch (Exception ex)
            {
                var error = "SaveExamOrder(" + patientExamDetail.PatientId + ")\n" + ex;
                HandleExceptions.LogExceptions(error, Logger, ex);
                return this.Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }

        [HttpDelete]
        public HttpResponseMessage DeletePatientExam(int examId)
        {
            try
            {
                this.patientExamsIt2Manager.DeletePatientExam(examId);
                return this.Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                var error = "Delete(" + string.Format("{0}", examId) + ")\n" + ex;
                HandleExceptions.LogExceptions(error, Logger, ex);
                return this.Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetExamTypes(int diagCodeId)
        {
            try
            {
                var examTypes = this.patientExamsIt2Manager.GetExamTypes(diagCodeId, this.companyId);
                return this.Request.CreateResponse(HttpStatusCode.OK, examTypes);
            }
            catch (Exception ex)
            {
                var error = "GetExamTypes(" + diagCodeId + ")\n" + ex;
                HandleExceptions.LogExceptions(error, Logger, ex);
                return this.Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetDiagCodeMedicalConditions([FromUri] List<int> diagCodeIdList)
        {
            try
            {
                var diagCodeMedicalConditions = this.patientExamsIt2Manager.GetDiagCodeMedicalConditions(diagCodeIdList);
                return this.Request.CreateResponse(HttpStatusCode.OK, diagCodeMedicalConditions);
            }
            catch (Exception ex)
            {
                var error = "GetDiagCodeMedicalConditions()\n" + ex;
                HandleExceptions.LogExceptions(error, Logger, ex);
                return this.Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetDiagnosisCodesByDescription(string description, string diagCodeA, string diagCodeB, string diagCodeC, string diagCodeD, string diagCodeE, string diagCodeF, string diagCodeG, string diagCodeH, string diagCodeI, string diagCodeJ, string diagCodeK, string diagCodeL)
        {
            try
            {
                var diagCodes = this.patientExamsIt2Manager.GetDiagnosisCodesByDescription(this.companyId, description, diagCodeA, diagCodeB, diagCodeC, diagCodeD, diagCodeE, diagCodeF, diagCodeG, diagCodeH, diagCodeI, diagCodeJ, diagCodeK, diagCodeL);
                return this.Request.CreateResponse(HttpStatusCode.OK, diagCodes);
            }
            catch (Exception ex)
            {
                var error = "GetDiagnosisCodesByDescription(" + description + ")\n" + ex;
                HandleExceptions.LogExceptions(error, Logger, ex);
                return this.Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }
    }
}