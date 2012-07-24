// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.Http.Controllers;

namespace System.Web.Http.Validation
{
    public sealed class ModelValidatingEventArgs : CancelEventArgs
    {
        public ModelValidatingEventArgs(HttpActionContext actionContext, ModelValidationNode parentNode)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            ActionContext = actionContext;
            ParentNode = parentNode;
        }

        public HttpActionContext ActionContext { get; private set; }

        public ModelValidationNode ParentNode { get; private set; }
    }
}
