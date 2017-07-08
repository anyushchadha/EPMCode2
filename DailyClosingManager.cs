// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DailyClosingManager.cs" company="Eyefinity, Inc.">
// Copyright © 2013 Eyefinity, Inc.  All rights reserved.  
// </copyright>
// <summary>
//   Defines the DailyClosingManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Business.Payment
{
    using System;
    using System.Collections.Generic;

    using Eyefinity.PracticeManagement.Data.Infrastructure;
    using Eyefinity.PracticeManagement.Model;

    /// <summary>
    /// The daily closing manager.
    /// </summary>
    public class DailyClosingManager
    {
        /// <summary>
        /// The calculate POS transaction summary by day close date new.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="dayCloseDate">
        /// The day close date.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool CalculatePosTransactionSummaryByDayCloseDate(string officeNumber, DateTime dayCloseDate)
        {
            using (var uow = new UnitOfWork(DatabaseHelper.GetSession()))
            {
                var arguments = new
                {
                    officenum = officeNumber,
                    TransDate = dayCloseDate.ToShortDateString()
                };
                return DatabaseHelper.ExecuteStoredProcedure(uow.Session, "CalcPOSTransactionSummaryByDayCloseDate", ":officenum, :TransDate", arguments);
            }
        }

        /// <summary>
        /// The undo daily closing.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="dayCloseDate">
        /// The day close date.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool UndoDailyClosing(string officeNumber, DateTime dayCloseDate)
        {
            using (var uow = new UnitOfWork(DatabaseHelper.GetSession()))
            {
                var arguments = new
                {
                    DayCloseDate = dayCloseDate.ToShortDateString(),
                    OfficeNum = officeNumber
                };
                return DatabaseHelper.ExecuteStoredProcedure(uow.Session, "UndoDayClose", ":DayCloseDate, :OfficeNum", arguments);
            }
        }

        /// <summary>
        /// The get last unclosed day.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static IEnumerable<DayClose> GetLastClosedDay(string officeNumber)
        {
            const string Sql = @"SELECT DayCloseId, TransDate FROM DayClose WHERE dayCloseId = (Select MAX(daycloseId) FROM dayClose WHERE OfficeNum = :officeNumber)";

            using (var uow = new UnitOfWork(DatabaseHelper.GetSession()))
            {
                var repo = new Repository<DayClose>(uow.Session);
                return repo.ListBySql(Sql, new { officeNumber });
            }
        }
    }
}
