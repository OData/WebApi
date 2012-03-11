namespace System.Web.Http.Routing
{
    // Represents a segment of a URI such as a separator or content
    internal abstract class PathSegment
    {
#if ROUTE_DEBUGGING
        public abstract string LiteralText
        {
            get;
        }
#endif
    }
}
