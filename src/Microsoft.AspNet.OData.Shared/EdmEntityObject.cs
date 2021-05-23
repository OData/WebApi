// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;
using Org.OData.Core.V1;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmEntityObject"/> with no backing CLR <see cref="Type"/>.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmEntityObject : EdmStructuredObject, IEdmEntityObject, IEdmChangedObject
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
            PersistentInstanceAnnotationsContainer = new ODataInstanceAnnotationContainer();
            TransientInstanceAnnotationContainer = new ODataInstanceAnnotationContainer();            
        }

        /// <summary>
        /// Instance Annotation container to hold Transient Annotations
        /// </summary>
        internal IODataInstanceAnnotationContainer TransientInstanceAnnotationContainer { get; set; }

        /// <summary>
        /// Instance Annotation container to hold Persistent Annotations
        /// </summary>
        public IODataInstanceAnnotationContainer PersistentInstanceAnnotationsContainer { get; set; }

        /// <summary>
        /// DeltaKind as Entry
        /// </summary>
        public virtual EdmDeltaEntityKind DeltaKind { get { return EdmDeltaEntityKind.Entry; } }

        /// <summary>
        /// Method to Add Data Modification Exception
        /// </summary>
        public void AddDataException(DataModificationExceptionType dataModificationException)
        {
            Contract.Assert(TransientInstanceAnnotationContainer != null);

            TransientInstanceAnnotationContainer.AddResourceAnnotation(SRResources.DataModificationException, dataModificationException);           
        }

        /// <summary>
        /// Method to Add Data Modification Exception
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public DataModificationExceptionType GetDataException()
        {
            Contract.Assert(TransientInstanceAnnotationContainer != null);

            DataModificationExceptionType dataModificationExceptionType = TransientInstanceAnnotationContainer.GetResourceAnnotation(SRResources.DataModificationException) as DataModificationExceptionType;

            return dataModificationExceptionType;
        }
    }
}
