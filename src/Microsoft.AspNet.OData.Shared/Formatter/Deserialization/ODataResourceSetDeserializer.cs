// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
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

            ODataReader resourceSetReader = readContext.IsChangedObjectCollection ? messageReader.CreateODataDeltaResourceSetReader() : messageReader.CreateODataResourceSetReader();
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

            ODataReader resourceSetReader = readContext.IsChangedObjectCollection ? await messageReader.CreateODataDeltaResourceSetReaderAsync() : await messageReader.CreateODataResourceSetReaderAsync();
            object resourceSet = await resourceSetReader.ReadResourceOrResourceSetAsync();
            return ReadInline(resourceSet, edmType, readContext);
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
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

            IEnumerable result = ReadResourceSet(resourceSet, elementType, readContext);

            //Handle Delta requests to create EdmChangedObjectCollection
            if (resourceSet.ResourceSetType == ResourceSetType.DeltaResourceSet)
            {
                IEdmEntityType actualType = elementType.AsEntity().Definition as IEdmEntityType;                
                
                if (readContext.IsUntyped)
                {
                    EdmChangedObjectCollection edmCollection = new EdmChangedObjectCollection(actualType);

                    foreach (IEdmChangedObject changedObject in result)
                    {
                        edmCollection.Add(changedObject);
                    }

                    return edmCollection;
                }
                else
                {
                    ICollection<IDeltaSetItem> deltaSet;
                    deltaSet = CreateDeltaSet(actualType.Key().Select(x=>x.Name).ToList(), readContext, elementType, result);

                    return deltaSet;
                }
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

        private static ICollection<IDeltaSetItem> CreateDeltaSet(IList<string> keys, ODataDeserializerContext readContext, IEdmStructuredTypeReference elementType, IEnumerable result)
        {
            ICollection<IDeltaSetItem> deltaSet;
            Type type = EdmLibHelpers.GetClrType(elementType, readContext.Model);
            Type changedObjCollType = typeof(DeltaSet<>).MakeGenericType(type);
                        
            deltaSet = Activator.CreateInstance(changedObjCollType, keys) as ICollection<IDeltaSetItem>;

            foreach (IDeltaSetItem changedObject in result)
            {
                deltaSet.Add(changedObject);
            }

            return deltaSet;
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
                foreach (ODataResourceWrapper resourceWrapper in resourceSet.Resources)
                {
                    yield return deserializer.ReadInline(resourceWrapper, elementType, readContext);
                }
            }
            else
            {
                Type clrType = EdmLibHelpers.GetClrType(elementType, readContext.Model);                           

                foreach (ODataResourceWrapper resourceWrapper in resourceSet.Resources)
                {
                    if (readContext.IsUntyped)
                    {
                        readContext.ResourceType = resourceWrapper.ResourceBase is ODataDeletedResource ? typeof(EdmDeltaDeletedEntityObject) : typeof(EdmEntityObject);
                    }
                    else
                    {
                        readContext.ResourceType = resourceWrapper.ResourceBase is ODataDeletedResource ? typeof(DeltaDeletedEntityObject<>).MakeGenericType(clrType) : typeof(Delta<>).MakeGenericType(clrType);
                    }                   

                    if (resourceWrapper != null)
                    {                        
                        yield return deserializer.ReadInline(resourceWrapper, elementType, readContext);                     
                    }                   
                }
            }
        }

    }
}
