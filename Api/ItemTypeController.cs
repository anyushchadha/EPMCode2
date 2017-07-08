// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemTypeController.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//   The item type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;

    /// <summary>
    ///     The item type.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class ItemTypeController : ApiController
    {
        /// <summary>
        ///     The item type manager.
        /// </summary>
        private readonly ItemTypeManager itemTypeManager;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemTypeController" /> class.
        /// </summary>
        public ItemTypeController()
        {
            this.itemTypeManager = new ItemTypeManager();
        }

        /// <summary>
        /// The get item types.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// The <see cref="Enumerable"/>.
        /// </returns>
        public IEnumerable<CompanyItemType> GetItemTypes(string officeNumber)
        {
            return this.itemTypeManager.GetItemTypes(officeNumber);
        }

        /// <summary>
        /// The save item types.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <param name="itemTypes">
        /// The item types.
        /// </param>
        [HttpPut]
        public void SaveItemTypes(string officeNumber, IEnumerable<CompanyItemType> itemTypes)
        {
            this.itemTypeManager.SaveItemType(itemTypes, officeNumber);
        }
    }
}