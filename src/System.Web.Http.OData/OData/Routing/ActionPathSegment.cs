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
        /// <param name="previous">The previous segment in the path.</param>
        /// <param name="action">The action being invoked.</param>
        public ActionPathSegment(ODataPathSegment previous, IEdmFunctionImport action)
            : base(previous)
        {
            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            IEdmTypeReference returnType = action.ReturnType;
            EdmType = returnType == null ? null : returnType.Definition;

            IEdmEntitySet functionEntitySet = null;
            if (action.TryGetStaticEntitySet(out functionEntitySet))
            {
                EntitySet = functionEntitySet;
            }
            Action = action;
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Action.Container.FullName() + "." + Action.Name;
        }
    }
}
