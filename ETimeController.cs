// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ETimeController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   Defines the HolidayController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Employee;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;

    /// <summary>
    /// The e time controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class ETimeController : ApiController
    {
        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly ETimeIt2Manager it2Business;

        /// <summary>
        /// Initializes a new instance of the <see cref="ETimeController"/> class.
        /// </summary>
        public ETimeController()
        {
            this.it2Business = new ETimeIt2Manager();
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// The <see cref="Enumerable"/>.
        /// </returns>
        public IEnumerable<ETime> Get(string officeNumber)
        {
            return this.it2Business.GetETime(officeNumber);
        }
    }
}
