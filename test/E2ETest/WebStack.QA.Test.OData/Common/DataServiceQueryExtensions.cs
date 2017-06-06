using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.OData.Client;

namespace WebStack.QA.Test.OData.Common
{
    public static class DataServiceQueryExtensions
    {
        public static Task<IEnumerable<T>> ExecuteAsync<T>(this DataServiceQuery<T> self)
        {
            return Task<IEnumerable<T>>.Factory.FromAsync(
                self.BeginExecute,
                self.EndExecute,
                null);
        }
    }
}
