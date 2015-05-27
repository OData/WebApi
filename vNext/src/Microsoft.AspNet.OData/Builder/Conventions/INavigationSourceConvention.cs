// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData.Builder.Conventions
{
    internal interface INavigationSourceConvention : IConvention
    {
        void Apply(INavigationSourceConfiguration configuration, ODataModelBuilder model);
    }
}
