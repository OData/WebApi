// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// Represents a container class that contains properties that are either selected or expanded using $select and $expand.
    /// </summary>
    /// <typeparam name="TElement">The element being selected and expanded.</typeparam>
    [JsonConverter(typeof(SelectExpandWrapperConverter))]
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
