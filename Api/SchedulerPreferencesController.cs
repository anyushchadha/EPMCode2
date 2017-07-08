// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SchedulerPreferencesController.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Globalization;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Interfaces;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;

    /// <summary>
    /// The scheduler preferences controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class SchedulerPreferencesController : ApiController
    {
        /// <summary>
        /// The scheduler preferences manager.
        /// </summary>
        private readonly ISchedulerPreferencesManager schedulerPreferencesManager;

        /// <summary>
        /// The scheduler preferences it 2 manager.
        /// </summary>
        private readonly SchedulerPreferencesIt2Manager schedulerPreferencesIt2Manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerPreferencesController"/> class.
        /// </summary>
        public SchedulerPreferencesController()
        {
            this.schedulerPreferencesManager = new SchedulerPreferencesManager();
            this.schedulerPreferencesIt2Manager = new SchedulerPreferencesIt2Manager();
        }

        #region Public Methods and Operators

        /// <summary>
        /// The get required patient profile fields ViewModel.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// The <see cref="RequiredPatientProfileFieldsVm"/>.
        /// </returns>
        [HttpGet]
        public SchedulerPreferencesVm GetSchedulerPreferencesVm(string officeNumber)
        {
            var preferences =
                SchedulerPreferencesVm.FromDictionary(
                    this.schedulerPreferencesManager.GetSchedulerPreferences(officeNumber));
            var additionalPreferences =
                this.schedulerPreferencesIt2Manager.GetAdditionalSchedulerPreferences(officeNumber);

            preferences.AutoConfirmAppointmentDays = additionalPreferences.AppointmentAutoConfirmDays;
            preferences.SupportsPreAppointments = additionalPreferences.SupportsPreAppointment.GetValueOrDefault();
            preferences.PracticeLocationId = officeNumber;
            return preferences;
        }

        /// <summary>
        /// The save required patient profile fields.
        /// </summary>
        /// <param name="vm">
        /// The VM.
        /// </param>
        [HttpPut]
        public void SaveSchedulerPreferences([FromBody] SchedulerPreferencesVm vm)
        {
            this.schedulerPreferencesManager.SaveSchedulerPreferences(vm.PracticeLocationId, vm.ToDictionary());
            //// Insert the Default View preference
            this.schedulerPreferencesManager.SaveSchedulerDefaultViewPreference(vm.PracticeLocationId, vm.DefaultView.ToString(CultureInfo.InvariantCulture));
            this.schedulerPreferencesIt2Manager.SaveAdditionalSchedulerPreferences(
                new Office
                    {
                        OfficeNumber = vm.PracticeLocationId, 
                        AppointmentAutoConfirmDays = vm.AutoConfirmAppointmentDays, 
                        SupportsPreAppointment = vm.SupportsPreAppointments
                    });
        }

        #endregion
    }
}