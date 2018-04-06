// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;

namespace Microsoft.Test.AspNet.OData.Builder
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
