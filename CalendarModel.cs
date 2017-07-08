// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalendarModel.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//   Defines the CalendarModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Model
{
    using System.Collections.Generic;
    using DHTMLX.Scheduler;

    using Eyefinity.PracticeManagement.Model.Appointment;
    using Eyefinity.WebScheduler.DataContracts;

    /// <summary>
    /// The calendar model.
    /// </summary>
    public class CalendarModel
    {
        /// <summary>
        /// Gets or sets the preferences.
        /// </summary>
        public SchedulerPreferences Preferences { get; set; }

        /// <summary>
        /// Gets or sets the colors.
        /// </summary>
        public Dictionary<string, string> Colors { get; set; }

        /// <summary>
        /// Gets or sets the office hours.
        /// </summary>
        public IEnumerable<ScheduleOfficeHours> OfficeHours { get; set; }

        /// <summary>
        /// Gets or set the offices in a multi-location company.
        /// </summary>
        public IEnumerable<Lookup> Offices { get; set; }

        /// <summary>
        /// Gets or sets the minimum duration for the resources in minutes.
        /// </summary>
        public int TimeLineMinutes { get; set; }

        /// <summary>
        /// Gets or sets the resources.
        /// </summary>
        public IEnumerable<Resource> Resources { get; set; }

        /// <summary>
        /// Gets or sets the sorted resources.
        /// </summary>
        public IEnumerable<Resource> SortedResources { get; set; }

        /// <summary>
        /// Gets or sets the resource details.
        /// </summary>
        public IEnumerable<ScheduleResourceDetails> ResourceDetails { get; set; }

        /// <summary>
        /// Gets or sets the holidays.
        /// </summary>
        public IEnumerable<Holiday> Holidays { get; set; }

        /// <summary>
        /// Gets or sets the scheduler.
        /// </summary>
        public DHXScheduler Scheduler { get; set; }
    }
}
