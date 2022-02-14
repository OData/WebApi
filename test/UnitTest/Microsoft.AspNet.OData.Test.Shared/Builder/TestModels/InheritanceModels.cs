//-----------------------------------------------------------------------------
// <copyright file="InheritanceModels.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    [MediaType]
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

        public Engine MyEngine { get; set; }

        public V4 MyV4Engine { get; set; }

    }

    public class Engine
    {
        public int Hp { get; set; }

        public Transmission Transmission { get; set; }
    }

    public class Transmission
    {
        public int Gears { get; set; }
    }

    public class Automatic: Transmission
    {

    }

    public class Manual : Transmission
    {

    }



    public class V2: Engine
    {

    }

    public class V4 : Engine
    {

    }

    public class V41: V4
    {
        public string MakeName { get; set; }
    }

    public class V42 : V4
    {
        public string Model { get; set; }
    }

    public class V422 : V42
    {
        
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

    public class Zoo
    {
        public int Id { get; set; }
        public Animal SpecialAnimal { get; set; }
    }

    public class StateZoo : Zoo
    {

    }

    public class NationZoo : Zoo
    {

    }

    public class SeaZoo : Zoo
    {

    }

    public class Creature
    {
        public int Id { get; set; }
    }

    public class Animal : Creature
    {
        public int Age { get; set; }
    }

    public class Gene : Creature
    {
    }

    public class Human : Animal
    {
        public int HumanId { get; set; }
    }

    public class Park
    {
        public int Id { get; set; }

        [DerivedTypeConstraint(typeof(Animal))]
        public Animal Animal { get; set; }
        public Human Human { get; set; }
    }

    public class Horse : Animal
    {
        public int HorseId { get; set; }
    }

    public class Zebra : Animal
    {
        public int ZebraId { get; set; }
    }

    public class ZooHorse
    {
        public int Id { get; set; }
        public Horse Horse { get; set; }

        [DerivedTypeConstraint(typeof(Horse), typeof(Zebra))]
        public Animal Animal { get; set; }
    }

    public class Plant
    {
        public int PlantId { get; set; }
    }

    public class OceanPlant : Plant
    {
        public int OcentPlantId { get; set; }
    }

    public class Phycophyta : OceanPlant
    {
        public int PhycophytaProperty { get; set; }
    }

    public class Mangrove : OceanPlant
    {
        public int MangroveId { get; set; }
    }

    public class LandPlant : Plant
    {
        public int LandPlantId { get; set; }
    }

    public class Tree : LandPlant
    {
        public int TreeId { get; set; }
    }

    public class Flower : LandPlant
    {
        public int FlowerId { get; set; }
    }

    public class Jasmine : Flower
    {
        public int JasminePoperty { get; set; }
    }

    public class PlantParkWithOceanPlantAndJasmine
    {
        public int Id { get; set; }
        public OceanPlant OceanPlant { get; set; }
        public Jasmine Jasmine { get; set; }
    }

    public class PlantPark
    {
        public int Id { get; set; }
        public Plant Pant { get; set; }
    }
}
