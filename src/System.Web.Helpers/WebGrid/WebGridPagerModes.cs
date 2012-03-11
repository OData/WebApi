namespace System.Web.Helpers
{
    [Flags]
    public enum WebGridPagerModes
    {
        Numeric = 0x1,
        NextPrevious = 0x2,
        FirstLast = 0x4,
        All = 0x7
    }
}
