// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a singleton in a path.
    /// </summary>
    public class SingletonPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonPathSegment" /> class.
        /// </summary>
        /// <param name="singleton">The singleton being accessed.</param>
        public SingletonPathSegment(IEdmSingleton singleton)
        {
            if (singleton == null)
            {
                throw Error.ArgumentNull("singleton");
            }

            Singleton = singleton;
            SingletonName = singleton.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonPathSegment" /> class.
        /// </summary>
        /// <param name="singletonName">Name of the singleton.</param>
        public SingletonPathSegment(string singletonName)
        {
            if (String.IsNullOrEmpty(singletonName))
            {
                throw Error.ArgumentNullOrEmpty("singletonName");
            }

            SingletonName = singletonName;
        }

        /// <summary>
        /// Gets the singleton represented by this segment.
        /// </summary>
        public IEdmSingleton Singleton { get; private set; }

        /// <summary>
        /// Gets the name of the singleton.
        /// </summary>
        public string SingletonName { get; private set; }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.Singleton;
            }
        }

        /// <inheritdoc/>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            if (Singleton != null)
            {
                return Singleton.EntityType();
            }

            return null;
        }

        /// <inheritdoc/>
        public override IEdmNavigationSource GetNavigationSource(IEdmNavigationSource previousNavigationSource)
        {
            return Singleton;
        }

        /// <summary>
        /// Returns a <see cref="String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return SingletonName;
        }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.Singleton)
            {
                SingletonPathSegment singletonSegment = (SingletonPathSegment)pathSegment;
                return singletonSegment.Singleton == Singleton && singletonSegment.SingletonName == SingletonName;
            }

            return false;
        }
    }
}
