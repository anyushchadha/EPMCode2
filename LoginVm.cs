// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginVm.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Eyefinity.PracticeManagement.Model.ViewModel
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     The login view model.
    /// </summary>
    public class LoginVm
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the action. Possible values are "Verify" or "Reset"
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether emergency access.
        /// </summary>
        public bool EmergencyAccess { get; set; }

        /// <summary>
        ///     Gets a value indicating whether is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.PracticeLocationId) && !string.IsNullOrWhiteSpace(this.LoginName)
                       && !string.IsNullOrWhiteSpace(this.Password);
            }
        }

        /// <summary>
        ///     Gets or sets the login name.
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// Gets or sets the office id.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed.")]
        public int OfficeId { get; set; }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        public string OldPassword { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether password verified.
        /// </summary>
        public string PasswordCheckResult { get; set; }

        /// <summary>
        ///     Gets or sets the password policy.
        /// </summary>
        public string PasswordPolicy { get; set; }

        /// <summary>
        ///     Gets or sets the office location id.
        /// </summary>
        public string PracticeLocationId { get; set; }

        /// <summary>
        ///     Gets or sets the company id that the office belongs to.
        /// </summary>
        public string CompanyId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remember my username.
        /// </summary>
        public bool RememberMyUsername { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remember office number.
        /// </summary>
        public bool RememberOfficeNumber { get; set; }

        /// <summary>
        ///     Gets or sets the user id.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        ///     Gets or sets the redirect url.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is day close authorized.
        /// </summary>
        public bool DayCloseInRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is day close authorized.
        /// </summary>
        public bool ChangePaymentTypesInRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is office is ehr enabled.
        /// </summary>
        public bool IsEhrEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating number of login attempts.
        /// </summary>
        public int NumberOfAttempts { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the ZipCode.
        /// </summary>
        public string CompanyZipCode { get; set; }

        /// <summary>
        /// Gets or sets the Office Phone Number.
        /// </summary>
        public string CompanyPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Security Question.
        /// </summary>
        public string SecurityQuestion { get; set; }

        /// <summary>
        /// Gets or sets the Security Answer.
        /// </summary>
        public string SecurityAnswer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating number of login attempts.
        /// </summary>
        public int NumberOfPasswordResetAttempts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this office has on premise Ecr Vault.
        /// </summary>
        public bool IsOnPremEcrVault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this office has on new Exam UI enabled
        /// </summary>
        public bool IsNewExamUiEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this office has on new Contact Lens Orders UI enabled
        /// </summary>
        public bool IsNewClOrderUiEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this office has on new Eyeglass Orders UI enabled
        /// </summary>
        public bool IsNewEgOrderUiEnabled { get; set; }

        #endregion
    }
}