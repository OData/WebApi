// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Values;

namespace System.Web.Http.OData
{
    internal class StoreGeneratedPatternAnnotation : IEdmDirectValueAnnotationBinding
    {
        private readonly IEdmElement element;

        internal const string AnnotationsNamespace = "http://schemas.microsoft.com/ado/2009/02/edm/annotation";

        internal const string AnnotationName = "StoreGeneratedPattern";

        private readonly object value;

        public StoreGeneratedPatternAnnotation(IEdmElement element, DatabaseGeneratedOption option)
        {
            this.element = element;
            this.value = new EdmStringConstant(EdmCoreModel.Instance.GetString(isNullable: false), option.ToString());
        }

        public IEdmElement Element
        {
            get { return this.element; }
        }

        /// <inheritdoc/>
        public string NamespaceUri
        {
            get { return AnnotationsNamespace; }
        }

        /// <inheritdoc/>
        public string Name
        {
            get { return AnnotationName; }
        }

        /// <inheritdoc/>
        public object Value
        {
            get { return value; }
        }
    }
}