// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// The annotation used on ODataNavigationLink instances to store the list of children for that navigation link.
    /// </summary>
    /// <remarks>
    /// A navigation link for a singleton navigation property can only contain one item - either ODataEntry or ODataEntityReferenceLink.
    /// A navigation link for a collection navigation property can contain any number of items - each is either ODataFeed or ODataEntityReferenceLink.
    /// </remarks>
    internal sealed class ODataNavigationLinkAnnotation : List<ODataItem>
    {
    }
}
