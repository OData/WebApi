// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.TestCommon.Models
{
    public interface IActivity
    {
        string ActivityName { get; set; }

        void DoActivity();
    }
}
