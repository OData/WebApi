// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    // Each entry we read in the deserializer is annotated with the ODataEntryAnnotation.
    // The ODataEntry class doesn't contain the list of navigation properties and hence needs this annotation.
    internal sealed class ODataEntryAnnotation : List<ODataNavigationLink>
    {
        // The entity resource update token for the entry.
        internal object EntityResource { get; set; }

        // The resolved entity type for the entry.
        internal IEdmEntityTypeReference EntityType { get; set; }
    }
}
