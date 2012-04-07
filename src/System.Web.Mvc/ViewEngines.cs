// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public static class ViewEngines
    {
        private static readonly ViewEngineCollection _engines = new ViewEngineCollection
        {
            new WebFormViewEngine(),
            new RazorViewEngine(),
        };

        public static ViewEngineCollection Engines
        {
            get { return _engines; }
        }
    }
}
