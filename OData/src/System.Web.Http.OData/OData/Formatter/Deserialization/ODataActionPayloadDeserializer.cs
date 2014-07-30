// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> for reading OData action parameters.
    /// </summary>
    public class ODataActionPayloadDeserializer : ODataDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionPayloadDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataActionPayloadDeserializer(ODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.Parameter)
        {
            if (deserializerProvider == null)
            {
                throw Error.ArgumentNull("deserializerProvider");
            }

            DeserializerProvider = deserializerProvider;
        }

        /// <summary>
        /// Gets the deserializer provider to use to read inner objects.
        /// </summary>
        public ODataDeserializerProvider DeserializerProvider { get; private set; }

        /// <inheritdoc />
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEdmFunctionImport action = GetFunctionImport(readContext);

            // Create the correct resource type;
            Dictionary<string, object> payload;
            if (type == typeof(ODataActionParameters))
            {
                payload = new ODataActionParameters();
            }
            else
            {
                payload = new ODataUntypedActionParameters(action);
            }

            ODataParameterReader reader = messageReader.CreateODataParameterReader(action);

            while (reader.Read())
            {
                string parameterName = null;
                IEdmFunctionParameter parameter = null;

                switch (reader.State)
                {
                    case ODataParameterReaderState.Value:
                        parameterName = reader.Name;
                        parameter = action.Parameters.SingleOrDefault(p => p.Name == parameterName);
                        // ODataLib protects against this but asserting just in case.
                        Contract.Assert(parameter != null, String.Format(CultureInfo.InvariantCulture, "Parameter '{0}' not found.", parameterName));
                        if (parameter.Type.IsPrimitive())
                        {
                            payload[parameterName] = reader.Value;
                        }
                        else
                        {
                            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(parameter.Type);
                            payload[parameterName] = deserializer.ReadInline(reader.Value, parameter.Type, readContext);
                        }
                        break;

                    case ODataParameterReaderState.Collection:
                        parameterName = reader.Name;
                        parameter = action.Parameters.SingleOrDefault(p => p.Name == parameterName);
                        // ODataLib protects against this but asserting just in case.
                        Contract.Assert(parameter != null, String.Format(CultureInfo.InvariantCulture, "Parameter '{0}' not found.", parameterName));
                        IEdmCollectionTypeReference collectionType = parameter.Type as IEdmCollectionTypeReference;
                        Contract.Assert(collectionType != null);
                        ODataCollectionValue value = ODataCollectionDeserializer.ReadCollection(reader.CreateCollectionReader());
                        ODataCollectionDeserializer collectionDeserializer = DeserializerProvider.GetEdmTypeDeserializer(collectionType) as ODataCollectionDeserializer;
                        payload[parameterName] = collectionDeserializer.ReadInline(value, collectionType, readContext);
                        break;

                    default:
                        break;
                }
            }

            return payload;
        }

        internal static IEdmFunctionImport GetFunctionImport(ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            ODataPath path = readContext.Path;
            if (path == null)
            {
                throw new SerializationException(SRResources.ODataPathMissing);
            }

            ActionPathSegment lastSegment = path.Segments.Last() as ActionPathSegment;
            if (lastSegment == null)
            {
                string message = Error.Format(SRResources.RequestNotActionInvocation, path.ToString());
                throw new SerializationException(message);
            }
            return lastSegment.Action;
        }
    }
}