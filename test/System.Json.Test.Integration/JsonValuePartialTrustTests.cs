// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Security;
using System.Security.Policy;
using Xunit;

namespace System.Json
{
    /// <summary>
    /// Tests for using the <see cref="JsonValue"/> types in partial trust.
    /// </summary>
    [Serializable]
    public class JsonValuePartialTrustTests
    {
        /// <summary>
        /// Validates the condition, throwing an exception if it is false.
        /// </summary>
        /// <param name="condition">The condition to be evaluated.</param>
        /// <param name="msg">The exception message to be thrown, in case the condition is false.</param>
        public static void AssertIsTrue(bool condition, string msg)
        {
            if (!condition)
            {
                throw new InvalidOperationException(msg);
            }
        }

        /// <summary>
        /// Validates that the two objects are equal, throwing an exception if it is false.
        /// </summary>
        /// <param name="obj1">The first object to be compared.</param>
        /// <param name="obj2">The second object to be compared.</param>
        /// <param name="msg">The exception message to be thrown, in case the condition is false.</param>
        public static void AssertAreEqual(object obj1, object obj2, string msg)
        {
            if (obj1 == obj2)
            {
                return;
            }

            if (obj1 == null || obj2 == null || !obj1.Equals(obj2))
            {
                throw new InvalidOperationException(String.Format("[{0}, {2}] and [{1}, {3}] expected to be equal. {4}", obj1, obj2, obj1.GetType().Name, obj2.GetType().Name, msg));
            }
        }

        /// <summary>
        /// Partial trust tests for <see cref="JsonValue"/> instances where no dynamic references are used.
        /// </summary>
        [Fact(Skip = "Re-enable when CSDMain 216528: 'Partial trust support for Web API' has been fixed")]
        public void RunNonDynamicTest()
        {
            RunInPartialTrust(this.NonDynamicTest);
        }

        /// <summary>
        /// Partial trust tests for <see cref="JsonValue"/> with dynamic references.
        /// </summary>
        [Fact(Skip = "Re-enable when CSDMain 216528: 'Partial trust support for Web API' has been fixed")]
        public void RunDynamicTest()
        {
            RunInPartialTrust(this.DynamicTest);
        }

        /// <summary>
        /// Tests for <see cref="JsonValue"/> instances without dynamic references.
        /// </summary>
        public void NonDynamicTest()
        {
            int seed = GetRandomSeed();
            Log.Info("Seed: {0}", seed);
            Random rndGen = new Random(seed);

            AssertIsTrue(Assembly.GetExecutingAssembly().IsFullyTrusted == false, "Executing assembly not expected to be fully trusted!");

            Person person = new Person(rndGen);
            Person person2 = new Person(rndGen);

            person.AddFriends(3, rndGen);
            person2.AddFriends(3, rndGen);

            JsonValue jo = JsonValueExtensions.CreateFrom(person);
            JsonValue jo2 = JsonValueExtensions.CreateFrom(person2);

            AssertAreEqual(person.Address.City, jo["Address"]["City"].ReadAs<string>(), "Address.City");
            AssertAreEqual(person.Friends[1].Age, jo["Friends"][1]["Age"].ReadAs<int>(), "Friends[1].Age");

            string newCityName = "Bellevue";

            jo["Address"]["City"] = newCityName;
            AssertAreEqual(newCityName, (string)jo["Address"]["City"], "Address.City2");

            jo["Friends"][1] = jo2;
            AssertAreEqual(person2.Age, (int)jo["Friends"][1]["Age"], "Friends[1].Age2");

            AssertAreEqual(person2.Address.City, jo.ValueOrDefault("Friends").ValueOrDefault(1).ValueOrDefault("Address").ValueOrDefault("City").ReadAs<string>(), "Address.City3");
            AssertAreEqual(person2.Age, (int)jo.ValueOrDefault("Friends").ValueOrDefault(1).ValueOrDefault("Age"), "Friends[1].Age3");

            AssertAreEqual(person2.Address.City, jo.ValueOrDefault("Friends", 1, "Address", "City").ReadAs<string>(), "Address.City3");
            AssertAreEqual(person2.Age, (int)jo.ValueOrDefault("Friends", 1, "Age"), "Friends[1].Age3");

            int newAge = 42;
            JsonValue ageValue = jo["Friends"][1]["Age"] = newAge;
            AssertAreEqual(newAge, (int)ageValue, "Friends[1].Age4");
        }

        /// <summary>
        /// Tests for <see cref="JsonValue"/> instances with dynamic references.
        /// </summary>
        public void DynamicTest()
        {
            int seed = GetRandomSeed();
            Log.Info("Seed: {0}", seed);
            Random rndGen = new Random(seed);

            AssertIsTrue(Assembly.GetExecutingAssembly().IsFullyTrusted == false, "Executing assembly not expected to be fully trusted!");

            Person person = new Person(rndGen);
            person.AddFriends(1, rndGen);

            dynamic jo = JsonValueExtensions.CreateFrom(person);

            AssertAreEqual(person.Friends[0].Name, jo.Friends[0].Name.ReadAs<string>(), "Friends[0].Name");
            AssertAreEqual(person.Address.City, jo.Address.City.ReadAs<string>(), "Address.City");
            AssertAreEqual(person.Friends[0].Age, (int)jo.Friends[0].Age, "Friends[0].Age");

            string newCityName = "Bellevue";

            jo.Address.City = newCityName;
            AssertAreEqual(newCityName, (string)jo.Address.City, "Address.City2");

            AssertAreEqual(person.Friends[0].Address.City, jo.ValueOrDefault("Friends").ValueOrDefault(0).ValueOrDefault("Address").ValueOrDefault("City").ReadAs<string>(), "Friends[0].Address.City");
            AssertAreEqual(person.Friends[0].Age, (int)jo.ValueOrDefault("Friends").ValueOrDefault(0).ValueOrDefault("Age"), "Friends[0].Age2");

            AssertAreEqual(person.Friends[0].Address.City, jo.ValueOrDefault("Friends", 0, "Address", "City").ReadAs<string>(), "Friends[0].Address.City");
            AssertAreEqual(person.Friends[0].Age, (int)jo.ValueOrDefault("Friends", 0, "Age"), "Friends[0].Age2");

            int newAge = 42;
            JsonValue ageValue = jo.Friends[0].Age = newAge;
            AssertAreEqual(newAge, (int)ageValue, "Friends[0].Age3");

            AssertIsTrue(jo.NonExistentProperty is JsonValue, "Expected a JsonValue");
            AssertIsTrue(jo.NonExistentProperty.JsonType == JsonType.Default, "Expected default JsonValue");
        }

        private static void RunInPartialTrust(CrossAppDomainDelegate testMethod)
        {
            Assert.True(Assembly.GetExecutingAssembly().IsFullyTrusted);

            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            PermissionSet perms = PermissionsHelper.InternetZone;
            AppDomain domain = AppDomain.CreateDomain("PartialTrustSandBox", null, setup, perms);

            domain.DoCallBack(testMethod);
        }

        private static int GetRandomSeed()
        {
            DateTime now = DateTime.Now;
            return (now.Year * 10000) + (now.Month * 100) + now.Day;
        }

        internal static class PermissionsHelper
        {
            private static PermissionSet internetZone;

            public static PermissionSet InternetZone
            {
                get
                {
                    if (internetZone == null)
                    {
                        Evidence evidence = new Evidence();
                        evidence.AddHostEvidence(new Zone(SecurityZone.Internet));

                        internetZone = SecurityManager.GetStandardSandbox(evidence);
                    }

                    return internetZone;
                }
            }
        }
    }
}
