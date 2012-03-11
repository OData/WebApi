namespace System.Web.Http.Filters
{
    public enum FilterScope
    {
        First = 0,
        Global = 10,
        Controller = 20,
        Action = 30,
        Last = 100
    }
}
