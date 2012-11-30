// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Facebook.Models
{
    public class ChangeNotification
    {
        public string Object { get; set; }
        public IEnumerable<ChangeEntry> Entry { get; set; }
    }
}
