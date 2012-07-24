// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    // Controller now supports asynchronous operations.
    // This class only exists 
    // a) for backwards compat for callers that derive from it,
    // b) ActionMethodSelector can detect it to bind to ActionAsync/ActionCompleted patterns. 
    public abstract class AsyncController : Controller
    {
    }
}
