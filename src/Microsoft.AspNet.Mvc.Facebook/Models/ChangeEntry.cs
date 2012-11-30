// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Mvc.Facebook.Models
{
    public class ChangeEntry
    {
        [JsonProperty("UId")]
        public long UserId { get; set; }

        [JsonProperty("Changed_Fields")]
        public IEnumerable<string> ChangedFields { get; set; }

        public long Time { get; set; }
    }
}
