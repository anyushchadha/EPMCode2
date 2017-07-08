// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidationController.cs" company="Eyefinity, Inc.">
// 2013 Eyefinity, Inc.  
// </copyright>
// <summary>
//   Defines the ValidationController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Web.Http;

    using Eyefinity.PracticeManagement.Common.Api;

    using IT2.Ioc;
    using IT2.Services;

    /// <summary>
    /// The validation controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class ValidationController : ApiController
    {
        /// <summary>The address services.</summary>
        private readonly IAddressServices addressServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationController"/> class.
        /// </summary>
        /// <param name="addressServices">
        /// The address services.
        /// </param>
        public ValidationController(IAddressServices addressServices)
        {
            this.addressServices = addressServices;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationController"/> class.
        /// </summary>
        public ValidationController()
        {
            this.addressServices = Container.Resolve<IAddressServices>();
        }

        /// <summary>The validate zip code.</summary>
        /// <param name="zipCodes">The zip codes.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool GetValidateZipCode(string zipCodes)
        {
            var status = true;
            var array = zipCodes.Split(',');
            foreach (var s in array)
            {
                var list = this.addressServices.GetZipCodes(s);
                if (list.Count == 0)
                {
                    status = false;
                }
            }

            return status;
        }
    }
}
