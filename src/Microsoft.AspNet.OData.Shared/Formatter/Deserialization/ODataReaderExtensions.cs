// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
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
                ReadCollectionItem(reader, itemsStack, ref topLevelItem);
            }

            Contract.Assert(reader.State == ODataReaderState.Completed, "We should have consumed all of the input by now.");
            Contract.Assert(topLevelItem != null, "A top level resource or resource set should have been read by now.");

            return topLevelItem;
        }

        /// <summary>
        /// Reads a <see cref="ODataResource"/> or <see cref="ODataResourceSet"/> object.
        /// </summary>
        /// <param name="reader">The OData reader to read from.</param>
        /// <returns>The read resource or resource set.</returns>
        public static async Task<ODataItemBase> ReadResourceOrResourceSetAsync(this ODataReader reader)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }

            ODataItemBase topLevelItem = null;
            Stack<ODataItemBase> itemsStack = new Stack<ODataItemBase>();

            while (await reader.ReadAsync())
            {
                ReadCollectionItem(reader, itemsStack, ref topLevelItem);
            }

            Contract.Assert(reader.State == ODataReaderState.Completed, "We should have consumed all of the input by now.");
            Contract.Assert(topLevelItem != null, "A top level resource or resource set should have been read by now.");

            return topLevelItem;
        }

        private static void ReadCollectionItem(ODataReader reader, Stack<ODataItemBase> itemsStack, ref ODataItemBase topLevelItem)
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
                        AddResourceToParent(itemsStack, resourceWrapper);
                    }

                    itemsStack.Push(resourceWrapper);
                    break;

                case ODataReaderState.ResourceEnd:
                case ODataReaderState.DeletedResourceEnd:
                    Contract.Assert(
                        itemsStack.Count > 0 && (reader.Item == null || itemsStack.Peek().Item == reader.Item),
                        "The resource which is ending should be on the top of the items stack.");
                    itemsStack.Pop();
                    break;
                case ODataReaderState.DeletedResourceStart:
                    ODataDeletedResource deletedResource = (ODataDeletedResource)reader.Item;
                    Contract.Assert(deletedResource != null, "Deleted resource should not be null");

                    ODataResourceWrapper deletedResourceWrapper = new ODataResourceWrapper(deletedResource);

                    Contract.Assert(itemsStack.Count != 0, "Deleted Resource should not be top level item");

                    AddResourceToParent(itemsStack, deletedResourceWrapper);

                    itemsStack.Push(deletedResourceWrapper);
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
                        AddNestedResourceInfo(itemsStack, resourceSetWrapper);
                    }
                    else
                    {
                        topLevelItem = resourceSetWrapper;
                    }

                    itemsStack.Push(resourceSetWrapper);
                    break;

                case ODataReaderState.ResourceSetEnd:
                case ODataReaderState.DeltaResourceSetEnd:
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
                case ODataReaderState.DeltaResourceSetStart:
                    ODataDeltaResourceSet deltaResourceSet = (ODataDeltaResourceSet)reader.Item;
                    Contract.Assert(deltaResourceSet != null, "ResourceSet should never be null.");

                    ODataDeltaResourceSetWrapper deltaResourceSetWrapper = new ODataDeltaResourceSetWrapper(deltaResourceSet);

                    if (itemsStack.Count > 0)
                    {
                        AddNestedResourceInfo(itemsStack, deltaResourceSetWrapper);
                    }
                    else
                    {
                        topLevelItem = deltaResourceSetWrapper;
                    }

                    itemsStack.Push(deltaResourceSetWrapper);
                    break;

                case ODataReaderState.DeltaLink:
                case ODataReaderState.DeltaDeletedLink:

                    //Throw error if Delta Link appears
                    throw Error.NotSupported(SRResources.DeltaLinkNotSupported);
                    
                default:
                    Contract.Assert(false, "We should never get here, it means the ODataReader reported a wrong state.");
                    break;
            }
        }


        private static void AddNestedResourceInfo(Stack<ODataItemBase> itemsStack, ODataResourceSetWrapperBase resourceSetWrapper)
        {
            ODataNestedResourceInfoWrapper parentNestedResourceInfo = (ODataNestedResourceInfoWrapper)itemsStack.Peek();
            Contract.Assert(parentNestedResourceInfo != null, "this has to be an inner resource set. inner resource sets always have a nested resource info.");
            Contract.Assert(parentNestedResourceInfo.NestedResourceInfo.IsCollection == true, "Only collection nested properties can contain resource set as their child.");
            parentNestedResourceInfo.NestedItems.Add(resourceSetWrapper);
        }

        private static void AddResourceToParent(Stack<ODataItemBase> itemsStack, ODataResourceWrapper resourceWrapper)
        {
            ODataItemBase parentItem = itemsStack.Peek();
            ODataResourceSetWrapperBase parentResourceSet = parentItem as ODataResourceSetWrapperBase;
            if (parentResourceSet != null)
            {
                parentResourceSet.Resources.Add(resourceWrapper);
            }
            else
            {
                ODataNestedResourceInfoWrapper parentNestedResource = (ODataNestedResourceInfoWrapper)parentItem;                
                parentNestedResource.NestedItems.Add(resourceWrapper);
            }
        }
    }
}
