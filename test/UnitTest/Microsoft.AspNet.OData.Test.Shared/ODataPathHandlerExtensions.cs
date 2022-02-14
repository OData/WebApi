//-----------------------------------------------------------------------------
// <copyright file="ODataPathHandlerExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test
{
    public static class ODataPathHandlerExtensions
    {
        public static ODataPath Parse(this IODataPathHandler handler, IEdmModel model, string serviceRoot,
            string odataPath, ODataUriResolver resolver = null)
        {
            Contract.Assert(handler != null);
            Action<IContainerBuilder> action;
            if (resolver != null)
            {
                action = b => b.AddService(ServiceLifetime.Singleton, sp => model)
                    .AddService(ServiceLifetime.Singleton, sp => resolver);
            }
            else
            {
                // By default, create the Uri resolver for unqualified functions & actions,
                // and existing handling of namespace-qualified functions & actions is still preserved.
                action = b => b.AddService(ServiceLifetime.Singleton, sp => model)
                    .AddService(
                        ServiceLifetime.Singleton,
                        typeof(ODataUriResolver),
                        sp => new UnqualifiedCallAndEnumPrefixFreeResolver
                        {
                            EnableCaseInsensitive = false
                        });
            }

            return handler.Parse(serviceRoot, odataPath, new MockContainer(action));
        }

        public static ODataPathTemplate ParseTemplate(this IODataPathTemplateHandler handler, IEdmModel model, string odataPathTemplate)
        {
            Contract.Assert(handler != null);

            return handler.ParseTemplate(odataPathTemplate, new MockContainer(model));
        }
    }
}
