//-----------------------------------------------------------------------------
// <copyright file="ODataResourceDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> for reading OData resource payloads.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class ODataResourceDeserializer : ODataEdmTypeDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataResourceDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataResourceDeserializer(ODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.Resource, deserializerProvider)
        {
        }

        /// <inheritdoc />
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEdmStructuredTypeReference structuredType = GetStructuredType(type, readContext);
            IEdmNavigationSource navigationSource = GetNavigationSource(structuredType, readContext);
            ODataReader odataReader = messageReader.CreateODataResourceReader(navigationSource, structuredType.StructuredDefinition());
            ODataResourceWrapper topLevelResource = odataReader.ReadResourceOrResourceSet() as ODataResourceWrapper;
            Contract.Assert(topLevelResource != null);

            return ReadInline(topLevelResource, structuredType, readContext);
        }

        /// <inheritdoc />
        public override async Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEdmStructuredTypeReference structuredType = GetStructuredType(type, readContext);
            IEdmNavigationSource navigationSource = GetNavigationSource(structuredType, readContext);
            ODataReader odataReader = await messageReader.CreateODataResourceReaderAsync(navigationSource, structuredType.StructuredDefinition());
            ODataResourceWrapper topLevelResource = await odataReader.ReadResourceOrResourceSetAsync() as ODataResourceWrapper;
            Contract.Assert(topLevelResource != null);

            return ReadInline(topLevelResource, structuredType, readContext);
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            if (edmType.IsComplex() && item == null)
            {
                return null;
            }

            if (item == null)
            {
                throw Error.ArgumentNull("item");
            }

            if (!edmType.IsStructured())
            {
                throw Error.Argument("edmType", SRResources.ArgumentMustBeOfType, "Entity or Complex");
            }

            ODataResourceWrapper resourceWrapper = item as ODataResourceWrapper;
            if (resourceWrapper == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataResource).Name);
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();
            resourceWrapper = UpdateResourceWrapper(resourceWrapper, readContext);

            return ReadResource(resourceWrapper, edmType.AsStructured(), readContext);
        }

        /// <summary>
        /// Deserializes the given <paramref name="resourceWrapper"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="resourceWrapper">The OData resource to deserialize.</param>
        /// <param name="structuredType">The type of the resource to deserialize.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized resource.</returns>
        public virtual object ReadResource(ODataResourceWrapper resourceWrapper, IEdmStructuredTypeReference structuredType,
            ODataDeserializerContext readContext)
        {
            if (resourceWrapper == null)
            {
                throw Error.ArgumentNull("resourceWrapper");
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }
                        

            if (!String.IsNullOrEmpty(resourceWrapper.ResourceBase.TypeName) && structuredType.FullName() != resourceWrapper.ResourceBase.TypeName)
            {
                // received a derived type in a base type deserializer. delegate it to the appropriate derived type deserializer.
                IEdmModel model = readContext.Model;

                if (model == null)
                {
                    throw Error.Argument("readContext", SRResources.ModelMissingFromReadContext);
                }

                IEdmStructuredType actualType = model.FindType(resourceWrapper.ResourceBase.TypeName) as IEdmStructuredType;
                if (actualType == null)
                {
                    throw new ODataException(Error.Format(SRResources.ResourceTypeNotInModel, resourceWrapper.ResourceBase.TypeName));
                }

                if (actualType.IsAbstract)
                {
                    string message = Error.Format(SRResources.CannotInstantiateAbstractResourceType, resourceWrapper.ResourceBase.TypeName);
                    throw new ODataException(message);
                }

                IEdmTypeReference actualStructuredType;
                IEdmEntityType actualEntityType = actualType as IEdmEntityType;

                if (actualEntityType != null)
                {
                    actualStructuredType = new EdmEntityTypeReference(actualEntityType, isNullable: false);
                }
                else
                {
                    actualStructuredType = new EdmComplexTypeReference(actualType as IEdmComplexType, isNullable: false);
                }

                ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(actualStructuredType);
                if (deserializer == null)
                {
                    throw new SerializationException(
                        Error.Format(SRResources.TypeCannotBeDeserialized, actualEntityType.FullName()));
                }

                object resource = deserializer.ReadInline(resourceWrapper, actualStructuredType, readContext);

                EdmStructuredObject structuredObject = resource as EdmStructuredObject;
                if (structuredObject != null)
                {
                    structuredObject.ExpectedEdmType = structuredType.StructuredDefinition();
                }

                return resource;
            }
            else
            {
                object resource = CreateResourceInstance(structuredType, readContext);
                ApplyResourceProperties(resource, resourceWrapper, structuredType, readContext);

                ODataDeletedResource deletedResource = resourceWrapper.ResourceBase as ODataDeletedResource;

                if (deletedResource != null)
                {
                    AppendDeletedProperties(resource, deletedResource, readContext.IsUntyped);
                }

                return resource;
            }
        }


        /// <summary>
        /// Creates a new instance of the backing CLR object for the given resource type.
        /// </summary>
        /// <param name="structuredType">The EDM type of the resource to create.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The created CLR object.</returns>
        public virtual object CreateResourceInstance(IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            if (structuredType == null)
            {
                throw Error.ArgumentNull("structuredType");
            }

            IEdmModel model = readContext.Model;
            if (model == null)
            {
                throw Error.Argument("readContext", SRResources.ModelMissingFromReadContext);
            }

            if (readContext.IsUntyped)
            {
                if (structuredType.IsEntity())
                {
                    if (readContext.IsDeltaDeletedEntity)
                    {
                        return new EdmDeltaDeletedEntityObject(structuredType.AsEntity());
                    }

                    return new EdmEntityObject(structuredType.AsEntity());
                }

                return new EdmComplexObject(structuredType.AsComplex());
            }
            else
            {
                Type clrType = EdmLibHelpers.GetClrType(structuredType, model);
                if (clrType == null)
                {
                    throw new ODataException(
                        Error.Format(SRResources.MappingDoesNotContainResourceType, structuredType.FullName()));
                }

                if (readContext.IsDeltaOfT)
                {
                    IEnumerable<string> structuralProperties = structuredType.StructuredDefinition().Properties()
                        .Select(edmProperty => EdmLibHelpers.GetClrPropertyName(edmProperty, model));
                    
                    PropertyInfo instanceAnnotationProperty = EdmLibHelpers.GetInstanceAnnotationsContainer(
                           structuredType.StructuredDefinition(), model);

                    if (structuredType.IsOpen())
                    {
                        PropertyInfo dynamicDictionaryPropertyInfo = EdmLibHelpers.GetDynamicPropertyDictionary(
                            structuredType.StructuredDefinition(), model);

                        return Activator.CreateInstance(readContext.ResourceType, clrType, structuralProperties,
                            dynamicDictionaryPropertyInfo, instanceAnnotationProperty);
                    }
                    else
                    {
                        return Activator.CreateInstance(readContext.ResourceType, clrType, structuralProperties, null, instanceAnnotationProperty);
                    }
                }
                else
                {
                    return Activator.CreateInstance(clrType);
                }
            }
        }

        private static void AppendDeletedProperties(dynamic resource, ODataDeletedResource deletedResource, bool isUntyped)
        {
            if (isUntyped)
            {
                resource.Id = deletedResource.Id.ToString();
            }
            else
            {
                resource.Id = deletedResource.Id;
            }

            resource.Reason = deletedResource.Reason.Value;            
        }

        /// <summary>
        /// Deserializes the nested properties from <paramref name="resourceWrapper"/> into <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The object into which the nested properties should be read.</param>
        /// <param name="resourceWrapper">The resource object containing the nested properties.</param>
        /// <param name="structuredType">The type of the resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyNestedProperties(object resource, ODataResourceWrapper resourceWrapper,
            IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (resourceWrapper == null)
            {
                throw Error.ArgumentNull("resourceWrapper");
            }

            foreach (ODataNestedResourceInfoWrapper nestedResourceInfo in resourceWrapper.NestedResourceInfos)
            {
                ApplyNestedProperty(resource, nestedResourceInfo, structuredType, readContext);
            }
        }

        /// <summary>
        /// Deserializes the nested property from <paramref name="resourceInfoWrapper"/> into <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The object into which the nested property should be read.</param>
        /// <param name="resourceInfoWrapper">The nested resource info.</param>
        /// <param name="structuredType">The type of the resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyNestedProperty(object resource, ODataNestedResourceInfoWrapper resourceInfoWrapper,
             IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (resource == null)
            {
                throw Error.ArgumentNull("resource");
            }

            if (resourceInfoWrapper == null)
            {
                throw Error.ArgumentNull("resourceInfoWrapper");
            }

            IEdmProperty edmProperty = structuredType.FindProperty(resourceInfoWrapper.NestedResourceInfo.Name);
            if (edmProperty == null)
            {
                if (!structuredType.IsOpen())
                {
                    throw new ODataException(
                        Error.Format(SRResources.NestedPropertyNotfound, resourceInfoWrapper.NestedResourceInfo.Name,
                            structuredType.FullName()));
                }
            }

            IList<ODataItemBase> nestedItems;
            ODataEntityReferenceLinkBase[] referenceLinks = resourceInfoWrapper.NestedItems.OfType<ODataEntityReferenceLinkBase>().ToArray();
            if (referenceLinks.Length > 0)
            {
                // Be noted:
                // 1) OData v4.0, it's "Orders@odata.bind", and we get "ODataEntityReferenceLinkWrapper"(s) for that.
                // 2) OData v4.01, it's {"odata.id" ...}, and we get "ODataResource"(s) for that.
                // So, in OData v4, if it's a single, NestedItems contains one ODataEntityReferenceLinkWrapper,
                // if it's a collection, NestedItems contains multiple ODataEntityReferenceLinkWrapper(s)
                // We can use the following code to adjust the `ODataEntityReferenceLinkWrapper` to `ODataResourceWrapper`.
                // In OData v4.01, we will not be here.
                // Only supports declared property
                Contract.Assert(edmProperty != null);

                nestedItems = new List<ODataItemBase>();
                if (edmProperty.Type.IsCollection())
                {
                    IEdmCollectionTypeReference edmCollectionTypeReference = edmProperty.Type.AsCollection();
                    ODataResourceSetWrapper resourceSetWrapper = CreateResourceSetWrapper(edmCollectionTypeReference, referenceLinks, readContext);
                    nestedItems.Add(resourceSetWrapper);
                }
                else
                {
                    ODataResourceWrapper resourceWrapper = CreateResourceWrapper(edmProperty.Type, referenceLinks[0], readContext);
                    nestedItems.Add(resourceWrapper);
                }
            }
            else
            {
                nestedItems = resourceInfoWrapper.NestedItems;
            }

            foreach (ODataItemBase childItem in nestedItems)
            {
                // it maybe null.
                if (childItem == null)
                {
                    if (edmProperty == null)
                    {
                        // for the dynamic, OData.net has a bug. see https://github.com/OData/odata.net/issues/977
                        ApplyDynamicResourceInNestedProperty(resourceInfoWrapper.NestedResourceInfo.Name, resource,
                            structuredType, null, readContext);
                    }
                    else
                    {
                        ApplyResourceInNestedProperty(edmProperty, resource, null, readContext);
                    }
                }
                
                ODataResourceSetWrapperBase resourceSetWrapper = childItem as ODataResourceSetWrapperBase;
                if (resourceSetWrapper != null)
                {
                    if (edmProperty == null)
                    {
                        ApplyDynamicResourceSetInNestedProperty(resourceInfoWrapper.NestedResourceInfo.Name,
                            resource, structuredType, resourceSetWrapper, readContext);
                    }
                    else
                    {
                        ApplyResourceSetInNestedProperty(edmProperty, resource, resourceSetWrapper, readContext);
                    }

                    continue;
                }

                // It must be resource by now.
                ODataResourceWrapper resourceWrapper = (ODataResourceWrapper)childItem;
                if (resourceWrapper != null)
                {
                    if (edmProperty == null)
                    {
                        ApplyDynamicResourceInNestedProperty(resourceInfoWrapper.NestedResourceInfo.Name, resource,
                            structuredType, resourceWrapper, readContext);
                    }
                    else
                    {
                        ApplyResourceInNestedProperty(edmProperty, resource, resourceWrapper, readContext);
                    }
                }
            }
        }


        private ODataResourceSetWrapper CreateResourceSetWrapper(IEdmCollectionTypeReference edmPropertyType, 
            IList<ODataEntityReferenceLinkBase> refLinks, ODataDeserializerContext readContext)
        {
            ODataResourceSet resourceSet = new ODataResourceSet
            {
                TypeName = edmPropertyType.FullName(),
            };

            IEdmTypeReference elementType = edmPropertyType.ElementType();
            ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(resourceSet);
            foreach (ODataEntityReferenceLinkBase refLinkWrapper in refLinks)
            {
                ODataResourceWrapper resourceWrapper = CreateResourceWrapper(elementType, refLinkWrapper, readContext);
                resourceSetWrapper.Resources.Add(resourceWrapper);
            }

            return resourceSetWrapper;
        }

        private ODataResourceWrapper CreateResourceWrapper(IEdmTypeReference edmPropertyType, ODataEntityReferenceLinkBase refLink, ODataDeserializerContext readContext)
        { 
            Contract.Assert(readContext != null);

            ODataResource resource = new ODataResource
            {
                TypeName = edmPropertyType.FullName(),
            };

            resource.Properties = CreateKeyProperties(refLink.EntityReferenceLink.Url, readContext);
         
            if (refLink.EntityReferenceLink.InstanceAnnotations != null)
            {
                foreach (ODataInstanceAnnotation instanceAnnotation in refLink.EntityReferenceLink.InstanceAnnotations)
                {
                    resource.InstanceAnnotations.Add(instanceAnnotation);
                };
            }

            return new ODataResourceWrapper(resource);
        }

        /// <summary>
        /// Update the resource wrapper if it has the "Id" value.
        /// </summary>
        /// <param name="resourceWrapper">The resource wrapper.</param>
        /// <param name="readContext">The read context.</param>
        /// <returns>The resource wrapper.</returns>
        private ODataResourceWrapper UpdateResourceWrapper(ODataResourceWrapper resourceWrapper, ODataDeserializerContext readContext)
        { 
            Contract.Assert(readContext != null);

            if (resourceWrapper?.ResourceBase?.Id == null)
            {
                return resourceWrapper;
            }

            IEnumerable<ODataProperty> keys = CreateKeyProperties(resourceWrapper.ResourceBase.Id, readContext);
            if (keys == null)
            {
                return resourceWrapper;
            }

            if (resourceWrapper.ResourceBase.Properties == null)
            {
                resourceWrapper.ResourceBase.Properties = keys;
            }
            else
            {
                IDictionary<string, ODataProperty> newPropertiesDic = resourceWrapper.ResourceBase.Properties.ToDictionary(p => p.Name, p => p);
                foreach (ODataProperty key in keys)
                {
                    if (!newPropertiesDic.ContainsKey(key.Name))
                    {
                        newPropertiesDic[key.Name] = key;
                    }
                }

                resourceWrapper.ResourceBase.Properties = newPropertiesDic.Values;
            }

            return resourceWrapper;
        }

        /// <summary>
        /// Do uri parsing to get the key values.
        /// </summary>
        /// <param name="id">The key Id.</param>
        /// <param name="readContext">The reader context.</param>
        /// <returns>The key properties.</returns>
        private static IList<ODataProperty> CreateKeyProperties(Uri id, ODataDeserializerContext readContext)
        {
            Contract.Assert(id != null);
            Contract.Assert(readContext != null);
            IList<ODataProperty> properties = new List<ODataProperty>();
            if (readContext.Request == null)
            {
                return properties;
            }

            IODataPathHandler pathHandler = readContext.InternalRequest.PathHandler;
            IWebApiRequestMessage internalRequest = readContext.InternalRequest;
            IWebApiUrlHelper urlHelper = readContext.InternalUrlHelper;
            try
            {
                string serviceRoot = urlHelper.CreateODataLink(
                    internalRequest.Context.RouteName,
                    internalRequest.PathHandler,
                    new List<ODataPathSegment>());
                ODataPath odataPath = pathHandler.Parse(serviceRoot, id.OriginalString, internalRequest.RequestContainer);
                KeySegment keySegment = odataPath.Segments.OfType<KeySegment>().LastOrDefault();
                if (keySegment != null)
                {
                    foreach (KeyValuePair<string, object> key in keySegment.Keys)
                    {
                        properties.Add(new ODataProperty
                        {
                            Name = key.Key,
                            Value = key.Value
                        });
                    }
                }

                return properties;
            }
            catch (Exception)
            {
                return properties;
            }          
        }

        /// <summary>
        /// Deserializes the structural properties from <paramref name="resourceWrapper"/> into <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The object into which the structural properties should be read.</param>
        /// <param name="resourceWrapper">The resource object containing the structural properties.</param>
        /// <param name="structuredType">The type of the resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyStructuralProperties(object resource, ODataResourceWrapper resourceWrapper,
            IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (resourceWrapper == null)
            {
                throw Error.ArgumentNull("resourceWrapper");
            }

            foreach (ODataProperty property in resourceWrapper.ResourceBase.Properties)
            {
                ApplyStructuralProperty(resource, property, structuredType, readContext);
            }
        }

        /// <summary>
        /// Deserializes the instance annotations from <paramref name="resourceWrapper"/> into <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The object into which the annotations should be read.</param>
        /// <param name="resourceWrapper">The resource object containing the annotations.</param>
        /// <param name="structuredType">The type of the resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyInstanceAnnotations(object resource, ODataResourceWrapper resourceWrapper,
            IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (resourceWrapper == null)
            {
                throw Error.ArgumentNull("resourceWrapper");
            }

            DeserializationHelpers.ApplyInstanceAnnotations(resource, structuredType, resourceWrapper.ResourceBase,DeserializerProvider, readContext);
        }

        /// <summary>
        /// Deserializes the given <paramref name="structuralProperty"/> into <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The object into which the structural property should be read.</param>
        /// <param name="structuralProperty">The structural property.</param>
        /// <param name="structuredType">The type of the resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyStructuralProperty(object resource, ODataProperty structuralProperty,
            IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        { 
            if (resource == null)
            {
                throw Error.ArgumentNull("resource");
            }

            if (structuralProperty == null)
            {
                throw Error.ArgumentNull("structuralProperty");
            }

            DeserializationHelpers.ApplyProperty(structuralProperty, structuredType, resource, DeserializerProvider, readContext);
        }

        private void ApplyResourceProperties(object resource, ODataResourceWrapper resourceWrapper,
            IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        
        {
            ApplyStructuralProperties(resource, resourceWrapper, structuredType, readContext);
            ApplyNestedProperties(resource, resourceWrapper, structuredType, readContext);
            ApplyInstanceAnnotations(resource, resourceWrapper, structuredType, readContext);
        }

        private void ApplyResourceInNestedProperty(IEdmProperty nestedProperty, object resource,
            ODataResourceWrapper resourceWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(nestedProperty != null);
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            object value = ReadNestedResourceInline(resourceWrapper, nestedProperty.Type, readContext);
            
            // First resolve Data member alias or annotation, then set the regular
            // or delta resource accordingly.
            string propertyName = EdmLibHelpers.GetClrPropertyName(nestedProperty, readContext.Model);

            DeserializationHelpers.SetProperty(resource, propertyName, value);
        }

        private void ApplyDynamicResourceInNestedProperty(string propertyName, object resource, IEdmStructuredTypeReference resourceStructuredType,
            ODataResourceWrapper resourceWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            object value = null;
            if (resourceWrapper != null)
            {
                IEdmSchemaType elementType = readContext.Model.FindDeclaredType(resourceWrapper.ResourceBase.TypeName);
                IEdmTypeReference edmTypeReference = elementType.ToEdmTypeReference(true);

                value = ReadNestedResourceInline(resourceWrapper, edmTypeReference, readContext);
            }

            DeserializationHelpers.SetDynamicProperty(resource, propertyName, value,
                resourceStructuredType.StructuredDefinition(), readContext.Model);
        }

        private object ReadNestedResourceInline(ODataResourceWrapper resourceWrapper, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            Contract.Assert(edmType != null);
            Contract.Assert(readContext != null);

            if (resourceWrapper == null)
            {
                return null;
            }

            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(edmType);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized, edmType.FullName()));
            }

            IEdmStructuredTypeReference structuredType = edmType.AsStructured();

            var nestedReadContext = new ODataDeserializerContext
            {
                Path = readContext.Path,
                Model = readContext.Model,
                Request = readContext.Request
            };

            Type clrType = null;
            if (readContext.IsUntyped)
            {
                clrType = structuredType.IsEntity()
                    ? (readContext.IsDeltaEntity ? (readContext.IsDeltaDeletedEntity? typeof(EdmDeltaDeletedEntityObject) : typeof(EdmDeltaEntityObject) ) : typeof(EdmEntityObject))
                    : typeof(EdmComplexObject);
            }
            else
            {
                clrType = EdmLibHelpers.GetClrType(structuredType, readContext.Model);

                if (clrType == null)
                {
                    throw new ODataException(
                        Error.Format(SRResources.MappingDoesNotContainResourceType, structuredType.FullName()));
                }
            }

            nestedReadContext.ResourceType = readContext.IsDeltaOfT
                ? typeof(Delta<>).MakeGenericType(clrType)
                : clrType;
            return deserializer.ReadInline(resourceWrapper, edmType, nestedReadContext);
        }
        private void ApplyResourceSetInNestedProperty(IEdmProperty nestedProperty, object resource,
            ODataResourceSetWrapperBase resourceSetWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(nestedProperty != null);
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            object value = ReadNestedResourceSetInline(resourceSetWrapper, nestedProperty.Type, readContext);

            string propertyName = EdmLibHelpers.GetClrPropertyName(nestedProperty, readContext.Model);
            DeserializationHelpers.SetCollectionProperty(resource, nestedProperty, value, propertyName, resourceSetWrapper.ResourceSetType == ResourceSetType.DeltaResourceSet);
        }

        private void ApplyDynamicResourceSetInNestedProperty(string propertyName, object resource, IEdmStructuredTypeReference structuredType,
            ODataResourceSetWrapperBase resourceSetWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            if (String.IsNullOrEmpty(resourceSetWrapper.ResourceSetBase.TypeName))
            {
                string message = Error.Format(SRResources.DynamicResourceSetTypeNameIsRequired, propertyName);
                throw new ODataException(message);
            }

            string elementTypeName =
                DeserializationHelpers.GetCollectionElementTypeName(resourceSetWrapper.ResourceSetBase.TypeName,
                    isNested: false);
            IEdmSchemaType elementType = readContext.Model.FindDeclaredType(elementTypeName);

            IEdmTypeReference edmTypeReference = elementType.ToEdmTypeReference(true);
            EdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(edmTypeReference));

            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(collectionType);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized, collectionType.FullName()));
            }

            IEnumerable value = ReadNestedResourceSetInline(resourceSetWrapper, collectionType, readContext) as IEnumerable;
            object result = value;
            if (value != null)
            {
                if (readContext.IsUntyped)
                {
                    result = value.ConvertToEdmObject(collectionType);
                }
            }

            DeserializationHelpers.SetDynamicProperty(resource, structuredType, EdmTypeKind.Collection, propertyName,
                result, collectionType, readContext.Model);
        }

        private object ReadNestedResourceSetInline(ODataResourceSetWrapperBase resourceSetWrapper, IEdmTypeReference edmType,
            ODataDeserializerContext readContext)
        {
            Contract.Assert(resourceSetWrapper != null);
            Contract.Assert(edmType != null);
            Contract.Assert(readContext != null);

            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(edmType);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized, edmType.FullName()));
            }

            IEdmStructuredTypeReference structuredType = edmType.AsCollection().ElementType().AsStructured();
            var nestedReadContext = new ODataDeserializerContext
            {
                Path = readContext.Path,
                Model = readContext.Model,
                Request = readContext.Request
            };

            if (readContext.IsUntyped)
            {
                if (structuredType.IsEntity())
                {
                    nestedReadContext.ResourceType = (readContext.IsDeltaOfT && resourceSetWrapper.ResourceSetType == ResourceSetType.DeltaResourceSet)? 
                        typeof(EdmChangedObjectCollection): typeof(EdmEntityObjectCollection);
                }
                else
                {
                    nestedReadContext.ResourceType = typeof(EdmComplexObjectCollection);
                }
            }
            else
            {
                Type clrType = EdmLibHelpers.GetClrType(structuredType, readContext.Model);

                if (clrType == null)
                {
                    throw new ODataException(
                        Error.Format(SRResources.MappingDoesNotContainResourceType, structuredType.FullName()));
                }

                nestedReadContext.ResourceType = (readContext.IsDeltaOfT && resourceSetWrapper.ResourceSetType == ResourceSetType.DeltaResourceSet)
                ? typeof(DeltaSet<>).MakeGenericType(clrType) : typeof(List<>).MakeGenericType(clrType);
            }

            return deserializer.ReadInline(resourceSetWrapper, edmType, nestedReadContext);
        }

        private static IEdmStructuredTypeReference GetStructuredType(Type type, ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            if (!edmType.IsStructured())
            {
                throw Error.Argument("type", SRResources.ArgumentMustBeOfType, "Structured");
            }

            return edmType.AsStructured();
        }

        private static IEdmNavigationSource GetNavigationSource(IEdmStructuredTypeReference edmType, ODataDeserializerContext readContext)
        {
            IEdmNavigationSource navigationSource = null;
            if (edmType.IsEntity())
            {
                if (readContext.Path == null)
                {
                    throw Error.Argument("readContext", SRResources.ODataPathMissing);
                }

                navigationSource = readContext.Path.NavigationSource;
                if (navigationSource == null)
                {
                    throw new SerializationException(SRResources.NavigationSourceMissingDuringDeserialization);
                }
            }

            return navigationSource;
        }
    }
}
