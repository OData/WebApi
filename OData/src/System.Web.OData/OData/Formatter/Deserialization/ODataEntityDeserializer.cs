// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;

namespace System.Web.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> for reading OData entry payloads.
    /// </summary>
    public class ODataEntityDeserializer : ODataEdmTypeDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntityDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataEntityDeserializer(ODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.Entry, deserializerProvider)
        {
        }

        /// <inheritdoc />
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            if (readContext.Path == null)
            {
                throw Error.Argument("readContext", SRResources.ODataPathMissing);
            }

            IEdmNavigationSource navigationSource = readContext.Path.NavigationSource;
            if (navigationSource == null)
            {
                throw new SerializationException(SRResources.NavigationSourceMissingDuringDeserialization);
            }

            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            if (!edmType.IsEntity())
            {
                throw Error.Argument("type", SRResources.ArgumentMustBeOfType, EdmTypeKind.Entity);
            }

            IEdmEntityTypeReference entityType = edmType.AsEntity();

            ODataReader odataReader = messageReader.CreateODataEntryReader(navigationSource, entityType.EntityDefinition());
            ODataEntryWithNavigationLinks topLevelEntry = ReadEntryOrFeed(odataReader) as ODataEntryWithNavigationLinks;
            Contract.Assert(topLevelEntry != null);

            return ReadInline(topLevelEntry, entityType, readContext);
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                throw Error.ArgumentNull("item");
            }
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }
            if (!edmType.IsEntity())
            {
                throw Error.Argument("edmType", SRResources.ArgumentMustBeOfType, EdmTypeKind.Entity);
            }

            ODataEntryWithNavigationLinks entryWrapper = item as ODataEntryWithNavigationLinks;
            if (entryWrapper == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataEntry).Name);
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            return ReadEntry(entryWrapper, edmType.AsEntity(), readContext);
        }

        /// <summary>
        /// Deserializes the given <paramref name="entryWrapper"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="entryWrapper">The OData entry to deserialize.</param>
        /// <param name="entityType">The entity type of the entry to deserialize.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized entity.</returns>
        public virtual object ReadEntry(ODataEntryWithNavigationLinks entryWrapper, IEdmEntityTypeReference entityType,
            ODataDeserializerContext readContext)
        {
            if (entryWrapper == null)
            {
                throw Error.ArgumentNull("entryWrapper");
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            if (!String.IsNullOrEmpty(entryWrapper.Entry.TypeName) && entityType.FullName() != entryWrapper.Entry.TypeName)
            {
                // received a derived type in a base type deserializer. delegate it to the appropriate derived type deserializer.
                IEdmModel model = readContext.Model;

                if (model == null)
                {
                    throw Error.Argument("readContext", SRResources.ModelMissingFromReadContext);
                }

                IEdmEntityType actualType = model.FindType(entryWrapper.Entry.TypeName) as IEdmEntityType;
                if (actualType == null)
                {
                    throw new ODataException(Error.Format(SRResources.EntityTypeNotInModel, entryWrapper.Entry.TypeName));
                }

                if (actualType.IsAbstract)
                {
                    string message = Error.Format(SRResources.CannotInstantiateAbstractEntityType, entryWrapper.Entry.TypeName);
                    throw new ODataException(message);
                }

                IEdmTypeReference actualEntityType = new EdmEntityTypeReference(actualType, isNullable: false);
                ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(actualEntityType);
                if (deserializer == null)
                {
                    throw new SerializationException(
                        Error.Format(SRResources.TypeCannotBeDeserialized, actualEntityType.FullName(), typeof(ODataMediaTypeFormatter).Name));
                }

                object resource = deserializer.ReadInline(entryWrapper, actualEntityType, readContext);

                EdmStructuredObject structuredObject = resource as EdmStructuredObject;
                if (structuredObject != null)
                {
                    structuredObject.ExpectedEdmType = entityType.EntityDefinition();
                }

                return resource;
            }
            else
            {
                object resource = CreateEntityResource(entityType, readContext);
                ApplyEntityProperties(resource, entryWrapper, entityType, readContext);
                return resource;
            }
        }

        /// <summary>
        /// Creates a new instance of the backing CLR object for the given entity type.
        /// </summary>
        /// <param name="entityType">The EDM type of the entity to create.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The created CLR object.</returns>
        public virtual object CreateEntityResource(IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }
            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }

            IEdmModel model = readContext.Model;
            if (model == null)
            {
                throw Error.Argument("readContext", SRResources.ModelMissingFromReadContext);
            }

            if (readContext.IsUntyped)
            {
                return new EdmEntityObject(entityType);
            }
            else
            {
                Type clrType = EdmLibHelpers.GetClrType(entityType, model);
                if (clrType == null)
                {
                    throw new ODataException(
                        Error.Format(SRResources.MappingDoesNotContainEntityType, entityType.FullName()));
                }

                if (readContext.IsDeltaOfT)
                {
                    IEnumerable<string> structuralProperties = entityType.StructuralProperties()
                        .Select(edmProperty => EdmLibHelpers.GetClrPropertyName(edmProperty, model));

                    if (entityType.IsOpen())
                    {
                        PropertyInfo dynamicDictionaryPropertyInfo = EdmLibHelpers.GetDynamicPropertyDictionary(
                            entityType.StructuredDefinition(), model);

                        return Activator.CreateInstance(readContext.ResourceType, clrType, structuralProperties,
                            dynamicDictionaryPropertyInfo);
                    }
                    else
                    {
                        return Activator.CreateInstance(readContext.ResourceType, clrType, structuralProperties);
                    }
                }
                else
                {
                    return Activator.CreateInstance(clrType);
                }
            }
        }

        /// <summary>
        /// Deserializes the navigation properties from <paramref name="entryWrapper"/> into <paramref name="entityResource"/>.
        /// </summary>
        /// <param name="entityResource">The object into which the navigation properties should be read.</param>
        /// <param name="entryWrapper">The entry object containing the navigation properties.</param>
        /// <param name="entityType">The entity type of the entity resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyNavigationProperties(object entityResource, ODataEntryWithNavigationLinks entryWrapper,
            IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
        {
            if (entryWrapper == null)
            {
                throw Error.ArgumentNull("entryWrapper");
            }

            foreach (ODataNavigationLinkWithItems navigationLink in entryWrapper.NavigationLinks)
            {
                ApplyNavigationProperty(entityResource, navigationLink, entityType, readContext);
            }
        }

        /// <summary>
        /// Deserializes the navigation property from <paramref name="navigationLinkWrapper"/> into <paramref name="entityResource"/>.
        /// </summary>
        /// <param name="entityResource">The object into which the navigation property should be read.</param>
        /// <param name="navigationLinkWrapper">The navigation link.</param>
        /// <param name="entityType">The entity type of the entity resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyNavigationProperty(object entityResource, ODataNavigationLinkWithItems navigationLinkWrapper,
             IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
        {
            if (navigationLinkWrapper == null)
            {
                throw Error.ArgumentNull("navigationLinkWrapper");
            }

            if (entityResource == null)
            {
                throw Error.ArgumentNull("entityResource");
            }

            IEdmNavigationProperty navigationProperty = entityType.FindProperty(navigationLinkWrapper.NavigationLink.Name) as IEdmNavigationProperty;
            if (navigationProperty == null)
            {
                throw new ODataException(
                    Error.Format(SRResources.NavigationPropertyNotfound, navigationLinkWrapper.NavigationLink.Name, entityType.FullName()));
            }

            foreach (ODataItemBase childItem in navigationLinkWrapper.NestedItems)
            {
                ODataEntityReferenceLinkBase entityReferenceLink = childItem as ODataEntityReferenceLinkBase;
                if (entityReferenceLink != null)
                {
                    // ignore links.
                    continue;
                }

                ODataFeedWithEntries feed = childItem as ODataFeedWithEntries;
                if (feed != null)
                {
                    ApplyFeedInNavigationProperty(navigationProperty, entityResource, feed, readContext);
                    continue;
                }

                // It must be entry by now.
                ODataEntryWithNavigationLinks entry = (ODataEntryWithNavigationLinks)childItem;
                if (entry != null)
                {
                    ApplyEntryInNavigationProperty(navigationProperty, entityResource, entry, readContext);
                }
            }
        }

        /// <summary>
        /// Deserializes the structural properties from <paramref name="entryWrapper"/> into <paramref name="entityResource"/>.
        /// </summary>
        /// <param name="entityResource">The object into which the structural properties should be read.</param>
        /// <param name="entryWrapper">The entry object containing the structural properties.</param>
        /// <param name="entityType">The entity type of the entity resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyStructuralProperties(object entityResource, ODataEntryWithNavigationLinks entryWrapper,
            IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
        {
            if (entryWrapper == null)
            {
                throw Error.ArgumentNull("entryWrapper");
            }

            foreach (ODataProperty property in entryWrapper.Entry.Properties)
            {
                ApplyStructuralProperty(entityResource, property, entityType, readContext);
            }
        }

        /// <summary>
        /// Deserializes the given <paramref name="structuralProperty"/> into <paramref name="entityResource"/>.
        /// </summary>
        /// <param name="entityResource">The object into which the structural property should be read.</param>
        /// <param name="structuralProperty">The entry object containing the structural properties.</param>
        /// <param name="entityType">The entity type of the entity resource.</param>
        /// <param name="readContext">The deserializer context.</param>
        public virtual void ApplyStructuralProperty(object entityResource, ODataProperty structuralProperty,
            IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
        {
            if (entityResource == null)
            {
                throw Error.ArgumentNull("entityResource");
            }

            if (structuralProperty == null)
            {
                throw Error.ArgumentNull("structuralProperty");
            }

            DeserializationHelpers.ApplyProperty(structuralProperty, entityType, entityResource, DeserializerProvider, readContext);
        }

        /// <summary>
        /// Reads an ODataFeed or an ODataItem from the reader.
        /// </summary>
        /// <param name="reader">The OData reader to read from.</param>
        /// <returns>The read feed or entry.</returns>
        public static ODataItemBase ReadEntryOrFeed(ODataReader reader)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("odataReader");
            }

            ODataItemBase topLevelItem = null;
            Stack<ODataItemBase> itemsStack = new Stack<ODataItemBase>();

            while (reader.Read())
            {
                switch (reader.State)
                {
                    case ODataReaderState.EntryStart:
                        ODataEntry entry = (ODataEntry)reader.Item;
                        ODataEntryWithNavigationLinks entryWrapper = null;
                        if (entry != null)
                        {
                            entryWrapper = new ODataEntryWithNavigationLinks(entry);
                        }

                        if (itemsStack.Count == 0)
                        {
                            Contract.Assert(entry != null, "The top-level entry can never be null.");
                            topLevelItem = entryWrapper;
                        }
                        else
                        {
                            ODataItemBase parentItem = itemsStack.Peek();
                            ODataFeedWithEntries parentFeed = parentItem as ODataFeedWithEntries;
                            if (parentFeed != null)
                            {
                                parentFeed.Entries.Add(entryWrapper);
                            }
                            else
                            {
                                ODataNavigationLinkWithItems parentNavigationLink = (ODataNavigationLinkWithItems)parentItem;
                                Contract.Assert(parentNavigationLink.NavigationLink.IsCollection == false, "Only singleton navigation properties can contain entry as their child.");
                                Contract.Assert(parentNavigationLink.NestedItems.Count == 0, "Each navigation property can contain only one entry as its direct child.");
                                parentNavigationLink.NestedItems.Add(entryWrapper);
                            }
                        }
                        itemsStack.Push(entryWrapper);
                        break;

                    case ODataReaderState.EntryEnd:
                        Contract.Assert(itemsStack.Count > 0 && (reader.Item == null || itemsStack.Peek().Item == reader.Item), "The entry which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
                        break;

                    case ODataReaderState.NavigationLinkStart:
                        ODataNavigationLink navigationLink = (ODataNavigationLink)reader.Item;
                        Contract.Assert(navigationLink != null, "Navigation link should never be null.");

                        ODataNavigationLinkWithItems navigationLinkWrapper = new ODataNavigationLinkWithItems(navigationLink);
                        Contract.Assert(itemsStack.Count > 0, "Navigation link can't appear as top-level item.");
                        {
                            ODataEntryWithNavigationLinks parentEntry = (ODataEntryWithNavigationLinks)itemsStack.Peek();
                            parentEntry.NavigationLinks.Add(navigationLinkWrapper);
                        }

                        itemsStack.Push(navigationLinkWrapper);
                        break;

                    case ODataReaderState.NavigationLinkEnd:
                        Contract.Assert(itemsStack.Count > 0 && itemsStack.Peek().Item == reader.Item, "The navigation link which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
                        break;

                    case ODataReaderState.FeedStart:
                        ODataFeed feed = (ODataFeed)reader.Item;
                        Contract.Assert(feed != null, "Feed should never be null.");

                        ODataFeedWithEntries feedWrapper = new ODataFeedWithEntries(feed);
                        if (itemsStack.Count > 0)
                        {
                            ODataNavigationLinkWithItems parentNavigationLink = (ODataNavigationLinkWithItems)itemsStack.Peek();
                            Contract.Assert(parentNavigationLink != null, "this has to be an inner feed. inner feeds always have a navigation link.");
                            Contract.Assert(parentNavigationLink.NavigationLink.IsCollection == true, "Only collection navigation properties can contain feed as their child.");
                            parentNavigationLink.NestedItems.Add(feedWrapper);
                        }
                        else
                        {
                            topLevelItem = feedWrapper;
                        }

                        itemsStack.Push(feedWrapper);
                        break;

                    case ODataReaderState.FeedEnd:
                        Contract.Assert(itemsStack.Count > 0 && itemsStack.Peek().Item == reader.Item, "The feed which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
                        break;

                    case ODataReaderState.EntityReferenceLink:
                        ODataEntityReferenceLink entityReferenceLink = (ODataEntityReferenceLink)reader.Item;
                        Contract.Assert(entityReferenceLink != null, "Entity reference link should never be null.");
                        ODataEntityReferenceLinkBase entityReferenceLinkWrapper = new ODataEntityReferenceLinkBase(entityReferenceLink);

                        Contract.Assert(itemsStack.Count > 0, "Entity reference link should never be reported as top-level item.");
                        {
                            ODataNavigationLinkWithItems parentNavigationLink = (ODataNavigationLinkWithItems)itemsStack.Peek();
                            parentNavigationLink.NestedItems.Add(entityReferenceLinkWrapper);
                        }

                        break;

                    default:
                        Contract.Assert(false, "We should never get here, it means the ODataReader reported a wrong state.");
                        break;
                }
            }

            Contract.Assert(reader.State == ODataReaderState.Completed, "We should have consumed all of the input by now.");
            Contract.Assert(topLevelItem != null, "A top level entry or feed should have been read by now.");
            return topLevelItem;
        }

        private void ApplyEntityProperties(object entityResource, ODataEntryWithNavigationLinks entryWrapper,
            IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
        {
            ApplyStructuralProperties(entityResource, entryWrapper, entityType, readContext);
            ApplyNavigationProperties(entityResource, entryWrapper, entityType, readContext);
        }

        private void ApplyEntryInNavigationProperty(IEdmNavigationProperty navigationProperty, object entityResource,
            ODataEntryWithNavigationLinks entry, ODataDeserializerContext readContext)
        {
            Contract.Assert(navigationProperty != null && navigationProperty.PropertyKind == EdmPropertyKind.Navigation, "navigationProperty != null && navigationProperty.TypeKind == ResourceTypeKind.EntityType");
            Contract.Assert(entityResource != null, "entityResource != null");

            if (readContext.IsDeltaOfT)
            {
                string message = Error.Format(SRResources.CannotPatchNavigationProperties, navigationProperty.Name, navigationProperty.DeclaringEntityType().FullName());
                throw new ODataException(message);
            }

            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(navigationProperty.Type);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized, navigationProperty.Type.FullName(), typeof(ODataMediaTypeFormatter)));
            }
            object value = deserializer.ReadInline(entry, navigationProperty.Type, readContext);

            string propertyName = EdmLibHelpers.GetClrPropertyName(navigationProperty, readContext.Model);
            DeserializationHelpers.SetProperty(entityResource, propertyName, value);
        }

        private void ApplyFeedInNavigationProperty(IEdmNavigationProperty navigationProperty, object entityResource, ODataFeedWithEntries feed, ODataDeserializerContext readContext)
        {
            Contract.Assert(navigationProperty != null && navigationProperty.PropertyKind == EdmPropertyKind.Navigation, "navigationProperty != null && navigationProperty.TypeKind == ResourceTypeKind.EntityType");
            Contract.Assert(entityResource != null, "entityResource != null");

            if (readContext.IsDeltaOfT)
            {
                string message = Error.Format(SRResources.CannotPatchNavigationProperties, navigationProperty.Name, navigationProperty.DeclaringEntityType().FullName());
                throw new ODataException(message);
            }

            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(navigationProperty.Type);
            if (deserializer == null)
            {
                throw new SerializationException(Error.Format(SRResources.TypeCannotBeDeserialized, navigationProperty.Type.FullName(), typeof(ODataMediaTypeFormatter)));
            }
            object value = deserializer.ReadInline(feed, navigationProperty.Type, readContext);

            string propertyName = EdmLibHelpers.GetClrPropertyName(navigationProperty, readContext.Model);
            DeserializationHelpers.SetCollectionProperty(entityResource, navigationProperty, value, propertyName);
        }
    }
}
