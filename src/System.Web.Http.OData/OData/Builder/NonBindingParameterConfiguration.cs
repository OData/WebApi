// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Represents a non-binding procedure parameter.
    /// <remarks>
    /// Non binding parameters are provided in the POST body for Actions
    /// Non binding parameters are provided in 3 ways for Functions
    /// - ~/.../Function(p1=value)
    /// - ~/.../Function(p1=@x)?@x=value
    /// - ~/.../Function?p1=value (only allowed if the Function is the last url path segment).
    /// </remarks>
    /// </summary>
    public class NonbindingParameterConfiguration : ParameterConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NonbindingParameterConfiguration"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameterType">The EDM type of the parameter.</param>
        public NonbindingParameterConfiguration(string name, IEdmTypeConfiguration parameterType)
            : base(name, parameterType)
        {
            EdmTypeKind kind = parameterType.Kind;
            if (kind == EdmTypeKind.Collection)
            {
                kind = (parameterType as CollectionTypeConfiguration).ElementType.Kind;
            }
            if (kind == EdmTypeKind.Entity)
            {
                throw Error.Argument("parameterType", SRResources.InvalidParameterType, parameterType.FullName);
            }
        }
    }
}
