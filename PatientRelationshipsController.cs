// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatientRelationshipsController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   The patient relationships controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;
    using Eyefinity.PracticeManagement.Model.Patient;

    /// <summary>The patient relationships controller.</summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientRelationshipsController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>The get all dependents for the company.</summary>
        /// <param name="officeNumber"></param>
        /// <param name="patientId">The patient id.</param>
        /// <returns>The list of dependents and responsible party name.</returns>
        [HttpGet]
        public HttpResponseMessage GetAllDependents(string officeNumber, [FromUri]int patientId)
        {        
            var patientRelationshipsIt2Manager = new PatientRelationshipsIt2Manager();
            try
            {
                patientRelationshipsIt2Manager.GetAllDependents(patientId);
                foreach (var dependent in patientRelationshipsIt2Manager.InvalidDependents.Dependents)
                {
                    Logger.Error(
                        "Found a Patient with a Dependent that belongs to a different office. [CompanyId="
                        + officeNumber + ", PatientId=" + patientId + ", DependentCompanyId=" + dependent.CompanyId
                        + ", DependentPatientId=" + dependent.PatientId + "] ");
                }

                return Request.CreateResponse(HttpStatusCode.OK, patientRelationshipsIt2Manager.ValidDependents);
            }
            catch (Exception ex)
            {
                var error = "GetAllDependents( patientId=" + patientId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }
		
        /// <summary>The insert or update dependent.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="patientRelationships">The patient relationships.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPut]
        public HttpResponseMessage Put(string officeNumber, [FromBody]PatientRelationships patientRelationships)
        {
            var patientRelationshipsIt2Manager = new PatientRelationshipsIt2Manager();
            try
            {
                patientRelationshipsIt2Manager.InsertOrUpdateDependent(officeNumber, patientRelationships);
                return this.Request.CreateResponse(HttpStatusCode.OK, "Dependent saved.");
            }
            catch (Exception ex)
            {
                var error = "Put( patientId=" + patientRelationships.PatientId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>The delete.</summary>
        /// <param name="relationshipsObj">The relationships object.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpDelete]
        public HttpResponseMessage Delete([FromBody]PatientRelationships relationshipsObj)
        {
            var patientRelationshipsIt2Manager = new PatientRelationshipsIt2Manager();
            try
            {
                var result = patientRelationshipsIt2Manager.DeleteDependent(relationshipsObj);
                return result
                    ? this.Request.CreateResponse(HttpStatusCode.OK, "Dependent deleted.")
                    : this.Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to delete this dependent.");
            }
            catch (Exception ex)
            {
                var error = "Put( patientId=" + relationshipsObj.PatientId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }
    }
}