//-----------------------------------------------------------------------------
// <copyright file="ODataConventionModelBuilderFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Web.Http;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create ODataConventionModelBuilder.
    /// </summary>
    public class ODataConventionModelBuilderFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder Create()
        {
            return new ODataConventionModelBuilder();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder Create(HttpConfiguration configuration)
        {
            return new ODataConventionModelBuilder(configuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <param name="isQueryCompositionMode">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder Create(HttpConfiguration configuration, bool isQueryCompositionMode)
        {
            return new ODataConventionModelBuilder(configuration, isQueryCompositionMode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="modelAliasing">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder CreateWithModelAliasing(bool modelAliasing)
        {
            ODataConventionModelBuilder builder = Create();
            builder.ModelAliasingEnabled = modelAliasing;
            return builder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <param name="modelAliasing">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder CreateWithModelAliasing(HttpConfiguration configuration, bool modelAliasing)
        {
            return new ODataConventionModelBuilder(configuration) { ModelAliasingEnabled = modelAliasing };
        }
    }
}
