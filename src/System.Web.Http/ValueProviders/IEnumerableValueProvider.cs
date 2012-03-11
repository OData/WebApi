using System.Collections.Generic;

namespace System.Web.Http.ValueProviders
{
    public interface IEnumerableValueProvider : IValueProvider
    {
        IDictionary<string, string> GetKeysFromPrefix(string prefix);
    }
}
