// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExamServiceController.cs" company="Eyefinity, Inc.">
//    Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//  The exam service manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Controllers.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Eyefinity.PracticeManagement.Business.Admin;
    using Eyefinity.PracticeManagement.Common.Api;
    using Eyefinity.PracticeManagement.Model.Admin;
    using Eyefinity.PracticeManagement.Model.Admin.ViewModel;

    /// <summary>
    ///     The exam service controller.
    /// </summary>
    [NoCache]
    [Authorize]
    [ValidateHttpAntiForgeryToken]
    public class ExamServiceController : ApiController
    {
        /// <summary>
        ///     The exam service manager.
        /// </summary>
        private readonly ExamServiceManager examServiceManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExamServiceController" /> class.
        /// </summary>
        public ExamServiceController()
        {
            this.examServiceManager = new ExamServiceManager();
        }

        /// <summary>
        /// Returns exam services for the company level.
        /// in stored procedure the companyId is being lookuped by the officeNumber
        /// which we passed in
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="cptCode">
        /// The CPT code.
        /// </param>
        /// <param name="serviceDescription">
        /// The service description.
        /// </param>
        /// <param name="activeOnly">
        /// The active only.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetExamServiceItems(
            string officeNumber, string cptCode, string serviceDescription, bool activeOnly)
        {
            return this.Request.CreateResponse(
                HttpStatusCode.OK, 
                new
                    {
                        ExamServiceItems =
                            this.examServiceManager.GetExamServiceItems(
                                officeNumber, cptCode, serviceDescription, activeOnly), 
                        ItemGroups = this.examServiceManager.GetItemGroups().Select(ItemGroupVm.FromItem)
                    });
        }

        /// <summary>
        /// The save exam service items.
        /// </summary>
        /// <param name="officeNumber">
        /// The office number.
        /// </param>
        /// <param name="examServices">
        /// The exam services.
        /// </param>
        [HttpPut]
        public void SaveExamServiceItems(string officeNumber, IEnumerable<ExamService> examServices)
        {
            var common = new Common();
            var enumerable = examServices as ExamService[] ?? examServices.ToArray();
            this.examServiceManager.SaveExamServiceItems(enumerable, officeNumber, common.GetCompanyId(officeNumber));
        }
    }
} 