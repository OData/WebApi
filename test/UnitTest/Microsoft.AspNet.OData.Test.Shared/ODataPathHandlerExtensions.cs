﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.AspNet.OData
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
                action = b => b.AddService(ServiceLifetime.Singleton, sp => model);
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
