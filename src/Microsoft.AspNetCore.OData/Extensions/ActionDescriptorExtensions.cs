//-----------------------------------------------------------------------------
// <copyright file="ActionDescriptorExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class ActionDescriptorExtensions
    {
        // Maintain the Microsoft.AspNet.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v3 assembly.  Concern is reduced here due to addition of user type name but prefix
        // also clearly ties the property to code in this assembly.
        private const string ModelKeyPrefix = "Microsoft.AspNet.OData.Model+";

        private static readonly object SyncLock = new object();

        internal static IEdmModel GetEdmModel(this ActionDescriptor actionDescriptor, HttpRequest request, Type entityClrType)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (entityClrType == null)
            {
                throw Error.ArgumentNull("entityClrType");
            }

            IEdmModel model = null;

            string key = ModelKeyPrefix + entityClrType.FullName;
            object modelAsObject = null;
            if (actionDescriptor.Properties.TryGetValue(key, out modelAsObject))
            {
                model = modelAsObject as IEdmModel;
            }
            else
            {
                ODataConventionModelBuilder builder =
                    new ODataConventionModelBuilder(request.HttpContext.RequestServices, isQueryCompositionMode: true);
                EntityTypeConfiguration entityTypeConfiguration = builder.AddEntityType(entityClrType);
                builder.AddEntitySet(entityClrType.Name, entityTypeConfiguration);
                model = builder.GetEdmModel();

                lock (SyncLock)
                {
                    if (!actionDescriptor.Properties.ContainsKey(key))
                    {
                        actionDescriptor.Properties.Add(key, model);
                    }
                }
            }

            return model;
        }
    }
}
