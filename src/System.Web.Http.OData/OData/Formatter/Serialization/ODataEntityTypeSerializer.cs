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
    /// ODataSerializer for serializing instances of <see cref="IEdmEntityType" />
    /// </summary>
    internal class ODataEntityTypeSerializer : ODataEntrySerializer
    {
        private const string Entry = "entry";

        private readonly IEdmEntityTypeReference _edmEntityTypeReference;

        public ODataEntityTypeSerializer(IEdmEntityTypeReference edmEntityType, ODataSerializerProvider serializerProvider)
            : base(edmEntityType, ODataPayloadKind.Entry, serializerProvider)
        {
            Contract.Assert(edmEntityType != null);
            _edmEntityTypeReference = edmEntityType;
        }

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

            if (graph == null)
            {
                throw new SerializationException(Error.Format(Properties.SRResources.CannotSerializerNull, Entry));
            }

            IEdmEntitySet entitySet = writeContext.EntitySet;

            if (entitySet == null)
            {
                throw new SerializationException(SRResources.EntitySetMissingDuringSerialization);
            }

            // No null check; entity type is not required for successful serialization.
            IEdmEntityType entityType = _edmEntityTypeReference.Definition as IEdmEntityType;

            ODataWriter writer = messageWriter.CreateODataEntryWriter(entitySet, entityType);
            WriteObjectInline(graph, writer, writeContext);
            writer.Flush();
        }

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

            if (graph != null)
            {
                IEnumerable<ODataProperty> propertyBag = CreatePropertyBag(graph, writeContext);
                WriteEntry(graph, propertyBag, writer, writeContext);
            }
            else
            {
                throw new SerializationException(Error.Format(Properties.SRResources.CannotSerializerNull, Entry));
            }
        }

        private void WriteEntry(object graph, IEnumerable<ODataProperty> propertyBag, ODataWriter writer, ODataSerializerContext writeContext)
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

            string typeName = _edmEntityTypeReference.FullName();

            ODataEntry entry = new ODataEntry
            {
                TypeName = typeName,
                Properties = propertyBag,
                Actions = CreateActions(entityInstanceContext, writeContext)
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

            writer.WriteStart(entry);
            WriteNavigationLinks(entityInstanceContext, writer, writeContext);
            writer.WriteEnd();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        private void WriteNavigationLinks(EntityInstanceContext context, ODataWriter writer, ODataSerializerContext writeContext)
        {
            Contract.Assert(writeContext != null);

            foreach (IEdmNavigationProperty navProperty in _edmEntityTypeReference.NavigationProperties())
            {
                IEdmTypeReference propertyType = navProperty.Type;

                if (writeContext.EntitySet != null)
                {
                    IEdmModel model = writeContext.Model;
                    EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(writeContext.EntitySet);
                    Uri navigationUrl = linkBuilder.BuildNavigationLink(context, navProperty, writeContext.MetadataLevel);

                    ODataNavigationLink navigationLink = new ODataNavigationLink
                    {
                        IsCollection = propertyType.IsCollection(),
                        Name = navProperty.Name,
                    };

                    if (navigationUrl != null)
                    {
                        navigationLink.Url = navigationUrl;
                    }

                    writer.WriteStart(navigationLink);
                    writer.WriteEnd();
                }
            }
        }

        private IEnumerable<ODataProperty> CreatePropertyBag(object graph, ODataSerializerContext writeContext)
        {
            IEnumerable<IEdmStructuralProperty> edmProperties = _edmEntityTypeReference.StructuralProperties();

            List<ODataProperty> properties = new List<ODataProperty>();
            foreach (IEdmStructuralProperty property in edmProperties)
            {
                ODataSerializer serializer = SerializerProvider.GetEdmTypeSerializer(property.Type);
                if (serializer == null)
                {
                    throw Error.NotSupported(SRResources.TypeCannotBeSerialized, property.Type.FullName(), typeof(ODataMediaTypeFormatter).Name);
                }

                object propertyValue = graph.GetType().GetProperty(property.Name).GetValue(graph, index: null);

                properties.Add(serializer.CreateProperty(propertyValue, property.Name, writeContext));
            }

            return properties;
        }

        private static IEnumerable<ODataAction> CreateActions(EntityInstanceContext context,
            ODataSerializerContext writeContext)
        {
            Contract.Assert(writeContext != null);

            return context.EdmModel.GetAvailableProcedures(context.EntityType)
                .Select(action => CreateODataAction(action, context, writeContext.MetadataLevel))
                .Where(action => action != null);
        }

        internal static ODataAction CreateODataAction(IEdmFunctionImport action, EntityInstanceContext context,
            ODataMetadataLevel metadataLevel)
        {
            IEdmModel model = context.EdmModel;

            if (ShouldOmitAction(action, model, metadataLevel))
            {
                return null;
            }

            ActionLinkBuilder builder = model.GetActionLinkBuilder(action);

            if (builder == null)
            {
                return null;
            }

            Uri target = builder.BuildActionLink(context);

            if (target == null)
            {
                return null;
            }

            Uri baseUri = new Uri(context.Url.ODataLink(new MetadataPathSegment()));
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

        internal static bool ShouldOmitAction(IEdmFunctionImport action, IEdmModel model,
            ODataMetadataLevel metadataLevel)
        {
            switch (metadataLevel)
            {
                case ODataMetadataLevel.MinimalMetadata:
                case ODataMetadataLevel.NoMetadata:
                    return model.IsAlwaysBindable(action);

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
            Contract.Assert(metadataLevel != ODataMetadataLevel.FullMetadata);

            switch (metadataLevel)
            {
                case ODataMetadataLevel.NoMetadata:
                    return true;
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
