// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing <see cref="IEdmComplexType" />'s.
    /// </summary>
    public class ODataResourceValueSerializer : ODataEdmTypeSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceValueSerializer"/>.
        /// </summary>
        public ODataResourceValueSerializer(ODataSerializerProvider serializerProvider)
            : base(ODataPayloadKind.Resource, serializerProvider)
        {
            if (serializerProvider == null)
            {
                throw Error.ArgumentNull("serializerProvider");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEdmTypeSerializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload that this serializer generates.</param>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write inner objects.</param>
        protected ODataResourceValueSerializer(ODataPayloadKind payloadKind, ODataSerializerProvider serializerProvider)
            : base(payloadKind, serializerProvider)
        {
            if (serializerProvider == null)
            {
                throw Error.ArgumentNull("serializerProvider");
            }
          
        }

        /// <inheritdoc/>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }
            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }
            if (writeContext.RootElementName == null)
            {
                throw Error.Argument("writeContext", SRResources.RootElementNameMissing, typeof(ODataSerializerContext).Name);
            }

            IEdmTypeReference edmType = writeContext.GetEdmType(graph, type);
            Contract.Assert(edmType != null);

            messageWriter.WriteProperty(CreateProperty(graph, edmType, writeContext.RootElementName, writeContext));
        }

        /// <inheritdoc/>
        public sealed override ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext)
        {
            if (!expectedType.IsStructured())
            {
                throw Error.InvalidOperation(SRResources.CannotWriteType, typeof(ODataResourceValueSerializer), expectedType.FullName());
            }

            ODataResourceValue value = CreateODataResourceValue(graph, expectedType.AsStructured(), writeContext);
            if (value == null)
            {
                return new ODataNullValue();
            }

            return value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "edmTypeSerializer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private ODataResourceValue CreateODataResourceValue(object graph, IEdmStructuredTypeReference expectedType, ODataSerializerContext writeContext)
        {
            ODataResourceValue resourceValue = new ODataResourceValue { TypeName = expectedType.FullName() };
            List<ODataProperty> properties = new List<ODataProperty>();

            foreach (PropertyInfo property in graph.GetType().GetProperties())
            {                
                object propertyValue = property.GetValue(graph);

                if (propertyValue != null)
                {
                    IEdmTypeReference edmTypeReference = writeContext.GetEdmType(propertyValue,
                        property.GetType());

                    ODataEdmTypeSerializer edmTypeSerializer = SerializerProvider.GetEdmTypeSerializer(edmTypeReference);

                    if (edmTypeSerializer != null)
                    {
                        ODataValue odataValue = edmTypeSerializer.CreateODataValue(propertyValue, edmTypeReference, writeContext);

                        if (odataValue != null)
                        {
                            properties.Add(new ODataProperty { Name = property.Name, Value = odataValue });
                        }
                    }
                }                
            }

            resourceValue.Properties = properties;

            return resourceValue;
        }
    }
}
