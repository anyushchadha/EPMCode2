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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;

    /// <summary>
    /// The holiday controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class AdminHolidayController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The holidays manager.
        /// </summary>
        private readonly HolidaysIt2Manager holidaysManager = new HolidaysIt2Manager();

        /// <summary>
        /// The get holidays.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// Holidays View model
        /// </returns>
        public IEnumerable<HolidayVm> GetHolidays(string officeNumber)
        {
            return this.holidaysManager.GetHolidays(officeNumber).Select(HolidayVm.FromHoliday);
        }

        /// <summary>
        /// The save holidays.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="holidays">
        /// The holidays.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage SaveHolidays(string officeNumber, IEnumerable<HolidayVm> holidays)
        {
            var holidayVms = holidays as HolidayVm[] ?? holidays.ToArray();
            var errors = this.holidaysManager.SaveHolidays(officeNumber, holidayVms.Where(h => !h.IsDeleted).Select(hvm => hvm.ToHoliday()));
            var enumerable = errors as string[] ?? errors.ToArray();
            if (enumerable.Any())
            {
                Logger.Error("SaveHolidays: " + string.Join(Environment.NewLine, enumerable));
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Join(Environment.NewLine, enumerable));
            }

            this.holidaysManager.Delete(holidayVms.Where(h => h.IsDeleted).Select(hvm => hvm.ToHoliday()));

            return this.Request.CreateResponse(HttpStatusCode.OK, this.GetHolidays(officeNumber));
        }
    }
}