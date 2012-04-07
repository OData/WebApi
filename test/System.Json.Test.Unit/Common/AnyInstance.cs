// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Json
{
    public static class AnyInstance
    {
        public const bool AnyBool = true;
        public const string AnyString = "hello";
        public const string AnyString2 = "world";
        public const char AnyChar = 'c';
        public const int AnyInt = 123456789;
        [CLSCompliant(false)]
        public const uint AnyUInt = 3123456789;
        public const long AnyLong = 123456789012345L;
        [CLSCompliant(false)]
        public const ulong AnyULong = UInt64.MaxValue;
        public const short AnyShort = -12345;
        [CLSCompliant(false)]
        public const ushort AnyUShort = 40000;
        public const byte AnyByte = 0xDC;
        [CLSCompliant(false)]
        public const sbyte AnySByte = -34;
        public const double AnyDouble = 123.45;
        public const float AnyFloat = 23.4f;
        public const decimal AnyDecimal = 1234.5678m;
        public static readonly Guid AnyGuid = new Guid(0x11223344, 0x5566, 0x7788, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00);
        public static readonly DateTime AnyDateTime = new DateTime(2010, 02, 15, 22, 45, 20, DateTimeKind.Utc);
        public static readonly DateTimeOffset AnyDateTimeOffset = new DateTimeOffset(2010, 2, 5, 15, 45, 20, TimeSpan.FromHours(-3));
        public static readonly Uri AnyUri = new Uri("http://tempuri.org/");

        public static readonly JsonArray AnyJsonArray;
        public static readonly JsonObject AnyJsonObject;

        public static readonly JsonPrimitive AnyJsonPrimitive = new JsonPrimitive("hello");

        public static readonly JsonValue AnyJsonValue1 = AnyJsonPrimitive;
        public static readonly JsonValue AnyJsonValue2;
        public static readonly JsonValue AnyJsonValue3 = null;

        public static readonly JsonValue DefaultJsonValue = GetDefaultJsonValue();

        public static readonly Person AnyPerson = Person.CreateSample();
        public static readonly Address AnyAddress = Address.CreateSample();

        public static readonly dynamic AnyDynamic = TestDynamicObject.CreatePersonAsDynamic(AnyPerson);

        public static JsonValue[] AnyJsonValueArray
        {
            get
            {
                return new JsonValue[]
                {
                    AnyInstance.AnyJsonArray,
                    AnyInstance.AnyJsonObject,
                    AnyInstance.AnyJsonPrimitive,
                    AnyInstance.DefaultJsonValue
                };
            }
        }

        static AnyInstance()
        {
            AnyJsonArray = new JsonArray { 1, 2, 3 };
            AnyJsonObject = new JsonObject { { "one", 1 }, { "two", 2 } };
            AnyJsonArray.Changing += new EventHandler<JsonValueChangeEventArgs>(PreventChanging);
            AnyJsonObject.Changing += new EventHandler<JsonValueChangeEventArgs>(PreventChanging);
            AnyJsonValue2 = AnyJsonArray;
        }

        private static void PreventChanging(object sender, JsonValueChangeEventArgs e)
        {
            throw new InvalidOperationException("AnyInstance.AnyJsonArray or AnyJsonObject cannot be modified; please clone the instance if the test needs to change it.");
        }

        private static JsonValue GetDefaultJsonValue()
        {
            PropertyInfo propInfo = typeof(JsonValue).GetProperty("DefaultInstance", BindingFlags.Static | BindingFlags.NonPublic);
            return propInfo.GetValue(null, null) as JsonValue;
        }
    }

    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public Address Address { get; set; }

        public List<Person> Friends { get; set; }

        public static Person CreateSample()
        {
            Person anyObject = new Person
            {
                Name = AnyInstance.AnyString,
                Age = AnyInstance.AnyInt,
                Address = Address.CreateSample(),
                Friends = new List<Person> { new Person { Name = "Bill Gates", Age = 23, Address = Address.CreateSample() }, new Person { Name = "Steve Ballmer", Age = 19, Address = Address.CreateSample() } }
            };

            return anyObject;
        }

        public string FriendsToString()
        {
            string s = "";

            if (this.Friends != null)
            {
                foreach (Person p in this.Friends)
                {
                    s += p + ",";
                }
            }

            return s;
        }

        public override string ToString()
        {
            return String.Format("{0}, {1}, [{2}], Friends=[{3}]", this.Name, this.Age, this.Address, this.FriendsToString());
        }
    }

    public class Address
    {
        public const string AnyStreet = "123 1st Ave";

        public const string AnyCity = "Springfield";

        public const string AnyState = "ZZ";

        public string Street { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public static Address CreateSample()
        {
            Address address = new Address
            {
                Street = AnyStreet,
                City = AnyCity,
                State = AnyState,
            };

            return address;
        }

        public override string ToString()
        {
            return String.Format("{0}, {1}, {2}", this.Street, this.City, this.State);
        }
    }

    public class TestDynamicObject : DynamicObject
    {
        private IDictionary<string, object> _values = new Dictionary<string, object>();

        public bool UseFallbackMethod { get; set; }
        public bool UseErrorSuggestion { get; set; }

        public string TestProperty { get; set; }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _values.Keys;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _values[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.UseFallbackMethod)
            {
                DynamicMetaObject target = new DynamicMetaObject(Expression.Parameter(this.GetType()), BindingRestrictions.Empty);
                DynamicMetaObject errorSuggestion = null;

                if (this.UseErrorSuggestion)
                {
                    errorSuggestion = new DynamicMetaObject(Expression.Throw(Expression.Constant(new TestDynamicObjectException())), BindingRestrictions.Empty);
                }

                DynamicMetaObject metaObj = binder.FallbackGetMember(target, errorSuggestion);
                Expression<Action> lambda = Expression.Lambda<Action>(metaObj.Expression, new ParameterExpression[] { });
                lambda.Compile().Invoke();
            }

            return _values.TryGetValue(binder.Name, out result);
        }

        public static dynamic CreatePersonAsDynamic(Person person)
        {
            dynamic dynObj = new TestDynamicObject();

            dynObj.Name = person.Name;
            dynObj.Age = person.Age;
            dynObj.Address = new Address();
            dynObj.Address.City = person.Address.City;
            dynObj.Address.Street = person.Address.Street;
            dynObj.Address.State = person.Address.State;
            dynObj.Friends = person.Friends;

            return dynObj;
        }

        public class TestDynamicObjectException : Exception
        {
        }
    }
}
