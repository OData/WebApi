// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing an action invocation.
    /// </summary>
    public class ActionPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionPathSegment" /> class.
        /// </summary>
        /// <param name="action">The action being invoked.</param>
        public ActionPathSegment(IEdmFunctionImport action)
        {
            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            Action = action;
            ActionName = Action.Container.FullName() + "." + Action.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionPathSegment" /> class.
        /// </summary>
        /// <param name="actionName">Name of the action.</param>
        public ActionPathSegment(string actionName)
        {
            if (actionName == null)
            {
                throw Error.ArgumentNull("actionName");
            }

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
        public IEdmFunctionImport Action
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

        /// <summary>
        /// Gets the EDM type for this segment.
        /// </summary>
        /// <param name="previousEdmType">The EDM type of the previous path segment.</param>
        /// <returns>
        /// The EDM type for this segment.
        /// </returns>
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

        /// <summary>
        /// Gets the entity set for this segment.
        /// </summary>
        /// <param name="previousEntitySet">The entity set of the previous path segment.</param>
        /// <returns>
        /// The entity set for this segment.
        /// </returns>
        public override IEdmEntitySet GetEntitySet(IEdmEntitySet previousEntitySet)
        {
            if (Action != null)
            {
                IEdmEntitySet functionEntitySet = null;
                if (Action.TryGetStaticEntitySet(out functionEntitySet))
                {
                    return functionEntitySet;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ActionName;
        }
    }
}
