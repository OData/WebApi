//-----------------------------------------------------------------------------
// <copyright file="EdmEntityObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Org.OData.Core.V1;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmEntityObject"/> with no backing CLR <see cref="Type"/>.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmEntityObject : EdmStructuredObject, IEdmEntityObject, IEdmChangedObject
    {
        private IODataInstanceAnnotationContainer persistentInstanceAnnotationsContainer;
        private IODataInstanceAnnotationContainer transientInstanceAnnotationsContainer;

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
        }

        /// <summary>
        /// Gets or sets the instance annotation container to hold persistent annotations.
        /// </summary>
        internal IODataInstanceAnnotationContainer PersistentInstanceAnnotationsContainer
        {
            get
            {
                if (persistentInstanceAnnotationsContainer == null)
                {
                    persistentInstanceAnnotationsContainer = new ODataInstanceAnnotationContainer();
                }

                return persistentInstanceAnnotationsContainer;
            }

            set { persistentInstanceAnnotationsContainer = value; }
        }

        /// <summary>
        /// Gets or sets the instance annotation container to hold transient annotations.
        /// </summary>
        internal IODataInstanceAnnotationContainer TransientInstanceAnnotationContainer
        {
            get
            {
                if (transientInstanceAnnotationsContainer == null)
                {
                    transientInstanceAnnotationsContainer = new ODataInstanceAnnotationContainer();
                }

                return transientInstanceAnnotationsContainer;
            }

            set { transientInstanceAnnotationsContainer = value; }
        }

        /// <summary>
        /// Gets or sets the container to hold ODataId.
        /// </summary>
        internal ODataIdContainer ODataIdContainer { get; set; }

        /// <summary>
        /// Gets or sets the OData path for the item.
        /// </summary>
        public ODataPath ODataPath { get; set; }

        /// <summary>
        /// Gets the kind of object within the delta payload.
        /// </summary>
        public virtual EdmDeltaEntityKind DeltaKind { get { return EdmDeltaEntityKind.Entry; } }

        /// <summary>
        /// Adds the specified data modification exception.
        /// </summary>
        public void AddDataException(DataModificationExceptionType dataModificationException)
        {
            TransientInstanceAnnotationContainer.AddResourceAnnotation(SRResources.DataModificationException, dataModificationException);
        }

        /// <summary>
        /// Gets the data modification exception.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public DataModificationExceptionType GetDataException()
        {
            DataModificationExceptionType dataModificationExceptionType = TransientInstanceAnnotationContainer.GetResourceAnnotation(SRResources.DataModificationException) as DataModificationExceptionType;

            return dataModificationExceptionType;
        }
    }
}
