// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatientReportsController.cs" company="Eyefinity, Inc.">
//   Eyefinity, Inc. - 2013
// </copyright>
// <summary>
//   Defines the PatientReportsController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    using Common;
    using Common.Api;
    using CrystalDecisions.Shared;

    using Enterprise.Business.Admin;
    using Enterprise.Business.Common;
    using Enterprise.Business.Patient;
    using Enterprise.Business.Reports;
    using FeatureManager.Services;
    using IT2.Core;
    using IT2.Core.Enums;
    using IT2.Core.Feature;
    using IT2.Reports;
    using IT2.Reports.SalesAudit;
    using IT2.Services;
    using Legacy.Reports;
    using log4net;
    using Model.Patient.Orders;
    using Model.Reports;

    using Container = IT2.Ioc.Container;
    using OfficeNotesReport = Legacy.Reports.OfficeNotesReport;
    using PatientAuditNotesReport = Legacy.Reports.PatientAuditNotesReport;
    using PatientCLReport = Legacy.Reports.PatientContactLensReport;
    using PatientEyeGlassesReport = Legacy.Reports.PatientEyeglassesReport;
    using PatientHardCLReport = Legacy.Reports.PatientHardContactLensReport;

    /// <summary>The patient reports controller.</summary>
    [NoCache]
    public class PatientReportsController : Controller
    {
        #region Fields
        /// <summary>The logger.</summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PatientReportsController));

        /// <summary>
        /// The company Id.
        /// </summary>
        private readonly string companyId = string.Empty;

        /// <summary>
        /// The office number.
        /// </summary>
        private readonly string officeNumber = string.Empty;

        /// <summary>
        /// The User Id.
        /// </summary>
        private readonly int userId;

        /// <summary>The office services.</summary>
        private readonly IOfficeServices officeServices;

        /// <summary>Feature manager services</summary>
        private readonly IFeatureManagerService featureManager;

        /// <summary>The report it 2 manager.</summary>
        private readonly ReportIt2Manager reportIt2Manager;

        /// <summary>The report it 2 manager.</summary>
        private readonly ReportsRxIt2Manager reportsRxIt2Manager;

        /// <summary>The patient reports it 2 manager.</summary>
        private readonly PatientReportsIt2Manager patientReportsIt2Manager;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PatientReportsController" /> class.
        /// </summary>
        public PatientReportsController()
        {
            this.reportIt2Manager = new ReportIt2Manager();
            this.reportsRxIt2Manager = new ReportsRxIt2Manager();
            this.officeServices = Container.Resolve<IOfficeServices>();
            this.patientReportsIt2Manager = new PatientReportsIt2Manager();
            this.featureManager = Container.Resolve<IFeatureManagerService>();

            var authorizationTicketHelper = new AuthorizationTicketHelper();
            this.companyId = authorizationTicketHelper.GetCompanyId();
            this.officeNumber = authorizationTicketHelper.GetPracticeLocationId();
            this.userId = authorizationTicketHelper.GetUserInfo().Id;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The show generic report in new window.
        /// </summary>
        /// <param name="reportsCriteria">
        /// The reports criteria.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [HttpPost]
        public string ShowGenericReportInNewWin(PatientReportCriteria reportsCriteria)
        {
            try
            {
                AccessControl.VerifyUserAccessToOffice(reportsCriteria.OfficeNum);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return "403";
            }

            var result = Guid.NewGuid().ToString();
            var httpSessionStateBase = this.HttpContext.Session;
            reportsCriteria.CompanyId = OfficeHelper.GetCompanyId(reportsCriteria.OfficeNum);

            try
            {
                ////Report Auditing ALSL-5986
                var auditLog = new AuditLogs();
                auditLog.AddAuditLogEntryForReport($"EPM PatientReportsController ShowGenericReportInNewWin {reportsCriteria.ReportName}");

                switch (reportsCriteria.ReportName)
                {
                    default:
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info(string.Format("Report Name " + reportsCriteria.ReportName + " is not handled for Patient Report."));
                        }

                        break;

                    case "PatientFollowupNotesReport":
                        var noteReport = this.reportIt2Manager.GetPatientFollowupNotesReport(reportsCriteria);
                        if (noteReport.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            var officeNotesReport = ReportFactory.CreateReportWithManualDispose<OfficeNotesReport>();
                            officeNotesReport.SetDataSource(noteReport);
                            var office = this.officeServices.GetByID(reportsCriteria.OfficeNum);
                            officeNotesReport.SetParameterValue("Store", office.Name);
                            officeNotesReport.SetParameterValue("OfficeNum", office.ID);
                            var phone = office.PhoneNumber ?? string.Empty;
                            officeNotesReport.SetParameterValue("Telephone", phone);

                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = officeNotesReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(officeNotesReport);
                        }

                        break;

                    case "PatientOpenEehrExamsReport":
                        var openEehrExams = this.reportIt2Manager.GetPatientOpenEehrExamsReport(reportsCriteria);
                        if (openEehrExams.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            var openEehrExamsReport = ReportFactory.CreateReportWithManualDispose<PatientOpenEehrExamReport>();
                            openEehrExamsReport.SetDataSource(openEehrExams);
                            var text = reportsCriteria.StartDate.ToShortDateString() + " - " + reportsCriteria.EndDate.ToShortDateString();
                            openEehrExamsReport.SetParameterValue("DateRange", text);

                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = openEehrExamsReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(openEehrExamsReport);
                        }

                        break;

                    case "PatientAuditNotesReport":
                        var notesReport = this.reportIt2Manager.GetPatientAuditNotesReport(reportsCriteria);
                        if (notesReport.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            var patientAuditNotesReport = ReportFactory.CreateReportWithManualDispose<PatientAuditNotesReport>();
                            patientAuditNotesReport.SetDataSource(notesReport);

                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = patientAuditNotesReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(patientAuditNotesReport);
                        }

                        break;

                    case "PatientBillingHistory":
                        PatientBillingHistoryReport patientBillingHistoryClass =
                            this.patientReportsIt2Manager.GetPatientBillingHistoryData(reportsCriteria);

                        if (patientBillingHistoryClass.BillingTransactionList.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            var patientBillingHistoryReport = ReportFactory.CreateReportWithManualDispose<PatientBillingHistoryTransactionReport>();
                            var reportDateRange = reportsCriteria.StartDate.ToShortDateString() + " - "
                                                    + reportsCriteria.EndDate.ToShortDateString();

                            patientBillingHistoryReport.SetDataSource(patientBillingHistoryClass.BillingTransactionList);

                            patientBillingHistoryReport.SetParameterValue(
                                "CompanyAddress",
                                string.IsNullOrEmpty(patientBillingHistoryClass.CompanyAddress) ? string.Empty : patientBillingHistoryClass.CompanyAddress);
                            patientBillingHistoryReport.SetParameterValue(
                                "CompanyName",
                                patientBillingHistoryClass.CompanyName);
                            patientBillingHistoryReport.SetParameterValue(
                                "CompanyCityStateZip",
                                string.IsNullOrEmpty(patientBillingHistoryClass.CompanyCityStateZip) ? string.Empty : patientBillingHistoryClass.CompanyCityStateZip);
                            patientBillingHistoryReport.SetParameterValue(
                                "PatientFullName",
                                string.IsNullOrEmpty(patientBillingHistoryClass.PatientFullName) ? string.Empty : patientBillingHistoryClass.PatientFullName);
                            patientBillingHistoryReport.SetParameterValue(
                                "PatientAddress",
                                string.IsNullOrEmpty(patientBillingHistoryClass.PatientAddress) ? string.Empty : patientBillingHistoryClass.PatientAddress);
                            patientBillingHistoryReport.SetParameterValue(
                                "PatientCityStateZip",
                                string.IsNullOrEmpty(patientBillingHistoryClass.PatientCityStateZip) ? string.Empty : patientBillingHistoryClass.PatientCityStateZip);
                            patientBillingHistoryReport.SetParameterValue(
                                "ProviderName",
                                string.IsNullOrEmpty(patientBillingHistoryClass.Provider) ? string.Empty : patientBillingHistoryClass.Provider);
                            patientBillingHistoryReport.SetParameterValue("StatementDateRange", reportDateRange);

                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = patientBillingHistoryReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(patientBillingHistoryReport);
                        }

                        break;

                    case "PatientStatementsReport":

                        bool isDetailedPatientLetter;
                        var patientStatementPreferences = new Dictionary<string, string>();
                        var featureDetailedPatientLetter = this.featureManager.GetFeatureConfigurationInCompany(FeatureUUIDs.DetailedPatientLetter, reportsCriteria.CompanyId);
                        isDetailedPatientLetter = featureDetailedPatientLetter.available;
                        if (isDetailedPatientLetter)
                        {
                            patientStatementPreferences = FeatureManagerService.ExtractFeatureProperties(featureDetailedPatientLetter);
                            bool isDisplayProviderNameandNPI = patientStatementPreferences[DetailedPatientStatements.IsProviderNameAndNpi.GetStringValue()] == "true";
                            var patientStatementsInfo = this.reportIt2Manager.GetPatientStatements(reportsCriteria, patientStatementPreferences);
                            if (patientStatementsInfo.PatientStatements.Count == 0
                                || patientStatementsInfo.PatientStatementsDetails == null)
                            {
                                result = "noData";
                            }
                            else
                            {
                                var patientStatementsReport =
                                    ReportFactory.CreateReportWithManualDispose<IT2.Reports.PatientStatements>();
                                patientStatementsInfo.PatientStatements =
                                    patientStatementsInfo.PatientStatements.Where(e => e.AmountDue != 0).ToList();
                                patientStatementsReport.SetDataSource(patientStatementsInfo.PatientStatements);
                                patientStatementsReport.Subreports[0].SetDataSource(
                                    patientStatementsInfo.PatientStatementsDetails);
                                patientStatementsReport.SetParameterValue(
                                    "CompanyPhone",
                                    patientStatementsInfo.CompanyPhone);
                                patientStatementsReport.SetParameterValue(
                                    "CompanyName",
                                    patientStatementsInfo.CompanyName);
                                patientStatementsReport.SetParameterValue(
                                    "CompanyAddress",
                                    patientStatementsInfo.CompanyAddress);
                                patientStatementsReport.SetParameterValue(
                                    "CompanyCityStateZip",
                                    patientStatementsInfo.CompanyCityStateZip);
                                patientStatementsReport.SetParameterValue("RemitName", patientStatementsInfo.RemitName);
                                patientStatementsReport.SetParameterValue(
                                    "RemitAddress",
                                    patientStatementsInfo.RemitAddress);
                                patientStatementsReport.SetParameterValue(
                                    "RemitCityStateZip",
                                    patientStatementsInfo.RemitCityStateZip);
                                patientStatementsReport.SetParameterValue(
                                    "IsDisplayProviderNameandNPI",
                                    isDisplayProviderNameandNPI);

                                this.AdjustLogoSize(
                                    patientStatementsReport.Section2,
                                    reportsCriteria.CompanyId,
                                    reportsCriteria.OfficeNum);

                                if (httpSessionStateBase != null)
                                {
                                    httpSessionStateBase[result] =
                                        patientStatementsReport.ExportToStream(ExportFormatType.CrystalReport)
                                            .ToBase64String();
                                }

                                ReportFactory.CloseAndDispose(patientStatementsReport);
                            }
                        }
                        else
                        {
                            string companyName;
                            string remitName;
                            string remitAddress;
                            string remitCityStateZip;
                            string officeNameandNum;
                            string compAddress;
                            string companyCityStateZip;

                            var statmentsReport = this.reportIt2Manager.GetPatientStatementsReport(
                                reportsCriteria,
                                out officeNameandNum,
                                out companyName,
                                out compAddress,
                                out companyCityStateZip,
                                out remitName,
                                out remitAddress,
                                out remitCityStateZip);
                            if (statmentsReport.Count == 0)
                            {
                                result = "noData";
                            }
                            else
                            {
                                var patientStatementsReport =
                                    ReportFactory.CreateReportWithManualDispose<Legacy.Reports.PatientStatementReport>();
                                patientStatementsReport.SetDataSource(statmentsReport);

                                patientStatementsReport.SetParameterValue("Office", officeNameandNum);
                                patientStatementsReport.SetParameterValue("CompanyName", companyName);
                                patientStatementsReport.SetParameterValue("CompanyAddress", compAddress);
                                patientStatementsReport.SetParameterValue("CompanyCityStateZip", companyCityStateZip);
                                patientStatementsReport.SetParameterValue("RemitName", remitName);
                                patientStatementsReport.SetParameterValue("RemitAddress", remitAddress);
                                patientStatementsReport.SetParameterValue("RemitCityStateZip", remitCityStateZip);

                                this.AdjustLogoSize(
                                 patientStatementsReport.Section2,
                                 reportsCriteria.CompanyId,
                                 reportsCriteria.OfficeNum);

                                if (httpSessionStateBase != null)
                                {
                                    httpSessionStateBase[result] =
                                        patientStatementsReport.ExportToStream(ExportFormatType.CrystalReport)
                                            .ToBase64String();
                                }

                                ReportFactory.CloseAndDispose(patientStatementsReport);
                            }
                        }

                        break;

                    case "PatientWorkInProgress":
                        string statusType;
                        string officeName;
                        string orderTypeText;

                        var workInProgressReport =
                            this.reportIt2Manager.GetPatientOrdersWorkInProgressReport(
                                reportsCriteria,
                                out officeName,
                                out statusType,
                                out orderTypeText);
                        if (workInProgressReport.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            var patientWorkInProgressReport = ReportFactory.CreateReportWithManualDispose<ExpectedDateReport>();
                            patientWorkInProgressReport.SetDataSource(workInProgressReport);

                            patientWorkInProgressReport.SetParameterValue(
                                "FromDate",
                                reportsCriteria.StartDate.ToString("MM/dd/yy"));
                            patientWorkInProgressReport.SetParameterValue(
                                "toDate",
                                reportsCriteria.EndDate.ToString("MM/dd/yy"));
                            patientWorkInProgressReport.SetParameterValue("Store", officeName);
                            patientWorkInProgressReport.SetParameterValue("Status", statusType);
                            patientWorkInProgressReport.SetParameterValue("OrderType", orderTypeText);

                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = patientWorkInProgressReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(patientWorkInProgressReport);
                        }

                        break;

                    case "UndeliveredOrders":
                        var undeliveredOrdersData = this.patientReportsIt2Manager.GetUndeliveredOrdersData(reportsCriteria);

                        if (undeliveredOrdersData.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            string reportHeader = string.Empty;
                            if (reportsCriteria.CashOnly)
                            {
                                reportHeader = "Undelivered Orders Report - Cash";
                            }
                            else
                            {
                                reportHeader = "Undelivered Orders Report - Insurance";
                            }

                            string rptbyorder = string.Empty;
                            if (reportsCriteria.ShowByOrders)
                            {
                                rptbyorder = "1";
                            }
                            else
                            {
                                rptbyorder = "0";
                            }

                            var undeliveredOrdersReport =
                                ReportFactory.CreateReportWithManualDispose<UndeliveredOrdersReport>();
                            undeliveredOrdersReport.SetDataSource(
                                (System.Collections.IEnumerable)
                                    undeliveredOrdersData.OrderBy(a => a.OfficeNum).ThenBy(a => a.OrderID));
                            undeliveredOrdersReport.SetParameterValue("ReportID", "AC137");
                            undeliveredOrdersReport.SetParameterValue("DateParameter",
                                reportsCriteria.StartDate.ToShortDateString());
                            undeliveredOrdersReport.SetParameterValue("ReportHeader", reportHeader);
                            undeliveredOrdersReport.SetParameterValue("ByOrder", rptbyorder);

                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] =
                                    undeliveredOrdersReport.ExportToStream(ExportFormatType.CrystalReport)
                                        .ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(undeliveredOrdersReport);
                        }

                        break;

                    case "PatientWelcomeForms":
                        var patientWelcomeFormsData = this.reportIt2Manager.GetPatientWelcomeForms(reportsCriteria);
                        if (patientWelcomeFormsData.PatientLiteDemographics.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            var patientWelcomeFormsReport = ReportFactory.CreateReportWithManualDispose<PatientWelcomeForms>();
                            patientWelcomeFormsReport.SetDataSource(patientWelcomeFormsData.PatientLiteDemographics);
                            patientWelcomeFormsReport.Subreports[0].SetDataSource(patientWelcomeFormsData.PatientLiteInsurances);
                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = patientWelcomeFormsReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(patientWelcomeFormsReport);
                        }

                        break;

                    case "PatientRemake":
                        var remakeData = this.patientReportsIt2Manager.GetPatientRemakeData(reportsCriteria);
                        if (remakeData.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            var groupType = "employee";
                            if (reportsCriteria.SortOption != null && reportsCriteria.SortOption.Equals("R"))
                            {
                                groupType = "reason";
                            }

                            var reasons = "ALL";
                            if (reportsCriteria.ReasonList != null && reportsCriteria.SelectedReasons != null && reportsCriteria.SelectedReasons.Count() != reportsCriteria.ReasonList.Count)
                            {
                                reasons = reportsCriteria.ReasonList
                                    .Where(x => reportsCriteria.SelectedReasons.Contains(x.Key))
                                    .OrderBy(s => s.Description)
                                    .Select(y => y.Description)
                                    .Aggregate((y, z) => y + ", " + z);
                            }

                            var report = ReportFactory.CreateReportWithManualDispose<RemakeReasonDetailsReport>();
                            report.SetDataSource(remakeData);
                            report.SetParameterValue("ReportID", "MISC101");
                            report.SetParameterValue("DateParameter", reportsCriteria.StartDate.ToShortDateString() + " - " + reportsCriteria.EndDate.ToShortDateString());
                            report.SetParameterValue("GroupType", groupType);
                            report.SetParameterValue("SelectedRemakeReasons", reasons);
                            report.SetParameterValue("ShowCompanyTotal", true);

                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = report.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(report);
                        }

                        break;

                    case "SavedOrders":
                        var savedOrders = this.patientReportsIt2Manager.GetSavedOrders(reportsCriteria);
                        if (savedOrders.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            var savedOrdersReport = ReportFactory.CreateReportWithManualDispose<CompanySavedOrders>();
                            savedOrdersReport.SetDataSource(savedOrders);
                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = savedOrdersReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(savedOrdersReport);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Patient Report Exception :" + ex);
            }

            return result;
        }

        /// <summary>
        /// The print rx report.
        /// </summary>
        /// <param name="examId">
        /// The exam id.
        /// </param>
        /// <param name="officeNum">
        /// The office number.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult PrintRxReport(int examId, string officeNum)
        {
            var patientHelper = new PatientHelper();
            var exam = patientHelper.GetPatientExamById(examId);
            var companyId = OfficeHelper.GetCompanyId(exam.OfficeNum);
            var util = new ReportUtilities();
            MemoryStream outStream;

            var providerId = patientHelper.GetProviderById(exam);

            if (exam.IsEyeglassExam())
            {
                var egrxreport = ReportFactory.CreateReportWithManualDispose<PatientEyeGlassesReport>();
                this.reportsRxIt2Manager.PrintEyeGlassesrxReport(exam, egrxreport, officeNum, examId, patientHelper, providerId);
                outStream =
               (MemoryStream)util.AddLogo(egrxreport, egrxreport.Section2, 0, companyId, exam.OfficeNum);
                ReportFactory.CloseAndDispose(egrxreport); // report has been streamed, it is safe to dispose it
            }
            else if (exam.ExamRxTypeID == IT2.Core.PatientExam.EXAM_TYPE_HARD_CONTACTLENS || exam.ExamRxTypeID == IT2.Core.PatientExam.EXAM_HARD_CL_INCOMPLETE)
            {
                var hardclreport = ReportFactory.CreateReportWithManualDispose<PatientHardCLReport>();
                this.reportsRxIt2Manager.PrintHardClrxReport(exam, hardclreport, officeNum, examId, patientHelper, providerId);
                outStream =
                (MemoryStream)util.AddLogo(hardclreport, hardclreport.Section2, 30, companyId, exam.OfficeNum);
                ReportFactory.CloseAndDispose(hardclreport); // report has been streamed, it is safe to dispose it
            }
            else
            {
                var clreport = ReportFactory.CreateReportWithManualDispose<PatientCLReport>();
                this.reportsRxIt2Manager.PrintClrxReport(exam, clreport, officeNum, examId, patientHelper, providerId);
                outStream =
                (MemoryStream)util.AddLogo(clreport, clreport.Section2, 0, companyId, exam.OfficeNum);
                ReportFactory.CloseAndDispose(clreport); // report has been streamed, it is safe to dispose it
            }

            if (outStream != null)
            {
                this.Response.Clear();
                this.Response.ClearHeaders();
                this.Response.ContentType = "application/pdf";
                ////this.Response.AddHeader("Content-Disposition", "inline; filename= filename=ContactLensRxReport.pdf");
                this.Response.BinaryWrite(outStream.ToArray());
                this.Response.End();
            }

            return null;
        }

        public ActionResult PrintEhrExamReport(int examId, string officeNum)
        {
            var manager = new PatientExamsIt2Manager();
            var companyId = OfficeHelper.GetCompanyId(officeNum);
            var util = new ReportUtilities();

            var patientExamReport = ReportFactory.CreateReportWithManualDispose<ExamReport>();
            manager.PrintPatientEhrExamReport(officeNum, examId, patientExamReport);
            var outStream = (MemoryStream)util.AddLogo(patientExamReport, patientExamReport.Section2, 0, companyId, officeNum);
            ReportFactory.CloseAndDispose(patientExamReport); // report has been streamed, it is safe to dispose it

            if (outStream != null)
            {
                this.Response.Clear();
                this.Response.ClearHeaders();
                this.Response.ContentType = "application/pdf";
                this.Response.BinaryWrite(outStream.ToArray());
                this.Response.End();
            }

            return null;
        }

        public ActionResult PrintOrderExamReport(int orderId, string officeNum)
        {
            var manager = new PatientExamsIt2Manager();
            var companyId = OfficeHelper.GetCompanyId(officeNum);
            var util = new ReportUtilities();

            var patientExamReport = ReportFactory.CreateReportWithManualDispose<ExamReport>();
            manager.PrintPatientOrderExamReport(orderId, patientExamReport);
            var outStream = (MemoryStream)util.AddLogo(patientExamReport, patientExamReport.Section2, 0, companyId, officeNum);
            ReportFactory.CloseAndDispose(patientExamReport); // report has been streamed, it is safe to dispose it

            if (outStream != null)
            {
                this.Response.Clear();
                this.Response.ClearHeaders();
                this.Response.ContentType = "application/pdf";
                this.Response.BinaryWrite(outStream.ToArray());
                this.Response.End();
            }

            return null;
        }

        [HttpPost]
        public string PrintContactLensOrderSummary(ContactLensOrderDetail detail)
        {
            var result = Guid.NewGuid().ToString();
            var httpSessionStateBase = this.HttpContext.Session;
            var authorizationTicketHelper = new AuthorizationTicketHelper();
            var officeNum = authorizationTicketHelper.GetPracticeLocationId();
            var userId = authorizationTicketHelper.GetUserInfo().Id;
            var manager = new ContactLensOrderIt2Manager();
            var companyId = authorizationTicketHelper.GetCompanyId();

            var report = ReportFactory.CreateReportWithManualDispose<ContactLensOrderSummaryReport>();
            manager.PrintContactLensOrderReport(detail.OrderNumber, report, detail.AwsResourceId, userId, officeNum, detail.PatientId, companyId, detail);
            if (httpSessionStateBase != null)
            {
                httpSessionStateBase[result] = report.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
            }

            ReportFactory.CloseAndDispose(report);
            return result;
        }

        public string PrintFromInProgressGrid(int orderId, string resourceId, int patientId)
        {
            var authorizationTicketHelper = new AuthorizationTicketHelper();
            var officeNum = authorizationTicketHelper.GetPracticeLocationId();
            var manager = new PatientOrderWorkInProgressManager();
            var summary = manager.GetIpOrderSummaryRow(this.companyId, patientId, resourceId);
            if (summary != null)
            {
                if (summary.OrderType != "Eyeglass")
                {
                   return this.PrintInProgressClOrderSummary(orderId, resourceId, patientId, this.userId, officeNum, this.companyId);
                }

                return this.PrintInProgressEgOrderSummary(resourceId, patientId, officeNum);
            }

            return null;
        }

        public string PrintInProgressEgOrderSummary(string resourceId, int patientId, string officeNum)
        {
            //// construct eg order detail from temp storage 

            var manager = new PatientOrderWorkInProgressManager();
            var detail = manager.GetIpEyeglassOrder(this.companyId, patientId, resourceId);
             return this.PrintEgOrderSummary(resourceId, detail);
        }

        public string PrintInProgressClOrderSummary(int orderId, string resourceId, int patientId, int user, string officeNum, string companyId)
        {
            var result = Guid.NewGuid().ToString();
            var httpSessionStateBase = this.HttpContext.Session;
            var manager = new ContactLensOrderIt2Manager();
            var util = new ReportUtilities();

            var report = ReportFactory.CreateReportWithManualDispose<ContactLensOrderSummaryReport>();
            manager.PrintContactLensOrderReport(orderId, report, resourceId, user, officeNum, patientId, companyId);
            if (httpSessionStateBase != null)
            {
                httpSessionStateBase[result] = report.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
            }

            ReportFactory.CloseAndDispose(report);
            return result;
        }

        [HttpPost]
        public string PrintEgOrderEstimatedCharges(int id, IEnumerable<EgOrderChargeLineItem> chargeLineItems)
        {
            var lst = chargeLineItems.ToList();
            var eligId = 0;
            var eligRow = new EgOrderChargeLineItem();
            if (lst.Count >= 1)
            {
                if (!string.IsNullOrEmpty(lst[0].Title))
                {
                    eligId = Convert.ToInt32(lst[0].Title);
                }

                eligRow = lst[0];
                lst.Remove(eligRow);
            }

            var result = Guid.NewGuid().ToString();
            var httpSessionStateBase = this.HttpContext.Session;
            var manager = new EyeglassOrderIt2Manager();

            var report = ReportFactory.CreateReportWithManualDispose<EgOrderEstimatedChargesReport>();
            manager.PrintEgOrderEstimatedChargesReport(id, eligId, lst, report);
            if (httpSessionStateBase != null)
            {
                httpSessionStateBase[result] = report.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
            }

            ReportFactory.CloseAndDispose(report);
            return result;
        }

        [HttpPost]
        public string PrintEgOrderSummary(string id, EyeglassOrderDetail detail)
        {
            var result = Guid.NewGuid().ToString();
            var httpSessionStateBase = this.HttpContext.Session;
            var manager = new EyeglassOrderIt2Manager();

            var report = ReportFactory.CreateReportWithManualDispose<EgOrderSummaryReport>();
            manager.PrintEgOrderSummary(id, this.companyId, this.officeNumber, detail, report);
            if (httpSessionStateBase != null)
            {
                httpSessionStateBase[result] = report.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
            }

            ReportFactory.CloseAndDispose(report);
            return result;
        }

        /// <summary>The Adjust Logo Size.</summary>
        /// <param name="section">The section id.</param>
        /// <param name="companyId">The company Id.</param>
        /// <param name="officeNum">The office number.</param>
        /// <returns></returns>
        private void AdjustLogoSize(CrystalDecisions.CrystalReports.Engine.Section section, string companyId, string officeNum)
        {
            Office office = null;
            byte[] image;

            if (!string.IsNullOrEmpty(officeNum))
            {
                office = this.officeServices.GetByID_OrNull(officeNum);
            }

            try
            {
                if (office != null && office.OfficeLogo == true)
                {
                    if (office.PrintLogo != null)
                    {
                        image = office.PrintLogo;
                    }
                    else
                    {
                        image = office.Logo;
                    }
                }
                else
                {
                    var company = this.officeServices.GetCompanyInformationByID(companyId);
                    if (company.PrintLogo != null)
                    {
                        image = company.PrintLogo;
                    }
                    else
                    {
                        image = company.Logo;
                    }
                }

                var maxWidth = 168; ////1.75 in = 168 pixels
                var maxHeight = 96; ////1 in = 96 pixels
                var picture = System.Drawing.Image.FromStream(new MemoryStream(image));
                var ratioX = (double)maxWidth / picture.Width;
                var ratioY = (double)maxHeight / picture.Height;
                var ratio = Math.Min(ratioX, ratioY);
                var newWidth = (int)(picture.Width * ratio);
                var newHeight = (int)(picture.Height * ratio);
                var widthTwips = (newWidth / 96.0) * 1440.0;
                var heightTwips = (newHeight / 96.0) * 1440.0;
                section.ReportObjects["Logo1"].Width = Convert.ToInt32(widthTwips);
                section.ReportObjects["Logo1"].Height = Convert.ToInt32(heightTwips);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
        }
    }
}
#endregion
