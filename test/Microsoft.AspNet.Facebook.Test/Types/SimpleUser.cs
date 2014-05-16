// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Facebook.Test.Types
{
    public class SimpleUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public FacebookConnection<FacebookPicture> Picture { get; set; }
    }
}
