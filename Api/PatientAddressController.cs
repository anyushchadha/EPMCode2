// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatientAddressController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   The address controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;

    /// <summary>The address controller.</summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientAddressController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly PatientAddressIt2Manager it2Business;

        /// <summary>Initializes a new instance of the <see cref="PatientAddressController"/> class.</summary>
        public PatientAddressController()
        {
            this.it2Business = new PatientAddressIt2Manager();
        }

        /// <summary>The get.</summary>
        /// <param name="patientId">The patient id.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public HttpResponseMessage Get(int patientId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
                return Request.CreateResponse(HttpStatusCode.OK, this.it2Business.GetAddresses(patientId));
            }
            catch (Exception ex)
            {
                var msg = string.Format("Get(patientid = {0} {1} {2}", patientId, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>The get address type.</summary>
        /// <returns>The <see cref="IList"/>.</returns>
        public IEnumerable<Lookup> GetAddressType()
        {
            return this.it2Business.GetAddressType();
        }

        /// <summary>The get zip code.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        public IEnumerable<Lookup> GetZipCode(string id)
        {
            return this.it2Business.GetZipCodes(id);
        }

        /// <summary>The post.</summary>
        /// <param name="address">The address.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public HttpResponseMessage Post([FromBody]Model.Patient.PatientAddress address)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(address.PatientId);
                this.it2Business.SavePatientAddress(address);
                return Request.CreateResponse(HttpStatusCode.OK, "Address saved.");
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetAllRxByPatientId(patientid = {0} {1} {2}", address.PatientId, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>The put.</summary>
        /// <param name="address">The address.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public HttpResponseMessage Put([FromBody]Model.Patient.PatientAddress address)
        { 
            try
            {
                AccessControl.VerifyUserAccessToPatient(address.PatientId);
                var result = this.it2Business.UpdatePatientAddress(address);
                return result ? this.Request.CreateResponse(HttpStatusCode.OK, "Address saved.") : this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Zip.");
            }
            catch (Exception ex)
            {
                var msg = string.Format("Put(patientid = {0} {1} {2}", address.PatientId, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>The delete.</summary>
        /// <param name="address">The address.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public HttpResponseMessage Delete(Model.Patient.PatientAddress address)
        { 
            try
            {
                AccessControl.VerifyUserAccessToPatient(address.PatientId);
                var result = this.it2Business.DeletePatientAddress(address);
                return result ? this.Request.CreateResponse(HttpStatusCode.OK, "Address deleted.") : this.Request.CreateResponse(HttpStatusCode.BadRequest, "Unable to delete this address.");
            }
            catch (Exception ex)
            {
                var msg = string.Format("Delete(patientid = {0} {1} {2}", address.PatientId, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }
    }
}
