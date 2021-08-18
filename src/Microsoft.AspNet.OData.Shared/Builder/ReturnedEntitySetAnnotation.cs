//-----------------------------------------------------------------------------
// <copyright file="ReturnedEntitySetAnnotation.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// This annotation indicates the mapping from an <see cref="IEdmOperation"/> to a <see cref="String"/>.
    /// The <see cref="IEdmOperation"/> is a bound action/function and the <see cref="String"/> is the
    /// entity set name given by user to indicate the entity set returned from this action/function.
    /// </summary>
    internal class ReturnedEntitySetAnnotation
    {
        public ReturnedEntitySetAnnotation(string entitySetName)
        {
            if (String.IsNullOrEmpty(entitySetName))
            {
                throw Error.ArgumentNullOrEmpty("entitySetName");
            }

            EntitySetName = entitySetName;
        }

        public string EntitySetName
        {
            get;
            private set;
        }
    }
}
