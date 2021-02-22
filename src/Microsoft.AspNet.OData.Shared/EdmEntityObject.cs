// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Org.OData.Core.V1;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmEntityObject"/> with no backing CLR <see cref="Type"/>.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmEntityObject : EdmStructuredObject, IEdmEntityObject
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
        }

        /// <summary>
        /// Instance Annotation container to hold Transient Annotations
        /// </summary>
        internal IODataInstanceAnnotationContainer TransientInstanceAnnotationContainer { get; set; }

        /// <summary>
        /// Instance Annotation container to hold Persistent Annotations
        /// </summary>/// <inheritdoc />
        public IODataInstanceAnnotationContainer PersistentInstanceAnnotationsContainer { get; set; }

        /// <summary>
        /// To set Persistent Instance Annotation
        /// </summary>        
        /// <param name="value">InstanceAnnotation container value</param>
        /// <returns>boolean representing whether the setting of instanceannotation was successful</returns>
        public virtual bool TrySetInstanceAnnotations(IODataInstanceAnnotationContainer value)
        {
            PersistentInstanceAnnotationsContainer = value;
            return true;
        }

        /// <summary>
        /// To get Persistent Instance Annotation
        /// </summary>        
        /// <returns>persistence instanceannotation container </returns>
        public virtual IODataInstanceAnnotationContainer TryGetInstanceAnnotations()
        {
            return PersistentInstanceAnnotationsContainer;            
        }

        /// <summary>
        /// Method to Add Data Modification Exception
        /// </summary>
        public void AddDataException(DataModificationExceptionType dataModificationException)
        {
            TransientInstanceAnnotationContainer.AddResourceAnnotation("Core.DataModificationException", dataModificationException);           
        }

        /// <summary>
        /// Method to Add Data Modification Exception
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public object GetDataException()
        {
            return TransientInstanceAnnotationContainer.GetResourceAnnotation("Core.DataModificationException");
        }
    }
}
