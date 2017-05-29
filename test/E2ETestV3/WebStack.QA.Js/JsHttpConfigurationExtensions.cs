using WebStack.QA.Js.Server;

namespace System.Web.Http
{
    public static class JsHttpConfigurationExtensions
    {
        private const string JsServerSettingsKey = "JsServerSettingsKey";

        public static void SetupJsTestServer(this HttpConfiguration config, string root, JsServerSettings settings)
        {
            settings.Root = root;
            config.Properties.TryAdd(JsServerSettingsKey, settings);
            config.Routes.MapHttpRoute("TestClientPage", root, new { controller = "JsTestPage" });
            config.Routes.MapHttpRoute("Script", root + "/Script", new { controller = "Resource", action = "GetScript" });
            config.Routes.MapHttpRoute("Css", root + "/Css", new { controller = "Resource", action = "GetCss" });
        }

        public static JsServerSettings GetJsServerSettings(this HttpConfiguration config)
        { 
            object settings;
            if (config.Properties.TryGetValue(JsServerSettingsKey, out settings))
            {
                return settings as JsServerSettings;
            }
            else
            {
                return null;
            }
        }
    }
}
