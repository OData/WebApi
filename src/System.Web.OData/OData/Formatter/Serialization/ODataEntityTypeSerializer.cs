// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Properties;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing instances of <see cref="IEdmEntityType"/>
    /// </summary>
    public class ODataEntityTypeSerializer : ODataEdmTypeSerializer
    {
        private const string Entry = "entry";

        /// <inheritdoc />
        public ODataEntityTypeSerializer(ODataSerializerProvider serializerProvider)
            : base(ODataPayloadKind.Entry, serializerProvider)
        {
        }

        /// <inheritdoc />
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter,
            ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            IEdmNavigationSource navigationSource = writeContext.NavigationSource;
            if (navigationSource == null)
            {
                throw new SerializationException(SRResources.NavigationSourceMissingDuringSerialization);
            }

            ODataWriter writer = messageWriter.CreateODataEntryWriter(navigationSource, navigationSource.EntityType());
            WriteObjectInline(graph, navigationSource.EntityType().ToEdmTypeReference(isNullable: false), writer, writeContext);
        }

        /// <inheritdoc />
        public override void WriteObjectInline(object graph, IEdmTypeReference expectedType, ODataWriter writer,
            ODataSerializerContext writeContext)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            if (graph == null)
            {
                throw new SerializationException(Error.Format(Properties.SRResources.CannotSerializerNull, Entry));
            }
            else
            {
                WriteEntry(graph, writer, writeContext);
            }
        }

        private void WriteEntry(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            Contract.Assert(writeContext != null);

            IEdmEntityTypeReference entityType = GetEntityType(graph, writeContext);
            EntityInstanceContext entityInstanceContext = new EntityInstanceContext(writeContext, entityType, graph);
            SelectExpandNode selectExpandNode = CreateSelectExpandNode(entityInstanceContext);
            if (selectExpandNode != null)
            {
                ODataEntry entry = CreateEntry(selectExpandNode, entityInstanceContext);
                if (entry != null)
                {
                    writer.WriteStart(entry);
                    WriteNavigationLinks(selectExpandNode.SelectedNavigationProperties, entityInstanceContext, writer);
                    WriteExpandedNavigationProperties(selectExpandNode.ExpandedNavigationProperties, entityInstanceContext, writer);
                    writer.WriteEnd();
                }
            }
        }

        /// <summary>
        /// Creates the <see cref="SelectExpandNode"/> that describes the set of properties and actions to select and expand while writing this entity.
        /// </summary>
        /// <param name="entityInstanceContext">Contains the entity instance being written and the context.</param>
        /// <returns>
        /// The <see cref="SelectExpandNode"/> that describes the set of properties and actions to select and expand while writing this entity.
        /// </returns>
        public virtual SelectExpandNode CreateSelectExpandNode(EntityInstanceContext entityInstanceContext)
        {
            if (entityInstanceContext == null)
            {
                throw Error.ArgumentNull("entityInstanceContext");
            }

            ODataSerializerContext writeContext = entityInstanceContext.SerializerContext;
            IEdmEntityType entityType = entityInstanceContext.EntityType;

            object selectExpandNode;
            Tuple<SelectExpandClause, IEdmEntityType> key = Tuple.Create(writeContext.SelectExpandClause, entityType);
            if (!writeContext.Items.TryGetValue(key, out selectExpandNode))
            {
                // cache the selectExpandNode so that if we are writing a feed we don't have to construct it again.
                selectExpandNode = new SelectExpandNode(writeContext.SelectExpandClause, entityType, writeContext.Model);
                writeContext.Items[key] = selectExpandNode;
            }
            return selectExpandNode as SelectExpandNode;
        }

        /// <summary>
        /// Creates the <see cref="ODataEntry"/> to be written while writing this entity.
        /// </summary>
        /// <param name="selectExpandNode">The <see cref="SelectExpandNode"/> describing the response graph.</param>
        /// <param name="entityInstanceContext">The context for the entity instance being written.</param>
        /// <returns>The created <see cref="ODataEntry"/>.</returns>
        public virtual ODataEntry CreateEntry(SelectExpandNode selectExpandNode, EntityInstanceContext entityInstanceContext)
        {
            if (selectExpandNode == null)
            {
                throw Error.ArgumentNull("selectExpandNode");
            }
            if (entityInstanceContext == null)
            {
                throw Error.ArgumentNull("entityInstanceContext");
            }

            string typeName = entityInstanceContext.EntityType.FullName();

            ODataEntry entry = new ODataEntry
            {
                TypeName = typeName,
                Properties = CreateStructuralPropertyBag(selectExpandNode.SelectedStructuralProperties, entityInstanceContext),
            };

            IEnumerable<ODataAction> actions = CreateODataActions(selectExpandNode.SelectedActions, entityInstanceContext);
            foreach (ODataAction action in actions)
            {
                entry.AddAction(action);
            }

            IEdmEntityType pathType = GetODataPathType(entityInstanceContext.SerializerContext);
            AddTypeNameAnnotationAsNeeded(entry, pathType, entityInstanceContext.SerializerContext.MetadataLevel);

            if (entityInstanceContext.NavigationSource != null)
            {
                if (!(entityInstanceContext.NavigationSource is IEdmContainedEntitySet))
                {
                    IEdmModel model = entityInstanceContext.SerializerContext.Model;
                    NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(entityInstanceContext.NavigationSource);
                    EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityInstanceContext, entityInstanceContext.SerializerContext.MetadataLevel);

                    if (selfLinks.IdLink != null)
                    {
                        entry.Id = selfLinks.IdLink;
                    }

                    if (selfLinks.ReadLink != null)
                    {
                        entry.ReadLink = selfLinks.ReadLink;
                    }

                    if (selfLinks.EditLink != null)
                    {
                        entry.EditLink = selfLinks.EditLink;
                    }
                }

                string etag = CreateETag(entityInstanceContext);
                if (etag != null)
                {
                    entry.ETag = etag;
                }
            }

            return entry;
        }

        /// <summary>
        /// Creates the ETag for the given entity.
        /// </summary>
        /// <param name="entityInstanceContext">The context for the entity instance being written.</param>
        /// <returns>The created ETag.</returns>
        public virtual string CreateETag(EntityInstanceContext entityInstanceContext)
        {
            if (entityInstanceContext.Request != null)
            {
                HttpConfiguration configuration = entityInstanceContext.Request.GetConfiguration();
                if (configuration == null)
                {
                    throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
                }

                IEnumerable<IEdmStructuralProperty> concurrencyProperties =
                    entityInstanceContext.EntityType.GetConcurrencyProperties().OrderBy(c => c.Name);

                IDictionary<string, object> properties = new Dictionary<string, object>();
                foreach (IEdmStructuralProperty etagProperty in concurrencyProperties)
                {
                    properties.Add(etagProperty.Name, entityInstanceContext.GetPropertyValue(etagProperty.Name));
                }
                EntityTagHeaderValue etagHeaderValue = configuration.GetETagHandler().CreateETag(properties);
                if (etagHeaderValue != null)
                {
                    return etagHeaderValue.ToString();
                }
            }

            return null;
        }

        private void WriteNavigationLinks(
            IEnumerable<IEdmNavigationProperty> navigationProperties, EntityInstanceContext entityInstanceContext, ODataWriter writer)
        {
            Contract.Assert(entityInstanceContext != null);

            IEnumerable<ODataNavigationLink> navigationLinks = CreateNavigationLinks(navigationProperties, entityInstanceContext);
            foreach (ODataNavigationLink navigationLink in navigationLinks)
            {
                writer.WriteStart(navigationLink);
                writer.WriteEnd();
            }
        }

        private void WriteExpandedNavigationProperties(
            IDictionary<IEdmNavigationProperty, SelectExpandClause> navigationPropertiesToExpand,
            EntityInstanceContext entityInstanceContext,
            ODataWriter writer)
        {
            Contract.Assert(navigationPropertiesToExpand != null);
            Contract.Assert(entityInstanceContext != null);
            Contract.Assert(writer != null);

            foreach (KeyValuePair<IEdmNavigationProperty, SelectExpandClause> navigationPropertyToExpand in navigationPropertiesToExpand)
            {
                IEdmNavigationProperty navigationProperty = navigationPropertyToExpand.Key;

                ODataNavigationLink navigationLink = CreateNavigationLink(navigationProperty, entityInstanceContext);
                if (navigationLink != null)
                {
                    writer.WriteStart(navigationLink);
                    WriteExpandedNavigationProperty(navigationPropertyToExpand, entityInstanceContext, writer);
                    writer.WriteEnd();
                }
            }
        }

        private void WriteExpandedNavigationProperty(
            KeyValuePair<IEdmNavigationProperty, SelectExpandClause> navigationPropertyToExpand,
            EntityInstanceContext entityInstanceContext,
            ODataWriter writer)
        {
            Contract.Assert(entityInstanceContext != null);
            Contract.Assert(writer != null);

            IEdmNavigationProperty navigationProperty = navigationPropertyToExpand.Key;
            SelectExpandClause selectExpandClause = navigationPropertyToExpand.Value;

            object propertyValue = entityInstanceContext.GetPropertyValue(navigationProperty.Name);

            if (propertyValue == null)
            {
                if (navigationProperty.Type.IsCollection())
                {
                    // A navigation property whose Type attribute specifies a collection, the collection always exists,
                    // it may just be empty.
                    // If a collection of entities can be related, it is represented as a JSON array. An empty
                    // collection of entities (one that contains no entities) is represented as an empty JSON array.
                    writer.WriteStart(new ODataFeed());
                }
                else
                {
                    // If at most one entity can be related, the value is null if no entity is currently related.
                    writer.WriteStart(entry: null);
                }

                writer.WriteEnd();
            }
            else
            {
                // create the serializer context for the expanded item.
                ODataSerializerContext nestedWriteContext = new ODataSerializerContext(entityInstanceContext, selectExpandClause, navigationProperty);

                // write object.
                ODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(navigationProperty.Type);
                if (serializer == null)
                {
                    throw new SerializationException(
                        Error.Format(SRResources.TypeCannotBeSerialized, navigationProperty.Type.ToTraceString(), typeof(ODataMediaTypeFormatter).Name));
                }

                serializer.WriteObjectInline(propertyValue, navigationProperty.Type, writer, nestedWriteContext);
            }
        }

        private IEnumerable<ODataNavigationLink> CreateNavigationLinks(
            IEnumerable<IEdmNavigationProperty> navigationProperties, EntityInstanceContext entityInstanceContext)
        {
            Contract.Assert(navigationProperties != null);
            Contract.Assert(entityInstanceContext != null);

            foreach (IEdmNavigationProperty navProperty in navigationProperties)
            {
                ODataNavigationLink navigationLink = CreateNavigationLink(navProperty, entityInstanceContext);
                if (navigationLink != null)
                {
                    yield return navigationLink;
                }
            }
        }

        /// <summary>
        /// Creates the <see cref="ODataNavigationLink"/> to be written while writing this entity.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for which the navigation link is being created.</param>
        /// <param name="entityInstanceContext">The context for the entity instance being written.</param>
        /// <returns>The navigation link to be written.</returns>
        public virtual ODataNavigationLink CreateNavigationLink(IEdmNavigationProperty navigationProperty, EntityInstanceContext entityInstanceContext)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }
            if (entityInstanceContext == null)
            {
                throw Error.ArgumentNull("entityInstanceContext");
            }

            ODataSerializerContext writeContext = entityInstanceContext.SerializerContext;
            ODataNavigationLink navigationLink = null;

            if (writeContext.NavigationSource != null)
            {
                IEdmTypeReference propertyType = navigationProperty.Type;
                IEdmModel model = writeContext.Model;
                NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(writeContext.NavigationSource);
                Uri navigationUrl = linkBuilder.BuildNavigationLink(entityInstanceContext, navigationProperty, writeContext.MetadataLevel);

                navigationLink = new ODataNavigationLink
                {
                    IsCollection = propertyType.IsCollection(),
                    Name = navigationProperty.Name,
                };

                if (navigationUrl != null)
                {
                    navigationLink.Url = navigationUrl;
                }
            }

            return navigationLink;
        }

        private IEnumerable<ODataProperty> CreateStructuralPropertyBag(
            IEnumerable<IEdmStructuralProperty> structuralProperties, EntityInstanceContext entityInstanceContext)
        {
            Contract.Assert(structuralProperties != null);
            Contract.Assert(entityInstanceContext != null);

            List<ODataProperty> properties = new List<ODataProperty>();
            foreach (IEdmStructuralProperty structuralProperty in structuralProperties)
            {
                ODataProperty property = CreateStructuralProperty(structuralProperty, entityInstanceContext);
                if (property != null)
                {
                    properties.Add(property);
                }
            }

            return properties;
        }

        /// <summary>
        /// Creates the <see cref="ODataProperty"/> to be written for the given entity and the structural property.
        /// </summary>
        /// <param name="structuralProperty">The EDM structural property being written.</param>
        /// <param name="entityInstanceContext">The context for the entity instance being written.</param>
        /// <returns>The <see cref="ODataProperty"/> to write.</returns>
        public virtual ODataProperty CreateStructuralProperty(IEdmStructuralProperty structuralProperty, EntityInstanceContext entityInstanceContext)
        {
            if (structuralProperty == null)
            {
                throw Error.ArgumentNull("structuralProperty");
            }
            if (entityInstanceContext == null)
            {
                throw Error.ArgumentNull("entityInstanceContext");
            }

            ODataSerializerContext writeContext = entityInstanceContext.SerializerContext;

            ODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(structuralProperty.Type);
            if (serializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeSerialized, structuralProperty.Type.FullName(), typeof(ODataMediaTypeFormatter).Name));
            }

            object propertyValue = entityInstanceContext.GetPropertyValue(structuralProperty.Name);
            return serializer.CreateProperty(propertyValue, structuralProperty.Type, structuralProperty.Name, writeContext);
        }

        private IEnumerable<ODataAction> CreateODataActions(
            IEnumerable<IEdmAction> actions, EntityInstanceContext entityInstanceContext)
        {
            Contract.Assert(actions != null);
            Contract.Assert(entityInstanceContext != null);

            foreach (IEdmAction action in actions)
            {
                ODataAction oDataAction = CreateODataAction(action, entityInstanceContext);
                if (oDataAction != null)
                {
                    yield return oDataAction;
                }
            }
        }

        /// <summary>
        /// Creates an <see cref="ODataAction" /> to be written for the given action and the entity instance.
        /// </summary>
        /// <param name="action">The OData action.</param>
        /// <param name="entityInstanceContext">The context for the entity instance being written.</param>
        /// <returns>The created action or null if the action should not be written.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2234: Pass System.Uri objects instead of strings", Justification = "This overload is equally good")]
        public virtual ODataAction CreateODataAction(IEdmAction action, EntityInstanceContext entityInstanceContext)
        {
            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            if (entityInstanceContext == null)
            {
                throw Error.ArgumentNull("entityInstanceContext");
            }

            ODataMetadataLevel metadataLevel = entityInstanceContext.SerializerContext.MetadataLevel;
            IEdmModel model = entityInstanceContext.EdmModel;

            ActionLinkBuilder builder = model.GetActionLinkBuilder(action);

            if (builder == null)
            {
                return null;
            }

            if (ShouldOmitAction(action, builder, metadataLevel))
            {
                return null;
            }

            Uri target = builder.BuildActionLink(entityInstanceContext);

            if (target == null)
            {
                return null;
            }

            Uri baseUri = new Uri(entityInstanceContext.Url.CreateODataLink(new MetadataPathSegment()));
            Uri metadata = new Uri(baseUri, "#" + CreateMetadataFragment(action));

            ODataAction odataAction = new ODataAction
            {
                Metadata = metadata,
            };

            bool alwaysIncludeDetails = metadataLevel == ODataMetadataLevel.FullMetadata;

            // Always omit the title in minimal/no metadata modes.
            if (alwaysIncludeDetails)
            {
                EmitTitle(model, action, odataAction);
            }

            // Omit the target in minimal/no metadata modes unless it doesn't follow conventions.
            if (alwaysIncludeDetails || !builder.FollowsConventions)
            {
                odataAction.Target = target;
            }

            return odataAction;
        }

        internal static void EmitTitle(IEdmModel model, IEdmOperation operation, ODataOperation odataAction)
        {
            // The title should only be emitted in full metadata.
            OperationTitleAnnotation titleAnnotation = model.GetOperationTitleAnnotation(operation);
            if (titleAnnotation != null)
            {
                odataAction.Title = titleAnnotation.Title;
            }
            else
            {
                odataAction.Title = operation.Name;
            }
        }

        internal static string CreateMetadataFragment(IEdmAction action)
        {
            // There can only be one entity container in OData V4.
            string actionName = action.Name;
            string fragment = action.Namespace + "." + actionName;

            return fragment;
        }

        private static IEdmEntityType GetODataPathType(ODataSerializerContext serializerContext)
        {
            Contract.Assert(serializerContext != null);
            if (serializerContext.NavigationProperty != null)
            {
                // we are in an expanded navigation property. use the navigation source to figure out the 
                // type.
                return serializerContext.NavigationSource.EntityType();
            }
            else
            {
                // figure out the type from the path.
                IEdmType edmType = serializerContext.Path.EdmType;
                if (edmType.TypeKind == EdmTypeKind.Collection)
                {
                    edmType = (edmType as IEdmCollectionType).ElementType.Definition;
                }

                return edmType as IEdmEntityType;
            }
        }

        internal static void AddTypeNameAnnotationAsNeeded(ODataEntry entry, IEdmEntityType odataPathType,
            ODataMetadataLevel metadataLevel)
        {
            // ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
            // null when values should not be serialized. The TypeName property is different and should always be
            // provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
            // to serialize the type name (a null value prevents serialization).

            // Note: In the current version of ODataLib the default behavior likely now matches the requirements for
            // minimal metadata mode. However, there have been behavior changes/bugs there in the past, so the safer
            // option is for this class to take control of type name serialization in minimal metadata mode.

            Contract.Assert(entry != null);

            string typeName = null; // Set null to force the type name not to serialize.

            // Provide the type name to serialize.
            if (!ShouldSuppressTypeNameSerialization(entry, odataPathType, metadataLevel))
            {
                typeName = entry.TypeName;
            }

            entry.SetAnnotation<SerializationTypeNameAnnotation>(new SerializationTypeNameAnnotation
            {
                TypeName = typeName
            });
        }

        internal static bool ShouldOmitAction(IEdmAction action, ActionLinkBuilder builder,
            ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(builder != null);

            switch (metadataLevel)
            {
                case ODataMetadataLevel.MinimalMetadata:
                case ODataMetadataLevel.NoMetadata:
                    return action.IsBound && builder.FollowsConventions;

                case ODataMetadataLevel.FullMetadata:
                default: // All values already specified; just keeping the compiler happy.
                    return false;
            }
        }

        internal static bool ShouldSuppressTypeNameSerialization(ODataEntry entry, IEdmEntityType edmType,
            ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(entry != null);

            switch (metadataLevel)
            {
                case ODataMetadataLevel.NoMetadata:
                    return true;
                case ODataMetadataLevel.FullMetadata:
                    return false;
                case ODataMetadataLevel.MinimalMetadata:
                default: // All values already specified; just keeping the compiler happy.
                    string pathTypeName = null;
                    if (edmType != null)
                    {
                        pathTypeName = edmType.FullName();
                    }
                    string entryTypeName = entry.TypeName;
                    return String.Equals(entryTypeName, pathTypeName, StringComparison.Ordinal);
            }
        }

        private IEdmEntityTypeReference GetEntityType(object graph, ODataSerializerContext writeContext)
        {
            Contract.Assert(graph != null);

            IEdmTypeReference edmType = writeContext.GetEdmType(graph, graph.GetType());
            Contract.Assert(edmType != null);

            if (!edmType.IsEntity())
            {
                throw new SerializationException(
                    Error.Format(SRResources.CannotWriteType, GetType().Name, edmType.FullName()));
            }

            return edmType.AsEntity();
        }
    }
}
