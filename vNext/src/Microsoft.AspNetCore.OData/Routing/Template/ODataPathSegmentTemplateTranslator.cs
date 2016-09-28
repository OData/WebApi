// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Translator an OData path to a path segment templates.
    /// </summary>
    public class ODataPathSegmentTemplateTranslator : PathSegmentTranslator<ODataPathSegmentTemplate>
    {
        /// <summary>
        /// Translate a TypeSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template</returns>
        public override ODataPathSegmentTemplate Translate(TypeSegment segment)
        {
            return new TypeSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a NavigationPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(NavigationPropertySegment segment)
        {
            return new NavigationPropertySegmentTemplate(segment);
        }

        /// <summary>
        /// Translate an EntitySetSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(EntitySetSegment segment)
        {
            return new EntitySetSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate an SingletonSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(SingletonSegment segment)
        {
            return new SingletonSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a KeySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(KeySegment segment)
        {
            return new KeySegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a PropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(PropertySegment segment)
        {
            return new PropertySegmentTemplate(segment);
        }

        /// <summary>
        /// Handle a PropertySegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override ODataPathSegmentTemplate Translate(PathTemplateSegment segment)
        {
            return new PathTemplateSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a OperationImportSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(OperationImportSegment segment)
        {
            return new OperationImportSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a OperationSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(OperationSegment segment)
        {
            return new OperationSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate an OpenPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(DynamicPathSegment segment)
        {
            return new DynamicSegmentTemplate(segment);
        }

        /// <summary>
        /// Visit a NavigationPropertyLinkSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(NavigationPropertyLinkSegment segment)
        {
            return new NavigationPropertyLinkSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a CountSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(CountSegment segment)
        {
            return new ODataPathSegmentTemplate<CountSegment>();
        }

        /// <summary>
        /// Translate a ValueSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(ValueSegment segment)
        {
            return new ODataPathSegmentTemplate<ValueSegment>();
        }

        /// <summary>
        /// Translate a BatchSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(BatchSegment segment)
        {
            return new ODataPathSegmentTemplate<BatchSegment>();
        }

        /// <summary>
        /// Translate a MetadataSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(MetadataSegment segment)
        {
            return new ODataPathSegmentTemplate<MetadataSegment>();
        }

        /// <summary>
        /// Translate a BatchReferenceSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataPathSegmentTemplate Translate(BatchReferenceSegment segment)
        {
            throw new ODataException(/*TODO: Error.Format(SRResources.TargetKindNotImplemented, "ODataPathSegment", "BatchReferenceSegment")*/);
        }
    }
}
