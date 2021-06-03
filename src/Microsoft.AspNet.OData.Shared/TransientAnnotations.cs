// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData
{
    internal static class TransientAnnotations
    {
        internal static HashSet<string> TransientAnnotationTerms = new HashSet<string>() { SRResources.ContentID, SRResources.DataModificationException };
    }
}
