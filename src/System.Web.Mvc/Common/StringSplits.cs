namespace System.Web.Mvc
{
    internal static class StringSplits
    {
        // note array contents not technically read-only; just... don't edit them!
        internal static readonly char[]
            Period = new[] { '.' },
            Comma = new[] { ',' };
    }
}
