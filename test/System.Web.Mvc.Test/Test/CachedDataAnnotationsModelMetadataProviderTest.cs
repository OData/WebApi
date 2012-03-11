namespace System.Web.Mvc.Test
{
    public class CachedDataAnnotationsModelMetadataProviderTest : DataAnnotationsModelMetadataProviderTestBase
    {
        protected override AssociatedMetadataProvider MakeProvider()
        {
            return new CachedDataAnnotationsModelMetadataProvider();
        }
    }
}
