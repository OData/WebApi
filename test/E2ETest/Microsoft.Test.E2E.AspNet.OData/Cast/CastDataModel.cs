//-----------------------------------------------------------------------------
// <copyright file="CastDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Cast
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Domain Domain { get; set; }
        public Double Weight { get; set; }
        public IList<int> DimensionInCentimeter { get; set; }
        public DateTimeOffset ManufacturingDate { get; set; }
    }

    [Flags]
    public enum Domain
    {
        Military = 1,
        Civil = 2,
        Both = 3,
    }


    public class AirPlane : Product
    {
        public int Speed { get; set; }
    }

    public class JetPlane : AirPlane
    {
        public string Company { get; set; }
    }

}
