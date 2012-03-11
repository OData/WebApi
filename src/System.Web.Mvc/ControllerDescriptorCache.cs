namespace System.Web.Mvc
{
    internal sealed class ControllerDescriptorCache : ReaderWriterCache<Type, ControllerDescriptor>
    {
        public ControllerDescriptorCache()
        {
        }

        public ControllerDescriptor GetDescriptor(Type controllerType, Func<ControllerDescriptor> creator)
        {
            return FetchOrCreateItem(controllerType, creator);
        }
    }
}
