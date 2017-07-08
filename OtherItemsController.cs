// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OtherItemsController.cs" company="Eyefinity, Inc.">
//    Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//  The otherItems manager.
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
    ///     The otherItems controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class OtherItemsController : ApiController
    {
        /// <summary>
        ///     The otherItems manager.
        /// </summary>
        private readonly OtherItemsIt2Manager otherItemsManager;
        private readonly string companyId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OtherItemsController" /> class.
        /// </summary>
        public OtherItemsController()
        {
            this.otherItemsManager = new OtherItemsIt2Manager();
            var user = new AuthorizationTicketHelper().GetUserInfo();
            this.companyId = user.CompanyId;
        }

        /// <summary>
        /// which we passed in
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="itemName">
        /// </param>
        /// <param name="activeOnly">
        /// The active only.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetShippingItems(
            string officeNumber, string itemName, bool activeOnly)
        {
            AccessControl.VerifyUserAccessToCompany(this.companyId);
            int shippingItemType = (int)ItemTypeEnum.Shipping;
            var shippingSetupItems = new List<OtherItems>();
            return this.Request.CreateResponse(
                HttpStatusCode.OK, 
                new
                    {
                    ShippingItems =
                          this.otherItemsManager.GetOtherItems(
                              shippingSetupItems, shippingItemType, itemName, activeOnly, officeNumber, this.companyId), 
                        ItemGroups = this.otherItemsManager.GetItemGroups().Select(ItemGroupVm.FromItem)
                    });
        }

        /// <summary>
        /// The save Shipping items.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="shipping">
        /// The shipping.
        /// </param>
        [HttpPut]
        public void SaveShippingItems(string officeNumber, IEnumerable<OtherItems> shipping)
        {
            AccessControl.VerifyUserAccessToCompany(this.companyId);
            int shippingItemType = (int)ItemTypeEnum.Shipping;
            var enumerable = shipping as OtherItems[] ?? shipping.ToArray();
            this.otherItemsManager.SaveOtherItems(enumerable, shippingItemType, officeNumber, this.companyId);
        }

        /// <summary>
        /// Returns Repairs for the company level.
        /// which we passed in
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="itemName">
        /// The repairs description.
        /// </param>
        /// <param name="activeOnly">
        /// The active only.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetRepairsItems(
            string officeNumber, string itemName, bool activeOnly)
        {
            AccessControl.VerifyUserAccessToCompany(this.companyId);
            int repairsItemType = (int)ItemTypeEnum.Repair;
            var repairsSetupItems = new List<OtherItems>();
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    RepairsItems =
                             this.otherItemsManager.GetOtherItems(
                              repairsSetupItems, repairsItemType, itemName, activeOnly, officeNumber, this.companyId),
                    ItemGroups = this.otherItemsManager.GetItemGroups().Select(ItemGroupVm.FromItem)
                });
        }

        /// <summary>
        /// The save repairs items.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="repairs">
        /// The repairs.
        /// </param>
        [HttpPut]
        public void SaveRepairsItems(string officeNumber, IEnumerable<OtherItems> repairs)
        {
            AccessControl.VerifyUserAccessToCompany(this.companyId);
            int repairsItemType = (int)ItemTypeEnum.Repair;
            var enumerable = repairs as OtherItems[] ?? repairs.ToArray();
            this.otherItemsManager.SaveOtherItems(enumerable, repairsItemType, officeNumber, this.companyId);
        }

        /// <summary>
        /// Returns miscFees for the company level.
        /// which we passed in
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="itemName">
        /// The miscFees description.
        /// </param>
        /// <param name="activeOnly">
        /// The active only.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetMiscFeesItems(
            string officeNumber, string itemName, bool activeOnly)
        {
            AccessControl.VerifyUserAccessToCompany(this.companyId);
            int miscFeesItemType = (int)ItemTypeEnum.MiscFee;
            var miscFeesSetupItems = new List<OtherItems>();
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    MiscFeesItems =
                            this.otherItemsManager.GetOtherItems(
                              miscFeesSetupItems, miscFeesItemType, itemName, activeOnly, officeNumber, this.companyId),
                    ItemGroups = this.otherItemsManager.GetItemGroups().Select(ItemGroupVm.FromItem)
                });
        }

        /// <summary>
        /// The save MiscFees items.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="miscFees">
        /// The MiscFees.
        /// </param>
        [HttpPut]
        public void SaveMiscFeesItems(string officeNumber, IEnumerable<OtherItems> miscFees)
        {
            AccessControl.VerifyUserAccessToCompany(this.companyId);
            int miscFeesItemType = (int)ItemTypeEnum.MiscFee;
            var enumerable = miscFees as OtherItems[] ?? miscFees.ToArray();
            this.otherItemsManager.SaveOtherItems(enumerable, miscFeesItemType, officeNumber, this.companyId);
        }
    }
} 