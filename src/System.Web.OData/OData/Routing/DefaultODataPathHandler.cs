// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Parses an OData path as an <see cref="ODataPath"/> and converts an <see cref="ODataPath"/> into an OData link.
    /// </summary>
    public class DefaultODataPathHandler : IODataPathHandler, IODataPathTemplateHandler
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

            Queue<string> segments = new Queue<string>(ParseSegments(odataPath));

            while (segments.Count > 0)
            {
                string nextSegment = segments.Dequeue();

                // ignore empty parenthesis
                if (FunctionResolver.IsEnclosedInParentheses(nextSegment) &&
                    String.IsNullOrWhiteSpace(nextSegment.Substring(1, nextSegment.Length - 2)))
                {
                    continue;
                }

                pathSegment = ParseNextSegment(model, pathSegment, previousEdmType, nextSegment, segments);

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
        /// Parses the specified OData path template as an <see cref="ODataPathTemplate"/> that can be matched to an <see cref="ODataPath"/>.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="odataPathTemplate">The OData path template to parse.</param>
        /// <returns>A parsed representation of the path template, or <c>null</c> if the path does not match the model.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly")]
        public virtual ODataPathTemplate ParseTemplate(IEdmModel model, string odataPathTemplate)
        {
            return Templatify(Parse(model, odataPathTemplate), odataPathTemplate);
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
                                yield return segment.Substring(openParensIndex, (i + 1) - openParensIndex);
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
        /// <param name="segments">The queue of pending segments.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseNextSegment(IEdmModel model, ODataPathSegment previous,
            IEdmType previousEdmType, string segment, Queue<string> segments)
        {
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }

            if (previous == null)
            {
                // Parse entry node
                return ParseEntrySegment(model, segment, segments);
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
                        return ParseAtCollection(model, previous, previousEdmType, segment, segments);

                    case EdmTypeKind.Entity:
                        return ParseAtEntity(model, previous, previousEdmType, segment, segments);

                    case EdmTypeKind.Complex:
                        return ParseAtComplex(model, previous, previousEdmType, segment, segments);

                    case EdmTypeKind.Primitive:
                        return ParseAtPrimitiveProperty(model, previous, previousEdmType, segment, segments);

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
        /// <param name="segments">The queue of pending segments.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseEntrySegment(IEdmModel model, string segment, Queue<string> segments)
        {
            if (segments == null)
            {
                throw Error.ArgumentNull("segments");
            }
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

            IEdmActionImport action = container.FindAction(segment, bindingParameterType: null);
            if (action != null)
            {
                return new ActionPathSegment(action);
            }

            // Try to match this to a function call
            FunctionPathSegment pathSegment = TryMatchFunctionCall(segment, segments, model, bindingType: null);
            if (pathSegment != null)
            {
                return pathSegment;
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
        /// <param name="segments">The queue of pending segments.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseAtCollection(IEdmModel model, ODataPathSegment previous,
            IEdmType previousEdmType, string segment, Queue<string> segments)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (segments == null)
            {
                throw Error.ArgumentNull("segments");
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
                    return ParseAtEntityCollection(model, previous, previousEdmType, segment, segments);

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
        /// <param name="segments">The queue of pending segments.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseAtComplex(IEdmModel model, ODataPathSegment previous,
            IEdmType previousEdmType, string segment, Queue<string> segments)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (segments == null)
            {
                throw Error.ArgumentNull("segments");
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
        /// <param name="segments">The queue of pending segments.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseAtEntityCollection(IEdmModel model, ODataPathSegment previous,
            IEdmType previousEdmType, string segment, Queue<string> segments)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (segments == null)
            {
                throw Error.ArgumentNull("segments");
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
            IEdmActionImport action = container.FindAction(segment, collectionType);
            if (action != null)
            {
                return new ActionPathSegment(action);
            }

            // Try to match this to a function call
            FunctionPathSegment pathSegment = TryMatchFunctionCall(segment, segments, model, bindingType: collectionType);
            if (pathSegment != null)
            {
                return pathSegment;
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
        /// <param name="segments">The queue of pending segments.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseAtPrimitiveProperty(IEdmModel model, ODataPathSegment previous,
            IEdmType previousEdmType, string segment, Queue<string> segments)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (segments == null)
            {
                throw Error.ArgumentNull("segments");
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
        /// <param name="segments">The queue of pending segments.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataPathSegment ParseAtEntity(IEdmModel model, ODataPathSegment previous,
            IEdmType previousEdmType, string segment, Queue<string> segments)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (segments == null)
            {
                throw Error.ArgumentNull("segments");
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
            IEdmActionImport action = container.FindAction(segment, previousType);
            if (action != null)
            {
                return new ActionPathSegment(action);
            }

            // Try to match this to a function call
            FunctionPathSegment pathSegment = TryMatchFunctionCall(segment, segments, model, bindingType: previousType);
            if (pathSegment != null)
            {
                return pathSegment;
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

        private static FunctionPathSegment TryMatchFunctionCall(string segment, Queue<string> segments, IEdmModel model,
            IEdmType bindingType)
        {
            IEdmEntityContainer container = ExtractEntityContainer(model);
            string nextSegment = segments.Count > 0 ? segments.Peek() : null;

            IEnumerable<IEdmFunctionImport> possibleFunctions = container.FindFunctions(segment, bindingType);
            FunctionPathSegment functionSegment = FunctionResolver.TryResolve(possibleFunctions, model, nextSegment);
            if (functionSegment != null && FunctionResolver.IsEnclosedInParentheses(nextSegment))
            {
                segments.Dequeue();
            }

            return functionSegment;
        }

        private static ODataPathTemplate Templatify(ODataPath path, string pathTemplate)
        {
            if (path == null)
            {
                throw new ODataException(Error.Format(SRResources.InvalidODataPathTemplate, pathTemplate));
            }

            List<ODataPathSegmentTemplate> templateSegments = new List<ODataPathSegmentTemplate>();
            foreach (ODataPathSegment pathSegment in path.Segments)
            {
                switch (pathSegment.SegmentKind)
                {
                    case ODataSegmentKinds._Unresolved:
                        throw new ODataException(
                            Error.Format(SRResources.UnresolvedPathSegmentInTemplate, pathSegment.ToString(), pathTemplate));

                    case ODataSegmentKinds._Key:
                        templateSegments.Add(new KeyValuePathSegmentTemplate(pathSegment as KeyValuePathSegment));
                        break;

                    case ODataSegmentKinds._Function:
                        templateSegments.Add(new FunctionPathSegmentTemplate(pathSegment as FunctionPathSegment));
                        break;

                    default:
                        templateSegments.Add(pathSegment);
                        break;
                }
            }

            return new ODataPathTemplate(templateSegments);
        }
    }
}