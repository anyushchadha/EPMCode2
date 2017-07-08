// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatientController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   The patient controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.Enterprise.Business.Common;
    using Eyefinity.Enterprise.Business.Miscellaneous;
    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Common.It2Converters;
    using Eyefinity.PracticeManagement.Model.Patient;
    using Eyefinity.PracticeManagement.Model.ViewModel;
    using Eyefinity.WebScheduler.DataContracts;
    using FeatureManager.Services;
    using IT2.Core;
    using IT2.Core.Feature;
    using IT2.DataAccess.Utility;
    using IT2.Ioc;
    using IT2.Services;
    using NHibernate.Util;

    using SchedulerWS;

    using PatientRecallDetail = Eyefinity.PracticeManagement.Model.Patient.PatientRecallDetail;
    using PatientRecallHistory = Eyefinity.PracticeManagement.Model.Patient.PatientRecallHistory;
    using PatientRecallResult = Eyefinity.PracticeManagement.Model.Patient.PatientRecalls;
    using PatientSearchCriteria = Eyefinity.PracticeManagement.Model.Patient.PatientSearchCriteria;

    /// <summary>
    ///     The patient controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The patient services.
        /// </summary>
        private readonly IPatientServices patientServices;

        /// <summary>
        /// The office services.
        /// </summary>
        private readonly IOfficeServices officeServices;

        /// <summary>
        /// The Scheduler WS client.
        /// </summary>
        private readonly SchedulerWS schedulerClient;

        /// <summary>
        /// The recall services.
        /// </summary>
        private readonly IRecallServices recallServices;

        /// <summary>
        /// The exam services.
        /// </summary>
        private readonly IExamServices examServices;

        /// <summary>
        /// The practice Id.
        /// </summary>
        private readonly string practiceLocationId = string.Empty;

        /// <summary>
        /// The lookup services.
        /// </summary>
        private readonly ILookupServices lookupServices;

        /// <summary>
        /// The lookup services.
        /// </summary>
        private readonly IClaimServices claimServices;

        /// <summary>
        /// The order services.
        /// </summary>
        private readonly IOrderServices orderServices;

        /// <summary>The employee services.</summary>
        private readonly IEmployeeServices employeeServices;

        /// <summary>Feature manager services</summary>
        private readonly IFeatureManagerService featureManager;

        /// <summary>
        /// The patient ledger manager.
        /// </summary>
        private readonly PatientDemographicsIt2Manager patientDemographicsIt2Manager;

        /// <summary>
        /// The patient ledger manager.
        /// </summary>
        private readonly PatientInsuranceManager patientInsuranceManager;

        #region Mock Data
        /*
        /// <summary>
        ///     The patient mock data.
        /// </summary>
        private readonly PatientSearchResult[] patients = new[]
                                                              {
                                                                  new PatientSearchResult { PatientId = 1,  FirstName = "Joe",          LastName = "Smith", DateOfBirth = DateTime.Now, Age = 32, Address = "123 South Main St., Anytown, PA 12345", Phone = "(949) 555-1212" },
                                                                  new PatientSearchResult { PatientId = 2,  FirstName = "James",        LastName = "Smith", DateOfBirth = DateTime.Now, Age = 39, Address = "123 South Main St., Anytown, PA 12345", Phone = "(949) 555-1212" },
                                                                  new PatientSearchResult { PatientId = 3,  FirstName = "Frank",        LastName = "Smith", DateOfBirth = DateTime.Now, Age = 35, Address = "123 South Main St., Anytown, PA 12345", Phone = "(949) 555-1212" },
                                                                  new PatientSearchResult { PatientId = 4,  FirstName = "Miliho'omalu", LastName = "Smith", DateOfBirth = DateTime.Now, Age = 52, Address = "123 South Main St., Anytown, PA 12345", Phone = "(949) 555-1212" },
                                                                  new PatientSearchResult { PatientId = 5,  FirstName = "Ikaika",       LastName = "Smith", DateOfBirth = DateTime.Now, Age = 32, Address = "123 South Main St., Anytown, PA 12345", Phone = "(949) 555-1212" },
                                                                  new PatientSearchResult { PatientId = 6,  FirstName = "Pulelehua",    LastName = "Smith", DateOfBirth = DateTime.Now, Age = 92, Address = "123 South Main St., Anytown, PA 12345", Phone = "(949) 555-1212" },
                                                                  new PatientSearchResult { PatientId = 7,  FirstName = "Sidney",       LastName = "Smith", DateOfBirth = DateTime.Now, Age = 12, Address = "123 South Main St., Anytown, PA 12345", Phone = "(949) 555-1212" },
                                                                  new PatientSearchResult { PatientId = 8,  FirstName = "Lisa",         LastName = "Smith", DateOfBirth = DateTime.Now, Age = 42, Address = "123 South Main St., Anytown, PA 12345", Phone = "(949) 555-1212" },
                                                                  new PatientSearchResult { PatientId = 9,  FirstName = "Pauline",      LastName = "Smith", DateOfBirth = DateTime.Now, Age = 82, Address = "123 South Main St., Anytown, PA 12345", Phone = "(949) 555-1212" },
                                                                  new PatientSearchResult { PatientId = 10, FirstName = "Alfred",       LastName = "Smith", DateOfBirth = DateTime.Now, Age = 42, Address = "123 South Main St., Anytown, PA 12345", Phone = "(949) 555-1212" }
                                                              };
*/
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientController"/> class.
        /// </summary>
        /// <param name="patientServices">
        /// The patient services.
        /// </param>
        /// <param name="officeServices">
        /// The office services.
        /// </param>
        /// <param name="recallServices">
        /// The recall services.
        /// </param>
        /// <param name="examServices">
        /// The exam services.
        /// </param>
        /// <param name="lookupServices">
        /// The lookup services.
        /// </param>
        /// <param name="orderServices">
        /// The order services.
        /// </param>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        public PatientController(
            IPatientServices patientServices,
            IOfficeServices officeServices,
            IRecallServices recallServices,
            IExamServices examServices,
            ILookupServices lookupServices,
            IOrderServices orderServices,
            IClaimServices claimServices,
            IFeatureManagerService featureServices,
            string officeNumber)
        {
            this.patientServices = patientServices;
            this.officeServices = officeServices;
            this.schedulerClient = new SchedulerWS();
            this.recallServices = recallServices;
            this.examServices = examServices;
            this.lookupServices = lookupServices;
            this.orderServices = orderServices;
            this.practiceLocationId = officeNumber;
            this.claimServices = claimServices;
            this.featureManager = featureServices;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientController"/> class.
        /// </summary>
        public PatientController()
        {
            this.patientServices = Container.Resolve<IPatientServices>();
            this.officeServices = Container.Resolve<IOfficeServices>();
            this.claimServices = Container.Resolve<IClaimServices>();
            this.schedulerClient = new SchedulerWS();
            this.recallServices = Container.Resolve<IRecallServices>();
            this.examServices = Container.Resolve<IExamServices>();
            this.lookupServices = Container.Resolve<ILookupServices>();
            this.orderServices = Container.Resolve<IOrderServices>();
            this.employeeServices = Container.Resolve<IEmployeeServices>();
            this.patientDemographicsIt2Manager = new PatientDemographicsIt2Manager();
            this.patientInsuranceManager = new PatientInsuranceManager();

            if (!this.User.Identity.IsAuthenticated)
            {
                return;
            }

            var authorizationTicketHelper = new AuthorizationTicketHelper();
            this.practiceLocationId = authorizationTicketHelper.GetPracticeLocationId();
            this.featureManager = Container.Resolve<IFeatureManagerService>();
        }

        #region Patient Search Public

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <param name="id">
        /// The patient id.
        /// </param>
        /// <returns>
        /// The found patient.
        /// </returns>
        public PatientSearchResult Get(string officeNumber, int id)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(officeNumber);
            var companyId = OfficeHelper.GetCompanyId(this.officeServices, officeNumber);
            var result = this.patientServices.GetPatientLiteByID(companyId, id);

            if (result != null)
            {
                return SearchResultFromPatientLite(result);
            }

            return null;
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The patient
        /// </returns>
        public HttpResponseMessage Get(int id)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(id);
                var result = PatientManager.GetPatientByPatientId(id);
                return result == null ? this.Request.CreateResponse(HttpStatusCode.NotFound) : this.Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                var msg = "Get( patientId=" + id + ")\n" + e;
                return HandleExceptions.LogExceptions(msg, Logger, e);
            }
        }

        /// <summary>
        /// The get patient by id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Patient
        /// </returns>
        public HttpResponseMessage GetPatientById(int id)
        {
            return this.Get(id);
        }

        /// <summary>The get.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="dateOfBirth">The date of birth.</param>
        /// <param name="phoneNumber">The phone Number.</param>
        /// <param name="inactive">The inactive.</param>
        /// <param name="isPatient">The is patient.</param>
        /// <returns>The The list of patients.</returns>
        [HttpGet]
        public HttpResponseMessage Search(
            string officeNumber,
            string lastName,
            string firstName,
            string dateOfBirth,
            string phoneNumber,
            string inactive,
            string isPatient)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(officeNumber);

            try
            {
                var companyId = OfficeHelper.GetCompanyId(officeNumber);
                var patients =
                    PatientSearchManager.Search(
                        new PatientSearchCriteria
                        {
                            HomeOffice = officeNumber,
                            CompanyId = companyId,
                            FirstName = firstName,
                            LastName = lastName,
                            DateOfBirth = dateOfBirth,
                            PhoneNumber = phoneNumber,
                            InActive = inactive,
                            IsPatient = isPatient
                        });
                return Request.CreateResponse(HttpStatusCode.OK, patients);
            }
            catch (Exception e)
            {
                var error = "PatientController.cs Search(): Error occurred while search for patient.\n" + e;
                return HandleExceptions.LogExceptions(error, Logger, e);
            }
        }

        /// <summary>The duplicate search.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="firstName">The first name.</param>
        /// <returns>The The list of patients.</returns>
        [HttpGet]
        public HttpResponseMessage DuplicateSearch(
            string officeNumber,
            string lastName,
            string firstName)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(officeNumber);
            try
            {
                var companyId = OfficeHelper.GetCompanyId(officeNumber);

                var patients = PatientSearchManager.DuplicateSearch(new PatientSearchCriteria
                {
                    HomeOffice = officeNumber,
                    CompanyId = companyId,
                    FirstName = firstName,
                    LastName = lastName
                });
                return patients == null ? this.Request.CreateResponse(HttpStatusCode.NotFound) : this.Request.CreateResponse(HttpStatusCode.OK, patients);
            }
            catch (Exception ex)
            {
                var error = string.Format("DuplicateSearch(officeNumber= {0} {1} {2} ", officeNumber, "\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <param name="searchCriteria">
        /// The search Criteria.
        /// </param>
        /// <returns>
        /// The list of patients.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage Search(string officeNumber, [FromUri]PatientSearchCriteria searchCriteria)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(officeNumber);
            return this.Search(
                officeNumber,
                searchCriteria.LastName,
                searchCriteria.FirstName,
                searchCriteria.DateOfBirth,
                searchCriteria.PhoneNumber,
                searchCriteria.InActive,
                searchCriteria.IsPatient);
        }

        #endregion

        #region Patient Appointments Public

        /// <summary>
        /// The get all appointments by patient id.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="patientId">
        /// The patient id.
        /// </param>
        /// <returns>
        /// The List of Patient Appointments
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetAllAppointmentsByPatientId(string officeNumber, int patientId)
        {
            var patientAppointments = new List<PatientAppointment>();

            try
            {
                AccessControl.VerifyUserAccessToMultiLocationOffice(officeNumber);
                AccessControl.VerifyUserAccessToPatient(patientId);
                var appointments = this.schedulerClient.GetAllAppointmentsByPatientId(patientId);
                Dictionary<long, string> officeIdMap = new Dictionary<long, string>();
                var offices = this.officeServices.GetAllSiblingOffices(officeNumber);
                offices.ForEach(o => officeIdMap.Add(o.OfficeId, o.ID));
                this._AddPatientAppointmentToList(officeIdMap, appointments, patientAppointments);
            }
            catch (Exception e)
            {
                var error = "GetAllAppointmentsByPatientId(officeNumber=" + officeNumber + ", patientId=" + patientId + ")\n" + e;
                return HandleExceptions.LogExceptions(error, Logger, e);
            }

            return Request.CreateResponse(HttpStatusCode.OK, patientAppointments);
        }

        /// <summary>
        /// The update patient appointment status.
        /// </summary>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="patientAppointment">
        /// The patient Appointment.
        /// </param>
        /// <returns>
        /// The List of Patient Appointments
        /// </returns>
        [HttpPut]
        public HttpResponseMessage UpdatePatientAppointmentStatus(int userId, PatientAppointment patientAppointment)
        {
            if (patientAppointment == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            try
            {
                var appointment = this.schedulerClient.GetAppointmentDetail(patientAppointment.AppointmentId);
                if (appointment == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                appointment.Detail.ConfirmationStatus = (AppointmentConfirmationStatus?)EnumerationExtensions.GetSelectedEnumValue(
                    new AppointmentConfirmationStatus(), patientAppointment.AppointmentConfirmationStatus);
                appointment.CanceledFlag = false;
                if (patientAppointment.AppointmentShowStatus == "Canceled")
                {
                    appointment.CanceledFlag = true;
                }
                else
                {
                    appointment.Detail.ShowStatus = (AppointmentShowStatus?)EnumerationExtensions.GetSelectedEnumValue(new AppointmentShowStatus(), patientAppointment.AppointmentShowStatus);
                }

                this.schedulerClient.SaveAppointmentDetail(appointment, userId);
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var msg = "UpdatePatientAppointmentStatus(appointmentId=" + patientAppointment.AppointmentId + ", userId=" + userId + ")\n" + ex;
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>
        /// The get appointment actions.
        /// </summary>
        /// <returns>
        /// The List of Appointment Show Status Types
        /// </returns>
        public List<string> GetAppointmentShowStatusTypes()
        {
            var statusList = EnumerationExtensions.GetEnumList(new AppointmentShowStatus(), true);
            statusList.Add("Canceled");
            return statusList.OrderBy(x => new string(x.ToCharArray())).ToList();
        }

        /// <summary>
        /// The get appointment confirmation status types.
        /// </summary>
        /// <returns>
        /// The List of Appointment Confirmation Status Types
        /// </returns>
        public List<string> GetAppointmentConfirmationStatusTypes()
        {
            var confirmationStatusList = EnumerationExtensions.GetEnumList(new AppointmentConfirmationStatus(), true);
            confirmationStatusList.RemoveAt(0);
            return confirmationStatusList;
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="patientId">
        /// The patient id.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The list 
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetAllRecallsByPatientId(int patientId, int userId, string officeNumber)
        {
            ////ToDo: After the login is implemented, pass the logged-in userId 
            ////ToDo: Remove the hardcoded userId
            ////ToDo: Need to implement this code, commented out for now
            ////var externalIP = miscellaneousServices.GetExternalIP();
            ////var internalIP = Request.ServerVariables["REMOTE_ADDR"].ToString();

            ////var remoteIPvalue = internalIP + " || " + externalIP;
            ////string emergencyUser;
            ////if (AuthenticationTicket.IsEmergencyAccess != null && AuthenticationTicket.IsEmergencyAccess == true)
            ////{
            ////    emergencyUser = " - Emergency Access User";
            ////}
            ////else emergencyUser = "";
            ////IList<AuditLog> auditlogHistory = auditLogServices.GetPatientNotesByEmployerByDay(patientId, userId, Common.ConvertDateTimeToTimeZone(DateTime.Now, 
            ////                                  OfficeData.TimeZone, OfficeData.UseDST).Date, "Viewed Recall" + emergencyUser);
            ////if (auditlogHistory == null || (auditlogHistory != null && auditlogHistory.Count == 0))
            ////{
            ////    auditLogServices.AuditLogUpdate(patientId, UserId, Common.ConvertDateTimeToTimeZone(DateTime.Now, OfficeData.TimeZone, OfficeData.UseDST), 
            ////                                    RemoteIPvalue, "Viewed Recall" + emergencyUser);
            ////}
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                var recalls = new PatientRecallResult
                {
                    PatientRecallDetails = new List<PatientRecallDetail>(),
                    PatientRecallHistories = new List<PatientRecallHistory>()
                };
                recalls.PatientRecallDetails = this.GetDetails(patientId);
                recalls.PatientRecallHistories = this.GetHistory(patientId, officeNumber);
                return Request.CreateResponse(HttpStatusCode.OK, recalls);
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetAllRecallsByPatientId(patientId = {0}, userId = {1} {2} {3}", patientId, userId, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        // GET api/office/999/PatientRecall/GetRecallTypes

        /// <summary>The get recall types.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <returns>The <see><cref>List</cref></see>.</returns>
        [HttpGet]
        public IEnumerable<Model.Admin.RecallType> GetRecallTypes(string officeNumber)
        {
            return new RecallTypeManager().GetRecallTypes(officeNumber).OrderBy(a => a.Name).Where(a => a.IsActive).ToList();
        }

        /// <summary>
        /// The post.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="recallDetail">
        /// The recall detail.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage Post(string officeNumber, PatientRecallDetail recallDetail)
        {
            if (string.IsNullOrEmpty(officeNumber) || string.IsNullOrEmpty(recallDetail.NextRecall))
            {
                return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "Invalid Recall!!!");
            }

            AccessControl.VerifyUserAccessToMultiLocationOffice(officeNumber);
            try
            {
                AccessControl.VerifyUserAccessToPatient(recallDetail.PatientId);
                var patientRecall = new PatientRecall();

                if (recallDetail.Id != 0)
                {
                    patientRecall = this.recallServices.GetPatientRecallbyCriteria(recallDetail.Id);
                }

                patientRecall.RecallTypeID = this.recallServices.GetRecallTypeByCriteria(recallDetail.RecallTypeId, OfficeHelper.GetCompanyId(officeNumber)).ID;
                patientRecall.RecallDate = DateTime.Parse(recallDetail.NextRecall);
                patientRecall.PatientID = recallDetail.PatientId;
                patientRecall.UpdateDate = DateTime.Now;
                patientRecall.OfficeNum = officeNumber;

                this.recallServices.SavePatientRecall(patientRecall);
                return Request.CreateResponse(HttpStatusCode.OK, "Recall Saved");
            }
            catch (Exception ex)
            {
                var msg = string.Format("Post(patientId = {0}, recallDetailId = {1} {2} {3}", recallDetail.PatientId, recallDetail.Id, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        // DELETE api/PatientRecall/5

        /// <summary>The delete recall.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpDelete]
        public HttpResponseMessage DeleteRecall([FromUri]int id)
        {
            var valid = false;
            try
            {
                if (id != 0)
                {
                    var patientRecall = this.recallServices.GetPatientRecallbyCriteria(id);
                    if (patientRecall != null)
                    {
                        AccessControl.VerifyUserAccessToPatient(patientRecall.PatientID);
                        valid = new RecallIt2Manager().DeleteOrInactivatePatientRecall(id);
                    }
                }

                return valid
                    ? Request.CreateResponse(HttpStatusCode.OK, "OK")
                    : Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to delete this recall.");
            }
            catch (Exception ex)
            {
                var msg = string.Format("DeleteRecall(id = {0} {1} {2}", id, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        #endregion

        #region Patient Overview Publics

        [HttpGet]
        public HttpResponseMessage GetPatientDependent(int patientId)
        {
            var patientOverview = new PatientOverview();
            var patientDependents = new List<PatientRelationships>();
            var helper = new Helper();
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                var dependents = this.patientServices.GetByID(patientId).Dependents;
                dependents = dependents.Where(e => e.IsPatientMerged == false && e.InActive != true).ToList();

                if (dependents != null && dependents.Count > 0)
                {
                    patientDependents.AddRange(
                        dependents.Select(
                            patientDependent =>
                                new PatientRelationships
                                {
                                    PatientId = patientDependent.ID,
                                    FullName = patientDependent.FullName,
                                    DateOfBirth =
                                        Convert.ToDateTime(patientDependent.BirthDateDisplay)
                                        .ToString("d") + " Age " + patientDependent.Age,
                                    RelationshipToInsured =
                                        this.ParseRelationship(patientDependent)
                                }).Take(5));
                }
                else
                {
                    var responsibleParty = this.patientServices.GetByID(patientId).ResponsibleParty;
                    if (responsibleParty != null && responsibleParty.InActive != true)
                    {
                        patientDependents.Add(
                            new PatientRelationships
                            {
                                PatientId = responsibleParty.ID,
                                FullName = responsibleParty.FullName,
                                DateOfBirth =
                                    Convert.ToDateTime(responsibleParty.BirthDateDisplay)
                                    .ToString("d") + " Age " + responsibleParty.Age,
                                RelationshipToInsured = "Responsible Party"
                            });
                    }
                }

                patientOverview.PatientDependents = patientDependents;
            }
            catch (Exception e)
            {
                var error = "GetPatientDependent(patientId=" + patientId + ")\n" + e;
                return HandleExceptions.LogExceptions(error, Logger, e);
            }

            return Request.CreateResponse(HttpStatusCode.OK, patientOverview);
        }

        [HttpGet]
        [Authorize(Roles = "Combine Patient Profiles")]
        public HttpResponseMessage GetPatientMergeConfirmation(string officeNumber, int patientId, int patientId2)
        {
            var patientConfirmation = new PatientMergeConfirmation();
            try
            {
                var patient = this.patientServices.GetByID(patientId);
                var patient2 = this.patientServices.GetByID(patientId2);
                patientConfirmation.Credits = this.patientDemographicsIt2Manager.GetPatientCreditAmount(patientId);
                string errorMessage;
                patientConfirmation.PatientInsuranceBalance = this.patientDemographicsIt2Manager.GetPatientInsuranceBalance(patientId, out errorMessage);
                patientConfirmation.PatientClaimsCount =
                    this.claimServices.GetClaimDetailsByPatient(patientId).ToList().Count;
                patientConfirmation.MergedPatient = new CultureInfo("en").TextInfo.ToTitleCase(patient.FirstName + "  " + patient.LastName);
                patientConfirmation.MergedIntoPatient = new CultureInfo("en").TextInfo.ToTitleCase(patient2.FirstName + "  " + patient2.LastName);
                if (patientConfirmation.PatientInsuranceBalance == 0 && patientConfirmation.Credits == 0)
                {
                    patientConfirmation.HasPatientCreditOrBalance = false;
                }
                else
                {
                    patientConfirmation.HasPatientCreditOrBalance = true;
                }
            }
            catch (Exception e)
            {
                var error = "GetPatientMergeConfirmation(officeNumber=" + officeNumber + ", patientId=" + patientId + ")\n" + e;
                return HandleExceptions.LogExceptions(error, Logger, e);
            }

            return Request.CreateResponse(HttpStatusCode.OK, patientConfirmation);
        }

        [HttpGet]
        public HttpResponseMessage GetPatientOverview(string officeNumber, int patientId)
        {
            var patientOverview = new PatientOverview();
            var patientAppointments = new List<PatientAppointment>();
            var patientOrders = new List<PatientOrder>();
            var helper = new Helper();

            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                var officeId = OfficeHelper.GetOfficeIdFromOfficeNum(officeNumber);
                var patient = this.patientServices.GetByID(patientId);
                patientOverview.FullName = patient.FirstName +
                                           (!string.IsNullOrEmpty(patient.NickName)
                                               ? " \"" + patient.NickName + "\" " + patient.LastName + " (" + patient.Sex + ")" 
                                               : " " + patient.LastName + " (" + patient.Sex + ")");

                patientOverview.HomeOffice = patient.HomeOffice;

                if (patient.PrimaryPhone?.PhoneTypeID != null)
                {
                    switch (patient.PrimaryPhone.PhoneTypeID.Description)
                    {
                        case "Home":
                            patientOverview.PrimaryPhone = helper.FormatPhone(patient.PrimaryPhone.PhoneNumberDisplay) + " (" + "h)";
                            break;
                        case "Mobile":
                            patientOverview.PrimaryPhone = helper.FormatPhone(patient.PrimaryPhone.PhoneNumberDisplay) + " (" + "c)";
                            break;
                        default:
                            patientOverview.PrimaryPhone = helper.FormatPhone(patient.PrimaryPhone.PhoneNumberDisplay) + " (" + "w)";
                            break;
                    }
                }

                if (patient.SecondaryPhone?.PhoneTypeID != null)
                {
                    switch (patient.SecondaryPhone.PhoneTypeID.Description)
                    {
                        case "Home":
                            patientOverview.SecondaryPhone = helper.FormatPhone(patient.SecondaryPhone.PhoneNumberDisplay) + " (" + "h)";
                            break;
                        case "Mobile":
                            patientOverview.SecondaryPhone = helper.FormatPhone(patient.SecondaryPhone.PhoneNumberDisplay) + " (" + "c)";
                            break;
                        default:
                            patientOverview.SecondaryPhone = helper.FormatPhone(patient.SecondaryPhone.PhoneNumberDisplay) + " (" + "w)";
                            break;
                    }
                }

                patientOverview.Email = patient.EmailDisplay;
                patientOverview.ResponsibleParty = patient.ResponsibleParty == null ? string.Empty : patient.ResponsibleParty.FullName;
                patientOverview.LastExamDate = patient.LastExamDate?.ToShortDateString() ?? "Never";
                if (patient.ProviderEmployeeId != null && patient.ProviderEmployeeId != 0)
                {
                    var provider = this.employeeServices.GetByID((int)patient.ProviderEmployeeId);
                    if (provider.Company != null && provider.Company.ID.Equals(officeNumber))
                    {
                        patientOverview.ProviderName = string.IsNullOrEmpty(provider.ProfessionalCredential)
                        ? provider.FullName
                        : provider.FullName + ", " + provider.ProfessionalCredential;
                    }
                }

                patientOverview.IsHipaaSignatureOnFile = patient.Signature ?? false;
                var age = patient.Age?.ToString() ?? string.Empty;
                DateTime parsedDate;
                var dateOfBirth = !DateTime.TryParse(patient.BirthDateDisplay, out parsedDate) ? DateTime.MinValue : parsedDate;
                if (dateOfBirth != DateTime.MinValue)
                {
                    patientOverview.DateOfBirth = dateOfBirth.ToString("d") + " (" + age + ")";
                }

                if (patient.PrimaryAddress != null)
                {
                    var address = patient.PrimaryAddress.ToModel();
                    patientOverview.Address = address.Address1;
                    if (!string.IsNullOrEmpty(address.Address2))
                    {
                        patientOverview.Address += ", " + address.Address2;
                    }

                    patientOverview.CityStateZip = address.City + ", " + address.State + " " + address.ZipCode;
                }

                string error;
                patientOverview.OutstandingBalance = this.patientDemographicsIt2Manager.GetCustomerBalance(patientId, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    Logger.Error(error);
                }

                patientOverview.PatientCredit = this.patientDemographicsIt2Manager.GetPatientCreditAmount(patientId);
                string errorMessage;
                patientOverview.PatientInsuranceBalance = this.patientDemographicsIt2Manager.GetPatientInsuranceBalance(patientId, out errorMessage);
                patientOverview.PatientUid = patient.PatientUid;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logger.Error(errorMessage);
                }

                var insurances = this.patientInsuranceManager.GetPatientInsurances(patient.ID, true);
                var primaryInsurances = new List<string>();
                var nonPrimaryInsurances = new List<string>();
                foreach (var insurance in insurances)
                {
                    if (insurance.IsPrimaryInsurance)
                    {
                        primaryInsurances.Add(insurance.CarrierDisplay);
                    }
                    else
                    {
                        nonPrimaryInsurances.Add(insurance.CarrierDisplay);
                    }
                }

                if (primaryInsurances.Count > 0)
                {
                    switch (primaryInsurances.Count)
                    {
                        case 1:
                            patientOverview.InsuranceOne = primaryInsurances.FirstOrDefault();
                            if (nonPrimaryInsurances.Count > 0)
                            {
                                patientOverview.InsuranceTwo = nonPrimaryInsurances.FirstOrDefault();
                            }

                            break;
                        case 2:
                            patientOverview.InsuranceOne = primaryInsurances.FirstOrDefault();
                            patientOverview.InsuranceTwo = primaryInsurances[1];
                            break;
                        default:
                            patientOverview.InsuranceOne = primaryInsurances.FirstOrDefault();
                            patientOverview.InsuranceTwo = primaryInsurances[1];
                            patientOverview.InsuranceThree = primaryInsurances[2];
                            break;
                    }
                }
                else
                {
                    if (nonPrimaryInsurances.Count > 0)
                    {
                        switch (nonPrimaryInsurances.Count)
                        {
                            case 1:
                                patientOverview.InsuranceOne = nonPrimaryInsurances.FirstOrDefault();
                                break;
                            case 2:
                                patientOverview.InsuranceOne = nonPrimaryInsurances.FirstOrDefault();
                                patientOverview.InsuranceTwo = nonPrimaryInsurances[1];
                                break;
                            default:
                                patientOverview.InsuranceOne = nonPrimaryInsurances.FirstOrDefault();
                                patientOverview.InsuranceTwo = nonPrimaryInsurances[1];
                                patientOverview.InsuranceThree = nonPrimaryInsurances[2];
                                break;
                        }
                    }
                }

                if (patient.Orders != null && patient.Orders.Count > 0)
                {
                    patientOrders.AddRange(patient.Orders.Select(patientOrder => new PatientOrder
                    {
                        OrderDate = Convert.ToDateTime(patientOrder.OrderDate).ToString("d"),
                        OrderStatus = this.OrderStatusCodeDisplayString(patientOrder),
                        OrderType = this.OrderTypeDisplayString(patientOrder.OrderType)
                    }));

                    patientOrders = patientOrders.Take(3).ToList();
                }

                var appointments = this.schedulerClient.GetAllAppointmentsByPatientId(patientId).OrderByDescending(x => x.AppointmentDate).ToList();

                if (appointments.Count > 0)
                {
                    patientAppointments.AddRange(appointments.Select(patientAppointment => new PatientAppointment
                    {
                        AppointmentType = patientAppointment.ServiceDescription,
                        AppointmentDate = Convert.ToDateTime(patientAppointment.AppointmentDate).ToString("d")
                        + " " + Convert.ToDateTime(patientAppointment.AppointmentStartTime).ToString("h:mm tt"),
                        AppointmentShowStatus = this.AppointmentShowDescription(patientAppointment.CanceledFlag.GetValueOrDefault(), patientAppointment.ShowStatus.GetValueOrDefault()),
                    }).ToList());
                }

                patientOverview.TotalCancellations = patientAppointments.Count(n => n.AppointmentShowStatus == "Canceled");
                patientOverview.TotalNoShows = patientAppointments.Count(n => n.AppointmentShowStatus == "No Show");
                patientOverview.PatientAppointments = patientAppointments.Take(3);
                patientOverview.PatientOrders = patientOrders;
                patientOverview.PatientRecalls = this.GetDetails(patientId);
                if (patientId > 0)
                {
                    var oph = new OfficePatientHistory { OfficeNum = officeNumber, PatientID = patientId };
                    this.officeServices.SaveOfficePatientHistory(oph);
                }
            }
            catch (Exception e)
            {
                var error = "GetPatientOverview(officeNumber=" + officeNumber + ", patientId=" + patientId + ")\n" + e;
                return HandleExceptions.LogExceptions(error, Logger, e);
            }

            return Request.CreateResponse(HttpStatusCode.OK, patientOverview);
        }

        #endregion

        #region Patient Merge
        /// <summary>
        /// Merges two patients.
        /// </summary>
        /// <param name="patient1Id">The patient to merge into.</param>
        /// <param name="patient2Id">The patient to be merged.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Combine Patient Profiles")]
        public HttpResponseMessage MergePatients(string officeNumber, MergePatientsParameters patientIds)
        {
            try
            {
                if (patientIds.Patient1Id != patientIds.Patient2Id)
                {
                    AccessControl.VerifyUserAccessToPatient(patientIds.Patient1Id);
                    AccessControl.VerifyUserAccessToPatient(patientIds.Patient2Id);

                    var office = this.officeServices.GetByID(officeNumber);

                    var selectedPatientForMerge = patientIds.SelectedPatient;

                    var authorizationTicketHelper = new AuthorizationTicketHelper();
                    var user = authorizationTicketHelper.GetUserInfo();

                    // Switch the patients if the Second patient needs the merge
                    if (selectedPatientForMerge == 2)
                    {
                        var patient1Id = patientIds.Patient1Id;
                        patientIds.Patient1Id = patientIds.Patient2Id;
                        patientIds.Patient2Id = patient1Id;
                    }

                    SchedulerWS scheduler = new SchedulerWS();
                    List<WebScheduler.DataContracts.Appointment> pat2Appts =
                        scheduler.GetClassicAppointmentsByPatId(
                            patientIds.Patient2Id,
                            authorizationTicketHelper.GetCompanyId());

                    if (pat2Appts.Count > 0)
                    {
                        foreach (var appointment in pat2Appts)
                        {
                            appointment.PatientId = patientIds.Patient1Id;
                            scheduler.SaveAppointmentDetail(appointment, user.Id);
                        }
                    }

                    this.patientServices.MergePatients(
                        patientIds.Patient1Id,
                        patientIds.Patient2Id,
                        user.Id,
                        OfficeHelper.ConvertDateTimeToTimeZone(DateTime.Now, office.TimeZone, office.UseDST),
                        authorizationTicketHelper.GetCompanyId());
                    
                    var patient2 = this.patientServices.GetByID(patientIds.Patient2Id);
                    patient2.InActive = true;
                    patient2.IsPatientMerged = true;
                    this.patientServices.Save(patient2);
                }

                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Merge operation MergePatients(patient1Id = {0}, patient2Id = {1}) failed.", patientIds.Patient1Id, patientIds.Patient2Id);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }
        #endregion

        #region Patient Exams Rx Public

        /// <summary>
        /// The get all rx by patient id.
        /// </summary>
        /// <param name="patientId">
        /// The patient id.
        /// </param>
        /// <param name="officeNumber">
        /// The patient id.
        /// </param>
        /// <returns>
        /// The patient exams
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetAllRxByPatientId(int patientId, string officeNumber)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                var patientExams = this.examServices.GetPatientExamsByPatient(patientId).OrderByDescending(x => x.ExamDate);
                var patientRxes =
                    patientExams.Select(
                        patientExam =>
                        new PatientRx
                        {
                            Id = patientExam.ID,
                            ExamDate = Convert.ToDateTime(patientExam.ExamDate).ToString("d"),
                            ExpirationDate = Convert.ToDateTime(patientExam.ExpirationDate).ToString("d"),
                            RxType = this.GetExamRxTypeDisplay(patientExam),
                            DoctorName = GetRxDoctorName(patientExam),
                            EmployeeName = GetRxEmployeeName(patientExam),
                            OutsideRx = patientExam.Doctor == null && patientExam.OutsideDoctor != null,
                            HasBeenRechecked = patientExam.HasBeenRechecked ?? false,
                            HasOrders = this.HasOrdersByExamId(patientExam.ID),
                            SourceDescription = GetRxSourceDescription(patientExam),
                            ExamRxTypeId = patientExam.ExamRxTypeID,
                            OfficeNumber = patientExam.OfficeNum,
                            RecheckExamId = patientExam.RecheckExamID.GetValueOrDefault()
                        }).ToList();

                var isNewExamUiEnabled = this.featureManager.GetFeatureConfigurationInProduct(FeatureUUIDs.EpmNewExamsUi, FeatureProducts.EPM.ToString()).available;
                if (isNewExamUiEnabled)
                {
                    patientRxes.RemoveAll(x => x.ExamRxTypeId == PatientExam.EXAM_TYPE_NORX);
                }

                if (patientRxes.Count > 0)
                {
                    var data = patientRxes.Select(x => new KeyValuePair<int, DateTime>(x.Id, DateTime.UtcNow)).ToList();
                    MessageTracking.SignalAlSupportTracking(data, "Listed");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, patientRxes);
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetAllRxByPatientId(patientid = {0} {1} {2}", patientId, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetOverviewRxByPatientId(int patientId, string officeNumber)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                var patientExams = this.examServices.GetPatientExamsByPatient(patientId).OrderByDescending(x => x.ExamDate);
                var patientRxes =
                    patientExams.Select(
                        patientExam =>
                            new PatientRx
                            {
                                Id = patientExam.ID,
                                ExamDate = Convert.ToDateTime(patientExam.ExamDate).ToString("d"),
                                ExpirationDate = Convert.ToDateTime(patientExam.ExpirationDate).ToString("d"),
                                RxType = this.GetExamRxTypeDisplay(patientExam),
                                ExamRxTypeId = patientExam.ExamRxTypeID
                            }).ToList();
                
                var isNewExamUiEnabled = this.featureManager.GetFeatureConfigurationInProduct(FeatureUUIDs.EpmNewExamsUi, FeatureProducts.EPM.ToString()).available;
                if (isNewExamUiEnabled)
                {
                    patientRxes.RemoveAll(x => x.ExamRxTypeId == PatientExam.EXAM_TYPE_NORX);
                }

                if (patientRxes.Count > 0)
                {
                    var data = patientRxes.Select(x => new KeyValuePair<int, DateTime>(x.Id, DateTime.UtcNow)).ToList();
                    MessageTracking.SignalAlSupportTracking(data, "Listed");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, patientRxes);
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetOverviewRxByPatientId(patientid = {0} {1} {2}", patientId, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>
        /// The is eyeglass exam.
        /// </summary>
        /// <param name="examId">
        /// The exam id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [HttpGet]
        public bool IsEyeglassExam(int examId)
        {
            var exam = this.examServices.GetPatientExamByID(Convert.ToInt32(examId));
            return exam.IsEyeglassExam();
        }

        /// <summary>
        /// The delete patient exam by.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpDelete]
        public HttpResponseMessage DeletePatientExam(int id)
        {
            var patientIt2Manager = new PatientIt2Manager();
            var patientNoteIt2Manager = new PatientNotesIt2Manager();
            var exam = this.examServices.GetPatientExamByID(id);
            AccessControl.VerifyUserAccessToPatient(exam.Patient.ID);
            while (exam.Details.Count > 0)
            {
                var first = exam.Details.FirstOrDefault();
                if (first != null)
                {
                    PatientExamRxManager.DeletePatientExamDetailAlsl(first.ID);
                }

                exam.Details.RemoveAt(0);
            }

            try
            {
                PatientExamRxManager.DeletePatientExamAlsl(id);
                var note = patientNoteIt2Manager.GetLatestEntityNote(exam);
                if (note != null)
                {
                    patientNoteIt2Manager.DeleteExamNote(note);
                }

                var result = patientIt2Manager.DeletePatientExam(id);

                return result
                    ? this.Request.CreateResponse(HttpStatusCode.OK, "Exam deleted.")
                    : this.Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to delete this Exam.");
            }
            catch (Exception e)
            {
                var error = "DeletePatientExam(id=" + id + "\n " + e;
                return HandleExceptions.LogExceptions(error, Logger, e);
            }
        }
        #endregion

        #region Patient Exams Rx Private Static

        /// <summary>
        /// The get rx employee name.
        /// </summary>
        /// <param name="exam">
        /// The exam.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetRxEmployeeName(PatientExam exam)
        {
            try
            {
                var employeeName = string.Empty;
                if (exam.Employee != null)
                {
                    employeeName = exam.Employee.FullName;
                }

                return employeeName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// The get rx source description.
        /// </summary>
        /// <param name="exam">
        /// The exam.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetRxSourceDescription(PatientExam exam)
        {
            try
            {
                string source;
                if (exam.Doctor == null && exam.OutsideDoctor != null)
                {
                    source = "Outside";
                }
                else
                {
                    source = exam.SourceDescription;
                }

                return source;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// The get doctor name.
        /// </summary>
        /// <param name="exam">
        /// The exam.
        /// </param>
        /// <returns>
        /// The doctor name
        /// </returns>
        private static string GetRxDoctorName(PatientExam exam)
        {
            try
            {
                var doctorName = string.Empty;
                if (exam.Doctor != null)
                {
                    doctorName = exam.Doctor.FullName;
                }
                else if (exam.OutsideDoctor != null)
                {
                    doctorName = exam.OutsideDoctor.FirstName + " " + exam.OutsideDoctor.LastName;
                }

                return doctorName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        #endregion
        #region Patient Search Private

        /// <summary>
        /// The search result from patient lite.
        /// </summary>
        /// <param name="patientLite">
        /// The patient lite.
        /// </param>
        /// <returns>
        /// The <see cref="PatientSearchResult"/>.
        /// </returns>
        private static PatientSearchResult SearchResultFromPatientLite(PatientLite patientLite)
        {
            // TODO:
            // Mapping lib should be used. Also Need to examine type conversion
            // rules between nullable/nonnullable types and strings
            var address = string.Empty;
            if (patientLite.Address1Display != string.Empty || patientLite.Address2Display != string.Empty || patientLite.CityDisplay != string.Empty)
            {
                address = string.Format("{0} {1} {2} {3} {4} {5}", patientLite.Address1Display, " ", patientLite.Address2Display, " ", patientLite.CityDisplay, ", ");
            }

            address = string.Format("{0} {1} {2} {3}", address, patientLite.State, " ", patientLite.ZipCodeDisplay);
            var primaryPhone = string.Empty;
            if (patientLite.PhoneNumberDisplay.Length >= 10)
            {
                double phone;
                double.TryParse(patientLite.PhoneNumberDisplay.Substring(0, 10), out phone);
                primaryPhone = string.Format("{0:(###) ###-####}", phone);
            }

            return new PatientSearchResult
            {
                PatientId = patientLite.ID,
                FirstName = patientLite.FirstName,
                LastName = patientLite.LastName,
                Address = address,
                Age = patientLite.Age.GetValueOrDefault(),
                DateOfBirth = Convert.ToDateTime(patientLite.BirthDateDisplay).ToString("d"),
                Phone = primaryPhone,
            };
        }

        #endregion

        #region Patient Appointments Private

        /// <summary>
        /// The appointment show description.
        /// </summary>
        /// <param name="canceledFlag">
        /// The canceled flag.
        /// </param>
        /// <param name="showStatus">
        /// The show status.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string AppointmentShowDescription(bool canceledFlag, AppointmentShowStatus showStatus)
        {
            var status = showStatus;
            if (canceledFlag)
            {
                return "Canceled";
            }

            if (status < 0)
            {
                status = 0;
            }

            return EnumerationExtensions.GetSelectedEnumString(new AppointmentShowStatus(), Convert.ToInt32(status));
        }

        /// <summary>
        /// The appointment confirm description.
        /// </summary>
        /// <param name="confirmationStatus">
        /// The confirmation status.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string AppointmentConfirmDescription(AppointmentConfirmationStatus? confirmationStatus)
        {
            return EnumerationExtensions.GetSelectedEnumString(
                new AppointmentConfirmationStatus(),
                Convert.ToInt32(confirmationStatus.GetValueOrDefault()));
        }

        /// <summary>
        /// combines appointments from one list with appointments from others
        /// </summary>
        /// <param name="appointments"></param>
        /// <param name="patientAppointments"></param>
        private void _AddPatientAppointmentToList(Dictionary<long, string> officeIdMap, List<PatientAppointments> appointments, List<PatientAppointment> patientAppointments)
        {
            if (appointments != null)
            {
                patientAppointments.AddRange(appointments.Select(patientAppointment => new PatientAppointment
                {
                    AppointmentId = patientAppointment.Id,
                    AppointmentConfirmationStatus = this.AppointmentConfirmDescription(patientAppointment.ConfirmationStatus),
                    AppointmentDate = Convert.ToDateTime(patientAppointment.AppointmentDate).ToString("d"),
                    AppointmentShowStatus = this.AppointmentShowDescription(patientAppointment.CanceledFlag.GetValueOrDefault(), patientAppointment.ShowStatus.GetValueOrDefault()),
                    AppointmentTime = Convert.ToDateTime(patientAppointment.AppointmentStartTime).ToString("h:mm tt"),
                    AppointmentType = patientAppointment.ServiceDescription,
                    DoctorFullName = patientAppointment.ResourceFullName,
                    OfficeId = officeIdMap[patientAppointment.LocationId],
                    AppointmentDateTime =
                        DateTime.Parse(Convert.ToDateTime(patientAppointment.AppointmentDate).ToString("d") + " " +
                                       Convert.ToDateTime(patientAppointment.AppointmentStartTime).ToString("h:mm tt"))
                            .ToString("yyyyMMddHHmmss")
                }));
            }
        }

        #endregion
        #region Patient Appointments Recalls Private
        /// <summary>
        /// The get history.
        /// </summary>
        /// <param name="patientId">
        /// The patient id.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private List<PatientRecallHistory> GetHistory(int patientId, string officeNumber)
        {
            var patientCorrespondence = this.recallServices.GetPatientCorrespondence(patientId) as List<PatientCorrespondence>;

            patientCorrespondence = patientCorrespondence.Where(e => e.OfficeNum == officeNumber).ToList();
            var correspondenceItems = new List<PatientRecallHistory>();

            if (patientCorrespondence != null)
            {
                /* TECH DEBT: We should be driving the correspondence type name off of the correspondencetype enum.
                 this enum was added in ALSCL-1890. There was no time to alter existing code to use this.
                 currently correspondence records are written with the correspondencename field.
                 However this field *should* be used for the name of the partner sending the correspondence.
                
                 Ideally:
                
                 new PatientRecallHistory
                 {
                    CorrespondenceDate = item.CorrespondencePrtDate.ToShortDateString(),
                    RecallType = this.recallServices.RecallDiscription(item.RecallTypeNum),
                    NoticeNum = item.RecallNoticeNum,
                    CorrespondenceName = item.CorrespondenceName,
                    CorrespondeceType = Enum.GetName(CorrespondenceType, item.CorrespondenceType)
                    IsPartnerCorrespondence = item.PartnerCorrespondence
                 }
                */

                correspondenceItems.AddRange(patientCorrespondence.Select(
                       item =>
                       new PatientRecallHistory
                       {
                           CorrespondenceDate = item.CorrespondencePrtDate.ToShortDateString(),
                           RecallType = this.recallServices.RecallDiscription(item.RecallTypeNum),
                           NoticeNum = item.RecallNoticeNum,
                           CorrespondenceName = item.CorrespondenceName,
                           IsPartnerCorrespondence = item.PartnerCorrespondence,
                           OfficeNumber = item.OfficeNum
                       }));
            }

            return correspondenceItems;
        }

        /// <summary>
        /// The get details.
        /// </summary>
        /// <param name="patientId">
        /// The patient id.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private List<PatientRecallDetail> GetDetails(int patientId)
        {
            var patientRecalls = this.recallServices.GetAllPatientRecalls(patientId) as List<PatientRecall>;

            var recallItems = new List<PatientRecallDetail>();
            if (patientRecalls == null)
            {
                return recallItems;
            }

            var patientActiveRecalls =
                patientRecalls.Where(r => r.IsActive == null || r.IsActive.GetValueOrDefault()).ToList();

            recallItems.AddRange(patientActiveRecalls.Select(
                item =>
                    new PatientRecallDetail
                    {
                        NextRecall = item.RecallDate.ToShortDateString(),
                        Id = item.ID,
                        RecallType = this.recallServices.RecallDiscription(item.RecallTypeID),
                        MonthsToRecall = this.GetMonths(item.RecallDate),
                        RecallTypeId = item.RecallTypeID,
                        IsActive = item.IsActive == null || item.IsActive.GetValueOrDefault(),
                        OfficeNumber = item.OfficeNum
                    }));

            return recallItems;
        }

        /// <summary>
        /// The get months.
        /// </summary>
        /// <param name="recallDate">
        /// The recall date.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int GetMonths(DateTime recallDate)
        {
            int months = (recallDate.Year - DateTime.Now.Year) * 12;
            months -= DateTime.Now.Month + 1;
            months += recallDate.Month;
            return months + 1;
        }

        #endregion

        #region Patient Exams Rx Private

        /// <summary>
        /// The get exam rx type display.
        /// </summary>
        /// <param name="exam">
        /// The exam.
        /// </param>
        /// <returns/>
        /// The <see cref="string"/>.
        /////// </returns>
        private string GetExamRxTypeDisplay(PatientExam exam)
        {
            var display = PatientExam.ExamRxTypeDisplay(exam.ExamRxTypeID ?? 0);
            var hasRAddPower = false;
            var hasLAddPower = false;

            if (display.Contains("EG"))
            {
                display = display.Replace("EG", "Eyeglass");

                if (exam.RxTypeID != 749 && exam.RxTypeID != 90004 && exam.RxTypeID != 90009 && exam.RxTypeID != 90001 && exam.Source == (int)PatientExamSourceEnum.EMR)
                {
                    if (exam.Details[0].AddPower1.HasValue)
                    {
                        hasRAddPower = true;
                    }

                    if (exam.Details[1].AddPower1.HasValue)
                    {
                        hasLAddPower = true;
                    }

                    if (hasRAddPower || hasLAddPower)
                    {
                        exam.RxTypeID = 749;
                        this.examServices.SavePatientExam(exam);
                    }
                }

                if (exam.ExamRxTypeID != PatientExam.EXAM_EG_INCOMPLETE && exam.Source == (int)PatientExamSourceEnum.EMR)
                {
                    var examDetailAlsl = PatientExamRxManager.GetPatientExamDetailAlsl(exam.ID);
                    var errors = PatientIt2Manager.ValidateRxExamForCompleteness(examDetailAlsl, exam);
                    if (errors != null && errors.Count > 0)
                    {
                        exam.ExamRxTypeID = PatientExam.EXAM_EG_INCOMPLETE;
                        display = display.Replace("Eyeglass Rx", "Eyeglass Incomplete");
                        this.examServices.SavePatientExam(exam);
                    }
                }
            }
            else if (display.Contains("CL"))
            {
                if (exam.Source == (int)PatientExamSourceEnum.EMR)
                {
                    var examDetailAlsl = PatientExamRxManager.GetPatientExamDetailAlsl(exam.ID);
                    var errors = PatientIt2Manager.ValidateRxExamForCompleteness(examDetailAlsl, exam);
                    if (errors != null && errors.Count > 0)
                    {
                        exam.ExamRxTypeID = PatientExam.EXAM_CL_INCOMPLETE;
                        display = display.Replace("CL Rx", "CL Incomplete");
                        this.examServices.SavePatientExam(exam);
                    }
                }
            }
            else
            {
                if (exam.ExamRxTypeID == PatientExam.EXAM_TYPE_NORX)
                {
                    display = display.Replace("No RX", "Exam Only");
                }
            }

            if (exam.RxTypeID.HasValue)
            {
                GenLookup type = this.lookupServices.GetById(exam.RxTypeID.Value);
                var description = type.Description;
                if (description.ToLower() == "reading over cl's")
                {
                    description = "Reading Over Contacts"; 
                }

                display += " (" + description + ")";
            }

            return display;
        }

        /// <summary>
        /// The has orders by exam id.
        /// </summary>
        /// <param name="examId">
        /// The exam id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool HasOrdersByExamId(int examId)
        {
            return this.orderServices.hasOrdersByExamId(examId);
        }

        #endregion

        #region Private
        
        private string OrderStatusCodeDisplayString(Order o)
        {
           ////IList<GenLookup> orderStatusList = this.lookupServices.GetAll(LookupType.ORDER_STATUS);
           var orderStatusDisplayString = string.Empty;
           string invoiceStatusDisplayString = string.Empty;
           this.orderServices.GetStatusDisplayStrings(o, out orderStatusDisplayString, out invoiceStatusDisplayString);
           return orderStatusDisplayString;
        }

        private string OrderTypeDisplayString(string orderType)
        {
            switch (orderType)
            {
                case "E":
                    return "Eyeglass";
                case "S":
                    return "Soft CL";
                case "H":
                    return "Hard CL";
                case "M":
                    return "Misc.";
                default:
                    return "Exam";
            }
        }

        private string ParseRelationship(IT2.Core.Patient patientDependent)
        {
            if (!patientDependent.HasInsurance || patientDependent.Insurances.Count <= 0)
            {
                return null;
            }

            foreach (var insurance in patientDependent.Insurances)
            {
                if (!string.IsNullOrEmpty(insurance.Relationship?.Description))
                {
                    if (insurance.Relationship.Description != "Self")
                    {
                        return insurance.Relationship.Description;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}