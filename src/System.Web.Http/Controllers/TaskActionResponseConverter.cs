using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Controllers
{
    internal sealed class TaskActionResponseConverter : ActionResponseConverter
    {
        public override Task<HttpResponseMessage> Convert(HttpControllerContext controllerContext, object responseValue, CancellationToken cancellation)
        {
            Task responseTask = (Task)responseValue;
            return responseTask.Then(() =>
            {
                return VoidHttpResponseMessageConverter.Convert(controllerContext, null, cancellation);
            }, cancellation);
        }
    }

    internal sealed class TaskActionResponseConverter<TResponseValue> : ActionResponseConverter
    {
        private readonly ActionResponseConverter _innerConverter;

        public TaskActionResponseConverter(ActionResponseConverter innerConverter)
        {
            _innerConverter = innerConverter;
        }

        public override Task<HttpResponseMessage> Convert(HttpControllerContext controllerContext, object responseValue, CancellationToken cancellation)
        {
            Task<TResponseValue> responseTask = (Task<TResponseValue>)responseValue;
            return responseTask.Then<TResponseValue, HttpResponseMessage>(result =>
            {
                return _innerConverter.Convert(controllerContext, result, cancellation);
            }, cancellation);
        }
    }
}
