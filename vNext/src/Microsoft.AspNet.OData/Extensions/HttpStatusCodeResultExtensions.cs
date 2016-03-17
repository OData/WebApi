using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.OData.Extensions
{
    public static class HttpStatusCodeResultExtensions
    {
        public static bool IsSuccessStatusCode(this StatusCodeResult response)
        {
            return response?.StatusCode >= 200 && response.StatusCode < 300;
        }
    }
}
