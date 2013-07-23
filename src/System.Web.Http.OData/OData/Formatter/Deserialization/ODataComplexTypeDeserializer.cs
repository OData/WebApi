// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData complex type payloads.
    /// </summary>
    public class ODataComplexTypeDeserializer : ODataEdmTypeDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataComplexTypeDeserializer"/> class.
        /// </summary>
        /// <param name="edmType">The complex type that this deserializer can read. </param>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataComplexTypeDeserializer(IEdmComplexTypeReference edmType, ODataDeserializerProvider deserializerProvider)
            : base(edmType, ODataPayloadKind.Property, deserializerProvider)
        {
            ComplexType = edmType;
        }

        /// <summary>
        /// Gets the <see cref="IEdmComplexTypeReference"/> this deserializer can read.
        /// </summary>
        public IEdmComplexTypeReference ComplexType { get; private set; }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, ODataDeserializerContext readContext)
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

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            return ReadComplexValue(complexValue, readContext);
        }

        /// <summary>
        /// Deserializes the given <paramref name="complexValue"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="complexValue">The complex value to deserialize.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized complex value.</returns>
        public virtual object ReadComplexValue(ODataComplexValue complexValue, ODataDeserializerContext readContext)
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

            object complexResource = CreateResource(ComplexType, readContext);
            foreach (ODataProperty complexProperty in complexValue.Properties)
            {
                DeserializationHelpers.ApplyProperty(complexProperty, ComplexType, complexResource, DeserializerProvider, readContext);
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
