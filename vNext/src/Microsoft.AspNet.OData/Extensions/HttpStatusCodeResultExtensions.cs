using Microsoft.AspNet.Mvc;

namespace Microsoft.AspNet.OData.Extensions
{
    public static class HttpStatusCodeResultExtensions
    {
        public static bool IsSuccessStatusCode(this HttpStatusCodeResult response)
        {
            return response == null || (response.StatusCode >= 200 && response.StatusCode < 300);
        }
    }
}
