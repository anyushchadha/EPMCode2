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
    using Eyefinity.Enterprise.Business.Integrations;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Admin;

    using ImageResizer;

    [NoCache]
    [Authorize]
    public class OfficeInformationController : ApiController
    {
        /// <summary>
        /// The practice information it 2 manager.
        /// </summary>
        private readonly OfficeIt2Manager officeIt2Manager;

        private readonly PmiIt2IntegrationManager pmiManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OfficeInformationController" /> class.
        /// </summary>
        public OfficeInformationController()
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
        /// <param name="practiceLocationId">
        /// The practice location id.
        /// </param>
        /// <returns>
        /// The <see cref="OfficeInfo"/>.
        /// </returns>
        /// <exception cref="HttpResponseException">
        /// throw exception if not matching
        /// </exception>
        [HttpGet]
        [Authorize(Roles = "Company Setup")]
        [ValidateHttpAntiForgeryToken]
        public OfficeInfo GetOfficeInformation(string practiceLocationId)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(practiceLocationId);

            var officeInfo = this.officeIt2Manager.GetOfficeInformation(practiceLocationId);
            officeInfo.ContactInformation.Phone = FormatPhone(officeInfo.ContactInformation.Phone);
            officeInfo.ContactInformation.Fax = FormatPhone(officeInfo.ContactInformation.Fax);
            officeInfo.ContactInformation.OfficeTaxId = FormatTaxId(officeInfo.ContactInformation.OfficeTaxId);
            officeInfo.ContactInformation.OfficeNpi = officeInfo.ContactInformation.OfficeNpi;
            officeInfo.TaxDetails = PracticeInformationManager.GetTaxInformation(practiceLocationId);

            var zip = officeInfo.ContactInformation.Zip.Replace("-", string.Empty);
            if (zip.Length > 5)
            {
                officeInfo.ContactInformation.Zip = zip.Substring(0, 5);
                officeInfo.ContactInformation.ZipExtension = zip.Substring(5);
            }

            // EPM is more strict than ALE in that "No Policy" is not allowed. Also password
            // expire days is split from the strength so only pull the 90 day policies or
            // else duplicate results will get returned (differing only in Id that is)
            officeInfo.PasswordPolicy = PracticeInformationManager.GetPasswordPolicy().Where(
                p => p.ExpirationDays > 0 && !p.Description.Contains("No Policy")).ToList();

            // If for some reason this practice has a PasswordPolicyId that we've disallowed
            // default them back to the 1 upper or lower policy. We have to do it this funny
            // way because PasswordPolicy.Id is an IDENTITY and we can't rely on the Ids
            // remaining constant. This has already bit us.
            if (officeInfo.PasswordPolicy.All(p => p.Id != officeInfo.PasswordPolicyId))
            {
                var policy = officeInfo.PasswordPolicy.FirstOrDefault(p => p.Description.Contains(" or ")) ??
                    officeInfo.PasswordPolicy.First();

                officeInfo.PasswordPolicyId = policy.Id;
            }

            officeInfo.PasswordExpireDays = this.officeIt2Manager.GetPasswordExpireDaysByOffice(practiceLocationId);

            if (!string.IsNullOrWhiteSpace(officeInfo.Branding.Logo))
            {
                var root = this.ControllerContext.Configuration.VirtualPathRoot;
                root = root.EndsWith("/") ? root : root + "/";
                officeInfo.Branding.Logo = officeInfo.Branding.Logo.Replace("~/", root);
            }

            officeInfo.IsVspCompany = this.pmiManager.IsVspPractice(practiceLocationId);
            ////practiceInfo.IsVspPractice = false;

            return officeInfo;
        }

        /// <summary>
        ///     The get zip code.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "Company Setup")]
        [ValidateHttpAntiForgeryToken]
        public IEnumerable<string> GetListOfStates()
        {
            return this.officeIt2Manager.GetListOfStates();
        }

        [HttpGet]
        [Authorize(Roles = "Company Setup")]
        [ValidateHttpAntiForgeryToken]
        public IList<PmiDoctor> ListOfVspProviders(string practiceLocationId)
        {
            return this.pmiManager.GetVspPracticeProviders(practiceLocationId);
        }

        [HttpPut]
        [Authorize(Roles = "Company Setup")]
        [ValidateHttpAntiForgeryToken]
        public HttpResponseMessage SaveVspProviders([FromBody] IList<PmiDoctor> doctors, string practiceLocationId)
        {
            var returnVariable = this.pmiManager.SaveVspProviders(doctors, practiceLocationId);
            return this.Request.CreateResponse(HttpStatusCode.OK, returnVariable);
        }

        /// <summary> 
        /// The save practice information.
        /// </summary>
        /// <param name="officeInfo">
        /// The practice information to save
        /// </param>
        /// <param name="practiceLocationId">
        /// The office Number.
        /// </param>
        /// <exception cref="HttpResponseException">
        /// throw exception if not matching
        /// </exception>
        [HttpPut]
        [Authorize(Roles = "Company Setup")]
        [ValidateHttpAntiForgeryToken]
        public void SaveOfficeInformation([FromBody] OfficeInfo officeInfo, string practiceLocationId)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(practiceLocationId);
            this.officeIt2Manager.SaveOfficeInformation(officeInfo);
            if (officeInfo.TaxDetails != null)
            {
                officeInfo.TaxDetails.ForEach(PracticeInformationManager.SaveTaxInformation);
            }
        }

        /// <summary>
        /// The save logo crop.
        /// </summary>
        /// <param name="practiceLocationId">
        /// The office number.
        /// </param>
        /// <param name="cropValues">
        /// The crop values.
        /// </param>
        [HttpPut]
        [Authorize(Roles = "Company Setup")]
        [ValidateHttpAntiForgeryToken]
        public void SaveLogoCrop([FromBody] CropValues cropValues, string practiceLocationId)
        {
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
                this.officeIt2Manager.GetOfficeLogo(practiceLocationId, out logo, out contentType);
                new ImageJob(logo, ms, settings).Build();
                ms.Position = 0;
                this.officeIt2Manager.SaveOfficeLogo(practiceLocationId, ms.ToArray(), "image/png");
            }
        }

        /// <summary>
        /// The logo.
        /// </summary>
        /// <param name="practiceLocationId">
        /// The office number.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "Company Setup")]
        public HttpResponseMessage Logo(string practiceLocationId)
        {
            byte[] logo;
            string contentType;
            this.officeIt2Manager.GetOfficeLogo(practiceLocationId, out logo, out contentType);

            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(logo) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return response;
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
