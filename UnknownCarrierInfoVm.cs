// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CarrierInformationVM.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//   Defines the CarrierInformationVM type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Model.ViewModel
{
    /// <summary>
    /// The carrier information view model.
    /// </summary>
    public class UnknownCarrierInfoVm
    {
        /// <summary>
        /// Gets or sets the patient id.
        /// </summary>
        public virtual int PatientId { get; set; }

        /// <summary>
        /// Gets or sets the patient Insurance id.
        /// </summary>
        public virtual int PatientInsuranceId { get; set; }

        /// <summary>
        /// Gets or sets the insurance plan id.
        /// </summary>
        public virtual int InsurancePlanId { get; set; }

        /// <summary>
        /// Gets or sets the address id.
        /// </summary>
        public virtual int CarrierId { get; set; }

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
        /// Gets or sets the contact name.
        /// </summary>
        public virtual string ContactName { get; set; }

        /// <summary>
        /// Gets or sets the primary phone.
        /// </summary>
        public virtual string PrimaryPhone { get; set; }

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        public virtual string Extension { get; set; }

        /// <summary>
        /// Gets or sets the fax.
        /// </summary>
        public virtual string Fax { get; set; }

        /// <summary>
        /// Gets or sets the address id.
        /// </summary>
        public virtual int AddressId { get; set; }
    }
}
