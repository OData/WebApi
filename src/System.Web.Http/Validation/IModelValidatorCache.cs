// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// Defines a cache for <see cref="ModelValidator"/>s. This cache is keyed on the type or property that the metadata is associated with.
    /// </summary>
    internal interface IModelValidatorCache
    {
        ModelValidator[] GetValidators(ModelMetadata metadata);
    }
}
