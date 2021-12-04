//-----------------------------------------------------------------------------
// <copyright file="TransientAnnotations.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData
{
    internal static class TransientAnnotations
    {
        internal static HashSet<string> TransientAnnotationTerms = new HashSet<string>() { SRResources.ContentID, SRResources.DataModificationException };
    }
}
