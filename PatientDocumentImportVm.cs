namespace Eyefinity.PracticeManagement.Model.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using IT2.Core;

    /// <summary>
    /// The Patient Document Import view model.
    /// </summary>
    public class PatientDocumentImportVm
    {
        /// <summary>
        ///     Gets or sets the Property Name (Key)
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        ///     Gets or sets the IsChanged Property
        /// </summary>
        public bool IsChanged { get; set; }

        /// <summary>
        ///     Gets or sets the Isprimary Property (Used for Adresses, Phones and Insurances Only
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        ///     Gets or sets the IsSecondary Property (Used for Phones Only
        /// </summary>
        public bool IsSecondary { get; set; }

        /// <summary>
        ///     Gets or sets the Property Name (Key)
        /// </summary>
        public string DisplayValue { get; set; }

        /// <summary>
        ///     Gets or sets the Original Value (From Db)
        /// </summary>
        public string DBValue { get; set; }

        /// <summary>
        ///     Gets or sets the New Value (from JSON)
        /// </summary>
        public string JSONValue { get; set; }

        /// <summary>
        ///     Gets or sets the ID value associated with the New JSON Value
        /// </summary>
        public int? JSONIdValue { get; set; }

        /// <summary>
        ///     Gets or sets the ID String value associated with the New JSON Value
        /// </summary>
        public string JSONIdValueString { get; set; }

        /// <summary>
        ///     Gets or sets the FormDisplayAttribute propertyassociated with Property Name
        /// </summary>
        public FormDisplayAttribute FormDisplayAttribute { get; set; }

        /// <summary>
        ///     Gets or sets the JSON Array Order for the the Phone
        /// </summary>
        public int AddressSubmitOrder { get; set; }

        /// <summary>
        ///     Gets or sets the JsonAddress property (Address filled from JSON )
        /// </summary>
        public IT2.Core.Address JsonAddress { get; set; }

        /// <summary>
        ///     Gets or sets the CurrentAddress property (Address filled from DB )
        /// </summary>
        public IT2.Core.Address DBAddress { get; set; }

        /// <summary>
        ///     Gets or sets the JSON Array Order for the the Phone
        /// </summary>
        public int PhoneSubmitOrder { get; set; }

        /// <summary>
        ///     Gets or sets the JsonPhone property (Phone filled from JSON )
        /// </summary>
        public IT2.Core.Phone JsonPhone { get; set; }

        /// <summary>
        ///     Gets or sets the CurrentPhone property (Phone filled from DB )
        /// </summary>
        public IT2.Core.Phone DBPhone { get; set; }
    }
}
