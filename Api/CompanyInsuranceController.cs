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
    using IT2.Ioc;
    using IT2.Services;
    using log4net;
    using Model.Insurance;

    [NoCache]
    [ValidateHttpAntiForgeryToken]
    [Authorize]
    public class CompanyInsuranceController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CompanyInsuranceController));

        private readonly InsuranceIt2Manager insuranceIt2Manager;

        private readonly InsuranceManager insuranceManager;

        private readonly IOfficeServices officeServices;

        public CompanyInsuranceController(InsuranceManager insuranceManager, InsuranceIt2Manager insuranceIt2Manager, IOfficeServices officeServices)
        {
            this.insuranceManager = insuranceManager;
            this.insuranceIt2Manager = insuranceIt2Manager;
            this.officeServices = officeServices;
        }

        public CompanyInsuranceController()
        {
            this.insuranceManager = new InsuranceManager();
            this.insuranceIt2Manager = new InsuranceIt2Manager();
            this.officeServices = Container.Resolve<IOfficeServices>();
        }

        [HttpPost]
        public HttpResponseMessage SaveCompanyInsuranceMapping(string officeNumber, string companyId, IEnumerable<InsurancePlanSearchResult> mappings)
        {
            var planNames = new List<string>();
            try
            {
                var gedi = new List<string>();
                var userId = new AuthorizationTicketHelper().GetUserInfo().Id;
                AccessControl.VerifyUserAccessToCompany(companyId);
                foreach (var mapping in mappings)
                {
                   try
                    {
                        if (mapping.ElectronicClaimSubmissionToGedi != mapping.EClaimSubmission)
                        {
                            if (!gedi.Contains(mapping.CarrierName))
                            {
                                gedi.Add(mapping.CarrierName);
                            }
                        }

                        if (mapping.IsMapped)
                        {
                            ////[MapInsurancePlan] takes care of mapping this for all the offices
                            this.insuranceIt2Manager.MapInsurancePlanToCompany(mapping.PlanID, officeNumber, userId);
                        }
                        else
                        {
                            ////[UnMapInsurancePlan] takes care of un-mapping this for all the offices
                            this.insuranceIt2Manager.UnMapInsurancePlanFromCompany(mapping.PlanID, officeNumber);
                        }

                        if (mapping.ElectronicClaimSubmissionToGedi)
                        {
                            this.insuranceIt2Manager.SetupGedi(companyId, userId, mapping);
                        }
                        else
                        {
                            this.insuranceIt2Manager.RemoveGedi(companyId, userId, mapping);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"SaveCompanyInsuranceMapping failed for office {officeNumber} carrier {mapping.CarrierName} plan {mapping.PlanName}", ex);
                    }
                }

                if (gedi.Count > 0)
                {
                    foreach (var g in gedi)
                    {
                        var plan = this.insuranceManager.SearchManualInsurancePlans(g, officeNumber, null);
                        var results = plan.Where(x => x.PayerId != string.Empty && x.CarrierName == g).ToList();
                        if (results.Count > 1)
                        {
                            planNames.Add("@" + g);
                            planNames.AddRange(from t in results select t.PlanName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "SaveCompany Insurance Mapping error: " + "\n" + ex;
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, planNames);
        }

        [HttpGet]
        public HttpResponseMessage SearchManualInsurancePlans(string officeNumber, string companyId, string carrier = null, string payerId = null, bool mappedOnly = false)
        {
            IList<InsurancePlanSearchResult> results;
            try
            {
                AccessControl.VerifyUserAccessToCompany(companyId);
                results = this.insuranceManager.SearchManualInsurancePlans(carrier, officeNumber, payerId, mappedOnly);
            }
            catch (Exception ex)
            {
                string msg = "Search Manual Insurance Plans error: " + "\n" + ex;
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, results);
        }
    }
}