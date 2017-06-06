using System.Configuration;
using Nuwa.WebStack.Descriptor;
using Nuwa.WebStack.Route;
using WebStack.QA.Common.WebHost;

namespace Nuwa.WebStack.Host
{
    public class AzureWebsiteHostElement : WebBaseHostElement
    {
        public AzureWebsiteHostElement(TestTypeDescriptor typeDescriptor, IRouteFactory routeFactory, ITemporaryDirectoryProvider dirProvider)
            : base(typeDescriptor, routeFactory, dirProvider)
        {
            this.Name = "Azure Web Site";
        }

        protected override void InitOptions(WebHostContext context)
        {
            AppSettingNotEmptyOrNull(NuwaGlobalConfiguration.AzureWebsiteUrl, NuwaGlobalConfiguration.AzureWebsiteUrlKey);
            AppSettingNotEmptyOrNull(NuwaGlobalConfiguration.AzureFtpUrl, NuwaGlobalConfiguration.AzureFtpUrlKey);
            AppSettingNotEmptyOrNull(NuwaGlobalConfiguration.AzureFtpUserName, NuwaGlobalConfiguration.AzureFtpUserNameKey);
            AppSettingNotEmptyOrNull(NuwaGlobalConfiguration.AzureFtpPassword, NuwaGlobalConfiguration.AzureFtpPasswordKey);

            context.DeploymentOptions = new FtpDeploymentOptions(
                NuwaGlobalConfiguration.AzureFtpUrl,
                NuwaGlobalConfiguration.AzureFtpUserName,
                NuwaGlobalConfiguration.AzureFtpPassword);
            context.DeploymentOptions.WebConfigTransformers.Add(new WebConfigTransformer(
                config =>
                {
                    config.ConfigureCustomError("Off");
                }));
            context.HostOptions = new AzureWebsiteHostOptions(NuwaGlobalConfiguration.AzureWebsiteUrl);

        }

        public static void AppSettingNotEmptyOrNull(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ConfigurationErrorsException(string.Format("{0} can't be null", name));
            }
        }
    }
}
