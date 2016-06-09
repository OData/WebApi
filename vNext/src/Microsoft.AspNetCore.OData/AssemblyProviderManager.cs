using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData
{
    internal class AssemblyProviderManager
    {
        private static IAssemblyProvider _provider;

        public static void Register([NotNull] IAssemblyProvider provider)
        {
            _provider = provider;
        }

        public static IAssemblyProvider Instance()
        {
            return _provider;
        }
    }
}
