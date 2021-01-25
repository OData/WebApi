// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using System.Collections.Generic;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// <see cref="IEntityObjectInstanceAnnotations" />Interface to hold instance annotations
    /// </summary>
    public interface IEntityObjectInstanceAnnotations
    {
        /// <summary>
        /// Instance Annotation container
        /// </summary>
        IODataInstanceAnnotationContainer TransientInstanceAnnotationContainer { get; set; }

        /// <summary>
        /// To hold Persistent Annotations
        /// </summary>
        IODataInstanceAnnotationContainer PersistentInstanceAnnotationsContainer { get; set; }
    }

    internal static class TransientAnnotations
    {
        internal static HashSet<string> TransientAnnotationContainer = new HashSet<string>() { "Core.ContentID", "Core.DataModificationException" };
    }
}
