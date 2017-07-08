// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppointmentServicesController.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//   The appointment services controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Collections.Generic;
    using System.Web.Http;

    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;

    /// <summary>
    ///     The appointment services controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class AppointmentServicesController : ApiController
    {
        /// <summary>
        /// The item type data manager.
        /// </summary>
        private readonly ItemTypeManager itemTypeDataManager = new ItemTypeManager();

        /// <summary>
        /// The get appointment services.
        /// </summary>
        /// <param name="serviceCptCode">
        /// The service CPT Code.
        /// </param>
        /// <param name="serviceExamName">
        /// The service Exam Name.
        /// </param>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// Returns Appointment Services Setup
        /// </returns>
        [HttpGet]
        public List<AppointmentServicesSetup> GetAppointmentServices(string serviceCptCode, string serviceExamName, string officeNumber)
        {
            return this.itemTypeDataManager.GetAppointmentItemTypes(officeNumber, serviceCptCode, serviceExamName);
        }

        /// <summary>
        /// The save appointment services.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        [HttpPut]
        public void SaveAppointmentServices(List<AppointmentServicesSetup> model, string officeNumber)
        {
            this.itemTypeDataManager.SaveAppointmentItemType(model, officeNumber);
        }
    }
}