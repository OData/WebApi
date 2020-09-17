// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    ///Initializes a new instance of the <see cref="ODataInstanceAnnotationContainer"/> class.
    ///Inteded as Default implementation for IODataInstanceAnnotationContainer
    /// </summary>
    public class ODataInstanceAnnotationContainer : IODataInstanceAnnotationContainer
    {
        private IDictionary<string, IDictionary<string, object>> instanceAnnotations;


        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        public ODataInstanceAnnotationContainer()
        {
            instanceAnnotations = new Dictionary<string, IDictionary<string, object>>();
        }

        /// <summary>
        /// Method to Add an Instance Annotation to the CLR type
        /// </summary>
        /// <param name="annotationName">Name of Annotation</param>
        /// <param name="value">Value of Annotation</param>
        public void AddResourceAnnotation(string annotationName, object value)
        {
            SetInstanceAnnotation(string.Empty, annotationName, value);
        }

        /// <summary>
        /// Method to Add an Instance Annotation to a property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="annotationName">Name of Annotation</param>
        /// <param name="value">Value of Annotation</param>
        public void AddPropertyAnnotation(string propertyName, string annotationName, object value)
        {
            SetInstanceAnnotation(propertyName, annotationName, value);
        }

        /// <summary>
        /// Get an Instance Annotation from CLR Type
        /// </summary>
        /// <param name="annotationName">Name of Annotation</param>
        /// <returns>Get Annotation value for the given annotation</returns>
        public object GetResourceAnnotation(string annotationName)
        {
            return GetInstanceAnnotation(string.Empty, annotationName);
        }

        /// <summary>
        /// Get an Instance Annotation from the Property
        /// </summary>
        /// <param name="propertyName">Name of the Property</param>
        /// <param name="annotationName">Name of the Annotation</param>
        /// <returns>Get Annotation value for the given annotation and property</returns>
        public object GetPropertyAnnotation(string propertyName, string annotationName)
        {
            return GetInstanceAnnotation(propertyName, annotationName);
        }

        /// <summary>
        /// Get All Annotations from CLR Type
        /// </summary>
        /// <returns>Dictionary of string(annotation name) and object value(annotation value)</returns>
        public IDictionary<string, object> GetResourceAnnotations()
        {
            return GetAllInstanceAnnotations(string.Empty);
        }

        /// <summary>
        /// Get all Annotation for a Property
        /// </summary>
        /// <param name="propertyName">Name of Property</param>
        /// <returns>Dictionary of string(annotation name) and object value(annotation value)</returns>
        public IDictionary<string, object> GetPropertyAnnotations(string propertyName)
        {
            return GetAllInstanceAnnotations(propertyName);
        }

        private void SetInstanceAnnotation(string propertyName, string annotationName, object value)
        {
            IDictionary<string, object> annotationDictionary;
            if (!instanceAnnotations.TryGetValue(propertyName, out annotationDictionary))
            {
                annotationDictionary = new Dictionary<string, object>();
                instanceAnnotations.Add(propertyName, annotationDictionary);
            }

            annotationDictionary[annotationName] = value;
        }

        private object GetInstanceAnnotation(string propertyName, string annotationName)
        {
            IDictionary<string, object> annotationDictionary;
            if (instanceAnnotations.TryGetValue(propertyName, out annotationDictionary))
            {
                object annotationValue;
                if (annotationDictionary.TryGetValue(annotationName, out annotationValue))
                {
                    return annotationValue;
                }
            }

            return null;
        }

        private IDictionary<string, object> GetAllInstanceAnnotations(string propertyName)
        {
            IDictionary<string, object> annotationDictionary;
            if (instanceAnnotations.TryGetValue(propertyName, out annotationDictionary))
            {
                return annotationDictionary;
            }

            return null;
        }
    }
}
