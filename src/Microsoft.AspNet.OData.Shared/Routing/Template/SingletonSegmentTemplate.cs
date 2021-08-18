//-----------------------------------------------------------------------------
// <copyright file="SingletonSegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="SingletonSegment"/>.
    /// </summary>
    public class SingletonSegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The singleton segment</param>
        public SingletonSegmentTemplate(SingletonSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            Segment = segment;
        }

        /// <summary>
        /// Gets or sets the singleton segment.
        /// </summary>
        public SingletonSegment Segment { get; private set; }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            SingletonSegment otherSingleton = pathSegment as SingletonSegment;
            return otherSingleton != null && otherSingleton.Singleton == Segment.Singleton;
        }
    }
}
