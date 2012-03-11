namespace System.Web.Mvc.Test
{
    public class Resolver<T> : IResolver<T>
    {
        public T Current { get; set; }
    }
}
