using System.Net.Http.Formatting;

namespace System.Web.Http.ApiExplorer
{
    public class ItemFormatter : BufferedMediaTypeFormatter
    {
        public override bool CanReadType(Type type)
        {
            return typeof(System.Web.Http.ApiExplorer.ItemController.Item).IsAssignableFrom(type);
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(System.Web.Http.ApiExplorer.ItemController.Item).IsAssignableFrom(type);
        }

        public override object OnReadFromStream(Type type, IO.Stream stream, Net.Http.Headers.HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            return base.OnReadFromStream(type, stream, contentHeaders, formatterLogger);
        }

        public override void OnWriteToStream(Type type, object value, IO.Stream stream, Net.Http.Headers.HttpContentHeaders contentHeaders, Net.TransportContext transportContext)
        {
            base.OnWriteToStream(type, value, stream, contentHeaders, transportContext);
        }
    }
}
