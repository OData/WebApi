namespace System.Web.Mvc.Test
{
    public class DataAnnotationsModelMetadataProviderTest : DataAnnotationsModelMetadataProviderTestBase
    {
        protected override AssociatedMetadataProvider MakeProvider()
        {
            return new DataAnnotationsModelMetadataProvider();
        }
    }
}
