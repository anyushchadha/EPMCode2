// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppointmentsController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the AppointmentsController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Mvc;

    using DHTMLX.Common;
    using DHTMLX.Scheduler;
    using DHTMLX.Scheduler.Controls;
    using DHTMLX.Scheduler.Data;

    using Eyefinity.Enterprise.Business.Appointment;
    using Eyefinity.Enterprise.Business.Common;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Legacy.Reports;
    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;
    using Eyefinity.PracticeManagement.Model.Appointment;
    using Eyefinity.WebScheduler.DataContracts;

    using IT2.Reports;

    /// <summary>
    ///     The appointments controller.
    /// </summary>
    [NoCache]
    public class AppointmentsController : Controller
    {
        /// <summary>
        ///     The appointment manager.
        /// </summary>
        private readonly AppointmentManager appointmentManager;

        private readonly OfficeHelper officeHelper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AppointmentsController" /> class.
        /// </summary>
        public AppointmentsController()
        {
            this.appointmentManager = new AppointmentManager();
            this.officeHelper = new OfficeHelper();
        }

        // GET: /Appointments/

        /// <summary>
        /// The calendar.
        /// </summary>
        /// <param name="id">
        /// The patient id.
        /// </param>
        /// <param name="firstName">
        /// The first name.
        /// </param>
        /// <param name="lastName">
        /// The last name.
        /// </param>
        /// <param name="view">
        /// The view.
        /// </param>
        /// <param name="officeNum">
        /// The officeNum.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Calendar(int? id, string firstName, string lastName, string view, string officeNum)
        {
            if (id != null)
            {
                try
                {
                    AccessControl.VerifyUserAccessToPatient(id.GetValueOrDefault());
                }
                catch (HttpResponseException)
                {
                    return new HttpForbiddenResult();
                }
            }

            var authorizationTicketHelper = new AuthorizationTicketHelper();
            var user = authorizationTicketHelper.GetUserInfo();
            var employeeId = this.appointmentManager.GetEmployeeIdByUserId(user.Id).GetValueOrDefault();
            var startDate = DateTime.Now.Date.AddDays(-Convert.ToDouble(DateTime.Now.DayOfWeek));
            var endDate = DateTime.Now.Date.AddDays(6 - Convert.ToDouble(DateTime.Now.DayOfWeek));
            var userOfficeId = Convert.ToInt32(user.OfficeId);
            if (!string.IsNullOrEmpty(officeNum))
            {
                if (user.OfficeNum != officeNum)
                {
                    var companyId = OfficeHelper.GetCompanyId(officeNum);
                    if (companyId != user.CompanyId)
                    {
                        return new HttpForbiddenResult();
                    }
                }

                userOfficeId = this.officeHelper.GetOfficeById(officeNum).OfficeId;
            }

            var scheduler = this.appointmentManager.GetSchedulerConfiguration(
                userOfficeId, (int)employeeId, startDate, endDate, view, user.CompanyId);
            var sched = new DHXScheduler
                            {
                                DataAction = this.Url.Action("Data"), 
                                SaveAction = this.Url.Action("Save"), 
                                Skin = DHXScheduler.Skins.Terrace, 
                                Config =
                                    {
                                        multi_day = true, 
                                        start_on_monday = false, 
                                        use_select_menu_space = false, 
                                        fix_tab_position = false, 
                                        show_loading = false, 
                                        mark_now = true, 
                                        touch = true, 
                                        touch_drag = 700,
                                        touch_tip = false,
                                        time_step = 5,
                                        hour_date = "%h:%i %A"
                                    }, 
                                InitialDate = DateTime.Now //// beginDate //// DateTime.Now
                            };

            sched.Extensions.Add(SchedulerExtensions.Extension.PDF);

            // load data initially
            sched.LoadData = true;

            sched.UpdateFieldsAfterSave();

            // save client-side changes
            sched.EnableDataprocessor = true;
            sched.EnableDynamicLoading(SchedulerDataLoader.DynamicalLoadingMode.Week);

            // add resource units to the scheduler and replace the normal daily view.
            sched.Views.Items.RemoveAt(2); ////day (will replace this one.)
            sched.Views.Items.RemoveAt(0); ////month

            ////initializes the view
            var agenda = new AgendaView { Label = "List", StartDate = startDate, EndDate = endDate };
            sched.Views.Items.Insert(0, agenda);

            var units = new UnitsView("resource", "Id") { Label = "Day", Property = "resourceId", SkipIncorrect = true };

            units.AddOptions(
                scheduler.Resources.Select(
                    resource => new CalendarUnit { key = resource.Id.ToString(), label = resource.DisplayName })
                         .ToList());
            units.Size = 50;
            sched.Views.Items.Insert(1, units);
            for (var i = 0; i < sched.Views.Count; i++)
            {
                sched.Views[i].TabPosition = 15 + (i * 60);
                sched.Views[i].TabClass = "week_tab";
            }

            var preferences = SchedulerPreferencesVm.FromDictionary(new SchedulerPreferencesManager().GetSchedulerPreferences(user.OfficeNum));
            if (view == "resource" || preferences.DefaultView == (int)WorkspaceType.DailyView)
            {
                sched.InitialView = sched.Views[1].Name;
                sched.InitialDate = endDate;
            }
            
            ////sched.DataAction = "Data?userId=" + userId + "&officeId=" + officeId;
            sched.DataAction = "Data?userId=" + user.Id + "&officeId=" + userOfficeId;
            sched.Calendars.AttachMiniCalendar();

            ////var cal = sched.Calendars.AttachMiniCalendar();
            ////cal.Navigation = true;
            ////cal.Position = "dhx_cal_tab";
            scheduler.Scheduler = sched;
            this.ViewBag.id = id;
            this.ViewBag.firstName = firstName;
            this.ViewBag.lastName = lastName;
            var security = new Security();
            var isExceptionPermitted = security.IsSingleLocationUserInRole(user.Id, "Schedule Exceptions");
            this.ViewBag.isExceptionPermitted = isExceptionPermitted;
            return this.View(scheduler);
        }

        /// <summary>
        /// The get resources availabilities.
        /// </summary>
        /// <param name="officeId">
        /// The office Id.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="from">
        /// The from.
        /// </param>
        /// <param name="to">
        /// The to.
        /// </param>
        /// <returns>
        /// The Resource Details
        /// </returns>
        public JsonResult GetResourcesAvailabilities(int officeId, int userId, string from, string to)
        {
            var employeeId = this.appointmentManager.GetEmployeeIdByUserId(userId).GetValueOrDefault();
            var startDate = DateTime.Parse(from);
            var endDate = DateTime.Parse(to);
            var availabilities = this.appointmentManager.GetResourcesAvailability(
                officeId, (int)employeeId, startDate, endDate);
            return this.Json(availabilities, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The appointment dialog.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult AppointmentDialog(string id)
        {
            this.ViewBag.Id = id;
            return this.View();
        }

        /// <summary>
        /// The data.
        /// </summary>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="officeId">
        /// The office Id.
        /// </param>
        /// <param name="from">
        /// The from.
        /// </param>
        /// <param name="to">
        /// The to.
        /// </param>
        /// <returns>
        /// The <see cref="ContentResult"/>.
        /// </returns>
        public ContentResult Data(int userId, int officeId, string from, string to)
        {
            var employeeId = this.appointmentManager.GetEmployeeIdByUserId(userId).GetValueOrDefault();
            var startDate = DateTime.Parse(from);
            var endDate = DateTime.Parse(to);
            var authorizationTicketHelper = new AuthorizationTicketHelper();
            var companyId = authorizationTicketHelper.GetUserInfo().CompanyId;
            var items = this.appointmentManager.GetAllAppointments(officeId, (int)employeeId, startDate, endDate, companyId);
            var data = new SchedulerAjaxData(items);
            return this.Content(data, "text/json");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="actionValues">
        /// The action values.
        /// </param>
        /// <param name="officeId">
        /// The office Id.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <returns>
        /// The <see cref="ContentResult"/>.
        /// </returns>
        public ContentResult Save(int? id, FormCollection actionValues, int officeId, int userId)
        {
            var employeeId = this.appointmentManager.GetEmployeeIdByUserId(userId).GetValueOrDefault();
            var action = new DataAction(actionValues);
            var changedEvent = DHXEventsHelper.Bind<DhxAppointment>(actionValues);
            changedEvent.ErrorMessages = string.Empty;
            try
            {
                switch (action.Type)
                {
                    case DataActionTypes.Insert:
                    case DataActionTypes.Update:
                        changedEvent = this.appointmentManager.SaveAppointment(
                            changedEvent, employeeId, officeId, false, false);
                        break;
                    case DataActionTypes.Delete:
                        this.appointmentManager.DeleteAppointment(changedEvent.id, employeeId);
                        break;
                    default:
                        action.Type = DataActionTypes.Error;
                        break;
                }

                action.TargetId = changedEvent.id;
            }
            catch
            {
                action.Type = DataActionTypes.Error;
            }

            if (!string.IsNullOrEmpty(changedEvent.ErrorMessages))
            {
                action.Type = DataActionTypes.Error;
                action.Message = changedEvent.ErrorMessages;
            }
            else
            {
                action.Message = changedEvent.text;
            }

            var response = new AjaxSaveResponse(action);
            response.UpdateField("start_date", changedEvent.start_date);
            response.UpdateField("end_date", changedEvent.end_date);

            return this.Content(response, "text/xml");
        }

        /// <summary>
        ///     The doctor schedule.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" />.
        /// </returns>
        public ActionResult ResourceSchedule()
        {
            var authorizationTicketHelper = new AuthorizationTicketHelper();
            var user = authorizationTicketHelper.GetUserInfo();
            return this.View(user);
        }

        /// <summary>
        ///     The office hours.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" />.
        /// </returns>
        public ActionResult OfficeHours()
        {
            return this.View();
        }

        /// <summary>
        ///     The holidays.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" />.
        /// </returns>
        public ActionResult Holidays()
        {
            return this.View();
        }

        /// <summary>
        ///     The appointment confirmation.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" />.
        /// </returns>
        public ActionResult AppointmentConfirmations()
        {
            return this.View();
        }

        /// <summary>
        ///     The get scheduler url.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        [System.Web.Mvc.HttpPost]
        public string GetSchedulerUrl()
        {
            return ConfigurationManager.AppSettings["SchedulerUrl"] + "WebScheduler/Dialogs/SchedulerView.aspx";
        }

        /// <summary>
        /// The print appointment confirmations.
        /// </summary>
        /// <param name="confirmationDate">
        /// The confirmation date.
        /// </param>
        /// <param name="officeNum">
        /// The office number.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The the result
        /// </returns>
        public ActionResult PrintAppointmentConfirmations(string confirmationDate, string officeNum, int userId)
        {
            var date = DateTime.Parse(confirmationDate);
            var confirmationSummaries = this.appointmentManager.GetAppointmentConfirmations(date, officeNum, userId);
            foreach (var confirmation in confirmationSummaries)
            {
                switch (confirmation.ConfirmationStatus)
                {
                    case "Confirmed":
                        {
                            confirmation.ConfirmationStatus = "<span style='font-weight:bold'>Confirmed</span>" + "</br>" + "Left Message" + "</br>" + "None" + 
                                "</br>" + "Not Available";
                            break;
                        }

                    case "Left Message":
                        {
                            confirmation.ConfirmationStatus = "Confirmed" + "</br>" + "<span style='font-weight:bold'>Left Message</span>" + "</br>" + "None" +
                                "</br>" + "Not Available";
                            break;
                        }

                    case "Not Available":
                        {
                            confirmation.ConfirmationStatus = "Confirmed" + "</br>" + "Left Message" + "</br>" + "None" + "</br>" + "<span style='font-weight:bold'>Not Available</span>";
                            break;
                        }

                    case "None":
                        {
                            confirmation.ConfirmationStatus = "Confirmed" + "</br>" + "Left Message" + "</br>" + "<span style='font-weight:bold'>None</span>" + 
                                "</br>" + "Not Available";
                            break;
                        }

                    default:
                        {
                            confirmation.ConfirmationStatus = "Confirmed" + "</br>" + "Left Message" + "</br>" + "<span style='font-weight:bold'>None</span>" +
                                "</br>" + "Not Available";
                            break;
                        }
                }
            }

            var confirmationsReport = ReportFactory.CreateReportWithManualDispose<AppointmentConfirmationsReport>();
                
                // This will be disposed after the report is streamed.
            var companyId = OfficeHelper.GetCompanyId(officeNum);
            confirmationsReport.SetDataSource(confirmationSummaries);
            confirmationsReport.SetParameterValue("Date", confirmationDate);
            var util = new ReportUtilities();
            var outStream =
                (MemoryStream)util.AddLogo(confirmationsReport, confirmationsReport.Section1, 0, companyId, officeNum);

            if (outStream != null)
            {
                this.Response.Clear();
                this.Response.ClearHeaders();
                this.Response.ContentType = "application/pdf";
                this.Response.BinaryWrite(outStream.ToArray());
                this.Response.End();
            }

            ReportFactory.CloseAndDispose(confirmationsReport); // report has been streamed, it is safe to dispose it
            return null;
        }
    }
}