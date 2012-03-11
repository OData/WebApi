namespace System.Web.Mvc
{
    public abstract class ValueProviderFactory
    {
        public abstract IValueProvider GetValueProvider(ControllerContext controllerContext);
    }
}
