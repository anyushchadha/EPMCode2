// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SchedulerExceptionsController.cs" company="Eyefinity, Inc">
//   2013 Eyefinity, Inc
// </copyright>
// <summary>
//   Defines the SchedulerExceptionsController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Appointment;

    ////using log4net.Repository.Hierarchy;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;

    using ResourceException = Eyefinity.PracticeManagement.Model.Appointment.ResourceException;

    /// <summary>
    /// The scheduler exception controller.
    /// </summary>
    [NoCache]
    [System.Web.Mvc.Authorize(Roles = "Schedule Exceptions")]
    [ValidateHttpAntiForgeryToken]
    public class SchedulerExceptionsController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly SchedulerExceptionsIt2Manager exceptionsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerExceptionsController"/> class.
        /// </summary>
        /// <param name="exceptionsManager">
        /// The it 2 business.
        /// </param>
        public SchedulerExceptionsController(SchedulerExceptionsIt2Manager exceptionsManager)
        {
            this.exceptionsManager = exceptionsManager;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerExceptionsController"/> class.
        /// </summary>
        public SchedulerExceptionsController()
        {
            this.exceptionsManager = new SchedulerExceptionsIt2Manager();
        }

        /// <summary>
        /// The get scheduler exception.
        /// </summary>
        /// <param name="officeNum">
        /// The office Number.
        /// </param>
        /// <param name="employeeId">
        /// The employee id.
        /// </param>
        /// <param name="exceptionId">
        /// The exception Id.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetSchedulerException(int officeNum, int employeeId, int exceptionId)
        {
            try
            {
                 return Request.CreateResponse(HttpStatusCode.OK, this.exceptionsManager.GetSchedulerException(officeNum, employeeId, exceptionId));
            }
            catch (Exception ex)
            {
                var error = "GetSchedulerException(exceptionId=" + exceptionId + ", employeeId=" + employeeId + ")\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The get resource exception by id.
        /// </summary>
        /// <param name="exceptionId">
        /// The exception id.
        /// </param>
        /// <returns>
        /// The <see cref="ResourceException"/>.
        /// </returns>
        [HttpGet]
        public ResourceException GetResourceExceptionById(int exceptionId)
        {
            return this.exceptionsManager.GetResourceExceptionById(exceptionId);
        }

        /// <summary>
        /// The validate changed exception.
        /// </summary>
        /// <param name="resourceException">
        /// The resource exception.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        [HttpPost]
        public HttpResponseMessage ValidateException(ResourceException resourceException)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, this.exceptionsManager.ValidateException(resourceException));
            }
            catch (Exception ex)
            {
                var error = "ValidateException(exceptionId=" + resourceException.Id + "\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The validate exception by id.
        /// </summary>
        /// <param name="exceptionId">
        /// The exception id.
        /// </param>
        /// <param name="allSeries">
        /// The all Series.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage ValidateExceptionDeleteById(int exceptionId, bool allSeries)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, this.exceptionsManager.ValidateExceptionDelete(exceptionId, allSeries));
            }
            catch (Exception ex)
            {
                var error = "ValidateExceptionDeleteById(exceptionId=" + exceptionId + "\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The save scheduler exception.
        /// </summary>
        /// <param name="resourceException">
        /// The resource exception.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage SaveSchedulerException(ResourceException resourceException)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, this.exceptionsManager.SaveException(resourceException));
            }
            catch (Exception ex)
            {
                var error = "ValidateExceptionDeleteById(exceptionId=" + resourceException.Id + "\n" + ex;
                return HandleExceptions.LogExceptions(error, Logger, ex);
            }
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="exceptionId">
        /// The exception id.
        /// </param>
        /// <param name="deleteSeries">
        /// The delete series.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpDelete]
        public HttpResponseMessage Delete(int exceptionId, bool deleteSeries)
        {
            try
            {
                this.exceptionsManager.DeleteException(exceptionId, deleteSeries);
            }
            catch (Exception e)
            {
                Logger.Error("Exception - Delete(exceptionId=" + exceptionId + ")\n" + e);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Exception Deleted");
        }
    }
}
