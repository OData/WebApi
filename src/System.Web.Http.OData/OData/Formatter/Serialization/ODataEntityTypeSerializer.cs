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
                throw new SerializationException(Error.Format(Properties.SRResources.CannotSerializerNull,
                    ODataFormatterConstants.Entry));
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
                throw new SerializationException(Error.Format(Properties.SRResources.CannotSerializerNull, ODataFormatterConstants.Entry));
            }
        }

        private void WriteEntry(object graph, IEnumerable<ODataProperty> propertyBag, ODataWriter writer, ODataSerializerContext writeContext)
        {
            Contract.Assert(writeContext != null);

            IEdmEntityType entityType = _edmEntityTypeReference.EntityDefinition();
            EntityInstanceContext entityInstanceContext = new EntityInstanceContext
            {
                EdmModel = writeContext.Model,
                EntitySet = writeContext.EntitySet,
                EntityType = entityType,
                UrlHelper = writeContext.UrlHelper,
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

            if (writeContext.MetadataLevel == ODataMetadataLevel.NoMetadata)
            {
                return Enumerable.Empty<ODataAction>();
            }

            return context.EdmModel.GetAvailableProcedures(context.EntityType)
                .Select(action => CreateODataAction(action, context))
                .Where(action => action != null);
        }

        private static ODataAction CreateODataAction(IEdmFunctionImport action, EntityInstanceContext context)
        {
            ActionLinkBuilder builder = context.EdmModel.GetActionLinkBuilder(action);
            if (builder != null)
            {
                Uri target = builder.BuildActionLink(context);
                if (target != null)
                {
                    Uri baseUri = new Uri(context.UrlHelper.ODataLink(new MetadataPathSegment()));
                    Uri metadata = new Uri(baseUri, "#" + action.Container.Name + "." + action.Name);

                    return new ODataAction
                    {
                        Metadata = metadata,
                        Target = target,
                        Title = action.Name
                    };
                }
            }
            return null;
        }

        internal static void AddTypeNameAnnotationAsNeeded(ODataEntry entry, IEdmEntitySet entitySet,
            ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(entry != null);

            if (ShouldAddTypeNameAnnotation(metadataLevel))
            {
                string typeName;

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
            switch (metadataLevel)
            {
                case ODataMetadataLevel.Default:
                case ODataMetadataLevel.FullMetadata:
                    return false;
                case ODataMetadataLevel.MinimalMetadata:
                case ODataMetadataLevel.NoMetadata:
                default: // All values already specified; just keeping the compiler happy.
                    return true;
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
