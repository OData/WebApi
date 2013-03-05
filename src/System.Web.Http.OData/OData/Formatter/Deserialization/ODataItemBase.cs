// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Base class for all classes that wrap an <see cref="ODataItem"/>.
    /// </summary>
    public abstract class ODataItemBase
    {
        private ODataItem _item;

        /// <summary>
        /// Initializes a new instance of <see cref="ODataItemBase"/>.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        protected ODataItemBase(ODataItem item)
        {
            _item = item;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataItem"/>.
        /// </summary>
        public ODataItem Item
        {
            get
            {
                return _item;
            }
        }
    }
}
