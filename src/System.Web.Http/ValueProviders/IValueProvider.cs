namespace System.Web.Http.ValueProviders
{
    public interface IValueProvider
    {
        bool ContainsPrefix(string prefix);
        ValueProviderResult GetValue(string key);
    }
}
