// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public class HandleErrorInfo
    {
        public HandleErrorInfo(Exception exception, string controllerName, string actionName)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            if (String.IsNullOrEmpty(controllerName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "controllerName");
            }
            if (String.IsNullOrEmpty(actionName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "actionName");
            }

            Exception = exception;
            ControllerName = controllerName;
            ActionName = actionName;
        }

        public string ActionName { get; private set; }

        public string ControllerName { get; private set; }

        public Exception Exception { get; private set; }
    }
}
