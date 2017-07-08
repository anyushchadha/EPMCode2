// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DailyClosingController.cs" company="Eyefinity, Inc">
// 2013 Eyefinity, Inc    
// </copyright>
// <summary>
//   Defines the DailyClosingController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using CrystalDecisions.Shared;

    using Eyefinity.Enterprise.Business.Payment;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Legacy.Reports;
    using Eyefinity.PracticeManagement.Model;

    using NHibernate;

    /// <summary>
    /// The daily closing controller.
    /// </summary>
    [NoCache]
    [ValidateHttpAntiForgeryToken]
    public class DailyClosingController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly DailyClosingIt2Manager it2Business;

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyClosingController"/> class.
        /// </summary>
        /// <param name="it2Business">
        /// The it 2 business.
        /// </param>
        public DailyClosingController(DailyClosingIt2Manager it2Business)
        {
            this.it2Business = it2Business;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyClosingController"/> class.
        /// </summary>
        public DailyClosingController()
        {
            this.it2Business = new DailyClosingIt2Manager();
        }

        // GET api/dailyclosing

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="dailyClosing">
        /// The daily Closing.
        /// </param>
        /// <returns>
        /// The List
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "Daily Closing")]
        public HttpResponseMessage GetDailyClosing([FromUri]DailyClosing dailyClosing)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, this.it2Business.GetDailyClosing(dailyClosing.OfficeNumber, dailyClosing.PostingDate, dailyClosing.UnclosedDaysCount));
            }
            catch (ObjectNotFoundException e)
            {
                Logger.Error(string.Format("GetDailyClosing(postingDate = {0}, {1}, {2}", dailyClosing.PostingDate, "\n", e));
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetDailyClosing(postingDate = {0}, {1}, {2}",  dailyClosing.PostingDate, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>
        /// The check office closing time.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="postingDate">
        /// The posting date.
        /// </param>
        /// <returns>
        /// The <see cref="DailyClosing"/>.
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "Daily Closing")]
        public DailyClosing CheckOfficeClosingTime(string officeNumber, string postingDate)
        {
            return this.it2Business.CheckOfficeClosingTime(officeNumber, Convert.ToDateTime(postingDate));
        }

        /// <summary>
        /// The undo daily closing.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="postingDate">
        /// The posting date.
        /// </param>
        /// <returns>
        /// The result
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "Daily Closing")]
        public HttpResponseMessage UndoDailyClosing(string officeNumber, string postingDate)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, this.it2Business.UndoDailyClosing(officeNumber, Convert.ToDateTime(postingDate)));
            }
            catch (ObjectNotFoundException e)
            {
                Logger.Error(string.Format("GetDailyClosing(postingDate = {0}, {1}, {2}", postingDate, "\n", e));
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetDailyClosing(postingDate = {0}, {1}, {2}", postingDate, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        // GET api/dailyclosing/5

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Daily Closing
        /// </returns>
        [Authorize(Roles = "Daily Closing")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/dailyclosing

        /// <summary>
        /// The save daily closing.
        /// </summary>
        /// <param name="closingDetails">
        /// The closing details.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [Authorize(Roles = "Daily Closing")]
        public HttpResponseMessage SaveDailyClosing(DailyClosing closingDetails)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, this.it2Business.SaveDailyClosing(closingDetails, closingDetails.OfficeNumber, closingDetails.UserId));
            }
            catch (NHibernate.Exceptions.GenericADOException genericAdoException)
            {
                var msg = genericAdoException.InnerException;
              
                if (msg.ToString().Contains("Violation of UNIQUE KEY constraint 'AK_DayClose_OfficeNum_TransDate'"))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, this.it2Business.HandleDuplicateDayClosingTransactionDate(closingDetails.OfficeNumber));
                }

                var error = string.Format("GetDailyClosing(postingDate = {0}, {1}, {2}", closingDetails.PostingDate, "\n", genericAdoException);
                return HandleExceptions.LogExceptions(error, Logger, genericAdoException);
            }
            catch (ObjectNotFoundException e)
            {
                Logger.Error(string.Format("SaveDailyClosing(postingDate = {0}, {1}, {2}", closingDetails.PostingDate, "\n", e));
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetDailyClosing(postingDate = {0}, {1}, {2}", closingDetails.PostingDate, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>
        /// The get daily closing id.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <param name="selectedDate">
        /// The selected date.
        /// </param>
        /// <param name="flag">
        /// The flag.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [Authorize(Roles = "Daily Closing")]
        public int GetDailyClosingId(string officeNumber, string selectedDate, bool flag)
        {
            return this.it2Business.GetDailyClosingId(officeNumber, selectedDate, flag);
        }

        /// <summary>
        /// The get report content.
        /// </summary>
        /// <param name="officeId">
        /// The office id.
        /// </param>
        /// <param name="dayCloseId">
        /// The day close id.
        /// </param>
        /// <returns>
        /// The <see cref="MemoryStream"/>.
        /// </returns>
        [NonAction]
        [Authorize]
        public Stream GetReportContent(string officeId, int dayCloseId)
        {
            var dayClose = this.it2Business.GetReportContent(officeId, dayCloseId);

            dayClose.Details = dayClose.Details.OrderBy(x => x.PaymentTypeString).ToList();
            var report = ReportFactory.CreateReportWithManualDispose<DailyClosingReport>();

            // This will be disposed when the report has been streamed.
            report.SetDataSource(dayClose.Details);

            var office = this.it2Business.GetOfficeById(officeId);
            report.SetParameterValue("Store", office.ID + " " + office.Name);
            report.SetParameterValue("Phone", office.FormattedPhoneNumber);
            report.SetParameterValue("Date", dayClose.TransactionDate);
            report.SetParameterValue("Employee", this.it2Business.GetEmployeeFullName(dayClose.EmployeeId));
            report.SetParameterValue("ClosingTime", dayClose.CloseTime);

            var refunds = this.it2Business.GetTotalCashRefund(dayClose.ID);
            report.SetParameterValue("RefundQuantity", refunds[1]);
            report.SetParameterValue("RefundAmount", refunds[0]);

            var coupon = this.it2Business.GetTotalCoupon(dayClose.ID);
            report.SetParameterValue("CouponQuantity", coupon[1]);
            report.SetParameterValue("CouponAmount", coupon[0]);

            var check = this.it2Business.GetTotalChecks(dayClose.ID);
            report.SetParameterValue("CheckQuantity", check[1]);
            report.SetParameterValue("CheckAmount", check[0]);

            var insurancePayments = new DailyClosingIt2Manager().GetDailyCarrierPayments(office.Company.ID, dayClose.TransactionDate);
            var payments = insurancePayments.ToList();
            report.SetParameterValue("InsuranceChecks", payments[0].Actual);
            report.SetParameterValue("InsuranceEFT", payments[1].Actual);

            var ms = report.ExportToStream(ExportFormatType.PortableDocFormat);

            // report has been streamed, it is safe to dispose it
            ReportFactory.CloseAndDispose(report); 

            return ms;
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <returns>
        /// The count of unclosed business days
        /// </returns>
        [HttpGet]
        [Authorize]
        public HttpResponseMessage GetPreviousUnclosedBusinessDaysInfo(string officeNumber)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, this.it2Business.GetPreviousUnclosedBusinessDaysInfo(officeNumber));
            }
            catch (ObjectNotFoundException e)
            {
                Logger.Error(string.Format("GetPreviousUnclosedBusinessDaysInfo(officeNumber = {0} {1} {2}", officeNumber, "\n", e));
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetPreviousUnclosedBusinessDaysInfo(officeNumber = {0} {1} {2}", officeNumber, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <returns>
        /// The count of unclosed business days
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "Daily Closing")]
        public HttpResponseMessage GetPreviousUnclosedBusinessDaysCount(string officeNumber)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, this.it2Business.GetPreviousUnclosedBusinessDaysCount(officeNumber));
            }
            catch (ObjectNotFoundException e)
            {
                Logger.Error(string.Format("GetPreviousUnclosedBusinessDaysCount(officeNumber = {0} {1} {2}", officeNumber, "\n", e));
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetPreviousUnclosedBusinessDaysCount(officeNumber = {0} {1} {2}", officeNumber, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }
    }
}
