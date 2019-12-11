// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    internal static class SelectExpandPathExtensions
    {
        /// <summary>
        /// Verify the $select path and gets the first non type cast segment in a select path.
        /// For example: $select=NS.SubType1/abc/NS.SubType2/xyz
        /// => firstPropertySegment: "abc"
        /// => remainingSegments:  NS.SubType2/xyz
        /// </summary>
        /// <param name="selectPath">The input $select path.</param>
        /// <param name="remainingSegments">The remaining segments after the first non type segment.</param>
        /// <returns>First non-type cast segment.</returns>
        public static ODataPathSegment GetFirstNonTypeCastSegment(this ODataSelectPath selectPath,
            out IList<ODataPathSegment> remainingSegments)
        {
            if (selectPath == null)
            {
                throw new ArgumentNullException("selectPath");
            }

            // In fact, ODataSelectPath constructor verifies the supporting segments, we add the verification here for double check.
            return GetFirstNonTypeCastSegment(selectPath,
                //The middle segment should be "TypeSegment" or "PropertySegment".
                m => m is PropertySegment || m is TypeSegment,
                // The last segment could be "NavigationPropertySegment, PropertySegment, OperationSegment, DynamicPathSegment"
                s => s is NavigationPropertySegment || s is PropertySegment || s is OperationSegment || s is DynamicPathSegment,
                out remainingSegments);
        }

        /// <summary>
        /// Verify the $expand path and gets the first non type cast segment in this expand path.
        /// For example: $expand=NS.SubType1/abc/NS.SubType2/nav
        /// => firstPropertySegment: "abc"
        /// => remainingSegments:  NS.SubType2/nav
        /// => leadingTypeSegment: NS.SubType1
        /// </summary>
        /// <param name="expandPath">The input $expand path.</param>
        /// <param name="remainingSegments">The remaining segments after the first non type segment.</param>
        /// <returns>First non-type cast segment.</returns>
        public static ODataPathSegment GetFirstNonTypeCastSegment(this ODataExpandPath expandPath,
            out IList<ODataPathSegment> remainingSegments)
        {
            if (expandPath == null)
            {
                throw new ArgumentNullException("expandPath");
            }

            // In fact, ODataExpandPath constructor verifies the supporting segments, we add the verification here for double check.
            return GetFirstNonTypeCastSegment(expandPath,
                //The middle segment should be "TypeSegment" or "PropertySegment".
                m => m is PropertySegment || m is TypeSegment,
                // The last segment could be "NavigationPropertySegment"
                s => s is NavigationPropertySegment,
                out remainingSegments);
        }

        private static ODataPathSegment GetFirstNonTypeCastSegment(ODataPath path,
            Func<ODataPathSegment, bool> middleSegmentPredicte,
            Func<ODataPathSegment, bool> lastSegmentPredicte,
            out IList<ODataPathSegment> remainingSegments) // could be null
        {
            Contract.Assert(path != null);

            remainingSegments = null;
            ODataPathSegment firstNonTypeSegment = null;
            int lastIndex = path.Count - 1;
            int index = 0;
            foreach (var segment in path)
            {
                if (index == lastIndex)
                {
                    // Last segment
                    if (!lastSegmentPredicte(segment))
                    {
                        throw new ODataException(Error.Format(SRResources.InvalidLastSegmentInSelectExpandPath, segment.GetType().Name));
                    }
                }
                else
                {
                    // middle segment
                    if (!middleSegmentPredicte(segment))
                    {
                        throw new ODataException(Error.Format(SRResources.InvalidSegmentInSelectExpandPath, segment.GetType().Name));
                    }
                }

                index++;

                if (firstNonTypeSegment != null)
                {
                    if (remainingSegments == null)
                    {
                        remainingSegments = new List<ODataPathSegment>();
                    }

                    remainingSegments.Add(segment);
                    continue;
                }

                // Theoretically, a path like:  "~/NS.BaseType/NS.SubType1/NS.SubType2/PropertyOnSubType2" is valid(?) but not allowed.
                // However, the functionality of the above path is same as "~/NS.SubType2/PropertyOnSubType2" (omit the middle type cast).
                // So, Let's only care about the last segment in the leading segments.
                // if we have the leading segments, and the last segment must be the type segment and it's verified.
                if (segment is TypeSegment)
                {
                    // do nothing here, just skip the leading type segment
                }
                else
                {
                    firstNonTypeSegment = segment;
                }
            }

            return firstNonTypeSegment;
        }
    }
}
