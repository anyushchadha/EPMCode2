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

    using Castle.Core.Internal;

    using Eyefinity.Enterprise.Business.Admin;
    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common;
    using Eyefinity.PracticeManagement.Model;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;

    using IT2.Core;

    public class FrameController : ApiController
    {
        /// <summary>The logger</summary>
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly FrameIt2Manager frameIt2Manager;
        private readonly ProductsManager productsManager;
        private readonly string companyId;
        private readonly int dataSourceId;

        public FrameController()
        {
            this.frameIt2Manager = new FrameIt2Manager();
            this.productsManager = new ProductsManager();
            var user = new AuthorizationTicketHelper().GetUserInfo();
            this.companyId = user.CompanyId;
            this.dataSourceId = this.frameIt2Manager.LookupDataSourceIdByCompany(this.companyId);
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetBulkPricingModel()
        {
            ////var frameCollections = this.GetAllActiveFrameCollections();
            var fromPriceListTypes = this.GetFromPriceListForFramesBulkPricing();
            var toPriceListTypes = this.GetToPriceListForFramesBulkPricing();
            var vm = new FrameBulkPriceVm
            {
                ////FrameCollections = frameCollections.ToKeyValuePairs(),
                FromPriceListTypes = fromPriceListTypes.ToKeyValuePairs(),
                ToPriceListTypes = toPriceListTypes.ToKeyValuePairs()
            };

            return this.Request.CreateResponse(HttpStatusCode.OK, vm);
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetCustomFrameSetupModel()
        {
            var manufacturers = this.GetAllFrameManufacturers();
            var frameEdgeTypes = this.GetAllFrameEdgeTypes();
            var frameMaterialTypes = this.GetAllFrameTypes();
            var frameCategories = this.GetAllFrameCategories();
            var vm = new FrameSetupVm
            {
                FrameManufacturers = manufacturers.ToKeyValuePairs(),
                FrameEdgeTypes = frameEdgeTypes.ToKeyValuePairs(),
                MaterialTypes = frameMaterialTypes.ToKeyValuePairs(),
                FrameCategories = frameCategories.ToKeyValuePairs(),
            };

            return this.Request.CreateResponse(HttpStatusCode.OK, vm);
        }

        [System.Web.Mvc.HttpGet]
        public List<KeyValuePair<string, string>> GetAllFrameCollections()
        {
            var collections = this.GetAllFrameCollectionsByCompany();
            return collections.ToKeyValuePairs();
        }

        [System.Web.Mvc.HttpGet]
        public List<KeyValuePair<string, string>> GetFrameCollections(string searchtext)
        {
            var collections = this.GetFrameCollectionsByName(searchtext);
            return collections.ToKeyValuePairs();
        }

        [System.Web.Mvc.HttpGet]
        public List<KeyValuePair<string, string>> GetFrameCollectionsByManufacturer(string manufacturerId)
        {
            var collections = this.GetFrameCollectionsByManufacturerId(manufacturerId);
            return collections.ToKeyValuePairs();
        }

        [System.Web.Mvc.HttpGet]
        public List<KeyValuePair<string, string>> GetFrameManufacturers(string searchtext)
        {
            var manufacturers = this.GetFrameManufacturersByName(searchtext);
            return manufacturers.ToKeyValuePairs();
        }

        [System.Web.Mvc.HttpGet]
        public List<KeyValuePair<string, string>> GetFrameManufacturers()
        {
            var manufacturers = this.GetAllFrameManufacturers();
            return manufacturers.ToKeyValuePairs();
        }

        [System.Web.Mvc.HttpGet]
        public List<KeyValuePair<string, string>> GetFrameEdgeTypes()
        {
            var edgetypes = this.GetAllFrameEdgeTypes();
            return edgetypes.ToKeyValuePairs();
        }

        [System.Web.Mvc.HttpGet]
        public List<KeyValuePair<string, string>> GetFrameTypes()
        {
            var frameTypes = this.GetAllFrameTypes();
            return frameTypes.ToKeyValuePairs();
        }

        [System.Web.Mvc.HttpGet]
        public List<KeyValuePair<string, string>> GetFrameCategories()
        {
            var categories = this.GetAllFrameCategories();
            return categories.ToKeyValuePairs();
        }

        [System.Web.Mvc.HttpGet]
        public List<KeyValuePair<string, string>> GetFrameStyles(string collectionId)
        {
            var styles = this.GetFrameStylesByCollectionId(Convert.ToInt32(collectionId));
            return styles.ToKeyValuePairs();
        }

        [System.Web.Mvc.HttpGet]
        public HttpResponseMessage GetKeyFromWebConfig()
        {
            var key = System.Configuration.ConfigurationManager.AppSettings["bulkFrameSetup"];
            return this.Request.CreateResponse(HttpStatusCode.OK, key == "on");
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetCustomFrame(
            string officeNumber,
            string frameItemId)
        {
            FrameDetails result = this.frameIt2Manager.GetCustomFrame(frameItemId, this.companyId);
            return this.Request.CreateResponse(
                   HttpStatusCode.OK, result);
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetFrameCollectionByNameByManufacturer(
            string officeNumber,
            int manufacturerId,
            string collectionName)
        {
            var collectionId = this.frameIt2Manager.GetFrameCollectionByNameByManufacturer(manufacturerId, collectionName.Trim(), this.dataSourceId);
            return this.Request.CreateResponse(
                   HttpStatusCode.OK, collectionId);
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetFrameModelByNameByCollection(
            string officeNumber,
            int collectionId,
            string modelName)
        {
            var modelId = this.frameIt2Manager.GetFrameModelByNameByCollection(collectionId, modelName.Trim(), this.dataSourceId);
            return this.Request.CreateResponse(
                   HttpStatusCode.OK, modelId);
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetCustomFrameItemsAndStyleItem(
            string officeNumber,
            int collectionId,
            int modelId)
        {
            var results = this.GetCustomFrameItems(this.companyId, collectionId, modelId);
            var style = this.frameIt2Manager.GetFrameStyleById(modelId);
            return this.Request.CreateResponse(
                   HttpStatusCode.OK,
                   new
                   {
                       EdgeTypeId = style.FrameEdgeType,
                       CategoryId = style.FrameCategory.ID.ToString(CultureInfo.InvariantCulture),
                       MaterialId = style.FrameType.ID.ToString(CultureInfo.InvariantCulture),
                       Frames = results.ToKeyValuePairs(),
                       style.DataSourceId
                   }); 
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetFrameItems(
            string officeNumber,
            string collectionId,
            string styleId,
            string searchText,
            bool activeOnly,
            [FromUri]int pageSize = 20,
            [FromUri]int pageStart = 0,
            [FromUri]string sortCol = null,
            [FromUri]string sortDirection = null)
        {
            bool? active = null;

            int? frameCollectionId = null;
            if (!string.IsNullOrEmpty(collectionId))
            {
                frameCollectionId = Convert.ToInt32(collectionId);
            }

            int? frameStyleId = null;
            if (!string.IsNullOrEmpty(styleId))
            {
                frameStyleId = Convert.ToInt32(styleId);
            }

            if (activeOnly)
            {
                active = true;
            }

            if (frameCollectionId == null && string.IsNullOrEmpty(searchText))
            {
                var res = new Model.PagedResult<FrameSetup>
                {
                    PageSize = pageSize,
                    CurrentPage = pageStart / pageSize,
                    TotalCount = 0,
                    CurrentItems = new List<FrameSetup>().ToArray()
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, new { FrameSetupItems = res, HasCustomFrame = false });
            }

            var sortDir = "ASC";
            if (!string.IsNullOrEmpty(sortDirection))
            {
                if (sortDirection == "Descending")
                {
                    sortDir = "DESC";
                }
            }

            int totalRecords;
            var items = this.frameIt2Manager.GetFrameItems(this.companyId, frameCollectionId, frameStyleId, searchText, active.GetValueOrDefault(), out totalRecords, this.dataSourceId, pageSize, pageStart, sortCol, sortDir);
            var hasCustomFrame = false;
            if (items != null && items.Any())
            {
                var res = new Model.PagedResult<FrameSetup>
                {
                    PageSize = pageSize,
                    CurrentPage = pageStart / pageSize,
                    TotalCount = items.Count,
                    CurrentItems = items.Where((t, i) => i >= pageStart && i < pageStart + pageSize).ToArray() //// items.ToArray(),
                };

                if (items.Any(item => item.IsCustomFrame == true))
                {
                    hasCustomFrame = true;
                }

                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        FrameSetupItems = res, HasCustomFrame = hasCustomFrame
                    });
            }

            var result = new Model.PagedResult<FrameSetup>
            {
                PageSize = pageSize,
                CurrentPage = pageStart / pageSize,
                TotalCount = totalRecords,
                CurrentItems = new List<FrameSetup>().ToArray()
            };

            if (totalRecords != 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.RequestedRangeNotSatisfiable, totalRecords.ToString(CultureInfo.InvariantCulture));
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { FrameSetupItems = result, HasCustomFrame = false });
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage GetBulkPricePreviewItems(
            [FromUri]string officeNumber,
            [FromUri]string fromPriceList,
            [FromUri]string toPriceList,
            [FromUri]string changeTypeValue,
            [FromUri]bool changeTypePercentage,
            [FromUri]string dollarAmount,
            [FromUri]bool dollarAdd,
            [FromUri]string rounding,
            [FromUri]bool activeOnly,
            [FromUri]bool all,
            List<int> collectionIds,
            [FromUri]int pageSize = 20,
            [FromUri]int pageStart = 0,
            [FromUri]string sortCol = null,
            [FromUri]string sortDirection = null)
        {
            if (string.IsNullOrEmpty(fromPriceList) || string.IsNullOrEmpty(toPriceList) || (collectionIds == null))
            {
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }

           var items = this.productsManager.GetBulkPricing(
           this.companyId,
           fromPriceList,
           changeTypeValue,
           changeTypePercentage,
           dollarAmount,
           dollarAdd,
           rounding,
           activeOnly,
           collectionIds,
           pageSize,
           pageStart,
           sortCol,
           sortDirection);

            var frameBulkPrices = items as IList<FrameBulkPrice> ?? items.ToList();
            var firstOrDefault = frameBulkPrices.FirstOrDefault();
            var totalCount = 0;
            if (firstOrDefault != null)
            {
                totalCount = firstOrDefault.TotalRowNum;
            }

            var res = new Model.PagedResult<FrameBulkPrice>
            {
                PageSize = pageSize,
                CurrentPage = pageStart / pageSize,
                TotalCount = totalCount,
                CurrentItems = frameBulkPrices.ToArray()
            };
           
            return Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    PreviewItems = res,
                });
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetIsOfficeFramesMapped(string officeNumber)
        {
            // ReSharper disable once RedundantNameQualifier
            List<Model.Lookup> officeCollections = this.frameIt2Manager.GetAllActiveFrameCollections(this.companyId);
            return this.Request.CreateResponse(HttpStatusCode.OK, !officeCollections.IsNullOrEmpty());
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage SaveFrameBulkPricing(
            [FromUri]string officeNumber,
            [FromUri]string fromPriceList,
            [FromUri]string toPriceList,
            [FromUri]string changeTypeValue,
            [FromUri]bool changeTypePercentage,
            [FromUri]string dollarAmount,
            [FromUri]bool dollarAdd,
            [FromUri]string rounding,
            [FromUri]bool activeOnly,
            [FromUri]bool all,
            List<int> collectionIds)
        {
            var message = this.productsManager.SaveBulkPricing(this.companyId, fromPriceList, changeTypeValue, changeTypePercentage, dollarAmount, dollarAdd, rounding, activeOnly, all, collectionIds);
            return Request.CreateResponse(
                HttpStatusCode.OK,  
                new
                {
                    HasZeroPrice = message
                });
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage SaveCustomFrame(string officeNumber, FrameDetails items)
        {
            var token = new AuthorizationTicketHelper().GetToken();
            var frame = this.frameIt2Manager.SaveCustomFrameItems(items, this.companyId, this.dataSourceId, token);
            ////return Request.CreateResponse(HttpStatusCode.OK, frame); 
            return Request.CreateResponse(
                HttpStatusCode.OK, 
                new
                {
                    ManufacturerId = frame.Style.Collection.Manufacturer.ID,
                    CollectionId = frame.Style.Collection.ID,
                    ModelId = frame.Style.ID,
                    ItemId = frame.ID
                });
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage SaveFrameItems(string officeNumber, IEnumerable<FrameSetup> items)
        {
            var token = string.Empty; //// new AuthorizationTicketHelper().GetToken();
            this.frameIt2Manager.SaveFrameItems(items, this.companyId, token);
            return Request.CreateResponse(HttpStatusCode.OK, "Frame Items saved.");
        }

        [System.Web.Http.HttpDelete]
        public HttpResponseMessage DeleteCustomFrameItem(string officeNumber, int frameItemId)
        {
            try
            {
                var token = new AuthorizationTicketHelper().GetToken();
                this.frameIt2Manager.DeleteCustomFrameItem(frameItemId, this.companyId, token);
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                var msg = string.Format("DeleteCustomFrameItem\n {0}", ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        } // DeleteCustomFramesItems

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetCountFramesByUpcCode(string officeNumber, string upcCode)
        {
            try
            {
                if (!string.IsNullOrEmpty(upcCode))
                {
                    int count = 0;
                    var frames = this.frameIt2Manager.GetFramesByUpcCode(upcCode, this.companyId, this.dataSourceId);
                    if (frames != null)
                    {
                        count = frames.Count;
                    }

                    return this.Request.CreateResponse(HttpStatusCode.OK, count);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, 0);
            }
            catch (Exception ex)
            {
                var msg = string.Format("GetFramesByUpcCode(upcCode: {0}, companyId: {1}, dataSourceId: {2})\n {3}", upcCode, this.companyId, this.dataSourceId, ex);
                return HandleExceptions.LogExceptions(msg, Logger, ex);
            }
        } // GetFramesByUpcCode

        public HttpResponseMessage GetEgFrameBySearchCriteria(string searchCriteria, string officeNumber)
        {
            var token = new AuthorizationTicketHelper().GetToken();
            var status = string.Empty;
            var criteria = searchCriteria.Trim();
            var frames = this.frameIt2Manager.GetEgFrameBySearchCriteria(criteria, token, this.companyId, ref status);

            ////Customer supplied frame
            if (status == string.Empty && criteria == "PatientOwnFrame")
            {
                var upcCode = frames[0].UPCCode;
                var frameId = frames[0].Id;
                var selectedFrameDisplay = this.frameIt2Manager.GetEgFrameByFrame(token, this.companyId, officeNumber, upcCode, string.Empty, frameId, 2, token);
                ////ALSL-8077 VSP claim submission fails when non-stock frame is oversize
                if (selectedFrameDisplay != null)
                {
                    selectedFrameDisplay.Eye = null;
                }
                
                return Request.CreateResponse(HttpStatusCode.OK, selectedFrameDisplay);
            }

            return status == string.Empty ? Request.CreateResponse(HttpStatusCode.OK, frames) : Request.CreateResponse(HttpStatusCode.BadRequest, status);
        }

        public HttpResponseMessage GetEgFrameByFrame(int orderType, int frameId, string officeNumber, string upcCode, string productCode)
        {
            var token = new AuthorizationTicketHelper().GetToken();
            var upcCode1 = string.Format("{0}", System.Web.HttpUtility.UrlDecode(upcCode));
            var jobsonProductCode = string.Format("{0}", System.Web.HttpUtility.UrlDecode(productCode));
            var selectedFrameDisplay = this.frameIt2Manager.GetEgFrameByFrame(token, this.companyId, officeNumber, upcCode1.Trim(), jobsonProductCode.Trim(), frameId, orderType, token);

            return selectedFrameDisplay == null ? Request.CreateResponse(HttpStatusCode.BadRequest, selectedFrameDisplay) : Request.CreateResponse(HttpStatusCode.OK, selectedFrameDisplay);
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage UpdateEgFrameRetailPrice([FromBody]EyeglassOrderFrameVm frameDisplay, string officeNumber, string retailPrice)
        {
            var token = new AuthorizationTicketHelper().GetToken();
            var price = Convert.ToDouble(retailPrice);
            var errMsg = this.frameIt2Manager.UpdateSelectedEgFrame(this.companyId, officeNumber, price, frameDisplay, token);
            return errMsg == string.Empty ? Request.CreateResponse(HttpStatusCode.OK, "Frame retail price saved.") : Request.CreateResponse(HttpStatusCode.BadRequest, errMsg);
        }

        #region Private functions

        private List<SelectListItem> GetAllFrameCollectionsByCompany()
        {
            var result = new List<SelectListItem>();

            var lookups = this.frameIt2Manager.GetAllActiveFrameCollections(this.companyId);
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));
            return result;
        }

        private List<SelectListItem> GetFrameCollectionsByName(string searchText)
        {
            var result = new List<SelectListItem>();
            if (string.IsNullOrEmpty(searchText))
            {
                return result;
            }

            var lookups = this.frameIt2Manager.GetFrameCollectionsByName(searchText, this.dataSourceId);
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));
            return result;
        }

        private List<SelectListItem> GetFrameCollectionsByManufacturerId(string manufacturerId)
        {
            var result = new List<SelectListItem>();
            if (string.IsNullOrEmpty(manufacturerId))
            {
                return result;
            }

            var lookups = this.frameIt2Manager.GetFrameCollectionsByManufacturerId(Convert.ToInt32(manufacturerId), this.dataSourceId);
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));
            return result;
        }

        private List<SelectListItem> GetFrameManufacturersByName(string searchText)
        {
            var result = new List<SelectListItem>();
            if (string.IsNullOrEmpty(searchText))
            {
                return result;
            }

            var lookups = this.frameIt2Manager.GetFrameManufacturersByName(searchText);
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));
            return result;    
        }

        private List<SelectListItem> GetAllFrameManufacturers()
        {
            var result = new List<SelectListItem>();
            var lookups = this.frameIt2Manager.GetAllFrameManufacturers(this.dataSourceId);
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));
            return result;
        }

        private List<SelectListItem> GetAllFrameEdgeTypes()
        {
            var result = new List<SelectListItem>();
            var lookups = this.frameIt2Manager.GetAllFrameEdgeTypes();
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.KeyStr.ToString(CultureInfo.InvariantCulture) }));
            return result;
        }

        private List<SelectListItem> GetAllFrameTypes()
        {
            var result = new List<SelectListItem>();
            var lookups = this.frameIt2Manager.GetAllFrameTypes();
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));
            return result;
        }

        private List<SelectListItem> GetAllFrameCategories()
        {
            var result = new List<SelectListItem>();
            var lookups = this.frameIt2Manager.GetAllFrameCategories();
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));
            return result;
        }

        private List<SelectListItem> GetFrameStylesByCollectionId(int collectionId)
        {
            var result = new List<SelectListItem>();
            var lookups = this.frameIt2Manager.GetFrameStylesByCollectionId(collectionId, this.dataSourceId);
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));

            return result;
        }

        private List<SelectListItem> GetFromPriceListForFramesBulkPricing()
        {
            var result = new List<SelectListItem>();
            var lookups = this.frameIt2Manager.GetFromPriceListForFramesBulkPricing(this.companyId);
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.KeyStr.ToString(CultureInfo.InvariantCulture) }));

            return result;
        }

        private List<SelectListItem> GetToPriceListForFramesBulkPricing()
        {
            var result = new List<SelectListItem>();
            var lookups = this.frameIt2Manager.GetPriceListForFrames(this.companyId);
            lookups.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.KeyStr.ToString(CultureInfo.InvariantCulture) }));

            return result;
        }

        private List<SelectListItem> GetCustomFrameItems(string companyId, int collectionId, int modelId)
        {
            var result = new List<SelectListItem>();
            var activeFrames = this.frameIt2Manager.GetCustomFrameItems(companyId, collectionId, modelId, true, this.dataSourceId);
            var inactiveFrames = this.frameIt2Manager.GetCustomFrameItems(companyId, collectionId, modelId, false, this.dataSourceId);
            var mergedFrames = activeFrames.Union(inactiveFrames).ToList();

            mergedFrames.ForEach(l => result.Add(new SelectListItem { Selected = false, Text = l.Description, Value = l.Key.ToString(CultureInfo.InvariantCulture) }));
            return result;
        }

        #endregion // Private functions
    }
}