namespace System.Web.Http.Routing
{
    // Represents a literal subsegment of a ContentPathSegment
    internal sealed class PathLiteralSubsegment : PathSubsegment
    {
        public PathLiteralSubsegment(string literal)
        {
            Literal = literal;
        }

        public string Literal { get; private set; }

#if ROUTE_DEBUGGING
        public override string LiteralText
        {
            get
            {
                return Literal;
            }
        }

        public override string ToString()
        {
            return "\"" + Literal + "\"";
        }
#endif
    }
}
