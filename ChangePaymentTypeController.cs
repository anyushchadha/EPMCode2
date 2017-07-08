// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangePaymentTypeController.cs" company="Eyefinity, Inc.">
//   2013 Eyefinity, Inc.
// </copyright>
// <summary>
//   The change payment type controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Common;
    using Eyefinity.Enterprise.Business.Payment;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;

    /// <summary>
    /// The change payment type controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class ChangePaymentTypeController : ApiController
    {
        /// <summary>
        /// The it 2 business.
        /// </summary>
        private readonly PaymentIt2Manager it2Business;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePaymentTypeController"/> class.
        /// </summary>
        public ChangePaymentTypeController()
        {
            this.it2Business = new PaymentIt2Manager();
        }

        /// <summary>
        /// The get payment types.
        /// </summary>
        /// <param name="officeNumber">
        /// The office Number.
        /// </param>
        /// <returns>
        /// The list of payment types.
        /// </returns>
        public List<Lookup> GetPaymentTypes(string officeNumber)
        {
            return this.it2Business.GetPaymentTypes(officeNumber);
        }

        /// <summary>
        /// The get details.
        /// </summary>
        /// <param name="officeNumber">
        /// The office id.
        /// </param>
        /// <returns>
        /// The detail
        /// </returns>
        public IEnumerable<PaymentTransactionLite> GetDetails(string officeNumber)
        {
            return this.it2Business.GetDetails(officeNumber);
        }

        /// <summary>
        /// The get payment change log.
        /// </summary>
        /// <param name="paymentId">
        /// The payment id.
        /// </param>
        /// <returns>
        /// The payment change log list.
        /// </returns>
        public IEnumerable<PaymentChangeLog> GetPaymentChangeLog(int paymentId)
        {
            return this.it2Business.GetPaymentChangeLog(paymentId);
        }

        /// <summary>
        /// The get change payment type data.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <returns>
        /// The <see cref="ChangePaymentType"/>.
        /// </returns>
        [Authorize(Roles = "Change Payment Types")]
        public ChangePaymentType GetChangePaymentTypeData(string officeNumber)
        {
            var result = new ChangePaymentType();
            var companyId = OfficeHelper.GetCompanyId(officeNumber);
            var list = this.GetPaymentTypes(companyId);
            var office = new OfficeHelper().GetOfficeById(officeNumber);
            var paymentTypes = (from item in list orderby item.Description select item).ToList();
            result.PaymentTypes = paymentTypes;
            result.PaymentDetails = new List<PaymentTransactionLite>();
            result.PaymentDetails = this.GetDetails(officeNumber) as List<PaymentTransactionLite>;
            result.IsTodayClosed = new DailyClosingIt2Manager().IsTodayClosed(officeNumber, office.TimeZone, office.UseDST);
            return result;
        }

        /// <summary>
        /// The put payment type detail.
        /// </summary>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="o">
        /// List of PaymentTransactionLite typed as object.
        /// </param>
        public void PutPaymentTypeDetail(int userId, IEnumerable<PaymentTransactionLite> o)
        {
            this.it2Business.PutPaymentTypeDetail(userId, o);
        }
    }
}
