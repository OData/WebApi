namespace System.Web.Mvc
{
    public sealed class RouteDataValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            return new RouteDataValueProvider(controllerContext);
        }
    }
}
