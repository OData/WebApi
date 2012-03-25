using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace System.Web.Http.ModelBinding
{    
    // Supports JQuery schema on FormURL. 
    public class JQueryMvcFormUrlEncodedFormatter : FormUrlEncodedMediaTypeFormatter
    {
        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            // For simple types, defer to base class
            if (base.CanReadType(type))
            {
                return base.ReadFromStreamAsync(type, stream, contentHeaders, formatterLogger);
            }

            return base.ReadFromStreamAsync(typeof(FormDataCollection), stream, contentHeaders, formatterLogger).Then(
                (obj) =>
                {
                    FormDataCollection fd = (FormDataCollection)obj;
                    object result = fd.ReadAs(type);

                    return result;
                });
        }
    }
}
