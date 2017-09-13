// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Client;
using WebStack.QA.Test.OData.Common;

namespace WebStack.QA.Test.OData.Formatter
{
    public interface IODataFormatterTestBase
    {
        DataServiceContext ReaderClient(Uri serviceRoot, ODataProtocolVersion protocolVersion);
        DataServiceContext WriterClient(Uri serviceRoot, ODataProtocolVersion protocolVersion);
    }

    public class ODataFormatterTestBase : ODataTestBase, IODataFormatterTestBase
    {
        public virtual DataServiceContext ReaderClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            //By default reader uses the same configuration as writer. Reading is a more important scenario than writing
            //so this configuration allows for partial support for reading while using a standard configuration for writing.
            return WriterClient(serviceRoot, protocolVersion);
        }

        public virtual DataServiceContext WriterClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            DataServiceContext ctx = new DataServiceContext(serviceRoot, protocolVersion);
            return ctx;
        }
    }
}
