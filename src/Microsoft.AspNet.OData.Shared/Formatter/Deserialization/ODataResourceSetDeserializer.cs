//-----------------------------------------------------------------------------
// <copyright file="ODataResourceSetDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
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

            ODataResourceSetWrapper resourceSet = item as ODataResourceSetWrapper;
            if (resourceSet == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataResourceSetWrapper).Name);
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            IEdmStructuredTypeReference elementType = edmType.AsCollection().ElementType().AsStructured();

            IEnumerable result = ReadResourceSet(resourceSet, elementType, readContext);
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
        public virtual IEnumerable ReadResourceSet(ODataResourceSetWrapper resourceSet, IEdmStructuredTypeReference elementType, ODataDeserializerContext readContext)
        {
            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(elementType);
            if (deserializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeDeserialized, elementType.FullName()));
            }

            foreach (ODataResourceWrapper resourceWrapper in resourceSet.Resources)
            {
                yield return deserializer.ReadInline(resourceWrapper, elementType, readContext);
            }
        }
    }
}
