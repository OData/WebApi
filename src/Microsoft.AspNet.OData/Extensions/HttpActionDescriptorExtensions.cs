// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class HttpActionDescriptorExtensions
    {
        // Maintain the Microsoft.AspNet.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v3 assembly.  Concern is reduced here due to addition of user type name but prefix
        // also clearly ties the property to code in this assembly.
        private const string ModelKeyPrefix = "Microsoft.AspNet.OData.Model+";

        internal static IEdmModel GetEdmModel(this HttpActionDescriptor actionDescriptor, Type entityClrType)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            if (entityClrType == null)
            {
                throw Error.ArgumentNull("entityClrType");
            }

            // save the EdmModel to the action descriptor
            return actionDescriptor.Properties.GetOrAdd(ModelKeyPrefix + entityClrType.FullName, _ =>
                {
                    IAssembliesResolver resolver = actionDescriptor.Configuration.Services.GetAssembliesResolver();
                    ODataConventionModelBuilder builder =
                        new ODataConventionModelBuilder(new WebApiAssembliesResolver(resolver), isQueryCompositionMode: true);
                    EntityTypeConfiguration entityTypeConfiguration = builder.AddEntityType(entityClrType);
                    builder.AddEntitySet(entityClrType.Name, entityTypeConfiguration);
                    IEdmModel edmModel = builder.GetEdmModel();
                    Contract.Assert(edmModel != null);
                    return edmModel;
                }) as IEdmModel;
        }
    }
}
