using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace System.Web.Http.Controllers
{
    internal interface IActionHttpMethodProvider
    {
        Collection<HttpMethod> HttpMethods { get; }
    }
}
