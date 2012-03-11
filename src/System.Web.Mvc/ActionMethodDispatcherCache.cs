using System.Reflection;

namespace System.Web.Mvc
{
    internal sealed class ActionMethodDispatcherCache : ReaderWriterCache<MethodInfo, ActionMethodDispatcher>
    {
        public ActionMethodDispatcherCache()
        {
        }

        public ActionMethodDispatcher GetDispatcher(MethodInfo methodInfo)
        {
            return FetchOrCreateItem(methodInfo, () => new ActionMethodDispatcher(methodInfo));
        }
    }
}
