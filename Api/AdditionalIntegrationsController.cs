// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdditionalIntegrationsController.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//   Defines the AdditionalIntegrationsController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Mvc;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;

    /// <summary>
    /// The additional integrations controller.
    /// </summary>
    [NoCache]
    [System.Web.Http.Authorize]
    [ValidateHttpAntiForgeryToken]
    public class AdditionalIntegrationsController : ApiController
    {
        /// <summary>
        /// The get model.
        /// </summary>
        /// <param name="practiceLocationId">
        /// The practice id.
        /// </param>
        /// <returns>
        /// The <see cref="AdditionalIntegrations"/>.
        /// </returns>
        [System.Web.Http.HttpGet]
        public AdditionalIntegrationsVm GetModel(string practiceLocationId)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(practiceLocationId);

            var additionalIntegrations = AdditionalIntegrationsIt2Manager.GetDataFromIt2Service(practiceLocationId);
            var vm = new AdditionalIntegrationsVm
            {
                PracticeLocationId = additionalIntegrations.PracticeLocationId,
                SupplierId = Convert.ToInt32(additionalIntegrations.SupplierId),
                ClAccountNumber = additionalIntegrations.ClAccountNumber,
                ABBOrderProcessType = additionalIntegrations.ABBOrderProcessType == (int)IT2.Core.AbbOrderProcessType.SingleOrder ? "S" : "B",
                OfficeMateConversionId = additionalIntegrations.OfficeMateConversionId,
                GatewayEdiSite = additionalIntegrations.GatewayEdiSite,
                EhrAccountNumber = additionalIntegrations.EhrAccountNumber,
                EhrDomain = additionalIntegrations.EhrDomain,
                EyefinityPassword = additionalIntegrations.EyefinityPassword,
                EyefinityUsername = additionalIntegrations.EyefinityUsername,
                Suppliers = this.GetSuppliers(practiceLocationId).ToKeyValuePairs(),
                CreditCardProcessingType = additionalIntegrations.CreditCardProcessingType == (int)IT2.Core.Office.CreditCardProcessTypeEnum.Manual ? "M" : "A",
                EhrUrl = System.Configuration.ConfigurationManager.AppSettings["EEHR_URL"],
                EcrVaultIntegration = additionalIntegrations.EcrVaultIntegration.GetValueOrDefault(),
                EcrVaultUrl = additionalIntegrations.EcrVaultUrl,
                EcrVaultUsername = additionalIntegrations.EcrVaultUsername,
                EcrVaultPassword = additionalIntegrations.EcrVaultPassword
            };

            if (additionalIntegrations.EhrIntegration != null && (bool)additionalIntegrations.EhrIntegration)
            {
                vm.EhrIntegration = true;
            }
            else
            {
                vm.EhrIntegration = false;
            }

            vm.VisionWebCbsId = additionalIntegrations.VisionWebCbsId == null ? string.Empty : Convert.ToString(additionalIntegrations.VisionWebCbsId);

            return vm;
        }

        /// <summary>
        /// The get model.
        /// </summary>
        /// <param name="practiceLocationId">
        /// The practice id.
        /// </param>
        /// <returns>
        /// The <see cref="AdditionalIntegrations"/>.
        /// </returns>
        [System.Web.Http.HttpGet]
        public AdditionalIntegrationsVm GetOfficeModel(string practiceLocationId)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(practiceLocationId);

            var additionalIntegrations = AdditionalIntegrationsIt2Manager.GetDataFromIt2Service(practiceLocationId);
            var vm = new AdditionalIntegrationsVm
            {
                PracticeLocationId = additionalIntegrations.PracticeLocationId,
                SupplierId = Convert.ToInt32(additionalIntegrations.SupplierId),
                ClAccountNumber = additionalIntegrations.ClAccountNumber,
                ABBOrderProcessType = additionalIntegrations.ABBOrderProcessType == (int)IT2.Core.AbbOrderProcessType.SingleOrder ? "S" : "B",
                ABBBulkShippingThreshold = AdditionalIntegrationsIt2Manager.GetAbbBulkShippingThreshold(practiceLocationId),
                OfficeMateConversionId = additionalIntegrations.OfficeMateConversionId,
                GatewayEdiSite = additionalIntegrations.GatewayEdiSite,
                EhrAccountNumber = additionalIntegrations.EhrAccountNumber,
                EhrDomain = additionalIntegrations.EhrDomain,
                EyefinityPassword = additionalIntegrations.EyefinityPassword,
                EyefinityUsername = additionalIntegrations.EyefinityUsername,
                Suppliers = this.GetSuppliers(practiceLocationId).ToKeyValuePairs(),
                CreditCardProcessingType = additionalIntegrations.CreditCardProcessingType == (int)IT2.Core.Office.CreditCardProcessTypeEnum.Manual ? "M" : "A",
                EhrUrl = System.Configuration.ConfigurationManager.AppSettings["EEHR_URL"],
                EcrVaultIntegration = additionalIntegrations.EcrVaultIntegration.GetValueOrDefault(),
                EcrVaultUrl = additionalIntegrations.EcrVaultUrl,
                EcrVaultUsername = additionalIntegrations.EcrVaultUsername,
                EcrVaultPassword = additionalIntegrations.EcrVaultPassword
            };

            if (additionalIntegrations.EhrIntegration != null && (bool)additionalIntegrations.EhrIntegration)
            {
                vm.EhrIntegration = true;
            }
            else
            {
                vm.EhrIntegration = false;
            }

            vm.VisionWebCbsId = additionalIntegrations.VisionWebCbsId == null ? string.Empty : Convert.ToString(additionalIntegrations.VisionWebCbsId);

            return vm;
        }

        /// <summary>
        /// The get model.
        /// </summary>
        /// <param name="practiceLocationId">
        /// The practice id.
        /// </param>
        /// <returns>
        /// The <see cref="AdditionalIntegrations"/>.
        /// </returns>
        [System.Web.Http.HttpGet]
        public AdditionalIntegrationsVm GetCompanyModel(string companyId)
        {
            AccessControl.VerifyUserAccessToCompany(companyId);

            var additionalIntegrations = AdditionalIntegrationsIt2Manager.GetDataFromIt2Service(companyId);
            var vm = new AdditionalIntegrationsVm
            {
                PracticeLocationId = additionalIntegrations.PracticeLocationId,
                SupplierId = Convert.ToInt32(additionalIntegrations.SupplierId),
                ClAccountNumber = additionalIntegrations.ClAccountNumber,
                ABBOrderProcessType = additionalIntegrations.ABBOrderProcessType == (int)IT2.Core.AbbOrderProcessType.SingleOrder ? "S" : "B",
                OfficeMateConversionId = additionalIntegrations.OfficeMateConversionId,
                GatewayEdiSite = additionalIntegrations.GatewayEdiSite,
                EhrAccountNumber = additionalIntegrations.EhrAccountNumber,
                EhrDomain = additionalIntegrations.EhrDomain,
                EyefinityPassword = additionalIntegrations.EyefinityPassword,
                EyefinityUsername = additionalIntegrations.EyefinityUsername,
                Suppliers = this.GetSuppliers(companyId).ToKeyValuePairs(),
                CreditCardProcessingType = additionalIntegrations.CreditCardProcessingType == (int)IT2.Core.Office.CreditCardProcessTypeEnum.Manual ? "M" : "A",
                EhrUrl = System.Configuration.ConfigurationManager.AppSettings["EEHR_URL"],
                EcrVaultIntegration = additionalIntegrations.EcrVaultIntegration.GetValueOrDefault(),
                EcrVaultUrl = additionalIntegrations.EcrVaultUrl,
                EcrVaultUsername = additionalIntegrations.EcrVaultUsername,
                EcrVaultPassword = additionalIntegrations.EcrVaultPassword
            };

            if (additionalIntegrations.EhrIntegration != null && (bool)additionalIntegrations.EhrIntegration)
            {
                vm.EhrIntegration = true;
            }
            else
            {
                vm.EhrIntegration = false;
            }

            vm.VisionWebCbsId = additionalIntegrations.VisionWebCbsId == null ? string.Empty : Convert.ToString(additionalIntegrations.VisionWebCbsId);

            return vm;
        }

        /// <summary>
        /// The save additional integrations.
        /// </summary>
        /// <param name="vm">
        /// The VM.
        /// </param>
        /// <exception cref="HttpResponseException">
        /// Throws HTTP Response Exception.
        /// </exception>
        [System.Web.Http.HttpPut]
        public HttpResponseMessage SaveAdditionalIntegrations([FromBody] AdditionalIntegrationsVm vm)
        {
            AccessControl.VerifyUserAccessToMultiLocationOffice(vm.PracticeLocationId);

            var model = new AdditionalIntegrations { PracticeLocationId = vm.PracticeLocationId, SupplierId = vm.SupplierId };

            if (!string.IsNullOrEmpty(vm.ClAccountNumber))
            {
                model.ClAccountNumber = vm.ClAccountNumber.Trim();
            }

            model.ABBOrderProcessType = vm.ABBOrderProcessType == "S" ? (int)IT2.Core.AbbOrderProcessType.SingleOrder : (int)IT2.Core.AbbOrderProcessType.BatchOrder;

            if (!string.IsNullOrEmpty(vm.OfficeMateConversionId))
            {
                model.OfficeMateConversionId = vm.OfficeMateConversionId.Trim();
            }

            model.AbbBulkShippingThreshold = vm.ABBBulkShippingThreshold;
            
            model.GatewayEdiSite = (vm.GatewayEdiSite ?? string.Empty).Trim();

            if (!string.IsNullOrEmpty(vm.EhrAccountNumber))
            {
                model.EhrAccountNumber = vm.EhrAccountNumber.Trim();
            }

            if (!string.IsNullOrEmpty(vm.EhrDomain))
            {
                model.EhrDomain = vm.EhrDomain.Trim();
            }

            ////if (!string.IsNullOrEmpty(vm.ExamUrl))
            ////{
            ////    model.ExamUrl = vm.ExamUrl.Trim();
            ////}

            if (!string.IsNullOrEmpty(vm.EyefinityPassword))
            {
                model.EyefinityPassword = vm.EyefinityPassword.Trim();
            }

            if (!string.IsNullOrEmpty(vm.EyefinityUsername))
            {
                model.EyefinityUsername = vm.EyefinityUsername.Trim();
            }
            
            model.CreditCardProcessingType = vm.CreditCardProcessingType == "M" ? (int)IT2.Core.Office.CreditCardProcessTypeEnum.Manual : (int)IT2.Core.Office.CreditCardProcessTypeEnum.TMSwithEMV;
            model.EhrIntegration = vm.EhrIntegration;

            if (string.IsNullOrEmpty(vm.VisionWebCbsId))
            {
                model.VisionWebCbsId = null;
            }
            else
            {
                model.VisionWebCbsId = Convert.ToInt64(vm.VisionWebCbsId);
            }

            model.EcrVaultIntegration = vm.EcrVaultIntegration;
            model.EcrVaultUrl = vm.EcrVaultUrl;

            if (!string.IsNullOrEmpty(vm.EcrVaultUsername))
            {
                model.EcrVaultUsername = vm.EcrVaultUsername.Trim();
            }

            if (!string.IsNullOrEmpty(vm.EcrVaultPassword))
            {
                model.EcrVaultPassword = vm.EcrVaultPassword.Trim();
            }

            var result = AdditionalIntegrationsIt2Manager.SaveAdditionalIntegrations(model);
            if (!string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, result);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// The get suppliers.
        /// </summary>
        /// <param name="practiceLocationId">
        /// The practice id.
        /// </param>
        /// <returns>
        /// The List />.
        /// </returns>
        private List<SelectListItem> GetSuppliers(string practiceLocationId)
        {
            var result = new List<SelectListItem> { new SelectListItem { Selected = false, Text = "None", Value = "0" } };
            var lookups = AdditionalIntegrationsIt2Manager.GetSuppliers(practiceLocationId);
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = Convert.ToString(l.Key) }));

            return result;
        }
    }
}