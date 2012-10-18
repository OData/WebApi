// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Facebook.Models
{
    [Attributes.FacebookObject]
    public class FacebookObjectList<FacebookObject> : List<FacebookObject>
    {
        //public DateTime? LastUpdated { get; set; }
    }
}
