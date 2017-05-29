using System.Collections.Generic;
using System.Reflection;
using WebStack.QA.Js.Utils;

namespace WebStack.QA.Js.Server
{
    public class JsServerSettings
    {
        public JsServerSettings()
        {
            ResourceLoadFrom = new List<Assembly>();
            ResourceLoadFrom.Add(this.GetType().Assembly);
            Builder = new HtmlPageBuilder();
        }

        public HtmlPageBuilder Builder { get; set; }
        public List<Assembly> ResourceLoadFrom { get; set; }
        public string Root { get; set; }

        public ResourceLoader Loader
        {
            get
            {
                return new ResourceLoader() { LoadFrom = ResourceLoadFrom };
            }
        }
    }
}
