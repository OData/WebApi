using System.Web.Http;

namespace System.Web.Http
{
    public class DuplicateController : ApiController
    {
        public string GetAction()
        {
            return "dup";
        }
    }
}

namespace System.Web.Http2
{
    public class DuplicateController : ApiController
    {
        public string GetAction()
        {
            return "dup2";
        }
    }
}
