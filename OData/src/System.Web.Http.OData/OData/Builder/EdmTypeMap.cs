// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;

namespace System.Web.Http.OData.Builder
{
    internal class EdmTypeMap
    {
        public EdmTypeMap(
            Dictionary<Type, IEdmStructuredType> edmTypes,
            IEnumerable<IEdmDirectValueAnnotationBinding> directValueAnnotations)
        {
            EdmTypes = edmTypes;
            DirectValueAnnotations = directValueAnnotations;
        }

        public Dictionary<Type, IEdmStructuredType> EdmTypes { get; private set; }

        public IEnumerable<IEdmDirectValueAnnotationBinding> DirectValueAnnotations { get; private set; }
    }
}
