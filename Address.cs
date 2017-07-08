// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Address.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//   The address.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Model
{
    /// <summary>
    ///     The address.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Gets or sets the address id.
        /// </summary>
        public virtual int AddressId { get; set; }

        /// <summary>
        /// Gets or sets the address type id.
        /// </summary>
        public virtual int AddressTypeId { get; set; }

        /// <summary>
        /// Gets or sets the address 1.
        /// </summary>
        public virtual string Address1 { get; set; }

        /// <summary>
        /// Gets or sets the address 2.
        /// </summary>
        public virtual string Address2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public virtual string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public virtual string State { get; set; }

        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        public virtual string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is primary.
        /// </summary>
        public virtual bool IsPrimary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether address type.
        /// </summary>
        public virtual string AddressType { get; set; }
    }
}