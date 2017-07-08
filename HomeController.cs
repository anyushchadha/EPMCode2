// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HomeController.cs" company="Eyefinity, Inc.">
//    Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//  Defines the HomeController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Appointment;
    using Eyefinity.Enterprise.Business.Home;
    using Eyefinity.Enterprise.Business.Integrations;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;

    using Anonymous = Eyefinity.PracticeManagement.Common.Api.AllowAnonymousAttribute;

    /// <summary>The login controller.</summary>
    [NoCache]
    [ValidateHttpAntiForgeryToken]
    public class HomeController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The message center helper.
        /// </summary>
        private readonly MessageCenterHelper msgCenterHelper;

        /// <summary>
        /// The message dashBoard manager.
        /// </summary>
        private readonly DashBoardManager dashBoardManager;

        /// <summary>
        ///     The appointment manager.
        /// </summary>
        private readonly AppointmentManager appointmentManager;

        /// <summary>
        ///     The logiIntegration manager.
        /// </summary>
        private readonly LogiIntegrationManager logiIntegrationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        public HomeController()
        {
            this.msgCenterHelper = new MessageCenterHelper();
            this.dashBoardManager = new DashBoardManager();
            this.appointmentManager = new AppointmentManager();
            this.logiIntegrationManager = new LogiIntegrationManager();
        }

        [HttpGet]
        [Anonymous]
        public HttpResponseMessage LoadOrUpdateOfficeDashboard(string officeNum, string companyId)
        {
            var resourceIDs = string.Empty;
            try
            {
                var common = new Business.Admin.Common();
                var authorizationTicketHelper = new AuthorizationTicketHelper();
                var userOfficeId = Convert.ToInt32(authorizationTicketHelper.GetOfficeId());
                var user = authorizationTicketHelper.GetUserInfo();
                var employeeId = this.appointmentManager.GetEmployeeIdByUserId(user.Id).GetValueOrDefault();
                var resources = this.appointmentManager.GetResources(userOfficeId, (int)employeeId).ToList();
                resourceIDs = resources.Aggregate(resourceIDs, (current, resource) => current + (resource.Id.GetValueOrDefault() + ","));
                resourceIDs = resourceIDs.Substring(0, resourceIDs.Length - 1);
                common.LoadOrUpdateOfficeDashboard(officeNum, companyId, resourceIDs);
            }
            catch (Exception e)
            {
                Logger.Error("HomeController:LoadOrUpdateOfficeDashboard(): " + e);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, "OK");
        }

        [HttpGet]
        [Anonymous]
        public HttpResponseMessage LoadOrUpdateCompanyDashboard(string companyId)
        {
             var resourceIDs = string.Empty;
            try
            {
                var common = new Business.Admin.Common();
                common.LoadOrUpdateCompanyDashboard(companyId);
            }
            catch (Exception e)
            {
                Logger.Error("HomeController:LoadOrUpdateCompanyDashboard(): " + e);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, "OK");
        }

        [HttpGet]
        [Anonymous]
        public HttpResponseMessage GetCompanyDashBoard(string companyId)
        {
            var content = new CompanyDashboard();
            try
            {
                content = this.dashBoardManager.GetCompanyDashboardByCompanyId(companyId);
            }
            catch (Exception e)
            {
                Logger.Error("HomeController:GetCompanyDashBoard(): " + e);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, content);
        }

        [HttpGet]
        [Anonymous]
        public HttpResponseMessage GetOfficeDashBoard(string officeNumber)
        {
            var content = new OfficeDashboard();
            try
            {
                content = this.dashBoardManager.GetOfficeDashboardByOfficeNum(officeNumber);
            }
            catch (Exception e)
            {
                Logger.Error("HomeController:GetOfficeDashBoard(): " + e);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, content);
        }

        [HttpGet]
        [Anonymous]
        public HttpResponseMessage GetUserRoles()
        {
            bool hasDashBoardRole = false;
            try
            {
                var authorizationTicketHelper = new AuthorizationTicketHelper();
                var user = authorizationTicketHelper.GetUserInfo();
                var userId = user.Id;
                var isLogiEnabled = this.logiIntegrationManager.GetLogiFeatureAvailability(user.CompanyId);
                if (!isLogiEnabled.IsAvailable)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, false);
                }
          
                var content = new Security().GetUserRoles(userId, true).ToList().Find(a => a.Key == 66 || a.Key == 67 || a.Key == 63);
                if (content != null)
                {
                    hasDashBoardRole = true;
                }
            }
            catch (Exception e)
            {
                Logger.Error("HomeController:GetUserRoles(): " + e);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, hasDashBoardRole);
        }

        [HttpPut]
        public HttpResponseMessage CompleteOfficeDashboardSetupForModule([FromUri]string officeNumber, [FromUri]string chkBoxName)
        {
            var content = new OfficeDashboard();

            try
            {
                this.dashBoardManager.CompleteOfficeDashboardSetupForModule(officeNumber, chkBoxName);
                content = this.dashBoardManager.GetOfficeDashboardByOfficeNum(officeNumber);
            }
            catch (Exception e)
            {
                Logger.Error("HomeController:CompleteOfficeSetupForModule(): " + e);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, content);
        }

        [HttpPut]
        public HttpResponseMessage CompleteCompanyDashboardSetupForModule([FromUri]string companyId, [FromUri]string chkBoxName)
        {
            var content = new CompanyDashboard();

            try
            {
                this.dashBoardManager.CompleteCompanyDashboardSetupForModule(companyId, chkBoxName);
                content = this.dashBoardManager.GetCompanyDashboardByCompanyId(companyId);
            }
            catch (Exception e)
            {
                Logger.Error("HomeController:CompleteCompanyDashboardSetupForModule(): " + e);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, content);
        }

        [HttpPost]
        [Anonymous]
        public List<MessageCenter> GetMessageCenterCmsContent()
        {
            var content = new List<MessageCenter>();
            try 
            {
                // get the url from properties, then connect and retrieve data
                var url = ConfigurationManager.AppSettings["MagnoliaURL_MessageCenter"];
                content = this.msgCenterHelper.GetMessageCenterCmsMessaging(url);
            }  
            catch (Exception e)
            {
                Logger.Error("HomeController:GetMessageCenterCmsContent(): " + e);
            }

            return content;
        }

        [HttpPost]
        [Anonymous]
        public HttpResponseMessage GetLogiSession(string officeNum, string companyId)
        {
            var content = new LogiDashboard();
            try
            {
                AccessControl.VerifyUserAccessToMultiLocationOffice(officeNum);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            try
            {
                // get the url from properties, then connect and retrieve data
                var authorizationTicketHelper = new AuthorizationTicketHelper();
                var user = authorizationTicketHelper.GetUserInfo();
                var userId = user.Id;
                content = this.logiIntegrationManager.GetLogiSession(companyId, userId);
            }
            catch (Exception e)
            {
                Logger.Error("HomeController:GetLogiSession(): " + e);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, content);
        }
    }
}