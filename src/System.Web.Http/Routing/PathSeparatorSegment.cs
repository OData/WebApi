namespace System.Web.Http.Routing
{
    // Represents a "/" separator in a URI
    internal sealed class PathSeparatorSegment : PathSegment
    {
#if ROUTE_DEBUGGING
        public override string LiteralText
        {
            get
            {
                return "/";
            }
        }

        public override string ToString()
        {
            return "\"/\"";
        }
#endif
    }
}
