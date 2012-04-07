// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Web.Http.Data.Test.Models
{
    /// <summary>
    /// Sample data class
    /// </summary>
    /// <remarks>
    /// This class exposes several data types (City, County, State and Zip) and some sample
    /// data for each. 
    /// </remarks>
    public partial class CityData
    {
        private List<State> _states;
        private List<County> _counties;
        private List<City> _cities;
        private List<Zip> _zips;
        private List<ZipWithInfo> _zipsWithInfo;
        private List<CityWithInfo> _citiesWithInfo;

        public CityData()
        {
            _states = new List<State>()
            {
                new State() { Name="WA", FullName="Washington", TimeZone = TimeZone.Pacific },
                new State() { Name="OR", FullName="Oregon", TimeZone = TimeZone.Pacific },
                new State() { Name="CA", FullName="California", TimeZone = TimeZone.Pacific },
                new State() { Name="OH", FullName="Ohio", TimeZone = TimeZone.Eastern, ShippingZone=ShippingZone.Eastern }
            };

            _counties = new List<County>()
             {
                new County() { Name="King",         StateName="WA" },
                new County() { Name="Pierce",       StateName="WA" },
                new County() { Name="Snohomish",    StateName="WA" },

                new County() { Name="Tillamook",    StateName="OR" },
                new County() { Name="Wallowa",      StateName="OR" },
                new County() { Name="Jackson",      StateName="OR" },

                new County() { Name="Orange",       StateName="CA" },
                new County() { Name="Santa Barbara",StateName="CA" },

                new County() { Name="Lucas",        StateName="OH" }
            };
            foreach (State state in _states)
            {
                foreach (County county in _counties.Where(p => p.StateName == state.Name))
                {
                    state.Counties.Add(county);
                    county.State = state;
                }
            }

            _cities = new List<City>()
            {
                new CityWithInfo() {Name="Redmond", CountyName="King", StateName="WA", Info="Has Microsoft campus", LastUpdated=DateTime.Now},
                new CityWithInfo() {Name="Bellevue", CountyName="King", StateName="WA", Info="Means beautiful view", LastUpdated=DateTime.Now},
                new City() {Name="Duvall", CountyName="King", StateName="WA"},
                new City() {Name="Carnation", CountyName="King", StateName="WA"},
                new City() {Name="Everett", CountyName="King", StateName="WA"},
                new City() {Name="Tacoma", CountyName="Pierce", StateName="WA"},

                new City() {Name="Ashland", CountyName="Jackson", StateName="OR"},

                new City() {Name="Santa Barbara", CountyName="Santa Barbara", StateName="CA"},
                new City() {Name="Orange", CountyName="Orange", StateName="CA"},

                new City() {Name="Oregon", CountyName="Lucas", StateName="OH"},
                new City() {Name="Toledo", CountyName="Lucas", StateName="OH"}
            };

            _citiesWithInfo = new List<CityWithInfo>(this._cities.OfType<CityWithInfo>());

            foreach (County county in _counties)
            {
                foreach (City city in _cities.Where(p => p.CountyName == county.Name && p.StateName == county.StateName))
                {
                    county.Cities.Add(city);
                    city.County = county;
                }
            }

            _zips = new List<Zip>()
            {
                new Zip() { Code=98053, FourDigit=8625, CityName="Redmond", CountyName="King", StateName="WA" },
                new ZipWithInfo() { Code=98052, FourDigit=8300, CityName="Redmond", CountyName="King", StateName="WA", Info="Microsoft" },
                new Zip() { Code=98052, FourDigit=6399, CityName="Redmond", CountyName="King", StateName="WA" },
            };

            _zipsWithInfo = new List<ZipWithInfo>(this._zips.OfType<ZipWithInfo>());

            foreach (City city in _cities)
            {
                foreach (Zip zip in _zips.Where(p => p.CityName == city.Name && p.CountyName == city.CountyName && p.StateName == city.StateName))
                {
                    city.ZipCodes.Add(zip);
                    zip.City = city;
                }
            }

            foreach (CityWithInfo city in _citiesWithInfo)
            {
                foreach (ZipWithInfo zip in _zipsWithInfo.Where(p => p.CityName == city.Name && p.CountyName == city.CountyName && p.StateName == city.StateName))
                {
                    city.ZipCodesWithInfo.Add(zip);
                    zip.City = city;
                }
            }
        }

        public List<State> States { get { return this._states; } }
        public List<County> Counties { get { return this._counties; } }
        public List<City> Cities { get { return this._cities; } }
        public List<CityWithInfo> CitiesWithInfo { get { return this._citiesWithInfo; } }
        public List<Zip> Zips { get { return this._zips; } }
        public List<ZipWithInfo> ZipsWithInfo { get { return this._zipsWithInfo; } }
    }

    /// <summary>
    /// These types are simple data types that can be used to build
    /// mocks and simple data stores.
    /// </summary>
    public partial class State
    {
        private readonly List<County> _counties = new List<County>();

        [Key]
        public string Name { get; set; }
        [Key]
        public string FullName { get; set; }
        public TimeZone TimeZone { get; set; }
        public ShippingZone ShippingZone { get; set; }
        public List<County> Counties { get { return this._counties; } }
    }

    [DataContract(Name = "CityName", Namespace = "CityNamespace")]
    public enum ShippingZone
    {
        [EnumMember(Value = "P")]
        Pacific = 0,    // default

        [EnumMember(Value = "C")]
        Central,

        [EnumMember(Value = "E")]
        Eastern
    }

    public enum TimeZone
    {
        Central,
        Mountain,
        Eastern,
        Pacific
    }

    public partial class County
    {
        public County()
        {
            Cities = new List<City>();
        }

        [Key]
        public string Name { get; set; }
        [Key]
        public string StateName { get; set; }

        [IgnoreDataMember]
        public State State { get; set; }

        public List<City> Cities { get; set; }
    }

    [KnownType(typeof(CityWithEditHistory))]
    [KnownType(typeof(CityWithInfo))]
    public partial class City
    {
        public City()
        {
            ZipCodes = new List<Zip>();
        }

        [Key]
        public string Name { get; set; }
        [Key]
        public string CountyName { get; set; }
        [Key]
        public string StateName { get; set; }

        [IgnoreDataMember]
        public County County { get; set; }
        public string ZoneName { get; set; }
        public string CalculatedCounty { get { return this.CountyName; } set { } }
        public int ZoneID { get; set; }

        public List<Zip> ZipCodes { get; set; }

        public override string ToString()
        {
            return this.GetType().Name + " Name=" + this.Name + ", State=" + this.StateName + ", County=" + this.CountyName;
        }

        public int this[int index]
        {
            get
            {
                return index;
            }
            set
            {
            }
        }
    }

    public abstract partial class CityWithEditHistory : City
    {
        private string _editHistory;

        public CityWithEditHistory()
        {
            this.EditHistory = "new";
        }

        // Edit history always appends, never overwrites
        public string EditHistory
        {
            get
            {
                return this._editHistory;
            }
            set
            {
                this._editHistory = this._editHistory == null ? value : (this._editHistory + "," + value);
                this.LastUpdated = DateTime.Now;
            }
        }

        public DateTime LastUpdated
        {
            get;
            set;
        }

        public override string ToString()
        {
            return base.ToString() + ", History=" + this.EditHistory + ", Updated=" + this.LastUpdated;
        }

    }

    public partial class CityWithInfo : CityWithEditHistory
    {
        public CityWithInfo()
        {
            ZipCodesWithInfo = new List<ZipWithInfo>();
        }

        public string Info
        {
            get;
            set;
        }

        public List<ZipWithInfo> ZipCodesWithInfo { get; set; }

        public override string ToString()
        {
            return base.ToString() + ", Info=" + this.Info;
        }

    }

    [KnownType(typeof(ZipWithInfo))]
    public partial class Zip
    {
        [Key]
        public int Code { get; set; }
        [Key]
        public int FourDigit { get; set; }
        public string CityName { get; set; }
        public string CountyName { get; set; }
        public string StateName { get; set; }

        [IgnoreDataMember]
        public City City { get; set; }
    }

    public partial class ZipWithInfo : Zip
    {
        public string Info
        {
            get;
            set;
        }
    }
}
