// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppointmentController.cs" company="Eyefinity, Inc.">
// Eyefinity, Inc. - 2013    
// </copyright>
// <summary>
//   Defines the AppointmentController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Appointment;
    using Eyefinity.Enterprise.Business.Common;
    using Eyefinity.Enterprise.Business.Common.Enumerations;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Appointment;
    using Eyefinity.WebScheduler.DataContracts;

    using DhxAppointment = Eyefinity.PracticeManagement.Model.DhxAppointment;
    using Lookup = Eyefinity.PracticeManagement.Model.Lookup;

    /// <summary>
    /// The appointment controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class AppointmentController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The appointment manager.
        /// </summary>
        private readonly AppointmentManager appointmentManager;

        /// <summary>
        /// The practice Id.
        /// </summary>
        private readonly string companyId = string.Empty;

        /// <summary>
        ///     The exam service manager.
        /// </summary>
        private readonly ExamServiceManager examServiceManager;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AppointmentController"/> class.
        /// </summary>
        public AppointmentController()
        {
            if (User.Identity.IsAuthenticated)
            {
                var authorizationTicketHelper = new AuthorizationTicketHelper();
                
                this.companyId = authorizationTicketHelper.GetCompanyId();
            }

            this.appointmentManager = new AppointmentManager();
            this.examServiceManager = new ExamServiceManager();
        }

        #region Public Methods

        [HttpGet]
        public List<Lookup> GetCompanyOffices(string companyId)
        {
            return this.appointmentManager.GetCompanyOffices(companyId);
        }

        [HttpGet]
        public string GetLatestNotesForAppointment(long appointmentId)
        {
            return this.appointmentManager.GetLatestNotesForAppointment(appointmentId);
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="confirmationDate">
        /// The confirmation Date.
        /// </param>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetAppointmentConfirmations(DateTime confirmationDate, string officeNumber, int userId)
        {
            var message = "GetAppointmentConfirmations(confirmationDate=" + confirmationDate + ", officeNumber=" + officeNumber + ", userId=" + userId + ")\n";
            if (!AccessControl.VerifyCorrectOffice(officeNumber, message))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
           
            try
            {
                var confirmationSummaries = this.appointmentManager.GetAppointmentConfirmations(confirmationDate, officeNumber, userId);
                return Request.CreateResponse(HttpStatusCode.OK, confirmationSummaries);
            }
            catch (Exception ex)
            {
                Logger.Error(message + ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The get exam service items.
        /// </summary>
        /// <param name="confirmationDate">
        /// The confirmation date.
        /// </param>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="confirmations">
        /// The confirmations.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPut]
        public HttpResponseMessage UpdateAppointmentConfirmations(DateTime confirmationDate, string officeNumber, int userId, List<AppointmentConfirmation> confirmations)
        {
            var message = "UpdateAppointmentConfirmations(ConfirmationDate=" + confirmationDate + ", OfficeNumber=" + officeNumber + ", UserId=" + userId + ")\n";
            if (!AccessControl.VerifyCorrectOffice(officeNumber, message))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            try
            {
                this.appointmentManager.UpdateAppointmentConfirmations(confirmationDate, officeNumber, userId, confirmations);
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                Logger.Error(message + ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The get appointment details by id.
        /// </summary>
        /// <param name="appointmentId">
        /// The appointment id.
        /// </param>
        /// <param name="locationNum">
        /// The location number.
        /// </param>
        /// <param name="resourceId">
        /// The resource id.
        /// </param>
        /// <param name="patientId">
        /// The patient id.
        /// </param>
        /// <param name="apptStartDate">
        /// The appointment start date.
        /// </param>
        /// <param name="apptDuration">
        /// The appointment duration.
        /// </param>
        /// <param name="officeId">
        /// The office id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetAppointmentDetailsById(
            int appointmentId,
            int locationNum,
            int resourceId,
            int patientId,
            string apptStartDate,
            string apptDuration,
            string officeId)
        {
            var message = "GetAppointmentDetailsById(appointmentId=" + appointmentId + ", locationNum=" + locationNum + ", resourceId=" + resourceId
                    + "patientId=" + patientId + ", apptStartDate=" + apptStartDate + ", apptDuration=" + apptDuration + ", officeId=" + officeId + ")\n";
            if (!AccessControl.VerifyCorrectOffice(officeId, message))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            try
            {
                var apptDetail = this.appointmentManager.GetAppointmentDetailsById(
                    appointmentId,
                    locationNum,
                    resourceId,
                    patientId,
                    Convert.ToDateTime(apptStartDate),
                    apptDuration,
                    officeId);
                return Request.CreateResponse(HttpStatusCode.OK, apptDetail);
            }
            catch (Exception ex)
            {
                Logger.Error(message + ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>The get appointment services.</summary>
        /// <param name="officeId">The office id.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public HttpResponseMessage GetAppointmentServices(int officeId)
        {
            var apptServices = this.appointmentManager.GetAppointmentServices(officeId);
            return Request.CreateResponse(HttpStatusCode.OK, apptServices);
        }

        /// <summary>The get open appointments.</summary>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public HttpResponseMessage GetOpenAppointments([FromUri]OpenAppointmentCriteria criteria)
        {
            var openAppointments = this.appointmentManager.GetOpenAppointments(criteria);
            var appointments = (from item in openAppointments
                                orderby item.StartTime
                                select new OpenAppointments
                                {
                                     ResourceId = item.ResourceId,
                                     ResourceName = item.ResourceName,
                                     StartTime = item.StartTime == null ? string.Empty : ((DateTime)item.StartTime).ToString("M/d/yyyy h:mm:ss tt"),
                                     EndTime = item.EndTime == null ? string.Empty : ((DateTime)item.EndTime).ToString("M/d/yyyy h:mm:ss tt")
                                }).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, appointments);
        }

        /// <summary>
        /// The get exam service items.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="serviceId">
        /// The service Id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetExamServiceItems(string officeNumber, int serviceId)
        {
            var message = "GetExamServiceItems(officeNumber=" + officeNumber + ", serviceId=" + serviceId + ")\n ";
            if (!AccessControl.VerifyCorrectOffice(officeNumber, message))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            try
            {
                var firstOrDefault =
                    this.examServiceManager.GetExamServiceItems(officeNumber, null, null, true)
                        .FirstOrDefault(i => i.ItemId == serviceId);
                return this.Request.CreateResponse(HttpStatusCode.OK, firstOrDefault != null ? new { ExamServiceItems = firstOrDefault.ExamDurationMinutes } : new { ExamServiceItems = (int)0 });
            }
            catch (Exception ex)
            {
                Logger.Error(message + ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The get all resources.
        /// </summary>
        /// <param name="userId">
        /// The employee Id.
        /// </param>
        /// <param name="officeNum">
        /// The office Number.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetAllResources(int userId, string officeNum)
        {
            var message = "GetAllResources(userId=" + userId + ", officeNum=" + officeNum + ")\n ";
            if (!AccessControl.VerifyCorrectOffice(officeNum, message))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            try
            {
                var items = this.appointmentManager.GetAllResources(userId, officeNum);
                return Request.CreateResponse(HttpStatusCode.OK, items);
            }
            catch (Exception ex)
            {
                Logger.Error(message + ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The get resources.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="employeeId">
        /// The employee id.
        /// </param>
        /// <returns>
        /// The List of resources for this employee/>.
        /// </returns>
        [HttpGet]
        public List<Resource> GetResources(int officeNumber, int userId)
        {
            ////var message = "GetResources(officeNum=" + officeNumber + ", userId = " + userId + ")\n ";
            ////if (!AccessControl.VerifyCorrectOffice(officeNumber, message))
            ////{
            ////    return Request.CreateResponse(HttpStatusCode.Forbidden);
            ////}

            ////try
            ////{
            ////    var items = this.appointmentManager.GetAllResources(userId, officeNum);
            ////    return Request.CreateResponse(HttpStatusCode.OK, items);
            ////}
            ////catch (Exception ex)
            ////{
            ////    Logger.Error(message + ex);
            ////    return Request.CreateResponse(HttpStatusCode.InternalServerError);
            ////}
            return this.appointmentManager.GetResourcesByUserId(officeNumber, userId).ToList();
        }

        /// <summary>
        /// The update all resources.
        /// </summary>
        /// <param name="resourceslist">
        /// The resources list.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="officeNum">
        /// The office number.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPut]
        public HttpResponseMessage UpdateAllResources(object resourceslist, int userId, string officeNum)
        {
            var message = "UpdateAllResources(userId=" + userId + ", officeNum=" + officeNum + ")\n ";
            if (!AccessControl.VerifyCorrectOffice(officeNum, message))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            try
            {
                var resources = new List<Resource>();
                var enumerable = resourceslist as IEnumerable<object>;
                if (enumerable != null)
                {
                    foreach (dynamic item in enumerable)
                    {
                        resources.AddRange(
                            new[]
                                {
                                    new Resource
                                        {
                                            Active = item.Active,
                                            DisplayName = item.DisplayName,
                                            Id = item.Id,
                                            ViewOrder = item.ViewOrder
                                        }
                                });
                    }
                }

                this.appointmentManager.UpdateAllResources(resources, userId, officeNum);
                return Request.CreateResponse(HttpStatusCode.OK, resources.Where(r => (r.Active == true)).ToList());
            }
            catch (Exception ex)
            {
                Logger.Error(message + ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The get appointment confirmations configuration.
        /// </summary>
        /// <param name="officeNum">
        /// The office Number.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetAppointmentConfirmationsConfiguration(string officeNum)
        {
            var message = "GetAppointmentConfirmationsConfiguration(officeNum=" + officeNum + ")\n ";
            if (!AccessControl.VerifyCorrectOffice(officeNum, message))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            try
            {
                var config = this.appointmentManager.GetAppointmentConfirmationsConfiguration(officeNum);
                return Request.CreateResponse(HttpStatusCode.OK, config);
            }
            catch (Exception ex)
            {
                Logger.Error(message + ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="appointmentId">
        /// The appointment id.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpDelete]
        public HttpResponseMessage Delete(int appointmentId, int userId)
        {
            var employeeId = this.appointmentManager.GetEmployeeIdByUserId(userId);
            try
            {
                this.appointmentManager.DeleteAppointment(appointmentId, employeeId.GetValueOrDefault());
            }
            catch (Exception e)
            {
                Logger.Error("Appointment - Delete(appointmentId=" + appointmentId + "userId=" + userId + ")\n" + e);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Appointment Deleted");
        }

        /// <summary>
        /// The cancel appointment.
        /// </summary>
        /// <param name="appointmentId">
        /// The appointment id.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage CancelAppointment(int appointmentId, int userId)
        {
            try
            {
                this.appointmentManager.CancelAppointment(appointmentId, userId);
            }
            catch (Exception e)
            {
                Logger.Error("Appointment - Cancel(appointmentId=" + appointmentId + "userId=" + userId + ")\n" + e);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Appointment Canceled");
        }

        /// <summary>
        /// The update appointment status.
        /// </summary>
        /// <param name="appointmentStatus">
        /// The appointment status.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="appointmentId">
        /// The appointment id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage UpdateAppointmentStatus(int appointmentStatus, int userId, int appointmentId)
        {
            try
            {
                this.appointmentManager.UpdateAppointmentStatus(appointmentId, appointmentStatus, userId);
            }
            catch (Exception e)
            {
                Logger.Error("Appointment - UpdateStatus(appointmentStatus=" + appointmentStatus + "userId=" + userId + " appointmentId=" + appointmentId + ")\n" + e);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Appointment Updated");
        }

        /// <summary>
        /// The validate appointment.
        /// </summary>
        /// <param name="dhxAppointment">
        /// The DHX appointment.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="updateType">
        /// The update type.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage ValidateChangedAppointment(DhxAppointment dhxAppointment, long userId, AppointmentUpdateType updateType)
        {
            try 
            {
                var result = this.appointmentManager.ValidateChangedAppointment(userId, updateType, dhxAppointment);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Logger.Error("ValidateChangedAppointment(dhxAppointment.id=" + dhxAppointment.id + ", userId=" + userId + ", updateType=" + updateType + ")\n" + ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The save validated appointment.
        /// </summary>
        /// <param name="dhxAppointment">
        /// The DHX appointment.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="updateType">
        /// The update type.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPut]
        public HttpResponseMessage SaveValidatedAppointment(DhxAppointment dhxAppointment, long userId, AppointmentUpdateType updateType)
        {
            try
            {
                var result = this.appointmentManager.SaveValidatedChangedAppointment(userId, updateType, dhxAppointment);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Logger.Error("SaveValidatedAppointment(dhxAppointment.id=" + dhxAppointment.id + ", userId=" + userId + ", updateType=" + updateType + ")\n" + ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        #endregion //// Public Methods
    }
}
