using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Filters
{
    public interface IExceptionFilter : IFilter
    {
        Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken);
    }
}
