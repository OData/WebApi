//-----------------------------------------------------------------------------
// <copyright file="TypedServiceReferenceForOpenType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Test.E2E.AspNet.OData.OpenType.Typed.Client
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public partial class Account
    {
        public Account()
        {
            ShipAddresses = new List<Address>();
            LuckyNumbers = new List<int>();
            Emails = new List<string>();
        }
        public string OwnerAlias { get; set; }
        public Gender? OwnerGender { get; set; }
        public bool? IsValid { get; set; }
        public IList<Address> ShipAddresses { get; set; }
        public IList<int> LuckyNumbers { get; set; }
        public IList<string> Emails { get; set; }
    }

    public partial class Manager
    {
        public int? Level { get; set; }
        public Gender? Gender { get; set; }
        public IList<string> PhoneNumbers { get; set; }
    }

    public partial class AccountInfo
    {
        public AccountInfo()
        {
            Subs = new Collection<string>();
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "1.0.0")]
        public int Age
        {
            get
            {
                return this._Age;
            }
            set
            {
                this.OnAgeChanging(value);
                this._Age = value;
                this.OnAgeChanged();
                this.OnPropertyChanged("Age");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "1.0.0")]
        private int _Age;
        partial void OnAgeChanging(int value);
        partial void OnAgeChanged();

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "1.0.0")]
        public Nullable<Gender> Gender
        {
            get
            {
                return this._Gender;
            }
            set
            {
                this.OnGenderChanging(value);
                this._Gender = value;
                this.OnGenderChanged();
                this.OnPropertyChanged("Gender");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "1.0.0")]
        private Nullable<Gender> _Gender;
        partial void OnGenderChanging(Nullable<Gender> value);
        partial void OnGenderChanged();

        public Collection<string> Subs { get; set; }
    }

    public partial class Address
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "1.0.0")]
        public string CountryOrRegion
        {
            get
            {
                return this._countryOrRegion;
            }
            set
            {
                this.OnCountryChanging(value);
                this._countryOrRegion = value;
                this.OnCountryChanged();
                this.OnPropertyChanged("CountryOrRegion");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "1.0.0")]
        private string _countryOrRegion;
        partial void OnCountryChanging(string value);
        partial void OnCountryChanged();
    }

    public partial class Tags
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "1.0.0")]
        public string Tag1
        {
            get
            {
                return this._Tag1;
            }
            set
            {
                this.OnTag1Changing(value);
                this._Tag1 = value;
                this.OnTag1Changed();
                this.OnPropertyChanged("Tag1");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "1.0.0")]
        private string _Tag1;
        partial void OnTag1Changing(string value);
        partial void OnTag1Changed();

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "1.0.0")]
        public string Tag2
        {
            get
            {
                return this._Tag2;
            }
            set
            {
                this.OnTag1Changing(value);
                this._Tag2 = value;
                this.OnTag2Changed();
                this.OnPropertyChanged("Tag2");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "1.0.0")]
        private string _Tag2;
        partial void OnTag2Changing(string value);
        partial void OnTag2Changed();
    }
}
