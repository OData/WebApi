using Microsoft.AspNet.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Test.E2E.AspNet.OData.BulkInsert1
{
    public interface IBulkOperation
    {
        IEdmStructuredObject ApplyEntityOperation<T>(IEdmStructuredObject edmEntityObject, T objectType);
    }
}
