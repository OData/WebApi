namespace System.Web.Http.Routing
{
    public interface IHttpVirtualPathData
    {
        IHttpRoute Route { get; }

        string VirtualPath { get; }
    }
}
