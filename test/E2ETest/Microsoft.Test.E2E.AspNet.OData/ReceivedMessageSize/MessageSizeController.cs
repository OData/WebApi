// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.ReceivedMessageSize
{
    public class MessageSizeItemsController : InMemoryODataController<MessageSizeItem, int>
    {
        public MessageSizeItemsController()
            : base("Id")
        {
        }
    }
}
