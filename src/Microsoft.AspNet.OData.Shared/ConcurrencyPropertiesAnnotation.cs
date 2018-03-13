// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Annotation to store cache for concurrency properties
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Annotation suffix is more appropriate here.")]
    public class ConcurrencyPropertiesAnnotation : ConcurrentDictionary<IEdmNavigationSource, IEnumerable<IEdmStructuralProperty>>
    {
    }
}
