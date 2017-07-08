namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Mvc;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;

    using IT2.Core;

    [NoCache]
    [System.Web.Http.Authorize]
    [ValidateHttpAntiForgeryToken]
    public class EyeglassLensController : ApiController
    {
        private const int MaxItems = 1000;
        private readonly EyeglassLensIt2Manager eyeglassLensIt2Manager;
        private readonly ProductsManager productsManager;
        private readonly string companyId;

        public EyeglassLensController()
        {
            this.eyeglassLensIt2Manager = new EyeglassLensIt2Manager();
            this.productsManager = new ProductsManager();
            var user = new AuthorizationTicketHelper().GetUserInfo();
            this.companyId = user.CompanyId;
        }

        /// <summary>The get model.</summary>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetModel()
        {
            var lensAttributes = this.GetAllLensAttributes();
            var lensTypes = this.GetAllLensTypes();
            var lensMaterials = this.GetAllLensMaterials();
            var lensStyles = this.GetAllLensStyles();
            var vm = new EyeglassSetupVm
            {
                LensAttributes = lensAttributes.ToKeyValuePairs(),
                LensTypes = lensTypes.ToKeyValuePairs(),
                LensMaterials = lensMaterials.ToKeyValuePairs(),
                LensStyles = lensStyles.ToKeyValuePairs()
            };

            return this.Request.CreateResponse(HttpStatusCode.OK, vm);
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetLensTypes()
        {
            var lensTypes = this.GetAllLensTypes();
            return this.Request.CreateResponse(HttpStatusCode.OK, lensTypes.ToKeyValuePairs());
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetLensAttributesToPrice(int itemId)
        {
            var lensAttributes = this.eyeglassLensIt2Manager.GetLensAttributesToPrice(itemId, this.companyId);
            return this.Request.CreateResponse(HttpStatusCode.OK, lensAttributes);
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetEyeglassLensMaterials(int lensTypeId)
        {
            var materials = this.GetAvailableLensMaterials(lensTypeId);
            return this.Request.CreateResponse(HttpStatusCode.OK, materials.ToKeyValuePairs());
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetSingleLensPriceByItemId(int lensTypeId)
        {
            var price = this.eyeglassLensIt2Manager.GetSingleLensPriceByItemId(lensTypeId, this.companyId);
            return this.Request.CreateResponse(HttpStatusCode.OK, price);
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetEyeglassLensStyles(int lensTypeId, int lensMaterialId)
        {
            var styles = this.GetAvailableLensStyles(lensTypeId, lensMaterialId);
            return this.Request.CreateResponse(HttpStatusCode.OK, styles.ToKeyValuePairs());
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetEyeglassLensMaterialDetail(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent); 
            }

            var eyeglassLensMaterialDetail = this.eyeglassLensIt2Manager.GetEyeglassLensMaterialDetail(Convert.ToInt32(itemId));
            return this.Request.CreateResponse(HttpStatusCode.OK, eyeglassLensMaterialDetail);   
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetEyeglassLensDetail(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            var eyeglassLensDetail = this.eyeglassLensIt2Manager.GetEyeglassLensDetail(Convert.ToInt32(itemId), this.companyId);
            return this.Request.CreateResponse(HttpStatusCode.OK, eyeglassLensDetail);
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetEyeglassLensItems(string officeNumber, string lensAttribute, string lensType, string lensMaterial, string lensStyle, string searchText, bool activeOnly)
        {
            bool? active = null;
            var items = new List<EyeglassLensSetup>();
            var itemGroups = new List<Model.Admin.Item>();
            var lensPricingItemTypes = new EyeglassSetupVm();
           
            int? lensAttributeId = null;
            if (!string.IsNullOrEmpty(lensAttribute))
            {
                lensAttributeId = Convert.ToInt32(lensAttribute);
            }

            var flexiblePricing = this.eyeglassLensIt2Manager.GetFlexiblePricingAttribute(lensAttributeId);
            int? lensTypeId = null;
            if (!string.IsNullOrEmpty(lensType))
            {
                lensTypeId = Convert.ToInt32(lensType);
            }

            int? lensMaterialId = null;
            if (!string.IsNullOrEmpty(lensMaterial))
            {
                lensMaterialId = Convert.ToInt32(lensMaterial);
            }

            int? lensStyleId = null;
            if (!string.IsNullOrEmpty(lensStyle))
            {
                lensStyleId = Convert.ToInt32(lensStyle);
            }
      
            if (activeOnly)
            {
                active = true;
            }

                 itemGroups = this.eyeglassLensIt2Manager.GetItemGroups(lensAttributeId.GetValueOrDefault()).ToList();
                 items = this.eyeglassLensIt2Manager.GetEyeglassLensItems(officeNumber, lensAttributeId, lensTypeId, lensMaterialId, lensStyleId, active.GetValueOrDefault());

                if (items.Count > MaxItems && !string.IsNullOrEmpty(searchText))
                {
                    items = this.SearchItems(items, searchText);
                }

                if (items.Count > MaxItems)
                {
                    return this.Request.CreateResponse(HttpStatusCode.RequestedRangeNotSatisfiable, items.Count.ToString(CultureInfo.InvariantCulture));
                }

                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        EyeglassLensSetupItems = items,
                        ItemGroups = itemGroups.Select(ItemGroupVm.FromItem),
                        lensPricingItemTypes = flexiblePricing
                    });
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage SaveEyeglassLensItems(string officeNumber, string lensAttribute, IEnumerable<EyeglassLensSetup> items)
        {
            var common = new Business.Admin.Common();
            int? lensAttributeId;
            bool savePrice = true;
            if (!string.IsNullOrEmpty(lensAttribute))
            {
                lensAttributeId = Convert.ToInt32(lensAttribute);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Please select an item type.");
            }

            if (lensAttributeId.GetValueOrDefault() == (int)ItemTypeEnum.EyeglassLens)
            {
                savePrice = false;    
            }

            this.productsManager.SaveEyeglassLensItems(items, lensAttributeId.GetValueOrDefault(), common.GetCompanyId(officeNumber), savePrice);
            return Request.CreateResponse(HttpStatusCode.OK, "Eyeglass Lens Items saved.");
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage SaveLensAttributePricingAndMapping(int lensId, IEnumerable<EyeglassLensAttribute> itemsToPrice)
        {
            this.eyeglassLensIt2Manager.SaveLensPricingAndMapping(lensId, itemsToPrice, this.companyId);
            return Request.CreateResponse(HttpStatusCode.OK, "Eyeglass Lens Items saved.");
        }

        private List<EyeglassLensSetup> SearchItems(List<EyeglassLensSetup> items, string searchText)
        {
            var filteredItems = items;
            var searchArray = Regex.Split(searchText, @"\W+");

            if (searchArray.Length > 0)
            {
                foreach (var searchString in searchArray)
                {
                    filteredItems = filteredItems.FindAll(p => !string.IsNullOrEmpty(p.ReceiptDescription) && p.ReceiptDescription.ToLower().Contains(searchString.ToLower()));
                }
            }

            return filteredItems;
        }

        /// <summary>The get all lens attributes.</summary>
        /// <returns>The list.</returns>
        private List<SelectListItem> GetAllLensAttributes()
        {
            var result = new List<SelectListItem> { new SelectListItem { Selected = false, Text = "Select", Value = "0" } };
            var lookups = this.eyeglassLensIt2Manager.GetEyeglassLensAttributes();
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));

            return result;
        }

        private List<SelectListItem> GetAllLensTypes()
        {
            var result = new List<SelectListItem> { new SelectListItem { Selected = false, Text = "Select", Value = "0" } };
            var lookups = this.eyeglassLensIt2Manager.GetEyeglassLensTypes();
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));

            return result;
        }

        private List<SelectListItem> GetAllLensMaterials()
        {
            var result = new List<SelectListItem> { new SelectListItem { Selected = false, Text = "Select", Value = "0" } };
            var lookups = this.eyeglassLensIt2Manager.GetEyeglassLensMaterials();
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));

            return result;
        }

        private List<SelectListItem> GetAvailableLensMaterials(int lensTypeId)
        {
            var result = new List<SelectListItem> { new SelectListItem { Selected = false, Text = "Select", Value = "0" } };
            var lookups = this.eyeglassLensIt2Manager.GetEyeglassLensMaterials(lensTypeId);
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));

            return result;
        }

        private List<SelectListItem> GetAllLensStyles()
        {
            var result = new List<SelectListItem> { new SelectListItem { Selected = false, Text = "Select", Value = "0" } };
            var lookups = this.eyeglassLensIt2Manager.GetEyeglassLensStyles();
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));

            return result;
        }

        private List<SelectListItem> GetAvailableLensStyles(int lensTypeId, int lensMaterialId)
        {
            var result = new List<SelectListItem> { new SelectListItem { Selected = false, Text = "Select", Value = "0" } };
            var lookups = this.eyeglassLensIt2Manager.GetEyeglassLensStyles(lensTypeId, lensMaterialId);
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));

            return result;
        }
    }
}