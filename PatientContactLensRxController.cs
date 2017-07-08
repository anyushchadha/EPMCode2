// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatientContactLensRxController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the PatientContactLensRxController.cs type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Patient;

    /// <summary>
    /// The patient contact lens rx controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientContactLensRxController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The eye glasses manager.
        /// </summary>
        private readonly PatientContactLensRxIt2Manager it2Soft;

        /// <summary>
        /// The eye glasses manager.
        /// </summary>
        private readonly PatientHardContactLensRxIt2Manager it2Hard;

        #region public methods

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientContactLensRxController"/> class.
        /// </summary>
        public PatientContactLensRxController()
        {
            this.it2Soft = new PatientContactLensRxIt2Manager();
            this.it2Hard = new PatientHardContactLensRxIt2Manager();
        }

        #region GET

        /// <summary>
        /// The get contact lens rx lite.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="patientId">
        /// The patient Id.
        /// </param>
        /// <param name="examId">
        /// The exam id.
        /// </param>
        /// <param name="isRecheck"></param>
        /// <returns>
        /// The <see cref="PatientContactLensRxLite"/>.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetContactLensRxById(string officeNumber, int patientId, int examId, bool isRecheck)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                PatientContactLensRxLite patientContactLensRx = this.it2Soft.GetContactLensRxById(
                    officeNumber,
                    patientId,
                    examId, 
                    isRecheck);

                if (patientContactLensRx != null)
                {
                    var requestMessage = new List<KeyValuePair<int, DateTime>> { new KeyValuePair<int, DateTime>(examId, DateTime.UtcNow) };
                    MessageTracking.SignalAlSupportTracking(requestMessage, "Viewed");
                }

                return patientContactLensRx != null ? this.Request.CreateResponse(HttpStatusCode.OK, patientContactLensRx) : this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = "GetContactLensRxById(examId=" + examId + ", patientId=" + patientId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get hard contact lens rx by id.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="patientId"></param>
        /// <param name="examId">
        /// The exam id.
        /// </param>
        /// <param name="isRecheck"></param>
        /// <returns>
        /// The <see cref="PatientHardContactLens"/>.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetHardContactLensRxById(string officeNumber, int patientId, int examId, bool isRecheck)
        {
            try
            {
                PatientHardContactLens patientContactLensRx = this.it2Hard.GetHardContactLensRxById(officeNumber, patientId, examId, isRecheck);
                return patientContactLensRx != null
                    ? this.Request.CreateResponse(HttpStatusCode.OK, patientContactLensRx)
                    : this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = "GetHardContactLensRxById(examId=" + examId + ", officeNumber=" + officeNumber + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get styles by manufacturer.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="manufacturerId">
        /// The manufacturer id.
        /// </param>
        /// <param name="isHardLens">
        /// The is hard lens.
        /// </param>
        /// <returns>
        /// The list of option item
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetStylesByManufacturer(string officeNumber, string manufacturerId, string isHardLens)
        {
            try
            {
                List<Lookup> options = this.it2Soft.GetStylesByManufacturer(officeNumber, manufacturerId, isHardLens);
                return options != null ? this.Request.CreateResponse(HttpStatusCode.OK, options) : this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = "GetStylesByManufacturer(manufacturerId=" + manufacturerId + ", officeNumber=" + officeNumber + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get powers by style id.
        /// </summary>
        /// <param name="styleId">
        /// The style id.
        /// </param>
        /// <returns>
        /// The lookup list.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetPowersByStyleId(string styleId)
        {
            try
            {
                List<Lookup> options = this.it2Soft.GetPowersByStyleId(styleId);
                return options != null ? this.Request.CreateResponse(HttpStatusCode.OK, options) : this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = "GetStylesByManufacturer(styleId = " + styleId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get colors by power id.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="powerId">
        /// The power id.
        /// </param>
        /// <returns>
        /// The look up
        /// </returns>
        public HttpResponseMessage GetColorsByPowerId(string officeNumber, string powerId)
        {
            try 
            {
                List<Lookup> options = this.it2Soft.GetColorsByPowerId(officeNumber, powerId);
                return options != null ? this.Request.CreateResponse(HttpStatusCode.OK, options) : 
                    this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = "GetColorsByPowerId(powerId = " + powerId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get colors by power id.
        /// </summary>
        /// <param name="styleId">
        /// The power id.
        /// </param>
        /// <returns>
        /// The look up
        /// </returns>
        public HttpResponseMessage GetColorsByStyleId(string styleId)
        {
            try
            {
                List<Lookup> options = this.it2Soft.GetColorsByStyleId(styleId);
                return options != null ? this.Request.CreateResponse(HttpStatusCode.OK, options) :
                    this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = "GetColorsByStyleId(styleId = " + styleId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get item contact lens sphere by criteria.
        /// </summary>
        /// <param name="powerid">
        /// The power id.
        /// </param>
        /// <param name="clcolorcode">
        /// The color code.
        /// </param>
        /// <returns>
        /// The look up list
        /// </returns>
        public HttpResponseMessage GetItemContactLensSphereByCriteria(string powerid, string clcolorcode)
        {
            try
            {
                List<Lookup> result = this.it2Soft.GetItemContactLensSphereByCriteria(powerid, clcolorcode);
                return result != null ? this.Request.CreateResponse(HttpStatusCode.OK, result) : this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = "GetItemContactLensSphereByCriteria(powerId = " + powerid + "clcolorcode = " + clcolorcode + " )\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get item contact lens cylinder by criteria.
        /// </summary>
        /// <param name="powerid">
        /// The power id.
        /// </param>
        /// <param name="clcolorcode">
        /// The color code.
        /// </param>
        /// <param name="sphere">
        /// The sphere.
        /// </param>
        /// <returns>
        /// The lookup list.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetItemContactLensCylinderByCriteria(string powerid, string clcolorcode, string sphere)
        {
            try
            {
                List<Lookup> result = this.it2Soft.GetItemContactLensCylinderByCriteria(powerid, clcolorcode, sphere);
                return result != null ? this.Request.CreateResponse(HttpStatusCode.OK, result) : this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = string.Format("GetItemContactLensCylinderByCriteria(powerId = {0}, clcolorcode = {1}, sphere = {2} {3} {4})", powerid, clcolorcode, sphere, "\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get item contact lens axis by criteria.
        /// </summary>
        /// <param name="powerid">
        /// The power id.
        /// </param>
        /// <param name="clcolorcode">
        /// The color code.
        /// </param>
        /// <param name="sphere">
        /// The sphere.
        /// </param>
        /// <param name="cylinder">
        /// The cylinder.
        /// </param>
        /// <returns>
        /// The lookup list
        /// </returns>
        public HttpResponseMessage GetItemContactLensAxisByCriteria(
            string powerid,
            string clcolorcode,
            string sphere,
            string cylinder)
        {
            try
            {
                List<Lookup> result = this.it2Soft.GetItemContactLensAxisByCriteria(powerid, clcolorcode, sphere, cylinder);
                return result != null ? this.Request.CreateResponse(HttpStatusCode.OK, result) : this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = string.Format("GetItemContactLensAxisByCriteria(powerId = {0}, clcolorcode = {1}, sphere = {2}, cylinder = {3} {4} {5})", powerid, clcolorcode, sphere, cylinder, "\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get item contact lens add power by criteria.
        /// </summary>
        /// <param name="powerid">
        /// The power id.
        /// </param>
        /// <param name="clcolorcode">
        /// The color code.
        /// </param>
        /// <returns>
        /// The lookup
        /// </returns>
        public HttpResponseMessage GetItemContactLensAddPowerByCriteria(string powerid, string clcolorcode)
        {
            try
            {
                List<Lookup> result = this.it2Soft.GetItemContactLensAddPowerByCriteria(powerid, clcolorcode);
                return result != null ? this.Request.CreateResponse(HttpStatusCode.OK, result) : this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = string.Format("GetItemContactLensAddPowerByCriteria(powerId = {0}, clcolorcode = {1} {2} {3} )", powerid, clcolorcode, "\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        #endregion

        #region PUT

        /// <summary>
        /// The put.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="patientContactLensRx">
        /// The patient contact lens rx.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public HttpResponseMessage Put(string officeNumber, [FromBody] PatientContactLensRxLite patientContactLensRx)
        {
            if (patientContactLensRx == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, string.Empty);
            }

            try
            {
                AccessControl.VerifyUserAccessToPatient(patientContactLensRx.PatientId);
                var result = this.it2Soft.SaveExam(officeNumber, patientContactLensRx);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                var error = string.Format("Put(patientId = {0}, examId = {1} {2} {3} )", patientContactLensRx.PatientId, patientContactLensRx.ExamId, "\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The put.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="patientHardContactLensRx">
        /// The patient contact lens rx.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [HttpPost, HttpPut]
        public HttpResponseMessage SaveHardContactLensRx(string officeNumber, [FromBody] PatientHardContactLens patientHardContactLensRx)
        {
            if (patientHardContactLensRx == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, string.Empty);
            }

            try
            {
                AccessControl.VerifyUserAccessToPatient(patientHardContactLensRx.PatientId);
                var result = this.it2Hard.SaveHardClExam(officeNumber, patientHardContactLensRx);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                var error = string.Format("Put(patientId = {0}, examId = {1} {2} {3} )", patientHardContactLensRx.PatientId, patientHardContactLensRx.ExamId, "\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }
        #endregion

        #endregion
    }
}