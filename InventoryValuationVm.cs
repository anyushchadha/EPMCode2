// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InventoryValuationVm.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Model.ViewModel
{
    using System.Collections.Generic;

    /// <summary>The inventory valuation view model.</summary>
    public class InventoryValuationVm
    {
        /// <summary>Gets or sets the item types.</summary>
        public List<Lookup> ItemTypes { get; set; }
    }
}
