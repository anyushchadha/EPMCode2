// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PreferencesApiController.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;

    using RecallType = Eyefinity.PracticeManagement.Model.Admin.RecallType;

    /// <summary>The preferences API controller.</summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PreferencesApiController : ApiController
    {
        private readonly string companyId;

        #region Public Methods and Operators
        public PreferencesApiController()
        {
            var user = new AuthorizationTicketHelper().GetUserInfo();
            this.companyId = user.CompanyId;
        }

        /// <summary>
        /// The get required patient profile fields ViewModel.
        /// </summary>
        /// <param name="companyId">
        /// The company id.
        /// </param>
        /// <returns>
        /// The <see cref="RequiredPatientProfileFieldsVm"/>.
        /// </returns>
        [HttpGet]
        public RequiredPatientProfileFieldsVm GetRequiredPatientProfileFieldsVm(string companyId)
        {
            var result = new RequiredPatientProfileFieldsVm();
            var manager = new PreferencesManager();
            var dict = manager.GetPreferencesByCategory(companyId, PreferenceCategory.RequiredPatientProfileFields);
            result.CompanyId = companyId;
            result.IsAddressRequired = dict["IsAddressRequired"] == "true";
            result.IsBirthDateRequired = dict["IsBirthDateRequired"] == "true";
            result.IsEmailRequired = dict["IsEmailRequired"] == "true";
            result.IsMeaningfulUseMandatory = dict["IsMeaningfulUseMandatory"] == "true";
            result.IsTitleRequired = dict["IsTitleRequired"] == "true";
            result.IsPrimaryPhoneRequired = dict["IsPrimaryPhoneRequired"] == "true";
            result.IsDefaultFamilyCheck = manager.GetMaterialOrderPreferences(companyId);
            result.IsPatientCenterEnabled  = manager.GetPatientCenterEnabled(companyId);
            var patientStatementPreferences = manager.GetPatientStatementPreferences(companyId);
            result.IsDetailedPatientStatements = patientStatementPreferences["IsDetailedPatientStatements"] == "true";
            result.AutoConfirmDays = this.GetSchedulerAutoConfirmDays(companyId);
            result.IsInsuranceAdjustmentTypes = patientStatementPreferences["IsInsuranceAdjustmentTypes"] == "true";
            result.IsCarrierPayments = patientStatementPreferences["IsCarrierPayments"] == "true";
            result.IsProviderNameAndNpi = patientStatementPreferences["IsProviderNameAndNpi"] == "true";
            result.IsReasonForTransferToPatient = patientStatementPreferences["IsReasonForTransferToPatient"] == "true";
            return result;
        }

        /// <summary>
        /// The save required patient profile fields.
        /// </summary>
        /// <param name="vm">
        /// The VM.
        /// </param>
        [HttpPut]
        public void SaveRequiredPatientProfileFields([FromBody] RequiredPatientProfileFieldsVm vm)
        {
            var manager = new PreferencesManager();
            var dict = new Dictionary<string, string>
                           {
                               { "IsAddressRequired", vm.IsAddressRequired ? "true" : "false" }, 
                               {
                                   "IsBirthDateRequired", 
                                   vm.IsBirthDateRequired ? "true" : "false"
                               }, 
                               { "IsEmailRequired", vm.IsEmailRequired ? "true" : "false" }, 
                               {
                                   "IsMeaningfulUseMandatory", 
                                   vm.IsMeaningfulUseMandatory ? "true" : "false"
                               }, 
                               { "IsTitleRequired", vm.IsTitleRequired ? "true" : "false" }, 
                               {
                                   "IsPrimaryPhoneRequired", 
                                   vm.IsPrimaryPhoneRequired ? "true" : "false"
                               }
                           };

            manager.SavePreferencesByCategory(vm.CompanyId, PreferenceCategory.RequiredPatientProfileFields, dict);

            bool defaultFamilyCheck = vm.IsDefaultFamilyCheck ?? false;
            manager.SaveMaterialOrderPreferences(vm.CompanyId, defaultFamilyCheck);

            bool patientCenterEnabled = vm.IsPatientCenterEnabled;
            manager.SavePatientCenterEnabled(vm.CompanyId, patientCenterEnabled);

            var patientStatementPreferences = new Dictionary<string, string>
                {
                    { "IsDetailedPatientStatements",  vm.IsDetailedPatientStatements ? "true" : "false" },
                    { "IsInsuranceAdjustmentTypes", vm.IsInsuranceAdjustmentTypes ? "true" : "false" },
                    { "IsCarrierPayments", vm.IsCarrierPayments ? "true" : "false" },
                    { "IsProviderNameAndNpi", vm.IsProviderNameAndNpi ? "true" : "false" },
                    { "IsReasonForTransferToPatient", vm.IsReasonForTransferToPatient ? "true" : "false" },
                };

            bool isDetailedPatientReport = vm.IsDetailedPatientStatements;
            manager.SavePatientStatementPreferences(vm.CompanyId, isDetailedPatientReport, patientStatementPreferences);

            this.SaveSchedulerAutoConfirmDays(vm.CompanyId, vm.AutoConfirmDays);
        }

        /// <summary>
        /// The get recall types.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// The List of Recall Types
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "Recall Setup")]
        public IEnumerable<RecallType> GetRecallTypes(string officeNumber)
        {
            return new RecallTypeManager().GetRecallTypes(officeNumber);
        }

        /// <summary>
        /// The save recall types.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="recallTypes">
        /// The recall types.
        /// </param>
        /// <returns>
        /// The List of Recall Types
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Recall Setup")]
        public IEnumerable<RecallType> SaveRecallTypes(string officeNumber, IEnumerable<RecallType> recallTypes)
        {
            new RecallTypeManager().SaveRecallTypes(officeNumber, recallTypes);
            return this.GetRecallTypes(officeNumber);
        }

        [HttpGet]
        public InventoryConfigurationVm GetInventoryConfigurations(string officeNumber)
        {
            return new InventoryConfigurationIt2Manager().GetInventoryConfiguration(officeNumber);
        }

        [HttpPost]
        public void SaveInventoryConfiguration(InventoryConfigurationVm vm)
        {
            var manager = new InventoryConfigurationIt2Manager();
            manager.SaveInventoryConfiguration(vm);
        }

        [HttpGet]
        [Authorize(Roles = "Recall Setup")]
        public HttpResponseMessage GetAllRecallSchedules(string recallTypeId)
        {
            var recallManager = new RecallIt2Manager();
            var recallSchedules = recallManager.GetAllRecallSchedules(recallTypeId);
            var periodTypes = recallManager.GetPeriodLookupTypes();
            var whenTypes = recallManager.GetWhenLookupTypes();
            var recallDescription = recallManager.GetRecallTypeDescription(recallTypeId);
            var printTemplates = recallManager.GetPrintTemplateLookupTypes(this.companyId);
            printTemplates = printTemplates.OrderBy(x => x.Text).ToList();

            var removeString = "|" + this.companyId + "|";
            foreach (var item in printTemplates)
            {
                item.Text = item.Text.Replace(removeString, string.Empty);
            }

            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    RecallSchedules = recallSchedules,
                    PeriodTypes = periodTypes.ToKeyValuePairs(),
                    WhenTypes = whenTypes.ToKeyValuePairs(),
                    PrintTemplates = printTemplates.ToKeyValuePairs(),
                    RecallTypeDescription = recallDescription
                });
        }

        [HttpPost]
        [Authorize(Roles = "Recall Setup")]
        public void SaveRecallSchedules(List<RecallSchedule> recallSchedules)
        {
            new RecallIt2Manager().SaveRecallSchedules(recallSchedules);
        }

        [HttpDelete]
        [Authorize(Roles = "Recall Setup")]
        public void DeleteRecallSchedule(int id)
        {
            new RecallIt2Manager().DeleteRecallSchedule(id);
        }

        #endregion //// Public Methods and Operators

        #region private methods

        private int GetSchedulerAutoConfirmDays(string companyId)
        {
            var schedulerPreferencesIt2Manager = new SchedulerPreferencesIt2Manager();
            return schedulerPreferencesIt2Manager.GetAutoConfirmDaysPreference(companyId);
        }

        private void SaveSchedulerAutoConfirmDays(string companyId, int autoConfirmDays)
        {
            var schedulerPreferencesIt2Manager = new SchedulerPreferencesIt2Manager();
            schedulerPreferencesIt2Manager.SaveAutoConfirmDaysPreference(companyId, autoConfirmDays);
        }
        #endregion //// private methods
    }
}