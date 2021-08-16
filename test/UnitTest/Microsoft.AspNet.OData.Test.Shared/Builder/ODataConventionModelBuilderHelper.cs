//-----------------------------------------------------------------------------
// <copyright file="ODataConventionModelBuilderHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Abstraction;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class ODataConventionModelBuilderHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="modelAliasing">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder CreateWithModelAliasing(bool modelAliasing)
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ModelAliasingEnabled = modelAliasing;
            return builder;
        }
    }
}
