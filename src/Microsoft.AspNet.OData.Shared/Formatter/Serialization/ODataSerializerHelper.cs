//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
        /// <summary>
        /// Appends instance annotations to an ODataResource.
        /// </summary>
        /// <param name="resource">The ODataResource being annotated</param>
        /// <param name="resourceContext">The context for the resource instance to be annotated.</param>
        /// <param name="instanceAnnotationContainer">The annotations to write.</param>
        /// <param name="serializerProvider">The SerializerProvider to use to write annotations</param>
        internal static void AppendInstanceAnnotations(ODataResourceBase resource, ResourceContext resourceContext, IODataInstanceAnnotationContainer instanceAnnotationContainer, ODataSerializerProvider serializerProvider)
        {
            if (instanceAnnotationContainer == null)
            {
                return;
            }

            IDictionary<string, object> clrAnnotations = instanceAnnotationContainer.GetResourceAnnotations();

            if (clrAnnotations != null)
            {
                foreach (KeyValuePair<string, object> annotation in clrAnnotations)
                {
                    AddODataAnnotations(resource.InstanceAnnotations, resourceContext, annotation, serializerProvider);
                }
            }

            if (resource.Properties != null)
            {
                foreach (ODataProperty property in resource.Properties)
                {
                    string propertyName = property.Name;

                    IDictionary<string, object> propertyAnnotations = instanceAnnotationContainer.GetPropertyAnnotations(propertyName);

                    if (propertyAnnotations != null)
                    {
                        foreach (KeyValuePair<string, object> annotation in propertyAnnotations)
                        {
                            AddODataAnnotations(property.InstanceAnnotations, resourceContext, annotation, serializerProvider);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds instance annotations to the ODataInstanceAnnotation container. 
        /// </summary>
        /// <param name="instanceAnnotations">A collection of instance annotations.</param>
        /// <param name="resourceContext">The context for the resource instance to be annotated.</param>
        /// <param name="annotation">The annotation to be added to the instance annotations container.</param>
        /// <param name="serializerProvider">The SerializerProvider to use to write annotations.</param>
        private static void AddODataAnnotations(ICollection<ODataInstanceAnnotation> instanceAnnotations, ResourceContext resourceContext, KeyValuePair<string, object> annotation, ODataSerializerProvider serializerProvider)
        {
            ODataValue annotationValue = null;

            if (annotation.Value != null)
            {
                IEdmTypeReference edmTypeReference = resourceContext.SerializerContext.GetEdmType(annotation.Value,
                    annotation.Value.GetType());

                ODataEdmTypeSerializer edmTypeSerializer = GetEdmTypeSerializer(edmTypeReference, serializerProvider);

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
                instanceAnnotations.Add(new ODataInstanceAnnotation(annotation.Key, annotationValue));
            }
        }

        private static ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmTypeReference, ODataSerializerProvider serializerProvider)
        {
            ODataEdmTypeSerializer edmTypeSerializer;

            if (edmTypeReference.IsCollection())
            {
                edmTypeSerializer = new ODataCollectionSerializer(serializerProvider, true);
            }
            else if (edmTypeReference.IsStructured())
            {
                edmTypeSerializer = new ODataResourceValueSerializer(serializerProvider);
            }
            else
            {
                edmTypeSerializer = serializerProvider.GetEdmTypeSerializer(edmTypeReference);
            }

            return edmTypeSerializer;
        }
    }
}
