// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PasswordController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   The password controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Employee;
    using Eyefinity.PracticeManagement.Common.Api;

    using EmployeeSearch = Eyefinity.PracticeManagement.Model.Associate;

    /// <summary>
    /// The password controller.
    /// </summary>
    [NoCache]
    [ValidateHttpAntiForgeryToken]
    [Authorize]
    public class PasswordController : ApiController
    {
        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly PasswordIt2Manager it2Business;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordController"/> class.
        /// </summary>
        public PasswordController()
        {
            this.it2Business = new PasswordIt2Manager();
        }

        /// <summary>
        /// The get all employees.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// The result
        /// </returns>
        [HttpGet]
        public IEnumerable<EmployeeSearch> GetAllEmployees(string officeNumber)
        {
            return this.it2Business.SearchEmployees(officeNumber);
        }

        /// <summary>
        /// The reset resource password.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPut]
        public HttpResponseMessage ResetResourcePassword(int id)
        {
            this.it2Business.ResetPassword(id);
            return Request.CreateResponse(HttpStatusCode.OK, "Password Reset");
        }
    }
}