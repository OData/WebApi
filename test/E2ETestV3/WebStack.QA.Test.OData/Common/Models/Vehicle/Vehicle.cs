using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Services.Common;
using System.Linq;
using System.Text;

namespace WebStack.QA.Test.OData.Common.Models.Vehicle
{
    #region Models
    [EntitySet("InheritanceTests_MovingObject")]
    [DataServiceKey("Id")]
    public abstract class MovingObject
    {
        public int Id { get; set; }
    }

    [EntitySet("InheritanceTests_Vehicles")]
    [DataServiceKey("Id")]
    public class Vehicle : MovingObject
    {
        public string Model { get; set; }

        public string Name { get; set; }

        public virtual int WheelCount { get; set; }
    }

    [EntitySet("InheritanceTests_Cars")]
    [DataServiceKey("Id")]
    public class Car : Vehicle
    {
        public Car()
        {
            this.BaseTypeNavigationProperty = new List<Vehicle>();
            this.DerivedTypeNavigationProperty = new List<MiniSportBike>();
        }

        public override int WheelCount
        {
            get
            {
                return 4;
            }
            set
            {
            }
        }

        public int SeatingCapacity { get; set; }

        public List<Vehicle> BaseTypeNavigationProperty { get; set; }

        public List<MiniSportBike> DerivedTypeNavigationProperty { get; set; }

        public Vehicle SingleNavigationProperty { get; set; }
    }

    [EntitySet("InheritanceTests_Motorcycles")]
    [DataServiceKey("Id")]
    public class Motorcycle : Vehicle
    {
        public override int WheelCount
        {
            get
            {
                return 2;
            }
            set
            {
            }
        }

        public bool CanDoAWheelie { get; set; }
    }

    [EntitySet("InheritanceTests_SportBikes")]
    [DataServiceKey("Id")]
    public class SportBike : Motorcycle
    {
        public long TopSpeed { get; set; }
    }

    [EntitySet("InheritanceTests_MiniSportBikes")]
    [DataServiceKey("Id")]
    public class MiniSportBike : SportBike
    {
        [Required]
        public string Size { get; set; }
    }

    [EntitySet("InheritanceTests_Customers")]
    [DataServiceKey("Id")]
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
