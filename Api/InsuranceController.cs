// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InsuranceDetailsController.cs" company="Eyefinity, Inc.">
//    Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//  The InsuranceDetails controller.
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
    using Enterprise.Business.Patient;
    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;
    using IT2.Core;
    using Model;

    /// The InsuranceDetails controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class InsuranceController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly InsuranceDetailIt2Manager insuranceDetailManager = new InsuranceDetailIt2Manager();

        /// <summary>
        /// To get CarriersList.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// InsuranceDetails View model
        /// </returns>
        [HttpPost]
        public HttpResponseMessage SaveInsurancedetails(IEnumerable<InsuranceDetailsVm> insurancedetails, string officeNumber)
        {
            var insuranceVms = insurancedetails as InsuranceDetailsVm[] ?? insurancedetails.ToArray();
            this.insuranceDetailManager.Delete(insuranceVms.Where(h => h.IsDeleted));
            var returnVariable = this.insuranceDetailManager.SaveInsDetails(insuranceVms.Where(h => !h.IsDeleted).Where(h => h.Id <= -1), officeNumber);
            this.insuranceDetailManager.Delete(insuranceVms.Where(h => h.Id >= 0).Where(h => !h.IsDeleted));
            var returnVariable1 = this.insuranceDetailManager.SaveInsDetails(insuranceVms.Where(h => h.Id >= 0).Where(h => !h.IsDeleted), officeNumber);
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        public List<Lookup> GetAllCarrierWithPlansList(string officeNumber)
        {
            var carrierList = this.insuranceDetailManager.GetCarriersWithPlansLookup(officeNumber);

            return carrierList.Select(lookup => new Lookup(lookup.KeyStr, lookup.Description)).ToList();
        }

        public IEnumerable<InsuranceDetailsVm> GetInsDetailsData(string companyId, int employeeId)
        {
            return this.insuranceDetailManager.GetInsuranceDetailsData(companyId, employeeId)
                .Select(InsuranceDetailsVm.GetInsuranceDetails);
        }
    }
}