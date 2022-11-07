//-----------------------------------------------------------------------------
// <copyright file="DeltaSetOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IDeltaSet"/> that is a collection of <see cref="IDeltaSetItem"/>s.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [NonValidatingParameterBinding]
    public class DeltaSet<TStructuralType> : Collection<IDeltaSetItem>, IDeltaSet where TStructuralType : class
    {
        private Type _clrType;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private IList<string> _keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeltaSet{TStructuralType}"/> class.
        /// </summary>
        /// <param name="keys">List of key names for the type.</param>
        public DeltaSet(IList<string> keys)
        {
            _clrType = typeof(TStructuralType);
            _keys = keys;
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, IDeltaSetItem item)
        {
            Delta<TStructuralType> deltaItem = item as Delta<TStructuralType>;

            //To ensure we dont insert null or a non related type to deltaset
            if (deltaItem == null)
            {
                throw Error.Argument("item", SRResources.ChangedObjectTypeMismatch, item.GetType(), _clrType);
            }

            base.InsertItem(index, item);
        }
    }
}
