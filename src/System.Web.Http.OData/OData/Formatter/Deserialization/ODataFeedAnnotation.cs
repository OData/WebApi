// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// The annotation used on ODataFeed instances to store the list of entries in that feed.
    /// </summary>
    internal sealed class ODataFeedAnnotation : List<ODataEntry>
    {
    }
}
