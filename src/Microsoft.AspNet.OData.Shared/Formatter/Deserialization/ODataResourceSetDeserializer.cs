// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData resource sets.
    /// </summary>
    public class ODataResourceSetDeserializer : ODataEdmTypeDeserializer
    {
        private static readonly MethodInfo CastMethodInfo = typeof(Enumerable).GetMethod("Cast");

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataResourceSetDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataResourceSetDeserializer(ODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.ResourceSet, deserializerProvider)
        {
        }

        /// <inheritdoc />
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            // TODO: is it ok to read the top level collection of entity?
            if (!(edmType.IsCollection() && edmType.AsCollection().ElementType().IsStructured()))
            {
                throw Error.Argument("edmType", SRResources.ArgumentMustBeOfType, EdmTypeKind.Complex + " or " + EdmTypeKind.Entity);
            }

            ODataReader resourceSetReader = messageReader.CreateODataResourceSetReader();
            object resourceSet = resourceSetReader.ReadResourceOrResourceSet();
            return ReadInline(resourceSet, edmType, readContext);
        }
        
        /// <inheritdoc />
        public override async Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            // TODO: is it ok to read the top level collection of entity?
            if (!(edmType.IsCollection() && edmType.AsCollection().ElementType().IsStructured()))
            {
                throw Error.Argument("edmType", SRResources.ArgumentMustBeOfType, EdmTypeKind.Complex + " or " + EdmTypeKind.Entity);
            }

            ODataReader resourceSetReader = await messageReader.CreateODataResourceSetReaderAsync();
            object resourceSet = await resourceSetReader.ReadResourceOrResourceSetAsync();
            return ReadInline(resourceSet, edmType, readContext);
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                return null;
            }

            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            if (!edmType.IsCollection() || !edmType.AsCollection().ElementType().IsStructured())
            {
                throw Error.Argument("edmType", SRResources.TypeMustBeResourceSet, edmType.ToTraceString());
            }

            ODataResourceSetWrapperBase resourceSet = item as ODataResourceSetWrapperBase;
            if (resourceSet == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataResourceSetWrapperBase).Name);
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            IEdmStructuredTypeReference elementType = edmType.AsCollection().ElementType().AsStructured();

            IEdmEntityType actualType = edmType.AsCollection().ElementType().Definition as IEdmEntityType;

            IEnumerable result = ReadResourceSet(resourceSet, elementType, readContext);

            //Handle Delta requests to create EdmChangedObjectCollection
            if (resourceSet.ResourceSetType == ResourceSetType.DeltaResourceSet)
            {                
                EdmChangedObjectCollection changedObjCollection = new EdmChangedObjectCollection(actualType);

                foreach (IEdmChangedObject changedObject in result)
                {                
                    changedObjCollection.Add(changedObject);                    
                }

                return changedObjCollection;
            }

            if (result != null && elementType.IsComplex())
            {                
                if (readContext.IsUntyped)
                {
                    EdmComplexObjectCollection complexCollection = new EdmComplexObjectCollection(edmType.AsCollection());
                    foreach (EdmComplexObject complexObject in result)
                    {
                        complexCollection.Add(complexObject);
                    }
                    return complexCollection;
                }
                else
                {
                    Type elementClrType = EdmLibHelpers.GetClrType(elementType, readContext.Model);
                    IEnumerable castedResult =
                        CastMethodInfo.MakeGenericMethod(elementClrType).Invoke(null, new object[] { result }) as
                            IEnumerable;
                    return castedResult;
                }
            }            
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Deserializes the given <paramref name="resourceSet"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="resourceSet">The resource set to deserialize.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <param name="elementType">The element type of the resource set being read.</param>
        /// <returns>The deserialized resource set object.</returns>
        public virtual IEnumerable ReadResourceSet(ODataResourceSetWrapperBase resourceSet, IEdmStructuredTypeReference elementType, ODataDeserializerContext readContext)
        {
            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(elementType);
            if (deserializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeDeserialized, elementType.FullName()));
            }

            //Ideally we don't need to special case ResourceSetType.ResourceSet, since the code that handles a deltaresourcesetwrapper will also handle a ResourceSetWrapper, 
            //but it may be more efficient for the common case.

            if (resourceSet.ResourceSetType == ResourceSetType.ResourceSet)
            {
                foreach (ODataResourceWrapper resourceWrapper in resourceSet.Items)
                {
                    yield return deserializer.ReadInline(resourceWrapper, elementType, readContext);
                }
            }
            else
            {
                foreach (ODataItemBase odataItemBase in resourceSet.Items)
                {
                    ODataResourceWrapper resourceWrapper = odataItemBase as ODataResourceWrapper;

                    if(resourceWrapper != null)
                    {
                        ODataDeletedResource deletedResource = resourceWrapper.ResourceBase as ODataDeletedResource;

                        if (deletedResource != null)
                        {
                            yield return DeSerializeDeletedEntity(deletedResource, elementType, readContext);
                        }
                        else
                        {
                            yield return deserializer.ReadInline(resourceWrapper, elementType, readContext);
                        }
                    }
                    else
                    {
                        ODataDeltaLinkWrapper deltaLinkWrapper = odataItemBase as ODataDeltaLinkWrapper;

                        Contract.Assert(deltaLinkWrapper != null, "ODataDeltaLinkWrapper should not be null");

                        yield return DeserializeDeltaLink(deltaLinkWrapper, resourceSet.ResourceSetBase.TypeName, readContext);
                    }                    
                }
            }
        }


        private static IEdmDeltaLinkBase DeserializeDeltaLink(ODataDeltaLinkWrapper deltaLinkWrapper, string typeName, ODataDeserializerContext readContext)
        {
            ODataDeltaLinkBase deltalink = deltaLinkWrapper.DeltaLink;

            Contract.Assert(deltalink != null, "ODataDeltaLink should not be null");

            IEdmModel model = readContext.Model;
                
            if (model == null)
            {
                throw Error.Argument("readContext", SRResources.ModelMissingFromReadContext);
            }

            IEdmEntityType actualType = model.FindType(typeName) as IEdmEntityType;
            if (actualType == null)
            {
                throw new ODataException(Error.Format(SRResources.ResourceTypeNotInModel, typeName));
            }

            if (actualType.IsAbstract)
            {
                string message = Error.Format(SRResources.CannotInstantiateAbstractResourceType, deltalink.TypeAnnotation.TypeName);
                throw new ODataException(message);
            }
                        

            IEdmDeltaLinkBase edmDeltaLink;
            ODataDeltaDeletedLink deletedLink = deltaLinkWrapper.DeltaLink as ODataDeltaDeletedLink;
            if (deletedLink != null)
            {
                edmDeltaLink = new EdmDeltaDeletedLink(actualType);
            }
            else
            {
                edmDeltaLink = new EdmDeltaLink(actualType);
            }

            edmDeltaLink.Source = deltalink.Source;
            edmDeltaLink.Target = deltalink.Target;
            edmDeltaLink.Relationship = deltalink.Relationship;

            return edmDeltaLink;
        }

        private static EdmDeltaDeletedEntityObject DeSerializeDeletedEntity(ODataDeletedResource deletedResource, IEdmStructuredTypeReference elementType, ODataDeserializerContext readContext)
        {
            Contract.Assert(deletedResource != null, "ODataDeletedResource should not be null");
            
            IEdmModel model = readContext.Model;

            if (model == null)
            {
                throw Error.Argument("readContext", SRResources.ModelMissingFromReadContext);
            }

            string typeName = deletedResource.TypeName;

            IEdmEntityType actualType = model.FindType(typeName) as IEdmEntityType;
            if (actualType == null)
            {
                throw new ODataException(Error.Format(SRResources.ResourceTypeNotInModel, typeName));
            }
            string deletedResourceId = null;

            if (deletedResource.Id != null)
            {
                deletedResourceId = deletedResource.Id.ToString();
            }
            else 
            { 
                ODataProperty key = deletedResource.Properties.FirstOrDefault(x => x.Name == actualType.Key().First().Name);

                if(key != null)
                {
                    deletedResourceId = key.Value.ToString();
                }
            }

            Contract.Assert(deletedResourceId != null, "ODataDeletedResource Id should not be null");

            if (actualType.IsAbstract)
            {
                string message = Error.Format(SRResources.CannotInstantiateAbstractResourceType, typeName);
                throw new ODataException(message);
            }

            Type type = typeof(EdmDeltaDeletedEntityObject<>).MakeGenericType(EdmLibHelpers.GetClrType(elementType, model));
            
            EdmDeltaDeletedEntityObject deletedEntity = Activator.CreateInstance(type, actualType) as EdmDeltaDeletedEntityObject;

            deletedEntity.Id = deletedResourceId;
            deletedEntity.Reason = deletedResource.Reason.Value;

            return deletedEntity;
        }
    }
}
