﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Properties;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Deserialization
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
                IEdmOperationParameter parameter = null;

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
                        ODataCollectionDeserializer collectionDeserializer = (ODataCollectionDeserializer)DeserializerProvider.GetEdmTypeDeserializer(collectionType);
                        payload[parameterName] = collectionDeserializer.ReadInline(value, collectionType, readContext);
                        break;

                    case ODataParameterReaderState.Entry:
                        parameterName = reader.Name;
                        parameter = action.Parameters.SingleOrDefault(p => p.Name == parameterName);
                        Contract.Assert(parameter != null, String.Format(CultureInfo.InvariantCulture, "Parameter '{0}' not found.", parameterName));

                        IEdmEntityTypeReference entityTypeReference = parameter.Type as IEdmEntityTypeReference;
                        Contract.Assert(entityTypeReference != null);

                        ODataReader entryReader = reader.CreateEntryReader();
                        object item = ODataEntityDeserializer.ReadEntryOrFeed(entryReader);
                        ODataEntityDeserializer entityDeserializer = (ODataEntityDeserializer)DeserializerProvider.GetEdmTypeDeserializer(entityTypeReference);
                        payload[parameterName] = entityDeserializer.ReadInline(item, entityTypeReference, readContext);
                        break;

                    case ODataParameterReaderState.Feed:
                        parameterName = reader.Name;
                        parameter = action.Parameters.SingleOrDefault(p => p.Name == parameterName);
                        Contract.Assert(parameter != null, String.Format(CultureInfo.InvariantCulture, "Parameter '{0}' not found.", parameterName));

                        IEdmCollectionTypeReference feedType = parameter.Type as IEdmCollectionTypeReference;
                        Contract.Assert(feedType != null);

                        ODataReader feedReader = reader.CreateFeedReader();
                        object feed = ODataEntityDeserializer.ReadEntryOrFeed(feedReader);
                        ODataFeedDeserializer feedDeserializer = (ODataFeedDeserializer)DeserializerProvider.GetEdmTypeDeserializer(feedType);

                        object result = feedDeserializer.ReadInline(feed, feedType, readContext);

                        IEdmTypeReference elementTypeReference = feedType.ElementType();
                        Contract.Assert(elementTypeReference.IsEntity());

                        IEnumerable enumerable = result as IEnumerable;
                        if (enumerable != null)
                        {
                            if (readContext.IsUntyped)
                            {
                                EdmEntityObjectCollection entityCollection = new EdmEntityObjectCollection(feedType);
                                foreach (EdmEntityObject entityObject in enumerable)
                                {
                                    entityCollection.Add(entityObject);
                                }

                                payload[parameterName] = entityCollection;
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
                UnboundActionPathSegment unboundActionSegment = path.Segments.Last() as UnboundActionPathSegment;
                if (unboundActionSegment != null)
                {
                    action = unboundActionSegment.Action.Action;
                }
            }
            else
            {
                // otherwise, it may be a bound action
                BoundActionPathSegment actionSegment = path.Segments.Last() as BoundActionPathSegment;
                if (actionSegment != null)
                {
                    action = actionSegment.Action;
                }
            }

            if (action == null)
            {
                string message = Error.Format(SRResources.RequestNotActionInvocation, path.ToString());
                throw new SerializationException(message);
            }

            return action;
        }
    }
}