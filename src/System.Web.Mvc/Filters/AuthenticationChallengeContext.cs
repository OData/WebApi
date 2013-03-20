// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc.Filters
{
    public class AuthenticationChallengeContext : ControllerContext
    {
        private ActionDescriptor _actionDescriptor;
        private ActionResult _result;

        // parameterless constructor used for mocking
        public AuthenticationChallengeContext()
        {
        }

        public AuthenticationChallengeContext(ControllerContext controllerContext, ActionDescriptor actionDescriptor,
            ActionResult result)
            : base(controllerContext)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            _actionDescriptor = actionDescriptor;
            _result = result;
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

        public ActionResult Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }
    }
}
