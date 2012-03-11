using System.Collections.Generic;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    public interface IFilterProvider
    {
        IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor);
    }
}
