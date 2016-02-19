using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.OData
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
