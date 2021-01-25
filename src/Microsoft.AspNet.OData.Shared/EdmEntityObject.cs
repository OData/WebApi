// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Org.OData.Core.V1;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmEntityObject"/> with no backing CLR <see cref="Type"/>.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmEntityObject : EdmStructuredObject, IEdmEntityObject, IEntityObjectInstanceAnnotations
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdmEntityObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmEntityType"/> of this object.</param>
        public EdmEntityObject(IEdmEntityType edmType)
            : this(edmType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmEntityObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmEntityTypeReference"/> of this object.</param>
        public EdmEntityObject(IEdmEntityTypeReference edmType)
            : this(edmType.EntityDefinition(), edmType.IsNullable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmEntityObject"/> class.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmEntityType"/> of this object.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmEntityObject(IEdmEntityType edmType, bool isNullable)
            : base(edmType, isNullable)
        {
            TransientInstanceAnnotationContainer = new ODataInstanceAnnotationContainer();
            PersistentInstanceAnnotationsContainer = new ODataInstanceAnnotationContainer();
        }

        /// <inheritdoc />
        public IODataInstanceAnnotationContainer TransientInstanceAnnotationContainer { get; set; }

        /// <inheritdoc />
        public IODataInstanceAnnotationContainer PersistentInstanceAnnotationsContainer { get; set; }

        /// <summary>
        /// Method to Add DAta Modification Exception
        /// </summary>
        public void AddDataException(DataModificationExceptionType dataModificationException)
        {
            TransientInstanceAnnotationContainer.AddResourceAnnotation("Core.DataModificationException", dataModificationException);           
        }

        /// <summary>
        /// Method to Add DAta Modification Exception
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public object GetDataException()
        {
            return TransientInstanceAnnotationContainer.GetResourceAnnotation("Core.DataModificationException");
        }
    }
}
