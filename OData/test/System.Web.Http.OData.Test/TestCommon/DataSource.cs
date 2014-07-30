// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.TestCommon.Models;

namespace System.Web.Http.OData
{
    internal class DataSource
    {
        public static int MaxIndex = 4;

        public static string[] Names = new string[] { "Frank", "Steve", "Tom", "Chandler", "Ross" };

        public static string[] SSN = new string[] { "556-99-7890", "556-98-7898", "556-98-7789", "556-98-7777", "556-98-6666" };

        public static PhoneNumber[] HomePhoneNumbers = new PhoneNumber[]
        { 
            new PhoneNumber() { CountryCode = 1, AreaCode = 425, Number = 9879089, PhoneType = PhoneType.HomePhone },
            new PhoneNumber() { CountryCode = 1, AreaCode = 425, Number = 9879090, PhoneType = PhoneType.HomePhone },
            new PhoneNumber() { CountryCode = 1, AreaCode = 425, Number = 9879091, PhoneType = PhoneType.HomePhone },
            new PhoneNumber() { CountryCode = 1, AreaCode = 425, Number = 9879092, PhoneType = PhoneType.HomePhone },
            new PhoneNumber() { CountryCode = 1, AreaCode = 425, Number = 9879093, PhoneType = PhoneType.HomePhone }
        };

        public static PhoneNumber[] WorkPhoneNumbers = new PhoneNumber[]
        { 
            new PhoneNumber() { CountryCode = 1, AreaCode = 908, Number = 9879089, PhoneType = PhoneType.WorkPhone },
            new PhoneNumber() { CountryCode = 1, AreaCode = 908, Number = 9879090, PhoneType = PhoneType.WorkPhone },
            new PhoneNumber() { CountryCode = 1, AreaCode = 908, Number = 9879091, PhoneType = PhoneType.WorkPhone },
            new PhoneNumber() { CountryCode = 1, AreaCode = 908, Number = 9879092, PhoneType = PhoneType.WorkPhone },
            new PhoneNumber() { CountryCode = 1, AreaCode = 908, Number = 9879093, PhoneType = PhoneType.WorkPhone }
        };

        public static Address[] Address = new Address[]
        {
            new Address() { StreetAddress = "StreetAddress1", City = "City1", State = "State1", ZipCode = 1 },
            new Address() { StreetAddress = "StreetAddress2", City = "City2", State = "State2", ZipCode = 2 },
            new Address() { StreetAddress = "StreetAddress3", City = "City3", State = "State3", ZipCode = 3 },
            new Address() { StreetAddress = "StreetAddress4", City = "City4", State = "State4", ZipCode = 4 },
            new Address() { StreetAddress = "StreetAddress5", City = "City5", State = "State5", ZipCode = 5 },
        };
    }
}
