// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HolidayController.cs" company="Eyefinity, Inc.">
//  2013 Eyefinity, Inc.  
// </copyright>
// <summary>
//   Defines the HolidayController type.
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
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;
    using IT2.Core.Location;

    /// <summary>
    /// The holiday controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class HolidayController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly HolidayIt2Manager it2Business;

        /// <summary>
        /// Initializes a new instance of the <see cref="HolidayController"/> class.
        /// </summary>
        public HolidayController()
        {
            this.it2Business = new HolidayIt2Manager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HolidayController"/> class.
        /// </summary>
        /// <param name="manager">
        /// The manager.
        /// </param>
        public HolidayController(HolidayIt2Manager manager)
        {
            this.it2Business = manager;
        }

        // GET api/holiday

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <returns>
        /// The list of holidays /&gt;.
        /// </returns>
        public HttpResponseMessage Get(string officeNumber)
        {
            try
            {
                AccessControl.VerifyUserAccessToOffice(officeNumber);
                var officeHolidays = this.it2Business.GetAllHolidaysForOffice(officeNumber);
                if (officeHolidays != null)
                {
                    var holidays = (from item in officeHolidays select new Holiday(item.ID, item.Description, item.HolidayDate)).ToList();
                    return this.Request.CreateResponse(HttpStatusCode.OK, holidays);
                }

                return Request.CreateResponse(HttpStatusCode.NotFound, "Holiday Not Found.");
            }
            catch (Exception ex)
            {
                var msg = string.Format("Get(officeNumber = {0} {1} {2}", officeNumber, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        // GET api/holiday/5

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// Holiday /&gt;.
        /// </returns>
        public HttpResponseMessage Get(int id)
        {
            try
            {
                var officeHoliday = this.it2Business.GetHolidayById(id);
                if (officeHoliday == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Holiday not found.");
                }

                AccessControl.VerifyUserAccessToOffice(officeHoliday.OfficeNum);
                var holiday = new Holiday(officeHoliday.ID, officeHoliday.Description, officeHoliday.HolidayDate);
                return Request.CreateResponse(HttpStatusCode.OK, holiday);
            }
            catch (NHibernate.ObjectNotFoundException ex)
            {
                Logger.Error(string.Format("Get(id = {0} {1} {2}", id, "\n", ex));
                return Request.CreateResponse(HttpStatusCode.NotFound);        
            }
            catch (Exception ex)
            {
                var msg = string.Format("Get(id = {0} {1} {2}", id, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>
        /// The post.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="holiday">
        /// The holiday.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage Post(string officeNumber, Holiday holiday)
        {
            AccessControl.VerifyUserAccessToOffice(officeNumber);
            if (string.IsNullOrEmpty(officeNumber) || string.IsNullOrEmpty(holiday.Description))
            {
                return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "Invalid Holiday..!!!");
            }

            try 
            { 
                var officeHoliday = new OfficeHoliday { HolidayDate = DateTime.Parse(holiday.Date), OfficeNum = officeNumber, Description = holiday.Description };

                // Check for duplicate entries
                IList<OfficeHoliday> existingHolidays = this.it2Business.GetAllHolidaysForOffice(officeNumber);
                IList<OfficeHoliday> results = existingHolidays.Where(d => d.HolidayDate == officeHoliday.HolidayDate).ToList();
                if (results.Count == 0)
                {
                    officeHoliday.OfficeNum = officeNumber;
                    this.it2Business.SaveHoliday(officeHoliday);
                    var response = Request.CreateResponse(HttpStatusCode.OK, "Holiday Created");
                    return response;
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest, "Holiday Already Exists");
            }
            catch (Exception ex)
            {
                var msg = string.Format("Post(officeNumber = {0} {1} {2}", officeNumber, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        // DELETE api/holiday/5

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                var holiday = this.it2Business.GetHolidayById(id);
                AccessControl.VerifyUserAccessToOffice(holiday.OfficeNum);
                this.it2Business.DeleteHoliday(holiday);
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Delete(id = {0} {1} {2}", id, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }
    }
}
