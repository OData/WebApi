using System.Collections.Generic;
using System.Linq;

namespace System.Web.Razor.Utils
{
    internal static class EnumeratorExtensions
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.SelectMany(e => e);
        }
    }
}
