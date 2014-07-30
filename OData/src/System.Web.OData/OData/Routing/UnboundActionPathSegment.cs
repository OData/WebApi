// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing an unbound action invocation.
    /// </summary>
    public class UnboundActionPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnboundActionPathSegment" /> class.
        /// </summary>
        /// <param name="action">The action being invoked.</param>
        public UnboundActionPathSegment(IEdmActionImport action)
        {
            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            Action = action;
            ActionName = Action.Name;
        }

        // This constructor is intended for use by unit testing only.
        internal UnboundActionPathSegment(string actionName)
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
                return ODataSegmentKinds.UnboundAction;
            }
        }

        /// <summary>
        /// Gets the action being invoked.
        /// </summary>
        public IEdmActionImport Action
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        public string ActionName
        {
            get;
            private set;
        }

        /// <inheritdoc/>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            // for unbound action, the previous Edm type must be null
            if (previousEdmType != null)
            {
                throw Error.Argument("previousEdmType");
            }

            if (Action != null)
            {
                IEdmTypeReference returnType = Action.Action.ReturnType;
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
            // for unbound action, the previous navigation source must be null
            if (previousNavigationSource != null)
            {
                throw Error.Argument("previousNavigationSource");
            }

            if (Action != null)
            {
                IEdmEntitySet actionEntitySet = null;
                if (Action.TryGetStaticEntitySet(out actionEntitySet))
                {
                    return actionEntitySet;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="String" /> that represents this instance.
        /// </summary>
        /// <returns> A <see cref="String" /> to represent this instance.
        /// </returns>
        public override string ToString()
        {
            return ActionName;
        }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.UnboundAction)
            {
                UnboundActionPathSegment actionSegment = (UnboundActionPathSegment)pathSegment;
                return actionSegment.Action == Action && actionSegment.ActionName == ActionName;
            }

            return false;
        }
    }
}
