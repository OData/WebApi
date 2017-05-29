using System.IO;
using Nuwa.WebStack.Descriptor;
using Nuwa.WebStack.Route;
using WebStack.QA.Common.Extensions;
using WebStack.QA.Common.WebHost;

namespace Nuwa.WebStack.Host
{
    public class IISHostElement : WebBaseHostElement
    {
        public IISHostElement(TestTypeDescriptor typeDescriptor, IRouteFactory routeFactory, ITemporaryDirectoryProvider dirProvider)
            : base(typeDescriptor, routeFactory, dirProvider)
        {
            this.Name = "IIS";
        }

        protected override void InitOptions(WebHostContext context)
        {
            DirectoryInfo target = _dirProvider.CreateDirectory();
            context.DeploymentOptions = new LocalDeploymentOptions(target);

            var hostOptions = new IISHostOptions(target, target.Name);
            hostOptions.Users.Add(@"NT AUTHORITY\Network Service");
            hostOptions.Users.Add(@"NT AUTHORITY\IUSR");
            hostOptions.Users.Add(@".\Users");
            context.HostOptions = hostOptions;
        }
    }
}
