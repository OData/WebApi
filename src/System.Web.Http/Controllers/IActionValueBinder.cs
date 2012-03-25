namespace System.Web.Http.Controllers
{
    public interface IActionValueBinder
    {
        HttpActionBinding GetBinding(HttpActionDescriptor actionDescriptor);
    }
}
