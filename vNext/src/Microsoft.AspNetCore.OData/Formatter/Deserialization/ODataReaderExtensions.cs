// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Microsoft.OData;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{ 
    /// <summary>
    /// Extension methods for <see cref="ODataReader"/>.
    /// </summary>
    public static class ODataReaderExtensions
    {
        /// <summary>
        /// Reads a <see cref="ODataResource"/> or <see cref="ODataResourceSet"/> object.
        /// </summary>
        /// <param name="reader">The OData reader to read from.</param>
        /// <returns>The read resource or resource set.</returns>
        public static ODataItemBase ReadResourceOrResourceSet(this ODataReader reader)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }

            ODataItemBase topLevelItem = null;
            Stack<ODataItemBase> itemsStack = new Stack<ODataItemBase>();

            while (reader.Read())
            {
                switch (reader.State)
                {
                    case ODataReaderState.ResourceStart:
                        ODataResource resource = (ODataResource)reader.Item;
                        ODataResourceWrapper resourceWrapper = null;
                        if (resource != null)
                        {
                            resourceWrapper = new ODataResourceWrapper(resource);
                        }

                        if (itemsStack.Count == 0)
                        {
                            Contract.Assert(resource != null, "The top-level resource can never be null.");
                            topLevelItem = resourceWrapper;
                        }
                        else
                        {
                            ODataItemBase parentItem = itemsStack.Peek();
                            ODataResourceSetWrapper parentResourceSet = parentItem as ODataResourceSetWrapper;
                            if (parentResourceSet != null)
                            {
                                parentResourceSet.Resources.Add(resourceWrapper);
                            }
                            else
                            {
                                ODataNestedResourceInfoWrapper parentNestedResource = (ODataNestedResourceInfoWrapper)parentItem;
                                Contract.Assert(parentNestedResource.NestedResourceInfo.IsCollection == false, "Only singleton nested properties can contain resource as their child.");
                                Contract.Assert(parentNestedResource.NestedItems.Count == 0, "Each nested property can contain only one resource as its direct child.");
                                parentNestedResource.NestedItems.Add(resourceWrapper);
                            }
                        }

                        itemsStack.Push(resourceWrapper);
                        break;

                    case ODataReaderState.ResourceEnd:
                        Contract.Assert(
                            itemsStack.Count > 0 && (reader.Item == null || itemsStack.Peek().Item == reader.Item),
                            "The resource which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
                        break;

                    case ODataReaderState.NestedResourceInfoStart:
                        ODataNestedResourceInfo nestedResourceInfo = (ODataNestedResourceInfo)reader.Item;
                        Contract.Assert(nestedResourceInfo != null, "nested resource info should never be null.");

                        ODataNestedResourceInfoWrapper nestedResourceInfoWrapper = new ODataNestedResourceInfoWrapper(nestedResourceInfo);
                        Contract.Assert(itemsStack.Count > 0, "nested resource info can't appear as top-level item.");
                        {
                            ODataResourceWrapper parentResource = (ODataResourceWrapper)itemsStack.Peek();
                            parentResource.NestedResourceInfos.Add(nestedResourceInfoWrapper);
                        }

                        itemsStack.Push(nestedResourceInfoWrapper);
                        break;

                    case ODataReaderState.NestedResourceInfoEnd:
                        Contract.Assert(itemsStack.Count > 0 && itemsStack.Peek().Item == reader.Item,
                            "The nested resource info which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
                        break;

                    case ODataReaderState.ResourceSetStart:
                        ODataResourceSet resourceSet = (ODataResourceSet)reader.Item;
                        Contract.Assert(resourceSet != null, "ResourceSet should never be null.");

                        ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(resourceSet);
                        if (itemsStack.Count > 0)
                        {
                            ODataNestedResourceInfoWrapper parentNestedResourceInfo = (ODataNestedResourceInfoWrapper)itemsStack.Peek();
                            Contract.Assert(parentNestedResourceInfo != null, "this has to be an inner resource set. inner resource sets always have a nested resource info.");
                            Contract.Assert(parentNestedResourceInfo.NestedResourceInfo.IsCollection == true, "Only collection nested properties can contain resource set as their child.");
                            parentNestedResourceInfo.NestedItems.Add(resourceSetWrapper);
                        }
                        else
                        {
                            topLevelItem = resourceSetWrapper;
                        }

                        itemsStack.Push(resourceSetWrapper);
                        break;

                    case ODataReaderState.ResourceSetEnd:
                        Contract.Assert(itemsStack.Count > 0 && itemsStack.Peek().Item == reader.Item, "The resource set which is ending should be on the top of the items stack.");
                        itemsStack.Pop();
                        break;

                    case ODataReaderState.EntityReferenceLink:
                        ODataEntityReferenceLink entityReferenceLink = (ODataEntityReferenceLink)reader.Item;
                        Contract.Assert(entityReferenceLink != null, "Entity reference link should never be null.");
                        ODataEntityReferenceLinkBase entityReferenceLinkWrapper = new ODataEntityReferenceLinkBase(entityReferenceLink);

                        Contract.Assert(itemsStack.Count > 0, "Entity reference link should never be reported as top-level item.");
                        {
                            ODataNestedResourceInfoWrapper parentNavigationLink = (ODataNestedResourceInfoWrapper)itemsStack.Peek();
                            parentNavigationLink.NestedItems.Add(entityReferenceLinkWrapper);
                        }

                        break;

                    default:
                        Contract.Assert(false, "We should never get here, it means the ODataReader reported a wrong state.");
                        break;
                }
            }

            Contract.Assert(reader.State == ODataReaderState.Completed, "We should have consumed all of the input by now.");
            Contract.Assert(topLevelItem != null, "A top level resource or resource set should have been read by now.");
            return topLevelItem;
        }
    }
}
