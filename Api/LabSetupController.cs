// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LabSetupController.cs" company="Eyefinity, Inc.">
// Copyright © 2015 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
// Defines the LabSetupController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Insurance;

    using log4net;

    [NoCache]
    [ValidateHttpAntiForgeryToken]
    [Authorize]
    public class LabSetupController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LabSetupController));

        private readonly LabsIt2Manager labsIt2Manager;

        private readonly LabSetupManager labsManager;

        public LabSetupController(LabSetupManager labsManager, LabsIt2Manager labsIt2Manager)
        {
            this.labsManager = labsManager;
            this.labsIt2Manager = labsIt2Manager;
        }

        public LabSetupController()
        {
            this.labsManager = new LabSetupManager();
            this.labsIt2Manager = new LabsIt2Manager();
        }

        [HttpPost]
        public HttpResponseMessage SaveLabSetupMapping(string officeNumber, IEnumerable<LabSetupSearchResults> mappings)
        {
            var planNames = new List<string>();
            AccessControl.VerifyUserAccessToMultiLocationOffice(officeNumber);
            var gedi = new List<string>();
            int userId = new AuthorizationTicketHelper().GetUserInfo().Id;
            foreach (var mapping in mappings)
            {
                if (mapping.IsMapped)
                {
                    this.labsIt2Manager.MapLabsToCompany(officeNumber, mapping);
                }
                else
                {
                    this.labsIt2Manager.UnmapLabsToCompany(officeNumber, mapping);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, planNames);
        }

        [HttpGet]
        public HttpResponseMessage SearchLabNames(string officeNumber, string labName = null, bool mappedOnly = false, string state = null)
        {
            IList<LabSetupSearchResults> results;
            try
            {
                AccessControl.VerifyUserAccessToMultiLocationOffice(officeNumber);
                results = LabSetupManager.SearchLabNames(labName, officeNumber, state, mappedOnly);
            }
            catch (Exception ex)
            {
                string msg = "Search Lab Setup error: " + "\n" + ex;
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, results);
        }
    }
}