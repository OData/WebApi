// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    ///  Validator an OData path by walking throw all its segments.
    /// </summary>
    public class DefaultODataPathValidator : PathSegmentHandler
    {
        private readonly IEdmModel _edmModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataPathValidator"/> class.
        /// </summary>
        public DefaultODataPathValidator(IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            _edmModel = model;
        }

        /// <summary>
        /// Handle a EntitySetSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(EntitySetSegment segment)
        {
        }

        /// <summary>
        /// Handle a KeySegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(KeySegment segment)
        {
        }

        /// <summary>
        /// Handle a NavigationPropertyLinkSegment
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(NavigationPropertyLinkSegment segment)
        {
            if (EdmLibHelpers.IsNotNavigable(segment.NavigationProperty, _edmModel))
            {
                throw new ODataException(Error.Format(
                    SRResources.NotNavigablePropertyUsedInNavigation,
                    segment.NavigationProperty.Name));
            }
        }

        /// <summary>
        /// Handle a NavigationPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(NavigationPropertySegment segment)
        {
            if (EdmLibHelpers.IsNotNavigable(segment.NavigationProperty, _edmModel))
            {
                throw new ODataException(Error.Format(
                    SRResources.NotNavigablePropertyUsedInNavigation,
                    segment.NavigationProperty.Name));
            }
        }

        /// <summary>
        /// Handle a OpenPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(DynamicPathSegment segment)
        {
        }

        /// <summary>
        /// Handle a OperationImportSegment
        /// </summary>
        /// <param name="segment">the segment to Handle</param>
        public override void Handle(OperationImportSegment segment)
        {
        }

        /// <summary>
        /// Handle an OperationSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(OperationSegment segment)
        {
        }

        /// <summary>
        /// Handle a PropertySegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(PathTemplateSegment segment)
        {
            string value;
            switch (segment.TranslatePathTemplateSegment(out value))
            {
                case ODataSegmentKinds.DynamicProperty:
                    break;
                default:
                    throw new ODataException(Error.Format(
                        SRResources.InvalidAttributeRoutingTemplateSegment,
                        segment.LiteralText));
            }
        }

        /// <summary>
        /// Handle a PropertySegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(PropertySegment segment)
        {
        }

        /// <summary>
        /// Handle a SingletonSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(SingletonSegment segment)
        {
        }

        /// <summary>
        /// Handle a TypeSegment, we use "cast" for type segment.
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(TypeSegment segment)
        {
        }

        /// <summary>
        /// Handle a ValueSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(ValueSegment segment)
        {
        }

        /// <summary>
        /// Handle a CountSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(CountSegment segment)
        {
        }

        /// <summary>
        /// Handle a BatchSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(BatchSegment segment)
        {
        }

        /// <summary>
        /// Handle a MetadataSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override void Handle(MetadataSegment segment)
        {
        }

        /// <summary>
        /// Handle a UnresolvedPathSegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public virtual void Handle(UnresolvedPathSegment segment)
        {
        }
    }
}
