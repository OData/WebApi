// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query.SemanticAst;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing instances of <see cref="IEdmEntityType"/>
    /// </summary>
    public class ODataEntityTypeSerializer : ODataEdmTypeSerializer
    {
        private const string Entry = "entry";

        /// <inheritdoc />
        public ODataEntityTypeSerializer(IEdmEntityTypeReference edmType, ODataSerializerProvider serializerProvider)
            : base(edmType, ODataPayloadKind.Entry, serializerProvider)
        {
            Contract.Assert(edmType != null);
            EntityType = edmType;
        }

        /// <summary>
        /// Gets the <see cref="IEdmEntityTypeReference"/> that this serializer handles.
        /// </summary>
        public IEdmEntityTypeReference EntityType { get; private set; }

        /// <inheritdoc />
        public override void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            IEdmEntitySet entitySet = writeContext.EntitySet;

            if (entitySet == null)
            {
                throw new SerializationException(SRResources.EntitySetMissingDuringSerialization);
            }

            IEdmEntityType entityType = EntityType.EntityDefinition();
            ODataWriter writer = messageWriter.CreateODataEntryWriter(entitySet, entityType);
            WriteObjectInline(graph, writer, writeContext);
        }

        /// <inheritdoc />
        public override void WriteObjectInline(object graph, ODataWriter writer, ODataSerializerContext writeContext)
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

            EntityInstanceContext entityInstanceContext = new EntityInstanceContext(writeContext, EntityType, graph);
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
            return new SelectExpandNode(writeContext.SelectExpandClause, EntityType, writeContext.Model);
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

            string typeName = EntityType.FullName();

            ODataEntry entry = new ODataEntry
            {
                TypeName = typeName,
                Properties = CreateStructuralPropertyBag(selectExpandNode.SelectedStructuralProperties, entityInstanceContext),
                Actions = CreateODataActions(selectExpandNode.SelectedActions, entityInstanceContext)
            };

            AddTypeNameAnnotationAsNeeded(entry, entityInstanceContext.EntitySet, entityInstanceContext.SerializerContext.MetadataLevel);

            if (entityInstanceContext.EntitySet != null)
            {
                IEdmModel model = entityInstanceContext.SerializerContext.Model;
                EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(entityInstanceContext.EntitySet);
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

            return entry;
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

            ODataSerializerContext writeContext = entityInstanceContext.SerializerContext;

            IEdmNavigationProperty navigationProperty = navigationPropertyToExpand.Key;
            SelectExpandClause selectExpandClause = navigationPropertyToExpand.Value;

            object propertyValue = entityInstanceContext.GetPropertyValue(navigationProperty.Name);
            if (propertyValue != null)
            {
                // create the serializer context for the expanded item.
                ODataSerializerContext nestedWriteContext = new ODataSerializerContext(entityInstanceContext, selectExpandClause, navigationProperty);

                // write object.
                Type propertyType = propertyValue.GetType();
                ODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(writeContext.Model, propertyValue);
                if (serializer == null)
                {
                    throw new SerializationException(
                        Error.Format(SRResources.TypeCannotBeSerialized, propertyType.FullName, typeof(ODataMediaTypeFormatter).Name));
                }
                serializer.WriteObjectInline(propertyValue, writer, nestedWriteContext);
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

            if (writeContext.EntitySet != null)
            {
                IEdmTypeReference propertyType = navigationProperty.Type;
                IEdmModel model = writeContext.Model;
                EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(writeContext.EntitySet);
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
            return serializer.CreateProperty(propertyValue, structuralProperty.Name, writeContext);
        }

        private IEnumerable<ODataAction> CreateODataActions(
            IEnumerable<IEdmFunctionImport> actions, EntityInstanceContext entityInstanceContext)
        {
            Contract.Assert(actions != null);
            Contract.Assert(entityInstanceContext != null);

            foreach (IEdmFunctionImport action in actions)
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
        public virtual ODataAction CreateODataAction(IEdmFunctionImport action, EntityInstanceContext entityInstanceContext)
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

            if (ShouldOmitAction(action, model, builder, metadataLevel))
            {
                return null;
            }

            Uri target = builder.BuildActionLink(entityInstanceContext);

            if (target == null)
            {
                return null;
            }

            Uri baseUri = new Uri(entityInstanceContext.Url.ODataLink(new MetadataPathSegment()));
            Uri metadata = new Uri(baseUri, "#" + CreateMetadataFragment(action, model, metadataLevel));

            ODataAction odataAction = new ODataAction
            {
                Metadata = metadata,
            };

            bool alwaysIncludeDetails = metadataLevel == ODataMetadataLevel.Default ||
                metadataLevel == ODataMetadataLevel.FullMetadata;

            // Always omit the title in minimal/no metadata modes (it isn't customizable and thus always follows
            // conventions).
            if (alwaysIncludeDetails)
            {
                odataAction.Title = action.Name;
            }

            // Omit the target in minimal/no metadata modes unless it doesn't follow conventions.
            if (alwaysIncludeDetails || !builder.FollowsConventions)
            {
                odataAction.Target = target;
            }

            return odataAction;
        }

        internal static string CreateMetadataFragment(IEdmFunctionImport action, IEdmModel model,
            ODataMetadataLevel metadataLevel)
        {
            IEdmEntityContainer container = action.Container;
            string actionName = action.Name;
            string fragment;

            if ((metadataLevel == ODataMetadataLevel.MinimalMetadata || metadataLevel == ODataMetadataLevel.NoMetadata)
                && model.IsDefaultEntityContainer(container))
            {
                fragment = actionName;
            }
            else
            {
                fragment = container.Name + "." + actionName;
            }

            return fragment;
        }

        internal static void AddTypeNameAnnotationAsNeeded(ODataEntry entry, IEdmEntitySet entitySet,
            ODataMetadataLevel metadataLevel)
        {
            // ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
            // null when values should not be serialized. The TypeName property is different and should always be
            // provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
            // to serialize the type name (a null value prevents serialization).

            // Note that this annotation should not be used for Atom or JSON verbose formats, as it will interfere with
            // the correct default behavior for those formats.

            Contract.Assert(entry != null);

            // Only add an annotation if we want to override ODataLib's default type name serialization behavior.
            if (ShouldAddTypeNameAnnotation(metadataLevel))
            {
                string typeName;

                // Provide the type name to serialize (or null to force it not to serialize).
                if (ShouldSuppressTypeNameSerialization(entry, entitySet, metadataLevel))
                {
                    typeName = null;
                }
                else
                {
                    typeName = entry.TypeName;
                }

                entry.SetAnnotation<SerializationTypeNameAnnotation>(new SerializationTypeNameAnnotation
                {
                    TypeName = typeName
                });
            }
        }

        internal static bool ShouldAddTypeNameAnnotation(ODataMetadataLevel metadataLevel)
        {
            // Don't interfere with the correct default behavior in non-JSON light formats.
            // In all JSON light modes, take control of type name serialization.
            // Note: In the current version of ODataLib the default behavior likely now matches the requirements for
            // minimal metadata mode. However, there have been behavior changes/bugs there in the past, so the safer
            // option is for this class to take control of type name serialization in minimal metadata mode.
            return metadataLevel != ODataMetadataLevel.Default;
        }

        internal static bool ShouldOmitAction(IEdmFunctionImport action, IEdmModel model, ActionLinkBuilder builder,
            ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(model != null);
            Contract.Assert(builder != null);

            switch (metadataLevel)
            {
                case ODataMetadataLevel.MinimalMetadata:
                case ODataMetadataLevel.NoMetadata:
                    return model.IsAlwaysBindable(action) && builder.FollowsConventions;

                case ODataMetadataLevel.Default:
                case ODataMetadataLevel.FullMetadata:
                default: // All values already specified; just keeping the compiler happy.
                    return false;
            }
        }

        internal static bool ShouldSuppressTypeNameSerialization(ODataEntry entry, IEdmEntitySet entitySet,
            ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(entry != null);

            Contract.Assert(metadataLevel != ODataMetadataLevel.Default);

            switch (metadataLevel)
            {
                case ODataMetadataLevel.NoMetadata:
                    return true;
                case ODataMetadataLevel.FullMetadata:
                    return false;
                case ODataMetadataLevel.MinimalMetadata:
                default: // All values already specified; just keeping the compiler happy.
                    string entitySetTypeName = GetElementTypeName(entitySet);
                    string entryTypeName = entry.TypeName;
                    return String.Equals(entryTypeName, entitySetTypeName, StringComparison.Ordinal);
            }
        }

        private static string GetElementTypeName(IEdmEntitySet entitySet)
        {
            if (entitySet == null)
            {
                return null;
            }

            IEdmEntityType elementType = entitySet.ElementType;

            if (elementType == null)
            {
                return null;
            }

            return elementType.FullName();
        }
    }
}
