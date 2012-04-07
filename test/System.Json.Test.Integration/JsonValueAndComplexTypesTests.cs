// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Xunit;

namespace System.Json
{
    /// <summary>
    /// Tests for the methods to convert between <see cref="JsonValue"/> instances and complex types.
    /// </summary>
    public class JsonValueAndComplexTypesTests
    {
        static readonly Type[] testTypes = new Type[]
            {
                typeof(DCType_1),
                typeof(StructGuid),
                typeof(StructInt16),
                typeof(DCType_3),
                typeof(SerType_4),
                typeof(SerType_5),
                typeof(DCType_7),
                typeof(DCType_9),
                typeof(SerType_11),
                typeof(DCType_15),
                typeof(DCType_16),
                typeof(DCType_18),
                typeof(DCType_19),
                typeof(DCType_20),
                typeof(SerType_22),
                typeof(DCType_25),
                typeof(SerType_26),
                typeof(DCType_31),
                typeof(DCType_32),
                typeof(SerType_33),
                typeof(DCType_34),
                typeof(DCType_36),
                typeof(DCType_38),
                typeof(DCType_40),
                typeof(DCType_42),
                typeof(DCType_65),
                typeof(ListType_1),
                typeof(ListType_2),
                typeof(BaseType),
                typeof(PolymorphicMember),
                typeof(PolymorphicAsInterfaceMember),
                typeof(CollectionsWithPolymorphicMember),
            };

        /// <summary>
        /// Tests for the <see cref="JsonValueExtensions.CreateFrom"/> method.
        /// </summary>
        [Fact]
        public void CreateFromTests()
        {
            InstanceCreatorSurrogate oldSurrogate = CreatorSettings.CreatorSurrogate;
            try
            {
                CreatorSettings.CreatorSurrogate = new NoInfinityFloatSurrogate();
                DateTime now = DateTime.Now;
                int seed = (10000 * now.Year) + (100 * now.Month) + now.Day;
                Log.Info("Seed: {0}", seed);
                Random rndGen = new Random(seed);
                foreach (Type testType in testTypes)
                {
                    object instance = InstanceCreator.CreateInstanceOf(testType, rndGen);
                    JsonValue jv = JsonValueExtensions.CreateFrom(instance);

                    if (instance == null)
                    {
                        Assert.Null(jv);
                    }
                    else
                    {
                        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(instance == null ? testType : instance.GetType());
                        string fromDCJS;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            dcjs.WriteObject(ms, instance);
                            fromDCJS = Encoding.UTF8.GetString(ms.ToArray());
                        }

                        Log.Info("{0}: {1}", testType.Name, fromDCJS);

                        if (instance == null)
                        {
                            Assert.Null(jv);
                        }
                        else
                        {
                            string fromJsonValue = jv.ToString();
                            Assert.Equal(fromDCJS, fromJsonValue);
                        }
                    }
                }
            }
            finally
            {
                CreatorSettings.CreatorSurrogate = oldSurrogate;
            }
        }

        /// <summary>
        /// Tests for the <see cref="JsonValueExtensions.ReadAsType{T}(JsonValue)"/> method.
        /// </summary>
        [Fact]
        public void ReadAsTests()
        {
            InstanceCreatorSurrogate oldSurrogate = CreatorSettings.CreatorSurrogate;
            try
            {
                CreatorSettings.CreatorSurrogate = new NoInfinityFloatSurrogate();
                DateTime now = DateTime.Now;
                int seed = (10000 * now.Year) + (100 * now.Month) + now.Day;
                Log.Info("Seed: {0}", seed);
                Random rndGen = new Random(seed);

                this.ReadAsTest<DCType_1>(rndGen);
                this.ReadAsTest<StructGuid>(rndGen);
                this.ReadAsTest<StructInt16>(rndGen);
                this.ReadAsTest<DCType_3>(rndGen);
                this.ReadAsTest<SerType_4>(rndGen);
                this.ReadAsTest<SerType_5>(rndGen);
                this.ReadAsTest<DCType_7>(rndGen);
                this.ReadAsTest<DCType_9>(rndGen);
                this.ReadAsTest<SerType_11>(rndGen);
                this.ReadAsTest<DCType_15>(rndGen);
                this.ReadAsTest<DCType_16>(rndGen);
                this.ReadAsTest<DCType_18>(rndGen);
                this.ReadAsTest<DCType_19>(rndGen);
                this.ReadAsTest<DCType_20>(rndGen);
                this.ReadAsTest<SerType_22>(rndGen);
                this.ReadAsTest<DCType_25>(rndGen);
                this.ReadAsTest<SerType_26>(rndGen);
                this.ReadAsTest<DCType_31>(rndGen);
                this.ReadAsTest<DCType_32>(rndGen);
                this.ReadAsTest<SerType_33>(rndGen);
                this.ReadAsTest<DCType_34>(rndGen);
                this.ReadAsTest<DCType_36>(rndGen);
                this.ReadAsTest<DCType_38>(rndGen);
                this.ReadAsTest<DCType_40>(rndGen);
                this.ReadAsTest<DCType_42>(rndGen);
                this.ReadAsTest<DCType_65>(rndGen);
                this.ReadAsTest<ListType_1>(rndGen);
                this.ReadAsTest<ListType_2>(rndGen);
                this.ReadAsTest<BaseType>(rndGen);
                this.ReadAsTest<PolymorphicMember>(rndGen);
                this.ReadAsTest<PolymorphicAsInterfaceMember>(rndGen);
                this.ReadAsTest<CollectionsWithPolymorphicMember>(rndGen);
            }
            finally
            {
                CreatorSettings.CreatorSurrogate = oldSurrogate;
            }
        }

