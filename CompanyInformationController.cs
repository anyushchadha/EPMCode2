// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PracticeInformationController.cs" company="Eyefinity, Inc.">
// Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
// Practice Information controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.RegularExpressions;
    using System.Web.Http;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.Enterprise.Business.Common;
    using Eyefinity.Enterprise.Business.Integrations;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Admin;

    using ImageResizer;

    using Common = Eyefinity.PracticeManagement.Business.Admin.Common;

    /// <summary>
    ///     The practice information controller.
    /// </summary>
    [NoCache]
    [Authorize]
    public class CompanyInformationController : ApiController
    {
        /// <summary>
        /// The practice information it 2 manager.
        /// </summary>
        private readonly OfficeIt2Manager officeIt2Manager;

        private readonly PmiIt2IntegrationManager pmiManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompanyInformationController" /> class.
        /// </summary>
        public CompanyInformationController()
        {
            this.officeIt2Manager = new OfficeIt2Manager();
            this.pmiManager = new PmiIt2IntegrationManager();
        }

        #region Enums

        /// <summary>The policy.</summary>
        public enum Policy
        {
            /// <summary>The numeric upper and lower.</summary>
            NumericUpperAndLower = 1,

            /// <summary>The numeric upper or lower.</summary>
            NumericUpperOrLower,

            /// <summary>The no policy.</summary>
            NoPolicy
        }
        #endregion

        /// <summary>
        /// Get practice information for the associated office. Includes office and company information for multi-location offices.
        /// </summary>
        /// <param name="officeNumber">
        /// The practice location id.
        /// </param>
        /// <param name="companyId"></param>
        /// <returns>
        /// The <see cref="CompanyInfo"/>.
        /// </returns>
        /// <exception cref="HttpResponseException">
        /// throw exception if not matching
        /// </exception>
        [HttpGet]
        [Authorize(Roles = "Company Setup")]
        [ValidateHttpAntiForgeryToken]
        public CompanyInfo GetCompanyInformation(string officeNumber, string companyId)
        {
            AccessControl.VerifyUserAccessToCompany(companyId);

            var companyInfo = this.officeIt2Manager.GetCompanyInformation(companyId, officeNumber);
            companyInfo.ContactInformation.Phone = FormatPhone(companyInfo.ContactInformation.Phone);
            companyInfo.ContactInformation.Fax = FormatPhone(companyInfo.ContactInformation.Fax);
            companyInfo.ContactInformation.CompanyTaxId = FormatTaxId(companyInfo.ContactInformation.CompanyTaxId);
            companyInfo.TaxDetails = PracticeInformationManager.GetTaxInformation(officeNumber);

            var zip = companyInfo.ContactInformation.Zip.Replace("-", string.Empty);
            if (zip.Length > 5)
            {
                companyInfo.ContactInformation.Zip = zip.Substring(0, 5);
                companyInfo.ContactInformation.ZipExtension = zip.Substring(5);
            }

            // EPM is more strict than ALE in that "No Policy" is not allowed. Also password
            // expire days is split from the strength so only pull the 90 day policies or
            // else duplicate results will get returned (differing only in Id that is)
            companyInfo.PasswordPolicy = PracticeInformationManager.GetPasswordPolicy().Where(
                p => p.ExpirationDays > 0 && !p.Description.Contains("No Policy")).ToList();

            // If for some reason this practice has a PasswordPolicyId that we've disallowed
            // default them back to the 1 upper or lower policy. We have to do it this funny
            // way because PasswordPolicy.Id is an IDENTITY and we can't rely on the Ids
            // remaining constant. This has already bit us.
            if (companyInfo.PasswordPolicy.All(p => p.Id != companyInfo.PasswordPolicyId))
            {
                var policy = companyInfo.PasswordPolicy.FirstOrDefault(p => p.Description.Contains(" or ")) ??
                    companyInfo.PasswordPolicy.First();

                companyInfo.PasswordPolicyId = policy.Id;
            }

            companyInfo.PasswordExpireDays = this.officeIt2Manager.GetPasswordExpireDaysByOffice(officeNumber);

            if (!string.IsNullOrWhiteSpace(companyInfo.Branding.Logo))
            {
                var root = this.ControllerContext.Configuration.VirtualPathRoot;
                root = root.EndsWith("/") ? root : root + "/";
                companyInfo.Branding.Logo = companyInfo.Branding.Logo.Replace("~/", root);
            }

            ////practiceInfo.IsVspCompany = this.pmiManager.IsVspPractice(officeNumber);
            ////practiceInfo.IsVspPractice = false;

            return companyInfo;
        }

        /// <summary>
        ///     The get zip code.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        [HttpGet]
        [ValidateHttpAntiForgeryToken]
        public IEnumerable<string> GetListOfStates()
        {
            return this.officeIt2Manager.GetListOfStates();
        }
        
        /// <summary> 
        /// The save practice information.
        /// </summary>
        /// <param name="companyInfo">
        /// The practice information to save
        /// </param>
        /// <param name="companyId">
        /// The office Number.
        /// </param>
        /// <exception cref="HttpResponseException">
        /// throw exception if not matching
        /// </exception>
        [HttpPut]
        [Authorize(Roles = "Company Setup")]
        [ValidateHttpAntiForgeryToken]
        public void SaveCompanyInformation([FromBody] CompanyInfo companyInfo, string companyId)
        {
            AccessControl.VerifyUserAccessToCompany(companyId);
            this.officeIt2Manager.SaveCompanyInformation(companyInfo);
        }

        /// <summary>
        /// The save logo crop.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="companyId"></param>
        /// <param name="cropValues">
        /// The crop values.
        /// </param>
        [HttpPut]
        [Authorize(Roles = "Company Setup")]
        [ValidateHttpAntiForgeryToken]
        public void SaveLogoCrop([FromBody] CropValues cropValues, string companyId)
        {
            AccessControl.VerifyUserAccessToCompany(companyId);
            using (var ms = new MemoryStream())
            {
                var settings = new Instructions
                {
                    Format = "png",
                    CropRectangle = new double[] { cropValues.X1, cropValues.Y1, cropValues.X2, cropValues.Y2 },
                    Width = cropValues.Width,
                    Height = cropValues.Height
                };

                byte[] logo;
                string contentType;
                this.officeIt2Manager.GetCompanyLogo(companyId, out logo, out contentType);
                new ImageJob(logo, ms, settings).Build();
                ms.Position = 0;
                this.officeIt2Manager.SaveCompanyLogo(companyId, ms.ToArray(), "image/png");
            }
        }

        /// <summary>
        /// The logo.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="companyId"></param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "Company Setup")]
        public HttpResponseMessage Logo(string officeNumber, string companyId)
        {
            AccessControl.VerifyUserAccessToCompany(companyId);
            byte[] logo;
            string contentType;
            this.officeIt2Manager.GetCompanyLogo(companyId, out logo, out contentType);

            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(logo) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return response;
        }

        [HttpGet]
        public HttpResponseMessage GetOffices(string companyId)
        {
            AccessControl.VerifyUserAccessToCompany(companyId);
            var authorizationTicketHelper = new AuthorizationTicketHelper();
            if (companyId != authorizationTicketHelper.GetCompanyId())
            {
                return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, this.officeIt2Manager.GetOfficesForCompany(companyId));
        }

        [HttpGet]
        public string GetCompanyName(string companyId)
        {
            AccessControl.VerifyUserAccessToCompany(companyId);
            return this.officeIt2Manager.GetCompanyName(companyId);
        }

        [HttpGet]
        public string GetOfficeName(string officeNumber)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(officeNumber);
            return this.officeIt2Manager.GetOfficeName(officeNumber);
        }

        /// <summary>
        /// The verify practice.
        /// </summary>
        /// <param name="companyId">
        /// The company identifier of the practice.
        /// </param>
        /// <exception cref="HttpResponseException">
        /// throw exception if not matching
        /// </exception>
        [HttpGet]
        public HttpResponseMessage IsPatientOverviewEnabled(string companyId)
        {
            AccessControl.VerifyUserAccessToCompany(companyId);
            var request = ControllerContext.Request;
            var overviewEnabled = new PreferencesManager().GetPatientCenterEnabled(companyId);
            var responseMsg = request.CreateResponse(HttpStatusCode.OK, overviewEnabled);
            return responseMsg;
        }

        /// <summary>The format Phone.</summary>
        /// <param name="strPhone">The string Phone.</param>
        /// <returns>The string.</returns>
        private static string FormatPhone(string strPhone)
        {
            var phone = "(999) 999-9999";
            if (!string.IsNullOrEmpty(strPhone))
            {
                var phoneNumber = strPhone.Replace("(", string.Empty)
                                          .Replace(")", string.Empty)
                                          .Replace("-", string.Empty)
                                          .Replace(" ", string.Empty);
                phoneNumber = Regex.Replace(phoneNumber, "[^0-9]", string.Empty);
                phone = !string.IsNullOrEmpty(phoneNumber)
                            ? Convert.ToDouble(phoneNumber).ToString("(###) ###-####")
                            : "(999) 999-9999";
            }
            
            return phone;
        }

        /// <summary>The format Tax Id.</summary>
        /// <param name="strTaxId">The string Tax Id.</param>
        /// <returns>The string.</returns>
        private static string FormatTaxId(string strTaxId)
        {
            var taxId = "99-9999999";
            if (!string.IsNullOrEmpty(strTaxId))
            {
                var taxIdNumber = strTaxId.Replace("-", string.Empty);
                taxIdNumber = Regex.Replace(taxIdNumber, "[^0-9]", string.Empty);
                ////nine digit number
                if (taxIdNumber.Length == 9)
                {
                    taxId = taxIdNumber.Substring(0, 2) + "-" + taxIdNumber.Substring(2);
                }
            }

            return taxId;
        }
    }
}