// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData complex type payloads.
    /// </summary>
    public class ODataComplexTypeDeserializer : ODataEdmTypeDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataComplexTypeDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataComplexTypeDeserializer(ODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.Property, deserializerProvider)
        {
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            if (item == null)
            {
                return null;
            }

            ODataComplexValue complexValue = item as ODataComplexValue;
            if (complexValue == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataComplexValue).Name);
            }

            if (!edmType.IsComplex())
            {
                throw Error.Argument("edmType", SRResources.ArgumentMustBeOfType, EdmTypeKind.Complex);
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            return ReadComplexValue(complexValue, edmType.AsComplex(), readContext);
        }

        /// <summary>
        /// Deserializes the given <paramref name="complexValue"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="complexValue">The complex value to deserialize.</param>
        /// <param name="complexType">The EDM type of the complex value to read.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized complex value.</returns>
        public virtual object ReadComplexValue(ODataComplexValue complexValue, IEdmComplexTypeReference complexType,
            ODataDeserializerContext readContext)
        {
            if (complexValue == null)
            {
                throw Error.ArgumentNull("complexValue");
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            if (readContext.Model == null)
            {
                throw Error.Argument("readContext", SRResources.ModelMissingFromReadContext);
            }

            object complexResource = CreateResource(complexType, readContext);
            foreach (ODataProperty complexProperty in complexValue.Properties)
            {
                DeserializationHelpers.ApplyProperty(complexProperty, complexType, complexResource, DeserializerProvider, readContext);
            }
            return complexResource;
        }

        internal static object CreateResource(IEdmComplexTypeReference edmComplexType, ODataDeserializerContext readContext)
        {
            Contract.Assert(edmComplexType != null);

            if (readContext.IsUntyped)
            {
                return new EdmComplexObject(edmComplexType);
            }
            else
            {
                Type clrType = EdmLibHelpers.GetClrType(edmComplexType, readContext.Model);
                if (clrType == null)
                {
                    throw Error.InvalidOperation(SRResources.MappingDoesNotContainEntityType, edmComplexType.FullName());
                }

                return Activator.CreateInstance(clrType);
            }
        }
    }
}
