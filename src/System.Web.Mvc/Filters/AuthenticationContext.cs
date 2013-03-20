// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Mvc.Filters
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

        public IPrincipal Principal { get; set; }

        public ActionResult Result { get; set; }
    }
}
