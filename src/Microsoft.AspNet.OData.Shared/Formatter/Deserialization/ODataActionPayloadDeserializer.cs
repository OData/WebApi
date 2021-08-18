//-----------------------------------------------------------------------------
// <copyright file="ODataActionPayloadDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> for reading OData action parameters.
    /// </summary>
    public class ODataActionPayloadDeserializer : ODataDeserializer
    {
        private static readonly MethodInfo _castMethodInfo = typeof(Enumerable).GetMethod("Cast");

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
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
            Justification = "The majority of types referenced by this method are EdmLib types this method needs to know about to operate correctly")]
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEdmAction action = GetAction(readContext);
            Contract.Assert(action != null);

            // Create the correct resource type;
            Dictionary<string, object> payload = GetPayload(type, action);

            ODataParameterReader reader = messageReader.CreateODataParameterReader(action);

            while (reader.Read())
            {
                switch (reader.State)
                {
                    case ODataParameterReaderState.Value:
                        ReadValue(action, reader, readContext, DeserializerProvider, payload);
                        break;

                    case ODataParameterReaderState.Collection:
                        ReadCollection(action, reader, readContext, DeserializerProvider, payload);
                        break;

                    case ODataParameterReaderState.Resource:
                        ReadResource(action, reader, readContext, DeserializerProvider, payload);
                        break;

                    case ODataParameterReaderState.ResourceSet:
                        ReadResourceSet(action, reader, readContext, DeserializerProvider, payload);
                        break;
                }
            }

            return payload;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
            Justification = "The majority of types referenced by this method are EdmLib types this method needs to know about to operate correctly")]
        public override async Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEdmAction action = GetAction(readContext);
            Contract.Assert(action != null);

            // Create the correct resource type;
            Dictionary<string, object> payload = GetPayload(type, action);

            ODataParameterReader reader = await messageReader.CreateODataParameterReaderAsync(action);

            while (await reader.ReadAsync())
            {
                switch (reader.State)
                {
                    case ODataParameterReaderState.Value:
                        ReadValue(action, reader, readContext, DeserializerProvider, payload);
                        break;

                    case ODataParameterReaderState.Collection:
                        await ReadCollectionAsync(action, reader, readContext, DeserializerProvider, payload);
                        break;

                    case ODataParameterReaderState.Resource:
                        await ReadResourceAsync(action, reader, readContext, DeserializerProvider, payload);
                        break;

                    case ODataParameterReaderState.ResourceSet:
                        await ReadResourceSetAsync(action, reader, readContext, DeserializerProvider, payload);
                        break;
                }
            }

            return payload;
        }

        internal static IEdmAction GetAction(ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            ODataPath path = readContext.Path;
            if (path == null || path.Segments.Count == 0)
            {
                throw new SerializationException(SRResources.ODataPathMissing);
            }

            IEdmAction action = null;
            if (path.PathTemplate == "~/unboundaction")
            {
                // only one segment, it may be an unbound action
                OperationImportSegment unboundActionSegment = path.Segments.Last() as OperationImportSegment;
                if (unboundActionSegment != null)
                {
                    IEdmActionImport actionImport = unboundActionSegment.OperationImports.First() as IEdmActionImport;
                    if (actionImport != null)
                    {
                        action = actionImport.Action;
                    }
                }
            }
            else
            {
                // otherwise, it may be a bound action
                OperationSegment actionSegment = path.Segments.Last() as OperationSegment;
                if (actionSegment != null)
                {
                    action = actionSegment.Operations.First() as IEdmAction;
                }
            }

            if (action == null)
            {
                string message = Error.Format(SRResources.RequestNotActionInvocation, path.ToString());
                throw new SerializationException(message);
            }

            return action;
        }

        private static Dictionary<string, object> GetPayload(Type type, IEdmAction action)
        {
            // Create the correct resource type;
            if (type == typeof(ODataActionParameters))
            {
                return new ODataActionParameters();
            }
            else
            {
                return new ODataUntypedActionParameters(action);
            }
        }

        private static IEdmOperationParameter GetParameter(IEdmAction action, ODataParameterReader reader, out string parameterName)
        {
            string paramName = parameterName = reader.Name;
            IEdmOperationParameter parameter = action.Parameters.SingleOrDefault(p => p.Name == paramName);
            // ODataLib protects against this but asserting just in case.
            Contract.Assert(parameter != null, String.Format(CultureInfo.InvariantCulture, "Parameter '{0}' not found.", parameterName));
            return parameter;
        }

        private static IEdmCollectionTypeReference GetCollectionParameterType(IEdmAction action, ODataParameterReader reader, out string parameterName)
        {
            IEdmOperationParameter parameter = GetParameter(action, reader, out parameterName);
            IEdmCollectionTypeReference collectionType = parameter.Type as IEdmCollectionTypeReference;
            Contract.Assert(collectionType != null);
            return collectionType;
        }

        private static void ReadValue(IEdmAction action, ODataParameterReader reader, ODataDeserializerContext readContext, ODataDeserializerProvider deserializerProvider, Dictionary<string, object> payload)
        {
            string parameterName;
            IEdmOperationParameter parameter = GetParameter(action, reader, out parameterName);
            if (parameter.Type.IsPrimitive())
            {
                payload[parameterName] = reader.Value;
            }
            else
            {
                ODataEdmTypeDeserializer deserializer = deserializerProvider.GetEdmTypeDeserializer(parameter.Type);
                payload[parameterName] = deserializer.ReadInline(reader.Value, parameter.Type, readContext);
            }
        }

        private static void ReadCollection(IEdmAction action, ODataParameterReader reader, ODataDeserializerContext readContext, ODataDeserializerProvider deserializerProvider, Dictionary<string, object> payload)
        {
            string parameterName;
            IEdmCollectionTypeReference collectionType = GetCollectionParameterType(action, reader, out parameterName);
            ODataCollectionValue value = ODataCollectionDeserializer.ReadCollection(reader.CreateCollectionReader());
            ODataCollectionDeserializer collectionDeserializer = (ODataCollectionDeserializer)deserializerProvider.GetEdmTypeDeserializer(collectionType);
            payload[parameterName] = collectionDeserializer.ReadInline(value, collectionType, readContext);
        }

        private static async Task ReadCollectionAsync(IEdmAction action, ODataParameterReader reader, ODataDeserializerContext readContext, ODataDeserializerProvider deserializerProvider, Dictionary<string, object> payload)
        {
            string parameterName;
            IEdmCollectionTypeReference collectionType = GetCollectionParameterType(action, reader, out parameterName);
            ODataCollectionValue value = await ODataCollectionDeserializer.ReadCollectionAsync(reader.CreateCollectionReader());
            ODataCollectionDeserializer collectionDeserializer = (ODataCollectionDeserializer)deserializerProvider.GetEdmTypeDeserializer(collectionType);
            payload[parameterName] = collectionDeserializer.ReadInline(value, collectionType, readContext);
        }

        private static void ReadResource(IEdmAction action, ODataParameterReader reader, ODataDeserializerContext readContext, ODataDeserializerProvider deserializerProvider, Dictionary<string, object> payload)
        {
            string parameterName;
            IEdmOperationParameter parameter = GetParameter(action, reader, out parameterName);
            Contract.Assert(parameter.Type.IsStructured());

            object item = reader.CreateResourceReader().ReadResourceOrResourceSet();
            ODataResourceDeserializer resourceDeserializer = (ODataResourceDeserializer)deserializerProvider.GetEdmTypeDeserializer(parameter.Type);
            payload[parameterName] = resourceDeserializer.ReadInline(item, parameter.Type, readContext);
        }

        private static async Task ReadResourceAsync(IEdmAction action, ODataParameterReader reader, ODataDeserializerContext readContext, ODataDeserializerProvider deserializerProvider, Dictionary<string, object> payload)
        {
            string parameterName;
            IEdmOperationParameter parameter = GetParameter(action, reader, out parameterName);
            Contract.Assert(parameter.Type.IsStructured());

            object item = await reader.CreateResourceReader().ReadResourceOrResourceSetAsync();
            ODataResourceDeserializer resourceDeserializer = (ODataResourceDeserializer)deserializerProvider.GetEdmTypeDeserializer(parameter.Type);
            payload[parameterName] = resourceDeserializer.ReadInline(item, parameter.Type, readContext);
        }

        private static void ReadResourceSet(IEdmAction action, ODataParameterReader reader, ODataDeserializerContext readContext, ODataDeserializerProvider deserializerProvider, Dictionary<string, object> payload)
        {
            string parameterName;
            IEdmCollectionTypeReference resourceSetType = GetCollectionParameterType(action, reader, out parameterName);

            object feed = reader.CreateResourceSetReader().ReadResourceOrResourceSet();
            ProcessResourceSet(feed, resourceSetType, readContext, deserializerProvider, payload, parameterName);
        }

        private static async Task ReadResourceSetAsync(IEdmAction action, ODataParameterReader reader, ODataDeserializerContext readContext, ODataDeserializerProvider deserializerProvider, Dictionary<string, object> payload)
        {
            string parameterName;
            IEdmCollectionTypeReference resourceSetType = GetCollectionParameterType(action, reader, out parameterName);

            object feed = await reader.CreateResourceSetReader().ReadResourceOrResourceSetAsync();
            ProcessResourceSet(feed, resourceSetType, readContext, deserializerProvider, payload, parameterName);
        }

        private static void ProcessResourceSet(object feed, IEdmCollectionTypeReference resourceSetType, ODataDeserializerContext readContext, ODataDeserializerProvider deserializerProvider, Dictionary<string, object> payload, string parameterName)
        {
            ODataResourceSetDeserializer resourceSetDeserializer = (ODataResourceSetDeserializer)deserializerProvider.GetEdmTypeDeserializer(resourceSetType);

            object result = resourceSetDeserializer.ReadInline(feed, resourceSetType, readContext);

            IEdmTypeReference elementTypeReference = resourceSetType.ElementType();
            Contract.Assert(elementTypeReference.IsStructured());

            IEnumerable enumerable = result as IEnumerable;
            if (enumerable != null)
            {
                if (readContext.IsUntyped)
                {
                    payload[parameterName] = enumerable.ConvertToEdmObject(resourceSetType);
                }
                else
                {
                    Type elementClrType = EdmLibHelpers.GetClrType(elementTypeReference, readContext.Model);
                    IEnumerable castedResult =
                        _castMethodInfo.MakeGenericMethod(elementClrType)
                            .Invoke(null, new[] { result }) as IEnumerable;
                    payload[parameterName] = castedResult;
                }
            }
        }
    }
}
