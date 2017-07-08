// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdminOfficeHoursController.cs" company="Eyefinity, Inc.">
//    Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//  The office hours controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;

    /// <summary>
    ///     The office hours controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class AdminOfficeHoursController : ApiController
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get office hours.
        /// </summary>
        /// <param name="practiceLocationId">
        /// The practice Id.
        /// </param>
        /// <returns>
        /// The result /&gt;.
        /// </returns>
        [HttpGet]
        public List<OfficeHoursVm> GetOfficeHours(string practiceLocationId)
        {
            List<OfficeHoursVm> result = this.CreateEmptyViewModel(practiceLocationId);
            var manager = new OfficeHoursIt2Manager();
            var helper = new Helper();
            List<OfficeHours> tmp = manager.GetOfficeHours(practiceLocationId);
            foreach (OfficeHoursVm item in result)
            {
                OfficeHours a = tmp.Find(x => x.OperatingDay.ToString() == item.OperatingDay);
                if (a == null)
                {
                    continue;
                }

                item.Id = a.Id;
                item.PracticeLocationId = a.PracticeLocationId;
                item.IsOperatingDay = true;
                if (a.OpenFrom == null)
                {
                    continue;
                }

                string[] d = helper.IntegerToTimeDisplay((int)a.OpenFrom);
                item.OpenFrom = d[0];
                item.OpenFromAmPm = d[1];

                if (a.OpenTo == null)
                {
                    continue;
                }

                string[] dd = helper.IntegerToTimeDisplay((int)a.OpenTo);
                item.OpenTo = dd[0];
                item.OpenToAmPm = dd[1];
            }

            return result;
        }

        /// <summary>
        /// The save office hours.
        /// </summary>
        /// <param name="officehoursList">
        /// The office hours list.
        /// </param>
        [HttpPut]
        public void SaveOfficeHours([FromBody] OfficeHoursVm[] officehoursList)
        {
            var manager = new OfficeHoursIt2Manager();
            var helper = new Helper();
            string pid = officehoursList[0].PracticeLocationId;
            var hours = (from item in officehoursList where item.IsOperatingDay select new OfficeHours { Id = item.Id, PracticeLocationId = item.PracticeLocationId, OpenFrom = helper.StringToIntegerTimeConversion(item.OpenFrom, item.OpenFromAmPm), OpenTo = helper.StringToIntegerTimeConversion(item.OpenTo, item.OpenToAmPm), OperatingDay = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), item.OperatingDay) }).ToList();

            manager.SaveOfficeHours(pid, hours);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The create empty view model.
        /// </summary>
        /// <param name="practiceLocationId">
        /// The practice id.
        /// </param>
        /// <returns>
        /// The result"/&gt;.
        /// </returns>
        private List<OfficeHoursVm> CreateEmptyViewModel(string practiceLocationId)
        {
            var result = new List<OfficeHoursVm>();
            for (int i = 0; i < 7; i++)
            {
                var item = new OfficeHoursVm();
                var d = (DayOfWeek)i;
                string s = d.ToString();
                item.OperatingDay = s;
                item.Id = i;
                item.PracticeLocationId = practiceLocationId;
                item.OpenFrom = "08:00";
                item.OpenFromAmPm = "a.m.";
                item.OpenTo = "05:00";
                item.OpenToAmPm = "p.m.";
                item.IsOperatingDay = false;
                result.Add(item);
            }

            return result;
        }

        #endregion
    }
}