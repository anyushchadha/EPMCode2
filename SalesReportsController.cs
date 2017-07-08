// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SalesReportsController.cs" company="Eyefinity, Inc.">
//  Eyefinity, Inc. - 2013  
// </copyright>
// <summary>
//   Defines the SalesReportsController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    using CrystalDecisions.CrystalReports.Engine;
    using CrystalDecisions.Shared;
    using Eyefinity.Enterprise.Business.Common;
    using Eyefinity.Enterprise.Business.Miscellaneous;
    using Eyefinity.Enterprise.Business.Reports;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Legacy.Reports;
    using Eyefinity.PracticeManagement.Model.Reports;

    using IT2.Core;
    using IT2.Core.POS;
    using IT2.Core.Reports;
    using IT2.Ioc;
    using IT2.Reports;
    using IT2.Reports.SalesAudit;
    using IT2.Services;

    using AccountsReceivableAgingReport = Eyefinity.PracticeManagement.Legacy.Reports.AccountsReceivableAgingReport;
    using FlashSalesByResourceReport = Eyefinity.PracticeManagement.Legacy.Reports.FlashReport;
    using RefundAdjustmentReport = Eyefinity.PracticeManagement.Legacy.Reports.RefundAdjustmentReport;

    /// <summary>
    /// The sales reports controller.
    /// </summary>
    [NoCache]
    public class SalesReportsController : Controller
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>The report services.</summary>
        private readonly IReportServices reportServices;

        /// <summary>The employee services.</summary>
        private readonly IEmployeeServices employeeServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="SalesReportsController"/> class.
        /// </summary>
        public SalesReportsController()
        {
            this.reportServices = Container.Resolve<IReportServices>();
            this.employeeServices = Container.Resolve<IEmployeeServices>();
        }

        /// <summary>The show generic report in new window.</summary>
        /// <param name="reportsCriteria">The reports criteria.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [HttpPost]
        public string ShowGenericReportInNewWin(SalesReportCriteria reportsCriteria)
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
            reportsCriteria.CompanyName = OfficeHelper.GetCompanyName(reportsCriteria.OfficeNum);
            var manager = new SalesReportsIt2Manager();

            ////Report Auditing ALSL-5986
            var auditLog = new AuditLogs();
            auditLog.AddAuditLogEntryForReport($"EPM {reportsCriteria.ReportName}");

            string pagebreak;
            switch (reportsCriteria.ReportName)
            {
                case "DailyFlashSalesReport":
                    IList<IT2.Core.Reports.FlashReport> lstReport;
                    var dateFrom = reportsCriteria.StartDate;
                    var dateTo = reportsCriteria.EndDate;
                    var report = ReportFactory.CreateReportWithManualDispose<FlashSalesByResourceReport>();
                    var employeeHasValue = false;
                    var dailyFlashSalesReportManager = new SalesReportsIt2Manager();
                    reportsCriteria.OfficeNum = dailyFlashSalesReportManager.GetSelectedOffices(reportsCriteria);
                    if (reportsCriteria.SelectedItemType == 0)
                    {
                        lstReport = this.reportServices.GetStoreFlashReports(dateFrom, dateTo, reportsCriteria.OfficeNum);
                        report.SetDataSource(lstReport);
                        report.SetParameterValue("Employee", string.Empty);
                    }
                    else
                    {
                        var fullName = string.Empty;
                        var employee = this.employeeServices.GetByUserID(reportsCriteria.SelectedItemType);
                        foreach (var e in reportsCriteria.ItemTypes.Where(e => e.Key == reportsCriteria.SelectedItemType))
                        {
                            fullName = e.Description;
                        }

                        lstReport = this.reportServices.GetEmployeeFlashReports(dateFrom, dateTo, reportsCriteria.OfficeNum, employee.ID);
                        report.SetDataSource(lstReport);
                        report.SetParameterValue("Employee", fullName);
                        employeeHasValue = true;
                    }

                    if (lstReport.Count == 0)
                    {
                        result = "noData";
                    }
                    else
                    {
                        if (reportsCriteria.SelectedOffices.Count > 2)
                        {
                            reportsCriteria.OfficeNum = this.GetStoreName(reportsCriteria);
                            report.SetParameterValue("Store", reportsCriteria.OfficeNum);
                        }
                        else
                        {
                            report.SetParameterValue("Store", reportsCriteria.OfficeNum.Replace("'", string.Empty));
                        }

                        report.SetParameterValue("FromDate", dateFrom);
                        report.SetParameterValue("ToDate", dateTo);
                        report.SetParameterValue("IsEmployee", employeeHasValue);
                        if (httpSessionStateBase != null)
                        {
                            httpSessionStateBase[result] = report.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                        }
                    }

                    ReportFactory.CloseAndDispose(report);
                    break;

                case "CashReceiptSummaryReport":
                    var cashReceiptReportmanager = new SalesReportsIt2Manager();
                    var cashReportData = cashReceiptReportmanager.GetCashReceiptReportDataEpm(reportsCriteria);
                    if (cashReportData.Count == 0)
                    {
                        result = "noData";
                    }
                    else
                    {
                        var cashReceiptReport = ReportFactory.CreateReportWithManualDispose<Eyefinity.PracticeManagement.Legacy.Reports.CashReceiptSummaryReport>();
                        cashReceiptReport.SetDataSource(cashReportData);
                        var reportDateParameter = reportsCriteria.StartDate.ToShortDateString() + " - " + reportsCriteria.EndDate.ToShortDateString();
                        cashReceiptReport.SetParameterValue("DateRange", reportDateParameter);

                        if (httpSessionStateBase != null)
                        {
                            httpSessionStateBase[result] = cashReceiptReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                        }

                        ReportFactory.CloseAndDispose(cashReceiptReport);
                    }

                    break;

                case "DailyTransactionPaymentReport":
                    try
                    {
                        var dailyTransactionPaymentReportManager = new SalesReportsIt2Manager();
                        var paymentDetailReportList = dailyTransactionPaymentReportManager.GetDailyTransactionPaymentForDateAndOffice(reportsCriteria);

                        if (paymentDetailReportList.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            var dlyPaymentrep = ReportFactory.CreateReportWithManualDispose<Legacy.Reports.DailyPaymentReport>();
                            dlyPaymentrep.SetDataSource(paymentDetailReportList);
                            dlyPaymentrep.SetParameterValue(
                                "DateParameter",
                                reportsCriteria.StartDate.ToString("MM/dd/yy"));
                            dlyPaymentrep.SetParameterValue("ReportID", "POS004");

                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = dlyPaymentrep.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(dlyPaymentrep);
                        }
                    }
                    catch (Exception ex)
                    {
                        ////No row with the given identifier exists
                        result = "noData";
                        Logger.Error("Daily Transaction Payment Report\n" + ex.Message);
                        return result;
                    }

                    break;

                case "DailyTransactionSalesReport":
                    IList<TransactionReport> reportList;
                    var dailyTransactionSalesReportManager = new SalesReportsIt2Manager();
                    try
                    {
                        reportsCriteria.OfficeNum = dailyTransactionSalesReportManager.GetSelectedOffices(reportsCriteria);
                        reportList = this.reportServices.GetDailyTransactionReportForDateRangeAndOffice(reportsCriteria.StartDate, reportsCriteria.StartDate, reportsCriteria.OfficeNum);
                    }
                    catch (Exception ex)
                    {
                        ////No row with the given identifier exists
                        result = "noData";
                        Logger.Error("Daily Transaction Sales Report\n" + ex.Message);
                        return result;
                    }

                    if (reportList.Count == 0)
                    {
                        result = "noData";
                    }
                    else
                    {
                        var dtr = ReportFactory.CreateReportWithManualDispose<Legacy.Reports.DailyTransactionReport>();
                        double totalretail = 0;
                        double totalInsAllow = 0;
                        double totalDiscount = 0;
                        double totalCopay = 0;
                        double totalAmount = 0;
                        double totalInsAr = 0;
                        double totalTax = 0;
                        double totalNet = 0;
                        double totalPayment = 0;
                        double totalCredit = 0;
                        double totalNetAr = 0;
                        IList<OrderDetail> orderDetailsSource = new List<OrderDetail>();
                        foreach (var tReport in reportList)
                        {
                            tReport.NetAR = (tReport.NetSaleTotal + tReport.TaxTotal + tReport.Payment - tReport.ReceivableTotal);
                            totalPayment += tReport.Payment;
                            totalCredit += tReport.CustomerCredit;
                            totalNetAr += tReport.NetAR;
                            if (tReport.TransTypeEnum != TransactionType.OldSystemBalance)
                            {
                                totalNet += tReport.NetSaleTotal;
                                totalretail += tReport.RetailTotal;
                            }

                            if (tReport.TransTypeEnum != TransactionType.Sale
                                && tReport.TransTypeEnum != TransactionType.Return
                                && tReport.TransTypeEnum != TransactionType.Void
                                && tReport.TransTypeEnum != TransactionType.MerchandiseDrop
                                && tReport.TransTypeEnum != TransactionType.Remake
                                && tReport.TransTypeEnum != TransactionType.ReinstateDrop
                                && tReport.TransTypeEnum != TransactionType.Adjustment
                                && tReport.TransTypeEnum != TransactionType.OutOfStoreReturn)
                            {
                                continue;
                            }

                            if (tReport.OrderDetails.Count == 0)
                            {
                                var ord = new OrderDetail
                                {
                                    TransactionNum = tReport.TransactionNum,
                                    Retail = tReport.Payment
                                };
                                totalretail += tReport.Payment;
                                orderDetailsSource.Add(ord);
                            }

                            foreach (var detail in tReport.OrderDetails)
                            {
                                orderDetailsSource.Add(detail);
                                totalDiscount += detail.Discount;
                                totalInsAllow += detail.Allowance;
                                totalCopay += detail.Copay;
                                totalAmount += detail.CustomerAmount;
                                totalInsAr += detail.Receivable;
                                totalTax += detail.Tax;
                            }
                        }

                        dtr.SetDataSource(reportList);

                        var subReport = dtr.OpenSubreport(dtr.Subreports[0].Name);

                        // add a dummy order record if there is no record
                        if (orderDetailsSource.Count <= 0)
                        {
                            orderDetailsSource.Add(new OrderDetail());
                        }

                        if (subReport != null)
                        {
                            subReport.SetDataSource(orderDetailsSource);
                        }

                        dtr.SetParameterValue("FromDate", reportsCriteria.StartDate.ToString("MM/dd/yy"));
                        dtr.SetParameterValue("ToDate", reportsCriteria.StartDate.ToString("MM/dd/yy"));
                        if (reportsCriteria.SelectedOffices.Count > 2)
                        {
                            reportsCriteria.OfficeNum = this.GetStoreName(reportsCriteria);
                            dtr.SetParameterValue("Store", reportsCriteria.OfficeNum);
                        }
                        else
                        {
                            dtr.SetParameterValue("Store", reportsCriteria.OfficeNum.Replace("'", string.Empty));
                        }

                        dtr.SetParameterValue("RetailTotal", totalretail);
                        dtr.SetParameterValue("discTotal", totalDiscount);
                        dtr.SetParameterValue("InsAllowTotal", totalInsAllow);
                        dtr.SetParameterValue("CopayTotal", totalCopay);
                        dtr.SetParameterValue("CustAmountTotal", totalAmount);
                        dtr.SetParameterValue("InsARTotal", totalInsAr);
                        dtr.SetParameterValue("NetSaleTotal", totalNet);
                        dtr.SetParameterValue("SaleTaxTotal", totalTax);
                        dtr.SetParameterValue("PaymentTotal", totalPayment);
                        dtr.SetParameterValue("CustCredtTotal", totalCredit);
                        dtr.SetParameterValue("NetARTotal", totalNetAr);

                        if (httpSessionStateBase != null)
                        {
                            httpSessionStateBase[result] = dtr.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                        }

                        ReportFactory.CloseAndDispose(dtr);
                    }

                    break;

                case "ItemSalesReport":
                    IList<FrameAccessories> frameReportList;
                    var dateParameter = reportsCriteria.StartDate.ToShortDateString() + " - " + reportsCriteria.EndDate.ToShortDateString();
                    var salesReportsIt2Manager = new SalesReportsIt2Manager();
                    try
                    {
                        reportsCriteria.OfficeNum = salesReportsIt2Manager.GetSelectedOffices(reportsCriteria);
                        frameReportList = this.reportServices.GetFrameAccessoriesbyCriteria(reportsCriteria.StartDate, reportsCriteria.EndDate, reportsCriteria.SelectedItemStatus.ToString(CultureInfo.InvariantCulture), reportsCriteria.SelectedItemType, reportsCriteria.OfficeNum, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        ////No row with the given identifier exists
                        result = "noData";
                        Logger.Error("Item Sales Report\n" + ex.Message);
                        return result;
                    }

                    if (frameReportList.Count == 0)
                    {
                        result = "noData";
                    }
                    else
                    {
                        foreach (var frameaccess in frameReportList)
                        {
                            switch (frameaccess.ItemStatusID)
                            {
                                case 196:
                                    frameaccess.ItemStatusName = "Active";
                                    break;
                                case 197:
                                    frameaccess.ItemStatusName = "Keep";
                                    break;
                                default:
                                    frameaccess.ItemStatusName = "None";
                                    break;
                            }
                        }

                        var itemStatus = reportsCriteria.ItemStatus.Find(x => x.Key == reportsCriteria.SelectedItemStatus);
                        var itemStatusDescription = string.Empty;
                        if (itemStatus != null)
                        {
                            itemStatusDescription = itemStatus.Description;
                        }

                        var itemTypeDescription = string.Empty;
                        var itemType = reportsCriteria.ItemTypes.Find(x => x.Key == reportsCriteria.SelectedItemType);
                        if (itemType != null)
                        {
                            itemTypeDescription = itemType.Description;
                        }

                        var frameaccessoriesreport = ReportFactory.CreateReportWithManualDispose<FrameAccessoriesReport>();
                        frameaccessoriesreport.SetDataSource(frameReportList);
                        frameaccessoriesreport.SetParameterValue("ItemType", itemTypeDescription);
                        frameaccessoriesreport.SetParameterValue("DateRange", dateParameter);
                        if (reportsCriteria.SelectedOffices.Count > 2)
                        {
                            reportsCriteria.OfficeNum = this.GetStoreName(reportsCriteria);
                            frameaccessoriesreport.SetParameterValue("OfficeList", reportsCriteria.OfficeNum);
                        }
                        else
                        {
                            frameaccessoriesreport.SetParameterValue("OfficeList", reportsCriteria.OfficeNum.Replace("'", string.Empty));
                        }
                        
                        frameaccessoriesreport.SetParameterValue("ItemStatus", itemStatusDescription);
                        frameaccessoriesreport.SetParameterValue("ItemNum", string.Empty);
                        frameaccessoriesreport.SetParameterValue("ReportID", "MN101");

                        if (httpSessionStateBase != null)
                        {
                            httpSessionStateBase[result] = frameaccessoriesreport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                        }

                        ReportFactory.CloseAndDispose(frameaccessoriesreport);
                    }

                    break;

                case "OfficeFlashSalesReport":
                    var officeFlashSalesIt2Manager = new SalesReportsIt2Manager();
                    reportsCriteria.OfficeNum = officeFlashSalesIt2Manager.GetSelectedOffices(reportsCriteria);
                    var flashReportList = this.reportServices.GetStoreFlashReports(reportsCriteria.StartDate, reportsCriteria.EndDate, reportsCriteria.OfficeNum);
                    if (flashReportList.Count == 0)
                    {
                        result = "noData";
                    }
                    else
                    {
                        var flashReport = ReportFactory.CreateReportWithManualDispose<IT2.Reports.FlashReport>();
                        flashReport.SetDataSource(flashReportList);
                        flashReport.SetParameterValue("Employee", string.Empty);
                        if (reportsCriteria.SelectedOffices.Count > 2)
                        {
                            reportsCriteria.OfficeNum = this.GetStoreName(reportsCriteria);
                            flashReport.SetParameterValue("Store", reportsCriteria.OfficeNum);
                        }
                        else
                        {
                            flashReport.SetParameterValue("Store", reportsCriteria.OfficeNum.Replace("'", string.Empty));
                        }
                       
                        flashReport.SetParameterValue("FromDate", reportsCriteria.StartDate);
                        flashReport.SetParameterValue("ToDate", reportsCriteria.EndDate);
                        flashReport.SetParameterValue("IsEmployee", false);
                        flashReport.SetParameterValue("ReportID", "POS003");

                        if (httpSessionStateBase != null)
                        {
                            httpSessionStateBase[result] = flashReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                        }

                        ReportFactory.CloseAndDispose(flashReport);
                    }

                    break;

                case "RefundAdjustmentReport":
                    IList<RefundAdjustment> reportData;
                    try
                    {
                        reportData = manager.GetRefundAdjustmentsReportData(reportsCriteria);
                    }
                    catch (Exception ex)
                    {
                        ////No row with the given identifier exists
                        result = "noData";
                        Logger.Error("Refund Adjustment Report\n" + ex.Message);
                        return result;
                    }

                    if (reportData.Count == 0)
                    {
                        result = "noData";
                    }
                    else
                    {
                        var refunds = ReportFactory.CreateReportWithManualDispose<RefundAdjustmentReport>();
                        refunds.SetDataSource(reportData);
                        var reportDateParameter = reportsCriteria.StartDate.ToShortDateString() + " - " + reportsCriteria.EndDate.ToShortDateString();
                        refunds.SetParameterValue("DateParameter", reportDateParameter);
                        refunds.SetParameterValue("ReportID", "POS111");

                        if (httpSessionStateBase != null)
                        {
                            httpSessionStateBase[result] = refunds.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                        }

                        ReportFactory.CloseAndDispose(refunds);
                    }

                    break;

                case "DiscountAnalysis":
                    var bll = new SalesReportsIt2Manager();
                    IList<DiscountAnalysisDetailReport> data = new List<DiscountAnalysisDetailReport>();
                    try
                    {
                        data = bll.GetDiscountAnalysisReport(reportsCriteria, this.reportServices).ToList();
                    }
                    catch (Exception)
                    {
                        result = "timeout";
                    }

                    if (data.Count == 0)
                    {
                        if (result != "timeout")
                        {
                            result = "noData";
                        }
                    }
                    else
                    {
                        var discounts = ReportFactory.CreateReportWithManualDispose<Legacy.Reports.DiscountAnalysisDetailDisplay>();
                        discounts.SetDataSource(data);
                        var pagebreakneeded = reportsCriteria.CheckboxOne ? "1" : "0";
                        discounts.SetParameterValue("PageHeader", pagebreakneeded);
                        var reportDateParameter = reportsCriteria.StartDate.ToShortDateString() + " - " + reportsCriteria.EndDate.ToShortDateString();
                        discounts.SetParameterValue("DateRange", reportDateParameter);
                        if (httpSessionStateBase != null)
                        {
                            httpSessionStateBase[result] = discounts.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                        }

                        ReportFactory.CloseAndDispose(discounts);
                    }

                    break;

                case "ProductionByProvider":
                    var productionReport = ReportFactory.CreateReportWithManualDispose<ProductionByProviderReport>();
                    pagebreak = reportsCriteria.CheckboxOne ? "1" : "0";

                    try
                    {
                        var productionData = manager.GetProductionReport(reportsCriteria);
                        if (productionData != null && productionData.Count > 0)
                        {
                            productionReport.SetDataSource(productionData);
                            productionReport.SetParameterValue("Daterange", reportsCriteria.StartDate.ToShortDateString() + " - " + reportsCriteria.EndDate.ToShortDateString());
                            productionReport.SetParameterValue("PageBreak", pagebreak);
                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = productionReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(productionReport);
                        }
                        else
                        {
                            result = "noData";
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Production Report By Provider\n" + ex);
                    }

                    break;

                case "MonthlyProductionByProvider":
                    var monthlyProductionByProviderReport = ReportFactory.CreateReportWithManualDispose<Legacy.Reports.MonthlyProductionByProviderReport>();
                    pagebreak = "1";

                    try
                    {
                        manager.GetProviders(reportsCriteria);
                        var prlist = manager.GetMonthlyProductionReport(reportsCriteria);

                        if (prlist != null && prlist.Count > 0)
                        {
                            var depositSummaryReport = monthlyProductionByProviderReport.OpenSubreport(monthlyProductionByProviderReport.Subreports[0].Name);
                            var crlist = this.GetDepositSummary(reportsCriteria);
                            var monthlyProductionSubReport = monthlyProductionByProviderReport.OpenSubreport(monthlyProductionByProviderReport.Subreports[1].Name);
                            var summarylist = manager.GetMonthlyProductionSubReportByProvider(reportsCriteria);
                            var companySummary = manager.GetMonthlyProductionCompanyTotal(summarylist);
                            var drlist = reportsCriteria.Resources.Count();
                            var drnames = string.Empty;
                            for (var i = 0; i < drlist; i++)
                            {
                                drnames += summarylist[i].DrName + ',';
                            }

                            depositSummaryReport.SetDataSource(crlist);
                            monthlyProductionSubReport.SetDataSource(summarylist);
                            monthlyProductionByProviderReport.SetDataSource(prlist);
                            monthlyProductionByProviderReport.SetParameterValue("Daterange", reportsCriteria.StartDate.ToShortDateString() + " to " + reportsCriteria.EndDate.ToShortDateString());
                            monthlyProductionByProviderReport.SetParameterValue("PageBreak", pagebreak);
                            monthlyProductionByProviderReport.SetParameterValue("Store", reportsCriteria.CompanyName);
                            monthlyProductionByProviderReport.SetParameterValue("PatientAdjustment", companySummary[0].PatientAdjustment);
                            monthlyProductionByProviderReport.SetParameterValue("PatientPayment", companySummary[0].PatientPayment);
                            monthlyProductionByProviderReport.SetParameterValue("InsuranceAdjustment", companySummary[0].InsuranceAdjustment);
                            monthlyProductionByProviderReport.SetParameterValue("InsurancePayment", companySummary[0].InsurancePayment);
                            monthlyProductionByProviderReport.SetParameterValue("ProviderList", "[ " + drnames + "... ]");

                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = monthlyProductionByProviderReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(monthlyProductionByProviderReport);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Monthly Production Report By Provider\n" + ex);
                    }

                    break;

                case "MonthlyProductionSummary":
                    var monthlyProductionSummary = ReportFactory.CreateReportWithManualDispose<MonthlyProductionSummaryReport>();
                    pagebreak = "0";

                    try
                    {
                        manager.GetProviders(reportsCriteria);
                        var prlist = manager.GetMonthlyProductionReport(reportsCriteria);
                        if (prlist != null && prlist.Count > 0)
                        {
                            var depositSummaryReport = monthlyProductionSummary.OpenSubreport(monthlyProductionSummary.Subreports[0].Name);
                            var crlist = this.GetDepositSummary(reportsCriteria);
                            var monthlyProductionSummaryReport = monthlyProductionSummary.OpenSubreport(monthlyProductionSummary.Subreports[1].Name);
                            var summarylist = manager.GetMonthlyProductionSubReportByProvider(reportsCriteria);
                            var companySummary = manager.GetMonthlyProductionCompanyTotal(summarylist);
                            var drlist = reportsCriteria.Resources.Count();
                            var drnames = string.Empty;
                            for (var i = 0; i < drlist; i++)
                            {
                                drnames += summarylist[i].DrName + ',';
                            }

                            depositSummaryReport.SetDataSource(crlist);
                            monthlyProductionSummaryReport.SetDataSource(companySummary);
                            monthlyProductionSummary.SetDataSource(prlist);
                            monthlyProductionSummary.SetParameterValue("Daterange", reportsCriteria.StartDate.ToShortDateString() + " to " + reportsCriteria.EndDate.ToShortDateString());
                            monthlyProductionSummary.SetParameterValue("PageBreak", pagebreak);
                            monthlyProductionSummary.SetParameterValue("Store", reportsCriteria.CompanyName);
                            monthlyProductionSummary.SetParameterValue("ProviderList", "[ " + drnames + "... ]");
                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = monthlyProductionSummary.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(monthlyProductionSummary);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Monthly Production Summary Report\n" + ex);
                    }

                    break;

                case "PatientAccountsReceivableAging":
                    try
                    {
                        var accountsAgingData = manager.GetPatientAccountsReceivableAgingData(reportsCriteria);
                        if (accountsAgingData != null && accountsAgingData.Count > 0)
                        {
                            var accountsAgingReport = this.GetPatientAccountsReceivableAging(reportsCriteria, accountsAgingData);
                            if (accountsAgingReport != null)
                            {
                                if (httpSessionStateBase != null)
                                {
                                    httpSessionStateBase[result] = accountsAgingReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                                }

                                ReportFactory.CloseAndDispose(accountsAgingReport);
                            }
                            else
                            {
                                result = "noData";
                            }
                        }
                        else
                        {
                            result = "noData";
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Total Patient Account Receivable Aging Report\n" + ex);
                    }

                    break;

                case "CostOfSalesAnalysisByOffice":
                    IList<COSAnalysis> cosTransactionData;
                    try
                    {
                        cosTransactionData = manager.GetCostOfSalesAnalysisByOfficeData(reportsCriteria);
                    }
                    catch (Exception ex)
                    {
                        ////No row with the given identifier exists
                        result = "noData";
                        Logger.Error("Cost of Sale Analysis by Office Report\n" + ex);
                        return result;
                    }

                    if (cosTransactionData != null && cosTransactionData.Count > 0)
                    {
                        var cosTransactionReport = ReportFactory.CreateReportWithManualDispose<Legacy.Reports.COSAnalysisOfficeReport>();
                        cosTransactionReport.SetDataSource(cosTransactionData);
                        var reportDateParameter = reportsCriteria.StartDate.ToShortDateString() + " - " + reportsCriteria.EndDate.ToShortDateString();
                        cosTransactionReport.SetParameterValue("DateRange", reportDateParameter);
                        cosTransactionReport.SetParameterValue("ItemType", reportsCriteria.SelectedItemTypeName);
                        if (httpSessionStateBase != null)
                        {
                            httpSessionStateBase[result] = cosTransactionReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                        }

                        ReportFactory.CloseAndDispose(cosTransactionReport);
                    }
                    else
                    {
                        result = "noData";
                    }

                    break;

                case "MiscPayment":
                    try
                    {
                        var miscPaymentReportmanager = new SalesReportsIt2Manager();
                        var miscPaymentData = miscPaymentReportmanager.GetMiscPaymentDetailData(reportsCriteria);
                        if (miscPaymentData != null && !miscPaymentData.Any())
                        {
                            result = "noData";
                        }
                        else
                        {
                            var miscPaymentDetailReport = ReportFactory.CreateReportWithManualDispose<MiscPaymentDetailReport>();
                            miscPaymentDetailReport.SetDataSource(miscPaymentData);
                            miscPaymentDetailReport.SetParameterValue("ReportID", "AC122");
                            var reportDateParameter = reportsCriteria.StartDate.ToShortDateString() + " - " + reportsCriteria.EndDate.ToShortDateString();
                            miscPaymentDetailReport.SetParameterValue("DateParameter", reportDateParameter);

                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = miscPaymentDetailReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(miscPaymentDetailReport);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Misc. Payment Report\n" + ex);
                    }

                    break;

                case "TitledSalesAdjustment":
                    try
                    {
                        var salesAdjustmentReportmanager = new SalesReportsIt2Manager();
                        reportsCriteria.OfficeNum = salesAdjustmentReportmanager.GetSelectedOffices(reportsCriteria);
                        var salesAdjustmentData = salesAdjustmentReportmanager.GetSalesAdjustmentData(reportsCriteria);
                        if (salesAdjustmentData.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            var miscPaymentDetailReport = ReportFactory.CreateReportWithManualDispose<SalesAdjustmensByReasonReport>();
                            miscPaymentDetailReport.SetDataSource(salesAdjustmentData);
                            var reportDateParameter = reportsCriteria.StartDate.ToShortDateString() + " - " + reportsCriteria.EndDate.ToShortDateString();
                            miscPaymentDetailReport.SetParameterValue("DateRange", reportDateParameter);

                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = miscPaymentDetailReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(miscPaymentDetailReport);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Sales Adjustments Report\n" + ex);
                    }

                    break;

                case "PatientCreditBalance":
                    try
                    {
                        var patientCreditBalanceManager = new SalesReportsIt2Manager();
                        var creditBalanceData = patientCreditBalanceManager.GetPatientCreditBalanceData(reportsCriteria);
                        if (creditBalanceData.Count == 0)
                        {
                            result = "noData";
                        }
                        else
                        {
                            var miscPaymentDetailReport = ReportFactory.CreateReportWithManualDispose<OutstandingCreditBalanceAllOfficesReport>();
                            miscPaymentDetailReport.SetDataSource(creditBalanceData);
                            miscPaymentDetailReport.SetParameterValue("DateParameter", reportsCriteria.StartDate.ToShortDateString());
                            if (httpSessionStateBase != null)
                            {
                                httpSessionStateBase[result] = miscPaymentDetailReport.ExportToStream(ExportFormatType.CrystalReport).ToBase64String();
                            }

                            ReportFactory.CloseAndDispose(miscPaymentDetailReport);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Patient Credit Balances Report\n" + ex);
                    }

                    break;

                default:
                    result = "noData";
                    break;
            }

            return result;
        }

        public string GetStoreName(SalesReportCriteria reportsCriteria)
        {
            List<string> office = new List<string>();
            string storeName = string.Empty;
            if (!string.IsNullOrEmpty(reportsCriteria.OfficeNum))
            {
                office = reportsCriteria.OfficeNum.Split(',').ToList();
                for (int count = 0; count < 2; count++)
                {
                    if (count == 0)
                    {
                    storeName += office[count].Replace("'", string.Empty) + ",";
                    }
                    else
                    {
                        storeName += office[count].Replace("'", string.Empty);
                    }
                }

                storeName = storeName + " " + "and" + " " + (reportsCriteria.SelectedOffices.Count - 2) + " more";
                return storeName;
            }

            return storeName;
        }

        #region private functions

        private List<Model.CashReceiptSummary> GetDepositSummary(SalesReportCriteria reportsCriteria)
        {
            var depositSummaryReportmanager = new SalesReportsIt2Manager();
            var cr = new Model.CashReceiptSummary();
            var crlist = new List<Model.CashReceiptSummary>();
            var depositsReportData = depositSummaryReportmanager.GetCashReceiptReportDataEpm(reportsCriteria);

            if (depositsReportData.Count > 0)
            {
                foreach (var item in depositsReportData)
                {
                    cr.Amex += item.Amex;
                    cr.CashActual += item.CashActual;
                    cr.CashBankActual += item.CashBankActual;
                    cr.CashComputed += item.CashComputed;
                    cr.CheckActual += item.CheckActual;
                    cr.CheckBankActual += item.CheckBankActual;
                    cr.CheckComputed += item.CheckComputed;
                    cr.Discover += item.Discover;
                    cr.OtherPayments += item.OtherPayments;
                    cr.VisaMC += item.VisaMC;
                    cr.CompanyID = item.CompanyID;
                    cr.InsuranceChecks += item.InsuranceChecks;
                    cr.InsuranceEFT += item.InsuranceEFT;
                }
            }
            else
            {
                cr.Amex = 0;
                cr.CashActual = 0;
                cr.CashBankActual = 0;
                cr.CashComputed = 0;
                cr.CheckActual = 0;
                cr.CheckBankActual = 0;
                cr.CheckComputed = 0;
                cr.Discover = 0;
                cr.OtherPayments = 0;
                cr.VisaMC = 0;
                cr.CompanyID = reportsCriteria.CompanyId;
                cr.InsuranceChecks = 0;
                cr.InsuranceEFT = 0;
            }

            crlist.Add(cr);
            return crlist;
        }

        private IT2.Reports.AccountsReceivableAgingReport GetPatientAccountsReceivableAging(SalesReportCriteria reportsCriteria, IList<AccountsReceivable> agingSummary)
        {
            var agingSummaryLocal = agingSummary;
            var accountsAgingReport = ReportFactory.CreateReportWithManualDispose<IT2.Reports.AccountsReceivableAgingReport>();

            DateTime? from = null;
            DateTime? to = null;
            DateTime cutoff = reportsCriteria.StartDate;
            string totalBucket1 = string.Empty;
            string totalBucket2 = string.Empty;
            string totalBucket3 = string.Empty;
            string totalBucket4 = string.Empty;
            string totalBucket5 = string.Empty;
            string totalBucket6 = string.Empty;

            switch (reportsCriteria.SelectedReceivableAge)
            {
                case 0:
                    break;
                case 1:
                    from = cutoff.AddDays(-30);
                    to = cutoff;
                    break;
                case 2:
                    from = cutoff.AddDays(-45);
                    to = cutoff.AddDays(-31);
                    break;
                case 3:
                    from = cutoff.AddDays(-60);
                    to = cutoff.AddDays(-46);
                    break;
                case 4:
                    from = cutoff.AddDays(-90);
                    to = cutoff.AddDays(-61);
                    break;
                case 5:
                    from = cutoff.AddDays(-120);
                    to = cutoff.AddDays(-91);
                    break;
                case 6:
                    from = new DateTime(1753, 1, 1);
                    to = cutoff.AddDays(-121);
                    break;
                default:
                    return null;
            }

            if (!from.HasValue)
            {
                // get the total breakdowns for the time ranges
                double total1 = 0;
                double total2 = 0;
                double total3 = 0;
                double total4 = 0;
                double total5 = 0;
                double total6 = 0;

                foreach (AccountsReceivable ar in agingSummaryLocal)
                {
                    DateTime date = ar.OrderDate.Date;

                    if (date >= cutoff.AddDays(-30))
                    {
                        total1 += ar.Balance;
                    }
                    else if (date < cutoff.AddDays(-30) && date >= cutoff.AddDays(-45))
                    {
                        total2 += ar.Balance;
                    }
                    else if (date < cutoff.AddDays(-45) && date >= cutoff.AddDays(-60))
                    {
                        total3 += ar.Balance;
                    }
                    else if (date < cutoff.AddDays(-60) && date >= cutoff.AddDays(-90))
                    {
                        total4 += ar.Balance;
                    }
                    else if (date < cutoff.AddDays(-90) && date >= cutoff.AddDays(-120))
                    {
                        total5 += ar.Balance;
                    }
                    else if (date < cutoff.AddDays(-120))
                    {
                        total6 += ar.Balance;
                    }
                }

                totalBucket1 = total1.ToString("$0.00");
                totalBucket2 = total2.ToString("$0.00");
                totalBucket3 = total3.ToString("$0.00");
                totalBucket4 = total4.ToString("$0.00");
                totalBucket5 = total5.ToString("$0.00");
                totalBucket6 = total6.ToString("$0.00");
            }
            else
            {
                ////Filter the list based on From and To dates.
                agingSummaryLocal = agingSummaryLocal.Where(a => a.OrderDate >= from.Value && a.OrderDate < to.Value.AddDays(1).Date).ToList();
            }

            var reportTitle = reportsCriteria.SelectedReceivableAge == 0 ? "Total" : reportsCriteria.ReceivableAgings[reportsCriteria.SelectedReceivableAge].Description;
            accountsAgingReport.SetDataSource(agingSummaryLocal);
            if (reportsCriteria.SelectedOffices.Count > 2)
            {
                reportsCriteria.OfficeNum = this.GetStoreName(reportsCriteria);
                accountsAgingReport.SetParameterValue("Store", reportsCriteria.OfficeNum);
            }
            else
            {
                accountsAgingReport.SetParameterValue("Store", reportsCriteria.OfficeNum.Replace("'", string.Empty));
            }
           
            accountsAgingReport.SetParameterValue("IsTotal", reportsCriteria.SelectedReceivableAge == 0);
            accountsAgingReport.SetParameterValue("0-30Total", totalBucket1);
            accountsAgingReport.SetParameterValue("31-45Total", totalBucket2);
            accountsAgingReport.SetParameterValue("46-60Total", totalBucket3);
            accountsAgingReport.SetParameterValue("61-90Total", totalBucket4);
            accountsAgingReport.SetParameterValue("91-120Total", totalBucket5);
            accountsAgingReport.SetParameterValue("120+Total", totalBucket6);
            accountsAgingReport.SetParameterValue("Range", reportTitle);
            accountsAgingReport.SetParameterValue("ReportID", "AC128");
            accountsAgingReport.SetParameterValue("CutOffDate", cutoff);

            return accountsAgingReport;
        }

        #endregion ////private functions
    }
}
