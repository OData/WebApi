using System.Net.Http;

namespace System.Web.Http
{
    /// <summary>
    /// This attribute is used on action parameters to indicate
    /// they come only from the content body of the incoming <see cref="HttpRequestMessage"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class FromBodyAttribute : Attribute
    {
    }
}
