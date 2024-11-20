//-----------------------------------------------------------------------------
// <copyright file="ODataResourceDeserializerHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    internal static class ODataResourceDeserializerHelpers
    {
        /// <summary>
        /// It sets deleted properties on a resource.
        /// </summary>
        /// <param name="resource">The resource object.</param>
        /// <param name="deletedResource">The deleted resource.</param>
        /// <param name="isUntyped">Is it typed or not?</param>
        internal static void AppendDeletedProperties(dynamic resource, ODataDeletedResource deletedResource, bool isUntyped)
        {
            if (isUntyped)
            {
                resource.Id = deletedResource.Id.ToString();
            }
            else
            {
                resource.Id = deletedResource.Id;
            }

            if (deletedResource.Reason != null)
            {
                resource.Reason = deletedResource.Reason.Value;
            }
        }

        /// <summary>
        /// Creates a nested read context for nested resources.
        /// It ensures that the read context and the path are always correct for the level that is being read.
        /// </summary>
        /// <param name="resourceInfoWrapper">The nested resource info wrapper.</param>
        /// <param name="readContext">The read context.</param>
        /// <param name="edmProperty">The Edm property.</param>
        /// <returns>It returns the nested <see cref="ODataDeserializerContext"/> for the resource being read.</returns>
        internal static ODataDeserializerContext GenerateNestedReadContext(ODataNestedResourceInfoWrapper resourceInfoWrapper, ODataDeserializerContext readContext, IEdmProperty edmProperty)
        {
            Routing.ODataPath path = readContext.Path;

            // this code attempts to make sure that the path is always correct for the level that we are reading.
            if (edmProperty == null)
            {
                ODataNestedResourceInfo nestedResourceInfo = resourceInfoWrapper.NestedResourceInfo;
                IEdmType segmentType = null;
                string propertyTypeName = nestedResourceInfo.TypeAnnotation?.TypeName;

                if (!string.IsNullOrEmpty(propertyTypeName))
                {
                    segmentType = readContext.Model.FindType(propertyTypeName);
                }

                DynamicPathSegment pathSegment = new DynamicPathSegment(
                   nestedResourceInfo.Name,
                   segmentType,
                   null,
                   nestedResourceInfo.IsCollection != true);

                path = AppendToPath(path, pathSegment);
            }
            else
            {
                if (edmProperty.PropertyKind == EdmPropertyKind.Navigation)
                {
                    IEdmNavigationProperty navigationProperty = edmProperty as IEdmNavigationProperty;
                    IEdmNavigationSource parentNavigationSource = readContext.Path.NavigationSource;
                    IEdmPathExpression bindingPath = GetBindingPath(readContext.Path, navigationProperty);
                    IEdmNavigationSource navigationSource = parentNavigationSource?.FindNavigationTarget(navigationProperty, bindingPath);

                    if (navigationProperty.ContainsTarget || navigationSource == null || navigationSource is IEdmUnknownEntitySet)
                    {
                        path = AppendToPath(path, new NavigationPropertySegment(navigationProperty, navigationSource), navigationProperty.DeclaringType, parentNavigationSource);
                    }
                    else
                    {
                        try
                        {
                            ODataUriParser parser;
                            if (readContext != null && readContext.InternalRequest != null && readContext.InternalRequest.RequestContainer != null)
                            {
                                parser = new ODataUriParser(readContext.Model, new Uri(navigationSource.Path.Path, UriKind.Relative), readContext.InternalRequest.RequestContainer);
                            }
                            else
                            {
                                parser = new ODataUriParser(readContext.Model, new Uri(navigationSource.Path.Path, UriKind.Relative));
                            }

                            path = new Routing.ODataPath(parser.ParsePath());
                        }
                        catch (ODataException)
                        {
                            return BuildNestedContextFromCurrentContext(readContext, path);
                        }
                    }
                }
                else
                {
                    IEdmStructuralProperty structuralProperty = edmProperty as IEdmStructuralProperty;
                    path = AppendToPath(path, new PropertySegment(structuralProperty), structuralProperty.DeclaringType, null);
                }
            }

            return BuildNestedContextFromCurrentContext(readContext, path);
        }

        // Determines the binding path for an OData Path to a given navigationProperty
        private static IEdmPathExpression GetBindingPath(Routing.ODataPath path, IEdmNavigationProperty navigationProperty)
        {
            Contract.Assert(navigationProperty != null, "Called GetBindingPath with a null navigation property");
            if (path == null)
            {
                return null;
            }

            // Binding Path is made up of complex types, containment navigation properties, and type segments
            List<string> segments = new List<string>();
            foreach (ODataPathSegment segment in path.Segments)
            {
                if (segment is NavigationPropertySegment navSegment)
                {
                    segments.Add(navSegment.NavigationProperty.Name);
                }
                else if (segment is PropertySegment propertySegment)
                {
                    segments.Add(propertySegment.Property.Name);
                }
                else if (segment is TypeSegment typeSegment)
                {
                    segments.Add(typeSegment.Identifier);
                }
            }

            if(navigationProperty.DeclaringType != path.EdmType as IEdmStructuredType)
            {
                // Add a type cast segment
                segments.Add(navigationProperty.DeclaringType.FullTypeName());
            }

            segments.Add(navigationProperty.Name);

            return new EdmPathExpression(String.Join("/", segments));
        }

        /// <summary>
        /// It builds a nested deserializer context from the current deserializer context
        /// </summary>
        /// <param name="readContext">The current read context</param>
        /// <param name="path">The ODataPath.</param>
        /// <param name="resourceType">The resource type</param>
        /// <returns>The nested deserializer context.</returns>
        internal static ODataDeserializerContext BuildNestedContextFromCurrentContext(ODataDeserializerContext readContext, Routing.ODataPath path = null, Type resourceType = null)
        {
            return new ODataDeserializerContext
            {
                Path = path ?? readContext.Path,
                Model = readContext.Model,
                Request = readContext.Request,
                ResourceType = resourceType ?? readContext.ResourceType
            };
        }
        /// <summary>
        /// Appends a new segment to an ODataPath
        /// </summary>
        /// <param name="path">The ODataPath</param>
        /// <param name="segment">The ODataPath segment.</param>
        /// <returns>An ODataPath with a new segment appended to it.</returns>
        internal static Routing.ODataPath AppendToPath(Routing.ODataPath path, ODataPathSegment segment)
        {
            return AppendToPath(path, segment, null, null);
        }

        /// <summary>
        /// Appends a new segment to an ODataPath, adding a type segment if required.
        /// </summary>
        /// <param name="path">The ODataPath.</param>
        /// <param name="segment">The ODataPath segment. This includes a type segment.</param>
        /// <param name="declaringType">The declaring type of the path segment</param>
        /// <param name="navigationSource">The navigation source. </param>
        /// <returns>An ODataPath with a new segment appended to it.</returns>
        internal static Routing.ODataPath AppendToPath(Routing.ODataPath path, ODataPathSegment segment, IEdmType declaringType, IEdmNavigationSource navigationSource)
        {
            if (path?.Segments == null)
            {
                return null;
            }

            List<ODataPathSegment> segments = new List<ODataPathSegment>(path.Segments);
            IEdmType pathType = path.EdmType;

            // Append type cast segment if required
            if (declaringType != null && pathType != null && pathType != declaringType
                && declaringType.IsOrInheritsFrom(pathType.AsElementType()))
            {
                segments.Add(new TypeSegment(declaringType, pathType, navigationSource));
            }

            segments.Add(segment);

            return new Routing.ODataPath(segments);
        }

        /// <summary>
        /// It gets the ODataPath from a resource id.
        /// </summary>
        /// <param name="id">The resource id.</param>
        /// <param name="readContext">The ODataDeserializerContext.</param>
        /// <returns>The ODataPath.</returns>
        internal static ODataPath GetODataPath(string id, ODataDeserializerContext readContext)
        {
            Routing.IODataPathHandler pathHandler = readContext.InternalRequest.PathHandler;
            IWebApiRequestMessage internalRequest = readContext.InternalRequest;
            IWebApiUrlHelper urlHelper = readContext.InternalUrlHelper;
            ODataPath odataPath = null;

            if (internalRequest != null && urlHelper != null)
            {
                string serviceRoot = urlHelper.CreateODataLink(
                    internalRequest.Context.RouteName,
                    internalRequest.PathHandler,
                    new List<ODataPathSegment>());

                try
                {
                    odataPath = pathHandler.Parse(serviceRoot, id, internalRequest.RequestContainer).Path;
                }
                catch (ODataException)
                {
                    return null;
                }
            }

            return odataPath;
        }

        /// <summary>
        /// Applies ODataId to the ODataIdContainer.
        /// </summary>
        /// <param name="resource">The resource object.</param>
        /// <param name="resourceWrapper">The deserialized resource.</param>
        /// <param name="readContext">The ODataDeserilizerContext.</param>
        internal static void ApplyODataIdContainer(object resource, ODataResourceWrapper resourceWrapper,
            ODataDeserializerContext readContext)
        {
            Routing.ODataPath path = readContext.Path;

            if (path == null)
            {
                return;
            }

            if (path.EdmType.AsElementType() is IEdmEntityType entityType)
            {
                //Setting Odataid , for POCO classes, as a property in the POCO object itself(if user has OdataIDContainer property),
                //for Delta and EdmEntity object setting as an added property ODataIdContainer in those classes
                ODataPath odataPath = new ODataPath(path.Segments);

                // if there is no Id on the resource, try to compute one from path
                if (resourceWrapper.ResourceBase.Id == null)
                {
                    ODataUri odataUri = new ODataUri { Path = odataPath };
                    resourceWrapper.ResourceBase.Id = odataUri.BuildUri(ODataUrlKeyDelimiter.Parentheses);
                }

                if (resourceWrapper.ResourceBase.Id != null)
                {
                    string odataId = resourceWrapper.ResourceBase.Id.OriginalString;

                    ODataIdContainer container = new ODataIdContainer();
                    container.ODataId = odataId;

                    if (resource is EdmEntityObject edmObject)
                    {
                        edmObject.ODataIdContainer = container;
                        edmObject.ODataPath = odataPath;
                    }
                    else if (resource is IDeltaSetItem deltaSetItem)
                    {
                        deltaSetItem.ODataIdContainer = container;
                        deltaSetItem.ODataPath = odataPath;
                    }
                    else
                    {
                        // TODO: the logic to use the first 'ODataIdContainer' property as the container looks simple and error
                        PropertyInfo containerPropertyInfo = EdmLibHelpers.GetClrType(entityType, readContext.Model).GetProperties().FirstOrDefault(x => x.PropertyType == typeof(ODataIdContainer));
                        if (containerPropertyInfo != null)
                        {
                            ODataIdContainer odataIdContainer = containerPropertyInfo.GetValue(resource) as ODataIdContainer;
                            if (odataIdContainer != null)
                            {
                                containerPropertyInfo.SetValue(resource, odataIdContainer);
                            }
                            else
                            {
                                containerPropertyInfo.SetValue(resource, container);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// It parses a resource id, if it exists, as an OData Url and builds an ODataPath 
        /// from the created URL.
        /// </summary>
        /// <param name="readContext">The read context.</param>
        /// <param name="resourceWrapper">The resource wrapper.</param>
        /// <returns>An ODataPath.</returns>
        internal static Routing.ODataPath ApplyIdToPath(ODataDeserializerContext readContext, ODataResourceWrapper resourceWrapper)
        {
            // If an odata.id is provided, try to parse it as an OData Url.
            // This could fail (as the id is not required to be a valid OData Url)
            // in which case we fall back to building the path based on the current path and segments.
            if (resourceWrapper.ResourceBase.Id != null)
            {
                IWebApiRequestMessage internalRequest = readContext.InternalRequest;
                IWebApiUrlHelper urlHelper = readContext.InternalUrlHelper;
                ODataPath odataPath = null;

                if (internalRequest != null && urlHelper != null)
                {
                    string serviceRoot = urlHelper.CreateODataLink(
                        internalRequest.Context.RouteName,
                        internalRequest.PathHandler,
                        new List<ODataPathSegment>());

                    try
                    {
                        ODataUriParser parser;
                        if (internalRequest.RequestContainer != null)
                        {
                            parser = new ODataUriParser(readContext.Model, new Uri(serviceRoot), resourceWrapper.ResourceBase.Id, internalRequest.RequestContainer);
                        }
                        else
                        {
                            parser = new ODataUriParser(readContext.Model, new Uri(serviceRoot), resourceWrapper.ResourceBase.Id);
                        }

                        odataPath = parser.ParsePath();
                    }
                    catch (ODataException)
                    {
                        return null;
                    }
                    catch (UriFormatException)
                    {
                        return null;
                    }
                }

                if (odataPath != null)
                {
                    return new Routing.ODataPath(odataPath);
                }
            }

            Routing.ODataPath path = readContext.Path;

            if (path == null)
            {
                return null;
            }

            if (path.EdmType.AsElementType() is IEdmEntityType entityType && path.EdmType.TypeKind == EdmTypeKind.Collection)
            {
                // create the uri for the current object, using path and key values
                List<KeyValuePair<string, object>> keys = new List<KeyValuePair<string, object>>();
                foreach (IEdmStructuralProperty keyProperty in entityType.Key())
                {
                    string keyName = keyProperty.Name;
                    ODataProperty property = resourceWrapper.ResourceBase.Properties.FirstOrDefault(p => p.Name == keyName);

                    if (property == null && !readContext.DisableCaseInsensitiveRequestPropertyBinding)
                    {
                        //try case insensitive
                        List<ODataProperty> candidates = resourceWrapper.ResourceBase.Properties.Where(p => String.Equals(p.Name, keyName, StringComparison.OrdinalIgnoreCase)).ToList();
                        property = candidates.Count == 1 ? candidates[0] : null;
                    }

                    object keyValue = property?.Value;
                    if (keyValue == null)
                    {
                        // Note: may be null if the payload did not include key values,
                        // but still need to add the key so the path is semantically correct.
                        // Key value type is not validated, so just use empty string.
                        // Consider adding tests to ODL to ensure we don't validate key property type in future.
                        keyValue = string.Empty;
                    }

                    keys.Add(new KeyValuePair<string, object>(keyName, keyValue));
                }

                KeySegment keySegment = new KeySegment(keys, entityType, path.NavigationSource);
                return AppendToPath(path, keySegment);
            }

            return path;
        }

        /// <summary>
        /// Do uri parsing to get the key values.
        /// </summary>
        /// <param name="id">The key Id.</param>
        /// <param name="readContext">The reader context.</param>
        /// <returns>The key properties.</returns>
        internal static IList<ODataProperty> CreateKeyProperties(Uri id, ODataDeserializerContext readContext)
        {
            Contract.Assert(id != null);
            Contract.Assert(readContext != null);
            IList<ODataProperty> properties = new List<ODataProperty>();

            if (readContext.Request == null)
            {
                return properties;
            }

            ODataPath odataPath = GetODataPath(id.OriginalString, readContext);

            if (odataPath != null)
            {
                KeySegment keySegment = odataPath.OfType<KeySegment>().LastOrDefault();

                if (keySegment != null)
                {
                    foreach (KeyValuePair<string, object> key in keySegment.Keys)
                    {
                        properties.Add(new ODataProperty
                        {
                            Name = key.Key,
                            Value = key.Value
                        });
                    }
                }
            }

            return properties;
        }
    }
}
