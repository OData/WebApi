// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.Facebook.Test.Types
{
    public class UserTypeWithRenamedProperties
    {
        [JsonProperty("Id")]
        public string FacebookId { get; set; }

        public string Name { get; set; }

        [JsonProperty("picture")]
        public FacebookConnection<FacebookPicture> PictureConnection { get; set; }
    }
}
