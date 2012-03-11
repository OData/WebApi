using System.Web.Http.Common;

namespace System.Web.Http.Filters
{
    public sealed class FilterInfo
    {
        public FilterInfo(IFilter instance, FilterScope scope)
        {
            if (instance == null)
            {
                throw Error.ArgumentNull("instance");
            }

            Instance = instance;
            Scope = scope;
        }

        public IFilter Instance { get; private set; }

        public FilterScope Scope { get; private set; }
    }
}
