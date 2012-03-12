using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Query;

namespace System.Web.Http.Filters
{
    internal class QueryCompositionFilterProvider : IFilterProvider
    {
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor.ReturnType != null)
            {
                Type queryElementType = GetQueryElementTypeOrNull(actionDescriptor.ReturnType);
                if (queryElementType != null)
                {
                    QueryCompositionFilterAttribute filter = new QueryCompositionFilterAttribute(queryElementType, QueryValidator.Instance);
                    return new List<FilterInfo> { new FilterInfo(filter, FilterScope.Last) };
                }
            }

            return Enumerable.Empty<FilterInfo>();
        }

        private static Type GetQueryElementTypeOrNull(Type returnType)
        {
            Contract.Assert(returnType != null);

            return QueryTypeHelper.GetQueryableInterfaceInnerTypeOrNull(returnType); // IQueryable<T> => T 
        }
    }
}
