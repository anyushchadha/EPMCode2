// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatientDemographicsController.cs" company="Eyefinity, Inc.">
// 2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the PatientDemographicsController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.Enterprise.Business.Common;
    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Interfaces;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Common.It2Converters;
    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Patient;

    using IT2.Core;
    using IT2.Ioc;
    using IT2.Services;

    using CompanyInformation = IT2.Core.CompanyInformation;
    using Patient = IT2.Core.Patient;

    /// <summary>
    /// The patient demographics controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientDemographicsController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>The lookup services.</summary>
        private readonly ILookupServices lookupServices;

        /// <summary>The patient services.</summary>
        private readonly IPatientServices patientServices;

        /// <summary>The employee services.</summary>
        private readonly IEmployeeServices employeeServices;

        /// <summary>The office services.</summary>
        private readonly IOfficeServices officeServices;

        /// <summary>The note services.</summary>
        private readonly INoteServices noteServices;

        /// <summary>The address services.</summary>
        private readonly IAddressServices addressServices;

        /// <summary> The transaction services. </summary>
        private readonly IT2.Services.POS.ITransactionServices transactionServices;

        /// <summary>
        /// The practice information it 2 manager.
        /// </summary>
        private readonly OfficeIt2Manager officeIt2Manager;

        /// <summary>
        /// The office helper.
        /// </summary>
        private readonly OfficeHelper officeHelper;

        /// <summary>The lookup services.</summary>
        private readonly InMemoryCache memoryCache;

        /// <summary>
        /// Patient Demographics It2Manager 
        /// </summary>
        private readonly PatientDemographicsIt2Manager it2Manager;
        
        /// <summary>
        /// Authorization Ticke tHelper
        /// </summary>
        private readonly IAuthorizationTicketHelper authorizationTicketHelper;

        /// <summary>
        /// Authorization Ticke tHelper
        /// </summary>
        private readonly IPreferencesManager preferencesManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientDemographicsController"/> class.
        /// </summary>
        /// <param name="lookupServices">
        /// The lookup services.
        /// </param>
        /// <param name="patientServices">
        /// The patient services.
        /// </param>
        /// <param name="employeeServices">
        /// The employee Services.
        /// </param>
        /// <param name="officeServices">
        /// The office Services.
        /// </param>
        /// <param name="noteServices">
        /// The note Services.
        /// </param>
        /// <param name="addressServices">
        /// The address services.
        /// </param>
        /// <param name="transactionServices">
        /// The transaction Services.
        /// </param>
        /// <param name="demographicsIt2Manager"></param>
        /// <param name="officeIt2Manager"></param>
        /// <param name="officeHelper"></param>
        /// <param name="authorizationTicketHelper">
        /// The Authorization Ticket Helper Services.
        /// </param>
        public PatientDemographicsController(
            ILookupServices lookupServices,
            IPatientServices patientServices,
            IEmployeeServices employeeServices,
            IOfficeServices officeServices,
            INoteServices noteServices,
            IAddressServices addressServices,
            IT2.Services.POS.ITransactionServices transactionServices,
            PatientDemographicsIt2Manager demographicsIt2Manager,
            OfficeIt2Manager officeIt2Manager,
            OfficeHelper officeHelper,
            IAuthorizationTicketHelper authorizationTicketHelper,
            IPreferencesManager preferencesManager)
        {
            this.lookupServices = lookupServices;
            this.patientServices = patientServices;
            this.employeeServices = employeeServices;
            this.officeServices = officeServices;
            this.noteServices = noteServices;
            this.addressServices = addressServices;
            this.officeIt2Manager = officeIt2Manager;
            this.transactionServices = transactionServices;
            this.officeHelper = officeHelper;
            this.memoryCache = new InMemoryCache();
            this.it2Manager = demographicsIt2Manager;
            this.authorizationTicketHelper = authorizationTicketHelper;
            this.preferencesManager = preferencesManager;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientDemographicsController"/> class.
        /// </summary>
        public PatientDemographicsController()
        {
            this.lookupServices = Container.Resolve<ILookupServices>();
            this.patientServices = Container.Resolve<IPatientServices>();
            this.employeeServices = Container.Resolve<IEmployeeServices>();
            this.officeServices = Container.Resolve<IOfficeServices>();
            this.noteServices = Container.Resolve<INoteServices>();
            this.addressServices = Container.Resolve<IAddressServices>();
            this.officeIt2Manager = new OfficeIt2Manager();
            this.transactionServices = Container.Resolve<IT2.Services.POS.ITransactionServices>();
            this.officeHelper = new OfficeHelper();
            this.memoryCache = new InMemoryCache();
            this.it2Manager = new PatientDemographicsIt2Manager();
            this.authorizationTicketHelper = new AuthorizationTicketHelper();
            this.preferencesManager = new PreferencesManager();
        }

        // GET api/<controller>

        /// <summary>The get.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="id">The id.</param>
        /// <param name="saveQuicklist"></param>
        /// <returns>The Patient Demographics</returns>
        public HttpResponseMessage Get(string officeNumber, int id, bool saveQuicklist = true)
        {
            var patientDemographics = new PatientDemographics();

            this.LoadAllLookupTypes(ref patientDemographics);
            var office = this.officeServices.GetByID(officeNumber);
            var companyId = office.Company.ID;
            var providers = this.GetEmployeesInCompany(companyId, true);

            patientDemographics.Providers = providers;
            patientDemographics.Provider = office.Company.CompanyDoctor != null
                ? office.Company.CompanyDoctor.ID
                : 0;
            patientDemographics.Offices = this.GetOffices(companyId);
            patientDemographics.NickName = string.Empty;
            var dict = this.preferencesManager.GetPreferencesByCategory(companyId, PreferenceCategory.RequiredPatientProfileFields);
            var profileFieldsPreferences = new List<Lookup>();
            profileFieldsPreferences.AddRange(dict.Select(entry => new Lookup(entry.Key, entry.Value)));
            patientDemographics.ProfileFieldsPreference = profileFieldsPreferences;
            patientDemographics.CommunicationPreferenceTypes = this.memoryCache.Get<IEnumerable<Lookup>>("CommunicationPreferences", this.GetCommunicationPreferences);

            patientDemographics.Address = new Model.Address();
            patientDemographics.Activities = new List<int>();
            patientDemographics.PrimaryPhoneType =
                patientDemographics.PhoneTypes.First(x => x.Description.ToLower().Contains("mobile")).Key;
            patientDemographics.SecondaryPhoneType = patientDemographics.PrimaryPhoneType;

            // check if we need to return just a template 
            if (id == 0)
            {
                // initialize patient's Home Office with the one that we currently authenticated with
                patientDemographics.HomeOffice = this.authorizationTicketHelper.GetUserInfo().OfficeNum;
                return Request.CreateResponse(HttpStatusCode.OK, patientDemographics);
            }

            Patient patient;
            try
            {
                patient = this.patientServices.GetByID(id);
            }
            catch (NHibernate.ObjectNotFoundException e)
            {
                Logger.Error("Get(Id=" + id + ")\n" + e);
                return Request.CreateResponse(HttpStatusCode.NotFound, "Patient not found.");
            }

            if (patient == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Patient not found.");
            }

            AccessControl.VerifyUserAccessToPatient(id);

            patientDemographics.Id = patient.ID;
            patientDemographics.IsPatientMerged = patient.IsPatientMerged;
            patientDemographics.PatientUid = patient.PatientUid;
            patientDemographics.Title = (patient.Title != null) ? patient.Title.ID : 0;
            patientDemographics.FirstName = patient.FirstName;
            patientDemographics.MiddleInitial = patient.Middle;
            patientDemographics.LastName = patient.LastName;
            patientDemographics.NickName = patient.NickName;
            patientDemographics.Provider = patient.ProviderEmployeeId ?? 0;
            patientDemographics.IsPatient = patient.IsPatient;
            patientDemographics.IsResponsibleParty = patient.IsResponsibleParty;
            patientDemographics.EmploymentStatus = patient.EmploymentStatus;
            patientDemographics.HomeOffice = patient.HomeOffice;

            this.GetResponsiblePartyInfo(patientDemographics, patient, companyId);

            if (!string.IsNullOrEmpty(patient.SSNDisplay))
            {
                if (patient.SSNDisplay.Length > 9)
                {
                    double ssn;
                    double.TryParse(patient.SSNDisplay.Substring(0, 9), out ssn);
                    if (ssn > 0)
                    {
                        patientDemographics.SocialSecurityNumber = string.Format("{0:###-##-####}", ssn);
                    }
                }
                else
                {
                    patientDemographics.SocialSecurityNumber = patient.SSNDisplay;
                }
            }

            patientDemographics.Email = patient.EmailDisplay;
            patientDemographics.MedicalRecordNumber = patient.MRNDisplay;
            DateTime parsedDate;
            var dateOfBirth = !DateTime.TryParse(patient.BirthDateDisplay, out parsedDate) ? DateTime.MinValue : parsedDate;
            if (dateOfBirth != DateTime.MinValue)
            {
                patientDemographics.DateOfBirth = dateOfBirth.ToString("d");
            }

            patientDemographics.Sex = patient.Sex;
            patientDemographics.Age = patient.Age.HasValue ? patient.Age.ToString() : string.Empty;
            patientDemographics.Occupation = (patient.Occupation != null) ? patient.Occupation.ID : 0;
            patientDemographics.Active = !(patient.InActive ?? false);
            patientDemographics.Deceased = patient.Deceased ?? false;
            patientDemographics.SpecialNeeds = patient.SpecialNeeds ?? false;

            patientDemographics.LastExamDate = patient.LastExamDate.HasValue
                                                   ? patient.LastExamDate.Value.ToShortDateString()
                                                   : "Never";
            patientDemographics.Activities =
                (List<int>)(patient.Activities != null ? this.GetActivities(patient.Activities) : null);
            if (patient.PrimaryAddress != null)
            {
                patientDemographics.Address = patient.PrimaryAddress.ToModel();
            }

            var helper = new Helper();
            if (patient.PrimaryPhone != null)
            {
                patientDemographics.PrimaryPhone = helper.FormatPhone(patient.PrimaryPhone.PhoneNumberDisplay);
                if (patient.PrimaryPhone.PhoneTypeID != null)
                {
                    patientDemographics.PrimaryPhoneType = (patient.PrimaryPhone.PhoneTypeID != null)
                                                               ? patient.PrimaryPhone.PhoneTypeID.ID
                                                               : 0;
                }

                if (patient.PrimaryPhone.CallTimeLU != null)
                {
                    patientDemographics.PrimaryPhoneCallTime = (patient.PrimaryPhone.CallTimeLU != null)
                                                                   ? patient.PrimaryPhone.CallTimeLU.ID
                                                                   : 0;
                }
            }

            if (patient.SecondaryPhone != null)
            {
                patientDemographics.SecondaryPhone = helper.FormatPhone(patient.SecondaryPhone.PhoneNumberDisplay);
                if (patient.SecondaryPhone.PhoneTypeID != null)
                {
                    patientDemographics.SecondaryPhoneType = (patient.SecondaryPhone.PhoneTypeID != null)
                                                                 ? patient.SecondaryPhone.PhoneTypeID.ID
                                                                 : 0;
                }

                if (patient.SecondaryPhone.CallTimeLU != null)
                {
                    patientDemographics.SecondaryPhoneCallTime = (patient.SecondaryPhone.CallTimeLU != null)
                                                                     ? patient.SecondaryPhone.CallTimeLU.ID
                                                                     : 0;
                }
            }

            if (patient.MaritalStatus != null)
            {
                patientDemographics.MaritalStatus = patient.MaritalStatus.ID;
            }

            ////ALSL-1771
            if (patient.Races != null && patient.Races.Any())
            {
                patientDemographics.Races = (from lookupType in patient.Races select lookupType.ID).ToList();
            }

            if (patient.Ethnicity != null)
            {
                patientDemographics.Ethnicity = patient.Ethnicity.ID;
            }

            if (patient.PreferredLanguage != null)
            {
                patientDemographics.PreferredLanguage = patient.PreferredLanguage.ID;
            }

            patientDemographics.CommunicationPreference = CommunicationPreferenceExtension.ToEpmCommunicationPreference(patient.CommunicationPreference);
            patientDemographics.Referral = patient.ReferralID;
            patientDemographics.IsEmailForPromotions = patient.emailPromo ?? false;
            patientDemographics.IsEmailForRecalls = patient.emailRecall ?? false;
            patientDemographics.IsDoNotSendMailOffers = patient.IsDoNotSendMailOffers ?? false;
            patientDemographics.IsHipaaSignatureOnFile = patient.Signature ?? false;
			patientDemographics.IsTextForApptNotifications = patient.SendApptNotificationsViaText ?? false;
			patientDemographics.IsTextForOrderNotifications = patient.SendOrderNotificationsViaText ?? false;
            patientDemographics.IsBadAddress = patient.IsBadAddress ?? false;
            patientDemographics.IsBadEmail = patient.IsBadEmail ?? false;

            patientDemographics.UrgentNote = this.IsNotesUrgent(patient.ID);
            patientDemographics.FollowUpNote = this.IsNotesFollowUp(patient.ID);
            ////patientDemographics.UrgentNote = patient.Notes != null ? patient.Notes.Any(i => i.IsUrgent == true || i.IsFollowup == true) : false;
            string error;
            patientDemographics.OutstandingBalance = this.it2Manager.GetCustomerBalance(id, out error);
            if (!string.IsNullOrEmpty(error))
            {
                Logger.Error(error);
            }

            if (patientDemographics.Id > 0 && saveQuicklist && patientDemographics.IsPatient)
            {
                var oph = new OfficePatientHistory { OfficeNum = officeNumber, PatientID = patientDemographics.Id };
                this.officeServices.SaveOfficePatientHistory(oph);
            }

            patientDemographics.PatientCredit = this.it2Manager.GetPatientCreditAmount(id);
            string errorMessage;
            patientDemographics.PatientInsuranceBalance = this.it2Manager.GetPatientInsuranceBalance(id, out errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Logger.Error(errorMessage);
            }

            return Request.CreateResponse(HttpStatusCode.OK, patientDemographics);
        }

        // PUT api/<controller>/5

        /// <summary>The put.</summary>
        /// <param name="officeNumber">The office Number.</param>
        /// <param name="patientDemographics">The patient Demographics.</param>
        /// <param name="userId">The user Id.</param>
        /// <returns>The error message/&gt;.</returns>
        public HttpResponseMessage Put(string officeNumber, [FromBody]PatientDemographics patientDemographics, int userId)
        {
            if (patientDemographics == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (patientDemographics.Address == null)
            {
                patientDemographics.Address = new Model.Address();
            }

            if (patientDemographics.DateOfBirth == null)
            {
                patientDemographics.DateOfBirth = string.Empty;
            }

            try
            {
                Patient patient;
                if (patientDemographics.Id == 0)
                {
                    patientDemographics.Active = true;
                    patient = new Patient();
                    string companyId = OfficeHelper.GetCompanyId(officeNumber);
                    CompanyInformation company = this.officeServices.GetCompanyInformationByID(companyId);
                    patient.Company = company;
                    patient.HomeOffice = officeNumber;
                }
                else
                {
                    patient = this.patientServices.GetByID(patientDemographics.Id);
                    if (patient == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }

                    ////patientDemographics.UrgentNote = patient.Notes != null && patient.Notes.Any(i => i.IsUrgent == true || i.IsFollowup == true);
                    patientDemographics.UrgentNote = this.IsNotesUrgent(patient.ID);
                    patientDemographics.FollowUpNote = this.IsNotesFollowUp(patient.ID);

                    //// Update name changes
                    if ((patient.FirstName != patientDemographics.FirstName)
                        || (patient.LastName != patientDemographics.LastName))
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info(string.Format("Patient name changed from {0}, {1} to {2}, {3}", patient.LastName, patient.FirstName, patientDemographics.LastName, patientDemographics.FirstName));
                        }

                        var note = new Note
                        {
                            Detail =
                                string.Format(
                                    "Previous Last Name: {0}, Previous First Name: {1}",
                                    patient.LastName,
                                    patient.FirstName),
                            EmployeeID = userId,
                            Employee = this.employeeServices.GetByUserID(userId).User,
                            EntityType = EntityType.PATIENT,
                            EntityID = patient.ID,
                            PatientID = patient.ID,
                            Type = NoteType.PATIENT_NAME_CHANGE,
                            NoteDate = DateTime.Now
                        };

                        if (!(patient.Notes.Count > 0))
                        {
                            patient.Notes = new List<Note>();
                        }

                        patient.Notes.Add(note);

                        if (patient.Insurances != null)
                        {
                            foreach (var patientInsurance in patient.Insurances.Where(patientInsurance => patientInsurance.RelationshipID == RelationshipType.SELF))
                            {
                                if (Logger.IsInfoEnabled)
                                {
                                    Logger.Info(string.Format("Changing name for subscriber with id {0}", patientInsurance.Subscriber.ID));
                                }

                                patientInsurance.Subscriber.FirstName = patientDemographics.FirstName;
                                patientInsurance.Subscriber.LastName = patientDemographics.LastName;
                            }
                        }
                    }
                }

                patient.FirstName = patientDemographics.FirstName;
                patient.LastName = patientDemographics.LastName;
                patient.Middle = patientDemographics.MiddleInitial;
                patient.Title = patientDemographics.Title != 0 ? this.lookupServices.GetById(patientDemographics.Title) : null;
                patient.NickName = patientDemographics.NickName;
                patient.ProviderEmployeeId = patientDemographics.Provider;
                patient.IsPatient = patientDemographics.IsPatient;
                patient.IsResponsibleParty = patientDemographics.IsResponsibleParty;
                patient.EmploymentStatus = patientDemographics.EmploymentStatus;
                patient.HomeOffice = patientDemographics.HomeOffice;

                if (patientDemographics.ResponsiblePartyId > 0)
                {
                    patient.ResponsiblePartyID = patientDemographics.ResponsiblePartyId;
                }
                else
                {
                    if (Convert.ToInt32(patient.ResponsiblePartyID) > 0)
                    {
                        //// Reverting to Self
                        //// Inactivate the copied insurances from the responsible party
                        var relationshipsIt2Manager = new PatientRelationshipsIt2Manager();
                        relationshipsIt2Manager.InactivateInsuracesCopiedFromResponsibleParty(patient.ID, Convert.ToInt32(patient.ResponsiblePartyID));
                    }

                    patient.ResponsiblePartyID = null;  ////self
                }

                if (patientDemographics.PatientInsuranceId != 0)
                {
                    if (patient.Insurances != null && patient.Insurances.Count > 0)
                    {
                        var pi = patient.Insurances.First(x => (x.ID == patientDemographics.PatientInsuranceId));
                        pi.EmployerName = patientDemographics.EmployerName;
                    }
                }

                if (!string.IsNullOrEmpty(patientDemographics.DateOfBirth))
                {
                    patient.BirthDate = Convert.ToDateTime(patientDemographics.DateOfBirth).ToString("yyyy-MM-dd HH:mm:ss.fff");
                }

                patient.SSN = string.IsNullOrEmpty(patientDemographics.SocialSecurityNumber) ? patientDemographics.SocialSecurityNumber : Regex.Replace(patientDemographics.SocialSecurityNumber, "[^0-9]", string.Empty);
                patient.Email = patientDemographics.Email;
                patient.MedicalRecordNumber = patientDemographics.MedicalRecordNumber;
                patient.Sex = patientDemographics.Sex;
                patient.Deceased = patientDemographics.Deceased;
                patient.InActive = !patientDemographics.Active;
                patient.IsBadAddress = patientDemographics.IsBadAddress;
                patient.IsBadEmail = patientDemographics.IsBadEmail;
                if (patientDemographics.Deceased)
                {
                    patient.InActive = true;
                }

                patient.SpecialNeeds = patientDemographics.SpecialNeeds;
                if (patient.Activities == null || patient.Activities.Count != 0 || (patientDemographics.Activities != null && patientDemographics.Activities.Count != 0))
                {
                    patient.Activities = this.GetPatientLookups(patientDemographics.Activities);
                }

                patient.Occupation = patientDemographics.Occupation != 0 ? this.lookupServices.GetById(patientDemographics.Occupation) : null;

                if (patientDemographics.MaritalStatus != 0)
                {
                    var ms = this.lookupServices.GetById(patientDemographics.MaritalStatus);
                    patient.MaritalStatus = ms;
                    patient.MaritalStatusID = (MaritalStatusType)ms.ID;
                }
                else
                {
                    patient.MaritalStatus = null;
                    patient.MaritalStatusID = null;
                }

                /////ALSL-1771
                var races = this.GetRaceLookups(patientDemographics.Races);
                if (patient.Races == null || races.Count != patient.Races.Count)
                {
                    ////List of races has changed so do the costly update which performs deletes and inserts
                    patient.Races = races;
                }
                else
                {
                    if (patient.Races.Any(race => !races.Contains(race)))
                    {
                        ////List of races has changed so do the costly update which performs deletes and inserts
                        patient.Races = races;
                    }
                }

                patient.Ethnicity = patientDemographics.Ethnicity != 0 ? this.lookupServices.GetById(patientDemographics.Ethnicity) : null;
                patient.PreferredLanguage = patientDemographics.PreferredLanguage != 0 ? this.lookupServices.GetById(patientDemographics.PreferredLanguage) : null;
                var communicationPreference = CommunicationPreferenceExtension.ToIt2CommunicationPreference(patientDemographics.CommunicationPreference);
                patient.CommunicationPreference = communicationPreference >= 0 ? communicationPreference : -1;

                patient.emailPromo = patientDemographics.IsEmailForPromotions;
                patient.emailRecall = patientDemographics.IsEmailForRecalls;
                patient.IsDoNotSendMailOffers = patientDemographics.IsDoNotSendMailOffers;
                patient.Signature = patientDemographics.IsHipaaSignatureOnFile;
				patient.SendApptNotificationsViaText = patientDemographics.IsTextForApptNotifications;
				patient.SendOrderNotificationsViaText = patientDemographics.IsTextForOrderNotifications;
                if (patientDemographics.Referral != null && patientDemographics.Referral != 0)
                {
                    patient.ReferralID = patientDemographics.Referral;
                }
                else
                {
                    patient.ReferralID = null;
                }

                ////Save Primary Address
                var addresses = new List<IT2.Core.Address>(patient.Addresses);
                if (patientDemographics.Address == null)
                {
                    if (patient.PrimaryAddress != null)
                    {
                        var address = patient.PrimaryAddress;
                        address.City = null;
                        address.Address1 = null;
                        address.Address2 = null;
                        address.ZipCode = null;
                        address.State = null;
                        var addressIndex = patient.Addresses.IndexOf(addresses.Find(a => (a.ID == address.ID)));
                        patient.Addresses[addressIndex] = address;
                    }
                }
                else
                {
                    var address = patient.PrimaryAddress ?? new IT2.Core.Address();
                    address.IsPrimary = true;
                    address.City = patientDemographics.Address.City;
                    address.Address1 = patientDemographics.Address.Address1;
                    address.Address2 = patientDemographics.Address.Address2;
                    address.ZipCode = patientDemographics.Address.ZipCode;
                    if (patientDemographics.Address.State != null)
                    {
                        address.State = patientDemographics.Address.State.ToUpper();
                    }

                    address.ZipCode = patientDemographics.Address.ZipCode;
                    if (address.ID == 0)
                    {
                        patient.Addresses.Add(address);
                    }

                    address.AddressTypeID = AddressType.HOME;
                }

                ////Save phone number changes
                var phones = new List<IT2.Core.Phone>(patient.Phones);
                if (string.IsNullOrEmpty(patientDemographics.PrimaryPhone))
                {
                    if (patient.PrimaryPhone != null)
                    {
                        var phone = patient.PrimaryPhone;
                        phone.PhoneNumber = string.Empty;
                        phone.CallTimeLU = null;
                        phone.PhoneTypeID = null;
                        var phoneIndex = patient.Phones.IndexOf(phones.Find(a => a.ID == phone.ID));
                        patient.Phones[phoneIndex] = phone;
                    }
                }
                else
                {
                    var phone = patient.PrimaryPhone ?? new IT2.Core.Phone();
                    phone.PhoneNumber = patientDemographics.PrimaryPhone.Trim()
                                             .Replace("(", string.Empty)
                                             .Replace(")", string.Empty)
                                             .Replace("-", string.Empty)
                                             .Replace(" ", string.Empty);
                    phone.CallTimeLU = patientDemographics.PrimaryPhoneCallTime == 0 ? null : this.lookupServices.GetById(patientDemographics.PrimaryPhoneCallTime);
                    phone.PhoneTypeID = patientDemographics.PrimaryPhoneType == 0 ? null : this.lookupServices.GetById(patientDemographics.PrimaryPhoneType);
                    phone.IsPrimary = true;
                    if (phone.ID == 0)
                    {
                        patient.Phones.Add(phone);
                    }
                }

                var secondaryPhone = patient.SecondaryPhone ?? new IT2.Core.Phone();
                if (!string.IsNullOrEmpty(patientDemographics.SecondaryPhone))
                {
                    secondaryPhone.PhoneNumber =
                        patientDemographics.SecondaryPhone.Trim()
                                           .Replace("(", string.Empty)
                                           .Replace(")", string.Empty)
                                           .Replace("-", string.Empty)
                                           .Replace(" ", string.Empty);
                    secondaryPhone.CallTimeLU = patientDemographics.SecondaryPhoneCallTime == 0
                                                    ? null
                                                    : this.lookupServices.GetById(
                                                        patientDemographics.SecondaryPhoneCallTime);
                    secondaryPhone.PhoneTypeID = patientDemographics.SecondaryPhoneType == 0
                                                     ? null
                                                     : this.lookupServices.GetById(patientDemographics.SecondaryPhoneType);
                    secondaryPhone.IsPrimary = false;
                    if (secondaryPhone.ID == 0)
                    {
                        patient.Phones.Add(secondaryPhone);
                    }
                    else
                    {
                        var phoneIndex = patient.Phones.IndexOf(phones.Find(a => a.ID == secondaryPhone.ID));
                        patient.Phones[phoneIndex] = secondaryPhone;
                    }
                }
                else
                {
                    if (secondaryPhone.ID != 0 && patient.Phones.Count > 1)
                    {
                        var phoneIndex = patient.Phones.IndexOf(phones.Find(a => a.ID == secondaryPhone.ID));
                        var phone = patient.Phones[phoneIndex];
                        if (phone.IsPrimary == false)
                        {
                            patient.Phones.Remove(phone);
                        }
                    }
                }

                // Save Patient
                string errorMessage;
                var ehrEnabled = false;

                ////check to see if it EHR is enabled or not.
                var ehrIntegration = new AdditionalIntegrationsController().GetModel(officeNumber);
                if (ehrIntegration.EhrIntegration)
                {
                    ehrEnabled = true;
                }

                this.patientServices.Save(patient, ehrEnabled, out errorMessage);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logger.Error(errorMessage);
                }
     
                if (patientDemographics.Id == 0)
                {
                    var newPatient = this.patientServices.GetByID(patient.ID);
                    patientDemographics.Id = newPatient.ID;
                    patientDemographics.IsPatient = newPatient.IsPatient;
                }

                if (patientDemographics.IsPatient)
                {
                    var oph = new OfficePatientHistory { OfficeNum = officeNumber, PatientID = patientDemographics.Id };
                    this.officeServices.SaveOfficePatientHistory(oph);
                }

                //// ToDo: Add Audit Log and Update the save function with Office Data preferences.
                //// this.patientServices.Save(patient, userId, OfficeData.IsIntegratedEMROffice, Common.ConvertDateTimeToTimeZone(DateTime.Now, OfficeData.TimeZone, OfficeData.UseDST), RemoteIPvalue, OfficeData.AuditLogLevel, out errorMessage);
                //// Add Audit Log

                return Request.CreateResponse(HttpStatusCode.OK, patientDemographics);
            }
            catch (Exception ex)
            {
                var error = string.Format("Put( patientId = {0} {1} {2}", patientDemographics.Id, "\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get the communication preferences.
        /// </summary>
        /// <returns>
        /// The lookup list
        /// </returns>
        public List<Lookup> GetCommunicationPreferences()
        {
            var items = Enum.GetValues(typeof(CommunicationPreferences));
            var result = new List<Lookup>();

            foreach (var x in from object item in items select new Lookup { Key = (int)item })
            {
                x.Description = Enum.GetName(typeof(CommunicationPreferences), x.Key);
                result.Add(x);
            }

            return new List<Lookup>(result.OrderBy(a => a.Description));
        }

        /// <summary>
        /// The get the Employment Status.
        /// </summary>
        /// <returns>
        /// The lookup list
        /// </returns>
        public List<Lookup> GetEmploymentStatus()
        {
            var items = Enum.GetValues(typeof(Employmentstatus));
            var result = new List<Lookup>();

            foreach (var x in from object item in items select new Lookup { Key = (int)item })
            {
                x.Description = Enum.GetName(typeof(Employmentstatus), x.Key);
                result.Add(x);
            }

            return new List<Lookup>(result.OrderBy(a => a.Description));
        }

        /// <summary>
        /// The get patient summary by id.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetPatientSummaryById(string officeNumber, int id)
        {
            PatientDemographics patientSummary;
            Patient patient;

            // make sure we have a valid patient ID
            if (id <= 0)
            {
                Logger.Error("GetPatientSummaryById(Id=" + id + ")\n" + "Could not find patient.");
                return Request.CreateResponse(HttpStatusCode.PreconditionFailed);
            }

            // get the patient object
            try
            {
                patient = this.patientServices.GetByID(id);
            }
            catch (NHibernate.ObjectNotFoundException e)
            {
                Logger.Warn("GetPatientSummaryById(Id=" + id + ")\n" + e);
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            // make sure we have a patient before proceeding
            if (patient == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            // build the PatientSummary object
            try
            {
                var provider = patient.ProviderEmployeeId ?? 0;
                patientSummary = new PatientDemographics
                {
                    Id = patient.ID,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    MiddleInitial = patient.Middle,
                    Sex = patient.Sex,
                    Age = patient.Age.HasValue ? patient.Age.ToString() : string.Empty,
                    LastExamDate =
                                                 patient.LastExamDate.HasValue
                                                     ? patient.LastExamDate.Value.ToShortDateString()
                                                     : "Never",
                    NickName = patient.NickName,
                    Provider = provider,
                    Address = new Model.Address()
                };
                if (provider > 0)
                {
                    var defaultProvider = this.employeeServices.GetByID(provider);
                    patientSummary.ProviderName = defaultProvider.FullName;
                }

                if (!string.IsNullOrEmpty(patient.SSNDisplay))
                {
                    if (patient.SSNDisplay.Length >= 9)
                    {
                        double ssn;
                        double.TryParse(patient.SSNDisplay.Substring(0, 9), out ssn);
                        patientSummary.SocialSecurityNumber = string.Format("{0:###-##-####}", ssn);
                    }
                    else
                    {
                        patientSummary.SocialSecurityNumber = patient.SSNDisplay;
                    }
                }

                patientSummary.Email = patient.EmailDisplay;

                var dateOfBirth = Convert.ToDateTime(patient.BirthDateDisplay);
                if (dateOfBirth != DateTime.MinValue)
                {
                    patientSummary.DateOfBirth = dateOfBirth.ToString("d");
                }

                if (patient.PrimaryAddress != null)
                {
                    patientSummary.Address = patient.PrimaryAddress.ToModel();
                }

                var helper = new Helper();
                if (patient.PrimaryPhone != null)
                {
                    patientSummary.PrimaryPhone = helper.FormatPhone(patient.PrimaryPhone.PhoneNumberDisplay);
                }

                if (patient.SecondaryPhone != null)
                {
                    patientSummary.SecondaryPhone = helper.FormatPhone(patient.SecondaryPhone.PhoneNumberDisplay);
                }

                patientSummary.OutstandingBalance = this.transactionServices.GetCustomerBalance(id);
            }
            catch (Exception e)
            {
                Logger.Error("GetPatientSummaryById(Id=" + id + "): Error while creating PatientSummary object\n" + e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, patientSummary);
        }

        /// <summary>
        /// The get responsible party info.
        /// </summary>
        /// <param name="dem">
        /// The dem.
        /// </param>
        /// The Company ID
        /// <param name="patient">
        /// The patient.
        /// </param>
        /// <param name="companyId">
        /// The Company Id.
        /// </param>
        public void GetResponsiblePartyInfo(PatientDemographics dem, Patient patient, String companyId)
        {
            if (patient.ResponsibleParty != null)
            {
                // verify that the responsible party belongs to the same company as the user who is looking up the data
                if (patient.ResponsibleParty.Company != null &&
                    patient.ResponsibleParty.Company.ID != null &&
                    patient.ResponsibleParty.Company.ID == companyId)
                {
                    dem.ResponsiblePartyId = patient.ResponsiblePartyID ?? 0;
                    dem.ResponsibleName = patient.ResponsibleParty.FirstName + " " + patient.ResponsibleParty.LastName;
                    var patientInsurance = patient.GetFirstInsurance();
                    if (patientInsurance != null)
                    {
                        dem.EmployerName = patientInsurance.EmployerName;
                        dem.PatientInsuranceId = patientInsurance.ID;
                    }
                }
                else
                {
                    Logger.Error("Found a Patient with a Responsible Party that belongs to a different office. [CompanyId="
                        + companyId + ", PatientId=" + patient.ID + ", ResponsiblePartyCompanyId=" + patient.ResponsibleParty.Company.ID
                        + ", ResponsiblePartyPatientId=" + patient.ResponsiblePartyID + "] ");
                }
            }
            else
            {
                dem.ResponsibleName = string.Empty;
                dem.EmployerName = string.Empty;
                if (dem.ResponsiblePartyId == 0)
                {
                    return;
                }

                Patient p;
                try
                {
                    p = this.patientServices.GetByID(dem.ResponsiblePartyId);
                }
                catch (NHibernate.ObjectNotFoundException)
                {
                    return;
                }

                dem.ResponsibleName = p.FirstName + " " + p.LastName;
                if (p.Insurances == null)
                {
                    return;
                }

                if (p.Insurances.Count <= 0)
                {
                    return;
                }

                var pis =
                    p.Insurances.Where(t => !string.IsNullOrEmpty(t.EmployerName))
                        .OrderByDescending(t => t.InputDate)
                        .ToList();
                if (pis.Count <= 0)
                {
                    return;
                }

                dem.EmployerName = pis[0].EmployerName;
                dem.PatientInsuranceId = pis[0].ID;
            }
        }

        /// <summary>
        /// This loads all lookuptypes
        /// </summary>
        /// <param name="patientDemographics"></param>
        private void LoadAllLookupTypes(ref PatientDemographics patientDemographics)
        {
            var lookupTypes =
                this.lookupServices.GetAll(
                    new LookupType[]
                        {
                            LookupType.ACTIVITY, LookupType.PHONE_CALLTIME, LookupType.OCCUPATION,
                            LookupType.PATIENT_PHONE, LookupType.TITLE, LookupType.MARITAL_STATUS, LookupType.Race,
                            LookupType.Ethnicity, LookupType.Prefered_Language, LookupType.REFERRAL_TYPE,
                            LookupType.EMPLOYMENT_STATUS
                        });

            if (lookupTypes != null)
            {
                patientDemographics.ActivityTypes = this.memoryCache.Get(
                    LookupType.ACTIVITY.ToString(),
                    () =>
                    lookupTypes.Where(i => i.Type == LookupType.ACTIVITY)
                        .OrderBy(i => i.Description)
                        .Select(i => new Lookup(i.ID, i.Description)));
                patientDemographics.CallTimeTypes = this.memoryCache.Get(
                    LookupType.PHONE_CALLTIME.ToString(),
                    () =>
                    lookupTypes.Where(i => (i.Type == LookupType.PHONE_CALLTIME) && (i.Description != "Never"))
                        .Select(i => new Lookup(i.ID, i.Description)));
                patientDemographics.OccupationTypes = this.memoryCache.Get(
                    LookupType.OCCUPATION.ToString(),
                    () =>
                    lookupTypes.Where(i => i.Type == LookupType.OCCUPATION).Select(i => new Lookup(i.ID, i.Description)));
                patientDemographics.PhoneTypes = this.memoryCache.Get(
                    LookupType.PATIENT_PHONE.ToString(),
                    () =>
                    lookupTypes.Where(i => i.Type == LookupType.PATIENT_PHONE)
                        .Select(i => new Lookup(i.ID, i.Description)));
                patientDemographics.TitleTypes = this.memoryCache.Get(
                    LookupType.TITLE.ToString(),
                    () =>
                    lookupTypes.Where(i => i.Type == LookupType.TITLE).Select(i => new Lookup(i.ID, i.Description)));
                patientDemographics.MaritalStatusTypes = this.memoryCache.Get(
                    LookupType.MARITAL_STATUS.ToString(),
                    () =>
                    lookupTypes.Where(i => i.Type == LookupType.MARITAL_STATUS)
                        .Select(i => new Lookup(i.ID, i.Description)));
                patientDemographics.RaceTypes = this.memoryCache.Get(
                    LookupType.Race.ToString(),
                    () => lookupTypes.Where(i => i.Type == LookupType.Race).Select(i => new Lookup(i.ID, i.Description)));
                patientDemographics.EthnicityTypes = this.memoryCache.Get(
                    LookupType.Ethnicity.ToString(),
                    () =>
                    lookupTypes.Where(i => i.Type == LookupType.Ethnicity).Select(i => new Lookup(i.ID, i.Description)));
                patientDemographics.PreferredLanguageTypes =
                    this.memoryCache.Get(
                        LookupType.Prefered_Language.ToString(),
                        () =>
                        lookupTypes.Where(i => i.Type == LookupType.Prefered_Language)
                            .Select(i => new Lookup(i.ID, i.Description)));
                patientDemographics.ReferredByTypes = this.memoryCache.Get(
                    LookupType.REFERRAL_TYPE.ToString(),
                    () =>
                    lookupTypes.Where(i => i.Type == LookupType.REFERRAL_TYPE)
                        .Select(i => new Lookup(i.ID, i.Description)));
                patientDemographics.EmploymentStatusTypes = this.memoryCache.Get(
                    LookupType.EMPLOYMENT_STATUS.ToString(),
                    () =>
                    lookupTypes.Where(i => i.Type == LookupType.EMPLOYMENT_STATUS)
                        .Select(i => new Lookup(i.ID, i.Description)));
            }
        }

        /// <summary>The get all lookup types.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The List of Lookup Types</returns>
        private List<Lookup> GetAllLookupTypes(LookupType type)
        {
            var items = new List<Lookup>();
            var lookupTypes = this.lookupServices.GetAll(type, type);
            if (lookupTypes != null)
            {
                items = (
                    from lookupType in lookupTypes
                    orderby lookupType.Ordinal
                    select
                    new Lookup(lookupType.ID, lookupType.Description)).ToList();
            }

            return items;
        }

        /// <summary>The get activities.</summary>
        /// <param name="patientActivities">The patient activities.</param>
        /// <returns>The Activities</returns>
        private IEnumerable<int> GetActivities(IEnumerable<GenLookup> patientActivities)
        {
            var items = new List<int>();
            if (patientActivities != null)
            {
                items = (from lookupType in patientActivities select lookupType.ID).ToList();
            }

            return items;
        }

        /// <summary>The get patient lookups.</summary>
        /// <param name="lookups">The lookups.</param>
        /// <returns>The Patient lookup List</returns>
        private List<GenLookup> GetPatientLookups(IEnumerable<int> lookups)
        {
            var items = new List<GenLookup>();
            if (lookups != null)
            {
                items =
                    (from lookupType in lookups
                     select this.lookupServices.GetById(lookupType)).ToList();
            }

            return items;
        }

        private bool GetECRVaultFlag()
        {
            throw new NotImplementedException();
        }

        /// <summary>The get patient lookups.</summary>
        /// <param name="lookups">The lookups.</param>
        /// <returns>The Patient lookup List</returns>
        private List<GenLookup> GetRaceLookups(IEnumerable<int> lookups)
        {
            var items = new List<GenLookup>();
            if (lookups != null)
            {
                items =
                    (from lookupType in lookups
                     select this.lookupServices.GetById(lookupType)).ToList();
            }

            return items;
        }

        /// <summary>The search.</summary>
        /// <param name="companyId">The office number.</param>
        /// <param name="active">The active.</param>
        /// <returns>The Result.</returns>
        private List<Lookup> GetEmployeesInCompany(string companyId, bool active)
        {
            var employees = this.employeeServices.GetEmployeesInCompany(companyId, active);
            var result = new List<Lookup>();
            if (employees != null && employees.Count > 0)
            {
                foreach (var employee in employees.OrderBy(i => i.FirstName).Where(a => a.EmployeeType == (int)EmployeeType.Provider))
                {
                    result.Add(new Lookup(employee.ID, "Dr. " + employee.FirstName + " " + employee.LastName));
                }
            }

            return result;
        }

        /// <summary>The search.</summary>
        /// <param name="companyId">The company id.</param>
        /// <returns>The Result.</returns>
        private List<Lookup> GetOffices(string companyId)
        {
            var offices = this.officeServices.GetAllLiveOfficeLite(companyId);

            var result = new List<Lookup>();
            if (offices != null && offices.Count > 0)
            {
                result.AddRange(offices.Select(office => new Lookup(office.ID, String.Format("{0} - {1}", office.ID, office.Name))));
            }

            return result;
        }

        /// <summary>Are there any notes with urgent or follow-up?.</summary>
        /// <param name="patientId">The patient id.</param>
        /// <returns>true or false.</returns>
        private bool IsNotesUrgent(int patientId)
        {
            var searchCriteria = new PatientNotesSearchCriteria()
            {
                PatientId = patientId,
                Resource = string.Empty,
                DateFrom = null,
                DateTo = null,
                IsUrgent = true,
                IsSystemNotes = false
            };

            var notesList = new PatientNotesIt2Manager().GetSearchResults(searchCriteria);
            return notesList.Any();
        }

        private bool IsNotesFollowUp(int patientId)
        {
            var searchCriteria = new PatientNotesSearchCriteria()
            {
                PatientId = patientId,
                Resource = string.Empty,
                DateFrom = null,
                DateTo = null,
                IsFollowUp = true,
                IsSystemNotes = false
            };

            var notesList = new PatientNotesIt2Manager().GetSearchResults(searchCriteria);
            return notesList.Any();
        }
    }
}