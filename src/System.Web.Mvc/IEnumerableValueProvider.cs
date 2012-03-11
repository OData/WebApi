using System.Collections.Generic;

namespace System.Web.Mvc
{
    // Represents a special IValueProvider that has the ability to be enumerable.
    public interface IEnumerableValueProvider : IValueProvider
    {
        IDictionary<string, string> GetKeysFromPrefix(string prefix);
    }
}
