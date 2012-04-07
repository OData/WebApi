// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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
