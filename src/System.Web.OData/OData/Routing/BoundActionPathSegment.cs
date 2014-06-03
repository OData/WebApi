// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Validation;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a bound action invocation.
    /// </summary>
    public class BoundActionPathSegment : ODataPathSegment
    {
        private readonly IEdmModel _edmModel;

        internal BoundActionPathSegment(IEdmAction action, IEdmModel model)
        {
            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            Action = action;
            ActionName = Action.FullName();
            _edmModel = model;
        }

        internal BoundActionPathSegment(string actionName)
        {
            Contract.Assert(!String.IsNullOrEmpty(actionName));

            ActionName = actionName;
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.Action;
            }
        }

        /// <summary>
        /// Gets the action being invoked.
        /// </summary>
        public IEdmAction Action { get; private set; }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        public string ActionName { get; private set; }

        /// <inheritdoc/>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            if (Action != null)
            {
                IEdmTypeReference returnType = Action.ReturnType;
                if (returnType != null)
                {
                    return returnType.Definition;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override IEdmNavigationSource GetNavigationSource(IEdmNavigationSource previousNavigationSource)
        {
            if (_edmModel == null)
            {
                return null;
            }

            // Try to use the entity set annotation to get the target navigation source.
            ReturnedEntitySetAnnotation entitySetAnnotation =
                    _edmModel.GetAnnotationValue<ReturnedEntitySetAnnotation>(Action);

            if (entitySetAnnotation != null)
            {
                return _edmModel.EntityContainer.FindEntitySet(entitySetAnnotation.EntitySetName);
            }

            // Try to use the entity set path to get the target navigation source.
            if (previousNavigationSource != null && Action != null)
            {
                IEdmOperationParameter parameter;
                IEnumerable<IEdmNavigationProperty> navigationProperties;
                IEdmEntityType lastEntityType;
                IEnumerable<EdmError> errors;

                if (Action.TryGetRelativeEntitySetPath(_edmModel, out parameter, out navigationProperties,
                    out lastEntityType, out errors))
                {
                    IEdmNavigationSource targetNavigationSource = previousNavigationSource;
                    foreach (IEdmNavigationProperty navigationProperty in navigationProperties)
                    {
                        targetNavigationSource = targetNavigationSource.FindNavigationTarget(navigationProperty);
                        if (targetNavigationSource == null)
                        {
                            return null;
                        }
                    }

                    return targetNavigationSource;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ActionName;
        }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.Action)
            {
                BoundActionPathSegment actionSegment = (BoundActionPathSegment)pathSegment;
                return actionSegment.Action == Action && actionSegment.ActionName == ActionName;
            }

            return false;
        }
    }
}
