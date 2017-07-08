// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the ReportingController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.AccessControl;
    using System.Text;
    using System.Web.Mvc;

    using Castle.Core.Internal;

    using CrystalDecisions.CrystalReports.Engine;
    using CrystalDecisions.Shared;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using ScheduledRecallReport = Eyefinity.PracticeManagement.Legacy.Reports.ScheduledRecallReport;
    
    /// <summary>
    /// The reporting controller.
    /// </summary>
    [NoCache]
    [ValidateHttpAntiForgeryToken]
    public class ReportingController : Controller
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: /Reporting/

        /// <summary>The reporting.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult MyReports()
        {
            return this.View();
        }

        /// <summary>The reporting.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult MyDashboardReports()
        {
            return this.View();
        }

        /// <summary>The appointment status.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult AppointmentStatus()
        {
            return this.PartialView();
        }

        /// <summary>The appointment insurance.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult AppointmentInsurance()
        {
            return this.PartialView();
        }

        /// <summary>The appointment schedule.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult AppointmentSchedule()
        {
            return this.PartialView();
        }

        /// <summary>The appointment schedule.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult AppointmentScheduleSummary()
        {
            return this.PartialView();
        }

        /// <summary>The appointment reschedule.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult AppointmentReschedule()
        {
            return this.PartialView();
        }

        /// <summary>The appointment service type.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult AppointmentServiceType()
        {
            return this.PartialView();
        }

        /// <summary>The patient follow up notes.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult PatientFollowupNotes()
        {
            return this.PartialView();
        }

        /// <summary>The patient open EEHR exams.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult PatientOpenEehrExams()
        {
            return this.PartialView();
        }

        /// <summary>The patient audit.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult PatientAudit()
        {
            return this.PartialView();
        }

        /// <summary>The patient billing history.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult PatientBillingHistory()
        {
            return this.PartialView();
        }

        /// <summary>The patient audit.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult PatientStatements()
        {
            return this.PartialView();
        }

        /// <summary>The patient orders work in progress report.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult PatientWorkInProgress()
        {
            return this.PartialView();
        }

        /// <summary>The default.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult Default()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The marketing consultant patient recall.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Marketing And Recalls")]
        [HttpGet]
        public ActionResult MarketingConsultantPatientRecall()
        {
            // ReSharper disable once Mvc.PartialViewNotResolved
            return this.PartialView();
        }

        /// <summary>
        /// The marketing data mining.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Marketing And Recalls")]
        [HttpGet]
        public ActionResult MarketingDataMining()
        {
            // ReSharper disable once Mvc.PartialViewNotResolved
            return this.PartialView();
        }

        /// <summary>
        /// The marketing recall.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Marketing And Recalls")]
        [HttpGet]
        public ActionResult MarketingRecall()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The marketing survey.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Marketing And Recalls")]
        [HttpGet]
        public ActionResult MarketingSurvey()
        {
            // ReSharper disable once Mvc.PartialViewNotResolved
            return this.PartialView();
        }

        /// <summary>The Daily Flash Sales by Resource.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>/// 
        [Authorize(Roles = "Sales Reports")]
        [HttpGet]
        public ActionResult DailyFlashSales()
        {
            return this.PartialView();
        }

        /// <summary>The Cash Receipt Summary.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>/// 
        [Authorize(Roles = "Sales Reports")]
        [HttpGet]
        public ActionResult CashReceiptSummary()
        {
            return this.PartialView();
        }

        /// <summary>The Refunds & Adjustments Sales.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [Authorize(Roles = "Sales Reports")]
        [HttpGet]
        public ActionResult RefundAdjustmentSales()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The daily transaction sales.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Sales Reports")]
        [HttpGet]
        public ActionResult DailyTransactionPayment()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The daily transaction sales.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Sales Reports")]
        [HttpGet]
        public ActionResult DailyTransactionSales()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The office flash sales.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Sales Reports")]
        [HttpGet]
        public ActionResult OfficeFlashSales()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The item sales.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Sales Reports")]
        [HttpGet]
        public ActionResult DiscountAnalysis()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The item sales.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Sales Reports")]
        [HttpGet]
        public ActionResult ItemSales()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The item sales.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult InventoryFrameSalesByItem()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The item sales.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult ProductionByProvider()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The patient accounts receivable aging.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult PatientAccountsReceivableAging()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The monthly production by provider
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult MonthlyProductionByProvider()
        {
            return this.PartialView();
        }

        /// <summary>
        /// The monthly production summary
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult MonthlyProductionSummary()
        {
            return this.PartialView();
        }

        /// <summary>The inventory valuation report.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult InventoryValuation()
        {
            return this.PartialView();
        }

        /// <summary>The cost of sales analysis by transaction</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult CostOfSalesAnalysisByOffice()
        {
            return this.PartialView();
        }

        /// <summary>The patient undelivered orders report</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult UndeliveredOrders()
        {
            return this.PartialView();
        }

        /// <summary>The patient remake report</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult PatientRemake()
        {
            return this.PartialView();
        }

        /// <summary>Miscellaneous payment report</summary>
        /// <returns><see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult MiscPayment()
        {
            return this.PartialView();
        }

        /// <summary>Titled sales adjustment report</summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TitledSalesAdjustment()
        {
            return this.PartialView();
        }

        /// <summary>Patient credit balance report</summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PatientCreditBalance()
        {
            return this.PartialView();
        }

        /// <summary>The get PDF document.</summary>
        /// <param name="id">The id.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="fileType">The file type.</param> 
        /// <param name="officeNum">The office Number.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult GetFileReport(int id, string startDate, string endDate, string fullName, string fileType, string officeNum)
        {
            try
            {
                var dateRange = DateTime.Parse(startDate).ToString("MM/dd/yyyy") + " to " + DateTime.Parse(endDate).ToString("MM/dd/yyyy");
                var scheduledReport = ReportFactory.CreateReportWithManualDispose<ScheduledRecallReport>();
                var stream = new Api.PatientReportsController().GetReportContent(id, fileType, dateRange, fullName, officeNum, scheduledReport);
                if (stream != null)
                {
                    this.Response.Clear();
                    this.Response.ClearHeaders();
                    this.Response.Buffer = true;
                    if (fileType == "EXCEL")
                    {
                        Response.ContentType = "application/vnd.ms-excel";
                        Response.AddHeader("content-disposition", "attachment; filename=RecallDetails.xls");
                    }
                    else
                    {
                        this.Response.ContentType = "application/pdf";
                        this.Response.AppendHeader(
                            "Content-Disposition", fileType == "PDF" ? "inline; filename=RecallDetails.pdf" : "inline; filename=SurveyDetails.pdf");
                    }

                    this.Response.BinaryWrite(stream.ToArray());
                    this.Response.Flush();
                    this.Response.End();
                }
                else
                {
                    this.Response.Clear();
                    this.Response.Buffer = true;
                    this.Response.AddHeader("content-disposition", "attachment; filename=ExportReports.txt");
                    this.Response.ContentType = "application/octet-stream";
                    const string Msg = "This report has timed out, please re-run the report. If the problem continues, notify customer support.";
                    using (var stream2 = Msg.ToStream())
                    {
                        stream2.CopyTo(this.Response.OutputStream);
                    }

                    this.Response.End();

                    if (Logger.IsErrorEnabled)
                    {
                        Logger.Error(string.Format("GetReportContent failed for id {0} Office {1} fullname {2} filetype {3} daterange {4} ", id, officeNum, fullName, fileType, dateRange));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetFileReport generated exception - " + ex);
            }

            return null;
        }

        /// <summary>The get PDF containg labels.</summary>
        /// <param name="id">The id.</param>
        /// <param name="officeNum">The office Number.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult GetLabelsReport(int id, string officeNum)
        {
            ////try
            ////{
                var uploadsLocation = this.Server.MapPath("~/App_Data/Uploads");
                if (!Directory.Exists(uploadsLocation))
                {
                    Directory.CreateDirectory(uploadsLocation);
                    var di = new DirectoryInfo(uploadsLocation);
                    var fsar = new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow);
                    var ds = di.GetAccessControl();
                    ds.AddAccessRule(fsar);
                    di.SetAccessControl(ds);
                }

                var pdfFile = this.Server.MapPath("~/App_Data/Uploads/" + DateTime.Now.ToString("MMddyyhhmmssfff") + ".pdf");
                var content = new Api.PatientReportsController().GetPrintLabels(id, officeNum, pdfFile);
                if (content != null)
                {
                    this.Response.Clear();
                    this.Response.ClearHeaders();
                    this.Response.ContentType = "application/pdf";
                    this.Response.AppendHeader("Content-Disposition", "inline; filename=Document.pdf");
                    this.Response.OutputStream.Write(content, 0, content.Length);
                    this.Response.Flush();
                    this.Response.End();
                }
            ////}
            ////catch (Exception ex)
            ////{
            ////    Logger.Error("GetLabelsReport generated exception - " + ex);
            ////}

            return null;
        }

        /// <summary>The get PDF containg labels.</summary>
        /// <param name="recallId"></param>
        /// <param name="officeNum">The office Number.</param>
        /// <param name="printFormat"></param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult GeneratePrintableDocument(int recallId, string officeNum, int printFormat)
        {
            try
            {
                var contents = new Api.PatientReportsController().GeneratePrintableDocument(recallId, officeNum, printFormat);
                this.Response.Clear();
                this.Response.ClearHeaders();
                this.Response.ContentType = "application/pdf";
                this.Response.OutputStream.Write(contents, 0, contents.Length);
                this.Response.Flush();
                this.Response.End();
            }
            catch (Exception ex)
            {
                Logger.Error("GetLabelsReport generated exception - " + ex);
            }

            return null;
        }

        /// <summary>The PDF view.</summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult PdfView(string id)
        {
            return this.View();
        }

        /// <summary>The generate Excel file.</summary>
        /// <param name="content"></param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult GenerateExcel(string content)
        {
           if (content.IsNullOrEmpty())
            {
                this.Response.Clear();
                this.Response.Buffer = true;
                this.Response.AddHeader("content-disposition", "attachment; filename=ExportReports.txt");
                this.Response.ContentType = "application/octet-stream";

                const string Msg = "This report has timed out, please re-run the report. If the problem continues, notify customer support.";
                using (var stream2 = Msg.ToStream())
                {
                    stream2.CopyTo(this.Response.OutputStream);
                }

                this.Response.End();
            }
            else
            {
                this.Response.Clear();
                this.Response.Buffer = true;
                this.Response.AddHeader("content-disposition", "attachment; filename=ExportReports.xls");
                const string Content = "application/vnd.ms-excel";
                this.Response.ContentType = Content;
                using (var stream = content.ToStreamFromBase64String())
                {
                    if (stream == null || stream.Length <= 0)
                    {
                        const string Msg = "This report has timed out, please re-run the report. If the problem continues, notify customer support.";
                        using (var msgStream = Msg.ToStream())
                        {
                            msgStream.CopyTo(this.Response.OutputStream);
                        }
                    }
                    else
                    {
                        this.CreateTemporaryReportDocument(stream, Guid.NewGuid().ToString(), ExportFormatType.Excel);  
                    }
                }

                this.Response.End();
            }

            return null;
        }

        /// <summary>The generate PDF file.</summary>
        /// <param name="content">the file id.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult GeneratePdf(string content)
        {
            if (content.IsNullOrEmpty())
            {
                this.Response.Clear();
                this.Response.Buffer = true;
                this.Response.AddHeader("content-disposition", "attachment; filename=ExportReports.txt");
                this.Response.ContentType = "application/octet-stream";
                const string Msg = "This report has timed out, please re-run the report. If the problem continues, notify customer support.";
                using (var stream2 = Msg.ToStream())
                {
                    stream2.CopyTo(this.Response.OutputStream);
                }

                this.Response.End();
            }
            else
            {
                this.Response.Clear();
                this.Response.Buffer = true;
                this.Response.AddHeader("content-disposition", "attachment; filename=ExportReports.pdf");
                const string Content = "application/pdf";
                this.Response.ContentType = Content;
                using (var stream = content.ToStreamFromBase64String())
                {
                    if (stream == null || stream.Length <= 0)
                    {
                        const string Msg = "This report has timed out, please re-run the report. If the problem continues, notify customer support.";
                        using (var msgStream = Msg.ToStream())
                        {
                            msgStream.CopyTo(this.Response.OutputStream);
                        }
                    }
                    else
                    {
                        this.CreateTemporaryReportDocument(stream, Guid.NewGuid().ToString(), ExportFormatType.PortableDocFormat);
                    }
                }

                this.Response.Flush();
                this.Response.End();
            }

            return null;
        }

        public ActionResult ViewReport(string id)
        {
            var httpSessionStateBase = this.HttpContext.Session;
            if (httpSessionStateBase != null)
            {
                var content = (string)httpSessionStateBase[id];

                if (content.IsNullOrEmpty())
                {
                    this.Response.Clear();
                    this.Response.Buffer = true;
                    this.Response.AddHeader("content-disposition", "inline; filename=ExportReports.txt");
                    this.Response.ContentType = "application/octet-stream";
                    const string Msg = "This report has timed out, please re-run the report. If the problem continues, notify customer support.";
                    using (var stream2 = Msg.ToStream())
                    {
                        stream2.CopyTo(this.Response.OutputStream);
                    }

                    this.Response.End();
                }
                else
                {
                    this.Response.AddHeader("content-disposition", "inline; filename=ExportReports.pdf");
                    const string Content = "application/pdf";
                    this.Response.Clear();
                    this.Response.Buffer = true;
                    this.Response.ContentType = Content;
                    using (var stream = content.ToStreamFromBase64String())
                    {
                        if (stream == null || stream.Length <= 0)
                        {
                            const string Msg = "This report has timed out, please re-run the report. If the problem continues, notify customer support.";
                            using (var msgStream = Msg.ToStream())
                            {
                                msgStream.CopyTo(this.Response.OutputStream);
                            }
                        }
                        else
                        {
                            this.CreateTemporaryReportDocument(stream, id, ExportFormatType.PortableDocFormat);
                        }
                    }

                    httpSessionStateBase.Remove(id);
                    this.Response.Flush();
                    this.Response.End();
                }
            }
          
            return null;
        }

        private void CreateTemporaryReportDocument(Stream inputdata, string id, ExportFormatType formatType)
        {
            var filePath = string.Format("{0}{1}", Path.GetTempPath(), id);
            if (inputdata != null && inputdata.Length > 0)
            {
                using (var fs = System.IO.File.Create(filePath))
                {
                    inputdata.Seek(0, SeekOrigin.Begin);
                    inputdata.CopyTo(fs);
                }
                
                var rpt = new ReportDocument();
                rpt.Load(filePath);
                using (var outStream = rpt.ExportToStream(formatType))
                {
                    var data = outStream.ToByteArrayBytes();
                    if (data != null)
                    {
                        this.Response.OutputStream.Write(data, 0, data.Length);
                    }
                }
                
                rpt.Close();
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}