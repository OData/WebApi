﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing an indexing into an entity collection by key.
    /// </summary>
    public class KeyValuePathSegment : ODataPathSegment
    {
        private IDictionary<string, string> _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePathSegment" /> class.
        /// </summary>
        /// <param name="segment">The key segment.</param>
        public KeyValuePathSegment(KeySegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            Segment = segment;
            Value = segment.TranslateKeyValueToString();
        }

        /// <summary>
        /// Gets or sets the key segment.
        /// </summary>
        public KeySegment Segment { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePathSegment" /> class.
        /// </summary>
        /// <param name="value">The key value to use for indexing into the collection.</param>
        public KeyValuePathSegment(string value)
        {
            if (value == null)
            {
                throw Error.ArgumentNull("value");
            }

            Value = value;
        }

        /// <summary>
        /// The raw text of the path segment and the source of the Values collection.
        /// </summary>
        public string Value
        {
            get;
            private set;
        }

        internal IDictionary<string, string> Values
        {
            get
            {
                if (_values == null)
                {
                    if (Segment != null)
                    {
                        _values = Segment.TranslateKeyValueToDictionary();
                    }
                    else
                    {
                        _values = KeyValueParser.ParseKeys(Value);
                    }
                }

                return _values;
            }
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.Key;
            }
        }

        /// <inheritdoc/>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            if (Segment != null)
            {
                return Segment.EdmType;
            }

            IEdmCollectionType previousCollectionType = previousEdmType as IEdmCollectionType;
            if (previousCollectionType != null)
            {
                return previousCollectionType.ElementType.Definition;
            }

            return null;
        }

        /// <inheritdoc/>
        public override IEdmNavigationSource GetNavigationSource(IEdmNavigationSource previousNavigationSource)
        {
            if (Segment != null)
            {
                return Segment.NavigationSource;
            }
            return previousNavigationSource;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Value;
        }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            return pathSegment.SegmentKind == ODataSegmentKinds.Key;
        }
    }
}
