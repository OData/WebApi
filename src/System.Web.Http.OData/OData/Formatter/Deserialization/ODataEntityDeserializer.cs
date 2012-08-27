// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class ODataEntityDeserializer : ODataEntryDeserializer<ODataEntry>
    {
        protected const int RecursionLimit = 100;

        public ODataEntityDeserializer(IEdmEntityTypeReference edmEntityType, ODataDeserializerProvider deserializerProvider)
            : base(edmEntityType, ODataPayloadKind.Entry, deserializerProvider)
        {
            EdmEntityType = edmEntityType;
        }

        public IEdmEntityTypeReference EdmEntityType { get; private set; }

        public override object Read(ODataMessageReader messageReader, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            ODataReader odataReader = messageReader.CreateODataEntryReader(EdmEntityType.EntityDefinition());
            ODataEntry topLevelEntry = ReadEntryOrFeed(odataReader, readContext) as ODataEntry;
            Contract.Assert(topLevelEntry != null);

            return ReadInline(topLevelEntry, readContext);
        }

        public override object ReadInline(ODataEntry entry, ODataDeserializerContext readContext)
        {
            if (entry == null)
            {
                throw Error.Argument("entry", SRResources.ItemMustBeOfType, typeof(ODataEntry).Name);
            }

            if (EdmEntityType.FullName() != entry.TypeName)
            {
                // received a derived type in a base type deserializer.
                // delegate it to the appropriate derived type deserializer.
                IEdmEntityType entityType = EdmModel.FindType(entry.TypeName) as IEdmEntityType;
                Contract.Assert(entityType != null, "edmlib should have already validated that it knows the edm type and is the same as or derives from EdmEntityType");

                if (entityType.IsAbstract)
                {
                    throw Error.InvalidOperation(SRResources.CannotInstantiateAbstractEntityType, entry.TypeName);
                }

                ODataEntityDeserializer deserializer = DeserializerProvider.GetODataDeserializer(new EdmEntityTypeReference(entityType, isNullable: false)) as ODataEntityDeserializer;
                return deserializer.ReadInline(entry, readContext);
            }
            else
            {
                ODataEntryAnnotation entryAnnotation = entry.GetAnnotation<ODataEntryAnnotation>();
                Contract.Assert(entryAnnotation != null);

                CreateEntityResource(entryAnnotation, EdmEntityType, readContext);

                RecurseEnter(readContext);
                ApplyEntityProperties(entry, entryAnnotation, readContext);
                RecurseLeave(readContext);

                return entryAnnotation.EntityResource;
            }
        }

        internal static ODataItem ReadEntryOrFeed(ODataReader odataReader, ODataDeserializerContext readContext)
        {
            ODataItem topLevelItem = null;
            Stack<ODataItem> itemsStack = new Stack<ODataItem>();

            while (odataReader.Read())
            {
                switch (odataReader.State)
                {
                    case ODataReaderState.EntryStart:
                        ODataEntry entry = (ODataEntry)odataReader.Item;
                        ODataEntryAnnotation entryAnnotation = null;
                        if (entry != null)
                        {
                            entryAnnotation = new ODataEntryAnnotation();
                            entry.SetAnnotation(entryAnnotation);
                        }

                        if (itemsStack.Count == 0)
                        {
                            Contract.Assert(entry != null, "The top-level entry can never be null.");
                            topLevelItem = entry;
                        }
                        else
                        {
                            ODataItem parentItem = itemsStack.Peek();
                            ODataFeed parentFeed = parentItem as ODataFeed;
                            if (parentFeed != null)
                            {
                                ODataFeedAnnotation parentFeedAnnotation = parentFeed.GetAnnotation<ODataFeedAnnotation>();
                                Contract.Assert(parentFeedAnnotation != null, "Every feed we added to the stack should have the feed annotation on it.");
                                parentFeedAnnotation.Add(entry);
                            }
                            else
                            {
                                ODataNavigationLink parentNavigationLink = (ODataNavigationLink)parentItem;
                                ODataNavigationLinkAnnotation parentNavigationLinkAnnotation = parentNavigationLink.GetAnnotation<ODataNavigationLinkAnnotation>();
                                Contract.Assert(parentNavigationLinkAnnotation != null, "Every navigation link we added to the stack should have the navigation link annotation on it.");

                                Contract.Assert(parentNavigationLink.IsCollection == false, "Only singleton navigation properties can contain entry as their child.");
                                Contract.Assert(parentNavigationLinkAnnotation.Count == 0, "Each navigation property can contain only one entry as its direct child.");
                                parentNavigationLinkAnnotation.Add(entry);
                            }
                        }
                        itemsStack.Push(entry);
                        RecurseEnter(readContext);
                        break;

                    case ODataReaderState.EntryEnd:
                        Contract.Assert(itemsStack.Count > 0 && itemsStack.Peek() == odataReader.Item, "The entry which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
                        RecurseLeave(readContext);
                        break;

                    case ODataReaderState.NavigationLinkStart:
                        ODataNavigationLink navigationLink = (ODataNavigationLink)odataReader.Item;
                        Contract.Assert(navigationLink != null, "Navigation link should never be null.");

                        navigationLink.SetAnnotation(new ODataNavigationLinkAnnotation());
                        Contract.Assert(itemsStack.Count > 0, "Navigation link can't appear as top-level item.");
                        {
                            ODataEntry parentEntry = (ODataEntry)itemsStack.Peek();
                            ODataEntryAnnotation parentEntryAnnotation = parentEntry.GetAnnotation<ODataEntryAnnotation>();
                            Contract.Assert(parentEntryAnnotation != null, "Every entry we added to the stack should have the entry annotation on it.");
                            parentEntryAnnotation.Add(navigationLink);
                        }

                        itemsStack.Push(navigationLink);
                        RecurseEnter(readContext);
                        break;

                    case ODataReaderState.NavigationLinkEnd:
                        Contract.Assert(itemsStack.Count > 0 && itemsStack.Peek() == odataReader.Item, "The navigation link which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
                        RecurseLeave(readContext);
                        break;

                    case ODataReaderState.FeedStart:
                        ODataFeed feed = (ODataFeed)odataReader.Item;
                        Contract.Assert(feed != null, "Feed should never be null.");

                        feed.SetAnnotation(new ODataFeedAnnotation());
                        if (itemsStack.Count > 0)
                        {
                            ODataNavigationLink parentNavigationLink = (ODataNavigationLink)itemsStack.Peek();
                            Contract.Assert(parentNavigationLink != null, "this has to be an inner feed. inner feeds always have a navigation link.");
                            ODataNavigationLinkAnnotation parentNavigationLinkAnnotation = parentNavigationLink.GetAnnotation<ODataNavigationLinkAnnotation>();
                            Contract.Assert(parentNavigationLinkAnnotation != null, "Every navigation link we added to the stack should have the navigation link annotation on it.");

                            Contract.Assert(parentNavigationLink.IsCollection == true, "Only collection navigation properties can contain feed as their child.");
                            parentNavigationLinkAnnotation.Add(feed);
                        }
                        else
                        {
                            topLevelItem = feed;
                        }

                        itemsStack.Push(feed);
                        RecurseEnter(readContext);
                        break;

                    case ODataReaderState.FeedEnd:
                        Contract.Assert(itemsStack.Count > 0 && itemsStack.Peek() == odataReader.Item, "The feed which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
                        RecurseLeave(readContext);
                        break;

                    case ODataReaderState.EntityReferenceLink:
                        ODataEntityReferenceLink entityReferenceLink = (ODataEntityReferenceLink)odataReader.Item;
                        Contract.Assert(entityReferenceLink != null, "Entity reference link should never be null.");

                        Contract.Assert(itemsStack.Count > 0, "Entity reference link should never be reported as top-level item.");
                        {
                            ODataNavigationLink parentNavigationLink = (ODataNavigationLink)itemsStack.Peek();
                            ODataNavigationLinkAnnotation parentNavigationLinkAnnotation = parentNavigationLink.GetAnnotation<ODataNavigationLinkAnnotation>();
                            Contract.Assert(parentNavigationLinkAnnotation != null, "Every navigation link we added to the stack should have the navigation link annotation on it.");

                            parentNavigationLinkAnnotation.Add(entityReferenceLink);
                        }

                        break;

                    default:
                        Contract.Assert(false, "We should never get here, it means the ODataReader reported a wrong state.");
                        break;
                }
            }

            Contract.Assert(odataReader.State == ODataReaderState.Completed, "We should have consumed all of the input by now.");
            Contract.Assert(topLevelItem != null, "A top level entry or feed should have been read by now.");
            return topLevelItem;
        }

        private void CreateEntityResource(ODataEntryAnnotation entryAnnotation, IEdmEntityTypeReference entityType, ODataDeserializerContext readContext)
        {
            Type clrType = EdmLibHelpers.GetClrType(entityType, EdmModel);
            if (clrType == null)
            {
                throw Error.Argument("entityType", SRResources.MappingDoesNotContainEntityType, entityType.FullName());
            }

            object resource;

            if (!readContext.IsPatchMode)
            {
                resource = Activator.CreateInstance(clrType);
            }
            else
            {
                resource = Activator.CreateInstance(typeof(Delta<>).MakeGenericType(clrType));
            }

            entryAnnotation.EntityResource = resource;
            entryAnnotation.EntityType = entityType;
        }

        private void ApplyEntityProperties(ODataEntry entry, ODataEntryAnnotation entryAnnotation, ODataDeserializerContext readContext)
        {
            object entityResource = entryAnnotation.EntityResource;
            IEdmEntityTypeReference entityType = entryAnnotation.EntityType;

            ApplyValueProperties(entry, entityType, entityResource, readContext);
            ApplyNavigationProperties(entryAnnotation, entityType, entityResource, readContext);
        }

        private void ApplyNavigationProperties(ODataEntryAnnotation entryAnnotation, IEdmEntityTypeReference entityType, object entityResource, ODataDeserializerContext readContext)
        {
            Contract.Assert(entityType.TypeKind() == EdmTypeKind.Entity, "Only entity types can be specified for entities.");

            foreach (ODataNavigationLink navigationLink in entryAnnotation)
            {
                IEdmNavigationProperty navigationProperty = entityType.FindProperty(navigationLink.Name) as IEdmNavigationProperty;
                Contract.Assert(navigationProperty != null, "ODataLib reader should have already validated that all navigation properties are declared and none is open.");

                ApplyNavigationProperty(navigationLink, navigationProperty, entityResource, readContext);
            }
        }

        private void ApplyNavigationProperty(ODataNavigationLink navigationLink, IEdmNavigationProperty navigationProperty, object entityResource, ODataDeserializerContext readContext)
        {
            ODataNavigationLinkAnnotation navigationLinkAnnotation = navigationLink.GetAnnotation<ODataNavigationLinkAnnotation>();
            Contract.Assert(navigationLinkAnnotation != null, "navigationLinkAnnotation != null");
            Contract.Assert(navigationLink.IsCollection.HasValue, "We should know the cardinality of the navigation link by now.");

            foreach (ODataItem childItem in navigationLinkAnnotation)
            {
                ODataEntityReferenceLink entityReferenceLink = childItem as ODataEntityReferenceLink;
                if (entityReferenceLink != null)
                {
                    // ignore links.
                    continue;
                }

                ODataFeed feed = childItem as ODataFeed;
                if (feed != null)
                {
                    ApplyFeedInNavigationProperty(navigationProperty, entityResource, feed, readContext);
                    continue;
                }

                // It must be entry by now.
                ODataEntry entry = (ODataEntry)childItem;
                if (entry != null)
                {
                    ApplyEntryInNavigationProperty(navigationProperty, entityResource, entry, readContext);
                }
            }
        }

        private void ApplyEntryInNavigationProperty(IEdmNavigationProperty navigationProperty, object entityResource, ODataEntry entry, ODataDeserializerContext readContext)
        {
            Contract.Assert(navigationProperty != null && navigationProperty.PropertyKind == EdmPropertyKind.Navigation, "navigationProperty != null && navigationProperty.TypeKind == ResourceTypeKind.EntityType");
            Contract.Assert(entityResource != null, "entityResource != null");

            ODataEntryDeserializer deserializer = DeserializerProvider.GetODataDeserializer(navigationProperty.Type);
            object value = deserializer.ReadInline(entry, readContext);

            if (readContext.IsPatchMode)
            {
                throw Error.InvalidOperation(SRResources.CannotPatchNavigationProperties, navigationProperty.Name, navigationProperty.DeclaringEntityType().FullName());
            }

            SetProperty(entityResource, navigationProperty.Name, isDelta: false, value: value);
        }

        private void ApplyFeedInNavigationProperty(IEdmNavigationProperty navigationProperty, object entityResource, ODataFeed feed, ODataDeserializerContext readContext)
        {
            ODataFeedAnnotation feedAnnotation = feed.GetAnnotation<ODataFeedAnnotation>();
            Contract.Assert(feedAnnotation != null, "Each feed we create should gave annotation on it.");

            ODataEntryDeserializer deserializer = DeserializerProvider.GetODataDeserializer(navigationProperty.Type);
            object value = deserializer.ReadInline(feed, readContext);

            if (readContext.IsPatchMode)
            {
                throw Error.InvalidOperation(SRResources.CannotPatchNavigationProperties, navigationProperty.Name, navigationProperty.DeclaringEntityType().FullName());
            }

            SetCollectionProperty(entityResource, navigationProperty.Name, isDelta: false, value: value);
        }

        private void ApplyValueProperties(ODataEntry entry, IEdmStructuredTypeReference entityType, object entityResource, ODataDeserializerContext readContext)
        {
            foreach (ODataProperty property in entry.Properties)
            {
                ApplyProperty(property, entityType, entityResource, DeserializerProvider, readContext);
            }
        }
    }
}
