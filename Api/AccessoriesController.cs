// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccessoriesController.cs" company="Eyefinity, Inc.">
//    Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//  The accessories manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;

    using IT2.Core;

    /// <summary>
    ///     The accessories controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class AccessoriesController : ApiController
    {
        /// <summary>
        ///     The accessories manager.
        /// </summary>
        private readonly AccessoriesIt2Manager accessoriesManager;
        private readonly string companyId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccessoriesController" /> class.
        /// </summary>
        public AccessoriesController()
        {
            this.accessoriesManager = new AccessoriesIt2Manager();
            var user = new AuthorizationTicketHelper().GetUserInfo();
            this.companyId = user.CompanyId;
        }

        /// <summary>
        /// Returns accessories for the company level.
        /// in stored procedure the companyId is being lookuped by the officeNumber
        /// which we passed in
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="itemName">
        /// The accessory description.
        /// </param>
        /// <param name="activeOnly">
        /// The active only.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetAccessoryItems(
            string officeNumber, string itemName, bool activeOnly)
        {
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    AccessoryItems = this.accessoriesManager.GetAccessoryItems(
                        officeNumber, this.companyId, itemName, activeOnly),
                    ItemGroups = this.accessoriesManager.GetItemGroups().Select(ItemGroupVm.FromItem)
                });
        }

        /// <summary>
        /// The save accessories items.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="accessories">
        /// The accessories.
        /// </param>
        [HttpPut]
        public void SaveAccessoryItems(string officeNumber, IEnumerable<Accessory> accessories)
        {
            int accessoryItemType = (int)ItemTypeEnum.Accessory;
            var enumerable = accessories as Accessory[] ?? accessories.ToArray();
            this.accessoriesManager.SaveAccessoriesItems(enumerable, accessoryItemType, officeNumber, this.companyId);
        }
    }
} 