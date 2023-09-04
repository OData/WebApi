//-----------------------------------------------------------------------------
// <copyright file="SelectExpandWrapperOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// Represents a container class that contains properties that are either selected or expanded using $select and $expand.
    /// </summary>
    /// <typeparam name="TElement">The element being selected and expanded.</typeparam>
    [JsonConverter(typeof(SelectExpandWrapperConverter))]
#if NETCOREAPP3_1_OR_GREATER
    [System.Text.Json.Serialization.JsonConverter(typeof(SelectExpandWrapperJsonConverter))]
#endif
    internal class SelectExpandWrapper<TElement> : SelectExpandWrapper
    {
        /// <summary>
        /// Gets or sets the instance of the element being selected and expanded.
        /// </summary>
        public TElement Instance
        {
            get { return (TElement)UntypedInstance; }
            set { UntypedInstance = value; }
        }

        protected override Type GetElementType()
        {
            return UntypedInstance == null ? typeof(TElement) : UntypedInstance.GetType();
        }
    }
}
