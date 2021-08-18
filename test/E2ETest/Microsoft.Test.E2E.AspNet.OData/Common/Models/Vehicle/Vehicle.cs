//-----------------------------------------------------------------------------
// <copyright file="Vehicle.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Client;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models.Vehicle
{
    #region Models
    [EntitySet("InheritanceTests_MovingObject")]
    [Key("Id")]
    public abstract class MovingObject
    {
        public int Id { get; set; }
    }

    [EntitySet("InheritanceTests_Vehicles")]
    [Key("Id")]
    public class Vehicle : MovingObject
    {
        public string Model { get; set; }

        public string Name { get; set; }

        public virtual int WheelCount { get; set; }
    }

    [EntitySet("InheritanceTests_Cars")]
    [Key("Id")]
    public class Car : Vehicle
    {
        public Car()
        {
            this.BaseTypeNavigationProperty = new List<Vehicle>();
            this.DerivedTypeNavigationProperty = new List<MiniSportBike>();
        }

        public override int WheelCount
        {
            get { return 4; }
            set { }
        }

        public int SeatingCapacity { get; set; }

        public List<Vehicle> BaseTypeNavigationProperty { get; set; }

        public List<MiniSportBike> DerivedTypeNavigationProperty { get; set; }

        public Vehicle SingleNavigationProperty { get; set; }
    }

    [EntitySet("InheritanceTests_Motorcycles")]
    [Key("Id")]
    public class Motorcycle : Vehicle
    {
        public override int WheelCount
        {
            get { return 2; }
            set { }
        }

        public bool CanDoAWheelie { get; set; }
    }

    [EntitySet("InheritanceTests_SportBikes")]
    [Key("Id")]
    public class SportBike : Motorcycle
    {
        public long TopSpeed { get; set; }
    }

    [EntitySet("InheritanceTests_MiniSportBikes")]
    [Key("Id")]
    public class MiniSportBike : SportBike
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Size { get; set; }
    }

    [EntitySet("InheritanceTests_Customers")]
    [Key("Id")]
    public class Customer
    {
        public Customer()
        {
            this.Vehicles = new List<Vehicle>();
        }

        public int Id { get; set; }

        public List<Vehicle> Vehicles { get; set; }
    }

    #endregion
}
