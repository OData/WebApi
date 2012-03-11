using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class ApiControllerActionInvokerTest
    {
        [Fact]
        public void InvokeActionAsync_Calls_ActionMethod()
        {
            ApiControllerActionInvoker actionInvoker = new ApiControllerActionInvoker();
            UsersController controller = new UsersController();
            Func<HttpResponseMessage> actionMethod = controller.Get;
            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(instance: controller),
                new ReflectedHttpActionDescriptor { MethodInfo = actionMethod.Method });

            HttpResponseMessage response = actionInvoker.InvokeActionAsync(context, CancellationToken.None).Result;

            Assert.Equal("Default User", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void InvokeActionAsync_Cancels_IfCancellationTokenRequested()
        {
            ApiControllerActionInvoker actionInvoker = new ApiControllerActionInvoker();
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            var response = actionInvoker.InvokeActionAsync(ContextUtil.CreateActionContext(), cancellationSource.Token);

            Assert.Equal<TaskStatus>(TaskStatus.Canceled, response.Status);
        }

        [Fact]
        public void InvokeActionAsync_Throws_IfContextIsNull()
        {
            ApiControllerActionInvoker actionInvoker = new ApiControllerActionInvoker();

            Assert.ThrowsArgumentNull(
                () => actionInvoker.InvokeActionAsync(null, CancellationToken.None),
                "actionContext");
        }
    }
}
