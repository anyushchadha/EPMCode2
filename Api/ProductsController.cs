// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductsController.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//   Defines the ProductsController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Mvc;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;

    /// <summary>The products controller.</summary>
    [NoCache]
    [System.Web.Http.Authorize]
    [ValidateHttpAntiForgeryToken]
    public class ProductsController : ApiController
    {
        /// <summary>The products manager.</summary>
        private readonly ProductsManager productsManager;

        /// <summary>Initializes a new instance of the <see cref="ProductsController"/> class.</summary>
        public ProductsController()
        {
            this.productsManager = new ProductsManager();
        }

        /// <summary>The get model.</summary>
        /// <param name="manufacturerId">The manufacturer Id.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetModel(string manufacturerId)
        {
            List<SelectListItem> manufacturers = null;
            List<SelectListItem> lensTypes = null;
            List<SelectListItem> lensCategory = null;
            if (manufacturerId == "0")
            {
                manufacturers = GetAllManufacturers();
                lensTypes = GetAllLensTypes();
                lensCategory = GetAllLensCategory();
            }

            var vm = new ProductsSetupVm
            {
                Manufacturers = manufacturers.ToKeyValuePairs(),
                LensTypes = lensTypes.ToKeyValuePairs(),
                LensCategory = lensCategory.ToKeyValuePairs(),
                LensStyles = GetStyleNames(manufacturerId).ToKeyValuePairs()
            };

            return this.Request.CreateResponse(HttpStatusCode.OK, vm);
        }

        /// <summary>The get products items.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="manufacturer">The manufacturer.</param>
        /// <param name="lensType">The lens type.</param>
        /// <param name="lensStyle">The lens style.</param>
        /// <param name="lensCategory">The lens category.</param>
        /// <param name="activeOnly">The active only.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetProductsItems(
            string officeNumber, string manufacturer, string lensType, string lensStyle, string lensCategory, bool activeOnly)
        {
            int? lensStyleId = null;
            int? lensTypeId = null;
            bool? active = null;
            string manufacturerId = null;
            bool? hardLens;

            if (!string.IsNullOrEmpty(lensType) && lensType != "0")
            {
                lensTypeId = int.Parse(lensType);
            }

            if (!string.IsNullOrEmpty(lensStyle) && lensStyle != "0")
            {
                lensStyleId = int.Parse(lensStyle);
            }

            if (manufacturer != "0")
            {
                manufacturerId = manufacturer;
            }

            if (activeOnly)
            {
                active = true;
            }

            switch (lensCategory)
            {
                case "1": ////Hard Contact Lens
                    hardLens = true;
                    break;
                case "2": ////Soft Contact Lens
                    hardLens = false;
                    break;
                default:
                    hardLens = null;
                    break;
            }

            var itemGroups = ProductsIt2Manager.GetItemGroups();
            var enumerable = itemGroups as IList<Item> ?? itemGroups.ToList();
            var items = this.productsManager.SearchProducts(officeNumber, manufacturerId, lensStyleId, lensTypeId, hardLens, active);

            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    ProductsSetupItems = items,
                    ItemGroups = enumerable.Select(ItemGroupVm.FromItem)
                });
        }

        /// <summary>The get contact lens detail.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetContactLensDetail(int id)
        {
            var vm = this.productsManager.GetContactLensDetail(id) as List<ClDetail>;
            if ((vm != null) && (vm.Count > 0))
            {
                var styleId = vm[0].StyleId;
                if (styleId != null)
                {
                    var powers = this.productsManager.GetContactLensPowers((int)styleId) as List<ClPower>;
                    if ((powers != null) && (powers.Count > 0))
                    {
                        vm[0].Power = powers;
                    }
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, vm);
        }

        /// <summary>The save products items.</summary>
        /// <param name="officeNumber">The office number.</param>
        /// <param name="items">The items.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [System.Web.Http.HttpPut]
        public HttpResponseMessage SaveProductsItems(string officeNumber, IEnumerable<ProductsSetup> items)
        {
            var common = new Business.Admin.Common();
            this.productsManager.SaveProductsItems(items, common.GetCompanyId(officeNumber));
            return Request.CreateResponse(HttpStatusCode.OK, "Contact Lens saved.");
        }

        /// <summary>The get all manufacturers.</summary>
        /// <returns>The list.</returns>
        private static List<SelectListItem> GetAllManufacturers()
        {
            var result = new List<SelectListItem> { new SelectListItem { Selected = false, Text = "Select", Value = "0" } };
            var lookups = ProductsIt2Manager.GetAllManufacturers();
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.KeyStr }));

            return result;
        }

        /// <summary>The get all style names.</summary>
        /// <param name="manufacturerId">The manufacturer Id.</param>
        /// <returns>The list.</returns>
        private static List<SelectListItem> GetStyleNames(string manufacturerId)
        {
            var result = new List<SelectListItem>();
            var lookups = ProductsIt2Manager.GetStyleNames(manufacturerId);
            if (lookups.Count > 0)
            {
                result.Add(new SelectListItem { Selected = false, Text = "Select", Value = "0" });
                lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key + string.Empty }));
            }

            return result;
        }

        /// <summary>The get all lens types.</summary>
        /// <returns>The list.</returns>
        private static List<SelectListItem> GetAllLensTypes()
        {
            var result = new List<SelectListItem> { new SelectListItem { Selected = false, Text = "Select", Value = "0" } };
            var lookups = ProductsIt2Manager.GetAllLensTypes();
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key + string.Empty }));

            return result;
        }

        /// <summary>The get all lens category.</summary>
        /// <returns>The list.</returns>
        private static List<SelectListItem> GetAllLensCategory()
        {
            var result = new List<SelectListItem>
                             {
                                 new SelectListItem { Selected = false, Text = "Select", Value = "0" },
                                 new SelectListItem { Selected = false, Text = "Hard Contact Lens", Value = "1" },
                                 new SelectListItem { Selected = false, Text = "Soft Contact Lens", Value = "2" }
                             };

            return result;
        }
    }
}