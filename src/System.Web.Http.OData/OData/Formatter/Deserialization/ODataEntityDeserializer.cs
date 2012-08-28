// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class ODataEntityDeserializer : ODataEntryDeserializer
    {
        protected const int RecursionLimit = 100;

        public ODataEntityDeserializer(IEdmEntityTypeReference edmEntityType, ODataDeserializerProvider deserializerProvider)
            : base(edmEntityType, ODataPayloadKind.Entry, deserializerProvider)
        {
            EdmEntityType = edmEntityType;
        }

        public IEdmEntityTypeReference EdmEntityType { get; private set; }

        public override object Read(ODataMessageReader messageReader, ODataDeserializerReadContext readContext)
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
            ODataEntry topLevelEntry = ReadEntry(odataReader, EdmEntityType, readContext);

            return ReadInline(topLevelEntry, readContext);
        }

        public override object ReadInline(object item, ODataDeserializerReadContext readContext)
        {
            ODataEntry topLevelEntry = item as ODataEntry;
            if (item == null)
            {
                throw Error.Argument("item", SRResources.ItemMustBeOfType, typeof(ODataEntry).Name);
            }

            ODataEntryAnnotation topLevelEntryAnnotation = topLevelEntry.GetAnnotation<ODataEntryAnnotation>();
            Contract.Assert(topLevelEntryAnnotation != null);

            RecurseEnter(readContext);
            ApplyEntityProperties(topLevelEntry, topLevelEntryAnnotation, readContext);
            RecurseLeave(readContext);

            return topLevelEntryAnnotation.EntityResource;
        }

        private ODataEntry ReadEntry(ODataReader odataReader, IEdmEntityTypeReference entityType, ODataDeserializerReadContext readContext)
        {
            ODataEntry topLevelEntry = null;
            Stack<ODataItem> itemsStack = new Stack<ODataItem>();

            while (odataReader.Read())
            {
                if (itemsStack.Count >= RecursionLimit)
                {
                    throw Error.InvalidOperation(SRResources.RecursionLimitExceeded);
                }

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
                            topLevelEntry = entry;

                            CreateEntityResource(entryAnnotation, entityType, readContext);
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
                        break;

                    case ODataReaderState.EntryEnd:
                        Contract.Assert(itemsStack.Count > 0 && itemsStack.Peek() == odataReader.Item, "The entry which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
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
                        break;

                    case ODataReaderState.NavigationLinkEnd:
                        Contract.Assert(itemsStack.Count > 0 && itemsStack.Peek() == odataReader.Item, "The navigation link which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
                        break;

                    case ODataReaderState.FeedStart:
                        ODataFeed feed = (ODataFeed)odataReader.Item;
                        Contract.Assert(feed != null, "Feed should never be null.");

                        feed.SetAnnotation(new ODataFeedAnnotation());
                        Contract.Assert(itemsStack.Count > 0, "Since we always start reading entry, we should never get a feed as the top-level item.");
                        {
                            ODataNavigationLink parentNavigationLink = (ODataNavigationLink)itemsStack.Peek();
                            ODataNavigationLinkAnnotation parentNavigationLinkAnnotation = parentNavigationLink.GetAnnotation<ODataNavigationLinkAnnotation>();
                            Contract.Assert(parentNavigationLinkAnnotation != null, "Every navigation link we added to the stack should have the navigation link annotation on it.");

                            Contract.Assert(parentNavigationLink.IsCollection == true, "Only collection navigation properties can contain feed as their child.");
                            parentNavigationLinkAnnotation.Add(feed);
                        }

                        itemsStack.Push(feed);
                        break;

                    case ODataReaderState.FeedEnd:
                        Contract.Assert(itemsStack.Count > 0 && itemsStack.Peek() == odataReader.Item, "The feed which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
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
            Contract.Assert(topLevelEntry != null, "A top level entry should have been read by now.");
            return topLevelEntry;
        }

        private void CreateEntityResource(ODataEntryAnnotation entryAnnotation, IEdmEntityTypeReference entityType, ODataDeserializerReadContext readContext)
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

        private void ApplyEntityProperties(ODataEntry entry, ODataEntryAnnotation entryAnnotation, ODataDeserializerReadContext readContext)
        {
            object entityResource = entryAnnotation.EntityResource;
            IEdmEntityTypeReference entityType = entryAnnotation.EntityType;

            ApplyValueProperties(entry, entityType, entityResource, readContext);
            ApplyNavigationProperties(entryAnnotation, entityType, entityResource, readContext);
        }

        private void ApplyNavigationProperties(ODataEntryAnnotation entryAnnotation, IEdmEntityTypeReference entityType, object entityResource, ODataDeserializerReadContext readContext)
        {
            Contract.Assert(entityType.TypeKind() == EdmTypeKind.Entity, "Only entity types can be specified for entities.");

            foreach (ODataNavigationLink navigationLink in entryAnnotation)
            {
                IEdmNavigationProperty navigationProperty = entityType.FindProperty(navigationLink.Name) as IEdmNavigationProperty;
                Contract.Assert(navigationProperty != null, "ODataLib reader should have already validated that all navigation properties are declared and none is open.");

                ApplyNavigationProperty(navigationLink, navigationProperty, entityResource, readContext);
            }
        }

        private void ApplyNavigationProperty(ODataNavigationLink navigationLink, IEdmNavigationProperty navigationProperty, object entityResource, ODataDeserializerReadContext readContext)
        {
            ODataNavigationLinkAnnotation navigationLinkAnnotation = navigationLink.GetAnnotation<ODataNavigationLinkAnnotation>();
            Contract.Assert(navigationLinkAnnotation != null, "navigationLinkAnnotation != null");
            // Contract.Assert(navigationLinkAnnotation.Count > 0, "Each navigation link must have at least one child in request.");
            Contract.Assert(navigationLink.IsCollection.HasValue, "We should know the cardinality of the navigation link by now.");

            foreach (ODataItem childItem in navigationLinkAnnotation)
            {
                ODataEntityReferenceLink entityReferenceLink = childItem as ODataEntityReferenceLink;
                if (entityReferenceLink != null)
                {
                    ApplyEntityReferenceLinkInNavigationProperty(navigationProperty, entityResource, entityReferenceLink);
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
                ApplyEntryInNavigationProperty(navigationProperty, entityResource, entry, readContext);
            }
        }

        private void ApplyEntryInNavigationProperty(IEdmNavigationProperty navigationProperty, object entityResource, ODataEntry entry, ODataDeserializerReadContext readContext)
        {
            Contract.Assert(navigationProperty != null && navigationProperty.PropertyKind == EdmPropertyKind.Navigation, "navigationProperty != null && navigationProperty.TypeKind == ResourceTypeKind.EntityType");
            Contract.Assert(entityResource != null, "entityResource != null");

            if (entry == null)
            {
                SetResourceReferenceToNull(entityResource, navigationProperty);
            }
            else
            {
                PropertyInfo clrProperty = entityResource.GetType().GetProperty(navigationProperty.Name);

                IEdmEntityTypeReference elementType = navigationProperty.Type.AsEntity();
                object childEntityResource = CreateNestedEntityAndApplyProperties(entry, elementType, readContext);

                clrProperty.SetValue(entityResource, childEntityResource, index: null);
            }
        }

        private void ApplyFeedInNavigationProperty(IEdmNavigationProperty navigationProperty, object entityResource, ODataFeed feed, ODataDeserializerReadContext readContext)
        {
            ODataFeedAnnotation feedAnnotation = feed.GetAnnotation<ODataFeedAnnotation>();
            Contract.Assert(feedAnnotation != null, "Each feed we create should gave annotation on it.");

            IEdmEntityTypeReference elementType = ToEntityType(navigationProperty);
            foreach (ODataEntry entryInFeed in feedAnnotation)
            {
                object childEntityResource = CreateNestedEntityAndApplyProperties(entryInFeed, elementType, readContext);
                AddReferenceToCollection(entityResource, navigationProperty, childEntityResource);
            }
        }

        private static IEdmEntityTypeReference ToEntityType(IEdmNavigationProperty navigationProperty)
        {
            IEdmTypeReference target = navigationProperty.Type;
            if (target.TypeKind() == EdmTypeKind.Collection)
            {
                target = ((IEdmCollectionTypeReference)target).ElementType();
            }

            if (target.TypeKind() == EdmTypeKind.EntityReference)
            {
                target = new EdmEntityTypeReference(((IEdmEntityReferenceType)target).EntityType, isNullable: true);
            }

            return target.AsEntity();
        }

        private void AddReferenceToCollection(object entityResource, IEdmNavigationProperty navigationProperty, object childEntityResource)
        {
            PropertyInfo clrProperty = entityResource.GetType().GetProperty(navigationProperty.Name);
            IList list = clrProperty.GetValue(entityResource, index: null) as IList;
            if (list == null)
            {
                Type elementType = EdmLibHelpers.GetClrType(new EdmEntityTypeReference(navigationProperty.ToEntityType(), isNullable: true), EdmModel);
                list = Activator.CreateInstance(typeof(Collection<>).MakeGenericType(elementType)) as IList;
                clrProperty.SetValue(entityResource, list, index: null);
            }

            list.Add(childEntityResource);
        }

        private object CreateNestedEntityAndApplyProperties(ODataEntry entry, IEdmEntityTypeReference elementType, ODataDeserializerReadContext readContext)
        {
            ODataEntryAnnotation annotation = entry.GetAnnotation<ODataEntryAnnotation>();
            Contract.Assert(annotation != null);

            CreateEntityResource(annotation, elementType, readContext);

            ODataEntryDeserializer deserializer = DeserializerProvider.GetODataDeserializer(elementType);
            return deserializer.ReadInline(entry, readContext);
        }

        private void ApplyEntityReferenceLinkInNavigationProperty(IEdmNavigationProperty navigationProperty, object entityResource, ODataEntityReferenceLink entityReferenceLink)
        {
            if (entityReferenceLink.Url == null)
            {
                return;
            }

            string linkUrl = entityReferenceLink.Url.AbsoluteUri;

            // TODO: check for Atom
            if (true && linkUrl.Length == 0)
            {
                // Empty Url for atom:link (without any content in the link) is treated as null entity.
                SetResourceReferenceToNull(entityResource, navigationProperty);
            }
            else
            {
                // Resolve the link URL and set it to the navigation property.
                SetResourceReferenceToUrl(entityResource, navigationProperty, linkUrl);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "TODO: remove this when implement")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "entityResource", Justification = "TODO: remove this when implement")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "navigationProperty", Justification = "TODO: remove this when implement")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "linkUrl", Justification = "TODO: remove this when implement")]
        private void SetResourceReferenceToUrl(object entityResource, IEdmNavigationProperty navigationProperty, string linkUrl)
        {
            // throw new NotImplementedException();
            // Ignore Navigation Links for now
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "TODO: remove this when implement")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "entityResource", Justification = "TODO: remove this when implement")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "navigationProperty", Justification = "TODO: remove this when implement")]
        private void SetResourceReferenceToNull(object entityResource, IEdmNavigationProperty navigationProperty)
        {
            // throw new NotImplementedException();
            // Ignore Navigation Links for now
        }

        private void ApplyValueProperties(ODataEntry entry, IEdmStructuredTypeReference entityType, object entityResource, ODataDeserializerReadContext readContext)
        {
            foreach (ODataProperty property in entry.Properties)
            {
                ApplyProperty(property, entityType, entityResource, DeserializerProvider, readContext);
            }
        }

        private sealed class ODataEntryAnnotation : List<ODataNavigationLink>
        {
            /// <summary>The entity resource update token for the entry.</summary>
            internal object EntityResource { get; set; }

            /// <summary>The resolved entity type for the entry.</summary>
            internal IEdmEntityTypeReference EntityType { get; set; }
        }

        /// <summary>
        /// The annotation used on ODataFeed instances to store the list of entries in that feed.
        /// </summary>
        private sealed class ODataFeedAnnotation : List<ODataEntry>
        {
        }

        /// <summary>
        /// The annotation used on ODataNavigationLink instances to store the list of children for that navigation link.
        /// </summary>
        /// <remarks>
        /// A navigation link for a singleton navigation property can only contain one item - either ODataEntry or ODataEntityReferenceLink.
        /// A navigation link for a collection navigation property can contain any number of items - each is either ODataFeed or ODataEntityReferenceLink.
        /// </remarks>
        private sealed class ODataNavigationLinkAnnotation : List<ODataItem>
        {
        }
    }
}
