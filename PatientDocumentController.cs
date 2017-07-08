// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatientDocumentController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   The patient controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Eyefinity.Enterprise.Business.Miscellaneous;
    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using IT2.Core;
    using IT2.Ioc;
    using IT2.Services;

    /// <summary>
    ///     The patient document controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientDocumentController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The practice Id.
        /// </summary>
        private readonly string practiceLocationId = string.Empty;

        /// <summary>
        /// The patient ledger manager.
        /// </summary>
        private readonly PatientDocumentIt2Manager patientDocumentIt2Manager;

        /// <summary>
        /// The Document manager.
        /// </summary>
        private readonly DocumentIt2Manager documentManager;

        private readonly IMiscellaneousServices miscellaneousServices;
        private readonly ILookupServices lookupServices;
        private readonly IPatientServices patientServices;
        private readonly IPatientDocumentServices patientDocumentServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientDocumentController"/> class.
        /// </summary>
        /// <param name="patientDocumentIt2Manager">
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        public PatientDocumentController(
            ILookupServices lookupServices, 
            IMiscellaneousServices miscellaneousServices, 
            IPatientServices patientServices, 
            IPatientDocumentServices patientDocumentServices, 
            PatientDocumentIt2Manager patientDocumentIt2Manager, 
            DocumentIt2Manager documentIt2Manager, 
            string officeNumber)
        {
            this.lookupServices = lookupServices;
            this.miscellaneousServices = miscellaneousServices;
            this.patientServices = patientServices;
            this.patientDocumentServices = patientDocumentServices;
            this.patientDocumentIt2Manager = patientDocumentIt2Manager;

            this.documentManager = documentIt2Manager;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientController"/> class.
        /// </summary>
        public PatientDocumentController() 
        {
            this.lookupServices = Container.Resolve<ILookupServices>();
            this.miscellaneousServices = Container.Resolve<IMiscellaneousServices>();
            this.patientServices = Container.Resolve<IPatientServices>();
            this.patientDocumentServices = Container.Resolve<IPatientDocumentServices>();

            this.patientDocumentIt2Manager = new PatientDocumentIt2Manager(this.lookupServices, this.miscellaneousServices, this.patientServices, this.patientDocumentServices);
            this.documentManager = new DocumentIt2Manager();

            if (!this.User.Identity.IsAuthenticated)
            {
                return;
            }

            var authorizationTicketHelper = new AuthorizationTicketHelper();
            this.practiceLocationId = authorizationTicketHelper.GetPracticeLocationId();
        }

        [HttpGet]
        public HttpResponseMessage HasPatientDocument([FromBody] string officeNumber, int patientId)
        {
            return Request.CreateResponse(HttpStatusCode.OK, this.documentManager.HasPatientDocument(patientId, DocumentFormatType.JSON));
        }

        ////[HttpPut]
        [HttpGet]
        public HttpResponseMessage ImportPatientFromDocument(string officeNumber, int patientId)
        {
            var message = "Success";

            var user = new AuthorizationTicketHelper().GetUserInfo();

            var list = this.patientDocumentIt2Manager.ImportPatientFromDocument(officeNumber, patientId, user.Id);

            return Request.CreateResponse(HttpStatusCode.OK, message);
        }
    }
}