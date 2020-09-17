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

            if (graph == null)
            {
                return new ODataNullValue();
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
            List<ODataProperty> properties = new List<ODataProperty>();
            ODataResourceValue resourceValue = new ODataResourceValue { TypeName = expectedType.FullName() };
  
            IDelta delta = graph as IDelta;
            if (delta != null)
            {
                foreach (string propertyName in delta.GetChangedPropertyNames())
                {
                    object propertyValue;
                    
                    if (delta.TryGetPropertyValue(propertyName, out propertyValue))
                    {
                        SetPropertyValue(writeContext, properties, expectedType, propertyName, propertyValue);
                    }
                }

                foreach (string propertyName in delta.GetUnchangedPropertyNames())
                {
                    object propertyValue;
                    if(delta.TryGetPropertyValue(propertyName, out propertyValue))
                    {
                        SetPropertyValue(writeContext, properties, expectedType, propertyName, propertyValue);
                    }
                }
            }
            else
            {
                foreach (PropertyInfo property in graph.GetType().GetProperties())
                {
                    object propertyValue = property.GetValue(graph);

                    SetPropertyValue(writeContext, properties, expectedType, property.Name, propertyValue);
                }
            }            

            resourceValue.Properties = properties;

            return resourceValue;
        }

        private void SetPropertyValue(ODataSerializerContext writeContext, List<ODataProperty> properties, IEdmStructuredTypeReference expectedType, string propertyName, object propertyValue)
        {
            IEdmTypeReference edmTypeReference;
            ODataEdmTypeSerializer edmTypeSerializer;

            edmTypeReference = propertyValue == null ? expectedType : writeContext.GetEdmType(propertyValue,
                propertyValue.GetType());
            edmTypeSerializer = GetResourceValueEdmTypeSerializer(edmTypeReference);

            if (edmTypeSerializer != null)
            {
                ODataValue odataValue = edmTypeSerializer.CreateODataValue(propertyValue, edmTypeReference, writeContext);
                properties.Add(new ODataProperty { Name = propertyName, Value = odataValue });
            }
        }

        private ODataEdmTypeSerializer GetResourceValueEdmTypeSerializer(IEdmTypeReference edmTypeReference)
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
