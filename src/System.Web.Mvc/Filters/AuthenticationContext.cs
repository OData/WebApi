// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Mvc.Filters
{
    /// <summary>Represents an authentication context containing information for performing authentication.</summary>
    public class AuthenticationContext : ControllerContext
    {
        private ActionDescriptor _actionDescriptor;

        /// <summary>Initializes a new instance of the <see cref="AuthenticationContext"/> class.</summary>
        /// <remarks>This constructor should only be used for unit testing purposes.</remarks>
        public AuthenticationContext()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AuthenticationContext"/> class.</summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        public AuthenticationContext(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            : base(controllerContext)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            _actionDescriptor = actionDescriptor;
        }

        /// <summary>Gets the action descriptor.</summary>
        public ActionDescriptor ActionDescriptor
        {
            get
            {
                return _actionDescriptor;
            }
            set
            {
                _actionDescriptor = value;
            }
        }

        /// <summary>Gets or sets the currently authenticated principal.</summary>
        public IPrincipal Principal { get; set; }

        /// <summary>
        /// Gets or sets the error result, which indicates that authentication was attempted and failed.
        /// </summary>
        public ActionResult ErrorResult { get; set; }
    }
}
