// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClaimsInformationController.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//   Defines the ClaimInformationController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Mvc;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;
    using log4net;

    /// <summary>
    /// The claim information controller.
    /// </summary>
    [NoCache]
    [System.Web.Http.Authorize]
    [ValidateHttpAntiForgeryToken]
    public class ClaimsInformationController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CompanyInsuranceController));

        /// <summary>
        /// The get model.
        /// </summary>
        /// <param name="practiceLocationId">
        /// The office number of the practice.
        /// </param>
        /// <returns>
        /// The <see cref="ClaimsInformation"/>.
        /// </returns>
        [System.Web.Http.HttpGet]
        public ClaimsInformationVm GetModel(string practiceLocationId)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(practiceLocationId);
            var result = new ClaimsInformationVm();
            var office = ClaimsInformationIt2Manager.GetClaimsInfoFromIt2(practiceLocationId);
            if (office != null)
            {
                result.PracticeLocationId = practiceLocationId;
                result.Npi = office.Npi;
                result.CompanyBillingName = office.CompanyBillingName;
                result.BillingName = office.BillingName;
                result.BillingNpi = office.BillingNpi;
                result.BillingEin = office.BillingEin;
                result.BillingAddress1 = office.BillingAddress1;
                result.BillingAddress2 = office.BillingAddress2;
                result.CLIANumber = office.CLIANumber;
                result.NpiType = office.NpiType;
                if (office.CityStateZip != null)
                {
                    var zip = office.CityStateZip.Zip.Replace("-", string.Empty);
                    var zipExtension = string.Empty;

                    if (zip.Length > 5)
                    {
                        zipExtension = zip.Substring(5);
                        zip = zip.Substring(0, 5);
                    }

                    result.CityStateZip = new CityStateZip
                    {
                        City = office.CityStateZip.City,
                        State = office.CityStateZip.State,
                        Zip = zip,
                        ZipExtension = zipExtension
                    };
                }
                else
                {
                    result.CityStateZip = new CityStateZip
                    {
                        City = string.Empty,
                        State = string.Empty,
                        Zip = string.Empty,
                        ZipExtension = string.Empty
                    };
                }
            }

            this.LoadStates(ref result);

            return result;
        }

        /// <summary>
        /// The get model.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="placeOfServiceId"></param>
        /// <returns>
        /// The <see cref="ClaimsInformation"/>.
        /// </returns>
        [System.Web.Http.HttpGet]
        public PlaceOfServiceVm GetPlaceOfServiceDialogModel(string companyId, int placeOfServiceId)
        {
            AccessControl.VerifyUserAccessToCompany(companyId);
            var placeOfService = new PlaceOfServiceVm();
            var result = ClaimsInformationIt2Manager.GetPlaceOfServiceFacilityTypes();

            var facilityTypes = new List<SelectListItem>();
            result.ForEach(l => facilityTypes.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.KeyStr }));

            result = ClaimsInformationIt2Manager.GetPlaceOfServiceQualifiers();
            var qualifiers = new List<SelectListItem>();
            result.ForEach(l => qualifiers.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.KeyStr }));

            if (placeOfServiceId != 0)
            {
                var list = ClaimsInformationIt2Manager.GetPlaceOfServiceById(companyId, placeOfServiceId);
                placeOfService = list.FirstOrDefault();
                if (placeOfService == null)
                {
                    return null;
                }

                placeOfService.FacilityTypes = facilityTypes.ToKeyValuePairs();
                placeOfService.Qualifiers = qualifiers.ToKeyValuePairs();
            }
            else
            {
                placeOfService.FacilityTypes = facilityTypes.ToKeyValuePairs();
                placeOfService.Qualifiers = qualifiers.ToKeyValuePairs();
                placeOfService.Active = true;
            }

            return placeOfService;
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage SearchPlaceOfServiceItems(string companyId, string searchName = null, bool mappedOnly = false)
        {
            IList<PlaceOfServiceVm> results;
            try
            {
                AccessControl.VerifyUserAccessToCompany(companyId);
                results = ClaimsInformationIt2Manager.SearchPlaceOfServiceItems(searchName, companyId, mappedOnly);
            }
            catch (Exception ex)
            {
                var msg = "Search Place Of Service error: " + "\n" + ex;
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, results);
        }

        /// <summary>
        /// The save claims information.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        [System.Web.Http.HttpPut]
        public void SaveClaimsInformation([FromBody] ClaimsInformation model)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(model.PracticeLocationId);
            ClaimsInformationIt2Manager.SaveClaimsInformation(model, model.PracticeLocationId);
        }

        /// <summary>The add outside provider information.</summary>
        /// <param name="companyId">The office number.</param>
        /// <param name="placeOfService"></param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [System.Web.Http.HttpPost]
        public HttpResponseMessage SavePlaceOfServiceInformation(string companyId, [FromBody] PlaceOfServiceVm placeOfService)
        {
            placeOfService.CompanyID = companyId;
            AccessControl.VerifyUserAccessToMultiLocationOffice(companyId);
            var placeOfServiceId = ClaimsInformationIt2Manager.PlaceOfServiceSaveOrUpdate(companyId, placeOfService);

            return (placeOfServiceId > 0) ? this.Request.CreateResponse(HttpStatusCode.OK, new { validationmessage = "Place Of Service Added Successfully.", id = string.Empty + placeOfServiceId })
                                  : this.Request.CreateResponse(HttpStatusCode.BadRequest, new { validationmessage = "Unable to add this Place Of Service." });
        }

        /// <summary>The add outside provider information.</summary>
        /// <param name="companyId">The office number.</param>
        /// <param name="placeOfServices"></param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [System.Web.Http.HttpPut]
        public HttpResponseMessage SavePlaceOfServiceItems(string companyId, IEnumerable<PlaceOfServiceVm> placeOfServices)
        {
            try
            {
                AccessControl.VerifyUserAccessToMultiLocationOffice(companyId);
                ClaimsInformationIt2Manager.SavePlaceOfServiceItems(companyId, placeOfServices);
            }
            catch (Exception ex)
            {
                var msg = "SaveAllActivePlaceOfService: " + "\n" + ex;
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }

            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new { validationmessage = "Place Of Service Saved Successfully.", id = string.Empty });
        }

        /// <summary>
        /// The load states.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        private void LoadStates(ref ClaimsInformationVm model)
        {
            var result = new List<SelectListItem>();
            var states = ClaimsInformationIt2Manager.GetStatesFromIt2();
            if (states != null)
            {
                states.ForEach(item => result.Add(new SelectListItem { Selected = false, Text = item, Value = item }));
            }

            model.States = result.ToKeyValuePairs();
        }
    }
}
