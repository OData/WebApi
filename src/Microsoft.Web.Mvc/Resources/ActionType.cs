// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// This enum is used by the UrlHelper extension methods to create links within resource controllers
    /// </summary>
    public enum ActionType
    {
        Create,
        GetCreateForm,
        Index,
        Retrieve,
        Update,
        GetUpdateForm,
        Delete,
    }
}
