using System.Collections.Generic;

namespace System.Web.Mvc
{
    public interface IFilterProvider
    {
        IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor);
    }
}
