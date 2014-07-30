// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query.SemanticAst;

namespace System.Web.Http.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="SelectExpandQueryOption" /> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class SelectExpandQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="TopQueryOption" />.
        /// </summary>
        /// <param name="selectExpandQueryOption">The $select and $expand query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(SelectExpandQueryOption selectExpandQueryOption, ODataValidationSettings validationSettings)
        {
            if (selectExpandQueryOption == null)
            {
                throw Error.ArgumentNull("selectExpandQueryOption");
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            if (validationSettings.MaxExpansionDepth > 0)
            {
                ValidateDepth(selectExpandQueryOption.SelectExpandClause, validationSettings.MaxExpansionDepth);
            }
        }

        private static void ValidateDepth(SelectExpandClause selectExpand, int maxDepth)
        {
            // do a DFS to see if there is any node that is too deep.
            Stack<Tuple<int, SelectExpandClause>> nodesToVisit = new Stack<Tuple<int, SelectExpandClause>>();
            nodesToVisit.Push(Tuple.Create(0, selectExpand));
            while (nodesToVisit.Count > 0)
            {
                Tuple<int, SelectExpandClause> tuple = nodesToVisit.Pop();
                int currentDepth = tuple.Item1;
                SelectExpandClause currentNode = tuple.Item2;

                ExpandedNavigationSelectItem[] expandItems = currentNode.SelectedItems.OfType<ExpandedNavigationSelectItem>().ToArray();
                if (expandItems.Length > 0 && currentDepth == maxDepth)
                {
                    throw new ODataException(
                        Error.Format(SRResources.MaxExpandDepthExceeded, maxDepth, "MaxExpansionDepth"));
                }

                foreach (ExpandedNavigationSelectItem expandItem in expandItems)
                {
                    nodesToVisit.Push(Tuple.Create(currentDepth + 1, expandItem.SelectAndExpand));
                }
            }
        }
    }
}
