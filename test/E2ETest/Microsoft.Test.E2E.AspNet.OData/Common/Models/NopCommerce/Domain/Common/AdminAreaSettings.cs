// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Nop.Core.Configuration;

namespace Nop.Core.Domain.Common
{
    public class AdminAreaSettings : ISettings
    {
        public int GridPageSize { get; set; }

        public bool DisplayProductPictures { get; set; }
    }
}