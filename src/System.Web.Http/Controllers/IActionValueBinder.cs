using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Controllers
{
    public interface IActionValueBinder
    {
        HttpActionBinding GetBinding(HttpActionDescriptor actionDescriptor);
    }
}
