// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangePaymentType.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//   Model for the ChangePaymentType
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// The change payment type.
    /// </summary>
    public class ChangePaymentType
    {
        /// <summary>
        /// Gets or sets the payment types.
        /// </summary>
        public List<Lookup> PaymentTypes { get; set; }

        /// <summary>
        /// Gets or sets the payment details.
        /// </summary>
        public List<PaymentTransactionLite> PaymentDetails { get; set; }

        public bool IsTodayClosed { get; set; }
    }
}