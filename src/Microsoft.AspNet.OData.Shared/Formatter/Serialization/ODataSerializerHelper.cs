//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Helper class for OData Serialization
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
    internal class ODataSerializerHelper
    {
        internal static void AppendInstanceAnnotations(ODataResourceBase resource, ResourceContext resourceContext, object value, ODataSerializerProvider SerializerProvider)
        {
            IODataInstanceAnnotationContainer instanceAnnotationContainer = value as IODataInstanceAnnotationContainer;

            if (instanceAnnotationContainer != null)
            {
                IDictionary<string, object> clrAnnotations = instanceAnnotationContainer.GetResourceAnnotations();

                if (clrAnnotations != null)
                {
                    foreach (KeyValuePair<string, object> annotation in clrAnnotations)
                    {
                        AddODataAnnotations(resource.InstanceAnnotations, resourceContext, annotation, SerializerProvider);
                    }
                }

                if (resource.Properties != null)
                {
                    foreach (ODataProperty property in resource.Properties)
                    {
                        string propertyName = property.Name;

                        if (property.InstanceAnnotations == null)
                        {
                            property.InstanceAnnotations = new List<ODataInstanceAnnotation>();
                        }

                        IDictionary<string, object> propertyAnnotations = instanceAnnotationContainer.GetPropertyAnnotations(propertyName);

                        if (propertyAnnotations != null)
                        {
                            foreach (KeyValuePair<string, object> annotation in propertyAnnotations)
                            {
                                AddODataAnnotations(property.InstanceAnnotations, resourceContext, annotation, SerializerProvider);
                            }
                        }
                    }
                }
            }
        }


        internal static void AddODataAnnotations(ICollection<ODataInstanceAnnotation> InstanceAnnotations, ResourceContext resourceContext, KeyValuePair<string, object> annotation, ODataSerializerProvider SerializerProvider)
        {
            ODataValue annotationValue = null;

            if (annotation.Value != null)
            {
                IEdmTypeReference edmTypeReference = resourceContext.SerializerContext.GetEdmType(annotation.Value,
                                            annotation.Value.GetType());

                ODataEdmTypeSerializer edmTypeSerializer = GetEdmTypeSerializer(edmTypeReference, SerializerProvider);

                if (edmTypeSerializer != null)
                {
                    annotationValue = edmTypeSerializer.CreateODataValue(annotation.Value, edmTypeReference, resourceContext.SerializerContext);
                }                
            }
            else
            {
                annotationValue = new ODataNullValue();
            }

            if (annotationValue != null)
            {
                InstanceAnnotations.Add(new ODataInstanceAnnotation(annotation.Key, annotationValue));
            }
        }


        private static ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmTypeReference, ODataSerializerProvider SerializerProvider)
        {
            ODataEdmTypeSerializer edmTypeSerializer;

            if (edmTypeReference.IsCollection())
            {
                edmTypeSerializer = new ODataCollectionSerializer(SerializerProvider, true);
            }
            else if (edmTypeReference.IsStructured())
            {
                edmTypeSerializer = new ODataResourceValueSerializer(SerializerProvider);
            }
            else
            {
                edmTypeSerializer = SerializerProvider.GetEdmTypeSerializer(edmTypeReference);
            }

            return edmTypeSerializer;
        }

    }
}
