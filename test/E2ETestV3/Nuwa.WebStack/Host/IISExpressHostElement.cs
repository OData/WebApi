using Nuwa.WebStack.Descriptor;
using Nuwa.WebStack.Route;
using WebStack.QA.Common.WebHost;

namespace Nuwa.WebStack.Host
{
    public class IISExpressHostElement : WebBaseHostElement
    {
        public IISExpressHostElement(TestTypeDescriptor typeDescriptor, IRouteFactory routeFactory, ITemporaryDirectoryProvider dirProvider)
            : base(typeDescriptor, routeFactory, dirProvider)
        {
            this.Name = "IIS Express";
        }

        protected override void InitOptions(WebHostContext context)
        {
            var target = _dirProvider.CreateDirectory();
            context.DeploymentOptions = new LocalDeploymentOptions(target);

            var hostOptions = new IISExpressHostOptions(target, target.Name);
            hostOptions.Users.Add(@".\Users");
            context.HostOptions = hostOptions;
        }
    }
}
