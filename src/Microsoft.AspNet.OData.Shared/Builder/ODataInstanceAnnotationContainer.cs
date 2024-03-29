//-----------------------------------------------------------------------------
// <copyright file="ODataInstanceAnnotationContainer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>    
    ///Intended as Default implementation for IODataInstanceAnnotationContainer
    /// </summary>
    public sealed class ODataInstanceAnnotationContainer : IODataInstanceAnnotationContainer
    {
        private IDictionary<string, IDictionary<string, object>> instanceAnnotations;


        /// <summary>
        /// Initializes a new instance of the <see cref="ODataInstanceAnnotationContainer"/> class.
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
            if (string.IsNullOrEmpty(propertyName))
            {
                throw Error.ArgumentNull("propertyName");
            }

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
            if (string.IsNullOrEmpty(propertyName))
            {
                throw Error.ArgumentNull("propertyName");
            }

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
            if (string.IsNullOrEmpty(propertyName))
            {
                throw Error.ArgumentNull("propertyName");
            }

            return GetAllInstanceAnnotations(propertyName);
        }

        private void SetInstanceAnnotation(string propertyName, string annotationName, object value)
        {
            ValidateInstanceAnnotation(annotationName);

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
            if (string.IsNullOrEmpty(annotationName))
            {
                throw Error.ArgumentNull("annotationName");
            }

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

        private static void ValidateInstanceAnnotation(string annotationName)
        {
            if (string.IsNullOrEmpty(annotationName))
            {
                throw Error.ArgumentNull("annotationName");
            }

            if (annotationName[0] == '@' || annotationName[0] == '.' || annotationName[annotationName.Length-1] == '.')
            {
                throw new ArgumentException(SRResources.InstanceAnnotationNotContain, annotationName);
            }

            if (!annotationName.Contains("."))
            {
                throw new ArgumentException(SRResources.InstanceAnnotationShouldContain, annotationName);
            }
        }
    }
}
