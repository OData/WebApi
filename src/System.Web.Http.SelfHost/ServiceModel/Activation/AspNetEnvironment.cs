using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel.Channels;

namespace System.Web.Http.SelfHost.ServiceModel.Activation
{
    internal class AspNetEnvironment
    {
        public const string HostingMessagePropertyName = "webhost";

        private const string HostingMessagePropertyTypeName = "System.ServiceModel.Activation.HostingMessageProperty";
        private static AspNetEnvironment _current;
        private static readonly object _thisLock = new object();

        public static AspNetEnvironment Current
        {
            get
            {
                if (_current == null)
                {
                    lock (_thisLock)
                    {
                        if (_current == null)
                        {
                            _current = new AspNetEnvironment();
                        }
                    }
                }

                return _current;
            }
        }

        // ALTERED_FOR_PORT:
        // The GetHostingProperty() code below is an altered implementation from the System.ServiceModel.Activation.HostedAspNetEnvironment class.
        // The original implementation casts the hostingProperty to type System.ServiceModel.Activation.HostingMessageProperty.  However,
        //  this class is internal sealed, therefore we simply check the type name.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is existing public API")]
        public object GetHostingProperty(Message message)
        {
            object hostingProperty;
            if (message.Properties.TryGetValue(HostingMessagePropertyName, out hostingProperty))
            {
                string hostingPropertyName = hostingProperty.GetType().FullName;
                if (String.Equals(hostingPropertyName, HostingMessagePropertyTypeName, System.StringComparison.Ordinal))
                {
                    return hostingProperty;
                }
            }

            return null;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is existing public API")]
        public object GetConfigurationSection(string sectionPath)
        {
            return ConfigurationManager.GetSection(sectionPath);
        }
    }
}
