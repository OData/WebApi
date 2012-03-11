namespace System.Web.Mvc
{
    public static class ModelBinderProviders
    {
        private static readonly ModelBinderProviderCollection _binderProviders = new ModelBinderProviderCollection
        {
        };

        public static ModelBinderProviderCollection BinderProviders
        {
            get { return _binderProviders; }
        }
    }
}
