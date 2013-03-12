// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Mvc
{
    public class AuthenticationContext : ControllerContext
    {
        private ActionDescriptor _actionDescriptor;

        // parameterless constructor used for mocking
        public AuthenticationContext()
        {
        }

        public AuthenticationContext(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            : base(controllerContext)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            _actionDescriptor = actionDescriptor;
        }

        public virtual ActionDescriptor ActionDescriptor
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

        public virtual IPrincipal Principal { get; set; }

        public virtual ActionResult Result { get; set; }
    }
}
