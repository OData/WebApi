using System.Linq;
using Nuwa.WebStack.Descriptor;
using Nuwa.WebStack.Route;

namespace Nuwa.Sdk.Elements
{
    public abstract class BaseHostElement : AbstractRunElement
    {
        protected static readonly string KeyBaseAddresss = "HostElement_BaseAddress";

        private IRouteFactory _defaultRoute;

        public BaseHostElement(TestTypeDescriptor typeDescriptor, IRouteFactory routeFactory)
        {
            TypeDescriptor = typeDescriptor;
            _defaultRoute = routeFactory;
        }

        public TestTypeDescriptor TypeDescriptor { get; set; }

        public override void Initialize(RunFrame frame)
        {
            bool result = InitializeServer(frame);
        }

        public override void Cleanup(RunFrame frame)
        {
            ShutdownServer(frame);
        }

        public override void Recover(object testClass, NuwaTestCommand testCommand)
        {
            SetBaseAddress(testClass, testCommand.Frame);
        }

        protected abstract bool InitializeServer(RunFrame frame);

        protected abstract void ShutdownServer(RunFrame frame);

        /// <summary>
        /// Assign base address uri to the property marked by <paramref name="NuwaBaseAddressAttribute"/>
        /// assuming that there are not more than 1 property is marked by the attribute
        /// </summary>
        protected void SetBaseAddress(object testClass, RunFrame frame)
        {
            var baseAddress = frame.GetState(KeyBaseAddresss) as string;
            var testClassType = testClass.GetType();

            /// TODO: 1. check the number of marked property in test class command
            /// TODO: 2. to complain of the property type is not correct
            var baseAddressPrpt = testClassType.GetProperties()
                .Where(prop => { return prop.GetCustomAttributes(typeof(NuwaBaseAddressAttribute), false).Length == 1; })
                .FirstOrDefault();

            if (baseAddressPrpt != null && NuwaBaseAddressAttribute.Verify(baseAddressPrpt))
            {
                baseAddressPrpt.SetValue(testClass, baseAddress, null);
            }
            else
            {
                // TODO: warning
            }
        }

        protected string GetDefaultRouteTemplate()
        {
            return _defaultRoute.RouteTemplate;
        }
    }
}
