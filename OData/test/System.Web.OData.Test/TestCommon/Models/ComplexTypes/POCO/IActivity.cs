// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.TestCommon.Models
{
    public interface IActivity
    {
        string ActivityName { get; set; }

        void DoActivity();
    }
}
