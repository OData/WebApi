// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.ODataCountTest
{
    public class Hero
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<Weapon> Weapons { get; set; }
    }

    public class Weapon
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
