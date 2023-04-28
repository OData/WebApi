//-----------------------------------------------------------------------------
// <copyright file="ODataResourceSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing instances of <see cref="IEdmEntityType"/> or <see cref="IEdmComplexType"/>
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
    public class ODataResourceSerializer : ODataEdmTypeSerializer
    {
        private const string Resource = "Resource";

        /// <inheritdoc />
        public ODataResourceSerializer(ODataSerializerProvider serializerProvider)
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

            IEdmTypeReference edmType = writeContext.GetEdmType(graph, type);
            Contract.Assert(edmType != null);

            IEdmNavigationSource navigationSource = writeContext.NavigationSource;
            ODataWriter writer = messageWriter.CreateODataResourceWriter(navigationSource, edmType.ToStructuredType());
            WriteObjectInline(graph, edmType, writer, writeContext);
        }

        /// <inheritdoc />
        public override async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter,
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

            IEdmTypeReference edmType = writeContext.GetEdmType(graph, type);
            Contract.Assert(edmType != null);

            IEdmNavigationSource navigationSource = writeContext.NavigationSource;
            ODataWriter writer = await messageWriter.CreateODataResourceWriterAsync(navigationSource, edmType.ToStructuredType());
            await WriteObjectInlineAsync(graph, edmType, writer, writeContext);
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

            if (graph == null || graph is NullEdmComplexObject)
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, Resource));
            }
            else
            {
                WriteResource(graph, writer, writeContext, expectedType);
            }
        }

        /// <inheritdoc />
        public override Task WriteObjectInlineAsync(object graph, IEdmTypeReference expectedType, ODataWriter writer,
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

            if (graph == null || graph is NullEdmComplexObject)
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, Resource));
            }

            return WriteResourceAsync(graph, writer, writeContext, expectedType);
        }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a part of an existing OData message using the given
        /// deltaWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual void WriteDeltaObjectInline(object graph, IEdmTypeReference expectedType, ODataWriter writer,
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
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, Resource));
            }
            else
            {
                WriteDeltaResource(graph, writer, writeContext);
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
        public virtual Task WriteDeltaObjectInlineAsync(object graph, IEdmTypeReference expectedType, ODataWriter writer,
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
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, Resource));
            }

            return WriteDeltaResourceAsync(graph, writer, writeContext);
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
            IEdmStructuredType structuredType = resourceContext.StructuredType;

            object selectExpandNode;

            Tuple<SelectExpandClause, IEdmStructuredType> key = Tuple.Create(writeContext.SelectExpandClause, structuredType);
            if (!writeContext.Items.TryGetValue(key, out selectExpandNode))
            {
                // cache the selectExpandNode so that if we are writing a feed we don't have to construct it again.
                selectExpandNode = new SelectExpandNode(structuredType, writeContext);
                writeContext.Items[key] = selectExpandNode;
            }

            return selectExpandNode as SelectExpandNode;
        }

        /// <summary>
        /// Creates the <see cref="ODataResource"/> to be written while writing this resource.
        /// </summary>
        /// <param name="selectExpandNode">The <see cref="SelectExpandNode"/> describing the response graph.</param>
        /// <param name="resourceContext">The context for the resource instance being written.</param>
        /// <returns>The created <see cref="ODataResource"/>.</returns>
        public virtual ODataResource CreateResource(SelectExpandNode selectExpandNode, ResourceContext resourceContext)
        {
            return CreateResource(selectExpandNode, resourceContext, false) as ODataResource;
        }

        /// <summary>
        /// Creates the <see cref="ODataDeletedResource"/> to be written while writing this resource.
        /// </summary>
        /// <param name="selectExpandNode">The <see cref="SelectExpandNode"/> describing the response graph.</param>
        /// <param name="resourceContext">The context for the resource instance being written.</param>
        /// <returns>The created <see cref="ODataDeletedResource"/>.</returns>
        public virtual ODataDeletedResource CreateDeletedResource(SelectExpandNode selectExpandNode, ResourceContext resourceContext)
        {
            return CreateResource(selectExpandNode, resourceContext, true) as ODataDeletedResource;
        }

        /// <summary>
        /// Appends the dynamic properties of primitive, enum or the collection of them into the given <see cref="ODataResource"/>.
        /// If the dynamic property is a property of the complex or collection of complex, it will be saved into
        /// the dynamic complex properties dictionary of <paramref name="resourceContext"/> and be written later.
        /// </summary>
        /// <param name="resource">The <see cref="ODataResource"/> describing the resource.</param>
        /// <param name="selectExpandNode">The <see cref="SelectExpandNode"/> describing the response graph.</param>
        /// <param name="resourceContext">The context for the resource instance being written.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many classes.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple conversion function and cannot be split up.")]
        public virtual void AppendDynamicProperties(ODataResourceBase resource, SelectExpandNode selectExpandNode,
            ResourceContext resourceContext)
        {
            Contract.Assert(resource != null);
            Contract.Assert(selectExpandNode != null);
            Contract.Assert(resourceContext != null);

            if (!resourceContext.StructuredType.IsOpen || // non-open type
                (!selectExpandNode.SelectAllDynamicProperties && selectExpandNode.SelectedDynamicProperties == null))
            {
                return;
            }

            bool nullDynamicPropertyEnabled = false;
            if (resourceContext.EdmObject is EdmDeltaComplexObject || resourceContext.EdmObject is EdmDeltaEntityObject)
            {
                nullDynamicPropertyEnabled = true;
            }
            else if (resourceContext.InternalRequest != null)
            {
                nullDynamicPropertyEnabled = resourceContext.InternalRequest.Options.NullDynamicPropertyIsEnabled;
            }

            IEdmStructuredType structuredType = resourceContext.StructuredType;
            IEdmStructuredObject structuredObject = resourceContext.EdmObject;
            object value;
            IDelta delta = structuredObject as IDelta;
            if (delta == null)
            {
                PropertyInfo dynamicPropertyInfo = EdmLibHelpers.GetDynamicPropertyDictionary(structuredType,
                    resourceContext.EdmModel);
                if (dynamicPropertyInfo == null || structuredObject == null ||
                    !structuredObject.TryGetPropertyValue(dynamicPropertyInfo.Name, out value) || value == null)
                {
                    return;
                }
            }
            else
            {
                value = ((EdmStructuredObject)structuredObject).TryGetDynamicProperties();
            }

            IDictionary<string, object> dynamicPropertyDictionary = (IDictionary<string, object>)value;

            // Build a HashSet to store the declared property names.
            // It is used to make sure the dynamic property name is different from all declared property names.
            HashSet<string> declaredPropertyNameSet = new HashSet<string>(resource.Properties.Select(p => p.Name));
            List<ODataProperty> dynamicProperties = new List<ODataProperty>();

            // Due to the early return condition at the start of this method we know that either:
            //  1. SelectAllDynamicProperties == true
            //  2. SelectedDynamicProperties != null
            //  3. SelectAllDynamicProperties == true AND SelectedDynamicProperties != null
            //
            // Previous code simply checked the nullity of SelectedDynamicProperties here, but that ignored case 3 since which is possible
            // since SelectExpandNode.BuildSelectExpand will set SelectAllDynamicProperties to true even if SelectedDynamicProperties is
            // not-null. Instead we need to check both properties in order to determine if a dynamic property should be selected.
            IEnumerable<KeyValuePair<string, object>> dynamicPropertiesToSelect =
                dynamicPropertyDictionary.Where(
                    x => selectExpandNode.SelectAllDynamicProperties || 
                         selectExpandNode.SelectedDynamicProperties.Contains(x.Key));
            foreach (KeyValuePair<string, object> dynamicProperty in dynamicPropertiesToSelect)
            {
                if (String.IsNullOrEmpty(dynamicProperty.Key))
                {
                    continue;
                }

                if (dynamicProperty.Value == null)
                {
                    if (nullDynamicPropertyEnabled)
                    {
                        dynamicProperties.Add(new ODataProperty
                        {
                            Name = dynamicProperty.Key,
                            Value = new ODataNullValue()
                        });
                    }

                    continue;
                }

                if (declaredPropertyNameSet.Contains(dynamicProperty.Key))
                {
                    throw Error.InvalidOperation(SRResources.DynamicPropertyNameAlreadyUsedAsDeclaredPropertyName,
                        dynamicProperty.Key, structuredType.FullTypeName());
                }

                IEdmTypeReference edmTypeReference = resourceContext.SerializerContext.GetEdmType(dynamicProperty.Value,
                    dynamicProperty.Value.GetType());
                if (edmTypeReference == null)
                {
                    throw Error.NotSupported(SRResources.TypeOfDynamicPropertyNotSupported,
                        dynamicProperty.Value.GetType().FullName, dynamicProperty.Key);
                }

                if (edmTypeReference.IsStructured() ||
                    (edmTypeReference.IsCollection() && edmTypeReference.AsCollection().ElementType().IsStructured()))
                {
                    if (resourceContext.DynamicComplexProperties == null)
                    {
                        resourceContext.DynamicComplexProperties = new ConcurrentDictionary<string, object>();
                    }

                    resourceContext.DynamicComplexProperties.Add(dynamicProperty);
                }
                else
                {
                    ODataEdmTypeSerializer propertySerializer = SerializerProvider.GetEdmTypeSerializer(edmTypeReference);
                    if (propertySerializer == null)
                    {
                        throw Error.NotSupported(SRResources.DynamicPropertyCannotBeSerialized, dynamicProperty.Key,
                            edmTypeReference.FullName());
                    }

                    dynamicProperties.Add(propertySerializer.CreateProperty(
                        dynamicProperty.Value, edmTypeReference, dynamicProperty.Key, resourceContext.SerializerContext));
                }
            }

            if (dynamicProperties.Any())
            {
                resource.Properties = resource.Properties.Concat(dynamicProperties);
            }
        }

        /// <summary>
        /// Method to append InstanceAnnotations to the ODataResource and Property.
        /// Instance annotations are annotations for a resource or a property and couldb be of contain a primitive, comple , enum or collection type 
        /// These will be saved in to an Instance annotation dictionary
        /// </summary>
        /// <param name="resource">The <see cref="ODataResource"/> describing the resource, which is being annotated.</param>
        /// <param name="resourceContext">The context for the resource instance, which is being annotated.</param>        
        public virtual void AppendInstanceAnnotations(ODataResourceBase resource, ResourceContext resourceContext)
        {
            IEdmStructuredType structuredType = resourceContext.StructuredType;
            IEdmStructuredObject structuredObject = resourceContext.EdmObject;

            //For appending transient and persistent instance annotations for both enity object and normal resources

            PropertyInfo instanceAnnotationInfo = EdmLibHelpers.GetInstanceAnnotationsContainer(structuredType,
                resourceContext.EdmModel);

            object instanceAnnotations = null;
            IODataInstanceAnnotationContainer transientAnnotations = null;

            if (resourceContext.SerializerContext.IsDeltaOfT && resourceContext.ResourceInstance is IDelta delta) 
            {
                if (instanceAnnotationInfo != null)
                {
                    delta.TryGetPropertyValue(instanceAnnotationInfo.Name, out instanceAnnotations);
                }

                if (resourceContext.ResourceInstance is IDeltaSetItem deltaitem)
                {
                    transientAnnotations = deltaitem.TransientInstanceAnnotationContainer;
                }
            }
            else
            {
                if (structuredObject != null && (instanceAnnotationInfo == null || !structuredObject.TryGetPropertyValue(instanceAnnotationInfo.Name, out instanceAnnotations) || instanceAnnotations == null))
                {
                    if (structuredObject is EdmEntityObject edmEntityObject)
                    {
                        instanceAnnotations = edmEntityObject.PersistentInstanceAnnotationsContainer;
                        transientAnnotations = edmEntityObject.TransientInstanceAnnotationContainer;
                    }
                }
            }

            ODataSerializerHelper.AppendInstanceAnnotations(resource, resourceContext, instanceAnnotations as IODataInstanceAnnotationContainer, SerializerProvider);

            ODataSerializerHelper.AppendInstanceAnnotations(resource, resourceContext, transientAnnotations, SerializerProvider);
        }

        /// <summary>
        /// Creates the ETag for the given entity.
        /// </summary>
        /// <param name="resourceContext">The context for the resource instance being written.</param>
        /// <returns>The created ETag.</returns>
        public virtual string CreateETag(ResourceContext resourceContext)
        {
            if (resourceContext.InternalRequest != null)
            {
                IEdmModel model = resourceContext.EdmModel;
                IEdmNavigationSource navigationSource = resourceContext.NavigationSource;

                IEnumerable<IEdmStructuralProperty> concurrencyProperties;
                if (model != null && navigationSource != null)
                {
                    concurrencyProperties = model.GetConcurrencyProperties(navigationSource).OrderBy(c => c.Name);
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

                return resourceContext.InternalRequest.CreateETag(properties);
            }

            return null;
        }

        /// <summary>
        /// Creates the <see cref="ODataNestedResourceInfo"/> to be written while writing this dynamic complex property.
        /// </summary>
        /// <param name="propertyName">The dynamic property name.</param>
        /// <param name="propertyValue">The dynamic property value.</param>
        /// <param name="edmType">The edm type reference.</param>
        /// <param name="resourceContext">The context for the complex instance being written.</param>
        /// <returns>The nested resource info to be written. Returns 'null' will omit this serialization.</returns>
        /// <remarks>It enables customer to get more control by overriding this method. </remarks>
        public virtual ODataNestedResourceInfo CreateDynamicComplexNestedResourceInfo(string propertyName, object propertyValue, IEdmTypeReference edmType, ResourceContext resourceContext)
        {
            ODataNestedResourceInfo nestedInfo = null;
            if (propertyName != null && edmType != null)
            {
                nestedInfo = new ODataNestedResourceInfo
                {
                    IsCollection = edmType.IsCollection(),
                    Name = propertyName,
                };
            }

            return nestedInfo;
        }

        /// <summary>
        /// Creates the <see cref="ODataNestedResourceInfo"/> to be written while writing this complex property.
        /// </summary>
        /// <param name="complexProperty">The complex property for which the nested resource info is being created.</param>
        /// <param name="pathSelectItem">The corresponding sub select item belongs to this complex property.</param>
        /// <param name="resourceContext">The context for the complex instance being written.</param>
        /// <returns>The nested resource info to be written. Returns 'null' will omit this complex serialization.</returns>
        /// <remarks>It enables customer to get more control by overriding this method. </remarks>
        public virtual ODataNestedResourceInfo CreateComplexNestedResourceInfo(IEdmStructuralProperty complexProperty, PathSelectItem pathSelectItem, ResourceContext resourceContext)
        {
            if (complexProperty == null)
            {
                throw Error.ArgumentNull(nameof(complexProperty));
            }

            ODataNestedResourceInfo nestedInfo = null;

            if (complexProperty.Type != null)
            {
                nestedInfo = new ODataNestedResourceInfo
                {
                    IsCollection = complexProperty.Type.IsCollection(),
                    Name = complexProperty.Name
                };
            }

            return nestedInfo;
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
            IEdmNavigationSource navigationSource = writeContext.NavigationSource;
            ODataNestedResourceInfo navigationLink = null;

            if (navigationSource != null)
            {
                IEdmTypeReference propertyType = navigationProperty.Type;
                IEdmModel model = writeContext.Model;
                NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(navigationSource);
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
                    Error.Format(SRResources.TypeCannotBeSerialized, structuralProperty.Type.FullName()));
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

        /// <summary>
        /// Gets the resource context for the resource being written.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        /// <returns>The <see cref="ResourceContext"/>.</returns>
        internal ResourceContext GetResourceContext(object graph, ODataSerializerContext writeContext)
        {
            Contract.Assert(writeContext != null);

            IEdmStructuredTypeReference structuredType = GetResourceType(graph, writeContext);
            ResourceContext resourceContext = new ResourceContext(writeContext, structuredType, graph);
            EdmDeltaEntityObject deltaResource = graph as EdmDeltaEntityObject;
            if (deltaResource != null && deltaResource.NavigationSource != null)
            {
                resourceContext.NavigationSource = deltaResource.NavigationSource;
            }

            return resourceContext;
        }

        /// <summary>
        /// Writes the delta complex properties.
        /// </summary>
        /// <param name="selectExpandNode">Contains the set of properties and actions to use to select and expand while writing an entity.</param>
        /// <param name="resourceContext">The resource context for the resource being written.</param>
        /// <param name="writer">The ODataWriter.</param>
        internal void WriteDeltaComplexProperties(SelectExpandNode selectExpandNode,
            ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            IEnumerable<KeyValuePair<IEdmStructuralProperty, PathSelectItem>> complexProperties = GetPropertiesToWrite(selectExpandNode, resourceContext);

            foreach (KeyValuePair<IEdmStructuralProperty, PathSelectItem> complexProperty in complexProperties)
            {
                ODataNestedResourceInfo nestedResourceInfo = new ODataNestedResourceInfo
                {
                    IsCollection = complexProperty.Key.Type.IsCollection(),
                    Name = complexProperty.Key.Name
                };

                writer.WriteStart(nestedResourceInfo);
                WriteDeltaComplexAndExpandedNavigationProperty(complexProperty.Key, null, resourceContext, writer);
                writer.WriteEnd();
            }
        }

        /// <summary>
        /// Writes the delta navigation properties.
        /// </summary>
        /// <param name="selectExpandNode">Contains the set of properties and actions to use to select and expand while writing an entity.</param>
        /// <param name="resourceContext">The resource context for the resource being written.</param>
        /// <param name="writer">The ODataWriter.</param>
        internal void WriteDeltaNavigationProperties(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null, "The resource context cannot be null");
            Contract.Assert(writer != null, "The writer cannot be null");

            IEnumerable<KeyValuePair<IEdmNavigationProperty, Type>> navigationProperties = GetNavigationPropertiesToWrite(selectExpandNode, resourceContext);

            foreach (KeyValuePair<IEdmNavigationProperty, Type> navigationProperty in navigationProperties)
            {
                ODataNestedResourceInfo nestedResourceInfo = new ODataNestedResourceInfo
                {
                    IsCollection = navigationProperty.Key.Type.IsCollection(),
                    Name = navigationProperty.Key.Name
                };

                writer.WriteStart(nestedResourceInfo);
                WriteDeltaComplexAndExpandedNavigationProperty(navigationProperty.Key, null, resourceContext, writer, navigationProperty.Value);
                writer.WriteEnd();
            }
        }

        /// <summary>
        /// Writes delta navigation properties asynchronously.
        /// </summary>
        /// <param name="selectExpandNode">Contains the set of properties and actions to use to select and expand while writing an entity.</param>
        /// <param name="resourceContext">The resource context for the resource being written.</param>
        /// <param name="writer">The ODataWriter.</param>
        /// <returns>A task that represents the asynchronous write operation</returns>
        internal async Task WriteDeltaNavigationPropertiesAsync(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null, "The ResourceContext cannot be null");
            Contract.Assert(writer != null, "The ODataWriter cannot be null");

            IEnumerable<KeyValuePair<IEdmNavigationProperty, Type>> navigationProperties = GetNavigationPropertiesToWrite(selectExpandNode, resourceContext);

            foreach (KeyValuePair<IEdmNavigationProperty, Type> navigationProperty in navigationProperties)
            {
                ODataNestedResourceInfo nestedResourceInfo = new ODataNestedResourceInfo
                {
                    IsCollection = navigationProperty.Key.Type.IsCollection(),
                    Name = navigationProperty.Key.Name
                };

                await writer.WriteStartAsync(nestedResourceInfo);
                await WriteDeltaComplexAndExpandedNavigationPropertyAsync(navigationProperty.Key, null, resourceContext, writer, navigationProperty.Value);
                await writer.WriteEndAsync();
            }
        }

        /// <summary>
        /// Writes delta complex properties asynchronously.
        /// </summary>
        /// <param name="selectExpandNode">Contains the set of properties and actions to use to select and expand while writing an entity.</param>
        /// <param name="resourceContext">The resource context for the resource being written.</param>
        /// <param name="writer">The ODataWriter.</param>
        /// <returns>A task that represents the asynchronous write operation</returns>
        internal async Task WriteDeltaComplexPropertiesAsync(SelectExpandNode selectExpandNode,
            ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            IEnumerable<KeyValuePair<IEdmStructuralProperty, PathSelectItem>> complexProperties = GetPropertiesToWrite(selectExpandNode, resourceContext);

            foreach (KeyValuePair<IEdmStructuralProperty, PathSelectItem> complexProperty in complexProperties)
            {
                ODataNestedResourceInfo nestedResourceInfo = new ODataNestedResourceInfo
                {
                    IsCollection = complexProperty.Key.Type.IsCollection(),
                    Name = complexProperty.Key.Name
                };

                await writer.WriteStartAsync(nestedResourceInfo);
                await WriteDeltaComplexAndExpandedNavigationPropertyAsync(complexProperty.Key, null, resourceContext, writer);
                await writer.WriteEndAsync();
            }
        }

        /// <summary>
        /// Creates the <see cref="ODataStreamPropertyInfo"/> to be written for the given stream property.
        /// </summary>
        /// <param name="structuralProperty">The EDM structural property being written.</param>
        /// <param name="resourceContext">The context for the entity instance being written.</param>
        /// <returns>The <see cref="ODataStreamPropertyInfo"/> to write.</returns>
        internal virtual ODataStreamPropertyInfo CreateStreamProperty(IEdmStructuralProperty structuralProperty, ResourceContext resourceContext)
        {
            if (structuralProperty == null)
            {
                throw Error.ArgumentNull("structuralProperty");
            }

            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }

            if (structuralProperty.Type == null || !structuralProperty.Type.IsStream())
            {
                return null;
            }

            if (resourceContext.SerializerContext.MetadataLevel != ODataMetadataLevel.FullMetadata)
            {
                return null;
            }

            // TODO: we need to return ODataStreamReferenceValue if
            // 1) If we have the EditLink link builder
            // 2) If we have the ReadLink link builder
            // 3) If we have the Core.AcceptableMediaTypes annotation associated with the Stream property

            // We need a way for the user to specify a mediatype for an instance of a stream property.
            // If specified, we should explicitly write the streamreferencevalue and not let ODL fill it in.

            // Although the mediatype is represented as an instance annotation in JSON, it's really control information.
            // So we shouldn't use instance annotations to tell us the media type, but have a separate way to specify the media type.
            // Perhaps we define an interface (and stream wrapper class that derives from stream and implements the interface) that exposes a MediaType property.
            // If the stream property implements this interface, and it specifies a media-type other than application/octet-stream, we explicitly create and write a StreamReferenceValue with that media type.
            // We could also use this type to expose properties for things like ReadLink and WriteLink(and even ETag)
            // that the user could specify to something other than the default convention
            // if they wanted to provide custom routes for reading/writing the stream values or custom ETag values for the stream.

            // So far, let's return null and let OData.lib to calculate the ODataStreamReferenceValue by conventions.
            return null;
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

        internal static void AddTypeNameAnnotationAsNeeded(ODataResourceBase resource, IEdmStructuredType odataPathType,
            ODataMetadataLevel metadataLevel)
        {
            // ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
            // null when values should not be serialized. The TypeName property is different and should always be
            // provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
            // to serialize the type name (a null value prevents serialization).

            // Note: In the current version of ODataLib the default behavior likely now matches the requirements for
            // minimal metadata mode. However, there have been behavior changes/bugs there in the past, so the safer
            // option is for this class to take control of type name serialization in minimal metadata mode.

            Contract.Assert(resource != null);

            string typeName = null; // Set null to force the type name not to serialize.

            // Provide the type name to serialize.
            if (!ShouldSuppressTypeNameSerialization(resource, odataPathType, metadataLevel))
            {
                typeName = resource.TypeName;
            }

            resource.TypeAnnotation = new ODataTypeAnnotation(typeName);
        }

        internal static void AddTypeNameAnnotationAsNeededForComplex(ODataResourceBase resource, ODataMetadataLevel metadataLevel)
        {
            // ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
            // null when values should not be serialized. The TypeName property is different and should always be
            // provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
            // to serialize the type name (a null value prevents serialization).
            Contract.Assert(resource != null);

            // Only add an annotation if we want to override ODataLib's default type name serialization behavior.
            if (ShouldAddTypeNameAnnotationForComplex(metadataLevel))
            {
                string typeName;

                // Provide the type name to serialize (or null to force it not to serialize).
                if (ShouldSuppressTypeNameSerializationForComplex(metadataLevel))
                {
                    typeName = null;
                }
                else
                {
                    typeName = resource.TypeName;
                }

                resource.TypeAnnotation = new ODataTypeAnnotation(typeName);
            }
        }

        internal static bool ShouldAddTypeNameAnnotationForComplex(ODataMetadataLevel metadataLevel)
        {
            switch (metadataLevel)
            {
                // For complex types, the default behavior matches the requirements for minimal metadata mode, so no
                // annotation is necessary.
                case ODataMetadataLevel.MinimalMetadata:
                    return false;
                // In other cases, this class must control the type name serialization behavior.
                case ODataMetadataLevel.FullMetadata:
                case ODataMetadataLevel.NoMetadata:
                default: // All values already specified; just keeping the compiler happy.
                    return true;
            }
        }

        internal static bool ShouldSuppressTypeNameSerializationForComplex(ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(metadataLevel != ODataMetadataLevel.MinimalMetadata);

            switch (metadataLevel)
            {
                case ODataMetadataLevel.NoMetadata:
                    return true;
                case ODataMetadataLevel.FullMetadata:
                default: // All values already specified; just keeping the compiler happy.
                    return false;
            }
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

        internal static bool ShouldSuppressTypeNameSerialization(ODataResourceBase resource, IEdmStructuredType edmType,
            ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(resource != null);

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
                        pathTypeName = edmType.FullTypeName();
                    }
                    string resourceTypeName = resource.TypeName;
                    return String.Equals(resourceTypeName, pathTypeName, StringComparison.Ordinal);
            }
        }

        private ODataResourceBase CreateResource(SelectExpandNode selectExpandNode, ResourceContext resourceContext, bool isDeletedResource)
        {
            if (selectExpandNode == null)
            {
                throw Error.ArgumentNull("selectExpandNode");
            }

            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }

            if (resourceContext.SerializerContext.ExpandReference)
            {
                if (isDeletedResource)
                {
                    return new ODataDeletedResource
                    {
                        Id = resourceContext.GenerateSelfLink(false)
                    };
                }

                return new ODataResource
                {
                    Id = resourceContext.GenerateSelfLink(false)
                };
            }

            string typeName = resourceContext.StructuredType.FullTypeName();
            ODataResourceBase resource;

            if (isDeletedResource)
            {
                resource = new ODataDeletedResource
                {
                    TypeName = typeName,
                    Properties = CreateStructuralPropertyBag(selectExpandNode, resourceContext),
                };
            }
            else
            {
                resource = new ODataResource
                {
                    TypeName = typeName,
                    Properties = CreateStructuralPropertyBag(selectExpandNode, resourceContext),
                };
            }


            if (resourceContext.EdmObject is EdmDeltaEntityObject && resourceContext.NavigationSource != null)
            {
                ODataResourceSerializationInfo serializationInfo = new ODataResourceSerializationInfo();
                serializationInfo.NavigationSourceName = resourceContext.NavigationSource.Name;
                serializationInfo.NavigationSourceKind = resourceContext.NavigationSource.NavigationSourceKind();
                IEdmEntityType sourceType = resourceContext.NavigationSource.EntityType();
                if (sourceType != null)
                {
                    serializationInfo.NavigationSourceEntityTypeName = sourceType.Name;
                }
                resource.SetSerializationInfo(serializationInfo);
            }

            // Try to add the dynamic properties if the structural type is open.
            AppendDynamicProperties(resource, selectExpandNode, resourceContext);

            // Try to add instance annotations
            AppendInstanceAnnotations(resource, resourceContext);

            if (selectExpandNode.SelectedActions != null)
            {
                IEnumerable<ODataAction> actions = CreateODataActions(selectExpandNode.SelectedActions, resourceContext);
                foreach (ODataAction action in actions)
                {
                    resource.AddAction(action);
                }
            }

            if (selectExpandNode.SelectedFunctions != null)
            {
                IEnumerable<ODataFunction> functions = CreateODataFunctions(selectExpandNode.SelectedFunctions, resourceContext);
                foreach (ODataFunction function in functions)
                {
                    resource.AddFunction(function);
                }
            }

            IEdmStructuredType pathType = GetODataPathType(resourceContext.SerializerContext);
            if (resourceContext.StructuredType.TypeKind == EdmTypeKind.Complex)
            {
                AddTypeNameAnnotationAsNeededForComplex(resource, resourceContext.SerializerContext.MetadataLevel);
            }
            else
            {
                AddTypeNameAnnotationAsNeeded(resource, pathType, resourceContext.SerializerContext.MetadataLevel);
            }

            if (!isDeletedResource && resourceContext.StructuredType.TypeKind == EdmTypeKind.Entity && resourceContext.NavigationSource != null)
            {
                if (!(resourceContext.NavigationSource is IEdmContainedEntitySet))
                {
                    IEdmModel model = resourceContext.SerializerContext.Model;
                    NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(resourceContext.NavigationSource);
                    EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(resourceContext, resourceContext.SerializerContext.MetadataLevel);

                    if (selfLinks.IdLink != null)
                    {
                        resource.Id = selfLinks.IdLink;
                    }

                    if (selfLinks.ReadLink != null)
                    {
                        resource.ReadLink = selfLinks.ReadLink;
                    }

                    if (selfLinks.EditLink != null)
                    {
                        resource.EditLink = selfLinks.EditLink;
                    }
                }

                string etag = CreateETag(resourceContext);
                if (etag != null)
                {
                    resource.ETag = etag;
                }
            }

            return resource;
        }

        private void WriteDeltaComplexAndExpandedNavigationProperty(IEdmProperty edmProperty, SelectExpandClause selectExpandClause,
           ResourceContext resourceContext, ODataWriter writer, Type navigationPropertyType = null)
        {
            Contract.Assert(edmProperty != null);
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            object propertyValue = resourceContext.GetPropertyValue(edmProperty.Name);

            if (propertyValue == null || propertyValue is NullEdmComplexObject)
            {
                if (edmProperty.Type.IsCollection())
                {
                    // A complex or navigation property whose Type attribute specifies a collection, the collection always exists,
                    // it may just be empty.
                    // If a collection of complex or entities can be related, it is represented as a JSON array. An empty
                    // collection of resources (one that contains no resource) is represented as an empty JSON array.
                    writer.WriteStart(new ODataResourceSet
                    {
                        TypeName = edmProperty.Type.FullName()
                    });
                }
                else
                {
                    // If at most one resource can be related, the value is null if no resource is currently related.
                    writer.WriteStart(resource: null);
                }

                writer.WriteEnd();
            }
            else
            {
                // create the serializer context for the complex and expanded item.
                ODataSerializerContext nestedWriteContext = new ODataSerializerContext(resourceContext, selectExpandClause, edmProperty);
                nestedWriteContext.Type = navigationPropertyType;

                // write object.

                // TODO: enable overriding serializer based on type. Currentlky requires serializer supports WriteDeltaObjectinline, because it takes an ODataDeltaWriter
                // ODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(edmProperty.Type);
                // if (serializer == null)
                // {
                //     throw new SerializationException(
                //         Error.Format(SRResources.TypeCannotBeSerialized, edmProperty.Type.ToTraceString()));
                // }
                if (edmProperty.Type.IsCollection())
                {
                    ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(SerializerProvider);
                    serializer.WriteDeltaFeedInline(propertyValue, edmProperty.Type, writer, nestedWriteContext);
                }
                else
                {
                    ODataResourceSerializer serializer = new ODataResourceSerializer(SerializerProvider);
                    serializer.WriteDeltaObjectInline(propertyValue, edmProperty.Type, writer, nestedWriteContext);
                }
            }
        }

        private async Task WriteDeltaComplexAndExpandedNavigationPropertyAsync(IEdmProperty edmProperty, SelectExpandClause selectExpandClause,
            ResourceContext resourceContext, ODataWriter writer, Type navigationPropertyType = null)
        {
            Contract.Assert(edmProperty != null);
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            object propertyValue = resourceContext.GetPropertyValue(edmProperty.Name);

            if (propertyValue == null || propertyValue is NullEdmComplexObject)
            {
                if (edmProperty.Type.IsCollection())
                {
                    // A complex or navigation property whose Type attribute specifies a collection, the collection always exists,
                    // it may just be empty.
                    // If a collection of complex or entities can be related, it is represented as a JSON array. An empty
                    // collection of resources (one that contains no resource) is represented as an empty JSON array.
                    await writer.WriteStartAsync(new ODataResourceSet
                    {
                        TypeName = edmProperty.Type.FullName()
                    });
                }
                else
                {
                    // If at most one resource can be related, the value is null if no resource is currently related.
                    await writer.WriteStartAsync(resource: null);
                }

                await writer.WriteEndAsync();
            }
            else
            {
                // create the serializer context for the complex and expanded item.
                ODataSerializerContext nestedWriteContext = new ODataSerializerContext(resourceContext, selectExpandClause, edmProperty);
                nestedWriteContext.Type = navigationPropertyType;

                // write object.

                // TODO: enable overriding serializer based on type. Currentlky requires serializer supports WriteDeltaObjectinline, because it takes an ODataDeltaWriter
                // ODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(edmProperty.Type);
                // if (serializer == null)
                // {
                //     throw new SerializationException(
                //         Error.Format(SRResources.TypeCannotBeSerialized, edmProperty.Type.ToTraceString()));
                // }
                if (edmProperty.Type.IsCollection())
                {
                    ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(SerializerProvider);
                    await serializer.WriteDeltaFeedInlineAsync(propertyValue, edmProperty.Type, writer, nestedWriteContext);
                }
                else
                {
                    ODataResourceSerializer serializer = new ODataResourceSerializer(SerializerProvider);
                    await serializer.WriteDeltaObjectInlineAsync(propertyValue, edmProperty.Type, writer, nestedWriteContext);
                }
            }
        }

        private static IEnumerable<ODataProperty> CreateODataPropertiesFromDynamicType(EdmStructuredType structuredType, object graph,
            Dictionary<IEdmProperty, object> dynamicTypeProperties)
        {
            Contract.Assert(dynamicTypeProperties != null);

            var properties = new List<ODataProperty>();
            var dynamicObject = graph as DynamicTypeWrapper;
            if (dynamicObject == null)
            {
                var dynamicEnumerable = (graph as IEnumerable<DynamicTypeWrapper>);
                if (dynamicEnumerable != null)
                {
                    dynamicObject = dynamicEnumerable.SingleOrDefault();
                }
            }
            if (dynamicObject != null)
            {
                foreach (var prop in dynamicObject.Values)
                {
                    IEdmProperty edmProperty = structuredType?.Properties()
                            .FirstOrDefault(p => p.Name.Equals(prop.Key));
                    if (prop.Value != null
                        && (prop.Value is DynamicTypeWrapper || (prop.Value is IEnumerable<DynamicTypeWrapper>)))
                    {
                        if (edmProperty != null)
                        {
                            dynamicTypeProperties.Add(edmProperty, prop.Value);
                        }
                    }
                    else
                    {
                        ODataProperty property;
                        if (prop.Value == null)
                        {
                            property = new ODataProperty
                            {
                                Name = prop.Key,
                                Value = new ODataNullValue()
                            };
                        }
                        else
                        {
                            if (edmProperty != null)
                            {
                                property = new ODataProperty
                                {
                                    Name = prop.Key,
                                    Value = ODataPrimitiveSerializer.ConvertPrimitiveValue(prop.Value, edmProperty.Type.AsPrimitive())
                                };
                            }
                            else
                            {
                                property = new ODataProperty
                                {
                                    Name = prop.Key,
                                    Value = prop.Value
                                };
                            }
                        }

                        properties.Add(property);
                    }
                }
            }
            return properties;
        }

        private void WriteDynamicTypeResource(object graph, ODataWriter writer, IEdmTypeReference expectedType,
            ODataSerializerContext writeContext)
        {
            var dynamicTypeProperties = new Dictionary<IEdmProperty, object>();
            var structuredType = expectedType.Definition as EdmStructuredType;
            var resource = new ODataResource()
            {
                TypeName = expectedType.FullName(),
                Properties = CreateODataPropertiesFromDynamicType(structuredType, graph, dynamicTypeProperties)
            };

            resource.IsTransient = true;
            writer.WriteStart(resource);
            foreach (var property in dynamicTypeProperties.Keys)
            {
                var resourceContext = new ResourceContext(writeContext, expectedType.AsStructured(), graph);
                if (structuredType.NavigationProperties().Any(p => p.Type.Equals(property.Type)) && !(property.Type is EdmCollectionTypeReference))
                {
                    var navigationProperty = structuredType.NavigationProperties().FirstOrDefault(p => p.Type.Equals(property.Type));
                    var navigationLink = CreateNavigationLink(navigationProperty, resourceContext);
                    if (navigationLink != null)
                    {
                        writer.WriteStart(navigationLink);
                        WriteDynamicTypeResource(dynamicTypeProperties[property], writer, property.Type, writeContext);
                        writer.WriteEnd();
                    }
                }
                else
                {
                    ODataNestedResourceInfo nestedResourceInfo = new ODataNestedResourceInfo
                    {
                        IsCollection = property.Type.IsCollection(),
                        Name = property.Name
                    };

                    writer.WriteStart(nestedResourceInfo);
                    WriteDynamicComplexProperty(dynamicTypeProperties[property], property.Type, resourceContext, writer);
                    writer.WriteEnd();
                }
            }

            writer.WriteEnd();
        }

        private async Task WriteDynamicTypeResourceAsync(object graph, ODataWriter writer, IEdmTypeReference expectedType,
            ODataSerializerContext writeContext)
        {
            var dynamicTypeProperties = new Dictionary<IEdmProperty, object>();
            var structuredType = expectedType.Definition as EdmStructuredType;
            var resource = new ODataResource()
            {
                TypeName = expectedType.FullName(),
                Properties = CreateODataPropertiesFromDynamicType(structuredType, graph, dynamicTypeProperties)
            };

            resource.IsTransient = true;
            await writer.WriteStartAsync(resource);
            foreach (var property in dynamicTypeProperties.Keys)
            {
                var resourceContext = new ResourceContext(writeContext, expectedType.AsStructured(), graph);
                if (structuredType.NavigationProperties().Any(p => p.Type.Equals(property.Type)) && !(property.Type is EdmCollectionTypeReference))
                {
                    var navigationProperty = structuredType.NavigationProperties().FirstOrDefault(p => p.Type.Equals(property.Type));
                    var navigationLink = CreateNavigationLink(navigationProperty, resourceContext);
                    if (navigationLink != null)
                    {
                        await writer.WriteStartAsync(navigationLink);
                        await WriteDynamicTypeResourceAsync(dynamicTypeProperties[property], writer, property.Type, writeContext);
                        await writer.WriteEndAsync();
                    }
                }
                else
                {
                    ODataNestedResourceInfo nestedResourceInfo = new ODataNestedResourceInfo
                    {
                        IsCollection = property.Type.IsCollection(),
                        Name = property.Name
                    };

                    await writer.WriteStartAsync(nestedResourceInfo);
                    await WriteDynamicComplexPropertyAsync(dynamicTypeProperties[property], property.Type, resourceContext, writer);
                    await writer.WriteEndAsync();
                }
            }

            await writer.WriteEndAsync();
        }

        private void WriteResource(object graph, ODataWriter writer, ODataSerializerContext writeContext,
            IEdmTypeReference expectedType)
        {
            Contract.Assert(writeContext != null);

            if (EdmLibHelpers.IsDynamicTypeWrapper(graph.GetType()))
            {
                WriteDynamicTypeResource(graph, writer, expectedType, writeContext);
                return;
            }

            IEdmStructuredTypeReference structuredType = GetResourceType(graph, writeContext);
            ResourceContext resourceContext = new ResourceContext(writeContext, structuredType, graph);

            SelectExpandNode selectExpandNode = CreateSelectExpandNode(resourceContext);
            if (selectExpandNode != null)
            {
                ODataResource resource = CreateResource(selectExpandNode, resourceContext);
                if (resource != null)
                {
                    if (resourceContext.SerializerContext.ExpandReference)
                    {
                        writer.WriteEntityReferenceLink(new ODataEntityReferenceLink
                        {
                            Url = resource.Id
                        });
                    }
                    else
                    {
                        writer.WriteStart(resource);
                        WriteStreamProperties(selectExpandNode, resourceContext, writer);
                        WriteComplexProperties(selectExpandNode, resourceContext, writer);
                        WriteDynamicComplexProperties(resourceContext, writer);
                        WriteNavigationLinks(selectExpandNode, resourceContext, writer);
                        WriteExpandedNavigationProperties(selectExpandNode, resourceContext, writer);
                        WriteReferencedNavigationProperties(selectExpandNode, resourceContext, writer);
                        writer.WriteEnd();
                    }
                }
            }
        }

        private async Task WriteResourceAsync(object graph, ODataWriter writer, ODataSerializerContext writeContext,
            IEdmTypeReference expectedType)
        {
            Contract.Assert(writeContext != null);

            if (EdmLibHelpers.IsDynamicTypeWrapper(graph.GetType()))
            {
                await WriteDynamicTypeResourceAsync(graph, writer, expectedType, writeContext);
                return;
            }

            IEdmStructuredTypeReference structuredType = GetResourceType(graph, writeContext);
            ResourceContext resourceContext = new ResourceContext(writeContext, structuredType, graph);

            SelectExpandNode selectExpandNode = CreateSelectExpandNode(resourceContext);
            if (selectExpandNode != null)
            {
                ODataResource resource = CreateResource(selectExpandNode, resourceContext);
                if (resource != null)
                {
                    if (resourceContext.SerializerContext.ExpandReference)
                    {
                        await writer.WriteEntityReferenceLinkAsync(new ODataEntityReferenceLink
                        {
                            Url = resource.Id
                        });
                    }
                    else
                    {
                        await writer.WriteStartAsync(resource);
                        await WriteStreamPropertiesAsync(selectExpandNode, resourceContext, writer);
                        await WriteComplexPropertiesAsync(selectExpandNode, resourceContext, writer);
                        await WriteDynamicComplexPropertiesAsync(resourceContext, writer);
                        await WriteNavigationLinksAsync(selectExpandNode, resourceContext, writer);
                        await WriteExpandedNavigationPropertiesAsync(selectExpandNode, resourceContext, writer);
                        await WriteReferencedNavigationPropertiesAsync(selectExpandNode, resourceContext, writer);
                        await writer.WriteEndAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Write the navigation link for the select navigation properties.
        /// </summary>
        private void WriteNavigationLinks(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(selectExpandNode != null);
            Contract.Assert(resourceContext != null);

            if (selectExpandNode.SelectedNavigationProperties == null)
            {
                return;
            }

            IEnumerable<ODataNestedResourceInfo> navigationLinks = CreateNavigationLinks(selectExpandNode.SelectedNavigationProperties, resourceContext);
            foreach (ODataNestedResourceInfo navigationLink in navigationLinks)
            {
                writer.WriteStart(navigationLink);
                writer.WriteEnd();
            }
        }

        /// <summary>
        /// Asynchronously write the navigation link for the select navigation properties.
        /// </summary>
        private async Task WriteNavigationLinksAsync(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(selectExpandNode != null);
            Contract.Assert(resourceContext != null);

            if (selectExpandNode.SelectedNavigationProperties == null)
            {
                return;
            }

            IEnumerable<ODataNestedResourceInfo> navigationLinks = CreateNavigationLinks(selectExpandNode.SelectedNavigationProperties, resourceContext);
            foreach (ODataNestedResourceInfo navigationLink in navigationLinks)
            {
                await writer.WriteStartAsync(navigationLink);
                await writer.WriteEndAsync();
            }
        }

        private void WriteDynamicComplexProperties(ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(resourceContext.EdmModel != null);

            if (resourceContext.DynamicComplexProperties == null)
            {
                return;
            }

            foreach (KeyValuePair<string, object> dynamicComplexProperty in resourceContext.DynamicComplexProperties)
            {
                // If the dynamic property is "null", it should be treated ahead by creating an ODataProperty with ODataNullValue.
                // However, it's safety here to skip the null dynamic property.
                if (String.IsNullOrEmpty(dynamicComplexProperty.Key) || dynamicComplexProperty.Value == null)
                {
                    continue;
                }

                IEdmTypeReference edmTypeReference =
                    resourceContext.SerializerContext.GetEdmType(dynamicComplexProperty.Value,
                        dynamicComplexProperty.Value.GetType());

                if (edmTypeReference.IsStructured() ||
                    (edmTypeReference.IsCollection() && edmTypeReference.AsCollection().ElementType().IsStructured()))
                {
                    ODataNestedResourceInfo nestedResourceInfo
                       = CreateDynamicComplexNestedResourceInfo(dynamicComplexProperty.Key, dynamicComplexProperty.Value, edmTypeReference, resourceContext);
                    if (nestedResourceInfo != null)
                    {
                        writer.WriteStart(nestedResourceInfo);
                        WriteDynamicComplexProperty(dynamicComplexProperty.Value, edmTypeReference, resourceContext, writer);
                        writer.WriteEnd();
                    }
                }
            }
        }

        private async Task WriteDynamicComplexPropertiesAsync(ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(resourceContext.EdmModel != null);

            if (resourceContext.DynamicComplexProperties == null)
            {
                return;
            }

            foreach (KeyValuePair<string, object> dynamicComplexProperty in resourceContext.DynamicComplexProperties)
            {
                // If the dynamic property is "null", it should be treated ahead by creating an ODataProperty with ODataNullValue.
                // However, it's safety here to skip the null dynamic property.
                if (String.IsNullOrEmpty(dynamicComplexProperty.Key) || dynamicComplexProperty.Value == null)
                {
                    continue;
                }

                IEdmTypeReference edmTypeReference =
                    resourceContext.SerializerContext.GetEdmType(dynamicComplexProperty.Value,
                        dynamicComplexProperty.Value.GetType());

                if (edmTypeReference.IsStructured() ||
                    (edmTypeReference.IsCollection() && edmTypeReference.AsCollection().ElementType().IsStructured()))
                {
                    ODataNestedResourceInfo nestedResourceInfo
                        = CreateDynamicComplexNestedResourceInfo(dynamicComplexProperty.Key, dynamicComplexProperty.Value, edmTypeReference, resourceContext);
                    if (nestedResourceInfo != null)
                    {
                        await writer.WriteStartAsync(nestedResourceInfo);
                        await WriteDynamicComplexPropertyAsync(dynamicComplexProperty.Value, edmTypeReference, resourceContext, writer);
                        await writer.WriteEndAsync();
                    }
                }
            }
        }

        private void WriteDynamicComplexProperty(object propertyValue, IEdmTypeReference edmType, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            // If the dynamic property is "null", it should be treated ahead by creating an ODataProperty with ODataNullValue.
            Contract.Assert(propertyValue != null);

            // Create the serializer context for the nested and expanded item.
            ODataSerializerContext nestedWriteContext = new ODataSerializerContext(resourceContext, null, null);

            // Write object.
            ODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(edmType);
            if (serializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeSerialized, edmType.ToTraceString()));
            }

            serializer.WriteObjectInline(propertyValue, edmType, writer, nestedWriteContext);
        }

        private Task WriteDynamicComplexPropertyAsync(object propertyValue, IEdmTypeReference edmType, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            // If the dynamic property is "null", it should be treated ahead by creating an ODataProperty with ODataNullValue.
            Contract.Assert(propertyValue != null);

            // Create the serializer context for the nested and expanded item.
            ODataSerializerContext nestedWriteContext = new ODataSerializerContext(resourceContext, null, null);

            // Write object.
            ODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(edmType);
            if (serializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeSerialized, edmType.ToTraceString()));
            }

            return serializer.WriteObjectInlineAsync(propertyValue, edmType, writer, nestedWriteContext);
        }

        private void WriteComplexProperties(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(selectExpandNode != null);
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            IEnumerable<KeyValuePair<IEdmStructuralProperty, PathSelectItem>> complexProperties = GetPropertiesToWrite(selectExpandNode, resourceContext);

            foreach (KeyValuePair<IEdmStructuralProperty, PathSelectItem> selectedComplex in complexProperties)
            {
                IEdmStructuralProperty complexProperty = selectedComplex.Key;

                ODataNestedResourceInfo nestedResourceInfo = CreateComplexNestedResourceInfo(complexProperty, selectedComplex.Value, resourceContext);

                if (nestedResourceInfo != null)
                {
                    writer.WriteStart(nestedResourceInfo);
                    WriteComplexAndExpandedNavigationProperty(complexProperty, selectedComplex.Value, resourceContext, writer);
                    writer.WriteEnd();
                }
            }
        }

        private async Task WriteComplexPropertiesAsync(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(selectExpandNode != null);
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            IEnumerable<KeyValuePair<IEdmStructuralProperty, PathSelectItem>> complexProperties = GetPropertiesToWrite(selectExpandNode, resourceContext);

            foreach (KeyValuePair<IEdmStructuralProperty, PathSelectItem> selectedComplex in complexProperties)
            {
                IEdmStructuralProperty complexProperty = selectedComplex.Key;

                ODataNestedResourceInfo nestedResourceInfo = CreateComplexNestedResourceInfo(complexProperty, selectedComplex.Value, resourceContext);
                if (nestedResourceInfo != null)
                {
                    await writer.WriteStartAsync(nestedResourceInfo);
                    await WriteComplexAndExpandedNavigationPropertyAsync(complexProperty, selectedComplex.Value, resourceContext, writer);
                    await writer.WriteEndAsync();
                }
            }
        }

        private void WriteStreamProperties(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(selectExpandNode != null);
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            if (selectExpandNode.SelectedStructuralProperties != null)
            {
                IEnumerable<IEdmStructuralProperty> structuralProperties = selectExpandNode.SelectedStructuralProperties;

                foreach (IEdmStructuralProperty structuralProperty in structuralProperties)
                {
                    if (structuralProperty.Type != null && structuralProperty.Type.IsStream())
                    {
                        ODataStreamPropertyInfo property = CreateStreamProperty(structuralProperty, resourceContext);

                        if (property != null)
                        {
                            writer.WriteStart(property);
                            writer.WriteEnd();
                        }
                    }
                }
            }
        }

        private async Task WriteStreamPropertiesAsync(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(selectExpandNode != null);
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            if (selectExpandNode.SelectedStructuralProperties != null)
            {
                IEnumerable<IEdmStructuralProperty> structuralProperties = selectExpandNode.SelectedStructuralProperties;

                foreach (IEdmStructuralProperty structuralProperty in structuralProperties)
                {
                    if (structuralProperty.Type != null && structuralProperty.Type.IsStream())
                    {
                        ODataStreamPropertyInfo property = CreateStreamProperty(structuralProperty, resourceContext);

                        if (property != null)
                        {
                            await writer.WriteStartAsync(property);
                            await writer.WriteEndAsync();
                        }
                    }
                }
            }
        }

        private IEnumerable<KeyValuePair<IEdmStructuralProperty, PathSelectItem>> GetPropertiesToWrite(SelectExpandNode selectExpandNode, ResourceContext resourceContext)
        {
            IDictionary<IEdmStructuralProperty, PathSelectItem> complexProperties = selectExpandNode.SelectedComplexTypeProperties;

            if (complexProperties != null)
            {
                IEnumerable<string> changedProperties = null;

                if (resourceContext.EdmObject != null && resourceContext.EdmObject.IsDeltaResource())
                {
                    IDelta deltaObject = resourceContext.EdmObject as IDelta;
                    changedProperties = deltaObject.GetChangedPropertyNames();
                }

                foreach (KeyValuePair<IEdmStructuralProperty, PathSelectItem> complexProperty in complexProperties)
                {
                    if (changedProperties == null || changedProperties.Contains(complexProperty.Key.Name))
                    {
                        IEdmTypeReference type = complexProperty.Key?.Type;

                        if (type != null && type.IsStructured() && resourceContext.EdmModel != null)
                        {
                            Type clrType = EdmLibHelpers.GetClrType(type.AsStructured(), resourceContext.EdmModel);

                            if (clrType != null && clrType == typeof(ODataIdContainer))
                            {
                                continue;
                            }
                        }

                        yield return complexProperty;
                    }
                }
            }
        }

        private IEnumerable<KeyValuePair<IEdmNavigationProperty, Type>> GetNavigationPropertiesToWrite(SelectExpandNode selectExpandNode, ResourceContext resourceContext)
        {
            ISet<IEdmNavigationProperty> navigationProperties = selectExpandNode.SelectedNavigationProperties;

            if (navigationProperties == null)
            {
                yield break;
            }

            if (resourceContext.EdmObject is IDelta changedObject)
            {
                IEnumerable<string> changedProperties = changedObject.GetChangedPropertyNames();

                foreach (IEdmNavigationProperty navigationProperty in navigationProperties)
                {
                    if (changedProperties != null && changedProperties.Contains(navigationProperty.Name))
                    {
                        yield return new KeyValuePair<IEdmNavigationProperty, Type>(navigationProperty, typeof(IEdmChangedObject));
                    }
                }
            }
            else if (resourceContext.ResourceInstance is IDelta deltaObject)
            {
                IEnumerable<string> changedProperties = deltaObject.GetChangedPropertyNames();
                dynamic delta = deltaObject;

                foreach (IEdmNavigationProperty navigationProperty in navigationProperties)
                {
                    object obj = null;

                    if (changedProperties != null && changedProperties.Contains(navigationProperty.Name) && delta.DeltaNestedResources.TryGetValue(navigationProperty.Name, out obj))
                    {
                        if (obj != null)
                        {
                            yield return new KeyValuePair<IEdmNavigationProperty, Type>(navigationProperty, obj.GetType());
                        }
                    }
                }
            }
        }

        private void WriteExpandedNavigationProperties(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            IDictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem> navigationPropertiesToExpand = selectExpandNode.ExpandedProperties;
            if (navigationPropertiesToExpand == null)
            {
                return;
            }

            foreach (KeyValuePair<IEdmNavigationProperty, ExpandedNavigationSelectItem> navPropertyToExpand in navigationPropertiesToExpand)
            {
                IEdmNavigationProperty navigationProperty = navPropertyToExpand.Key;

                ODataNestedResourceInfo navigationLink = CreateNavigationLink(navigationProperty, resourceContext);
                if (navigationLink != null)
                {
                    writer.WriteStart(navigationLink);
                    WriteComplexAndExpandedNavigationProperty(navigationProperty, navPropertyToExpand.Value, resourceContext, writer);
                    writer.WriteEnd();
                }
            }
        }

        private async Task WriteExpandedNavigationPropertiesAsync(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            IDictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem> navigationPropertiesToExpand = selectExpandNode.ExpandedProperties;
            if (navigationPropertiesToExpand == null)
            {
                return;
            }

            foreach (KeyValuePair<IEdmNavigationProperty, ExpandedNavigationSelectItem> navPropertyToExpand in navigationPropertiesToExpand)
            {
                IEdmNavigationProperty navigationProperty = navPropertyToExpand.Key;

                ODataNestedResourceInfo navigationLink = CreateNavigationLink(navigationProperty, resourceContext);
                if (navigationLink != null)
                {
                    await writer.WriteStartAsync(navigationLink);
                    await WriteComplexAndExpandedNavigationPropertyAsync(navigationProperty, navPropertyToExpand.Value, resourceContext, writer);
                    await writer.WriteEndAsync();
                }
            }
        }

        private void WriteReferencedNavigationProperties(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> referencedPropertiesToExpand = selectExpandNode.ReferencedProperties;
            if (referencedPropertiesToExpand == null)
            {
                return;
            }

            foreach (KeyValuePair<IEdmNavigationProperty, ExpandedReferenceSelectItem> referenced in referencedPropertiesToExpand)
            {
                IEdmNavigationProperty navigationProperty = referenced.Key;

                ODataNestedResourceInfo nestedResourceInfo = CreateNavigationLink(navigationProperty, resourceContext);
                if (nestedResourceInfo != null)
                {
                    writer.WriteStart(nestedResourceInfo);
                    WriteComplexAndExpandedNavigationProperty(navigationProperty, referenced.Value, resourceContext, writer);
                    writer.WriteEnd();
                }
            }
        }

        private async Task WriteReferencedNavigationPropertiesAsync(SelectExpandNode selectExpandNode, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> referencedPropertiesToExpand = selectExpandNode.ReferencedProperties;
            if (referencedPropertiesToExpand == null)
            {
                return;
            }

            foreach (KeyValuePair<IEdmNavigationProperty, ExpandedReferenceSelectItem> referenced in referencedPropertiesToExpand)
            {
                IEdmNavigationProperty navigationProperty = referenced.Key;

                ODataNestedResourceInfo nestedResourceInfo = CreateNavigationLink(navigationProperty, resourceContext);
                if (nestedResourceInfo != null)
                {
                    await writer.WriteStartAsync(nestedResourceInfo);
                    await WriteComplexAndExpandedNavigationPropertyAsync(navigationProperty, referenced.Value, resourceContext, writer);
                    await writer.WriteEndAsync();
                }
            }
        }

        private void WriteComplexAndExpandedNavigationProperty(IEdmProperty edmProperty, SelectItem selectItem, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(edmProperty != null);
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            object propertyValue = resourceContext.GetPropertyValue(edmProperty.Name);

            if (propertyValue == null || propertyValue is NullEdmComplexObject)
            {
                if (edmProperty.Type.IsCollection())
                {
                    // A complex or navigation property whose Type attribute specifies a collection, the collection always exists,
                    // it may just be empty.
                    // If a collection of complex or entities can be related, it is represented as a JSON array. An empty
                    // collection of resources (one that contains no resource) is represented as an empty JSON array.
                    writer.WriteStart(new ODataResourceSet
                    {
                        TypeName = edmProperty.Type.FullName()
                    });
                }
                else
                {
                    // If at most one resource can be related, the value is null if no resource is currently related.
                    writer.WriteStart(resource: null);
                }

                writer.WriteEnd();
            }
            else
            {
                // create the serializer context for the complex and expanded item.
                ODataSerializerContext nestedWriteContext = new ODataSerializerContext(resourceContext, edmProperty, resourceContext.SerializerContext.QueryContext, selectItem);

                // write object.
                ODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(edmProperty.Type);
                if (serializer == null)
                {
                    throw new SerializationException(Error.Format(SRResources.TypeCannotBeSerialized, edmProperty.Type.ToTraceString()));
                }

                serializer.WriteObjectInline(propertyValue, edmProperty.Type, writer, nestedWriteContext);
            }
        }

        private async Task WriteComplexAndExpandedNavigationPropertyAsync(IEdmProperty edmProperty, SelectItem selectItem, ResourceContext resourceContext, ODataWriter writer)
        {
            Contract.Assert(edmProperty != null);
            Contract.Assert(resourceContext != null);
            Contract.Assert(writer != null);

            object propertyValue = resourceContext.GetPropertyValue(edmProperty.Name);

            if (propertyValue == null || propertyValue is NullEdmComplexObject || propertyValue is ODataIdContainer)
            {
                if (edmProperty.Type.IsCollection())
                {
                    // A complex or navigation property whose Type attribute specifies a collection, the collection always exists,
                    // it may just be empty.
                    // If a collection of complex or entities can be related, it is represented as a JSON array. An empty
                    // collection of resources (one that contains no resource) is represented as an empty JSON array.
                    await writer.WriteStartAsync(new ODataResourceSet
                    {
                        TypeName = edmProperty.Type.FullName()
                    });
                }
                else
                {
                    // If at most one resource can be related, the value is null if no resource is currently related.
                    await writer.WriteStartAsync(resource: null);
                }

                await writer.WriteEndAsync();
            }
            else
            {
                // create the serializer context for the complex and expanded item.
                ODataSerializerContext nestedWriteContext = new ODataSerializerContext(resourceContext, edmProperty, resourceContext.SerializerContext.QueryContext, selectItem);

                // write object.
                ODataEdmTypeSerializer serializer = SerializerProvider.GetEdmTypeSerializer(edmProperty.Type);
                if (serializer == null)
                {
                    throw new SerializationException(Error.Format(SRResources.TypeCannotBeSerialized, edmProperty.Type.ToTraceString()));
                }

                await serializer.WriteObjectInlineAsync(propertyValue, edmProperty.Type, writer, nestedWriteContext);
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

        private IEnumerable<ODataProperty> CreateStructuralPropertyBag(SelectExpandNode selectExpandNode, ResourceContext resourceContext)
        {
            Contract.Assert(selectExpandNode != null);
            Contract.Assert(resourceContext != null);

            List<ODataProperty> properties = new List<ODataProperty>();
            if (selectExpandNode.SelectedStructuralProperties != null)
            {
                IEnumerable<IEdmStructuralProperty> structuralProperties = selectExpandNode.SelectedStructuralProperties;

                if (resourceContext.EdmObject != null && resourceContext.EdmObject.IsDeltaResource())
                {
                    IDelta deltaObject = resourceContext.EdmObject as IDelta;
                    IEnumerable<string> changedProperties = deltaObject.GetChangedPropertyNames();
                    structuralProperties = structuralProperties.Where(p => changedProperties.Contains(p.Name));
                }

                bool isDeletedEntity = resourceContext.EdmObject is EdmDeltaDeletedEntityObject;

                foreach (IEdmStructuralProperty structuralProperty in structuralProperties)
                {
                    if (structuralProperty.Type != null && structuralProperty.Type.IsStream())
                    {
                        // skip the stream property, the stream property is written in its own logic
                        continue;
                    }

                    ODataProperty property = CreateStructuralProperty(structuralProperty, resourceContext);
                    if (property == null || (isDeletedEntity && property.Value == null))
                    {
                        continue;
                    }

                    properties.Add(property);
                }
            }

            return properties;
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

        private void WriteDeltaResource(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            Contract.Assert(writeContext != null);

            ResourceContext resourceContext = GetResourceContext(graph, writeContext);
            SelectExpandNode selectExpandNode = CreateSelectExpandNode(resourceContext);
            if (selectExpandNode != null)
            {
                ODataResource resource = CreateResource(selectExpandNode, resourceContext);

                if (resource != null)
                {
                    writer.WriteStart(resource);
                    WriteDeltaComplexProperties(selectExpandNode, resourceContext, writer);
                    WriteDeltaNavigationProperties(selectExpandNode, resourceContext, writer);
                    writer.WriteEnd();
                }
            }
        }

        private async Task WriteDeltaResourceAsync(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            ResourceContext resourceContext = GetResourceContext(graph, writeContext);
            SelectExpandNode selectExpandNode = CreateSelectExpandNode(resourceContext);
            if (selectExpandNode != null)
            {
                ODataResource resource = CreateResource(selectExpandNode, resourceContext);

                if (resource != null)
                {
                    await writer.WriteStartAsync(resource);
                    await WriteDeltaComplexPropertiesAsync(selectExpandNode, resourceContext, writer);
                    await WriteDeltaNavigationPropertiesAsync(selectExpandNode, resourceContext, writer);
                    await writer.WriteEndAsync();
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

            Uri baseUri = new Uri(resourceContext.InternalUrlHelper.CreateODataLink(MetadataSegment.Instance));
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

        private static IEdmStructuredType GetODataPathType(ODataSerializerContext serializerContext)
        {
            Contract.Assert(serializerContext != null);
            if (serializerContext.EdmProperty != null)
            {
                // we are in an nested complex or expanded navigation property.
                if (serializerContext.EdmProperty.Type.IsCollection())
                {
                    return serializerContext.EdmProperty.Type.AsCollection().ElementType().ToStructuredType();
                }
                else
                {
                    return serializerContext.EdmProperty.Type.AsStructured().StructuredDefinition();
                }
            }
            else
            {
                if (serializerContext.ExpandedResource != null)
                {
                    // we are in dynamic complex.
                    return null;
                }

                IEdmType edmType = null;

                // figure out the type from the navigation source
                if (serializerContext.NavigationSource != null)
                {
                    edmType = serializerContext.NavigationSource.EntityType();
                    if (edmType.TypeKind == EdmTypeKind.Collection)
                    {
                        edmType = (edmType as IEdmCollectionType).ElementType.Definition;
                    }
                }

                // figure out the type from the path.
                if (serializerContext.Path != null)
                {
                    // Note: The navigation source may be different from the path if the instance has redefined the context
                    // (for example, in a flattended delta response)
                    if (serializerContext.NavigationSource == null || serializerContext.NavigationSource == serializerContext.Path.NavigationSource)
                    {
                        edmType = serializerContext.Path.EdmType;
                        if (edmType != null && edmType.TypeKind == EdmTypeKind.Collection)
                        {
                            edmType = (edmType as IEdmCollectionType).ElementType.Definition;
                        }
                    }
                }

                return edmType as IEdmStructuredType;
            }
        }

        private IEdmStructuredTypeReference GetResourceType(object graph, ODataSerializerContext writeContext)
        {
            Contract.Assert(graph != null);

            IEdmTypeReference edmType = writeContext.GetEdmType(graph, graph.GetType());
            Contract.Assert(edmType != null);

            if (!edmType.IsStructured())
            {
                throw new SerializationException(
                    Error.Format(SRResources.CannotWriteType, GetType().Name, edmType.FullName()));
            }

            return edmType.AsStructured();
        }
    }
}
