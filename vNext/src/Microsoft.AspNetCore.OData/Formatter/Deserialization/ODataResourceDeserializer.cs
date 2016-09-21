// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="Microsoft.OData.ODataDeserializer"/> for reading OData resource payloads.
    /// </summary>
    public class ODataResourceDeserializer : ODataEdmTypeDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataResourceDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataResourceDeserializer(IODataDeserializerProvider deserializerProvider)
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

            IEdmStructuredTypeReference structuredType = edmType.AsStructured();

            IEdmNavigationSource navigationSource = null;
            if (structuredType.IsEntity())
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

            ODataReader odataReader = messageReader.CreateODataResourceReader(navigationSource, structuredType.StructuredDefinition());
            ODataResourceWrapper topLevelResource = odataReader.ReadResourceOrResourceSet() as ODataResourceWrapper;
            Contract.Assert(topLevelResource != null);

            return ReadInline(topLevelResource, structuredType, readContext);
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (edmType.IsComplex() && item == null)
            {
                return null;
            }

            if (item == null)
            {
                throw Error.ArgumentNull("item");
            }

            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
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

            if (!String.IsNullOrEmpty(resourceWrapper.Resource.TypeName) && structuredType.FullName() != resourceWrapper.Resource.TypeName)
            {
                // received a derived type in a base type deserializer. delegate it to the appropriate derived type deserializer.
                IEdmModel model = readContext.Model;

                if (model == null)
                {
                    throw Error.Argument("readContext", SRResources.ModelMissingFromReadContext);
                }

                IEdmStructuredType actualType = model.FindType(resourceWrapper.Resource.TypeName) as IEdmStructuredType;
                if (actualType == null)
                {
                    throw new ODataException("TODO:"/*Error.Format(SRResources.ResourceTypeNotInModel, resourceWrapper.Resource.TypeName)*/);
                }

                if (actualType.IsAbstract)
                {
                    //string message = Error.Format(SRResources.CannotInstantiateAbstractResourceType, resourceWrapper.Resource.TypeName);
                    throw new ODataException("TODO:");
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
                        Error.Format(SRResources.TypeCannotBeDeserialized, actualEntityType.FullName(), typeof(ODataInputFormatter).Name));
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
                    return new EdmEntityObject(structuredType.AsEntity());
                }

                return new EdmComplexObject(structuredType.AsComplex());
            }
            else
            {
                Type clrType = EdmLibHelpers.GetClrType(structuredType, model);
                if (clrType == null)
                {
                    throw new ODataException("TODO:"/*
                        Error.Format(SRResources.MappingDoesNotContainResourceType, structuredType.FullName())*/);
                }

                if (readContext.IsDeltaOfT)
                {
                    IEnumerable<string> structuralProperties = structuredType.StructuralProperties()
                        .Select(edmProperty => EdmLibHelpers.GetClrPropertyName(edmProperty, model));

                    if (structuredType.IsOpen())
                    {
                        PropertyInfo dynamicDictionaryPropertyInfo = EdmLibHelpers.GetDynamicPropertyDictionary(
                            structuredType.StructuredDefinition(), model);

                        return Activator.CreateInstance(readContext.ResourceType, clrType, structuralProperties,
                            dynamicDictionaryPropertyInfo);
                    }
                    else
                    {
                        return Activator.CreateInstance(readContext.ResourceType, clrType, structuralProperties);
                    }
                }
                else
                {
                    return Activator.CreateInstance(clrType);
                }
            }
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
                    throw new ODataException("TODO:"/*
                        Error.Format(SRResources.NestedPropertyNotfound, resourceInfoWrapper.NestedResourceInfo.Name,
                            structuredType.FullName())*/);
                }
            }

            foreach (ODataItemBase childItem in resourceInfoWrapper.NestedItems)
            {
                ODataEntityReferenceLinkBase entityReferenceLink = childItem as ODataEntityReferenceLinkBase;
                if (entityReferenceLink != null)
                {
                    // ignore entity reference links.
                    continue;
                }

                ODataResourceSetWrapper resourceSetWrapper = childItem as ODataResourceSetWrapper;
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

            foreach (ODataProperty property in resourceWrapper.Resource.Properties)
            {
                ApplyStructuralProperty(resource, property, structuredType, readContext);
            }
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
        }

        private void ApplyResourceInNestedProperty(IEdmProperty nestedProperty, object resource,
            ODataResourceWrapper resourceWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(nestedProperty != null);
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            if (readContext.IsDeltaOfT)
            {
                IEdmNavigationProperty navigationProperty = nestedProperty as IEdmNavigationProperty;
                if (navigationProperty != null)
                {
                    string message = Error.Format(SRResources.CannotPatchNavigationProperties, navigationProperty.Name,
                        navigationProperty.DeclaringEntityType().FullName());
                    throw new ODataException(message);
                }
            }

            object value = ReadNestedResourceInline(resourceWrapper, nestedProperty.Type, readContext);

            string propertyName = EdmLibHelpers.GetClrPropertyName(nestedProperty, readContext.Model);
            DeserializationHelpers.SetProperty(resource, propertyName, value);
        }

        private void ApplyDynamicResourceInNestedProperty(string propertyName, object resource, IEdmStructuredTypeReference resourceStructuredType,
            ODataResourceWrapper resourceWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            IEdmSchemaType elementType = readContext.Model.FindDeclaredType(resourceWrapper.Resource.TypeName);
            IEdmTypeReference edmTypeReference = elementType.ToEdmTypeReference(true);

            object value = ReadNestedResourceInline(resourceWrapper, edmTypeReference, readContext);

            DeserializationHelpers.SetDynamicProperty(resource, propertyName, value,
                resourceStructuredType.StructuredDefinition(), readContext.Model);
        }

        private object ReadNestedResourceInline(ODataResourceWrapper resourceWrapper, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            Contract.Assert(resourceWrapper != null);
            Contract.Assert(edmType != null);
            Contract.Assert(readContext != null);

            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(edmType);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized,
                    edmType.FullName(), typeof(ODataInputFormatter)));
            }

            IEdmStructuredTypeReference structuredType = edmType.AsStructured();

            var nestedReadContext = new ODataDeserializerContext
            {
                Path = readContext.Path,
                Model = readContext.Model,
            };

            if (readContext.IsUntyped)
            {
                if (structuredType.IsEntity())
                {
                    nestedReadContext.ResourceType = typeof(EdmEntityObject);
                }
                else
                {
                    nestedReadContext.ResourceType = typeof(EdmComplexObject);
                }
            }
            else
            {
                Type clrType = EdmLibHelpers.GetClrType(structuredType, readContext.Model);

                if (clrType == null)
                {
                    throw new ODataException("TODO:"/*
                        Error.Format(SRResources.MappingDoesNotContainResourceType, structuredType.FullName())*/);
                }

                nestedReadContext.ResourceType = clrType;
            }

            return deserializer.ReadInline(resourceWrapper, edmType, nestedReadContext);
        }

        private void ApplyResourceSetInNestedProperty(IEdmProperty nestedProperty, object resource,
            ODataResourceSetWrapper resourceSetWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(nestedProperty != null);
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            if (readContext.IsDeltaOfT)
            {
                IEdmNavigationProperty navigationProperty = nestedProperty as IEdmNavigationProperty;
                if (navigationProperty != null)
                {
                    string message = Error.Format(SRResources.CannotPatchNavigationProperties, navigationProperty.Name,
                        navigationProperty.DeclaringEntityType().FullName());
                    throw new ODataException(message);
                }
            }

            object value = ReadNestedResourceSetInline(resourceSetWrapper, nestedProperty.Type, readContext);

            string propertyName = EdmLibHelpers.GetClrPropertyName(nestedProperty, readContext.Model);
            DeserializationHelpers.SetCollectionProperty(resource, nestedProperty, value, propertyName);
        }

        private void ApplyDynamicResourceSetInNestedProperty(string propertyName, object resource, IEdmStructuredTypeReference structuredType,
            ODataResourceSetWrapper resourceSetWrapper, ODataDeserializerContext readContext)
        {
            Contract.Assert(resource != null);
            Contract.Assert(readContext != null);

            if (String.IsNullOrEmpty(resourceSetWrapper.ResourceSet.TypeName))
            {
                //string message = Error.Format(SRResources.DynamicResourceSetTypeNameIsRequired, propertyName);
                throw new ODataException("TODO: ");
            }

            string elementTypeName =
                DeserializationHelpers.GetCollectionElementTypeName(resourceSetWrapper.ResourceSet.TypeName,
                    isNested: false);
            IEdmSchemaType elementType = readContext.Model.FindDeclaredType(elementTypeName);

            IEdmTypeReference edmTypeReference = elementType.ToEdmTypeReference(true);
            EdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(edmTypeReference));

            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(collectionType);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized,
                    collectionType.FullName(), typeof(ODataInputFormatter)));
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

        private object ReadNestedResourceSetInline(ODataResourceSetWrapper resourceSetWrapper, IEdmTypeReference edmType,
            ODataDeserializerContext readContext)
        {
            Contract.Assert(resourceSetWrapper != null);
            Contract.Assert(edmType != null);
            Contract.Assert(readContext != null);

            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(edmType);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized,
                    edmType.FullName(), typeof(ODataInputFormatter)));
            }

            IEdmStructuredTypeReference structuredType = edmType.AsCollection().ElementType().AsStructured();
            var nestedReadContext = new ODataDeserializerContext
            {
                Path = readContext.Path,
                Model = readContext.Model,
            };

            if (readContext.IsUntyped)
            {
                if (structuredType.IsEntity())
                {
                    nestedReadContext.ResourceType = typeof(EdmEntityObjectCollection);
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
                    throw new ODataException("TODO:"/*
                        Error.Format(SRResources.MappingDoesNotContainResourceType, structuredType.FullName())*/);
                }

                nestedReadContext.ResourceType = typeof(List<>).MakeGenericType(clrType);
            }

            return deserializer.ReadInline(resourceSetWrapper, edmType, nestedReadContext);
        }
    }
}
