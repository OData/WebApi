using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace WebStack.QA.Share.Controllers
{
    public class JTokenController : ApiController
    {
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public JValue EchoJValue(JValue input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public JRaw EchoJRaw(JRaw input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public JObject EchoJObject(JObject input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public JArray EchoJArray(JArray input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public JConstructor EchoJConstructor(JConstructor input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public JToken EchoJToken(JToken input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public JContainer EchoJContainer(JContainer input)
        {
            return input;
        }
    }
}