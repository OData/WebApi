namespace Microsoft.Web.Mvc.Resources
{
    public class DefaultFormatManager : FormatManager
    {
        public DefaultFormatManager()
        {
            XmlFormatHandler xmlHandler = new XmlFormatHandler();
            JsonFormatHandler jsonHandler = new JsonFormatHandler();
            this.RequestFormatHandlers.Add(xmlHandler);
            this.RequestFormatHandlers.Add(jsonHandler);
            this.ResponseFormatHandlers.Add(xmlHandler);
            this.ResponseFormatHandlers.Add(jsonHandler);
        }
    }
}
