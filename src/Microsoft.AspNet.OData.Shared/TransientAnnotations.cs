// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using System.Collections.Generic;

namespace Microsoft.AspNet.OData
{
    internal static class TransientAnnotations
    {
        internal static HashSet<string> TransientAnnotationTerms = new HashSet<string>() { "Core.ContentID", "Core.DataModificationException" };
    }
}
