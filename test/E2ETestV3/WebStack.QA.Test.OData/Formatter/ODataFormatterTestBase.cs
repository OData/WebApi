using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Text;
using WebStack.QA.Test.OData.Common;

namespace WebStack.QA.Test.OData.Formatter
{
    public interface IODataFormatterTestBase
    {
        DataServiceContext ReaderClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion);
        DataServiceContext WriterClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion);
    }

    public class ODataFormatterTestBase : ODataTestBase, IODataFormatterTestBase
    {
        public virtual DataServiceContext ReaderClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion)
        {
            //By default reader uses the same configuration as writer. Reading is a more important scenario than writing
            //so this configuration allows for partial support for reading while using a standard configuration for writing.
            return WriterClient(serviceRoot, protocolVersion);
        }

        public virtual DataServiceContext WriterClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion)
        {
            DataServiceContext ctx = new DataServiceContext(serviceRoot, protocolVersion);
            return ctx;
        }
    }
}
