// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppointmentController.cs" company="Eyefinity, Inc.">
// Eyefinity, Inc. - 2013    
// </copyright>
// <summary>
//   Defines the QuickListController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Common;
    using Common.Api;
    using Enterprise.Business.Home;

    /// <summary>
    /// The appointment controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class QuickListController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
 
        /// <summary>
        /// The appointment manager.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private readonly QuickListManager quickListManager;

        /// <summary>
        /// The practice Id.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        ////private readonly string practiceLocationId = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppointmentController"/> class.
        /// </summary>
        public QuickListController()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return;
            }

            ////var authorizationTicketHelper = new AuthorizationTicketHelper();
            ////practiceLocationId = authorizationTicketHelper.GetPracticeLocationId();

            this.quickListManager = new QuickListManager();
        }

        #region Public Methods

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetTodayPatients(string officeNumber, int userId)
        {
            var message = "GetTodayPatients( officeNumber=" + officeNumber + ", userId=" + userId + ")\n";
            if (!AccessControl.VerifyCorrectOffice(officeNumber, message))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            try
            {
                var quicklistSummaries = this.quickListManager.GetPatientQuickList(officeNumber);
                var quicklistLocations = this.quickListManager.GetPatientLocations(officeNumber);
                quicklistLocations.Insert(0, new IT2.Core.PatientLocations() { LocationId = 0, Description = "Select", OfficeNum = officeNumber, IsLocationInUse = false });
                return Request.CreateResponse(HttpStatusCode.OK, new { quicklistSummaries, quicklistLocations });
            }
            catch (Exception ex)
            {
                var msg =
                   $"GetTodayPatients(officeNumber =  {officeNumber}, userId = {userId}, {ex}";
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>
        /// The get exam service items.
        /// </summary>
        /// <param name="officeNumber"></param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="patientAppointment"></param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPut]
        public HttpResponseMessage SaveTodayApptPatient(string officeNumber, int userId, Model.QuickList patientAppointment)
        {
            var message = "SaveTodayApptPatient(OfficeNumber=" + officeNumber + ", UserId=" + userId + ")\n";
            if (!AccessControl.VerifyCorrectOffice(officeNumber, message))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            try
            {
                this.quickListManager.SaveTodayApptPatient(
                    officeNumber, 
                    userId,
                    (int)patientAppointment.AppointmentId, 
                    patientAppointment.PatientId,
                    patientAppointment.SelectedLocationOrStatus);

                var quicklistSummaries = this.quickListManager.GetPatientQuickList(officeNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { quicklistSummaries });
            }
            catch (Exception ex)
            {
                var msg =
                    $"SaveTodayApptPatient(officeNumber =  {officeNumber}, userId = {userId}, locationOrStatus= {patientAppointment.SelectedLocationOrStatus}, {ex}";
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }
       
        #endregion //// Public Methods
    }
}
