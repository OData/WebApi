// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Test.E2E.AspNet.OData.Enums
{
    public class Employee
    {
        public int ID { get; set; }
        public String Name { get; set; }
        public List<Skill> SkillSet { get; set; }
        public Gender Gender { get; set; }
        public AccessLevel AccessLevel { get; set; }
        public FavoriteSports FavoriteSports { get; set; }
    }

    [Flags]
    public enum AccessLevel
    {
        Read = 1,
        Write = 2,
        Execute = 4
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public enum Skill
    {
        CSharp,
        Sql,
        Web,
    }

    public enum Sport
    {
        Pingpong,
        Basketball
    }

    public class FavoriteSports
    {
        public Sport LikeMost { get; set; }
        public List<Sport> Like { get; set; }
    }

    [DataContract]
    public enum Status
    {
        [EnumMember(Value = "Sold out")]
        SoldOut,

        [EnumMember(Value = "In store")]
        InStore
    }

    public class WeatherForecast
    {
        public int Id { get; set; }

        public Status Status { get; set; }

        public Skill Skill { get; set; }
    }
}
