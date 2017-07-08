// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatientReportsController.cs" company="Eyefinity, Inc.">
//  Eyefinity, Inc. - 2013  
// </copyright>
// <summary>
//   Defines the Patient Reports Controller type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Common;
    using Eyefinity.Enterprise.Business.Reports;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Legacy.Reports;
    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;
    using FeatureManager.Services;
    using IT2.Core.Feature;
    using IT2.Ioc;
    using IT2.Services;
    using Model.ViewModel;
    using NHibernate;

    using Lookup = Eyefinity.PracticeManagement.Model.Lookup;
    
    /// <summary>The patient reports controller.</summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientReportsController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>The office services.</summary>
        private readonly IOfficeServices officeServices;

        /// <summary>Feature manager services</summary>
        private readonly IFeatureManagerService featureManager;

        /// <summary>The get.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="report">The report.</param>
        /// <returns>The object.</returns>
        /// 
        public PatientReportsController()
        {
            this.officeServices = Container.Resolve<IOfficeServices>();
            this.featureManager = Container.Resolve<IFeatureManagerService>();
        }

        [HttpGet]
        public HttpResponseMessage Get(string officeNumber, string report)
        {
            try
            {
                AccessControl.VerifyUserAccessToOffice(officeNumber);
            }
            catch (Exception)
            {
                const string ValidationString = "You do not have security permission to access this area.<br/><br/> " +
                                                "Please contact your Office Manager or Office Administrator if you believe this is an error.";
                return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { validationmessage = ValidationString });
            }

            object vm = null;
            try
            {
                ////Report Auditing ALSL-5986
                var auditLog = new AuditLogs();
                auditLog.AddAuditLogEntryForReport($"EPM PatientReportsController API Get {report}");

                var companyId = OfficeHelper.GetCompanyId(officeNumber);
                switch (report)
                {
                    case "PatientWorkInProgress":
                        var dictProducts = new Dictionary<string, string> { { string.Empty, "All" }, { "E", "Eyeglass" }, { "S,H", "Contact Lens" } };
                        var productTypes = new List<Lookup>();
                        productTypes.AddRange(dictProducts.Select(entry => new Lookup(entry.Key, entry.Value)));
                        var patientReportsIt2Manager = new PatientReportsIt2Manager();
                        var orderStatus = patientReportsIt2Manager.GetOrderStatusListForWipReport();
                        vm = new PatientWorkInProgressVm
                                {
                                    OrderStatusList = orderStatus,
                                    ProductTypes = productTypes
                                };
                        break;

                    case "PatientAudit":
                        var dictNoteTypes = new Dictionary<int, string> { { 1, "Profile" }, { 3, "Appointments" }, { 6, "Exams" }, { 8, "Billing" }, { 101, "Audit" }, { 102, "User" }, { 103, "Patient Name Change" } };
                        var oh = new OfficeHelper();
                        var employees = oh.GetAllActiveEmployees(companyId);
                        var resource = new List<Lookup>();
                        resource.AddRange(employees.Select(entry => new Lookup(entry.ID, entry.FullName)).OrderBy(e => e.Description));
                        var noteTypes = new List<Lookup>();
                        noteTypes.AddRange(dictNoteTypes.Select(entry => new Lookup(entry.Key, entry.Value)));
                        vm = new PatientAuditVm
                        {
                            Resource = resource,
                            NoteTypes = noteTypes
                        };
                        break;

                    case "MarketingRecall":
                        var recallIt2Manager = new RecallReportsIt2Manager();
                        vm = recallIt2Manager.GetAllRecallsByCompany(companyId);
                        break;

                    case "InventoryValuation":
                        ////var dictItems = new Dictionary<string, string> { { "0", "All" }, { "1", "Frames" }, { "2", "Accessories" } };
                        var dictItems = new Dictionary<string, string> { { "0", "All" }, { "1", "Frames" } };
                        var itemTypes = new List<Lookup>();
                        itemTypes.AddRange(dictItems.Select(entry => new Lookup(entry.Key, entry.Value)));
                        vm = new InventoryValuationVm
                        {
                            ItemTypes = itemTypes
                        };
                        break;

                    case "PatientRemake":
                        var patientReportsIt2Manager2 = new PatientReportsIt2Manager();
                        vm = new PatientRemakeVm
                        {
                            RemakeReasonList = patientReportsIt2Manager2.GetPatientRemakeReasons(companyId)
                        };
                        break;

                    default:
                        Logger.Error("Unknown Patient Report Requested (companyId=" + companyId + ", report=" + report + ")");
                        break;
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, vm);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Get(officeNumber = {0}, {1}, {2}", officeNumber, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>The get.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="report">The report.</param>
        /// <returns>The object.</returns>
        [HttpGet]
        public HttpResponseMessage GetMarketingRecallLastExamTypes(string officeNumber)
        {
            var recallIt2Manager = new RecallReportsIt2Manager();
            try
            {
                var companyId = OfficeHelper.GetCompanyId(officeNumber);
                var examTypes = recallIt2Manager.GetMarketingRecallLastExamTypes(companyId);
                return this.Request.CreateResponse(HttpStatusCode.OK, examTypes);
            }
            catch (Exception ex)
            {
                var msg = $"GetMarketingRecallExamTypes(officeNum = {officeNumber}, {"\n"}, {ex}";
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>The delete marketing recall.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpDelete]
        public HttpResponseMessage DeleteMarketingRecall(int id)
        {
            var recallIt2Manager = new RecallReportsIt2Manager();
            try
            {
                var valid = recallIt2Manager.DeleteMarketingRecall(id);
                return valid
                    ? this.Request.CreateResponse(HttpStatusCode.OK, "Recall Deleted.")
                    : this.Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Unable to delete the recall.");
            }
            catch (Exception ex)
            {
                var msg = string.Format("DeleteMarketingRecall(id = {0}, {1}, {2}", id, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>The run marketing recall.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="marketingRecall">The marketing recall.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [HttpPost]
        public HttpResponseMessage Post(string officeNumber, [FromBody]MarketingRecall marketingRecall)
        {
            var recallIt2Manager = new RecallReportsIt2Manager();
            try
            {
                var isProcessing = recallIt2Manager.ValidateRecallReport(officeNumber, marketingRecall);
                if (isProcessing)
                {
                    const string ValidationString = "A report for this recall type and date range has already been generated. Select a different date range.";
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { validationmessage = ValidationString });
                }

                var recall = recallIt2Manager.RunMarketingRecallReport(officeNumber, marketingRecall);
                return this.Request.CreateResponse(HttpStatusCode.OK, recall);
            }
            catch (ObjectNotFoundException)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Post(id = {0}, {1}, {2}", marketingRecall.Id, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
         }

        /// <summary>The update marketing recall.</summary>
        /// <param name="marketingRecall">The marketing recall.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [HttpPut]
        public HttpResponseMessage Put([FromBody]MarketingRecall marketingRecall)
        {
            var recallIt2Manager = new RecallReportsIt2Manager();
            var user = new AuthorizationTicketHelper().GetUserInfo();
            var officeNumber = user.OfficeNum;

            try
            {
                var recall = recallIt2Manager.UpdateMarketingRecallReport(marketingRecall, officeNumber);
                return this.Request.CreateResponse(HttpStatusCode.OK, recall);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Put(id = {0}, {1}, {2}", marketingRecall.Id, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>The get report content.</summary>
        /// <param name="id">The id.</param>
        /// <param name="fileType">The file type.</param>
        /// <param name="dateRange">The date range.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="officeNumber">The office Number.</param>
        /// <param name="scheduledReport">The scheduled report.</param>
        /// <returns>The <see cref="MemoryStream"/>.</returns>
        [NonAction]
        public MemoryStream GetReportContent(int id, string fileType, string dateRange, string fullName, string officeNumber, ScheduledRecallReport scheduledReport)
        {
            MemoryStream mstream = new MemoryStream();
            try
            {
                var recallIt2Manager = new RecallReportsIt2Manager();
                mstream = recallIt2Manager.GetPatientMarketingRecallReport(id, fileType, dateRange, fullName, scheduledReport, officeNumber);
            }
            catch (Exception ex)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error(string.Format("GetReportContent failed {0} for Office {1} fullname {2} filetype {3} daterange {4} ", ex.Message, officeNumber, fullName, fileType, dateRange), ex);
                }

                mstream = null;
            }

            return mstream;
        }

        /// <summary>The get print labels content.</summary>
        /// <param name="id">The id.</param>
        /// <param name="officeNumber">The office Number.</param>
        /// <param name="pdfFile">The PDF file.</param>
        /// <returns>The <see cref="byte"/>.</returns>
        [NonAction]
        public byte[] GetPrintLabels(int id, string officeNumber, string pdfFile)
        {
            var recallIt2Manager = new RecallReportsIt2Manager();
            return recallIt2Manager.GetPrintLabelsReport(id, officeNumber, pdfFile);            
        }

        /// <summary>The get print labels content.</summary>
        /// <param name="recallId"></param>
        /// <param name="officeNumber">The office Number.</param>
        /// <param name="printFormat"></param>
        /// <returns>The <see cref="byte"/>.</returns>
        [NonAction]
        public byte[] GeneratePrintableDocument(int recallId, string officeNumber, int printFormat)
        {
            var recallReportsIt2Manager = new RecallReportsIt2Manager();
            return recallReportsIt2Manager.GeneratePrintableDocument(recallId, officeNumber, printFormat);            
        }
    }
}