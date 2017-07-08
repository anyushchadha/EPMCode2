namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Patient;

    ////using FluentValidation;
    ////using FluentValidation.Internal;
    using IT2.Core;

    using EyeGlassesRx = Eyefinity.PracticeManagement.Model.Patient.PatientEyeGlassesRx;

    /// <summary>
    /// The patient eye glasses rx controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientEyeGlassesRxController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The eye glasses manager.
        /// </summary>
        private readonly PatientEyeGlassesManager eyeGlassesManager;

        #region public methods

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientEyeGlassesRxController"/> class.
        /// </summary>
        public PatientEyeGlassesRxController()
        {
            this.eyeGlassesManager = new PatientEyeGlassesManager();
        }

        #region GET

        /// <summary>The get eye glasses rx by id.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="patientId">The patient Id.</param>
        /// <param name="examId">The exam Id.</param>
        /// <param name="isRecheck"></param>
        /// <returns>The <see cref="PatientEyeGlassesRx"/>.ObjectNotFoundException</returns>
        public HttpResponseMessage GetEyeGlassesRxById(string officeNumber, int patientId, int examId, bool isRecheck)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                if (patientId <= 0)
                {
                    return this.Request.CreateResponse(HttpStatusCode.NotFound, "Patient not found.");
                }

                var result = this.eyeGlassesManager.GetEyeGlassesRxById(officeNumber, patientId, examId, isRecheck);

                if (result != null)
                {
                    var requestMessage = new List<KeyValuePair<int, DateTime>> { new KeyValuePair<int, DateTime>(examId, DateTime.UtcNow) };
                    MessageTracking.SignalAlSupportTracking(requestMessage, "Viewed");
                }

                return result != null ? this.Request.CreateResponse(HttpStatusCode.OK, result) : this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = "GetEyeGlassesRxById(examId=" + examId + ", patientId=" + patientId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        #endregion
        #region PUT
        /// <summary>
        /// The post.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="patientEyeGlassesRx">
        /// The patient eye glasses rx.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public HttpResponseMessage Put(string officeNumber, [FromBody]EyeGlassesRx patientEyeGlassesRx)
        {
            if (patientEyeGlassesRx == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            try
            {
                var patientExam = this.eyeGlassesManager.AlslExamToIt2Exam(officeNumber, patientEyeGlassesRx);
                var examDetailAlsl = new List<PatientExamDetailAlsl>
                {
                    new PatientExamDetailAlsl
                        {   
                            UnderlyingCondition = Convert.ToInt32(patientEyeGlassesRx.RightUlConditionId)
                        },

                    new PatientExamDetailAlsl
                        {   
                                UnderlyingCondition = Convert.ToInt32(patientEyeGlassesRx.LeftUlConditionId)
                        }
                };
                var errors = PatientIt2Manager.ValidateRxExamForCompleteness(examDetailAlsl, patientExam);
                if (errors != null && errors.Count > 0)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, errors);
                }

                var rightValid = this.eyeGlassesManager.ValidateRx(patientEyeGlassesRx, LensRightLeft.Right);
                var leftValid = this.eyeGlassesManager.ValidateRx(patientEyeGlassesRx, LensRightLeft.Left);

                if (!leftValid || !rightValid)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                AccessControl.VerifyUserAccessToPatient(patientEyeGlassesRx.PatientId);
                var examId = this.eyeGlassesManager.SaveExam(patientExam, patientEyeGlassesRx.RightUlConditionId, patientEyeGlassesRx.LeftUlConditionId, patientEyeGlassesRx.HasBeenRechecked, patientEyeGlassesRx.RecheckExamId);
                return this.Request.CreateResponse(HttpStatusCode.OK, examId);
            }
            catch (Exception ex)
            {
                var error = "Put(examId=" + patientEyeGlassesRx.ExamId + ", patientId=" + patientEyeGlassesRx.PatientId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }  
        }
        #endregion
        #endregion
    }
}
