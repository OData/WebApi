// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Routing;

namespace System.Web.Mvc
{
    public interface IController
    {
        void Execute(RequestContext requestContext);
    }
}
