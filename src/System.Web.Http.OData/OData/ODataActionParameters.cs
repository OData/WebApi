// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
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
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }
            if (context.Request == null || context.Request.RequestUri == null)
            {
                throw Error.InvalidOperation(SRResources.DeserializerContextRequirementsNotSatisfied);
            }

            ODataPath path = context.Request.GetODataPath();
            if (path == null)
            {
                throw Error.InvalidOperation(SRResources.RequestNotODataPath, context.Request.RequestUri);
            }

            ActionPathSegment lastSegment = path.Segments.Last() as ActionPathSegment;
            if (lastSegment == null)
            {
                throw Error.InvalidOperation(SRResources.RequestNotActionInvocation, context.Request.RequestUri);
            }
            return lastSegment.Action;
        }
    }
}