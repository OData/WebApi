//-----------------------------------------------------------------------------
// <copyright file="IODataInstanceAnnotationContainer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Identifies a container for holding instance annotations. A default implementation is provided.
    /// Customers can have their own implementation.
    /// </summary>
    public interface IODataInstanceAnnotationContainer
    {
        /// <summary>
        /// Method to Add an Instance Annotation to the CLR type
        /// </summary>
        /// <param name="annotationName">Name of Annotation</param>
        /// <param name="value">Value of Annotation</param>
        void AddResourceAnnotation(string annotationName, object value);

        /// <summary>
        /// Method to Add an Instance Annotation to a property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="annotationName">Name of Annotation</param>
        /// <param name="value">Value of Annotation</param>
        void AddPropertyAnnotation(string propertyName, string annotationName, object value);

        /// <summary>
        /// Get an Instance Annotation from CLR Type
        /// </summary>
        /// <param name="annotationName">Name of Annotation</param>
        /// <returns>Get Annotation value for the given annotation</returns>
        object GetResourceAnnotation(string annotationName);

        /// <summary>
        /// Get an Instance Annotation from the Property
        /// </summary>
        /// <param name="propertyName">Name of the Property</param>
        /// <param name="annotationName">Name of the Annotation</param>
        /// <returns>Get Annotation value for the given annotation and property</returns>
        object GetPropertyAnnotation(string propertyName, string annotationName);

        /// <summary>
        /// Get All Annotations from CLR Type
        /// </summary>
        /// <returns>Dictionary of string(annotation name) and object value(annotation value)</returns>
        IDictionary<string,object> GetResourceAnnotations();

        /// <summary>
        /// Get all Annotations for a Property
        /// </summary>
        /// <param name="propertyName">Name of Property</param>
        /// <returns>Dictionary of string(annotation name) and object value(annotation value)</returns>
        IDictionary<string, object> GetPropertyAnnotations(string propertyName);
    }
}
