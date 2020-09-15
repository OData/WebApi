using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Interface to used as a Container for holding Instance Annotations, An default implementation is provided
    /// Custoer can implement the interface and can have their own implementation.
    /// </summary>
    public interface IODataInstanceAnnotationContainer
    {
        /// <summary>
        /// Method to Add an Instance Annotation to the CLR type
        /// </summary>
        /// <param name="annotationName">Name of Annotation</param>
        /// <param name="value">Value of Annotation</param>
        void AddTypeAnnotation(string annotationName, object value);

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
        object GetTypeAnnotation(string annotationName);

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
        IDictionary<string,object> GetAllTypeAnnotations();

        /// <summary>
        /// Get all Annotations for a Property
        /// </summary>
        /// <param name="propertyName">Name of Property</param>
        /// <returns>Dictionary of string(annotation name) and object value(annotation value)</returns>
        IDictionary<string, object> GetAllPropertyAnnotations(string propertyName);
    }
}
