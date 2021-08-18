//-----------------------------------------------------------------------------
// <copyright file="NonBindingParameterConfiguration.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Represents a non-binding operation parameter.
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
        }
    }
}
