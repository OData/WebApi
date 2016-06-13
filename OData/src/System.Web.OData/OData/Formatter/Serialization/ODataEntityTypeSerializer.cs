// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
using System.Web.OData.Query.Expressions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing instances of <see cref="IEdmEntityType"/>
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
    public class ODataEntityTypeSerializer : ODataEdmTypeSerializer
    {
        private const string Entry = "entry";

        /// <inheritdoc />
        public ODataEntityTypeSerializer(ODataSerializerProvider serializerProvider)
            : base(ODataPayloadKind.Resource, serializerProvider)
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

            var path = writeContext.Path;
            if (path == null)
            {
                throw new SerializationException(SRResources.ODataPathMissing);
            }

            ODataWriter writer = messageWriter.CreateODataResourceWriter(navigationSource, path.EdmType as IEdmEntityType);
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
                WriteEntry(graph, writer, writeContext, expectedType);
            }
        }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a part of an existing OData message using the given
        /// deltaWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual void WriteDeltaObjectInline(object graph, IEdmTypeReference expectedType, ODataDeltaWriter writer,
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
                WriteDeltaEntry(graph, writer, writeContext);
            }
        }

        private void WriteDeltaEntry(object graph, ODataDeltaWriter writer, ODataSerializerContext writeContext)
        {
            Contract.Assert(writeContext != null);

            IEdmEntityTypeReference entityType = GetEntityType(graph, writeContext);
            ResourceContext resourceContext = new ResourceContext(writeContext, entityType, graph);
            SelectExpandNode selectExpandNode = CreateSelectExpandNode(resourceContext);
            if (selectExpandNode != null)
            {
                ODataResource entry = CreateEntry(selectExpandNode, resourceContext);
                if (entry != null)
                {
                    writer.WriteStart(entry);
                    //TODO: Need to add support to write Navigation Links using Delta Writer
                    //https://github.com/OData/odata.net/issues/155
                    writer.WriteEnd();
                }
            }
        }

        private IEnumerable<ODataProperty> CreateODataPropertiesFromDynamicType(EdmEntityType entityType, object graph, Dictionary<IEdmTypeReference, object> navigationProperties = null)
        {
            var properties = new List<ODataProperty>();
            var dynamicObject = graph as DynamicTypeWrapper;
            foreach (var prop in graph.GetType().GetProperties())
            {
                object value;
                ODataProperty property = null;
                if (dynamicObject.TryGetPropertyValue(prop.Name, out value))
                {
                    bool isNavigationProperty = false;
                    if (navigationProperties != null)
                    {
                        if (entityType != null)
                        {
                            var navigationProperty =
                                entityType.NavigationProperties().FirstOrDefault(p => p.Name.Equals(prop.Name));
                            if (navigationProperty != null)
                            {
                                navigationProperties.Add(navigationProperty.Type, value);
                                isNavigationProperty = true;
                            }
                        }
                    }

                    if (!isNavigationProperty)
                    {
                        if (value != null && EdmLibHelpers.IsDynamicTypeWrapper(value.GetType()))
                        {
                            property = new ODataProperty
                            {
                                Name = prop.Name,
                                Value = new ODataComplexValue
                                {
                                    Properties = CreateODataPropertiesFromDynamicType(entityType, value)
                                }
                            };
                        }
                        else
                        {
                            property = new ODataProperty
                            {
                                Name = prop.Name,
                                Value = value
                            };
                        }

                        properties.Add(property);
                    }
                }
            }
            return properties;
        }

        private void WriteDynamicTypeEntry(object graph, ODataWriter writer, IEdmTypeReference expectedType,
            ODataSerializerContext writeContext)
        {
            var navigationProperties = new Dictionary<IEdmTypeReference, object>();
            var entityType = expectedType.Definition as EdmEntityType;
            var entry = new ODataResource()
            {
                TypeName = expectedType.FullName(),
                Properties = CreateODataPropertiesFromDynamicType(entityType, graph, navigationProperties)
            };

            entry.IsTransient = true;
            writer.WriteStart(entry);
            foreach (IEdmTypeReference type in navigationProperties.Keys)
            {
                var resourceContext = new ResourceContext(writeContext, expectedType.AsEntity(), graph);
                var navigationProperty = entityType.NavigationProperties().FirstOrDefault(p => p.Type.Equals(type));
                var navigationLink = CreateNavigationLink(navigationProperty, resourceContext);
                if (navigationLink != null)
                {
                    writer.WriteStart(navigationLink);
                    WriteDynamicTypeEntry(navigationProperties[type], writer, type, writeContext);
                    writer.WriteEnd();
                }
            }

            writer.WriteEnd();
        }

        private void WriteEntry(object graph, ODataWriter writer, ODataSerializerContext writeContext,
            IEdmTypeReference expectedType)
        {
            Contract.Assert(writeContext != null);

            if (EdmLibHelpers.IsDynamicTypeWrapper(graph.GetType()))
            {
                WriteDynamicTypeEntry(graph, writer, expectedType, writeContext);
                return;
            }

            IEdmEntityTypeReference entityType = GetEntityType(graph, writeContext);
            ResourceContext resourceContext = new ResourceContext(writeContext, entityType, graph);

            SelectExpandNode selectExpandNode = CreateSelectExpandNode(resourceContext);
            if (selectExpandNode != null)
            {
                ODataResource entry = CreateEntry(selectExpandNode, resourceContext);
                if (entry != null)
                {
                    writer.WriteStart(entry);
                    WriteNavigationLinks(selectExpandNode.SelectedNavigationProperties, resourceContext, writer);
                    WriteExpandedNavigationProperties(selectExpandNode.ExpandedNavigationProperties,
                        resourceContext, writer);
                    writer.WriteEnd();
                }
            }
        }

        /// <summary>
        /// Creates the <see cref="SelectExpandNode"/> that describes the set of properties and actions to select and expand while writing this entity.
        /// </summary>
        /// <param name="resourceContext">Contains the entity instance being written and the context.</param>
        /// <returns>
        /// The <see cref="SelectExpandNode"/> that describes the set of properties and actions to select and expand while writing this entity.
        /// </returns>
        public virtual SelectExpandNode CreateSelectExpandNode(ResourceContext resourceContext)
        {
            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }

            ODataSerializerContext writeContext = resourceContext.SerializerContext;
            IEdmEntityType entityType = resourceContext.StructuredType as IEdmEntityType;

            object selectExpandNode;
            Tuple<SelectExpandClause, IEdmEntityType> key = Tuple.Create(writeContext.SelectExpandClause, entityType);
            if (!writeContext.Items.TryGetValue(key, out selectExpandNode))
            {
                // cache the selectExpandNode so that if we are writing a feed we don't have to construct it again.
                selectExpandNode = new SelectExpandNode(entityType, writeContext);
                writeContext.Items[key] = selectExpandNode;
            }
            return selectExpandNode as SelectExpandNode;
        }

        /// <summary>
        /// Creates the <see cref="ODataResource"/> to be written while writing this entity.
        /// </summary>
        /// <param name="selectExpandNode">The <see cref="SelectExpandNode"/> describing the response graph.</param>
        /// <param name="resourceContext">The context for the entity instance being written.</param>
        /// <returns>The created <see cref="ODataResource"/>.</returns>
        public virtual ODataResource CreateEntry(SelectExpandNode selectExpandNode, ResourceContext resourceContext)
        {
            if (selectExpandNode == null)
            {
                throw Error.ArgumentNull("selectExpandNode");
            }
            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }

            string typeName = resourceContext.StructuredType.FullTypeName();

            ODataResource entry = new ODataResource
            {
                TypeName = typeName,
                Properties = CreateStructuralPropertyBag(selectExpandNode.SelectedStructuralProperties, resourceContext),
            };

            // Try to add the dynamic properties if the structural type is open.
            if ((resourceContext.StructuredType.IsOpen && selectExpandNode.SelectAllDynamicProperties) ||
                (resourceContext.StructuredType.IsOpen && selectExpandNode.SelectedDynamicProperties.Any()))
            {
                IEdmTypeReference structuredTypeReference =
                    resourceContext.StructuredType.ToEdmTypeReference(isNullable: false);
                List<ODataProperty> dynamicProperties = AppendDynamicProperties(resourceContext.EdmObject,
                    (IEdmStructuredTypeReference)structuredTypeReference,
                    resourceContext.SerializerContext,
                    entry.Properties.ToList(),
                    selectExpandNode.SelectedDynamicProperties.ToArray());

                if (dynamicProperties != null)
                {
                    entry.Properties = entry.Properties.Concat(dynamicProperties);
                }
            }

            IEnumerable<ODataAction> actions = CreateODataActions(selectExpandNode.SelectedActions, resourceContext);
            foreach (ODataAction action in actions)
            {
                entry.AddAction(action);
            }

            IEnumerable<ODataFunction> functions = CreateODataFunctions(selectExpandNode.SelectedFunctions, resourceContext);
            foreach (ODataFunction function in functions)
            {
                entry.AddFunction(function);
            }

            IEdmEntityType pathType = GetODataPathType(resourceContext.SerializerContext);
            AddTypeNameAnnotationAsNeeded(entry, pathType, resourceContext.SerializerContext.MetadataLevel);

            if (resourceContext.NavigationSource != null)
            {
                if (!(resourceContext.NavigationSource is IEdmContainedEntitySet))
                {
                    IEdmModel model = resourceContext.SerializerContext.Model;
                    NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(resourceContext.NavigationSource);
                    EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(resourceContext, resourceContext.SerializerContext.MetadataLevel);

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

                string etag = CreateETag(resourceContext);
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
        /// <param name="resourceContext">The context for the resource instance being written.</param>
        /// <returns>The created ETag.</returns>
        public virtual string CreateETag(ResourceContext resourceContext)
        {
            if (resourceContext.Request != null)
            {
                HttpConfiguration configuration = resourceContext.Request.GetConfiguration();
                if (configuration == null)
                {
                    throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
                }

                IEdmModel model = resourceContext.EdmModel;
                IEdmEntitySet entitySet = resourceContext.NavigationSource as IEdmEntitySet;

                IEnumerable<IEdmStructuralProperty> concurrencyProperties;
                if (model != null && entitySet != null)
                {
                    concurrencyProperties = model.GetConcurrencyProperties(entitySet).OrderBy(c => c.Name);
                }
                else
                {
                    concurrencyProperties = Enumerable.Empty<IEdmStructuralProperty>();
                }

                IDictionary<string, object> properties = new Dictionary<string, object>();
                foreach (IEdmStructuralProperty etagProperty in concurrencyProperties)
                {
                    properties.Add(etagProperty.Name, resourceContext.GetPropertyValue(etagProperty.Name));
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
            IEnumerable<IEdmNavigationProperty> navigationProperties, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);

            IEnumerable<ODataNestedResourceInfo> navigationLinks = CreateNavigationLinks(navigationProperties, resourceContext);
            foreach (ODataNestedResourceInfo navigationLink in navigationLinks)
            {
                writer.WriteStart(navigationLink);
                writer.WriteEnd();
            }
        }

        private void WriteExpandedNavigationProperties(
            IDictionary<IEdmNavigationProperty, SelectExpandClause> navigationPropertiesToExpand,
            ResourceContext resourceContext,
            ODataWriter writer)
        {
            Contract.Assert(navigationPropertiesToExpand != null);
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            foreach (KeyValuePair<IEdmNavigationProperty, SelectExpandClause> navigationPropertyToExpand in navigationPropertiesToExpand)
            {
                IEdmNavigationProperty navigationProperty = navigationPropertyToExpand.Key;

                ODataNestedResourceInfo navigationLink = CreateNavigationLink(navigationProperty, resourceContext);
                if (navigationLink != null)
                {
                    writer.WriteStart(navigationLink);
                    WriteExpandedNavigationProperty(navigationPropertyToExpand, resourceContext, writer);
                    writer.WriteEnd();
                }
            }
        }

        private void WriteExpandedNavigationProperty(
            KeyValuePair<IEdmNavigationProperty, SelectExpandClause> navigationPropertyToExpand,
            ResourceContext resourceContext,
            ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            IEdmNavigationProperty navigationProperty = navigationPropertyToExpand.Key;
            SelectExpandClause selectExpandClause = navigationPropertyToExpand.Value;

            object propertyValue = resourceContext.GetPropertyValue(navigationProperty.Name);

            if (propertyValue == null)
            {
                if (navigationProperty.Type.IsCollection())
                {
                    // A navigation property whose Type attribute specifies a collection, the collection always exists,
                    // it may just be empty.
                    // If a collection of entities can be related, it is represented as a JSON array. An empty
                    // collection of entities (one that contains no entities) is represented as an empty JSON array.
                    writer.WriteStart(new ODataResourceSet());
                }
                else
                {
                    // If at most one entity can be related, the value is null if no entity is currently related.
                    writer.WriteStart(resource: null);
                }

                writer.WriteEnd();
            }
            else
            {
                // create the serializer context for the expanded item.
                ODataSerializerContext nestedWriteContext = new ODataSerializerContext(resourceContext, selectExpandClause, navigationProperty);

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

        private IEnumerable<ODataNestedResourceInfo> CreateNavigationLinks(
            IEnumerable<IEdmNavigationProperty> navigationProperties, ResourceContext resourceContext)
        {
            Contract.Assert(navigationProperties != null);
            Contract.Assert(resourceContext != null);

            foreach (IEdmNavigationProperty navProperty in navigationProperties)
            {
                ODataNestedResourceInfo navigationLink = CreateNavigationLink(navProperty, resourceContext);
                if (navigationLink != null)
                {
                    yield return navigationLink;
                }
            }
        }

        /// <summary>
        /// Creates the <see cref="ODataNestedResourceInfo"/> to be written while writing this entity.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for which the navigation link is being created.</param>
        /// <param name="resourceContext">The context for the entity instance being written.</param>
        /// <returns>The navigation link to be written.</returns>
        public virtual ODataNestedResourceInfo CreateNavigationLink(IEdmNavigationProperty navigationProperty, ResourceContext resourceContext)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }
            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }

            ODataSerializerContext writeContext = resourceContext.SerializerContext;
            ODataNestedResourceInfo navigationLink = null;

            if (writeContext.NavigationSource != null)
            {
                IEdmTypeReference propertyType = navigationProperty.Type;
                IEdmModel model = writeContext.Model;
                NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(writeContext.NavigationSource);
                Uri navigationUrl = linkBuilder.BuildNavigationLink(resourceContext, navigationProperty, writeContext.MetadataLevel);

                navigationLink = new ODataNestedResourceInfo
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
            IEnumerable<IEdmStructuralProperty> structuralProperties, ResourceContext resourceContext)
        {
            Contract.Assert(structuralProperties != null);
            Contract.Assert(resourceContext != null);

            List<ODataProperty> properties = new List<ODataProperty>();
            foreach (IEdmStructuralProperty structuralProperty in structuralProperties)
            {
                ODataProperty property = CreateStructuralProperty(structuralProperty, resourceContext);
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
        /// <param name="resourceContext">The context for the entity instance being written.</param>
        /// <returns>The <see cref="ODataProperty"/> to write.</returns>
        public virtual ODataProperty CreateStructuralProperty(IEdmStructuralProperty structuralProperty, ResourceContext resourceContext)
        {
            if (structuralProperty == null)
            {
                throw Error.ArgumentNull("structuralProperty");
            }
            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }

            ODataSerializerContext writeContext = resourceContext.SerializerContext;

            ODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(structuralProperty.Type);
            if (serializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeSerialized, structuralProperty.Type.FullName(), typeof(ODataMediaTypeFormatter).Name));
            }

            object propertyValue = resourceContext.GetPropertyValue(structuralProperty.Name);

            IEdmTypeReference propertyType = structuralProperty.Type;
            if (propertyValue != null)
            {
                if (!propertyType.IsPrimitive() && !propertyType.IsEnum())
                {
                    IEdmTypeReference actualType = writeContext.GetEdmType(propertyValue, propertyValue.GetType());
                    if (propertyType != null && propertyType != actualType)
                    {
                        propertyType = actualType;
                    }
                }
            }

            return serializer.CreateProperty(propertyValue, propertyType, structuralProperty.Name, writeContext);
        }

        private IEnumerable<ODataAction> CreateODataActions(
            IEnumerable<IEdmAction> actions, ResourceContext resourceContext)
        {
            Contract.Assert(actions != null);
            Contract.Assert(resourceContext != null);

            foreach (IEdmAction action in actions)
            {
                ODataAction oDataAction = CreateODataAction(action, resourceContext);
                if (oDataAction != null)
                {
                    yield return oDataAction;
                }
            }
        }

        private IEnumerable<ODataFunction> CreateODataFunctions(
            IEnumerable<IEdmFunction> functions, ResourceContext resourceContext)
        {
            Contract.Assert(functions != null);
            Contract.Assert(resourceContext != null);

            foreach (IEdmFunction function in functions)
            {
                ODataFunction oDataFunction = CreateODataFunction(function, resourceContext);
                if (oDataFunction != null)
                {
                    yield return oDataFunction;
                }
            }
        }

        /// <summary>
        /// Creates an <see cref="ODataAction" /> to be written for the given action and the entity instance.
        /// </summary>
        /// <param name="action">The OData action.</param>
        /// <param name="resourceContext">The context for the entity instance being written.</param>
        /// <returns>The created action or null if the action should not be written.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2234: Pass System.Uri objects instead of strings", Justification = "This overload is equally good")]
        public virtual ODataAction CreateODataAction(IEdmAction action, ResourceContext resourceContext)
        {
            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }

            IEdmModel model = resourceContext.EdmModel;
            OperationLinkBuilder builder = model.GetOperationLinkBuilder(action);

            if (builder == null)
            {
                return null;
            }

            return CreateODataOperation(action, builder, resourceContext) as ODataAction;
        }

        /// <summary>
        /// Creates an <see cref="ODataFunction" /> to be written for the given action and the entity instance.
        /// </summary>
        /// <param name="function">The OData function.</param>
        /// <param name="resourceContext">The context for the entity instance being written.</param>
        /// <returns>The created function or null if the action should not be written.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2234: Pass System.Uri objects instead of strings",
            Justification = "This overload is equally good")]
        [SuppressMessage("Microsoft.Naming", "CA1716: Use function as parameter name", Justification = "Function")]
        public virtual ODataFunction CreateODataFunction(IEdmFunction function, ResourceContext resourceContext)
        {
            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }

            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }

            IEdmModel model = resourceContext.EdmModel;
            OperationLinkBuilder builder = model.GetOperationLinkBuilder(function);

            if (builder == null)
            {
                return null;
            }

            return CreateODataOperation(function, builder, resourceContext) as ODataFunction;
        }

        private static ODataOperation CreateODataOperation(IEdmOperation operation, OperationLinkBuilder builder, ResourceContext resourceContext)
        {
            Contract.Assert(operation != null);
            Contract.Assert(builder != null);
            Contract.Assert(resourceContext != null);

            ODataMetadataLevel metadataLevel = resourceContext.SerializerContext.MetadataLevel;
            IEdmModel model = resourceContext.EdmModel;

            if (ShouldOmitOperation(operation, builder, metadataLevel))
            {
                return null;
            }

            Uri target = builder.BuildLink(resourceContext);
            if (target == null)
            {
                return null;
            }

            Uri baseUri = new Uri(resourceContext.Url.CreateODataLink(MetadataSegment.Instance));
            Uri metadata = new Uri(baseUri, "#" + CreateMetadataFragment(operation));

            ODataOperation odataOperation;
            if (operation is IEdmAction)
            {
                odataOperation = new ODataAction();
            }
            else
            {
                odataOperation = new ODataFunction();
            }
            odataOperation.Metadata = metadata;

            // Always omit the title in minimal/no metadata modes.
            if (metadataLevel == ODataMetadataLevel.FullMetadata)
            {
                EmitTitle(model, operation, odataOperation);
            }

            // Omit the target in minimal/no metadata modes unless it doesn't follow conventions.
            if (!builder.FollowsConventions || metadataLevel == ODataMetadataLevel.FullMetadata)
            {
                odataOperation.Target = target;
            }

            return odataOperation;
        }

        internal static void EmitTitle(IEdmModel model, IEdmOperation operation, ODataOperation odataOperation)
        {
            // The title should only be emitted in full metadata.
            OperationTitleAnnotation titleAnnotation = model.GetOperationTitleAnnotation(operation);
            if (titleAnnotation != null)
            {
                odataOperation.Title = titleAnnotation.Title;
            }
            else
            {
                odataOperation.Title = operation.Name;
            }
        }

        internal static string CreateMetadataFragment(IEdmOperation operation)
        {
            // There can only be one entity container in OData V4.
            string actionName = operation.Name;
            string fragment = operation.Namespace + "." + actionName;

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

        internal static void AddTypeNameAnnotationAsNeeded(ODataResource entry, IEdmEntityType odataPathType,
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

        internal static bool ShouldOmitOperation(IEdmOperation operation, OperationLinkBuilder builder,
            ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(builder != null);

            switch (metadataLevel)
            {
                case ODataMetadataLevel.MinimalMetadata:
                case ODataMetadataLevel.NoMetadata:
                    return operation.IsBound && builder.FollowsConventions;

                case ODataMetadataLevel.FullMetadata:
                default: // All values already specified; just keeping the compiler happy.
                    return false;
            }
        }

        internal static bool ShouldSuppressTypeNameSerialization(ODataResource entry, IEdmEntityType edmType,
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
