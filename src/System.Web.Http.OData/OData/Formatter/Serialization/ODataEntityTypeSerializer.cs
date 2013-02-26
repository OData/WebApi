// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing instances of <see cref="IEdmEntityType"/>
    /// </summary>
    public class ODataEntityTypeSerializer : ODataEntrySerializer
    {
        private const string Entry = "entry";

        private readonly IEdmEntityTypeReference _edmEntityTypeReference;

        /// <inheritdoc />
        public ODataEntityTypeSerializer(IEdmEntityTypeReference edmType, ODataSerializerProvider serializerProvider)
            : base(edmType, ODataPayloadKind.Entry, serializerProvider)
        {
            Contract.Assert(edmType != null);
            _edmEntityTypeReference = edmType;
        }

        /// <summary>
        /// Gets the <see cref="IEdmEntityTypeReference"/> that this serializer handles.
        /// </summary>
        public IEdmEntityTypeReference EntityType
        {
            get
            {
                return _edmEntityTypeReference;
            }
        }

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

            IEdmEntityType entityType = _edmEntityTypeReference.EntityDefinition();
            ODataWriter writer = messageWriter.CreateODataEntryWriter(entitySet, entityType);
            WriteObjectInline(graph, writer, writeContext);
            writer.Flush();
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

            IEdmEntityType entityType = _edmEntityTypeReference.EntityDefinition();
            EntityInstanceContext entityInstanceContext = new EntityInstanceContext
            {
                Request = writeContext.Request,
                EdmModel = writeContext.Model,
                EntitySet = writeContext.EntitySet,
                EntityType = entityType,
                Url = writeContext.Url,
                EntityInstance = graph,
                SkipExpensiveAvailabilityChecks = writeContext.SkipExpensiveAvailabilityChecks
            };

            ODataEntry entry = CreateEntry(entityInstanceContext, writeContext);
            if (entry != null)
            {
                writer.WriteStart(entry);
                WriteNavigationLinks(entityInstanceContext, writer, writeContext);
                writer.WriteEnd();
            }
        }

        /// <summary>
        /// Creates the <see cref="ODataEntry"/> to be written while writing this entity.
        /// </summary>
        /// <param name="entityInstanceContext">The context for the entity instance being written.</param>
        /// <param name="writeContext">The serializer write context.</param>
        /// <returns>The created <see cref="ODataEntry"/>.</returns>
        public virtual ODataEntry CreateEntry(EntityInstanceContext entityInstanceContext, ODataSerializerContext writeContext)
        {
            string typeName = _edmEntityTypeReference.FullName();

            ODataEntry entry = new ODataEntry
            {
                TypeName = typeName,
                Properties = CreateStructuralPropertyBag(entityInstanceContext, writeContext),
                Actions = CreateODataActions(entityInstanceContext, writeContext)
            };

            AddTypeNameAnnotationAsNeeded(entry, writeContext.EntitySet, writeContext.MetadataLevel);

            if (writeContext.EntitySet != null)
            {
                IEdmModel model = writeContext.Model;
                EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(writeContext.EntitySet);
                EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityInstanceContext, writeContext.MetadataLevel);

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

        private void WriteNavigationLinks(EntityInstanceContext entityInstanceContext, ODataWriter writer, ODataSerializerContext writeContext)
        {
            Contract.Assert(writeContext != null);

            IEnumerable<ODataNavigationLink> navigationLinks = CreateNavigationLinks(entityInstanceContext, writeContext);
            if (navigationLinks != null)
            {
                foreach (ODataNavigationLink navigationLink in navigationLinks)
                {
                    writer.WriteStart(navigationLink);
                    writer.WriteEnd();
                }
            }
        }

        /// <summary>
        /// Creates the collection of <see cref="ODataNavigationLink"/>s to be written while writing this entity.
        /// </summary>
        /// <param name="entityInstanceContext">The context for the entity instance being written.</param>
        /// <param name="writeContext">The serializer write context.</param>
        /// <returns>The collection of navigation links to be written.</returns>
        public virtual IEnumerable<ODataNavigationLink> CreateNavigationLinks(EntityInstanceContext entityInstanceContext, ODataSerializerContext writeContext)
        {
            foreach (IEdmNavigationProperty navProperty in _edmEntityTypeReference.NavigationProperties())
            {
                IEdmTypeReference propertyType = navProperty.Type;

                if (writeContext.EntitySet != null)
                {
                    IEdmModel model = writeContext.Model;
                    EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(writeContext.EntitySet);
                    Uri navigationUrl = linkBuilder.BuildNavigationLink(entityInstanceContext, navProperty, writeContext.MetadataLevel);

                    ODataNavigationLink navigationLink = new ODataNavigationLink
                    {
                        IsCollection = propertyType.IsCollection(),
                        Name = navProperty.Name,
                    };

                    if (navigationUrl != null)
                    {
                        navigationLink.Url = navigationUrl;
                    }

                    yield return navigationLink;
                }
            }
        }

        /// <summary>
        /// Creates the collection of <see cref="ODataProperty" />s to be written while writing this entity.
        /// </summary>
        /// <param name="entityInstanceContext">The context for the entity instance being written.</param>
        /// <param name="writeContext">The serializer write context.</param>
        /// <returns>The collection of properties to be written.</returns>
        public virtual IEnumerable<ODataProperty> CreateStructuralPropertyBag(EntityInstanceContext entityInstanceContext, ODataSerializerContext writeContext)
        {
            IEnumerable<IEdmStructuralProperty> edmProperties = _edmEntityTypeReference.StructuralProperties();

            List<ODataProperty> properties = new List<ODataProperty>();
            foreach (IEdmStructuralProperty property in edmProperties)
            {
                properties.Add(CreateStructuralProperty(property, entityInstanceContext.EntityInstance, writeContext));
            }

            return properties;
        }

        /// <summary>
        /// Creates the <see cref="ODataProperty"/> to be written for the given entity and the structural property.
        /// </summary>
        /// <param name="structuralProperty">The EDM structural property being written.</param>
        /// <param name="entityInstance">The entity being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The <see cref="ODataProperty"/> to write.</returns>
        public virtual ODataProperty CreateStructuralProperty(IEdmStructuralProperty structuralProperty, object entityInstance, ODataSerializerContext writeContext)
        {
            ODataEntrySerializer serializer = SerializerProvider.GetEdmTypeSerializer(structuralProperty.Type);
            if (serializer == null)
            {
                throw Error.NotSupported(SRResources.TypeCannotBeSerialized, structuralProperty.Type.FullName(), typeof(ODataMediaTypeFormatter).Name);
            }

            object propertyValue = entityInstance.GetType().GetProperty(structuralProperty.Name).GetValue(entityInstance, index: null);
            return serializer.CreateProperty(propertyValue, structuralProperty.Name, writeContext);
        }

        /// <summary>
        /// Creates the collection of <see cref="ODataAction"/>s to be written while writing this entity.
        /// </summary>
        /// <param name="entityInstanceContext">The context for the entity instance being written. </param>
        /// <param name="writeContext">The serializer write context.</param>
        /// <returns>The collection of actions to be written.</returns>
        public virtual IEnumerable<ODataAction> CreateODataActions(EntityInstanceContext entityInstanceContext, ODataSerializerContext writeContext)
        {
            Contract.Assert(writeContext != null);

            return entityInstanceContext.EdmModel.GetAvailableProcedures(entityInstanceContext.EntityType)
                .Select(action => CreateODataAction(action, entityInstanceContext, writeContext.MetadataLevel))
                .Where(action => action != null);
        }

        /// <summary>
        /// Creates an <see cref="ODataAction" /> to be written for the given action and the entity instance.
        /// </summary>
        /// <param name="action">The OData action.</param>
        /// <param name="entityInstanceContext">The context for the entity instance being written.</param>
        /// <param name="metadataLevel">The <see cref="ODataMetadataLevel"/> of the response.</param>
        /// <returns>The created action or null if the action should not be written.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2234: Pass System.Uri objects instead of strings", Justification = "This overload is equally good")]
        public virtual ODataAction CreateODataAction(IEdmFunctionImport action, EntityInstanceContext entityInstanceContext, ODataMetadataLevel metadataLevel)
        {
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
