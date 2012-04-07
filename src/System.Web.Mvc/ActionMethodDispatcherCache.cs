// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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
