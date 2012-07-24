// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    // represents a result that doesn't do anything, like a controller action returning null
    public class EmptyResult : ActionResult
    {
        private static readonly EmptyResult _singleton = new EmptyResult();

        internal static EmptyResult Instance
        {
            get { return _singleton; }
        }

        public override void ExecuteResult(ControllerContext context)
        {
        }
    }
}
