// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddressController.cs" company="Eyefinity, Inc.">
//    Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//  The address controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Collections.Generic;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;

    /// <summary>
    /// The address controller.
    /// </summary>
    [NoCache]
    [ValidateHttpAntiForgeryToken]
    [Authorize]
    public class AddressController : ApiController
    {
        /// <summary>
        /// The get zip code.
        /// </summary>
        /// <param name="zipCode">
        /// The zip code.
        /// </param>
        /// <returns>
        /// The Result />.
        /// </returns>
        [HttpGet]
        public IEnumerable<CityStateZip> GetZipCode(string zipCode)
        {
            var c = new Common();
            return c.GetZipCode(zipCode);
        }
    }
}