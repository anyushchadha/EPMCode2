// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourcesController.cs" company="Eyefinity, Inc.">
// Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//  The resources controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;
    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.Enterprise.Business.Common;
    using Eyefinity.Enterprise.Business.Integrations;
    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;

    /// <summary>
    ///     The resources controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class ResourcesController : ApiController
    {
        /// <summary>The logger</summary>
        /// 
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>The Employee it 2 Manager.</summary>
        /// 
        private readonly EmployeeIt2Manager employeeIt2Manager;

        private readonly LogiIntegrationManager logiIntegrationManager;

        /// <summary>The practice information it 2 manager.</summary>
        private readonly OfficeIt2Manager officeIt2Manager;

        private readonly PatientIt2Manager patientIt2Manager;

        /// <summary>Initializes a new instance of the <see cref="ResourcesController" /> class.</summary>
        public ResourcesController()
        {
            this.employeeIt2Manager = new EmployeeIt2Manager();
            this.officeIt2Manager = new OfficeIt2Manager();
            this.patientIt2Manager = new PatientIt2Manager();
            this.logiIntegrationManager = new LogiIntegrationManager();
        }

        /// <summary>The anonymous delegate method</summary>
        public delegate string AllowUpdate();

        /// <summary>The search providers.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="active">The active.</param>
        /// <returns>The Result</returns>
        [HttpGet]
        [Authorize(Roles = "Resource Setup")]
        public IEnumerable<EmployeeSearchResultVm> SearchProviders(
            string officeNumber, string firstName, string lastName, bool active)
        {
            var companyId = this.employeeIt2Manager.GetCompanyId(officeNumber);
            var result = this.employeeIt2Manager.Search(officeNumber, firstName, lastName, active, EmployeeType.Provider)
                           .CurrentItems.Select(e => new EmployeeSearchResultVm
                           {
                               EmployeeNum = e.EmployeeNum,
                               FirstName = e.FirstName,
                               Id = e.EmployeeId,
                               LastName = e.LastName,
                               UserName = e.User == null
                                                            ? string.Empty
                                                            : e.User.Name.ToLower().Replace(GetUserNamePlaceHolderValue(companyId), string.Empty),
                                                          Npi = e.NationalProviderId,
                                                          Ssn = e.SsnNumber ?? string.Empty,
                                                          UserId = e.UserId,
                                                          EmployeeType = (int)EmployeeType.Provider
                                                      })
                           .OrderBy(e => e.LastName);
            return result;
        }

        /// <summary>The search outside providers.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="active">The active.</param>
        /// <returns>The Result</returns>
        [HttpGet]
        [Authorize(Roles = "Resource Setup")]
        public SearchResult<OutsideProvider> SearchOutsideProviders(
            string officeNumber, string firstName, string lastName, bool active)
        {
            var mngr = new ResourcesIt2Manager();
            var outsideProviders = mngr.GetOutsideDoctorByName(firstName, lastName, this.employeeIt2Manager.GetCompanyId(officeNumber), active); 
            var result = new SearchResult<OutsideProvider>
            {
                CurrentItems = outsideProviders,
                SearchCriteria = (lastName ?? string.Empty) + (firstName ?? string.Empty) + active
            };

            return result;
        }

        /// <summary>The search resources.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="active">The active.</param>
        /// <returns>The result</returns>
        [HttpGet]
        [Authorize(Roles = "Resource Setup")]
        public IEnumerable<EmployeeSearchResultVm> SearchResources(
            string officeNumber, string firstName, string lastName, bool active)
        {
            var companyId = this.employeeIt2Manager.GetCompanyId(officeNumber);
            var result = this.employeeIt2Manager.Search(officeNumber, firstName, lastName, active, EmployeeType.Staff)
                           .CurrentItems.Select(e => new EmployeeSearchResultVm
                                {
                                    EmployeeNum = e.EmployeeNum,
                                    FirstName = e.FirstName,
                                    Id = e.EmployeeId,
                                    LastName = e.LastName,
                                    UserName = e.User?.Name.ToLower().Replace(GetUserNamePlaceHolderValue(companyId), string.Empty) ?? string.Empty,
                                    Npi = e.NationalProviderId,
                                    Ssn = e.SsnNumber ?? string.Empty,
                                    UserId = e.UserId,
                                    EmployeeType = (int)EmployeeType.Staff 
                                })
                           .OrderBy(e => e.LastName);
            return result;
        }

        /// <summary>The search staff.</summary>
        /// <param name="practiceLocationId">The practice id.</param>
        /// <returns>The result /&gt;.</returns>
        [HttpGet]
        public IEnumerable<EmployeeSearchResultVm> GetInitialStaff(string practiceLocationId)
        {
            var result = this.SearchResources(practiceLocationId, string.Empty, string.Empty, true);
            return result;
        }

        /// <summary>The get employee.</summary>
        /// <param name="officeNumber">The office Number.</param>
        /// <param name="employeeId">The employee id.</param>
        /// <returns>The <see cref="EmployeeVm"/>.</returns>
        [HttpGet]
        [Authorize(Roles = "Resource Setup")]
        public EmployeeVm GetEmployee(string officeNumber, int employeeId)
        {
            var result = new EmployeeVm();
            var mngr = new EmployeeIt2Manager();
            var employeeIt2 = mngr.GetEmployeeFromIt2(employeeId);
            var ehrSystem = mngr.GetEmrSystemFromOfficeEmployee(officeNumber, employeeId);
            var allCompanyOffices = this.employeeIt2Manager.GetCompanyOffices(employeeIt2.CompanyId);

            result.Active = employeeIt2.Active;
            result.CompanyId = employeeIt2.CompanyId;
            result.DeaNumber = employeeIt2.DeaNumber;
            result.EinNumber = employeeIt2.EinNumber;
            result.EmcSubmitterId = employeeIt2.EmcSubmitterId;
            result.EmployeeId = employeeId;
            result.EmployeeNum = employeeIt2.EmployeeNum;
            result.EmployeeType = (EmployeeType)Enum.ToObject(typeof(EmployeeType), employeeIt2.EmployeeType);
            result.EmrId = employeeIt2.EmrId;
            result.Fax = employeeIt2.Fax;
            result.FirstName = employeeIt2.FirstName;
            result.HipaaDate = FormatDate(employeeIt2.HipaaDate);
            result.Hl7ProviderId = employeeIt2.Hl7ProviderId;
            result.LastName = employeeIt2.LastName;
            result.LicenceNumber = employeeIt2.LicenceNumber;
            result.NationalProviderId = employeeIt2.NationalProviderId;
            result.Phone = employeeIt2.Phone;
            result.ProfessionalCredential = employeeIt2.ProfessionalCredential;
            result.ProfessionalSignature = employeeIt2.ProfessionalSignature;
            result.SsnNumber = employeeIt2.SsnNumber;
            result.TpaNumber = employeeIt2.TpaNumber;
            result.UserId = employeeIt2.UserId;
            result.UserName = employeeIt2.User.Name.ToLower().Replace(GetUserNamePlaceHolderValue(employeeIt2.CompanyId), string.Empty);
            result.DefaultExamMinutes = Convert.ToString(employeeIt2.DefaultExamMinutes) + " Min.";
            result.AllowOverbooks = Convert.ToString(employeeIt2.AllowOverbooks);
            result.EyefinityEhr = ehrSystem == 1;
            result.IsWebSchedulable = employeeIt2.IsWebSchedulable;
            result.HomeOffice = employeeIt2.HomeOffice;
            result.IsElectronicNotifiable = employeeIt2.IsElectronicNotifiable;
            result.Offices = allCompanyOffices.Select(o => new EmployeeOfficeVm
            {
                EmployeeId = employeeId,
                OfficeNum = o.ID,
                OfficeName = o.Name,
                IsAuthorizedForAccess = employeeIt2.Offices.Any(a => a.OfficeNum == o.ID)
            }).ToList();

            return result;
        }

        /// <summary>The get outside provider.</summary>
        /// <param name="officeNumber">The office Number.</param>
        /// <param name="outsideProviderId">The outside provider id.</param>
        /// <returns>The <see cref="EmployeeVm"/>.</returns>
        [HttpGet]
        [Authorize(Roles = "Resource Setup")]
        public IEnumerable<OutsideProvider> GetOutsideProvider(string officeNumber, int outsideProviderId)
        {
            var mngr = new ResourcesIt2Manager();
            var outsideProvider = mngr.GetOutsideDoctorById(outsideProviderId);
            return outsideProvider;
        }

        /// <summary>The reset password.</summary>
        /// <param name="employeeId">The employee id.</param>
        /// <param name="defaultPassword">The default password.</param>
        [HttpPost]
        [Authorize(Roles = "Reset Resource Password")]
        public void ResetPassword(int employeeId, string defaultPassword)
        {
            this.employeeIt2Manager.ResetPassword(employeeId, defaultPassword);
        }

        /// <summary>The get random password.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        [HttpGet]
        [Authorize(Roles = "Reset Resource Password")]
        public string GetRandomPassword()
        {
            return this.employeeIt2Manager.GetRandomPassword();
        }

        /// <summary>The add outside provider information.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="outsideProvider">The outside provider info.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPost]
        [Authorize(Roles = "Resource Setup")]
        public HttpResponseMessage AddOutsideProviderInformation(string officeNumber, [FromBody] OutsideProvider outsideProvider)
        {
            outsideProvider.CompanyId = this.employeeIt2Manager.GetCompanyId(officeNumber);
            var mngr = new ResourcesIt2Manager();
            var doctorId = mngr.OutsideProviderSaveOrUpdate(outsideProvider);

            return (doctorId > 0) ? this.Request.CreateResponse(HttpStatusCode.OK, new { validationmessage = "Outside Provider Added Successfully.", id = string.Empty + doctorId })
                                  : this.Request.CreateResponse(HttpStatusCode.BadRequest, new { validationmessage = "Unable to add this Outside Provider." });
        }

        /// <summary>The add provider information.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="providerInfo">The provider info.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPost]
        [Authorize(Roles = "Resource Setup")]
        public HttpResponseMessage AddProviderInformation(string officeNumber, [FromBody] EmployeeVm providerInfo)
        {
            var companyId = this.employeeIt2Manager.GetCompanyId(officeNumber);
            var userName = string.Concat(providerInfo.UserName, GetUserNamePlaceHolderValue(companyId));
            var doesLoginNameExist = CheckForExistingLoginName(userName, companyId);
            var doesProviderIdExist = CheckForExistingProviderId(providerInfo.EmployeeNum, companyId);
            string validationString;
            var pattern = string.Empty;
            pattern += doesLoginNameExist ? "1" : "0";
            pattern += doesProviderIdExist ? "1" : "0";
            pattern += "0"; //// Product management decided to remove this validation

            switch (pattern)
            {
                case "111":
                    validationString = "The NPI Number already exists in the system. Enter a unique NPI Number for this provider.<br/> The Login Name already exists in the system. Enter a unique Login Name for this provider.<br/>The ID already exists in the system. Enter a unique ID for this provider.";
                    break;
                case "001":
                    validationString = "The NPI Number already exists in the system. Enter a unique NPI number for this provider.";
                    break;
                case "101":
                    validationString = "The NPI Number already exists in the system. Enter a unique NPI number for this provider.<br/>The Login Name already exists in the system. Enter a unique Login Name for this provider.";
                    break;
                case "110":
                    validationString = "The Login Name already exists in the system. Enter a unique Login Name for this provider.<br/>The ID already exists in the system. Enter a unique ID for this provider.";
                    break;
                case "011":
                    validationString = "The NPI Number already exists in the system. Enter a unique NPI number for this provider.<br/>The ID already exists in the system. Enter a unique ID for this provider.";
                    break;
                case "100":
                    validationString = "The Login Name already exists in the system. Enter a unique Login Name for this provider.";
                    break;
                case "010":
                    validationString = "The ID already exists in the system. Enter a unique ID.";
                    break;
                default:
                    validationString = string.Empty;
                    break;
            }

            if (validationString == string.Empty)
            {
                providerInfo.EmployeeType = EmployeeType.Provider;
                var employee = providerInfo.ToEmployee();
                employee.User.Name = userName;
                this.employeeIt2Manager.AddEmployee(officeNumber, employee);
                this._UpdateEmployeeOffices(providerInfo, employee, true);
                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        validationmessage = "Provider Added Successfully.",
                        employeeId = employee.EmployeeId,
                        userId = employee.UserId,
                        employeeType = Convert.ToInt32(providerInfo.EmployeeType),
                        isNew = true
                    });
            }

            Logger.Error("AddProviderInformation: " + validationString);
            return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { validationmessage = validationString });
        }

        /// <summary>The add provider information.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="staffInfo">The provider info.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPost]
        [Authorize(Roles = "Resource Setup")]
        public HttpResponseMessage AddStaffInformation(string officeNumber, [FromBody] EmployeeVm staffInfo)
        {
            var companyId = this.employeeIt2Manager.GetCompanyId(officeNumber);
            var userName = string.Concat(staffInfo.UserName, GetUserNamePlaceHolderValue(companyId));
            var doesStaffIdExist = CheckForExistingStaffId(staffInfo.EmployeeNum, companyId);
            var doesLoginNameExist = CheckForExistingLoginName(userName, companyId);
            var validationString = string.Empty;

            if (doesStaffIdExist && doesLoginNameExist)
            {
                validationString = "The Staff ID already exists in the system. Enter a unique Staff ID.<br/>"
                                 + "The Login Name already exists in the system. Enter a unique Login Name.";
            }
            else if (doesStaffIdExist)
            {
                validationString = "The Staff ID already exists in the system. Enter a unique Staff ID.";
            }
            else if (doesLoginNameExist)
            {
                validationString = "The Login Name already exists in the system. Enter a unique Login Name.";
            }
            else
            {
                staffInfo.ProfessionalCredential = string.Empty;
                staffInfo.EmployeeType = EmployeeType.Staff;
                var employee = staffInfo.ToEmployee();
                employee.User.Name = userName;

                this.employeeIt2Manager.AddStaff(officeNumber, employee);
                this._UpdateEmployeeOffices(staffInfo, employee, true);
                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        validationmessage = "Staff Added Successfully.",
                        employeeId = employee.EmployeeId,
                        userId = employee.UserId,
                        employeeType = Convert.ToInt32(staffInfo.EmployeeType),
                        isNew = true
                    });
            }

            Logger.Error("AddStaffInformation: " + validationString);
            return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { validationmessage = validationString });
        }

        /// <summary>The update provider information.</summary>
        /// <param name="officeNumber">The office Number.</param>
        /// <param name="outsideProvider">The outside provider info.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPut]
        [Authorize(Roles = "Resource Setup")]
        public HttpResponseMessage UpdateOutsideProviderInformation(string officeNumber, [FromBody] OutsideProvider outsideProvider)
        {
            outsideProvider.CompanyId = this.employeeIt2Manager.GetCompanyId(officeNumber);
 		    var mngr = new ResourcesIt2Manager();
            mngr.OutsideProviderSaveOrUpdate(outsideProvider);
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new { validationmessage = "Outside Provider Updated Successfully." });
        }

        /// <summary>The add provider information.</summary>
        /// <param name="officeNumber">The office Number.</param>
        /// <param name="providerInfo">The provider info.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPut]
        [Authorize(Roles = "Resource Setup")]
        public HttpResponseMessage UpdateProviderInformation(string officeNumber, [FromBody] EmployeeVm providerInfo)
        {
            AllowUpdate allowUpdate = delegate
            {
                ////Deactivate this provider then remove it linked to any patient
                var msg = string.Empty;
                if (providerInfo.Active == false)
                {
                    var practiceInfo = this.officeIt2Manager.GetCompanyInformation(OfficeHelper.GetCompanyId(officeNumber), officeNumber);
                    if (practiceInfo.ContactInformation.Owner != providerInfo.EmployeeId)
                    {
                        var patientIds = PatientManager.GetProvidersPatients(providerInfo.EmployeeId, officeNumber);
                        if (patientIds.Count > 0)
                        {
                            foreach (var t in patientIds)
                            {
                                this.patientIt2Manager.UpdatePatientsDoctor(t, practiceInfo.ContactInformation.Owner);
                            }
                        }
                    }
                    else
                    {
                        msg += "Unable to deactivate this provider because he/she is a current Company Owner.<br/>";
                    }
                }

                return msg;
            };

            bool valid;
            var companyId = this.employeeIt2Manager.GetCompanyId(officeNumber);
            var userName = string.Concat(providerInfo.UserName, GetUserNamePlaceHolderValue(companyId));
            var doesLoginNameExist = CheckForExistingLoginName(userName, companyId);
            ////var doesUserIdExist = CheckForExistingUserId(userName, companyId);
            var doesProviderIdExist = CheckForExistingProviderId(providerInfo.EmployeeNum, companyId);
            string validationString;

            if (doesProviderIdExist || doesLoginNameExist)
            {
                providerInfo.EmployeeType = EmployeeType.Provider;
                var employee = providerInfo.ToEmployee();
                employee.User.Name = userName;
                var isLoginNameChanged = this.employeeIt2Manager.CheckIfLoginChanged(employee);
                var isProviderIdChanged = this.employeeIt2Manager.CheckIfProviderIdChanged(employee);

                if (doesProviderIdExist && isProviderIdChanged && doesLoginNameExist && isLoginNameChanged)
                {
                    validationString = "The ID already exists in the system. Enter a unique ID.<br/>"
                                     + "The updated Login Name already exists in the system. Enter a unique Login Name.";
                }
                else if (doesProviderIdExist && isProviderIdChanged)
                {
                    validationString = "The ID already exists in the system. Enter a unique ID.";
                }
                else if (doesLoginNameExist && isLoginNameChanged)
                {
                    validationString = "The updated Login Name already exists in the system. Enter a unique Login Name.";
                }
                else
                {
                    validationString = allowUpdate();
                    valid = validationString == string.Empty;
                    if (valid)
                    {
                        this.employeeIt2Manager.UpdateProvider(employee, officeNumber);
                        this._UpdateEmployeeOffices(providerInfo, employee, false);

                        return this.Request.CreateResponse(
                            HttpStatusCode.OK,
                            new
                            {
                                validationmessage = "Provider Updated Successfully.",
                                employeeId = employee.EmployeeId,
                                userId = employee.UserId
                            });
                    }
                }
            }
            else
            {
                validationString = allowUpdate();
                valid = validationString == string.Empty;
                if (valid)
                {
                    providerInfo.EmployeeType = EmployeeType.Provider;
                    var employee = providerInfo.ToEmployee();
                    employee.User.Name = userName;
                    this.employeeIt2Manager.UpdateProvider(employee, officeNumber);

                    return this.Request.CreateResponse(
                        HttpStatusCode.OK,
                        new
                        {
                            validationmessage = "Provider Updated Successfully.",
                            employeeId = employee.EmployeeId,
                            userId = employee.UserId
                        });
                }
            }

            Logger.Error("UpdateProviderInformation: " + validationString);
            return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { validationmessage = validationString });
        }

        /// <summary>The add provider information.</summary>
        /// <param name="officeNumber">The office Number.</param>
        /// <param name="staffInfo">The provider info.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPut]
        [Authorize(Roles = "Resource Setup")]
        public HttpResponseMessage UpdateStaffInformation(string officeNumber, [FromBody] EmployeeVm staffInfo)
        {
            var companyId = this.employeeIt2Manager.GetCompanyId(officeNumber);
            var userName = string.Concat(staffInfo.UserName, GetUserNamePlaceHolderValue(companyId));
            var doesStaffIdExist = CheckForExistingStaffId(staffInfo.EmployeeNum, companyId);
            var doesLoginNameExist = CheckForExistingLoginName(userName, companyId);
            var validationString = string.Empty;

            if (doesStaffIdExist || doesLoginNameExist)
            {
                staffInfo.ProfessionalCredential = string.Empty;
                var employee = staffInfo.ToEmployee();
                employee.User.Name = userName;

                var isLoginNameChanged = this.employeeIt2Manager.CheckIfLoginChanged(employee);
                var isStaffIdChanged = this.employeeIt2Manager.CheckIfStaffIdChanged(employee);

                if (doesStaffIdExist && isStaffIdChanged && doesLoginNameExist && isLoginNameChanged)
                {
                    validationString = "The ID already exists in the system. Enter a unique ID.<br/>"
                                     + "The updated Login Name already exists in the system. Enter a unique Login Name.";
                }
                else if (doesStaffIdExist && isStaffIdChanged)
                {
                    validationString = "The ID already exists in the system. Enter a unique ID.";
                }
                else if (doesLoginNameExist && isLoginNameChanged)
                {
                    validationString = "The updated Login Name already exists in the system. Enter a unique Login Name.";
                }
                else
                {
                    this.employeeIt2Manager.UpdateStaff(employee);
                    this._UpdateEmployeeOffices(staffInfo, employee, false);

                    return this.Request.CreateResponse(
                        HttpStatusCode.OK,
                        new
                        {
                            validationmessage = "Staff Updated Successfully.",
                            employeeId = employee.EmployeeId,
                            userId = employee.UserId
                        });
                }
            }
            else
            {
                staffInfo.ProfessionalCredential = string.Empty;
                staffInfo.EmployeeType = EmployeeType.Staff;
                var employee = staffInfo.ToEmployee();
                employee.User.Name = userName;
                this.employeeIt2Manager.UpdateStaff(employee);
                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        validationmessage = "Staff Updated Successfully.",
                        employeeId = employee.EmployeeId,
                        userId = employee.UserId
                    });
            }

            Logger.Error("UpdateStaffInformation: " + validationString);
            return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { validationmessage = validationString });
        }

        /// <summary>The get security setup view model.</summary>
        /// <param name="employeeId">The employee id.</param>
        /// <returns>The <see cref="SecuritySetupVm"/>.</returns>
        [HttpGet]
        [Authorize(Roles = "Security Setup")]
        public SecuritySetupVm GetSecuritySetupVm(int employeeId)
        {
            var result = new SecuritySetupVm();
            var employee = this.employeeIt2Manager.GetEmployeeFromIt2(employeeId);
            
            if (employee != null)
            {
                result.EmployeeId = employeeId;
                result.UserName = employee.FirstName + " " + employee.LastName;
                var isLogiEnabled = this.logiIntegrationManager.GetLogiFeatureAvailability(employee.CompanyId);
                result.IsDashboardEnabled = isLogiEnabled.IsAvailable;

                var sec = new Security();
                var roles = sec.GetUserRoles(employee.UserId, true);
                if (roles.Count > 0)
                {
                    foreach (var lookup in roles)
                    {
                        SetRole(lookup, result);
                    }
                }
            }

            return result;
        }

        /// <summary>The save security setup.</summary>
        /// <param name="vm">The VM.</param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPut]
        [Authorize(Roles = "Security Setup")]
        public HttpResponseMessage SaveSecuritySetup([FromBody] SecuritySetupVm vm)
        {
            var sec = new SecurityManager();
            var secIt2 = new SecurityIt2Manager();
            var lst = new List<int>();
            var roles = secIt2.GetSingleLocationRoleLookups().ToList();
            var employee = this.employeeIt2Manager.GetEmployeeFromIt2(vm.EmployeeId);

            if (vm.AccountingReports)
            {
                lst.Add(GetKey("Accounting", roles));
            }

            if (vm.MarketingReports)
            {
                lst.Add(GetKey("Marketing And Recalls", roles));
            }

            if (vm.SalesReports)
            {
                lst.Add(GetKey("Sales Reports", roles));
            }

            if (vm.ChangeOrderDates)
            {
                lst.Add(GetKey("Change Order Dates", roles));
            }

            if (vm.PatientOrderRemakes)
            {
                lst.Add(GetKey("Patient Order Remakes", roles));
            }
            
            if (vm.PatientMerge)
            {
                lst.Add(GetKey("Combine Patient Profiles", roles));
            }

            if (vm.ScheduleExceptions)
            {
                lst.Add(GetKey("Schedule Exceptions", roles));
            }

            if (vm.ApplyDiscounts)
            {
                lst.Add(GetKey("Apply Discounts", roles));
            }

            if (vm.Collections)
            {
                lst.Add(GetKey("Collections", roles));
            }

            if (vm.ManualInsuranceCalculations)
            {
                lst.Add(GetKey("Manual Insurance Calculations", roles));
            }

            if (vm.OverrideDefaultDeposit)
            {
                lst.Add(GetKey("Override Default Deposit", roles));
            }

            if (vm.PatientReturns)
            {
                lst.Add(GetKey("Patient Returns", roles));
            }

            if (vm.PriceAdjustments)
            {
                lst.Add(GetKey("Price Adjustments", roles));
            }

            if (vm.ProcessRefunds)
            {
                lst.Add(GetKey("Refund", roles));
            }

            if (vm.DailyClosing)
            {
                lst.Add(GetKey("Daily Closing", roles));
            }

            if (vm.FinancialSetup)
            {
                lst.Add(GetKey("Financial Setup", roles));
            }

            if (vm.InsuranceAdmin)
            {
                lst.Add(GetKey("Insurance Admin", roles));
            }

            if (vm.PracticeSetup)
            {
                lst.Add(GetKey("Company Setup", roles));
            }

            if (vm.RecallSetup)
            {
                lst.Add(GetKey("Recall Setup", roles));
            }

            if (vm.ResourceSetup)
            {
                lst.Add(GetKey("Resource Setup", roles));
            }

            if (vm.BatchAdjustments)
            {
                lst.Add(GetKey("Billing Management", roles));
            }

            if (vm.BillingReports)
            {
                lst.Add(GetKey("Billing", roles));
            }

            if (vm.ProcessClaims)
            {
                lst.Add(GetKey("Billing Claims", roles));
            }

            if (vm.ProcessPayments)
            {
                lst.Add(GetKey("Billing Payments", roles));
            }

            if (vm.EmergencyAccess)
            {
                lst.Add(GetKey("Emergency Access", roles));
            }

            if (vm.ResetResourcePassword)
            {
                lst.Add(GetKey("Reset Resource Password", roles));
            }

            if (vm.SecuritySetup)
            {
                lst.Add(GetKey("Security Setup", roles));
            }

            if (vm.InventoryAdjustments)
            {
                lst.Add(GetKey("Inventory Adjustment", roles));
            }

            if (vm.ChangePaymentTypes)
            {
                lst.Add(GetKey("Change Payment Types", roles));
            }

            if (vm.BillingDashboard)
            {
                lst.Add(GetKey("Billing Dashboard", roles));
            }

            if (vm.InventoryDashboard)
            {
                lst.Add(GetKey("Inventory Dashboard", roles));
            }

            if (vm.MedicalDashboard)
            {
                lst.Add(GetKey("Medical Dashboard", roles));
            }

            if (vm.OperationsDashboard)
            {
                lst.Add(GetKey("Operations Dashboard", roles));
            }

            if (vm.SalesDashboard)
            {
                lst.Add(GetKey("Sales Dashboard", roles));
            }

            sec.SaveUserRoles(employee.UserId, lst);

            return Request.CreateResponse(HttpStatusCode.OK, "Security Settings saved.");
        }

        /// <summary>The check for existing StaffID.</summary>
        /// <param name="staffId">The StaffID.</param>
        /// <param name="companyId">The company Id.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool CheckForExistingStaffId(string staffId, string companyId)
        {
            var em = new EmployeeIt2Manager();
            return em.DoesEmployeeNumExists(staffId, companyId);
        }

        /// <summary>The check for existing UserID.</summary>
        /// <param name="loginName">The UserID.</param>
        /// <param name="companyId">The company Id.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool CheckForExistingLoginName(string loginName, string companyId)
        {
            var em = new EmployeeIt2Manager();
            return em.DoesLoginNameExists(loginName);
        }

        /// <summary>The check for existing ProviderID.</summary>
        /// <param name="providerId">The UserID.</param>
        /// <param name="companyId">The company Id.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool CheckForExistingProviderId(string providerId, string companyId)
        {
            var em = new EmployeeIt2Manager();
            return em.DoesEmployeeNumExists(providerId, companyId);
        }

        /// <summary>The set role.</summary>
        /// <param name="lookup">The lookup.</param>
        /// <param name="result">The result.</param>
        private static void SetRole(Lookup lookup, SecuritySetupVm result)
        {
            switch (lookup.Description)
            {
                case "Accounting":
                    result.AccountingReports = true;
                    break;
                case "Marketing And Recalls":
                    result.MarketingReports = true;
                    break;
                case "Sales Reports":
                    result.SalesReports = true;
                    break;
                case "Change Order Dates":
                    result.ChangeOrderDates = true;
                    break;
                case "Patient Order Remakes":
                    result.PatientOrderRemakes = true;
                    break;
                case "Combine Patient Profiles":
                    result.PatientMerge = true;
                    break;
                case "Schedule Exceptions":
                    result.ScheduleExceptions = true;
                    break;

                case "Apply Discounts":
                    result.ApplyDiscounts = true;
                    break;
                case "Collections":
                    result.Collections = true;
                    break;
                case "Manual Insurance Calculations":
                    result.ManualInsuranceCalculations = true;
                    break;
                case "Override Default Deposit":
                    result.OverrideDefaultDeposit = true;
                    break;
                case "Patient Returns":
                    result.PatientReturns = true;
                    break;
                case "Price Adjustments":
                    result.PriceAdjustments = true;
                    break;
                case "Refund":
                    result.ProcessRefunds = true;
                    break;
                case "Daily Closing":
                    result.DailyClosing = true;
                    break;

                case "Financial Setup":
                    result.FinancialSetup = true;
                    break;
                case "Insurance Admin":
                    result.InsuranceAdmin = true;
                    break;
                case "Company Setup":
                    result.PracticeSetup = true;
                    break;
                case "Recall Setup":
                    result.RecallSetup = true;
                    break;
                case "Resource Setup":
                    result.ResourceSetup = true;
                    break;
                case "Billing Management":
                    result.BatchAdjustments = true;
                    break;
                case "Billing":
                    result.BillingReports = true;
                    break;
                case "Billing Claims":
                    result.ProcessClaims = true;
                    break;
                case "Billing Payments":
                    result.ProcessPayments = true;
                    break;
                case "Emergency Access":
                    result.EmergencyAccess = true;
                    break;
                case "Reset Resource Password":
                    result.ResetResourcePassword = true;
                    break;
                case "Security Setup":
                    result.SecuritySetup = true;
                    break;
                case "Inventory Adjustment":
                    result.InventoryAdjustments = true;
                    break;
                case "Change Payment Types":
                    result.ChangePaymentTypes = true;
                    break;
                case "Billing Dashboard":
                    result.BillingDashboard = true;
                    break;
                case "Inventory Dashboard":
                    result.InventoryDashboard = true;
                    break;
                case "Medical Dashboard":
                    result.MedicalDashboard = true;
                    break;
                case "Operations Dashboard":
                    result.OperationsDashboard = true;
                    break;
                case "Sales Dashboard":
                    result.SalesDashboard = true;
                    break;
                default:
                    Logger.Error("SetRole: Unknown Role Lookup Description: " + lookup.Description);
                    break;
            }
        }

        /// <summary>The get key.</summary>
        /// <param name="name">The name.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="int"/>.</returns>
        private static int GetKey(string name, List<Lookup> list)
        {
            var result = 0;
            var item = list.Find(x => x.Description == name);
            if (item != null)
            {
                result = item.Key;
            }

            return result;
        }

        /// <summary>
        /// The get user name place holder value.
        /// </summary>
        /// <param name="companyId">
        /// The office number.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetUserNamePlaceHolderValue(string companyId)
        {
            var placeholdervalue = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["UserNamePlaceHolderValue"]) ? "PM" : System.Configuration.ConfigurationManager.AppSettings["UserNamePlaceHolderValue"];
            return string.Format("_{0}_{1}_{0}", placeholdervalue, companyId).ToLower();
        }

        /// <summary>The format date.</summary>
        /// <param name="date">The date.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string FormatDate(string date)
        {
            var result = date;
            if (!string.IsNullOrEmpty(date))
            {
                result = DateTime.Parse(date).ToString("MM/dd/yyyy");
            }

            return result;
        }

        private IList<string> _ExtractAuthenticatedOffices(List<EmployeeOfficeVm> offices)
        {
            return offices.Where(s => s.IsAuthorizedForAccess).Select(o => o.OfficeNum).ToList();
        }

        private IList<string> _ExtractUnauthenticatedOffices(List<EmployeeOfficeVm> offices)
        {
            return offices.Where(s => !s.IsAuthorizedForAccess).Select(o => o.OfficeNum).ToList();
        }

        private void _UpdateEmployeeOffices(EmployeeVm providerInfo, Employee employee, bool newEmployee)
        {
            var authenticatedOffices = this._ExtractAuthenticatedOffices(providerInfo.Offices);
            var removeOffices = this._ExtractUnauthenticatedOffices(providerInfo.Offices);

            this.employeeIt2Manager.SaveOrUpdateEmployeeOffices(
                employee.EmployeeId,
                authenticatedOffices,
                authenticatedOffices.Select(o => providerInfo.EyefinityEhr ? 1 : 0).ToList(),
                newEmployee);

            this.employeeIt2Manager.RemoveEmployeeOffices(
                employee.EmployeeId,
                removeOffices);
        }
    }
}