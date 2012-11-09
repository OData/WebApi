// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing an indexing into an entity collection by key.
    /// </summary>
    public class KeyValuePathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePathSegment" /> class.
        /// </summary>
        /// <param name="previous">The previous segment in the path.</param>
        /// <param name="value">The key value to use for indexing into the collection.</param>
        public KeyValuePathSegment(ODataPathSegment previous, string value)
            : base(previous)
        {
            if (value == null)
            {
                throw Error.ArgumentNull("value");
            }

            IEdmCollectionType previousEdmType = previous.EdmType as IEdmCollectionType;
            if (previousEdmType == null)
            {
                throw Error.Argument(SRResources.PreviousTypeForKeyMustBeCollection, previous.EdmType); 
            }

            EdmType = previousEdmType.ElementType.Definition;
            EntitySet = previous.EntitySet;
            Value = value;
        }

        /// <summary>
        /// Gets the key value to use for indexing into the collection.
        /// </summary>
        public string Value
        {
            get;
            private set;
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
    }
}
