// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BundleConfig.cs" company="Eyefinity, Inc.">
//    Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//  The register admin bundles.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.App_Start
{
    using System.Web.Optimization;

    /// <summary>
    ///     The bundle config.
    /// </summary>
    public static class BundleConfig
    {
        #region Public Methods and Operators

        /// <summary>
        ///     The register bundles.
        /// </summary>
        /// <param name="bundles">
        ///     The bundles.
        /// </param>
        public static void RegisterScriptBundles(BundleCollection bundles)
        {
            bundles.IgnoreList.Clear();
#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif

            ////Register Style Bundles for all Sites
            RegisterStyleBundles(bundles);

            ////Register Javascript Bundles for Admin Site
            RegisterAdminScriptBundles(bundles);

            ////Register Javascript Bundles for Billing Site
            RegisterBillingScriptBundles(bundles);

            ////Modernizer Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-{version}.js"));

            ////Jquery Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));

            ////Jquery UI Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include("~/Scripts/jquery-ui-{version}.js"));

            ////Jquery Notify Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/jquerynotify").Include("~/Scripts/jquery.notify-{version}.js"));

            ////Jquery Validate Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/jqueryvalidate").Include("~/Scripts/jquery.validate.js"));

            ////Jquery DataTables Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/jquery.datatables").Include("~/Scripts/jquery.datatables.js"));

            /////Bootstrap & Respond Javascript Bundle
            bundles.Add(
                new ScriptBundle("~/bundles/bootstrap").Include(
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select-{version}.js",
                    "~/Scripts/respond.js"));

            ////Sugar & Knockout Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/knockout").Include("~/Scripts/sugar.js", "~/Scripts/knockout-{version}.js"));

            ////Login Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/login").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Login/aes.js",
                    "~/Scripts/app/Login/Login.js"));

            ////Reset Password Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/resetpwd").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Login/aes.js",
                    "~/Scripts/app/Login/resetpwd.js"));

            ////Change PaymentType Javascript Bundle            
            bundles.Add(
                new ScriptBundle("~/bundles/changePaymentType").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Management/ChangePaymentType.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            /////Session Timeout Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/sessiontimeout").Include(
                    "~/Scripts/app/Common/jquery.idletimer.js",
                    "~/Scripts/app/Common/jquery.idletimeout.js",
                    "~/Scripts/app/Common/SessionTimeout.js"));

            //// Patient Search Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/commonbase").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            bundles.Add(new ScriptBundle("~/bundles/admincommonbase").Include(
                   "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/jquery.multiSelect-{version}.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.multiselect.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/common/footer.js"));

            //// Front Office Home Javascript Bundle
            bundles.Add(
                new ScriptBundle("~/bundles/frontOfficeHome").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js",
                    "~/Scripts/app/Home/Index.js"));

            //// Patient Search Javascript Bundle
            bundles.Add(new ScriptBundle("~/bundles/patientSearch").Include(
                "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/SearchPartial.js",
                    "~/Scripts/app/Patient/Search.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient Demographics Javascript Bundle
            bundles.Add(
                new ScriptBundle("~/bundles/patientDemographics").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/SearchPartial.js",
                    "~/Scripts/app/Patient/ViewModel/PatientDemographicsViewModel.js",
                    "~/Scripts/app/Patient/Demographics.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient  AdditionalAddresses Javascript Bundle
            bundles.Add(
                new ScriptBundle("~/bundles/patientAdditionalAddresses").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/AdditionalAddresses.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Appointments
            bundles.Add(
                new ScriptBundle("~/bundles/patientAppointments").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/Appointments.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/ContactLenses
            bundles.Add(
                new ScriptBundle("~/bundles/patientContactLenses").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/ProcedureDiagnosisCodes.js",
                    "~/Scripts/app/Patient/ContactLenses.js",
                    "~/Scripts/app/Patient/OutsideProvider.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Correspondence
            bundles.Add(
                new ScriptBundle("~/bundles/patientcorrespondence").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/Correspondence.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Documents
            bundles.Add(
                new ScriptBundle("~/bundles/patientdocuments").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/Documents.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/app/common/bootstrap-treeview.min.js"));

            //// Patient/FeeSlips
            bundles.Add(
                new ScriptBundle("~/bundles/patientFeeSlips").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/Patient/FeeSlips.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/FinancialInfo
            bundles.Add(
                new ScriptBundle("~/bundles/patientFinancialInfo").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/FinancialInfo.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Glasses
            bundles.Add(
                new ScriptBundle("~/bundles/patientGlasses").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/ProcedureDiagnosisCodes.js",
                    "~/Scripts/app/Patient/Glasses.js",
                    "~/Scripts/app/Patient/OutsideProvider.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// patient HardContactLenses
            bundles.Add(
                new ScriptBundle("~/bundles/patientHardContactLenses").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/ProcedureDiagnosisCodes.js",
                     "~/Scripts/app/Patient/ViewModel/PatientHardContactLensViewModel.js",
                    "~/Scripts/app/Patient/HardContactLenses.js",
                    "~/Scripts/app/Patient/OutsideProvider.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            bundles.Add(new ScriptBundle("~/bundles/patientInsuranceEligibility").Include(
                  "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/app/Common/common.ko-dataTable-1.9.4.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js",
                    "~/Scripts/app/Patient/ViewModel/BlankPatientInsurance.js",
                    "~/Scripts/app/Patient/ViewModel/PatientInsuranceViewModel.js",
                    "~/Scripts/app/Patient/ViewModel/AttributeViewModel.js",
                    "~/Scripts/app/Patient/ViewModel/EligibilityModel.js",
                    "~/Scripts/app/Patient/ViewModel/EligibilityModal.js",
                    "~/Scripts/app/Patient/ViewModel/EligibilityModalPmi.js",
                    "~/Scripts/app/Patient/ViewModel/EligibilityConfigurationViewModel.js",
                    "~/Scripts/app/Patient/InsuranceEligibility.js",
                    "~/Scripts/app/Patient/ViewModel/EligibilityStatus.js"));

            //// Patient/Insurance
            bundles.Add(
                new ScriptBundle("~/bundles/patientInsurance").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/Patient/ViewModel/BlankPatientInsurance.js",
                    "~/Scripts/app/Patient/ViewModel/PatientInsuranceViewModel.js",
                    "~/Scripts/app/Patient/Insurance.js",
                    "~/Scripts/app/Patient/AddInsurancePartial.js",
                    "~/Scripts/app/Patient/SubscriberInsurance.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Ledger
            bundles.Add(
                new ScriptBundle("~/bundles/patientLedger").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/Patient/Ledger.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/MaterialOrders
            bundles.Add(
                new ScriptBundle("~/bundles/patientMaterialOrders").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/MaterialOrders.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/MeaningfulUse
            bundles.Add(
                new ScriptBundle("~/bundles/patientMeaningfulUse").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/MeaningfulUse.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Merge
            bundles.Add(
                new ScriptBundle("~/bundles/patientMerge").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/SearchPartial.js",
                    "~/Scripts/app/Patient/Merge.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Notes
            bundles.Add(
                new ScriptBundle("~/bundles/patientNotes").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/Notes.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Recalls
            bundles.Add(
                new ScriptBundle("~/bundles/patientRecalls").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/Recalls.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Relationships
            bundles.Add(
                new ScriptBundle("~/bundles/patientRelationships").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/Relationships.js",
                    "~/Scripts/app/Patient/SearchPartial.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Rx
            bundles.Add(
                new ScriptBundle("~/bundles/patientRx").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/ProcedureDiagnosisCodes.js",
                    "~/Scripts/app/Patient/Rx.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Exams
            bundles.Add(
                new ScriptBundle("~/bundles/patientExams").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/Exams.js",
                    "~/Scripts/app/Patient/SearchPartial.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/NewExam
            bundles.Add(
                new ScriptBundle("~/bundles/patientNewExam").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/bootstrap-select-ajax-1.3.5.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/NewExam.js",
                    "~/Scripts/app/Patient/SearchPartial.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/ContactLensOrder
            bundles.Add(
                new ScriptBundle("~/bundles/patientContactLensOrder").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/ContactLensOrder.js",
                    "~/Scripts/app/Patient/InsurancePanelPartial.js",
                    "~/Scripts/app/Patient/SearchPartial.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/EyeglassOrder
            bundles.Add(
                new ScriptBundle("~/bundles/patientEyeglassOrder").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/EyeglassOrder.js",
                    "~/Scripts/app/Patient/EyeglassOrderChooseRx.js",
                     "~/Scripts/app/Patient/EyeglassOrderChooseExtras.js",
                    "~/Scripts/app/Patient/EyeglassOrderChooseLab.js",
                    "~/Scripts/app/Patient/EyeglassOrderTypePartial.js",
                    "~/Scripts/app/Patient/EyeglassOrderBuildOrderPartial.js",
                    "~/Scripts/app/Patient/EyeglassOrderChooseFramePartial.js",
                    "~/Scripts/app/Patient/EyeglassOrderChooseLensesPartial.js",
                    "~/Scripts/app/Patient/InsurancePanelPartial.js",
                    "~/Scripts/app/Patient/SearchPartial.js",
                    "~/Scripts/app/Patient/SearchPartial.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Patient/Overview
            bundles.Add(
            new ScriptBundle("~/bundles/patientOverview").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/bootstrap-select-ajax-1.3.5.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/Overview.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Security
            bundles.Add(
                new ScriptBundle("~/bundles/securityForbidden").Include(
                     "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/app/Security/ForbiddenDialog.js"));

            //// Appointments/AppointmentConfirmations
            bundles.Add(
                new ScriptBundle("~/bundles/AppointmentConfirmations").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Appointments/AppointmentConfirmations.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Appointments/Calendar
            bundles.Add(
                new ScriptBundle("~/bundles/AppointmentCalendar").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Patient/SearchPartial.js",
                    "~/Scripts/app/Patient/ViewModel/PatientDemographicsViewModel.js",
                    "~/Scripts/eyefinity.context.js",
                    "~/Scripts/dhtmlxScheduler/ext/dhtmlxscheduler_limit.js",
                    "~/Scripts/dhtmlxScheduler/ext/dhtmlxscheduler_tooltip.js",
                    "~/Scripts/dhtmlxScheduler/ext/dhtmlxscheduler_key_nav.js",
                    "~/Scripts/app/Appointments/AppointmentsOfficeContext.js",
                    "~/Scripts/app/Appointments/AppointmentDialog.js",
                    "~/Scripts/app/Appointments/SchedulerExceptions.js",
                    "~/Scripts/app/Appointments/dropit.js",
                    "~/Scripts/app/Appointments/Calendar.js",
                    "~/Scripts/app/Patient/ViewModel/PatientInsuranceViewModel.js",
                    "~/Scripts/app/Patient/ViewModel/BlankPatientInsurance.js",
                    "~/Scripts/app/Patient/AddInsurancePartial.js",
                    "~/Scripts/app/Patient/SubscriberInsurance.js",
                    "~/Scripts/app/common/footer.js",
                    "~/Scripts/app/Patient/Navigation.js",
                    "~/Scripts/app/Patient/ViewModel/AttributeViewModel.js",
                    "~/Scripts/app/Patient/ViewModel/EligibilityModel.js",
                    "~/Scripts/app/Patient/ViewModel/EligibilityModal.js",
                    "~/Scripts/app/Patient/ViewModel/EligibilityModalPmi.js",
                    "~/Scripts/app/Patient/ViewModel/EligibilityConfigurationViewModel.js",
                    "~/Scripts/app/Patient/InsuranceEligibility.js",
                    "~/Scripts/app/Patient/ViewModel/EligibilityStatus.js"));

            //// Appointments/Holidays
            bundles.Add(
                new ScriptBundle("~/bundles/appointmentholidays").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Appointments/Holidays.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Appointments/OfficeHours
            bundles.Add(
                new ScriptBundle("~/bundles/appointmentofficehours").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Appointments/OfficeHours.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Appointments/ResourceSchedule
            bundles.Add(
                new ScriptBundle("~/bundles/appointmentresourceschedule").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Appointments/ResourceSchedule.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// ClaimManagement
            bundles.Add(
                new ScriptBundle("~/bundles/claimmanagement").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/dataTables.js",
                    "~/Scripts/app/dataTables.bootstrap.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/common.date.js",
                    "~/Scripts/app/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/common.ko-combobox.js",
                    "~/Scripts/app/common.ko-datatable-{version}.js",
                    "~/Scripts/app/common.progressTimer.js",
                    "~/Scripts/app/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/ClaimManagement/ClaimManagement.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Daily Closing
            bundles.Add(
                new ScriptBundle("~/bundles/dailyclosing").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Common/DailyClosing.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Fee Slips
            bundles.Add(new StyleBundle("~/bundles/feeslips").Include("~/Scripts/app/FeeSlips/FeeSlips.js"));

            //// Product
            bundles.Add(new ScriptBundle("~/bundles/productInventory").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/dataTables.js",
                    "~/Scripts/app/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/common.date.js",
                    "~/Scripts/app/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/common.ko-combobox.js",
                    "~/Scripts/app/common.ko-datatable-{version}.js",
                    "~/Scripts/app/common.progressTimer.js",
                    "~/Scripts/app/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Product/Inventory.js",
                    "~/Scripts/app/common/footer.js"));

            bundles.Add(new ScriptBundle("~/bundles/productLookup").Include("~/Scripts/app/Product/Lookup.js"));

            bundles.Add(
                new ScriptBundle("~/bundles/productPrintLabels").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/dataTables.js",
                    "~/Scripts/app/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/common.date.js",
                    "~/Scripts/app/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/common.ko-combobox.js",
                    "~/Scripts/app/common.ko-datatable-{version}.js",
                    "~/Scripts/app/common.progressTimer.js",
                    "~/Scripts/app/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Product/PrintLabels.js",
                    "~/Scripts/app/common/footer.js"));

            bundles.Add(
                new ScriptBundle("~/bundles/productWorkInProgress").Include("~/Scripts/app/Product/WorkInProgress.js"));

            //// Reporting
            bundles.Add(new ScriptBundle("~/bundles/reporting/appointmentInsurance").Include(
                "~/Scripts/app/Reporting/AppointmentInsurance.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/appointmentReschedule").Include(
                "~/Scripts/app/Reporting/AppointmentReschedule.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/appointmentSchedule").Include(
               "~/Scripts/app/Reporting/AppointmentSchedule.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/appointmentScheduleSummary").Include(
               "~/Scripts/app/Reporting/AppointmentScheduleSummary.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/appointmentServiceType").Include(
               "~/Scripts/app/Reporting/AppointmentServiceType.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/appointmentStatus").Include(
               "~/Scripts/app/Reporting/AppointmentStatus.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/refundAdjustmentSales").Include(
               "~/Scripts/app/Reporting/RefundAdjustmentSales.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/dailyFlashSales").Include(
               "~/Scripts/app/Reporting/DailyFlashSales.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/cashReceiptSummary").Include(
              "~/Scripts/app/Reporting/CashReceiptSummary.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/dailyTransactionPayment").Include(
               "~/Scripts/app/Reporting/DailyTransactionPayment.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/dailyTransactionSales").Include(
               "~/Scripts/app/Reporting/DailyTransactionSales.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/itemSales").Include(
               "~/Scripts/app/Reporting/ItemSales.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/discountAnalysis").Include(
              "~/Scripts/app/Reporting/DiscountAnalysis.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/marketingRecall").Include(
               "~/Scripts/app/Reporting/MarketingRecall.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/officeFlashSales").Include(
                           "~/Scripts/app/Reporting/OfficeFlashSales.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/patientAudit").Include(
                           "~/Scripts/app/Reporting/PatientAudit.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/patientBillingHistory").Include(
                           "~/Scripts/app/Reporting/PatientBillingHistory.js",
                           "~/Scripts/app/Patient/SearchPartial.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/patientFollowupNotes").Include(
               "~/Scripts/app/Reporting/PatientFollowupNotes.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/patientOpenEehrExams").Include(
                "~/Scripts/app/Reporting/PatientOpenEehrExams.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/patientStatements").Include(
              "~/Scripts/app/Reporting/PatientStatements.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/patientWorkInProgress").Include(
              "~/Scripts/app/Reporting/PatientWorkInProgress.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/accountsReceivableAging").Include(
              "~/Scripts/app/Reporting/AccountsReceivableAging.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/InventoryFrameSalesByItem").Include(
             "~/Scripts/app/Reporting/InventoryFrameSalesByItem.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/productionByProvider").Include(
             "~/Scripts/app/Reporting/ProductionByProvider.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/monthlyProductionByProvider").Include(
            "~/Scripts/app/Reporting/MonthlyProductionByProvider.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/monthlyProductionSummary").Include(
            "~/Scripts/app/Reporting/MonthlyProductionSummary.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/pdfView").Include(
                "~/Scripts/app/Reporting/PdfView.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/inventoryValuation").Include(
                "~/Scripts/app/Reporting/InventoryValuation.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/costOfSalesAnalysisByOffice").Include(
                           "~/Scripts/app/Reporting/CostOfSalesAnalysisByOffice.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/undeliveredOrders").Include(
                "~/Scripts/app/Reporting/UndeliveredOrders.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/patientRemake").Include(
                "~/Scripts/app/Reporting/patientRemake.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/miscPayment").Include(
                "~/Scripts/app/Reporting/MiscPayment.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/titledSalesAdjustment").Include(
                "~/Scripts/app/Reporting/TitledSalesAdjustment.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting/patientCreditBalance").Include(
                "~/Scripts/app/Reporting/PatientCreditBalance.js"));

            bundles.Add(new ScriptBundle("~/bundles/reporting").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Reporting/Reporting.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            bundles.Add(new ScriptBundle("~/bundles/pdfReporting").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Reporting/Reporting.js",
                    "~/Scripts/app/common/footer.js"));

            //// Change Password
            bundles.Add(
                new ScriptBundle("~/bundles/changePassword").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Common/ChangePassword.js",
                    "~/Scripts/app/common/footer.js"));

            //// Security Question
            bundles.Add(
                new ScriptBundle("~/bundles/securityQuestion").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/Common/common.date.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-datatable-{version}.js",
                    "~/Scripts/app/Common/common.progressTimer.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.zipcode.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/app/Common/SecurityQuestion.js",
                    "~/Scripts/app/common/footer.js"));
        }

        #endregion

        #region Methods
        /// <summary>
        ///     The register Style bundles.
        /// </summary>
        /// <param name="bundles">
        ///     The bundles.
        /// </param>
        private static void RegisterStyleBundles(BundleCollection bundles)
        {
            ////Style Bundle for base Jquery UI library
            bundles.Add(new StyleBundle("~/content/themes/base/css")
                    .Include("~/Content/themes/base/jquery.ui.core.css")
                    .Include("~/Content/themes/base/jquery.ui.resizable.css")
                    .Include("~/Content/themes/base/jquery.ui.selectable.css")
                    .Include("~/Content/themes/base/jquery.ui.accordion.css")
                    .Include("~/Content/themes/base/jquery.ui.autocomplete.css")
                    .Include("~/Content/themes/base/jquery.ui.button.css")
                    .Include("~/Content/themes/base/jquery.ui.dialog.css")
                    .Include("~/Content/themes/base/jquery.ui.slider.css")
                    .Include("~/Content/themes/base/jquery.ui.tabs.css")
                    .Include("~/Content/themes/base/jquery.ui.datepicker.css")
                    .Include("~/Content/themes/base/jquery.ui.progressbar.css")
                    .Include("~/Content/themes/base/jquery.ui.theme.css")
                    .Include("~/Content/themes/base/jquery.dataTables-{version}.css")
                    .Include("~/Content/themes/base/jquery.selectboxit-{version}.css")
                    .Include("~/Content/themes/base/jquery.multiSelect-{version}.css")
                    .Include("~/Content/themes/base/jquery.notify-{version}.css")
                    .Include("~/Content/themes/base/jquery.Jcrop.css")
                    .Include("~/Content/themes/base/spectrum.css"));

            ////Style Bundle for common Custom CSS
            bundles.Add(new StyleBundle("~/content/themes/base/custom/common")
                    .Include("~/Content/themes/base/custom/bootstrap-select-custom.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/themes/base/custom/bootstrap-timepicker-custom.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/themes/base/custom/jquery.dataTables-custom.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for front office Custom CSS
            bundles.Add(new StyleBundle("~/content/themes/base/custom/frontoffice")
                    .Include("~/Content/themes/base/custom/bootstrap-template.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for billing Custom CSS
            bundles.Add(new StyleBundle("~/content/themes/base/custom/billing")
                    .Include("~/Content/themes/base/custom/bootstrap-template.billing.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/base/custom/jqueryuicustom")
                    .Include("~/Content/themes/base/custom/jquery.ui.theme.custom.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for Jquery
            bundles.Add(new StyleBundle("~/content/themes/app/styles/css")
                    .Include("~/Content/themes/app/styles/common.ko-combobox.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/themes/app/styles/common.ko-datatables.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for Jquery
            bundles.Add(new StyleBundle("~/content/themes/app/login/css")
                    .Include("~/Content/themes/app/Login/login.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/themes/app/Login/forbidden.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for Front Office
            bundles.Add(new StyleBundle("~/content/frontoffice")
                    .Include("~/Content/bootstrap.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/bootstrap-select.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/bootstrap-timepicker-{version}.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/icons.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/site.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/themes/app/Login/forbidden.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for Front Office
            bundles.Add(new StyleBundle("~/content/themes/app/Patient/overview")
                   .Include("~/Content/themes/app/Patient/Overview.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for Front Office
            bundles.Add(new StyleBundle("~/content/themes/app/Patient/rx")
                   .Include("~/Content/themes/app/Patient/Rx.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for Front Office Patient Merge
            bundles.Add(new StyleBundle("~/content/themes/app/Patient/merge")
                   .Include("~/Content/themes/app/Patient/Merge.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for Front Office
            bundles.Add(new StyleBundle("~/content/themes/app/Patient/ledger")
                   .Include("~/Content/themes/app/Patient/Ledger.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle Treeview
            bundles.Add(new StyleBundle("~/content/themes/app/Patient/bootstrap-treeview.min")
                 .Include("~/Content/themes/app/Patient/bootstrap-treeview.min.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for Billing
            bundles.Add(new StyleBundle("~/content/billing")
                    .Include("~/Content/bootstrap.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/bootstrap-select.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/icons.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/site.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for Dhtmlx Calendar
            bundles.Add(new StyleBundle("~/scripts/dhtmlxScheduler/content")
                    .Include("~/Scripts/dhtmlxScheduler/dhtmlxscheduler.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Scripts/dhtmlxScheduler/dhtmlxExtension.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Scripts/dhtmlxScheduler/dropit.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for Admin
            bundles.Add(new StyleBundle("~/content/themes/app/ProductsServices/itemtypes")
                   .Include("~/Content/themes/app/ProductsServices/ItemType.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Resources/securitysetup")
                   .Include("~/Content/themes/app/Resources/SecuritySetup.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Scheduler/officehours")
                    .Include("~/Content/themes/app/Scheduler/OfficeHours.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Scheduler/preferences")
                    .Include("~/Content/themes/app/Scheduler/Preferences.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Scheduler/resourceschedule")
                    .Include("~/Content/themes/app/Scheduler/ResourceSchedule.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Practice/practiceinformation")
                    .Include("~/Content/themes/app/Practice/NewPracticeInformation.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Office/claimsinformation")
                    .Include("~/Content/themes/app/Practice/ClaimsInformation.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Office/placeofservice")
                 .Include("~/Content/themes/app/Practice/PlaceOfService.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Practice/additionalintegrations")
                    .Include("~/Content/themes/app/Practice/AdditionalIntegrations.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Practice/labSetup")
                 .Include("~/Content/themes/app/Practice/LabSetup.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Practice/PatientLocations")
                .Include("~/Content/themes/app/Practice/PatientLocations.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Preferences/recalltypes")
                    .Include("~/Content/themes/app/Preferences/RecallTypes.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Preferences/recallschedules")
                    .Include("~/Content/themes/app/Preferences/RecallSchedules.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Preferences/Inventory")
                   .Include("~/Content/themes/app/Preferences/Inventory.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/ProductsServices/productssetup")
                    .Include("~/Content/themes/app/ProductsServices/ProductsSetup.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/ProductsServices/eyeglasslenssetup")
                    .Include("~/Content/themes/app/ProductsServices/EyeglassLensSetup.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/ProductsServices/eyeglasslenspricingdialog")
                    .Include("~/Content/themes/app/ProductsServices/EyeglassLensPricingDialog.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/ProductsServices/framesetup")
                    .Include("~/Content/themes/app/ProductsServices/FrameSetup.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/ProductsServices/bulkpricingframe")
                    .Include("~/Content/themes/app/ProductsServices/BulkPricingFrame.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/ProductsServices/addcustomframe")
                    .Include("~/Content/themes/app/ProductsServices/AddCustomFrame.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/ProductsServices/servicesetup")
                    .Include("~/Content/themes/app/ProductsServices/ServiceSetup.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Insurance/carrierandplansetup")
                   .Include("~/Content/themes/app/Insurance/CarrierAndPlanSetup.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Scheduler/appointmentservicesetup")
                    .Include("~/Content/themes/app/Scheduler/AppointmentServiceSetup.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Insurance/patientInsuranceeligibility")
                 .Include("~/Content/themes/app/Insurance/PatientInsuranceEligibility.css", new CssRewriteUrlTransformFixed()));

            ////Style Bundle for Front Office
            bundles.Add(new StyleBundle("~/content/themes/app/Patient/quicklist")
                   .Include("~/Content/themes/app/Patient/Quicklist.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/Scheduler/appointmentDialog")
                  .Include("~/Content/themes/app/Scheduler/Scheduler.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/admin")
                    .Include("~/Content/adminreset.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/adminsite.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/admin.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/adminheader.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/adminfooter.css", new CssRewriteUrlTransformFixed())
                    .Include("~/Content/icons.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/ProductsServices/accessorySetup")
                   .Include("~/Content/themes/app/ProductsServices/AccessorySetup.css", new CssRewriteUrlTransformFixed()));

            bundles.Add(new StyleBundle("~/content/themes/app/ProductsServices/otherItemsSetup")
                   .Include("~/Content/themes/app/ProductsServices/OtherItemsSetup.css", new CssRewriteUrlTransformFixed()));
        }

        /// <summary>
        ///     The register billing bundles.
        /// </summary>
        /// <param name="bundles">
        ///     The bundles.
        /// </param>
        private static void RegisterBillingScriptBundles(BundleCollection bundles)
        {
            //// Billing/CMSForm
            bundles.Add(
                new ScriptBundle("~/bundles/cmsform").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/dataTables.js",
                    "~/Scripts/app/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/common.date.js",
                    "~/Scripts/app/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/common.ko-combobox.js",
                    "~/Scripts/app/common.ko-datatable-{version}.js",
                    "~/Scripts/app/common.progressTimer.js",
                    "~/Scripts/app/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/App/BillingClaim/CMSForm.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Billing/GatewayEDITransmission
            bundles.Add(
                new ScriptBundle("~/bundles/gatewayeditransmission").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/dataTables.js",
                    "~/Scripts/app/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/common.date.js",
                    "~/Scripts/app/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/common.ko-combobox.js",
                    "~/Scripts/app/common.ko-datatable-{version}.js",
                    "~/Scripts/app/common.progressTimer.js",
                    "~/Scripts/app/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/app/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/App/BillingClaim/GatewayEDITransmission.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Billing/PatientLetter
            bundles.Add(
                new ScriptBundle("~/bundles/patientletter").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/dataTables.js",
                    "~/Scripts/app/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/common.date.js",
                    "~/Scripts/app/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/common.ko-combobox.js",
                    "~/Scripts/app/common.ko-datatable-{version}.js",
                    "~/Scripts/app/common.progressTimer.js",
                    "~/Scripts/app/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/app/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/App/BillingClaim/PatientLetter.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Billing/StatementForms
            bundles.Add(
                new ScriptBundle("~/bundles/statementforms").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/dataTables.js",
                    "~/Scripts/app/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/common.date.js",
                    "~/Scripts/app/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/common.ko-combobox.js",
                    "~/Scripts/app/common.ko-datatable-{version}.js",
                    "~/Scripts/app/common.progressTimer.js",
                    "~/Scripts/app/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/App/BillingClaim/StatementForms.js",
                    "~/Scripts/app/common/footer.js"));

            //// Billing/BatchAdjustments
            bundles.Add(
                new ScriptBundle("~/bundles/batchadjustments").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/dataTables.js",
                    "~/Scripts/app/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/common.date.js",
                    "~/Scripts/app/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/common.ko-combobox.js",
                    "~/Scripts/app/common.ko-datatable-{version}.js",
                    "~/Scripts/app/common.progressTimer.js",
                    "~/Scripts/app/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/app/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/App/Miscellaneous/BatchAdjustments.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Billing/CarrierPayments
            bundles.Add(
                new ScriptBundle("~/bundles/carrierpayments").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/dataTables.js",
                    "~/Scripts/app/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/common.date.js",
                    "~/Scripts/app/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/common.ko-combobox.js",
                    "~/Scripts/app/common.ko-datatable-{version}.js",
                    "~/Scripts/app/common.progressTimer.js",
                    "~/Scripts/app/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/app/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/App/ProcessPayments/CarrierPayments.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Billing/PatientPayments
            bundles.Add(
                new ScriptBundle("~/bundles/patientpayments").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/dataTables.js",
                    "~/Scripts/app/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/common.date.js",
                    "~/Scripts/app/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/common.ko-combobox.js",
                    "~/Scripts/app/common.ko-datatable-{version}.js",
                    "~/Scripts/app/common.progressTimer.js",
                    "~/Scripts/app/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/app/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/App/ProcessPayments/PatientPayments.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));

            //// Billing/BillingReports
            bundles.Add(
                new ScriptBundle("~/bundles/billingreports").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/bootstrap-select.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/bootstrap-timepicker-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/modernizr-{version}.js",
                    "~/Scripts/jquery.dataTables-{version}.js",
                    "~/Scripts/app/dataTables.js",
                    "~/Scripts/app/dataTables.bootstrap.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/Common/common.bootstrap-select.js",
                    "~/Scripts/app/Common/header.js",
                    "~/Scripts/app/common.date.js",
                    "~/Scripts/app/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/common.ko-combobox.js",
                    "~/Scripts/app/common.ko-datatable-{version}.js",
                    "~/Scripts/app/common.progressTimer.js",
                    "~/Scripts/app/common.utility.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Common/common.validation.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/dataTables.bootstrap.js",
                    "~/Scripts/app/common.zipcode.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/jquery.spinner.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/Common/messaging.bootstrap.js",
                    "~/Scripts/App/BillingReporting/BillingReports.js",
                    "~/Scripts/app/Patient/QuickList.js",
                    "~/Scripts/app/common/footer.js"));
        }

        /// <summary>
        ///     The register admin bundles.
        /// </summary>
        /// <param name="bundles">
        ///     The bundles.
        /// </param>
        private static void RegisterAdminScriptBundles(BundleCollection bundles)
        {
            //// practice Practice Information
            bundles.Add(new ScriptBundle("~/bundles/adminhome").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Home/AdminHome.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/common/footer.js"));

            //// practice Claims Information
            bundles.Add(new ScriptBundle("~/bundles/claimsInformation").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/office/ClaimsInformation.js",
                    "~/Scripts/app/AdminCommon/ko-zipcode.js",
                    "~/Scripts/app/common/footer.js"));

            //// practice Place Of Service
            bundles.Add(new ScriptBundle("~/bundles/placeOfService").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/office/PlaceOfService.js",
                    "~/Scripts/app/AdminCommon/ko-zipcode.js",
                    "~/Scripts/app/common/footer.js"));

            //// resource Provider Setup Dialog
            bundles.Add(new ScriptBundle("~/bundles/placeOfServiceDialog").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.placeholder.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/AdminCommon/ko-zipcode.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/office/PlaceOfServiceDialog.js",
                    "~/Scripts/app/common/footer.js"));

            //// company Practice Information
            bundles.Add(new ScriptBundle("~/bundles/companyInformation").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/AdminCommon/ko-upload.js",
                    "~/Scripts/jquery.Jcrop.js",
                    "~/Scripts/app/AdminCommon/ko-jcrop.js",
                    "~/Scripts/app/Company/CompanyInformation-ViewModel.js",
                    "~/Scripts/app/Company/CompanyInformation.js",
                    "~/Scripts/app/AdminCommon/ko-zipcode.js",
                    "~/Scripts/app/common/footer.js"));

            //// office information
            bundles.Add(new ScriptBundle("~/bundles/officeInformation").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/jquery.fileupload.js",
                    "~/Scripts/jquery.iframe-transport.js",
                    "~/Scripts/app/AdminCommon/ko-upload.js",
                    "~/Scripts/jquery.Jcrop.js",
                    "~/Scripts/app/AdminCommon/ko-jcrop.js",
                    "~/Scripts/app/Office/OfficeInformation-ViewModel.js",
                    "~/Scripts/app/Office/OfficeInformation.js",
                    "~/Scripts/app/AdminCommon/ko-zipcode.js",
                    "~/Scripts/app/common/footer.js"));

            //// preferences recall Types
            bundles.Add(new ScriptBundle("~/bundles/recallTypes").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Preferences/RecallTypes.js",
                    "~/Scripts/app/common/footer.js"));

            //// preferences recall Types
            bundles.Add(new ScriptBundle("~/bundles/recallSchedules").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/PatientEngagement/RecallSchedule.js",
                    "~/Scripts/app/common/footer.js"));

            //// preferences Required Patient Profile
            bundles.Add(new ScriptBundle("~/bundles/requiredPatientProfile").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Preferences/RequiredPatientProfile.js",
                    "~/Scripts/app/common/footer.js"));

            //// preferences inventory Preferences
            bundles.Add(new ScriptBundle("~/bundles/inventoryPreferences").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Preferences/Inventory.js",
                    "~/Scripts/app/common/footer.js"));

            //// products/services Contact Lens Setup
            bundles.Add(new ScriptBundle("~/bundles/contactLensSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/ProductsServices/ContactLensSetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// products/services Eyeglass Lens Setup
            bundles.Add(new ScriptBundle("~/bundles/eyeglassLensSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/knockout.popupTemplate.js",
                    "~/Scripts/app/ProductsServices/EyeglassLensSetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// products/services Eyeglass Lens Setup
            bundles.Add(new ScriptBundle("~/bundles/eyeglassLensPricingDialog").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/ProductsServices/EyeglassLensPricingDialog.js",
                    "~/Scripts/app/common/footer.js"));

            //// products/services Frame Setup
            bundles.Add(new ScriptBundle("~/bundles/frameSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/ProductsServices/FrameSetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// products/services Frame Setup Bulk Pricing
            bundles.Add(new ScriptBundle("~/bundles/bulkPricingFrame").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/jquery.multiSelect-{version}.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.multiselect.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/ProductsServices/BulkPricingFrame.js",
                    "~/Scripts/app/common/footer.js"));

            //// products/services Add Custom Frame
            bundles.Add(new ScriptBundle("~/bundles/addCustomFrame").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/jquery.multiSelect-{version}.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.multiselect.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/ProductsServices/AddCustomFrame.js",
                    "~/Scripts/app/common/footer.js"));

            //// products/services Item Types
            bundles.Add(new ScriptBundle("~/bundles/itemTypes").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/ProductsServices/ItemType.js",
                    "~/Scripts/app/common/footer.js"));

            //// products/services Service Setup
            bundles.Add(new ScriptBundle("~/bundles/serviceSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/ProductsServices/ServiceSetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// resource Provider Setup
            bundles.Add(new ScriptBundle("~/bundles/providerSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/jquery.multiSelect-{version}.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.multiselect.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Resources/ProviderSetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// resource Provider Setup Dialog
            bundles.Add(new ScriptBundle("~/bundles/providerSetupDialog").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/jquery.multiSelect-{version}.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.placeholder.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.multiselect.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Resources/ProviderSetupDialog.js",
                    "~/Scripts/app/common/footer.js"));

            //// resource Insurance Details
            bundles.Add(new ScriptBundle("~/bundles/providerInsuranceDetails").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.placeholder.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/ko-zipcode.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Resources/InsuranceDetail.js",
                    "~/Scripts/app/common/footer.js"));

            //// resource Staff Setup Dialog
            bundles.Add(new ScriptBundle("~/bundles/staffSetupDialog").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/jquery.multiSelect-{version}.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.placeholder.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.multiselect.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Resources/StaffSetupDialog.js",
                    "~/Scripts/app/common/footer.js"));

            //// resource Provider Setup Dialog
            bundles.Add(new ScriptBundle("~/bundles/outsideProviderSetupDialog").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.placeholder.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/AdminCommon/ko-zipcode.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Resources/OutsideProviderSetupDialog.js",
                    "~/Scripts/app/common/footer.js"));

            //// resource Reset Password
            bundles.Add(new ScriptBundle("~/bundles/resetPassword").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.placeholder.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Resources/ResetPassword.js",
                    "~/Scripts/app/common/footer.js"));

            //// Outside Provider Setup
            bundles.Add(new ScriptBundle("~/bundles/outsideProviderSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/jquery.multiSelect-{version}.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.multiselect.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Resources/OutsideProviderSetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// resource Security Setup
            bundles.Add(new ScriptBundle("~/bundles/securitySetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.placeholder.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Security/SecuritySetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// resource staff Setup
            bundles.Add(new ScriptBundle("~/bundles/staffSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/jquery.multiSelect-{version}.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.placeholder.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.multiselect.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Resources/StaffSetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// scheduler AppointmentServiceSetup
            bundles.Add(new ScriptBundle("~/bundles/appointmentServiceSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Scheduler/AppointmentServiceSetup.js",
                    "~/Scripts/spectrum-Colorpicker-1.1.1.js",
                    "~/Scripts/app/AdminCommon/ko-spectrum-colorpicker-1.1.1.js",
                    "~/Scripts/app/common/footer.js"));

            //// scheduler holidays
            bundles.Add(new ScriptBundle("~/bundles/holidays").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Scheduler/Holidays.js",
                    "~/Scripts/app/common/footer.js"));

            //// scheduler officeHours
            bundles.Add(new ScriptBundle("~/bundles/officeHours").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Scheduler/OfficeHours.js",
                    "~/Scripts/app/common/footer.js"));

            //// scheduler preferences
            bundles.Add(new ScriptBundle("~/bundles/preferences").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Scheduler/Preferences.js",
                    "~/Scripts/spectrum-Colorpicker-1.1.1.js",
                    "~/Scripts/app/AdminCommon/ko-spectrum-colorpicker-1.1.1.js",
                    "~/Scripts/app/common/footer.js"));

            //// scheduler resourceSchedule
            bundles.Add(new ScriptBundle("~/bundles/resourceSchedule").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Scheduler/ResourceSchedule.js",
                    "~/Scripts/app/common/footer.js"));

            //// Company Additional Integrations
            bundles.Add(new ScriptBundle("~/bundles/companyadditionalIntegrations").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Company/AdditionalIntegrations.js",
                    "~/Scripts/app/common/footer.js"));

            //// Office Additional Integrations
            bundles.Add(new ScriptBundle("~/bundles/officeadditionalIntegrations").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Office/AdditionalIntegrations.js",
                    "~/Scripts/app/common/footer.js"));

            //// Office Patient Locations
            bundles.Add(new ScriptBundle("~/bundles/officePatientLocations").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Office/PatientLocations.js",
                    "~/Scripts/app/common/footer.js"));

            //// Lab Setup
            bundles.Add(new ScriptBundle("~/bundles/labSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Office/LabSetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// Letter Templates
            bundles.Add(new ScriptBundle("~/bundles/LetterTemplates").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/iframeResizer.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/PatientEngagement/LetterTemplates.js",
                    "~/Scripts/app/common/footer.js"));

            //// Carrier And Plan Setup
            bundles.Add(new ScriptBundle("~/bundles/carrierAndPlanSetup").Include(
                   "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.placeholder.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/Insurance/CarrierAndPlanSetup.js",
                    "~/Scripts/app/common/footer.js"));

            // Report Viewer
            bundles.Add(new ScriptBundle("~/bundles/reportViewer").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/app/Reporting/CrReportsView.js"));

            //// products/services Accessories Setup
            bundles.Add(new ScriptBundle("~/bundles/accessoriesSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/ProductsServices/AccessoriesSetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// products/services Shipping Setup
            bundles.Add(new ScriptBundle("~/bundles/shippingSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/ProductsServices/ShippingSetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// products/services Repairs Setup
            bundles.Add(new ScriptBundle("~/bundles/repairsSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/ProductsServices/RepairsSetup.js",
                    "~/Scripts/app/common/footer.js"));

            //// products/services MiscFees Setup
            bundles.Add(new ScriptBundle("~/bundles/miscFeesSetup").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/sugar.js",
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/knockout-jqueryui.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.validate.js",
                    "~/Scripts/jquery.datatables-{version}.js",
                    "~/Scripts/app/Common/dataTables.js",
                    "~/Scripts/jquery.maskedinput-{version}.js",
                    "~/Scripts/jquery.notify-{version}.js",
                    "~/Scripts/jquery.selectBoxIt-{version}.core.js",
                    "~/Scripts/dateJS.js",
                    "~/Scripts/app/Common/common.dialog.js",
                    "~/Scripts/app/Common/common.js",
                    "~/Scripts/app/Common/common.utility.js",
                    "~/Scripts/app/Common/common.ko-combobox.js",
                    "~/Scripts/app/Common/common.ko-dataTable-{version}.js",
                    "~/Scripts/app/AdminCommon/header.js",
                    "~/Scripts/app/AdminCommon/common.accordion.js",
                    "~/Scripts/app/Common/common.apiclient.js",
                    "~/Scripts/app/AdminCommon/common.date.js",
                    "~/Scripts/app/AdminCommon/common.mask.js",
                    "~/Scripts/app/AdminCommon/common.messagebox.js",
                    "~/Scripts/app/AdminCommon/common.messaging.js",
                    "~/Scripts/app/AdminCommon/common.selectBoxIt.js",
                    "~/Scripts/app/AdminCommon/common.validation.js",
                    "~/Scripts/app/AdminCommon/AdminOfficeContext.js",
                    "~/Scripts/app/Common/jquery.ajax.spinner.js",
                    "~/Scripts/app/ProductsServices/MiscFeesSetup.js",
                    "~/Scripts/app/common/footer.js"));
        }
        #endregion
    }
}