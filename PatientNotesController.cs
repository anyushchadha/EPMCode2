// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatientNotesController.cs" company="Eyefinity, Inc.">
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
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Patient;
    using Eyefinity.PracticeManagement.Business.Patient;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Patient;

    using iTextSharp.text;

    using Lookup = Eyefinity.PracticeManagement.Model.Lookup;
    using PatientNotes = Eyefinity.PracticeManagement.Model.Patient.PatientNotes;
    using PatientNotesSearchCriteria = Eyefinity.PracticeManagement.Model.Patient.PatientNotesSearchCriteria;

    /// <summary>The patient notes controller.</summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class PatientNotesController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly PatientNotesIt2Manager it2Business;

        /// <summary>Initializes a new instance of the <see cref="PatientNotesController"/> class.</summary>
        public PatientNotesController()
        {
            this.it2Business = new PatientNotesIt2Manager();
        }

        #region GET

        /// <summary>Returns a PatientNotes</summary>
        /// <param name="searchCriteria">The search Criteria.</param>
        /// <returns>The <see cref="PatientNotes"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetPatientNotesBySearchCriteria([FromUri]PatientNotesSearchCriteria searchCriteria)
        {
            try
            {
                AccessControl.VerifyUserAccessToPatient(searchCriteria.PatientId);
                var noteTypes = GetNoteTypes();

                IList<PatientNotesSearchResults> results = this.it2Business.GetSearchResults(searchCriteria);
                var resources = this.GetResources(results);
                return this.Request.CreateResponse(HttpStatusCode.OK, new PatientNotes { NoteTypes = noteTypes, Results = results, Resources = resources });
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (NHibernate.ObjectNotFoundException ex)
            {
                Logger.Error("GetPatientNotesBySearchCriteria: " + ex);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, "Note Not Found.");
            }
            catch (Exception ex)
            {
                var msg = "GetPatientNotesBySearchCriteria: " + ex;
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }   
         }

        /// <summary>Returns a PatientNotes</summary>
        /// <param name="searchCriteria">The search Criteria.</param>
        /// <returns>The <see cref="PatientNotes"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetPatientNotesBySearchCriteriaForOverview([FromUri]PatientNotesSearchCriteria searchCriteria)
        {
            try
            {
                if (searchCriteria != null)
                {
                    AccessControl.VerifyUserAccessToPatient(searchCriteria.PatientId);
                    var noteTypes = GetNoteTypes();
                    IList<PatientNotesSearchResults> results = this.it2Business.GetSearchResults(searchCriteria);

                    results = results.Where(a => a.Note != null).ToList();

                    foreach (var notes in results)
                    {
                        if (!string.IsNullOrEmpty(notes.Note) && notes.Note.Length > 150)
                        {
                            notes.Note = notes.Note.Substring(0, 150);
                        }
                    }

                    var resources = this.GetResources(results);
                    return this.Request.CreateResponse(
                        HttpStatusCode.OK,
                        new PatientNotes { NoteTypes = noteTypes, Results = results, Resources = resources });
                }

                return this.Request.CreateResponse(HttpStatusCode.NotFound, "Note Not Found.");
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (NHibernate.ObjectNotFoundException ex)
            {
                Logger.Error("GetPatientNotesBySearchCriteriaForOverview: " + ex);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, "Note Not Found.");
            }
            catch (Exception ex)
            {
                var msg = "GetPatientNotesBySearchCriteriaForOverview: " + ex;
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }
        
        /// <summary>Returns a Patient Note given an ID</summary>
        /// <param name="noteId">The note id.</param>
        /// <returns>The <see cref="PatientNotesSearchResults"/>.</returns>
        [HttpGet]
        public PatientNotesSearchResults GetPatientNoteById([FromUri]string noteId)
        {
            var notes = this.it2Business.GetPatientNoteById(noteId);
            AccessControl.VerifyUserAccessToPatient(notes.PatientId);
            return notes;
        }

        #endregion

        #region POST

        /// <summary>Create a new note.</summary>
        /// <param name="noteObj">The note.</param>
        /// <returns>No Content</returns>
        [HttpPost]
        public HttpResponseMessage AddNote([FromBody]PatientNotesSearchResults noteObj)
        {  
            try
            {
                AccessControl.VerifyUserAccessToPatient(noteObj.PatientId);
                ////if (!string.IsNullOrEmpty(noteObj.Note))
                ////{
                    this.it2Business.AddNote(noteObj);
                ////}

                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var msg = string.Format("AddNote(patientid = {0}, {1}, {2}", noteObj.PatientId, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        #endregion

        #region PUT

        /// <summary>Update an existing note.</summary>
        /// <param name="noteObj">The note.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPut]
        public HttpResponseMessage UpdateNote([FromBody] PatientNotesSearchResults noteObj)
        {
            try
            {
                ////if (!string.IsNullOrEmpty(noteObj.Note))
                ////{
                    this.it2Business.UpdateNote(noteObj);
                ////}

                return this.Request.CreateResponse(HttpStatusCode.OK, "Note updated.");
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetAllRxByPatientId(noteId = {0}, patientId =  {1}, {2} {3}", noteObj.NoteId, noteObj.PatientId, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        /// <summary>Update all notes</summary>
        /// <param name="notes">The notes.</param>
        [HttpPut]
        public void UpdateAllNotes([FromBody]List<PatientNotesSearchResults> notes)
        {
            this.it2Business.UpdateAllNotes(notes);
        }
        #endregion

        #region DELETE

        /// <summary>The delete.</summary>
        /// <param name="noteObj">The note.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public HttpResponseMessage Delete([FromBody]PatientNotesSearchResults noteObj)
        {
            try
            {
                var note = this.GetPatientNoteById(noteObj.NoteId.ToString(CultureInfo.InvariantCulture));
                if (note != null)
                {
                    AccessControl.VerifyUserAccessToPatient(note.PatientId);
                    var pm = new PatientNoteManager();
                    pm.DeletePatientNoteById(noteObj.NoteId);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, "Note deleted.");
            }
            catch (NHibernate.ObjectNotFoundException)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, "Note Not Found");
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetAllRxByPatientId(patientid = {0}, noteId = {1}, {2} {3}", noteObj.PatientId, noteObj.NoteId, "\n", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        }

        #endregion

        /// <summary>Get the resources</summary>
        /// <param name="results">The results.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public IEnumerable<Lookup> GetResources(IEnumerable<PatientNotesSearchResults> results)
        {
            if (results != null)
            {
                var employeesAdded = new List<int>();
                var resources = new List<Lookup>();

                foreach (var result in results.Where(result => !employeesAdded.Contains(result.ResourceId)))
                {
                    employeesAdded.Add(result.ResourceId);
                    resources.Add(new Lookup(result.ResourceId, result.Resource));
                }

                return resources;
            }

            return null;
        }

        /// <summary>
        /// The print patient note.
        /// </summary>
        /// <param name="firstName">
        /// The first name.
        /// </param>
        /// <param name="lastName">
        /// The last name.
        /// </param>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <param name="noteId">
        /// The note Id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [NonAction]
        public MemoryStream PrintPatientNote(string firstName, string lastName, string officeNumber, string noteId)
        {
            var notes = this.it2Business.GetPatientNoteById(noteId);
            AccessControl.VerifyUserAccessToPatient(notes.PatientId);
            return this.it2Business.PrintPatientNote(firstName, lastName, officeNumber, noteId);
        }

        /// <summary>
        /// The get note types.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private static List<Lookup> GetNoteTypes()
        {
            var items = Enum.GetValues(typeof(NotesTypeEnum));
            var result = new List<Lookup>();

            foreach (var x in from object item in items select new Lookup { Key = (int)item })
            {
                x.Description = Enum.GetName(typeof(NotesTypeEnum), x.Key);
                result.Add(x);
            }

            return new List<Lookup>(result.OrderBy(a => a.Description));
        }
    }
}
