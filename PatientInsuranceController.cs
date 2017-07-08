namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Integrations;
    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Insurance;
    using Eyefinity.PracticeManagement.Model.Patient;
    using Eyefinity.PracticeManagement.Model.ViewModel;
    using IT2.Ioc;
    using IT2.Services;

    using CarrierInformation = Eyefinity.PracticeManagement.Model.Insurance.CarrierInformation;
    using PatientInsurance = Eyefinity.PracticeManagement.Model.Patient.PatientInsurance;

    /// <summary>
    /// The patient insurance controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientInsuranceController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly PatientAddressIt2Manager it2Business;

        /// <summary>
        /// The patient insurance manager.
        /// </summary>
        private readonly PatientInsuranceManager patientInsuranceManager;
      
        /// <summary> The transaction services. </summary>
        private readonly IPatientServices patientServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientInsuranceController"/> class.
        /// </summary>
        public PatientInsuranceController()
        {
            this.it2Business = new PatientAddressIt2Manager();
            this.patientInsuranceManager = new PatientInsuranceManager();

            // TODO: This needs a better design. PatientInsuranceManager can't make its own InsuranceManager
            // or even reference once because PatientInsuranceManager is in Eyefinity.Enterprise.Business and 
            // InsuranceManager is in Eyefinity.PracticeManagement.Business which already has a dependency 
            // on Eyefinity.Enterprise.Business (it would make a circular reference)
            this.patientInsuranceManager.GetMappedVspPlanIdsForOffice = officenum => new InsuranceManager()
                .SearchManualInsurancePlans("VSP", officenum, null, true)
                .Select(plan => plan.PlanID);

            this.patientServices = Container.Resolve<IPatientServices>();
        }

        /// <summary>The get patient insurances.</summary>
        /// <param name="patientId">The patient id.</param>
        /// <param name="activeOnly">The active Only.</param>
        /// <returns>The Insurance List.</returns>
        public HttpResponseMessage GetPatientInsurances(int patientId, bool activeOnly)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                return Request.CreateResponse(HttpStatusCode.OK, this.patientInsuranceManager.GetPatientInsurances(patientId, activeOnly));
            }
            catch (Exception ex)
            {
                var error = "GetPatientInsurances(" + string.Format("{0}, {1}", patientId, activeOnly) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <param name="patientId">
        /// The patient Id.
        /// </param>
        /// <param name="insuranceId">
        /// The insurance Id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetPatientInsuranceById(string officeNumber, int patientId, int insuranceId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                var ins = this.patientInsuranceManager.GetPatientInsuranceById(officeNumber, patientId, insuranceId);
                if (insuranceId > 0)
                {
                    ins.Eligibilities = this.patientInsuranceManager.GetEligibilitiesAndConfigurations(insuranceId);
                }

                return Request.CreateResponse(HttpStatusCode.OK, ins);
            }
            catch (Exception ex)
            {
                var error = "GetPatientInsuranceById(" + string.Format("{0}, {1}, {2}", officeNumber, patientId, insuranceId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get carrier plans lookup.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <param name="carrierId">
        /// The carrier id.
        /// </param>
        /// <returns>
        /// The list of plans for the carrier.
        /// </returns>
        public IEnumerable<Lookup> GetCarrierPlansLookup(string officeNumber, string carrierId)
        {
            return this.patientInsuranceManager.GetCarrierPlansLookup(officeNumber, carrierId);
        }

        /// <summary>
        /// The get carrier information.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="carrierId">
        /// The carrier id.
        /// </param>
        /// <returns>
        /// The <see cref="CarrierInformation"/>.
        /// </returns>
        public CarrierInformation GetCarrierInformation(string officeNumber, string carrierId)
        {
            return this.patientInsuranceManager.GetCarrierInformation(officeNumber, carrierId);
        }

        /// <summary>The get unknown carrier info VM.</summary>
        /// <param name="patientId">The patient id.</param>
        /// <param name="planId">The plan id.</param>
        /// <returns>The <see cref="UnknownCarrierInfoVm"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetUnknownCarrierInfoVm(int patientId, int planId)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, this.patientInsuranceManager.GetUnknownCarrierInfoVm(patientId, planId));
            }
            catch (Exception ex)
            {
                var error = "GetUnknownCarrierInfoVm(" + string.Format("{0}, {1}", patientId, planId) + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>The get PMI member info.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="patientId">The patient id.</param>
        /// <param name="insuranceId">The patient insurance id.</param>
        /// <returns>The <see cref="GetPmiMemberInfo"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetPmiMemberInfo(string officeNumber, int patientId, int insuranceId)
        {
            try
            {
                var errorMsg = string.Empty;
                var manager = new PmiIt2IntegrationManager();
                var pmiMemberInfo = manager.GetPmiMemberInfo(officeNumber, patientId, insuranceId, out errorMsg);
                if (errorMsg != string.Empty)
                {
                    errorMsg = FormatErrorMessage(errorMsg);
                }

                var arrList = new List<PatientPmiMemberInfo>[2];
                arrList[0] = new List<PatientPmiMemberInfo>();
                arrList[1] = new List<PatientPmiMemberInfo>();
                foreach (var t in pmiMemberInfo)
                {
                    if (t.IsDefaultAuthorization)
                    {
                        arrList[0].Add(t);
                    }
                    else
                    {
                        arrList[1].Add(t);
                    }
                }

                return errorMsg == string.Empty
                    ? Request.CreateResponse(HttpStatusCode.OK, arrList)
                    : Request.CreateResponse(HttpStatusCode.BadRequest, errorMsg);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("GetPmiMemberInfo(officeNumber = {0}, patientId = {1}, insuranceId = {2} {3} {4}", officeNumber, patientId, insuranceId, "\n", ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>The retrieving existing VSP authorization.</summary>
        /// <param name="request">The VSP info.</param>
        /// <returns>The <see cref="RetrieveExistingVspAuthorization"/>.</returns>
        [HttpGet]
        public HttpResponseMessage RetrieveExistingVspAuthorization([FromUri]PatientPmiRequestParameters request)
        {
            PatientPmiMemberInfo eligInfo = null;
            var errorMsg = string.Empty;
            try
            {
                var manager = new PmiIt2IntegrationManager();
                eligInfo = manager.RetrieveExistingVspAuthorization(request);

                if (eligInfo != null && eligInfo.ErrorMessage != null)
                {
                    errorMsg = FormatErrorMessage(eligInfo.ErrorMessage);
                    eligInfo.ErrorMessage = errorMsg;
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }

            if (errorMsg == string.Empty)
            {
				this.VerifyAndUpdateMemberId(request.PatientInsuranceId, request.PatientId, request.OfficeNum, eligInfo?.MembershipId);
                return this.Request.CreateResponse(HttpStatusCode.OK, eligInfo);
            }

            Logger.Error(request.GetErrorString(errorMsg));
            if (eligInfo == null)
            {
                eligInfo = new PatientPmiMemberInfo { ErrorMessage = errorMsg };
            }

            return Request.CreateResponse(HttpStatusCode.OK, eligInfo);
        }

        /// <summary>The get PMI member info.</summary>
        /// <param name="request">The VSP info.</param>
        /// <returns>The <see cref="GetPmiAuthorization"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetPmiAuthorization([FromUri]PatientPmiRequestParameters request)
        {
            PatientPmiMemberInfo eligInfo = null;
            var errorMsg = string.Empty;
            try
            {
                var manager = new PmiIt2IntegrationManager();
                eligInfo = manager.GetPmiAuthorization(request);
                if (eligInfo != null && eligInfo.ErrorMessage != null)
                {
                    errorMsg = FormatErrorMessage(eligInfo.ErrorMessage);
                    eligInfo.ErrorMessage = errorMsg;
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.ToString();
            }

            if (errorMsg == string.Empty)
            {
				this.VerifyAndUpdateMemberId(request.PatientInsuranceId, request.PatientId, request.OfficeNum, eligInfo?.MembershipId);
                return this.Request.CreateResponse(HttpStatusCode.OK, eligInfo);
            }
   
            Logger.Error(request.GetErrorString(errorMsg));
            if (eligInfo == null)
            {
                eligInfo = new PatientPmiMemberInfo { ErrorMessage = errorMsg };
            }

            return Request.CreateResponse(HttpStatusCode.OK, eligInfo);
        }

        /// <summary>
        /// Saves an Eligibility.
        /// </summary>
        /// <param name="saveEligibilityViewModel">The information to save.</param>
        /// <returns>Saved eligibility with identifiers generated.</returns>
        [HttpPost, HttpPut]
        public HttpResponseMessage SaveEligibility(SaveEligibilityViewModel saveEligibilityViewModel)
        {
            try
            {
                saveEligibilityViewModel.EmployeeId = Convert.ToInt32(this.User.Identity.Name);
                var validationErrors = this.patientInsuranceManager.SaveEligibility(saveEligibilityViewModel);
                if (string.IsNullOrEmpty(validationErrors))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, saveEligibilityViewModel);
                }

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, validationErrors);
            }
            catch (Exception ex)
            {
                var error = string.Format("SaveEligibility(employeeId = {0}, insuranceId = {1} {2} {3}", saveEligibilityViewModel.EmployeeId, saveEligibilityViewModel.InsuranceId, ")\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// Deletes an Eligibility.
        /// </summary>
        /// <param name="insuranceId">The Insurance ID.</param>
        /// <param name="eligibilityId">The Eligibility ID.</param>
        /// <param name="officeNumber">The office number.</param>
        /// <returns>HTTP response message.</returns>
        [HttpDelete]
        public HttpResponseMessage DeleteEligibility([FromUri]int insuranceId, [FromUri]int eligibilityId, [FromUri]string authorizationNumber, [FromUri]string officeNumber)
        {
            try
            {
                var validationMessage = this.patientInsuranceManager.DeleteEligibility(insuranceId, eligibilityId, officeNumber);
                if (string.IsNullOrEmpty(validationMessage))
                {
					////This is needed to understand who is requesting delete authorization requests
	                Logger.Error($"AN EPM USER {User.Identity.Name} REQUESTED TO DELETE AUTHORIZATION NUMBER {authorizationNumber} FOR INSURANCE ID {insuranceId}, ELIGIBILITY ID {eligibilityId} FOR OFFICENUMBER {officeNumber}");
					return Request.CreateResponse(HttpStatusCode.OK, "OK");
                }
				
                return Request.CreateResponse(HttpStatusCode.BadRequest, validationMessage);
            }
            catch (Exception ex)
            {
                var error = string.Format("DeleteEligibility(eligibilityId = {0}, insuranceId = {1} {2} {3}", eligibilityId, insuranceId, ")\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get all carrier with plans list.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// The list of carriers with plans.
        /// </returns>
        [HttpGet]
        public List<Lookup> GetAllCarrierWithPlansList(string officeNumber)
        {
            var carrierList = this.patientInsuranceManager.GetCarriersWithPlansLookup(officeNumber);

            return carrierList.Select(lookup => new Lookup(lookup.KeyStr, lookup.Description)).ToList();
        }

        /// <summary>The put.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="patientInsurance">The patient insurance.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public HttpResponseMessage Put(string officeNumber, [FromBody] PatientInsurance patientInsurance, int userId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientInsurance.PatientId);
                if (patientInsurance.PlanId == 0 || patientInsurance.InsuranceCarrier == null || string.IsNullOrEmpty(patientInsurance.InsuranceCarrier.Code))
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                var isNew = patientInsurance.Id == 0;
                var id = this.patientInsuranceManager.SavePatientInsurance(officeNumber, patientInsurance, userId);
                patientInsurance.Id = id;
                SavePatientInsuranceExtension(patientInsurance, isNew);
                patientInsurance.Carriers = this.patientInsuranceManager.GetPatientInsurancesForScheduler(patientInsurance.PatientId, true);
                return Request.CreateResponse(HttpStatusCode.OK, patientInsurance);
            }
            catch (Exception ex)
            {
                var error = string.Format("Put(userId = {0}, insuranceId = {1} {2} {3}", userId, patientInsurance.Id, ")\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The save patient insurances.
        /// </summary>
        /// <param name="insurances">
        /// The insurances.
        /// </param>
        /// <param name="patientId">
        /// The patient Id.
        /// </param>
        /// <returns>No Content</returns>
        public HttpResponseMessage SavePatientInsurances(List<PatientInsurance> insurances, int patientId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                var alslInsurances = new List<PatientInsuranceAlsl>();
                if (insurances != null)
                {
                    alslInsurances.AddRange(insurances.Select(item => new PatientInsuranceAlsl
                                                                          {
                                                                              PatientId = patientId,
                                                                              PatientInsuranceId = item.Id,
                                                                              IsActive = item.IsActive,
                                                                              IsPrimaryInsurance = item.IsPrimaryInsurance
                                                                          }));
                }

                PatientManager.SavePatientInsuranceAlsl(alslInsurances);
                new PatientRelationshipsIt2Manager().SaveDependentInsuranceExtensions(patientId, alslInsurances);
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var error = string.Format("SavePatientInsurances(patientId = {0} {1} {2}", patientId, ")\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="patientId"> Patient ID </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage Delete(int patientId, int id)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                if (id != 0)
                {
                    // Delete Insurance
                    // Insurance can not be deleted if it has been used in a previous transaction or claim
                    if (this.patientInsuranceManager.CanDeletePatientInsurance(id))
                    {
                        var relationshipsIt2Manager = new PatientRelationshipsIt2Manager();
                        var matchingDependentInsurances = relationshipsIt2Manager.GetMatchingDependentInsurances(patientId, id);
                        //// Delete Dependents Insurance
                        if (matchingDependentInsurances != null && matchingDependentInsurances.Any())
                        {
                            foreach (var dependentInsurance in matchingDependentInsurances)
                            {
                                if (this.patientInsuranceManager.CanDeletePatientInsurance(dependentInsurance.ID))
                                {
                                    PatientManager.DeletePatientInsuranceAlsl(dependentInsurance.ID);
                                    this.patientInsuranceManager.DeletePatientInsuranceByPatientInsuranceId(dependentInsurance.PatientID.GetValueOrDefault(), dependentInsurance.ID);
                                }
                                else
                                {
                                    //// Inactivate dependent insurance as it can't be deleted
                                    var dependentPatientInsuranceExtension = PatientManager.GetPatientInsuranceExtension(dependentInsurance.PatientID.GetValueOrDefault(), dependentInsurance.ID);
                                    dependentPatientInsuranceExtension.IsActive = false;
                                    var alslInsurances = new List<PatientInsuranceAlsl> { dependentPatientInsuranceExtension };
                                    PatientManager.SavePatientInsuranceAlsl(alslInsurances);
                                }
                            }
                        }

                        //// Delete Parent Insurance
                        PatientManager.DeletePatientInsuranceAlsl(id);
                        this.patientInsuranceManager.DeletePatientInsuranceByPatientInsuranceId(patientId, id);
                        return new HttpResponseMessage(HttpStatusCode.NoContent);
                    }

                    const string Message = "The insurance carrier/plan has associated eligibilities and cannot be deleted. Instead of deleting the insurance carrier/plan, you can inactivate it.";
                    return Request.CreateResponse(HttpStatusCode.Conflict, Message);
                }

                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                var error = string.Format("Delete({0}, {1}{2} {3}", patientId, id, ")\n", ex);
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>Gets the eligibilities and configuration for a given insurance.</summary>
        /// <param name="insuranceId">Id of the Patient Insurance</param>
        /// <returns>The eligibilities and configuration</returns>
        public EligibilitiesAndConfiguration GetEligibilities(int insuranceId)
        {
            return this.patientInsuranceManager.GetEligibilitiesAndConfigurations(insuranceId);
        }

        /// <summary>
        /// Gets the eligibilities.
        /// </summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="patientId">The patient identifier.</param>
        /// <returns>Insurance and eligibilities</returns>
        public HttpResponseMessage GetEligibilities(string officeNumber, int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            var insurances = this.patientInsuranceManager.GetPatientInsurances(patientId, true);
            foreach (var ins in insurances)
            {
                ins.Eligibilities = this.GetEligibilities(ins.Id);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { insurances });
        }

        /// <summary>
        /// Gets the eligibilities.
        /// </summary>
        /// <param name="officeNumber"></param>
        /// <param name="insuranceId"></param>
        /// <param name="patientId">The patient identifier.</param>
        /// <returns>Insurance and eligibilities</returns>
        public HttpResponseMessage GetEligibilitiesByInsuranceId(string officeNumber, int insuranceId, int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            var result = this.patientInsuranceManager.GetEligibilitiesByInsuranceId(officeNumber, insuranceId);
            return this.Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Gets inactive insurances.
        /// </summary>
        /// <param name="patientId">The patient id.</param>
        /// <returns>Inactive Insurance id</returns>
        public IEnumerable<int> GetInactiveInsurances(int patientId)
        {
            AccessControl.VerifyUserAccessToPatient(patientId);
            return PatientManager.GetPatientInsuranceExtensions(patientId)
                .Where(i => !i.IsActive)
                .Select(i => i.PatientInsuranceId);
        }

        /// <summary>The AddVspInsurancePlanFromSubscriber.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="subscriber">The subscriber insurance.</param>
        /// <param name="patientId">The patient id.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPut]
        public HttpResponseMessage AddVspInsurancePlanFromSubscriber(string officeNumber, [FromBody] InsuranceSubscriber subscriber, int patientId, int userId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                int selectedInsurance;
                var addedCount = this.patientInsuranceManager.AddVspInsurancesToPatientThatIsNotTheMember(officeNumber, patientId, userId, out selectedInsurance, subscriber);
                var insuranceList = this.patientInsuranceManager.GetPatientInsurancesForScheduler(patientId, true);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { error = string.Empty, addedCount = addedCount, insuranceList = insuranceList, selectedInsurance = selectedInsurance });
            }
            catch (Exception ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { error = ex.Message, addedCount = 0 });
            }
        }

        /// <summary>Get patient VSP insurance plans.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="parameters">The parms.</param>
        /// <returns>VSP insurance plans.</returns>
        [HttpPost]
        public HttpResponseMessage AddVspInsurancePlansToMemberPatient(string officeNumber, AddVspInsurancePlansToMemberPatientParameters parameters)
        {
            var patientId = parameters.PatientId;
            var ssn = parameters.Ssn;

            AccessControl.VerifyUserAccessToPatient(patientId);

            var userId = new AuthorizationTicketHelper().GetUserInfo().Id;

            try
            {
                int selectedInsurance;
                var addedCount = this.patientInsuranceManager.AddVspInsurancesToPatientThatIsTheMember(officeNumber, patientId, userId, out selectedInsurance, ssn);
                var insuranceList = this.patientInsuranceManager.GetPatientInsurancesForScheduler(patientId, true);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { error = string.Empty, addedCount = addedCount, insuranceList = insuranceList, selectedInsurance = selectedInsurance });
            }
            catch (ApplicationException ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { error = ex.Message, addedCount = 0 });
            }
        }

        /// <summary>The get print labels content.</summary>
        /// <param name="authorizeNumber">The authorize Number.</param>
        /// <returns>The <see cref="byte"/>.</returns>
        [NonAction]
        public byte[] GetRecordReport(string authorizeNumber)
        {
            var manager = new PmiIt2IntegrationManager();
            return manager.GetRecordReport(authorizeNumber);
        }

        /// <summary>The save patient insurance extension.</summary>
        /// <param name="patientInsurance">The patient insurance.</param>
        /// <param name="isNew">indicate whether it is new.</param>
        private static void SavePatientInsuranceExtension(PatientInsurance patientInsurance, bool isNew)
        {
            if (isNew)
            {
                patientInsurance.IsActive = true;
            }

            var ext = new PatientInsuranceAlsl
                      {
                          PatientId = patientInsurance.PatientId,
                          PatientInsuranceId = patientInsurance.Id,
                          IsActive = patientInsurance.IsActive,
                          IsPrimaryInsurance = patientInsurance.IsPrimaryInsurance
                      };
            var alslInsurances = new List<PatientInsuranceAlsl> { ext };

            PatientManager.SavePatientInsuranceAlsl(alslInsurances);
        }

        /// <summary>The format error message.</summary>
        /// <param name="errorMsg">The error message.</param>
        /// <returns>The <see cref="string"/>result.</returns>
        private static string FormatErrorMessage(string errorMsg)
        {
            var result = errorMsg;
            var index = errorMsg.IndexOf("|PLmpg=", StringComparison.Ordinal);
            if (index > 0)
            {
                result = errorMsg.Substring(0, index);
            }

            result = result.Replace(" =", " ")
                           .Replace("|", string.Empty)
                           .Replace("Eld=", string.Empty);
            return result;
        }

	    private void VerifyAndUpdateMemberId(int patientInsuranceId, int patientId, string officeNumber, string memberId)
	    {
		    try
		    {
			    if (patientInsuranceId <= 0 || memberId.Length <= 4)
			    {
				    return;
			    }

			    var companyEncryption = new Security().GetCompanyEncryption(string.Empty, officeNumber);
			    var ins = this.patientInsuranceManager.GetPatientInsuranceById(officeNumber, patientId, patientInsuranceId);
			    if (ins == null)
			    {
				    return;
			    }

			    if (ins.InsuredId.Length > 4)
			    {
				    return;
			    }

			    ins.InsuredId = memberId;
			    this.patientInsuranceManager.SavePatientInsurance(officeNumber, ins, Convert.ToInt32(this.User.Identity.Name));
			    Logger.Error($"UPDATED MEMBER/SUBSCRIBER ID FOR INSURANCE ID {patientInsuranceId} PATIENT ID {patientId} FOR OFFICENUMBER {officeNumber}");
		    }
		    catch (Exception)
		    {
			    //// ignore the exception as we don't want to stop the process of pulling eligibility/authorization just because we can't update member id
		    }
	    }
    }
}
