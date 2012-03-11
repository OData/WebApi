using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Internal;

namespace System.Web.Http.Controllers
{
    internal abstract class ActionResponseConverter
    {
        private static readonly Type _genericResponseMessageConverterType = typeof(HttpResponseMessageConverter<>);
        private static readonly Type _genericTaskActionResponseConverterType = typeof(TaskActionResponseConverter<>);
        protected static readonly ActionResponseConverter SimpleHttpResponseMessageConverter = new HttpResponseMessageConverter();
        protected static readonly ActionResponseConverter HttpContentMessageConverter = new HttpContentMessageConverter();
        protected static readonly ActionResponseConverter VoidHttpResponseMessageConverter = new VoidHttpResponseMessageConverter();
        protected static readonly ActionResponseConverter TaskActionResponseConverter = new TaskActionResponseConverter();

        public abstract Task<HttpResponseMessage> Convert(HttpControllerContext controllerContext, object responseValue, CancellationToken cancellation);

        public static ActionResponseConverter GetResponseMessageConverter(Type responseContentType)
        {
            if (typeof(Task).IsAssignableFrom(responseContentType))
            {
                if (responseContentType.IsGenericType)
                {
                    Type actualResponseContentType = responseContentType.GetGenericArguments()[0];
                    return GetGenericTaskActionResponseConverter(actualResponseContentType);
                }
                else
                {
                    return TaskActionResponseConverter;
                }
            }
            else if (responseContentType == typeof(void))
            {
                return VoidHttpResponseMessageConverter;
            }
            else
            {
                responseContentType = TypeHelper.GetHttpResponseOrContentInnerTypeOrNull(responseContentType) ?? responseContentType;

                if (TypeHelper.IsHttpResponse(responseContentType))
                {
                    return SimpleHttpResponseMessageConverter;
                }
                else if (TypeHelper.IsHttpContent(responseContentType))
                {
                    return HttpContentMessageConverter;
                }
                else
                {
                    return GetGenericHttpResponseMessageConverter(responseContentType);
                }
            }
        }

        // This is the general purpose catch-all converter for arbitrary objects. 
        private static ActionResponseConverter GetGenericHttpResponseMessageConverter(Type responseContentType)
        {
            Type closedConverterType = _genericResponseMessageConverterType.MakeGenericType(new Type[] { responseContentType });
            ConstructorInfo constructor = closedConverterType.GetConstructor(Type.EmptyTypes);

            // REVIEW: Cache converter?
            return constructor.Invoke(null) as ActionResponseConverter;
        }

        private static ActionResponseConverter GetGenericTaskActionResponseConverter(Type responseContentType)
        {
            Type closedConverterType = _genericTaskActionResponseConverterType.MakeGenericType(new Type[] { responseContentType });
            ConstructorInfo constructor = closedConverterType.GetConstructor(new Type[] { typeof(ActionResponseConverter) });

            // REVIEW: Cache converter?
            return constructor.Invoke(new object[] { GetResponseMessageConverter(responseContentType) }) as ActionResponseConverter;
        }
    }
}
