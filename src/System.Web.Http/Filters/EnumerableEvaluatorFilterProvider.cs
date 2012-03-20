using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;

namespace System.Web.Http.Filters
{
    internal class EnumerableEvaluatorFilterProvider : IFilterProvider
    {
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            var contentType = TypeHelper.GetUnderlyingContentInnerType(actionDescriptor.ReturnType);

            if (EnumerableEvaluatorFilter.IsSupportedDeclaredContentType(contentType))
            {
                // Register filter in FilterScope.First so that it's closest to HttpDispatcher. This means
                // the filter's "after" code path will be one of the last things to run.
                return new[] { new FilterInfo(EnumerableEvaluatorFilter.Instance, FilterScope.First) };
            }
            else
            {
                return Enumerable.Empty<FilterInfo>();
            }
        }
    }
}
