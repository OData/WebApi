// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a bound action invocation.
    /// </summary>
    public class BoundActionPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundActionPathSegment" /> class.
        /// </summary>
        /// <param name="action">The action being invoked.</param>
        public BoundActionPathSegment(IEdmAction action)
        {
            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            Action = action;
            ActionName = Action.FullName();
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
            // For bound action, the previous navigation source is the bounding navigation source.
            return previousNavigationSource;
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
