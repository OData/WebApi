// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System.Web.Http.OData.Builder.TestModels
{
    public abstract class Vehicle
    {
        [Key]
        public int Model { get; set; }

        [Key]
        public string Name { get; set; }

        public abstract int WheelCount { get; set; }
    }

    public class Car : Vehicle
    {
        public override int WheelCount
        {
            get { return 4; }
            set { }
        }

        public int SeatingCapacity { get; set; }

        public CarManufacturer Manufacturer { get; set; }
    }

    public class Motorcycle : Vehicle
    {
        public override int WheelCount
        {
            get { return 2; }
            set { }
        }

        public bool CanDoAWheelie { get; set; }

        [NotMapped]
        public int ID { get; set; }

        public MotorcycleManufacturer Manufacturer { get; set; }

        [NotMapped]
        public IEnumerable<MotorcycleManufacturer> Manufacturers { get; set; }
    }

    public class SportBike : Motorcycle
    {
        [NotMapped]
        public int SportBikeProperty_NotVisible { get; set; }
    }

    public abstract class Manufacturer
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public ManufacturerAddress Address { get; set; }
    }

    public class MotorcycleManufacturer : Manufacturer
    {
    }

    public class CarManufacturer : Manufacturer
    {
    }

    public class ManufacturerAddress
    {
        public int HouseNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string IgnoreThis { get; set; }
    }

    public class CarManufacturerAddress : ManufacturerAddress
    {
        [NotMapped]
        public int ID { get; set; }
    }

    public class MotorcycleManufacturerAddress : ManufacturerAddress
    {
    }
}
