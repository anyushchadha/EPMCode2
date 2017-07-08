// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdminHolidayController.cs" company="Eyefinity, Inc.">
//    Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//  The holiday controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Common.Api;
    using Enterprise.Business.Home;
    using IT2.Core;
    using Model.Admin.ViewModel;

    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientLocationsController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The holidays manager.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private readonly QuickListManager patientLocationManager = new QuickListManager();

        /// <summary>
        /// The get holidays.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// Holidays View model
        /// </returns>
        public List<PatientLocations> GetLocations(string officeNumber)
        {
            return this.patientLocationManager.GetPatientLocations(officeNumber);
        }

        /// <summary>
        /// The save holidays.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="patientLocations">
        /// The holidays.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage SavePatientLocations(string officeNumber, IEnumerable<PatientLocationsVm> patientLocations)
        {
            var patientLocation = (from locations in patientLocations
                select new Model.Admin.PatientLocations()
                    {
                        LocationId = locations.LocationId,
                        Description = locations.Description,
                        OfficeNum = locations.OfficeNum,
                        IsDeleted = locations.IsDeleted
                    }).ToList();

            var location = this.GetLocations(officeNumber);
            var matches = location.Where(x => x.Description == patientLocation[0].Description);

            if (patientLocation.Count > 0  && (!patientLocation[0].IsDeleted) && matches.Any())
            {
                this.GetLocations(officeNumber);
                return this.Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        "The location already exists.It cannot be inserted / updated");
             }
            
            if (patientLocation.Count > 0)
            {
                this.patientLocationManager.SavePatientLocations(officeNumber, patientLocation);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, this.GetLocations(officeNumber));
        }
    }
}