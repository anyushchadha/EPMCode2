// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CashReceiptSummary.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//   Model for the CashReceiptSummary
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>The CashReceiptSummary.</summary>
    public class CashReceiptSummary
    {
        /// <summary>Gets or sets the day close ID.</summary>
        public virtual int DayCloseID { get; set; }

        /// <summary>Gets or sets the company ID.</summary>
        public virtual string CompanyID { get; set; }

        /// <summary>Gets or sets the office number.</summary>
        public virtual string OfficeNum { get; set; }

        /// <summary>Gets or sets the office name.</summary>
        public virtual string OfficeName { get; set; }

        /// <summary>Gets or sets the transaction date.</summary>
        public virtual DateTime TransactionDate { get; set; }

        /// <summary>Gets or sets the cash actual.</summary>
        public virtual decimal CashActual { get; set; }

        /// <summary>Gets or sets the check actual.</summary>
        public virtual decimal CheckActual { get; set; }

        /// <summary>Gets or sets the cash computed.</summary>
        public virtual decimal CashComputed { get; set; }

        /// <summary>Gets or sets the check computed.</summary>
        public virtual decimal CheckComputed { get; set; }

        /// <summary>Gets or sets the cash bank actual.</summary>
        public virtual decimal CashBankActual { get; set; }

        /// <summary>Gets or sets the check bank actual.</summary>
        public virtual decimal CheckBankActual { get; set; }

        /// <summary>Gets or sets the visa master card.</summary>
        public virtual decimal VisaMC { get; set; }

        /// <summary>Gets or sets the Discover card.</summary>
        public virtual decimal Discover { get; set; }

        /// <summary>Gets or sets the Amex.</summary>
        public virtual decimal Amex { get; set; }

        /// <summary>Gets or sets the private label.</summary>
        public virtual decimal PrivateLabel { get; set; }

        /// <summary>Gets or sets the other payments.</summary>
        public virtual decimal OtherPayments { get; set; }

        /// <summary>Gets or sets the over or short.</summary>
        public virtual decimal OverOrShort
        {
            get { return this.CashActual + this.CheckActual - (this.CashBankActual + this.CheckBankActual); }
        }

        /// <summary>Gets or sets the deposit.</summary>
        public virtual decimal Deposit
        {
            get { return this.CashActual + this.CheckActual; }
        }

        /// <summary>Gets or sets the credit card total.</summary>
        public virtual decimal CreditCardTotal
        {
            get { return this.VisaMC + this.Discover + this.Amex; }
        }

        /// <summary>Gets or sets the total receipts.</summary>
        public virtual decimal TotalReceipts
        {
            get { return this.Deposit + this.CreditCardTotal + this.PrivateLabel + this.OtherPayments; }
        }

        /// <summary>Gets or sets the insurance checks.</summary>
        public virtual decimal InsuranceChecks { get; set; }

        /// <summary>Gets or sets the insurance EFT.</summary>
        public virtual decimal InsuranceEFT { get; set; }
    }
}
