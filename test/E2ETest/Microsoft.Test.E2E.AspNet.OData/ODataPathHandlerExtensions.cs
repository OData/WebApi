//-----------------------------------------------------------------------------
// <copyright file="ODataPathHandlerExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.Test.E2E.AspNet.OData
{
    public static class ODataPathHandlerExtensions
    {
        private static readonly IServiceProvider DefaultContainer = null/*new DefaultContainerBuilder().BuildContainer()*/;

        public static ODataPath Parse(this IODataPathHandler handler, IEdmModel model, string serviceRoot, string odataPath)
        {
            Contract.Assert(handler != null);

            return handler.Parse(serviceRoot, odataPath, DefaultContainer);
        }

        public static ODataPathTemplate ParseTemplate(this IODataPathTemplateHandler handler, IEdmModel model, string odataPathTemplate)
        {
            Contract.Assert(handler != null);

            return handler.ParseTemplate(odataPathTemplate, DefaultContainer);
        }
    }
}
