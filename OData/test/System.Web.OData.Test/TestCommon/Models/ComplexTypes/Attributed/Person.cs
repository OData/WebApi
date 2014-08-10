// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;

namespace System.Web.OData.TestCommon.Models
{
    [DataContract]
    public class Person
    {
        [DataMember]
        public int Age { get; set; }

        [DataMember]
        public Gender Gender { get; set; }

        [DataMember(IsRequired = true)]
        public string FirstName { get; set; }

        [DataMember]
        public string[] Alias { get; set; }

        [DataMember]
        public Address Address { get; set; }

        [DataMember]
        public PhoneNumber HomeNumber { get; set; }

        public string UnserializableSSN { get; set; }

        [DataMember]
        public HobbyActivity FavoriteHobby { get; set; }

        public Person(int index, ReferenceDepthContext context)
        {
            this.Age = index + 20;
            this.Address = new Address(index, context);
            this.Alias = new string[] { "Alias" + index };
            this.FirstName = DataSource.Names[index];
            this.Gender = Gender.Male;
            this.HomeNumber = DataSource.HomePhoneNumbers[index];
            this.UnserializableSSN = DataSource.SSN[index];
            this.FavoriteHobby = new HobbyActivity("Xbox Gaming");
        }

        [DataContract]
        public class HobbyActivity : IActivity
        {
            public HobbyActivity(string hobbyName)
            {
                this.ActivityName = hobbyName;
            }

            [DataMember]
            public string ActivityName
            {
                get;
                set;
            }

            public void DoActivity()
            {
                // Some Action
            }
        }
    }
}