        /// <summary>
        /// Tests for the <see cref="JsonValueExtensions.CreateFrom"/> for <see cref="DateTime"/>
        /// and <see cref="DateTimeOffset"/> values.
        /// </summary>
        [Fact]
        public void CreateFromDateTimeTest()
        {
            DateTime dt = DateTime.Now;
            DateTimeOffset dto = DateTimeOffset.Now;

            JsonValue jvDt1 = (JsonValue)dt;
            JsonValue jvDt2 = JsonValueExtensions.CreateFrom(dt);

            JsonValue jvDto1 = (JsonValue)dto;
            JsonValue jvDto2 = JsonValueExtensions.CreateFrom(dto);

            Assert.Equal(dt, (DateTime)jvDt1);
            Assert.Equal(dt, (DateTime)jvDt2);

            Assert.Equal(dto, (DateTimeOffset)jvDto1);
            Assert.Equal(dto, (DateTimeOffset)jvDto2);

            Assert.Equal(dt, jvDt1.ReadAs<DateTime>());
            Assert.Equal(dt, jvDt2.ReadAs<DateTime>());

            Assert.Equal(dto, jvDto1.ReadAs<DateTimeOffset>());
            Assert.Equal(dto, jvDto2.ReadAs<DateTimeOffset>());

            Assert.Equal(jvDt1.ToString(), jvDt2.ToString());
            Assert.Equal(jvDto1.ToString(), jvDto2.ToString());
        }

        /// <summary>
        /// Tests for creating <see cref="JsonValue"/> instances from dynamic objects.
        /// </summary>
        [Fact]
        public void CreateFromDynamic()
        {
            string expectedJson = "{\"int\":12,\"str\":\"hello\",\"jv\":[1,{\"a\":true}],\"dyn\":{\"char\":\"c\",\"null\":null}}";
            MyDynamicObject obj = new MyDynamicObject();
            obj.fields.Add("int", 12);
            obj.fields.Add("str", "hello");
            obj.fields.Add("jv", new JsonArray(1, new JsonObject { { "a", true } }));
            MyDynamicObject dyn = new MyDynamicObject();
            obj.fields.Add("dyn", dyn);
            dyn.fields.Add("char", 'c');
            dyn.fields.Add("null", null);

            JsonValue jv = JsonValueExtensions.CreateFrom(obj);
            Assert.Equal(expectedJson, jv.ToString());
        }

        void ReadAsTest<T>(Random rndGen)
        {
            T instance = InstanceCreator.CreateInstanceOf<T>(rndGen);
            Log.Info("ReadAsTest<{0}>, instance = {1}", typeof(T).Name, instance);
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(T));
            JsonValue jv;
            using (MemoryStream ms = new MemoryStream())
            {
                dcjs.WriteObject(ms, instance);
                Log.Info("{0}: {1}", typeof(T).Name, Encoding.UTF8.GetString(ms.ToArray()));
                ms.Position = 0;
                jv = JsonValue.Load(ms);
            }

            if (instance == null)
            {
                Assert.Null(jv);
            }
            else
            {
                T newInstance = jv.ReadAsType<T>();
                Assert.Equal(instance, newInstance);
            }
        }

        /// <summary>
        /// Test class.
        /// </summary>
        public class MyDynamicObject : DynamicObject
        {
            /// <summary>
            /// Test member
            /// </summary>
            public Dictionary<string, object> fields = new Dictionary<string, object>();

            /// <summary>
            /// Returnes the member names in this dynamic object.
            /// </summary>
            /// <returns>The member names in this dynamic object.</returns>
            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return fields.Keys;
            }

            /// <summary>
            /// Attempts to get a named member from this dynamic object.
            /// </summary>
            /// <param name="binder">The dynamic binder which contains the member name.</param>
            /// <param name="result">The value of the member, if it exists in this dynamic object.</param>
            /// <returns><code>true</code> if the member can be returned; <code>false</code> otherwise.</returns>
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (binder != null && binder.Name != null && this.fields.ContainsKey(binder.Name))
                {
                    result = this.fields[binder.Name];
                    return true;
                }

                return base.TryGetMember(binder, out result);
            }

            /// <summary>
            /// Attempts to set a named member from this dynamic object.
            /// </summary>
            /// <param name="binder">The dynamic binder which contains the member name.</param>
            /// <param name="value">The value of the member to be set.</param>
            /// <returns><code>true</code> if the member can be set; <code>false</code> otherwise.</returns>
            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                if (binder != null && binder.Name != null)
                {
                    this.fields[binder.Name] = value;
                    return true;
                }

                return base.TrySetMember(binder, value);
            }
        }

        // Currently there are some differences in treatment of infinity between
        // JsonValue (which writes them as Infinity/-Infinity) and DataContractJsonSerializer
        // (which writes them as INF/-INF). This prevents those values from being used in the test.
        // This also allows the creation of an instance of an IEmptyInterface type, used in the test.
        class NoInfinityFloatSurrogate : InstanceCreatorSurrogate
        {
            public override bool CanCreateInstanceOf(Type type)
            {
                return type == typeof(float) || type == typeof(double) || type == typeof(IEmptyInterface) || type == typeof(BaseType);
            }

            public override object CreateInstanceOf(Type type, Random rndGen)
            {
                if (type == typeof(float))
                {
                    float result;
                    do
                    {
                        result = PrimitiveCreator.CreateInstanceOfSingle(rndGen);
                    }
                    while (float.IsInfinity(result));
                    return result;
                }
                else if (type == typeof(double))
                {
                    double result;
                    do
                    {
                        result = PrimitiveCreator.CreateInstanceOfDouble(rndGen);
                    }
                    while (double.IsInfinity(result));
                    return result;
                }
                else
                {
                    return new DerivedType(rndGen);
                }
            }
        }
    }
}
