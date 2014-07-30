// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace System.Web.OData.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class HttpActionDescriptorExtensions
    {
        // Maintain the System.Web.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v3 assembly.  Concern is reduced here due to addition of user type name but prefix
        // also clearly ties the property to code in this assembly.
        private const string ModelKeyPrefix = "System.Web.OData.Model+";

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
                    ODataConventionModelBuilder builder =
                        new ODataConventionModelBuilder(actionDescriptor.Configuration, isQueryCompositionMode: true);
                    EntityTypeConfiguration entityTypeConfiguration = builder.AddEntityType(entityClrType);
                    builder.AddEntitySet(entityClrType.Name, entityTypeConfiguration);
                    IEdmModel edmModel = builder.GetEdmModel();
                    Contract.Assert(edmModel != null);
                    return edmModel;
                }) as IEdmModel;
        }
    }
}
