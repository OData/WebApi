// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Parses an OData path as an <see cref="ODataPath"/> and converts an <see cref="ODataPath"/> into an OData link.
    /// </summary>
    public class DefaultODataPathHandler : IODataPathHandler
    {
        /// <summary>
        /// Parses the specified OData path as an <see cref="ODataPath"/> that contains additional information about the EDM type and entity set for the path.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="odataPath">The OData path to parse.</param>
        /// <returns>A parsed representation of the path, or <c>null</c> if the path does not match the model.</returns>
        public virtual ODataPath Parse(IEdmModel model, string odataPath)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            List<ODataPathSegment> pathSegments = new List<ODataPathSegment>();
            ODataPathSegment pathSegment = null;
            IEdmType previousEdmType = null;
            foreach (string segment in ParseSegments(odataPath))
            {
                pathSegment = ParseNextSegment(model, pathSegment, previousEdmType, segment);

                // If the Uri stops matching the model at any point, return null
                if (pathSegment == null)
                {
                    return null;
                }

                pathSegments.Add(pathSegment);
                previousEdmType = pathSegment.GetEdmType(previousEdmType);
            }
            return new ODataPath(pathSegments);
        }

        /// <summary>
        /// Parses the OData path into segments.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <returns>The segments of the OData path.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly")]
        protected internal virtual IEnumerable<string> ParseSegments(string odataPath)
        {
            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            string[] segments = odataPath.Split('/');

            foreach (string segment in segments)
            {
                int startIndex = 0;
                int openParensIndex = 0;
                bool insideParens = false;
                for (int i = 0; i < segment.Length; i++)
                {
                    switch (segment[i])
                    {
                        case '(':
                            openParensIndex = i;
                            insideParens = true;
                            break;
                        case ')':
                            if (insideParens)
                            {
                                if (openParensIndex > startIndex)
                                {
                                    yield return segment.Substring(startIndex, openParensIndex - startIndex);
                                }
                                if (i > openParensIndex + 1)
                                {
                                    // yield parentheses substring if there are any characters inside the parentheses
                                    yield return segment.Substring(openParensIndex, (i + 1) - openParensIndex);
                                }
                                startIndex = i + 1;
                                insideParens = false;
                            }
                            break;
                    }
                }

                if (startIndex < segment.Length)
                {
                    yield return segment.Substring(startIndex);
                }
            }
        }

        /// <summary>
        /// Parses the next OData path segment.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseNextSegment(IEdmModel model, ODataPathSegment previous, IEdmType previousEdmType, string segment)
        {
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }

            if (previous == null)
            {
                // Parse entry node
                return ParseEntrySegment(model, segment);
            }
            else
            {
                // Parse non-entry node
                if (previousEdmType == null)
                {
                    throw new ODataException(Error.Format(SRResources.InvalidPathSegment, segment, previous));
                }

                switch (previousEdmType.TypeKind)
                {
                    case EdmTypeKind.Collection:
                        return ParseAtCollection(model, previous, previousEdmType, segment);

                    case EdmTypeKind.Entity:
                        return ParseAtEntity(model, previous, previousEdmType, segment);

                    case EdmTypeKind.Complex:
                        return ParseAtComplex(model, previous, previousEdmType, segment);

                    case EdmTypeKind.Primitive:
                        return ParseAtPrimitiveProperty(model, previous, previousEdmType, segment);

                    default:
                        throw new ODataException(Error.Format(SRResources.InvalidPathSegment, segment, previous));
                }
            }
        }

        /// <summary>
        /// Parses the first OData segment following the service base URI.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseEntrySegment(IEdmModel model, string segment)
        {
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }

            if (segment == ODataSegmentKinds.Metadata)
            {
                return new MetadataPathSegment();
            }
            if (segment == ODataSegmentKinds.Batch)
            {
                return new BatchPathSegment();
            }

            IEdmEntityContainer container = ExtractEntityContainer(model);
            IEdmEntitySet entitySet = container.FindEntitySet(segment);
            if (entitySet != null)
            {
                return new EntitySetPathSegment(entitySet);
            }

            IEdmFunctionImport function = container.FunctionImports().SingleOrDefault(fi => fi.Name == segment && fi.IsBindable == false);
            if (function != null)
            {
                return new ActionPathSegment(function);
            }

            // segment does not match the model
            return null;
        }

        /// <summary>
        /// Parses the next OData path segment following a collection.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseAtCollection(IEdmModel model, ODataPathSegment previous, IEdmType previousEdmType, string segment)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }

            if (previousEdmType == null)
            {
                throw Error.InvalidOperation(SRResources.PreviousSegmentEdmTypeCannotBeNull);
            }

            IEdmCollectionType collection = previousEdmType as IEdmCollectionType;
            if (collection == null)
            {
                throw Error.Argument(SRResources.PreviousSegmentMustBeCollectionType, previousEdmType);
            }

            switch (collection.ElementType.Definition.TypeKind)
            {
                case EdmTypeKind.Entity:
                    return ParseAtEntityCollection(model, previous, previousEdmType, segment);

                default:
                    throw new ODataException(Error.Format(SRResources.InvalidPathSegment, segment, previous));
            }
        }

        /// <summary>
        /// Parses the next OData path segment following a complex-typed segment.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseAtComplex(IEdmModel model, ODataPathSegment previous, IEdmType previousEdmType, string segment)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }

            IEdmComplexType previousType = previousEdmType as IEdmComplexType;
            if (previousType == null)
            {
                throw Error.Argument(SRResources.PreviousSegmentMustBeComplexType, previousEdmType);
            }

            // look for properties
            IEdmProperty property = previousType.Properties().SingleOrDefault(p => p.Name == segment);
            if (property != null)
            {
                return new PropertyAccessPathSegment(property);
            }

            // Treating as an open property
            return new UnresolvedPathSegment(segment);
        }

        /// <summary>
        /// Parses the next OData path segment following an entity collection.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseAtEntityCollection(IEdmModel model, ODataPathSegment previous, IEdmType previousEdmType, string segment)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }

            if (previousEdmType == null)
            {
                throw Error.InvalidOperation(SRResources.PreviousSegmentEdmTypeCannotBeNull);
            }
            IEdmCollectionType collectionType = previousEdmType as IEdmCollectionType;
            if (collectionType == null)
            {
                throw Error.Argument(SRResources.PreviousSegmentMustBeEntityCollectionType, previousEdmType);
            }
            IEdmEntityType elementType = collectionType.ElementType.Definition as IEdmEntityType;
            if (elementType == null)
            {
                throw Error.Argument(SRResources.PreviousSegmentMustBeEntityCollectionType, previousEdmType);
            }

            // look for keys first.
            if (segment.StartsWith("(", StringComparison.Ordinal) && segment.EndsWith(")", StringComparison.Ordinal))
            {
                Contract.Assert(segment.Length >= 2);
                string value = segment.Substring(1, segment.Length - 2);
                return new KeyValuePathSegment(value);
            }

            // next look for casts
            IEdmEntityType castType = model.FindDeclaredType(segment) as IEdmEntityType;
            if (castType != null)
            {
                IEdmType previousElementType = collectionType.ElementType.Definition;
                if (!castType.IsOrInheritsFrom(previousElementType) && !previousElementType.IsOrInheritsFrom(castType))
                {
                    throw new ODataException(Error.Format(SRResources.InvalidCastInPath, castType, previousElementType));
                }
                return new CastPathSegment(castType);
            }

            // now look for bindable actions
            IEdmEntityContainer container = ExtractEntityContainer(model);
            IEdmFunctionImport procedure = container.FunctionImports().FindBindableAction(collectionType, segment);
            if (procedure != null)
            {
                return new ActionPathSegment(procedure);
            }

            throw new ODataException(Error.Format(SRResources.NoActionFoundForCollection, segment, collectionType.ElementType));
        }

        /// <summary>
        /// Parses the next OData path segment following a primitive property.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseAtPrimitiveProperty(IEdmModel model, ODataPathSegment previous, IEdmType previousEdmType, string segment)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }

            if (segment == ODataSegmentKinds.Value)
            {
                return new ValuePathSegment();
            }

            throw new ODataException(Error.Format(SRResources.InvalidPathSegment, segment, previous));
        }

        /// <summary>
        /// Parses the next OData path segment following an entity.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseAtEntity(IEdmModel model, ODataPathSegment previous, IEdmType previousEdmType, string segment)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }
            IEdmEntityType previousType = previousEdmType as IEdmEntityType;
            if (previousType == null)
            {
                throw Error.Argument(SRResources.PreviousSegmentMustBeEntityType, previousEdmType);
            }

            if (segment == ODataSegmentKinds.Links)
            {
                return new LinksPathSegment();
            }

            // first look for navigation properties
            IEdmNavigationProperty navigation = previousType.NavigationProperties().SingleOrDefault(np => np.Name == segment);
            if (navigation != null)
            {
                return new NavigationPathSegment(navigation);
            }

            // next look for properties
            IEdmProperty property = previousType.Properties().SingleOrDefault(p => p.Name == segment);
            if (property != null)
            {
                return new PropertyAccessPathSegment(property);
            }

            // next look for type casts
            IEdmEntityType castType = model.FindDeclaredType(segment) as IEdmEntityType;
            if (castType != null)
            {
                if (!castType.IsOrInheritsFrom(previousType) && !previousType.IsOrInheritsFrom(castType))
                {
                    throw new ODataException(Error.Format(SRResources.InvalidCastInPath, castType, previousType));
                }
                return new CastPathSegment(castType);
            }

            // finally look for bindable procedures
            IEdmEntityContainer container = ExtractEntityContainer(model);
            IEdmFunctionImport procedure = container.FunctionImports().FindBindableAction(previousType, segment);
            if (procedure != null)
            {
                return new ActionPathSegment(procedure);
            }

            // Treating as an open property
            return new UnresolvedPathSegment(segment);
        }

        private static IEdmEntityContainer ExtractEntityContainer(IEdmModel model)
        {
            IEnumerable<IEdmEntityContainer> containers = model.EntityContainers();
            int containerCount = containers.Count();
            if (containerCount != 1)
            {
                throw Error.Argument("model", SRResources.ParserModelMustHaveOneContainer, containerCount);
            }
            return containers.Single();
        }

        /// <summary>
        /// Converts an instance of <see cref="ODataPath" /> into an OData link.
        /// </summary>
        /// <param name="path">The OData path to convert into a link.</param>
        /// <returns>
        /// The generated OData link.
        /// </returns>
        public virtual string Link(ODataPath path)
        {
            return path.ToString();
        }
    }
}