// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace System.Web.OData
{
    public static class ODataPathHandlerExtensions
    {
        public static ODataPath Parse(this IODataPathHandler handler, IEdmModel model, string serviceRoot, string odataPath)
        {
            Contract.Assert(handler != null);

            return handler.Parse(model, serviceRoot, odataPath, DependencyInjectionHelper.BuildContainer(null));
        }

        public static ODataPathTemplate ParseTemplate(this IODataPathTemplateHandler handler, IEdmModel model, string odataPathTemplate)
        {
            Contract.Assert(handler != null);

            return handler.ParseTemplate(model, odataPathTemplate, DependencyInjectionHelper.BuildContainer(null));
        }
    }
}
