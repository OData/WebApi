using System.Collections.Generic;

namespace System.Web.Http.Metadata.Providers
{
    public class CachedDataAnnotationsModelMetadataProvider : CachedAssociatedMetadataProvider<CachedDataAnnotationsModelMetadata>
    {
        protected override CachedDataAnnotationsModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName)
        {
            return new CachedDataAnnotationsModelMetadata(this, containerType, modelType, propertyName, attributes);
        }

        protected override CachedDataAnnotationsModelMetadata CreateMetadataFromPrototype(CachedDataAnnotationsModelMetadata prototype, Func<object> modelAccessor)
        {
            return new CachedDataAnnotationsModelMetadata(prototype, modelAccessor);
        }
    }
}
