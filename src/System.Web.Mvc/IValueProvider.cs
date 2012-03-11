namespace System.Web.Mvc
{
    public interface IValueProvider
    {
        bool ContainsPrefix(string prefix);
        ValueProviderResult GetValue(string key);
    }
}
