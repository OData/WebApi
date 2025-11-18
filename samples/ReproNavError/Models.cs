namespace ReproNavError
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.OData.Edm;

    public class ApplicationUsage
    {
        public string Id { get; set; }
        [Contained]

        public List<KeyCredentialUsage> KeyCredentials { get; set; }
    }

    public class KeyCredentialUsage
    {
        public string Id { get; set; }

        [Contained]
        [AutoExpand]
        public List<StatBucket> Buckets { get; set; }
    }

    public class StatBucket
    {
        public string Id { get; set; }
        public int Count { get; set; }
    }

    public static class ModelHelper
    {
        public static readonly IEdmModel Model = BuildModel();

        private static IEdmModel BuildModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ApplicationUsage>("ApplicationUsages");
            var appUsageFunction = builder.Function("GetAppUsage")
                .ReturnsCollectionFromEntitySet<ApplicationUsage>("ApplicationUsages");
            appUsageFunction.Parameter<DateTimeOffset>("intervalStartDateTime").Optional();
            appUsageFunction.Parameter<DateTimeOffset>("exclusiveIntervalEndDateTime").Optional();
            appUsageFunction.Parameter<int>("aggregationWindow").Optional();
            appUsageFunction.IsComposable = true;

            return builder.GetEdmModel();
        }
    }
}
