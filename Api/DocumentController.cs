// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the DocumentController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Common;
    using Eyefinity.Enterprise.Business.Miscellaneous;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;

    using log4net;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The document controller.
    /// </summary>
    [NoCache]
    [ValidateHttpAntiForgeryToken]
    [Authorize]
    public class DocumentController : ApiController
    {
        /// <summary>The logger.</summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DocumentController));
        private readonly string companyId;
        private readonly int userId;

        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly DocumentIt2Manager it2Business;

        /////// <summary>
        /////// The PAtient Manager
        /////// </summary>
        ////private readonly new Business.Patient.PatientManager patientManager;
         
        /// <summary>Initializes a new instance of the <see cref="DocumentController"/> class.</summary>
        public DocumentController()
        {
            this.it2Business = new DocumentIt2Manager();
            var user = new AuthorizationTicketHelper().GetUserInfo();
            this.companyId = user.CompanyId;
            this.userId = user.Id;
        }

        /// <summary>The upload.</summary>
        /// <param name="doc">The doc.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPost]
        public HttpResponseMessage Upload(Document doc)
        {
            bool result;
            var errorMsg = string.Format("An error was encountered while uploading this document.  Please try again. <br /> <br />If you continue to receive this error call Eyefinity Customer Care."); 
            try
            {
                AccessControl.VerifyUserAccessToPatient(doc.PatientId);
                var awsTemporaryStorage = new S3TemporaryDocumentStorage();
                var stream = awsTemporaryStorage.RetrieveTempDocument(this.companyId, doc.FilePath);
                result = this.it2Business.Upload(doc, stream, this.companyId, this.userId);
                awsTemporaryStorage.DeleteTempDocument(this.companyId, doc.FilePath);
            }
            catch (Exception ex)
            {
                var msg = "Patient document upload error: " + "\n" + ex;
                HandleExceptions.LogExceptions(msg, Logger, ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, errorMsg);
            }

            return result ? Request.CreateResponse(HttpStatusCode.OK, "Document saved.") : Request.CreateResponse(HttpStatusCode.BadRequest, errorMsg);
        }

        /// <summary>The get all documents.</summary>
        /// <param name="patientId">The patient id.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public HttpResponseMessage GetAllDocuments(int patientId)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(patientId);
            
                var documentlistlite = this.it2Business.GetPatientDocumentsLite(patientId);
                var documents = new List<Document>();
                if (documentlistlite.Count > 0)
                {
                    documents = (from item in documentlistlite
                                                orderby item.CreatedDate, item._EmployeeName, item.DocumentType descending
                                                select new Document
                                                       {
                                                            UserId = 0,
                                                            PatientId = patientId,
                                                            Id = item.ID,
                                                            Date = item.CreatedDate.ToString("MM/dd/yyyy"),
                                                            EnteredBy = item.Employee.EmployeeName,
                                                            DocumentName = item.ImageName,
                                                            DocumentType = item.DocumentType,
                                                            Comments = item.Comments,
                                                            FilePath = string.Empty,
                                                            DocumentFormat = item.DocumentFormat.ToString()
                                                }).ToList();
                }

                return Request.CreateResponse(HttpStatusCode.OK, documents);
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetAllDocuments(patientid = {0} {1} {2}", patientId, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>The get document type.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        public IEnumerable<Lookup> GetDocumentType()
        {
            var tocs = this.it2Business.GetDocumentType();
            var documentType = (from item in tocs
                                orderby item.DocumentDescription
                                select new Lookup(item.ID, item.DocumentDescription)).ToList();

            return documentType;
        }

        /// <summary>The delete.</summary>
        /// <param name="id">The id.</param>
        /// <param name="patientId"></param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public HttpResponseMessage Delete(int id, int patientId)
        {
            var document = this.it2Business.GetImageById(this.companyId, patientId, id);
            try
            {
                if (document == null)
                {
                    AccessControl.VerifyUserAccessToPatient(patientId);
                }
                else
                {
                    AccessControl.VerifyUserAccessToPatient(document.PatientID);
                }

                var msg = this.it2Business.DeleteDocument(id, this.companyId, patientId);
                return Request.CreateResponse(HttpStatusCode.OK, msg);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Delete(id = {0} {1} {2}", id, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>The get document content.</summary>
        /// <param name="id">The id.</param>
        /// <param name="uploadsLocation">The uploads location.</param>
        /// <param name="patientId"></param>
        /// <returns>The <see cref="string"/>.</returns>
        [NonAction]
        public byte[] GetDocumentContent(int id, string uploadsLocation, int patientId)
        {
            var authorizationTicketHelper = new AuthorizationTicketHelper();
            var userOfficeId = Convert.ToInt32(authorizationTicketHelper.GetOfficeId());
            var user = authorizationTicketHelper.GetUserInfo();

            return this.it2Business.GetDocumentContent(id, uploadsLocation, this.companyId, patientId, user);
        }

        /// <summary>The create document.</summary>
        /// <param name="file">The file.</param>
        /// <param name="uploadsLocation">The uploads location.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [NonAction]
        public string CreateDocument(HttpPostedFileBase file)
        {
            return this.it2Business.CreateDocument(file, this.companyId);
        }
    }
}