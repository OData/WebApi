//-----------------------------------------------------------------------------
// <copyright file="ODataConventionModelBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Provides extension methods for the <see cref="ODataConventionModelBuilder"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataConventionModelBuilderExtensions
    {
        /// <summary>
        /// Enable lower camel case with default <see cref="NameResolverOptions"/>
        /// NameResolverOptions.ProcessReflectedPropertyNames |
        /// NameResolverOptions.ProcessDataMemberAttributePropertyNames |
        /// NameResolverOptions.ProcessExplicitPropertyNames.
        /// </summary>
        /// <param name="builder">The <see cref="ODataConventionModelBuilder"/> to be enabled with lower camel case.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public static ODataConventionModelBuilder EnableLowerCamelCase(this ODataConventionModelBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            return builder.EnableLowerCamelCase(
                NameResolverOptions.ProcessReflectedPropertyNames |
                NameResolverOptions.ProcessDataMemberAttributePropertyNames |
                NameResolverOptions.ProcessExplicitPropertyNames);
        }

        /// <summary>
        /// Enable lower camel case with given <see cref="NameResolverOptions"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ODataConventionModelBuilder"/> to be enabled with lower camel case.</param>
        /// <param name="options">The <see cref="NameResolverOptions"/> for the lower camel case.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public static ODataConventionModelBuilder EnableLowerCamelCase(
            this ODataConventionModelBuilder builder, 
            NameResolverOptions options)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }
            
            builder.OnModelCreating += new LowerCamelCaser(options).ApplyLowerCamelCase;
            return builder;
        }
    }
}
