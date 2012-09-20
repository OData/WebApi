// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// ActionPayload holds the Parameter names and values provided by a client in a POST request
    /// to invoke a particular Action. The Parameter values are stored in the dictionary keyed using the Parameter name.
    /// </summary>
    [ODataParameterBinding]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Pending, will remove once class has appropriate base type.")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Pending, will remove once class has appropriate base type.")]
    public class ODataActionParameters : Dictionary<string, object>
    {
        /// <summary>
        /// Gets the IEdmFunctionImport that describes the payload.
        /// </summary>
        public virtual IEdmFunctionImport GetFunctionImport(ODataDeserializerContext context)
        {
            HttpConfiguration configuration = context.Request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
            }
            IODataActionResolver resolver = configuration.GetODataActionResolver();
            Contract.Assert(resolver != null);
            return resolver.Resolve(context);
        }
    }
}