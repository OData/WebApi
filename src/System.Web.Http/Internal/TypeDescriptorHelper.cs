using System.ComponentModel;

namespace System.Web.Http.Internal
{
    // REVIEW: Rename to match XxxUtil pattern
    // REVIEW: Ensure that using this directly still indirectly uses any user-registered descriptor provider
    internal static class TypeDescriptorHelper
    {
        internal static ICustomTypeDescriptor Get(Type type)
        {
            //// REVIEW: this will cause a security exception
            ////return new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);

            return TypeDescriptor.GetProvider(type).GetTypeDescriptor(type);
        }
    }
}
