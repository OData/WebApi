// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace System.Web.Http.OData.Builder.TestModels
{
    public abstract class Vehicle
    {
        [Key]
        public int Model { get; set; }

        [Key]
        public string Name { get; set; }

        public abstract int WheelCount { get; }
    }

    public class Car : Vehicle
    {
        public override int WheelCount
        {
            get { return 4; }
        }

        public int SeatingCapacity { get; set; }
    }

    public class Motorcycle : Vehicle
    {
        public override int WheelCount
        {
            get { return 2; }
        }

        public bool CanDoAWheelie { get; set; }

        public int ID { get; set; }
    }

    public class Cruiser : Motorcycle
    {
    }
}
