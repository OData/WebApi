namespace System.Web.Http
{
    /// <summary>
    /// Sample ApiControler
    /// </summary>
    public class SampleController : ApiController
    {
        [RequireAdmin]
        public string Get()
        {
            return "hello";
        }
    }
}