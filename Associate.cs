// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Associate.cs" company="Eyefinity, Inc.">
//   Copyright © 2013 Eyefinity, Inc.  All rights reserved.
// </copyright>
// <summary>
//   Model for the Reset Password
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Eyefinity.PracticeManagement.Model
{
    /// <summary>
    /// The associate.
    /// </summary>
    public class Associate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Associate"/> class.
        /// </summary>
        public Associate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Associate"/> class.
        /// </summary>
        /// <param name="lastName">
        /// The last name.
        /// </param>
        /// <param name="firstName">
        /// The first name.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="roles">
        /// The roles.
        /// </param>
        /// <param name="employeeId">
        /// The employee id.
        /// </param>
        public Associate(string lastName, string firstName, string userName, string roles, int employeeId)
        {
            this.LastName    = lastName;
            this.FirstName   = firstName;
            this.UserName = userName;
            this.Roles = roles;
            this.EmployeeId = employeeId;
        }

        /// <summary>
        /// Gets or sets the employee id.
        /// </summary>
        public int EmployeeId { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        public string Roles { get; set; }
    }
}