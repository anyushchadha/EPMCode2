// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OfficeHoursController.cs" company="Eyefinity, Inc.">
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

    using Eyefinity.Enterprise.Business.Appointment;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;

    using Helper = Eyefinity.PracticeManagement.Business.Common.Helper;

    /// <summary>The office hours controller.</summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class OfficeHoursController : ApiController
    {
        #region Public Methods and Operators

        /// <summary>The get office hours.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <returns>The result /&gt;.</returns>
        [HttpGet]
        public IEnumerable<OfficeHoursViewModel> GetOfficeHours(string officeNumber)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(officeNumber);
            var result = CreateEmptyViewModel();
            var manager = new AppointmentOfficeHoursManager();
            var list = manager.GetOfficeHours(officeNumber);
            var helper = new Helper();
            foreach (var item in result)
            {
                item.OfficeNumber = officeNumber;
                var a = list.Find(x => x.OperatingDay.ToString() == item.OperatingDay);
                if (a == null)
                {
                    continue;
                }

                item.Id = a.Id;
                item.IsOperatingDay = true;
                if (a.OpenFrom == null)
                {
                    continue;
                }

                var d = helper.IntegerToTimeDisplay((int)a.OpenFrom);
                item.OpenFrom = d[0];
                item.OpenFromAmPm = d[1];

                if (a.OpenTo == null)
                {
                    continue;
                }

                var dd = helper.IntegerToTimeDisplay((int)a.OpenTo);
                item.OpenTo = dd[0];
                item.OpenToAmPm = dd[1];
            }

            return result;
        }

        /// <summary>The save office hours.</summary>
        /// <param name="officehoursList">The office hours list.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPut]
        public HttpResponseMessage SaveOfficeHours([FromBody] OfficeHoursViewModel[] officehoursList)
        {
            var officeNumber = officehoursList[0].OfficeNumber;
            AccessControl.VerifyUserAccessToMultiLocationOffice(officeNumber);
            var manager = new AppointmentOfficeHoursManager();
            var helper = new Helper();             
            var hours = (from item in officehoursList
                         where item.IsOperatingDay
                         select new OfficeHours
                         {
                             Id = item.Id,
                             OpenFrom = helper.StringToIntegerTimeConversion(item.OpenFrom, item.OpenFromAmPm),
                             OpenTo = helper.StringToIntegerTimeConversion(item.OpenTo, item.OpenToAmPm),
                             OperatingDay = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), item.OperatingDay)
                         }).ToList();

            manager.SaveOfficeHours(officeNumber, hours);

            return Request.CreateResponse(HttpStatusCode.OK, "Office Hours updated.");
        }

        #endregion

        #region Methods

        /// <summary>The create empty view model.</summary>
        /// <returns>The result.</returns>
        private static List<OfficeHoursViewModel> CreateEmptyViewModel()
        {
            var result = new List<OfficeHoursViewModel>();
            for (var i = 1; i < 8; i++)
            {
                var indx = i == 7 ? 0 : i;
                var item = new OfficeHoursViewModel();
                var d = (DayOfWeek)indx;

                item.Id = indx;
                item.IsOperatingDay = false;
                item.OperatingDay = d.ToString();
                item.OpenFrom = "08:00";
                item.OpenFromAmPm = "a.m.";
                item.OpenTo = "05:00";
                item.OpenToAmPm = "p.m.";
                result.Add(item);
            }

            return result;
        }

        #endregion
    }
}