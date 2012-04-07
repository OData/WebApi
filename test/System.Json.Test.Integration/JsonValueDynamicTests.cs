// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization.Json;
using Xunit;

namespace System.Json
{
    /// <summary>
    /// Tests for the dynamic support for <see cref="JsonValue"/>.
    /// </summary>
    public class JsonValueDynamicTests
    {
        string teamNameValue = "WCF RIA Base";
        string[] teamMembersValues = { "Carlos", "Chris", "Joe", "Miguel", "Yavor" };

        /// <summary>
        /// Tests for the dynamic getters in <see cref="JsonObject"/> instances.
        /// </summary>
        [Fact]
        public void JsonObjectDynamicGetters()
        {
            dynamic team = new JsonObject();
            team["TeamSize"] = this.teamMembersValues.Length;
            team["TeamName"] = this.teamNameValue;
            team["TeamMascots"] = null;
            team["TeamMembers"] = new JsonArray 
            { 
                this.teamMembersValues[0], this.teamMembersValues[1], this.teamMembersValues[2],
                this.teamMembersValues[3], this.teamMembersValues[4] 
            };

            Assert.Equal(this.teamMembersValues.Length, (int)team.TeamSize);
            Assert.Equal(this.teamNameValue, (string)team.TeamName);
            Assert.NotNull(team.TeamMascots);
            Assert.True(team.TeamMascots is JsonValue); // default

            for (int i = 0; i < this.teamMembersValues.Length; i++)
            {
                Assert.Equal(this.teamMembersValues[i], (string)team.TeamMembers[i]);
            }

            for (int i = 0; i < this.teamMembersValues.Length; i++)
            {
                Assert.Equal(this.teamMembersValues[i], (string)team.TeamMembers[i]);
            }

            // Negative tests for getters
            JsonValueTests.ExpectException<InvalidCastException>(delegate { int fail = (int)team.NonExistentProp; });
        }

        /// <summary>
        /// Tests for the dynamic setters in <see cref="JsonObject"/> instances.
        /// </summary>
        [Fact]
        public void JsonObjectDynamicSetters()
        {
            dynamic team = new JsonObject();
            team.TeamSize = this.teamMembersValues.Length;
            team.TeamName = this.teamNameValue;
            team.TeamMascots = null;
            team.TeamMembers = new JsonArray 
            { 
                this.teamMembersValues[0], this.teamMembersValues[1], this.teamMembersValues[2],
                this.teamMembersValues[3], this.teamMembersValues[4] 
            };

            Assert.Equal(this.teamMembersValues.Length, (int)team["TeamSize"]);
            Assert.Equal(this.teamNameValue, (string)team["TeamName"]);
            Assert.NotNull(team["TeamMascots"]);
            Assert.True(team["TeamMascots"] is JsonValue);

            for (int i = 0; i < this.teamMembersValues.Length; i++)
            {
                Assert.Equal(this.teamMembersValues[i], (string)team["TeamMembers"][i]);
            }

            // Could not come up with negative setter
        }

        /// <summary>
        /// Tests for the dynamic indexers in <see cref="JsonArray"/> instances.
        /// </summary>
        [Fact]
        public void JsonArrayDynamicSanity()
        {
            // Sanity test for JsonArray to ensure [] still works even if dynamic
            dynamic people = new JsonArray();
            foreach (string member in this.teamMembersValues)
            {
                people.Add(member);
            }

            Assert.Equal(this.teamMembersValues[0], (string)people[0]);
            Assert.Equal(this.teamMembersValues[1], (string)people[1]);
            Assert.Equal(this.teamMembersValues[2], (string)people[2]);
            Assert.Equal(this.teamMembersValues[3], (string)people[3]);
            Assert.Equal(this.teamMembersValues[4], (string)people[4]);

            // Note: this test and the above execute the dynamic binder differently.
            for (int i = 0; i < people.Count; i++)
            {
                Assert.Equal(this.teamMembersValues[i], (string)people[i]);
            }

            people.Add(this.teamMembersValues.Length);
            people.Add(this.teamNameValue);

            Assert.Equal(this.teamMembersValues.Length, (int)people[5]);
            Assert.Equal(this.teamNameValue, (string)people[6]);
        }

        /// <summary>
        /// Tests for calling methods in dynamic references to <see cref="JsonValue"/> instances.
        /// </summary>
        [Fact]
        public void DynamicMethodCalling()
        {
            JsonObject jo = new JsonObject();
            dynamic dyn = jo;
            dyn.Foo = "bar";
            Assert.Equal(1, jo.Count);
            Assert.Equal(1, dyn.Count);
            dyn.Remove("Foo");
            Assert.Equal(0, jo.Count);
        }

        /// <summary>
        /// Tests for using boolean operators in dynamic references to <see cref="JsonValue"/> instances.
        /// </summary>
        [Fact(Skip = "Ignore")]
        public void DynamicBooleanOperators()
        {
            JsonValue jv;
            dynamic dyn;
            foreach (bool value in new bool[] { true, false })
            {
                jv = value;
                dyn = jv;
                Log.Info("IsTrue, {0}", jv);
                if (dyn)
                {
                    Assert.True(value, "Boolean evaluation should not enter 'if' clause.");
                }
                else
                {
                    Assert.False(value, "Boolean evaluation should not enter 'else' clause.");
                }
            }

            foreach (string value in new string[] { "true", "false", "True", "False" })
            {
                bool isTrueValue = value.Equals("true", StringComparison.InvariantCultureIgnoreCase);
                jv = new JsonPrimitive(value);
                dyn = jv;
                Log.Info("IsTrue, {0}", jv);
                if (dyn)
                {
                    Assert.True(isTrueValue, "Boolean evaluation should not enter 'if' clause.");
                }
                else
                {
                    Assert.False(isTrueValue, "Boolean evaluation should not enter 'else' clause.");
                }
            }

            foreach (bool first in new bool[] { false, true })
            {
                dynamic dyn1 = new JsonPrimitive(first);
                Log.Info("Negation, {0}", first);
                Assert.Equal(!first, !dyn1);
                foreach (bool second in new bool[] { false, true })
                {
                    dynamic dyn2 = new JsonPrimitive(second);
                    Log.Info("Boolean AND, {0} && {1}", first, second);
                    Assert.Equal(first && second, (bool)(dyn1 && dyn2));
                    Log.Info("Boolean OR, {0} && {1}", first, second);
                    Assert.Equal(first || second, (bool)(dyn1 || dyn2));
                }
            }

            Log.Info("Invalid boolean operator usage");
            dynamic boolDyn = new JsonPrimitive(true);
            dynamic intDyn = new JsonPrimitive(1);
            dynamic strDyn = new JsonPrimitive("hello");

            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info("{0}", !intDyn); });

            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info("{0}", !strDyn); });
            JsonValueTests.ExpectException<InvalidCastException>(() => { Log.Info("{0}", intDyn && intDyn); });
            JsonValueTests.ExpectException<InvalidCastException>(() => { Log.Info("{0}", intDyn || true); });
            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info("{0}", boolDyn && 1); });
            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info("{0}", boolDyn && intDyn); });
            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info("{0}", boolDyn && "hello"); });
            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info("{0}", boolDyn && strDyn); });
            JsonValueTests.ExpectException<FormatException>(() => { Log.Info("{0}", strDyn && boolDyn); });
            JsonValueTests.ExpectException<FormatException>(() => { Log.Info("{0}", strDyn || true); });

            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info("{0}", !intDyn.NotHere); });
            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info("{0}", !intDyn.NotHere && true); });
            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info("{0}", !intDyn.NotHere || false); });
        }

        /// <summary>
        /// Tests for using relational operators in dynamic references to <see cref="JsonValue"/> instances.
        /// </summary>
        [Fact(Skip = "Ignore")]
        public void DynamicRelationalOperators()
        {
            JsonValue jv = new JsonObject { { "one", 1 }, { "one_point_two", 1.2 }, { "decimal_one_point_one", 1.1m }, { "trueValue", true }, { "str", "hello" } };
            dynamic dyn = jv;
            JsonValue defaultJsonValue = jv.ValueOrDefault(-1);

            Log.Info("Equality");
            Assert.True(dyn.one == 1);
            Assert.True(dyn.one_point_two == 1.2);
            Assert.False(dyn.one == 1.2);
            Assert.False(dyn.one_point_two == 1);
            Assert.False(dyn.one == 2);
            Assert.False(dyn.one_point_two == 1.3);
            Assert.True(dyn.one == 1m);
            Assert.False(dyn.one == 2m);
            Assert.True(dyn.decimal_one_point_one == 1.1m);

            Assert.True(dyn.NotHere == null);
            Assert.True(dyn.NotHere == dyn.NotHere);
            Assert.True(dyn.NotHere == defaultJsonValue);
            // DISABLED, 197375, Assert.False(dyn.NotHere == 1);
            Assert.False(dyn.NotHere == jv);

            Log.Info("Inequality");
            Assert.False(dyn.one != 1);
            Assert.False(dyn.one_point_two != 1.2);
            Assert.True(dyn.one != 1.2);
            Assert.True(dyn.one_point_two != 1);
            Assert.True(dyn.one != 2);
            Assert.True(dyn.one_point_two != 1.3);
            Assert.False(dyn.one != 1m);
            Assert.True(dyn.one != 2m);

            Assert.False(dyn.NotHere != null);
            Assert.False(dyn.NotHere != dyn.NotHere);
            Assert.False(dyn.NotHere != defaultJsonValue);
            // DISABLED, 197375, Assert.True(dyn.NotHere != 1);
            Assert.True(dyn.NotHere != jv);

            Log.Info("Less than");
            Assert.True(dyn.one < 2);
            Assert.False(dyn.one < 1);
            Assert.False(dyn.one < 0);
            Assert.True(dyn.one_point_two < 1.3);
            Assert.False(dyn.one_point_two < 1.2);
            Assert.False(dyn.one_point_two < 1.1);

            Assert.True(dyn.one < 1.1);
            Assert.Equal(1 < 1.0, dyn.one < 1.0);
            Assert.False(dyn.one < 0.9);
            Assert.True(dyn.one_point_two < 2);
            Assert.False(dyn.one_point_two < 1);
            Assert.Equal(1.2 < 1.2f, dyn.one_point_two < 1.2f);

            Log.Info("Greater than");
            Assert.False(dyn.one > 2);
            Assert.False(dyn.one > 1);
            Assert.True(dyn.one > 0);
            Assert.False(dyn.one_point_two > 1.3);
            Assert.False(dyn.one_point_two > 1.2);
            Assert.True(dyn.one_point_two > 1.1);

            Assert.False(dyn.one > 1.1);
            Assert.Equal(1 > 1.0, dyn.one > 1.0);
            Assert.True(dyn.one > 0.9);
            Assert.False(dyn.one_point_two > 2);
            Assert.True(dyn.one_point_two > 1);
            Assert.Equal(1.2 > 1.2f, dyn.one_point_two > 1.2f);

            Log.Info("Less than or equals");
            Assert.True(dyn.one <= 2);
            Assert.True(dyn.one <= 1);
            Assert.False(dyn.one <= 0);
            Assert.True(dyn.one_point_two <= 1.3);
            Assert.True(dyn.one_point_two <= 1.2);
            Assert.False(dyn.one_point_two <= 1.1);

            Assert.True(dyn.one <= 1.1);
            Assert.Equal(1 <= 1.0, dyn.one <= 1.0);
            Assert.False(dyn.one <= 0.9);
            Assert.True(dyn.one_point_two <= 2);
            Assert.False(dyn.one_point_two <= 1);
            Assert.Equal(1.2 <= 1.2f, dyn.one_point_two <= 1.2f);

            Log.Info("Greater than or equals");
            Assert.False(dyn.one >= 2);
            Assert.True(dyn.one >= 1);
            Assert.True(dyn.one >= 0);
            Assert.False(dyn.one_point_two >= 1.3);
            Assert.True(dyn.one_point_two >= 1.2);
            Assert.True(dyn.one_point_two >= 1.1);

            Assert.False(dyn.one >= 1.1);
            Assert.Equal(1 >= 1.0, dyn.one >= 1.0);
            Assert.True(dyn.one >= 0.9);
            Assert.False(dyn.one_point_two >= 2);
            Assert.True(dyn.one_point_two >= 1);
            Assert.Equal(1.2 >= 1.2f, dyn.one_point_two >= 1.2f);

            Log.Info("Invalid number conversions");
            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info(dyn.decimal_one_point_one == 1.1); });
            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info(dyn.one != (uint)2); });

            Log.Info("Invalid data types for relational operators");
            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info(dyn.trueValue >= dyn.trueValue); });
            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info(dyn.NotHere < dyn.NotHere); });
            JsonValueTests.ExpectException<InvalidOperationException>(() => { Log.Info(dyn.str < "Jello"); });

            // DISABLED, 197315
            Log.Info("Conversions from string");
            jv = new JsonObject { { "one", "1" }, { "twelve_point_two", "1.22e1" } };
            dyn = jv;
            Assert.True(dyn.one == 1);
            Assert.True(dyn.twelve_point_two == 1.22e1);
            Assert.True(dyn.one >= 0.5f);
            Assert.True(dyn.twelve_point_two <= 13);
            Assert.True(dyn.one < 2);
            Assert.Equal(dyn.twelve_point_two.ReadAs<int>() > 12, dyn.twelve_point_two > 12);
        }

        /// <summary>
        /// Tests for using arithmetic operators in dynamic references to <see cref="JsonValue"/> instances.
        /// </summary>
        [Fact(Skip = "Ignore")]
        public void ArithmeticOperators()
        {
            int seed = MethodBase.GetCurrentMethod().Name.GetHashCode();
            Log.Info("Seed: {0}", seed);
            Random rndGen = new Random(seed);
            int i1 = rndGen.Next(-10000, 10000);
            int i2 = rndGen.Next(-10000, 10000);
            JsonValue jv1 = i1;
            JsonValue jv2 = i2;
            Log.Info("jv1 = {0}, jv2 = {1}", jv1, jv2);
            dynamic dyn1 = jv1;
            dynamic dyn2 = jv2;

            string str1 = i1.ToString(CultureInfo.InvariantCulture);
            string str2 = i2.ToString(CultureInfo.InvariantCulture);
            JsonValue jvstr1 = str1;
            JsonValue jvstr2 = str2;

            Log.Info("Unary +");
            Assert.Equal<int>(+i1, +dyn1);
            Assert.Equal<int>(+i2, +dyn2);

            Log.Info("Unary -");
            Assert.Equal<int>(-i1, -dyn1);
            Assert.Equal<int>(-i2, -dyn2);

            Log.Info("Unary ~ (bitwise NOT)");
            Assert.Equal<int>(~i1, ~dyn1);
            Assert.Equal<int>(~i2, ~dyn2);

            Log.Info("Binary +: {0}", i1 + i2);
            Assert.Equal<int>(i1 + i2, dyn1 + dyn2);
            Assert.Equal<int>(i1 + i2, dyn2 + dyn1);
            Assert.Equal<int>(i1 + i2, dyn1 + i2);
            Assert.Equal<int>(i1 + i2, dyn2 + i1);

            // DISABLED, 197394
            // Assert.Equal<int>(i1 + i2, dyn1 + str2);
            // Assert.Equal<int>(i1 + i2, dyn1 + jvstr2);

            Log.Info("Binary -: {0}, {1}", i1 - i2, i2 - i1);
            Assert.Equal<int>(i1 - i2, dyn1 - dyn2);
            Assert.Equal<int>(i2 - i1, dyn2 - dyn1);
            Assert.Equal<int>(i1 - i2, dyn1 - i2);
            Assert.Equal<int>(i2 - i1, dyn2 - i1);

            Log.Info("Binary *: {0}", i1 * i2);
            Assert.Equal<int>(i1 * i2, dyn1 * dyn2);
            Assert.Equal<int>(i1 * i2, dyn2 * dyn1);
            Assert.Equal<int>(i1 * i2, dyn1 * i2);
            Assert.Equal<int>(i1 * i2, dyn2 * i1);

            while (i1 == 0)
            {
                i1 = rndGen.Next(-10000, 10000);
                jv1 = i1;
                dyn1 = jv1;
                Log.Info("Using new (non-zero) i1 value: {0}", i1);
            }

            while (i2 == 0)
            {
                i2 = rndGen.Next(-10000, 10000);
                jv2 = i2;
                dyn2 = jv2;
                Log.Info("Using new (non-zero) i2 value: {0}", i2);
            }

            Log.Info("Binary / (integer division): {0}, {1}", i1 / i2, i2 / i1);
            Assert.Equal<int>(i1 / i2, dyn1 / dyn2);
            Assert.Equal<int>(i2 / i1, dyn2 / dyn1);
            Assert.Equal<int>(i1 / i2, dyn1 / i2);
            Assert.Equal<int>(i2 / i1, dyn2 / i1);

            Log.Info("Binary % (modulo): {0}, {1}", i1 % i2, i2 % i1);
            Assert.Equal<int>(i1 % i2, dyn1 % dyn2);
            Assert.Equal<int>(i2 % i1, dyn2 % dyn1);
            Assert.Equal<int>(i1 % i2, dyn1 % i2);
            Assert.Equal<int>(i2 % i1, dyn2 % i1);

            Log.Info("Binary & (bitwise AND): {0}", i1 & i2);
            Assert.Equal<int>(i1 & i2, dyn1 & dyn2);
            Assert.Equal<int>(i1 & i2, dyn2 & dyn1);
            Assert.Equal<int>(i1 & i2, dyn1 & i2);
            Assert.Equal<int>(i1 & i2, dyn2 & i1);

            Log.Info("Binary | (bitwise OR): {0}", i1 | i2);
            Assert.Equal<int>(i1 | i2, dyn1 | dyn2);
            Assert.Equal<int>(i1 | i2, dyn2 | dyn1);
            Assert.Equal<int>(i1 | i2, dyn1 | i2);
            Assert.Equal<int>(i1 | i2, dyn2 | i1);

            Log.Info("Binary ^ (bitwise XOR): {0}", i1 ^ i2);
            Assert.Equal<int>(i1 ^ i2, dyn1 ^ dyn2);
            Assert.Equal<int>(i1 ^ i2, dyn2 ^ dyn1);
            Assert.Equal<int>(i1 ^ i2, dyn1 ^ i2);
            Assert.Equal<int>(i1 ^ i2, dyn2 ^ i1);

            i1 = rndGen.Next(1, 10);
            i2 = rndGen.Next(1, 10);
            jv1 = i1;
            jv2 = i2;
            dyn1 = jv1;
            dyn2 = jv2;
            Log.Info("New i1, i2: {0}, {1}", i1, i2);

            Log.Info("Left shift: {0}", i1 << i2);
            Assert.Equal<int>(i1 << i2, dyn1 << dyn2);
            Assert.Equal<int>(i1 << i2, dyn1 << i2);

            i1 = i1 << i2;
            jv1 = i1;
            dyn1 = jv1;
            Log.Info("New i1: {0}", i1);
            Log.Info("Right shift: {0}", i1 >> i2);
            Assert.Equal<int>(i1 >> i2, dyn1 >> dyn2);
            Assert.Equal<int>(i1 >> i2, dyn1 >> i2);

            i2 += 4;
            jv2 = i2;
            dyn2 = jv2;
            Log.Info("New i2: {0}", i2);
            Log.Info("Right shift: {0}", i1 >> i2);
            Assert.Equal<int>(i1 >> i2, dyn1 >> dyn2);
            Assert.Equal<int>(i1 >> i2, dyn1 >> i2);
        }

        /// <summary>
        /// Tests for conversions between data types in arithmetic operations.
        /// </summary>
        [Fact(Skip = "Ignore")]
        public void ArithmeticConversion()
        {
            JsonObject jo = new JsonObject
            {
                { "byteVal", (byte)10 },
                { "sbyteVal", (sbyte)10 },
                { "shortVal", (short)10 },
                { "ushortVal", (ushort)10 },
                { "intVal", 10 },
                { "uintVal", (uint)10 },
                { "longVal", 10L },
                { "ulongVal", (ulong)10 },
                { "charVal", (char)10 },
                { "decimalVal", 10m },
                { "doubleVal", 10.0 },
                { "floatVal", 10f },
            };
            dynamic dyn = jo;

            Log.Info("Conversion from byte");
            // DISABLED, 197387, ValidateResult<int>(dyn.byteVal + (byte)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.byteVal + (sbyte)10));
            ValidateResult<short>(dyn.byteVal + (short)10, 20);
            ValidateResult<ushort>(dyn.byteVal + (ushort)10, 20);
            ValidateResult<int>(dyn.byteVal + (int)10, 20);
            ValidateResult<uint>(dyn.byteVal + (uint)10, 20);
            ValidateResult<long>(dyn.byteVal + 10L, 20);
            ValidateResult<ulong>(dyn.byteVal + (ulong)10, 20);
            ValidateResult<decimal>(dyn.byteVal + 10m, 20);
            ValidateResult<float>(dyn.byteVal + 10f, 20);
            ValidateResult<double>(dyn.byteVal + 10.0, 20);

            Log.Info("Conversion from sbyte");
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.sbyteVal + (byte)10));
            // DISABLED, 197387, ValidateResult<int>(dyn.sbyteVal + (sbyte)10, 20);
            ValidateResult<short>(dyn.sbyteVal + (short)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.sbyteVal + (ushort)10));
            ValidateResult<int>(dyn.sbyteVal + (int)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.sbyteVal + (uint)10));
            ValidateResult<long>(dyn.sbyteVal + 10L, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.sbyteVal + (ulong)10));
            ValidateResult<decimal>(dyn.sbyteVal + 10m, 20);
            ValidateResult<float>(dyn.sbyteVal + 10f, 20);
            ValidateResult<double>(dyn.sbyteVal + 10.0, 20);

            Log.Info("Conversion from short");
            ValidateResult<short>(dyn.shortVal + (byte)10, 20);
            ValidateResult<short>(dyn.shortVal + (sbyte)10, 20);
            ValidateResult<short>(dyn.shortVal + (short)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.shortVal + (ushort)10));
            ValidateResult<int>(dyn.shortVal + (int)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.shortVal + (uint)10));
            ValidateResult<long>(dyn.shortVal + 10L, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.shortVal + (ulong)10));
            ValidateResult<decimal>(dyn.shortVal + 10m, 20);
            ValidateResult<float>(dyn.shortVal + 10f, 20);
            ValidateResult<double>(dyn.shortVal + 10.0, 20);

            Log.Info("Conversion from ushort");
            ValidateResult<ushort>(dyn.ushortVal + (byte)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.ushortVal + (sbyte)10));
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.ushortVal + (short)10));
            ValidateResult<ushort>(dyn.ushortVal + (ushort)10, 20);
            ValidateResult<int>(dyn.ushortVal + (int)10, 20);
            ValidateResult<uint>(dyn.ushortVal + (uint)10, 20);
            ValidateResult<long>(dyn.ushortVal + 10L, 20);
            ValidateResult<ulong>(dyn.ushortVal + (ulong)10, 20);
            ValidateResult<decimal>(dyn.ushortVal + 10m, 20);
            ValidateResult<float>(dyn.ushortVal + 10f, 20);
            ValidateResult<double>(dyn.ushortVal + 10.0, 20);

            Log.Info("Conversion from int");
            ValidateResult<int>(dyn.intVal + (byte)10, 20);
            ValidateResult<int>(dyn.intVal + (sbyte)10, 20);
            ValidateResult<int>(dyn.intVal + (short)10, 20);
            ValidateResult<int>(dyn.intVal + (ushort)10, 20);
            ValidateResult<int>(dyn.intVal + (int)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.intVal + (uint)10));
            ValidateResult<long>(dyn.intVal + 10L, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.intVal + (ulong)10));
            ValidateResult<decimal>(dyn.intVal + 10m, 20);
            ValidateResult<float>(dyn.intVal + 10f, 20);
            ValidateResult<double>(dyn.intVal + 10.0, 20);

            Log.Info("Conversion from uint");
            ValidateResult<uint>(dyn.uintVal + (byte)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.uintVal + (sbyte)10));
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.uintVal + (short)10));
            ValidateResult<uint>(dyn.uintVal + (ushort)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.uintVal + (int)10));
            ValidateResult<uint>(dyn.uintVal + (uint)10, 20);
            ValidateResult<long>(dyn.uintVal + 10L, 20);
            ValidateResult<ulong>(dyn.uintVal + (ulong)10, 20);
            ValidateResult<decimal>(dyn.uintVal + 10m, 20);
            ValidateResult<float>(dyn.uintVal + 10f, 20);
            ValidateResult<double>(dyn.uintVal + 10.0, 20);

            Log.Info("Conversion from long");
            ValidateResult<long>(dyn.longVal + (byte)10, 20);
            ValidateResult<long>(dyn.longVal + (sbyte)10, 20);
            ValidateResult<long>(dyn.longVal + (short)10, 20);
            ValidateResult<long>(dyn.longVal + (ushort)10, 20);
            ValidateResult<long>(dyn.longVal + (int)10, 20);
            ValidateResult<long>(dyn.longVal + (uint)10, 20);
            ValidateResult<long>(dyn.longVal + 10L, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.longVal + (ulong)10));
            ValidateResult<decimal>(dyn.longVal + 10m, 20);
            ValidateResult<float>(dyn.longVal + 10f, 20);
            ValidateResult<double>(dyn.longVal + 10.0, 20);

            Log.Info("Conversion from ulong");
            ValidateResult<ulong>(dyn.ulongVal + (byte)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.ulongVal + (sbyte)10));
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.ulongVal + (short)10));
            ValidateResult<ulong>(dyn.ulongVal + (ushort)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.ulongVal + (int)10));
            ValidateResult<ulong>(dyn.ulongVal + (uint)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.ulongVal + (long)10));
            ValidateResult<ulong>(dyn.ulongVal + (ulong)10, 20);
            ValidateResult<decimal>(dyn.ulongVal + 10m, 20);
            ValidateResult<float>(dyn.ulongVal + 10f, 20);
            ValidateResult<double>(dyn.ulongVal + 10.0, 20);

            Log.Info("Conversion from float");
            ValidateResult<float>(dyn.floatVal + (byte)10, 20);
            ValidateResult<float>(dyn.floatVal + (sbyte)10, 20);
            ValidateResult<float>(dyn.floatVal + (short)10, 20);
            ValidateResult<float>(dyn.floatVal + (ushort)10, 20);
            ValidateResult<float>(dyn.floatVal + (int)10, 20);
            ValidateResult<float>(dyn.floatVal + (uint)10, 20);
            ValidateResult<float>(dyn.floatVal + 10L, 20);
            ValidateResult<float>(dyn.floatVal + (ulong)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.floatVal + 10m));
            ValidateResult<float>(dyn.floatVal + 10f, 20);
            ValidateResult<double>(dyn.floatVal + 10.0, 20);

            Log.Info("Conversion from double");
            ValidateResult<double>(dyn.doubleVal + (byte)10, 20);
            ValidateResult<double>(dyn.doubleVal + (sbyte)10, 20);
            ValidateResult<double>(dyn.doubleVal + (short)10, 20);
            ValidateResult<double>(dyn.doubleVal + (ushort)10, 20);
            ValidateResult<double>(dyn.doubleVal + (int)10, 20);
            ValidateResult<double>(dyn.doubleVal + (uint)10, 20);
            ValidateResult<double>(dyn.doubleVal + 10L, 20);
            ValidateResult<double>(dyn.doubleVal + (ulong)10, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.doubleVal + 10m));
            ValidateResult<double>(dyn.doubleVal + 10f, 20);
            ValidateResult<double>(dyn.doubleVal + 10.0, 20);

            Log.Info("Conversion from decimal");
            ValidateResult<decimal>(dyn.decimalVal + (byte)10, 20);
            ValidateResult<decimal>(dyn.decimalVal + (sbyte)10, 20);
            ValidateResult<decimal>(dyn.decimalVal + (short)10, 20);
            ValidateResult<decimal>(dyn.decimalVal + (ushort)10, 20);
            ValidateResult<decimal>(dyn.decimalVal + (int)10, 20);
            ValidateResult<decimal>(dyn.decimalVal + (uint)10, 20);
            ValidateResult<decimal>(dyn.decimalVal + 10L, 20);
            ValidateResult<decimal>(dyn.decimalVal + (ulong)10, 20);
            ValidateResult<decimal>(dyn.decimalVal + 10m, 20);
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.decimalVal + 10f));
            JsonValueTests.ExpectException<InvalidOperationException>(() => Log.Info("{0}", dyn.decimalVal + 10.0));
        }

        /// <summary>
        /// Tests for implicit casts between dynamic references to <see cref="JsonPrimitive"/> instances
        /// and the supported CLR types.
        /// </summary>
        [Fact]
        public void ImplicitPrimitiveCastTests()
        {
            DateTime now = DateTime.Now;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Log.Info("Seed: {0}", seed);
            Random rndGen = new Random(seed);
            int intValue = rndGen.Next(1, 127);
            Log.Info("Value: {0}", intValue);

            uint uintValue = (uint)intValue;
            short shortValue = (short)intValue;
            ushort ushortValue = (ushort)intValue;
            long longValue = (long)intValue;
            ulong ulongValue = (ulong)intValue;
            byte byteValue = (byte)intValue;
            sbyte sbyteValue = (sbyte)intValue;
            float floatValue = (float)intValue;
            double doubleValue = (double)intValue;
            decimal decimalValue = (decimal)intValue;
            string stringValue = intValue.ToString(CultureInfo.InvariantCulture);

            dynamic dyn = new JsonObject
            {
                { "Byte", byteValue },
                { "SByte", sbyteValue },
                { "Int16", shortValue },
                { "UInt16", ushortValue },
                { "Int32", intValue },
                { "UInt32", uintValue },
                { "Int64", longValue },
                { "UInt64", ulongValue },
                { "Double", doubleValue },
                { "Single", floatValue },
                { "Decimal", decimalValue },
                { "String", stringValue },
                { "True", "true" },
                { "False", "false" },
            };

            Log.Info("dyn: {0}", dyn);

            Log.Info("Casts to Byte");

            byte byteFromByte = dyn.Byte;
            byte byteFromSByte = dyn.SByte;
            byte byteFromShort = dyn.Int16;
            byte byteFromUShort = dyn.UInt16;
            byte byteFromInt = dyn.Int32;
            byte byteFromUInt = dyn.UInt32;
            byte byteFromLong = dyn.Int64;
            byte byteFromULong = dyn.UInt64;
            byte byteFromDouble = dyn.Double;
            byte byteFromFloat = dyn.Single;
            byte byteFromDecimal = dyn.Decimal;
            byte byteFromString = dyn.String;

            Assert.Equal<byte>(byteValue, byteFromByte);
            Assert.Equal<byte>(byteValue, byteFromSByte);
            Assert.Equal<byte>(byteValue, byteFromShort);
            Assert.Equal<byte>(byteValue, byteFromUShort);
            Assert.Equal<byte>(byteValue, byteFromInt);
            Assert.Equal<byte>(byteValue, byteFromUInt);
            Assert.Equal<byte>(byteValue, byteFromLong);
            Assert.Equal<byte>(byteValue, byteFromULong);
            Assert.Equal<byte>(byteValue, byteFromDouble);
            Assert.Equal<byte>(byteValue, byteFromFloat);
            Assert.Equal<byte>(byteValue, byteFromDecimal);
            Assert.Equal<byte>(byteValue, byteFromString);

            Log.Info("Casts to SByte");

            sbyte sbyteFromByte = dyn.Byte;
            sbyte sbyteFromSByte = dyn.SByte;
            sbyte sbyteFromShort = dyn.Int16;
            sbyte sbyteFromUShort = dyn.UInt16;
            sbyte sbyteFromInt = dyn.Int32;
            sbyte sbyteFromUInt = dyn.UInt32;
            sbyte sbyteFromLong = dyn.Int64;
            sbyte sbyteFromULong = dyn.UInt64;
            sbyte sbyteFromDouble = dyn.Double;
            sbyte sbyteFromFloat = dyn.Single;
            sbyte sbyteFromDecimal = dyn.Decimal;
            sbyte sbyteFromString = dyn.String;

            Assert.Equal<sbyte>(sbyteValue, sbyteFromByte);
            Assert.Equal<sbyte>(sbyteValue, sbyteFromSByte);
            Assert.Equal<sbyte>(sbyteValue, sbyteFromShort);
            Assert.Equal<sbyte>(sbyteValue, sbyteFromUShort);
            Assert.Equal<sbyte>(sbyteValue, sbyteFromInt);
            Assert.Equal<sbyte>(sbyteValue, sbyteFromUInt);
            Assert.Equal<sbyte>(sbyteValue, sbyteFromLong);
            Assert.Equal<sbyte>(sbyteValue, sbyteFromULong);
            Assert.Equal<sbyte>(sbyteValue, sbyteFromDouble);
            Assert.Equal<sbyte>(sbyteValue, sbyteFromFloat);
            Assert.Equal<sbyte>(sbyteValue, sbyteFromDecimal);
            Assert.Equal<sbyte>(sbyteValue, sbyteFromString);

            Log.Info("Casts to Short");

            short shortFromByte = dyn.Byte;
            short shortFromSByte = dyn.SByte;
            short shortFromShort = dyn.Int16;
            short shortFromUShort = dyn.UInt16;
            short shortFromInt = dyn.Int32;
            short shortFromUInt = dyn.UInt32;
            short shortFromLong = dyn.Int64;
            short shortFromULong = dyn.UInt64;
            short shortFromDouble = dyn.Double;
            short shortFromFloat = dyn.Single;
            short shortFromDecimal = dyn.Decimal;
            short shortFromString = dyn.String;

            Assert.Equal<short>(shortValue, shortFromByte);
            Assert.Equal<short>(shortValue, shortFromSByte);
            Assert.Equal<short>(shortValue, shortFromShort);
            Assert.Equal<short>(shortValue, shortFromUShort);
            Assert.Equal<short>(shortValue, shortFromInt);
            Assert.Equal<short>(shortValue, shortFromUInt);
            Assert.Equal<short>(shortValue, shortFromLong);
            Assert.Equal<short>(shortValue, shortFromULong);
            Assert.Equal<short>(shortValue, shortFromDouble);
            Assert.Equal<short>(shortValue, shortFromFloat);
            Assert.Equal<short>(shortValue, shortFromDecimal);
            Assert.Equal<short>(shortValue, shortFromString);

            Log.Info("Casts to UShort");

            ushort ushortFromByte = dyn.Byte;
            ushort ushortFromSByte = dyn.SByte;
            ushort ushortFromShort = dyn.Int16;
            ushort ushortFromUShort = dyn.UInt16;
            ushort ushortFromInt = dyn.Int32;
            ushort ushortFromUInt = dyn.UInt32;
            ushort ushortFromLong = dyn.Int64;
            ushort ushortFromULong = dyn.UInt64;
            ushort ushortFromDouble = dyn.Double;
            ushort ushortFromFloat = dyn.Single;
            ushort ushortFromDecimal = dyn.Decimal;
            ushort ushortFromString = dyn.String;

            Assert.Equal<ushort>(ushortValue, ushortFromByte);
            Assert.Equal<ushort>(ushortValue, ushortFromSByte);
            Assert.Equal<ushort>(ushortValue, ushortFromShort);
            Assert.Equal<ushort>(ushortValue, ushortFromUShort);
            Assert.Equal<ushort>(ushortValue, ushortFromInt);
            Assert.Equal<ushort>(ushortValue, ushortFromUInt);
            Assert.Equal<ushort>(ushortValue, ushortFromLong);
            Assert.Equal<ushort>(ushortValue, ushortFromULong);
            Assert.Equal<ushort>(ushortValue, ushortFromDouble);
            Assert.Equal<ushort>(ushortValue, ushortFromFloat);
            Assert.Equal<ushort>(ushortValue, ushortFromDecimal);
            Assert.Equal<ushort>(ushortValue, ushortFromString);

            Log.Info("Casts to Int");

            int intFromByte = dyn.Byte;
            int intFromSByte = dyn.SByte;
            int intFromShort = dyn.Int16;
            int intFromUShort = dyn.UInt16;
            int intFromInt = dyn.Int32;
            int intFromUInt = dyn.UInt32;
            int intFromLong = dyn.Int64;
            int intFromULong = dyn.UInt64;
            int intFromDouble = dyn.Double;
            int intFromFloat = dyn.Single;
            int intFromDecimal = dyn.Decimal;
            int intFromString = dyn.String;

            Assert.Equal<int>(intValue, intFromByte);
            Assert.Equal<int>(intValue, intFromSByte);
            Assert.Equal<int>(intValue, intFromShort);
            Assert.Equal<int>(intValue, intFromUShort);
            Assert.Equal<int>(intValue, intFromInt);
            Assert.Equal<int>(intValue, intFromUInt);
            Assert.Equal<int>(intValue, intFromLong);
            Assert.Equal<int>(intValue, intFromULong);
            Assert.Equal<int>(intValue, intFromDouble);
            Assert.Equal<int>(intValue, intFromFloat);
            Assert.Equal<int>(intValue, intFromDecimal);
            Assert.Equal<int>(intValue, intFromString);

            Log.Info("Casts to UInt");

            uint uintFromByte = dyn.Byte;
            uint uintFromSByte = dyn.SByte;
            uint uintFromShort = dyn.Int16;
            uint uintFromUShort = dyn.UInt16;
            uint uintFromInt = dyn.Int32;
            uint uintFromUInt = dyn.UInt32;
            uint uintFromLong = dyn.Int64;
            uint uintFromULong = dyn.UInt64;
            uint uintFromDouble = dyn.Double;
            uint uintFromFloat = dyn.Single;
            uint uintFromDecimal = dyn.Decimal;
            uint uintFromString = dyn.String;

            Assert.Equal<uint>(uintValue, uintFromByte);
            Assert.Equal<uint>(uintValue, uintFromSByte);
            Assert.Equal<uint>(uintValue, uintFromShort);
            Assert.Equal<uint>(uintValue, uintFromUShort);
            Assert.Equal<uint>(uintValue, uintFromInt);
            Assert.Equal<uint>(uintValue, uintFromUInt);
            Assert.Equal<uint>(uintValue, uintFromLong);
            Assert.Equal<uint>(uintValue, uintFromULong);
            Assert.Equal<uint>(uintValue, uintFromDouble);
            Assert.Equal<uint>(uintValue, uintFromFloat);
            Assert.Equal<uint>(uintValue, uintFromDecimal);
            Assert.Equal<uint>(uintValue, uintFromString);

            Log.Info("Casts to Long");

            long longFromByte = dyn.Byte;
            long longFromSByte = dyn.SByte;
            long longFromShort = dyn.Int16;
            long longFromUShort = dyn.UInt16;
            long longFromInt = dyn.Int32;
            long longFromUInt = dyn.UInt32;
            long longFromLong = dyn.Int64;
            long longFromULong = dyn.UInt64;
            long longFromDouble = dyn.Double;
            long longFromFloat = dyn.Single;
            long longFromDecimal = dyn.Decimal;
            long longFromString = dyn.String;

            Assert.Equal<long>(longValue, longFromByte);
            Assert.Equal<long>(longValue, longFromSByte);
            Assert.Equal<long>(longValue, longFromShort);
            Assert.Equal<long>(longValue, longFromUShort);
            Assert.Equal<long>(longValue, longFromInt);
            Assert.Equal<long>(longValue, longFromUInt);
            Assert.Equal<long>(longValue, longFromLong);
            Assert.Equal<long>(longValue, longFromULong);
            Assert.Equal<long>(longValue, longFromDouble);
            Assert.Equal<long>(longValue, longFromFloat);
            Assert.Equal<long>(longValue, longFromDecimal);
            Assert.Equal<long>(longValue, longFromString);

            Log.Info("Casts to ULong");

            ulong ulongFromByte = dyn.Byte;
            ulong ulongFromSByte = dyn.SByte;
            ulong ulongFromShort = dyn.Int16;
            ulong ulongFromUShort = dyn.UInt16;
            ulong ulongFromInt = dyn.Int32;
            ulong ulongFromUInt = dyn.UInt32;
            ulong ulongFromLong = dyn.Int64;
            ulong ulongFromULong = dyn.UInt64;
            ulong ulongFromDouble = dyn.Double;
            ulong ulongFromFloat = dyn.Single;
            ulong ulongFromDecimal = dyn.Decimal;
            ulong ulongFromString = dyn.String;

            Assert.Equal<ulong>(ulongValue, ulongFromByte);
            Assert.Equal<ulong>(ulongValue, ulongFromSByte);
            Assert.Equal<ulong>(ulongValue, ulongFromShort);
            Assert.Equal<ulong>(ulongValue, ulongFromUShort);
            Assert.Equal<ulong>(ulongValue, ulongFromInt);
            Assert.Equal<ulong>(ulongValue, ulongFromUInt);
            Assert.Equal<ulong>(ulongValue, ulongFromLong);
            Assert.Equal<ulong>(ulongValue, ulongFromULong);
            Assert.Equal<ulong>(ulongValue, ulongFromDouble);
            Assert.Equal<ulong>(ulongValue, ulongFromFloat);
            Assert.Equal<ulong>(ulongValue, ulongFromDecimal);
            Assert.Equal<ulong>(ulongValue, ulongFromString);

            Log.Info("Casts to Float");

            float floatFromByte = dyn.Byte;
            float floatFromSByte = dyn.SByte;
            float floatFromShort = dyn.Int16;
            float floatFromUShort = dyn.UInt16;
            float floatFromInt = dyn.Int32;
            float floatFromUInt = dyn.UInt32;
            float floatFromLong = dyn.Int64;
            float floatFromULong = dyn.UInt64;
            float floatFromDouble = dyn.Double;
            float floatFromFloat = dyn.Single;
            float floatFromDecimal = dyn.Decimal;
            float floatFromString = dyn.String;

            Assert.Equal<float>(floatValue, floatFromByte);
            Assert.Equal<float>(floatValue, floatFromSByte);
            Assert.Equal<float>(floatValue, floatFromShort);
            Assert.Equal<float>(floatValue, floatFromUShort);
            Assert.Equal<float>(floatValue, floatFromInt);
            Assert.Equal<float>(floatValue, floatFromUInt);
            Assert.Equal<float>(floatValue, floatFromLong);
            Assert.Equal<float>(floatValue, floatFromULong);
            Assert.Equal<float>(floatValue, floatFromDouble);
            Assert.Equal<float>(floatValue, floatFromFloat);
            Assert.Equal<float>(floatValue, floatFromDecimal);
            Assert.Equal<float>(floatValue, floatFromString);

            Log.Info("Casts to Double");

            double doubleFromByte = dyn.Byte;
            double doubleFromSByte = dyn.SByte;
            double doubleFromShort = dyn.Int16;
            double doubleFromUShort = dyn.UInt16;
            double doubleFromInt = dyn.Int32;
            double doubleFromUInt = dyn.UInt32;
            double doubleFromLong = dyn.Int64;
            double doubleFromULong = dyn.UInt64;
            double doubleFromDouble = dyn.Double;
            double doubleFromFloat = dyn.Single;
            double doubleFromDecimal = dyn.Decimal;
            double doubleFromString = dyn.String;

            Assert.Equal<double>(doubleValue, doubleFromByte);
            Assert.Equal<double>(doubleValue, doubleFromSByte);
            Assert.Equal<double>(doubleValue, doubleFromShort);
            Assert.Equal<double>(doubleValue, doubleFromUShort);
            Assert.Equal<double>(doubleValue, doubleFromInt);
            Assert.Equal<double>(doubleValue, doubleFromUInt);
            Assert.Equal<double>(doubleValue, doubleFromLong);
            Assert.Equal<double>(doubleValue, doubleFromULong);
            Assert.Equal<double>(doubleValue, doubleFromDouble);
            Assert.Equal<double>(doubleValue, doubleFromFloat);
            Assert.Equal<double>(doubleValue, doubleFromDecimal);
            Assert.Equal<double>(doubleValue, doubleFromString);

            Log.Info("Casts to Decimal");

            decimal decimalFromByte = dyn.Byte;
            decimal decimalFromSByte = dyn.SByte;
            decimal decimalFromShort = dyn.Int16;
            decimal decimalFromUShort = dyn.UInt16;
            decimal decimalFromInt = dyn.Int32;
            decimal decimalFromUInt = dyn.UInt32;
            decimal decimalFromLong = dyn.Int64;
            decimal decimalFromULong = dyn.UInt64;
            decimal decimalFromDouble = dyn.Double;
            decimal decimalFromFloat = dyn.Single;
            decimal decimalFromDecimal = dyn.Decimal;
            decimal decimalFromString = dyn.String;

            Assert.Equal<decimal>(decimalValue, decimalFromByte);
            Assert.Equal<decimal>(decimalValue, decimalFromSByte);
            Assert.Equal<decimal>(decimalValue, decimalFromShort);
            Assert.Equal<decimal>(decimalValue, decimalFromUShort);
            Assert.Equal<decimal>(decimalValue, decimalFromInt);
            Assert.Equal<decimal>(decimalValue, decimalFromUInt);
            Assert.Equal<decimal>(decimalValue, decimalFromLong);
            Assert.Equal<decimal>(decimalValue, decimalFromULong);
            Assert.Equal<decimal>(decimalValue, decimalFromDouble);
            Assert.Equal<decimal>(decimalValue, decimalFromFloat);
            Assert.Equal<decimal>(decimalValue, decimalFromDecimal);
            Assert.Equal<decimal>(decimalValue, decimalFromString);

            Log.Info("Casts to String");

            string stringFromByte = dyn.Byte;
            string stringFromSByte = dyn.SByte;
            string stringFromShort = dyn.Int16;
            string stringFromUShort = dyn.UInt16;
            string stringFromInt = dyn.Int32;
            string stringFromUInt = dyn.UInt32;
            string stringFromLong = dyn.Int64;
            string stringFromULong = dyn.UInt64;
            string stringFromDouble = dyn.Double;
            string stringFromFloat = dyn.Single;
            string stringFromDecimal = dyn.Decimal;
            string stringFromString = dyn.String;

            Assert.Equal(stringValue, stringFromByte);
            Assert.Equal(stringValue, stringFromSByte);
            Assert.Equal(stringValue, stringFromShort);
            Assert.Equal(stringValue, stringFromUShort);
            Assert.Equal(stringValue, stringFromInt);
            Assert.Equal(stringValue, stringFromUInt);
            Assert.Equal(stringValue, stringFromLong);
            Assert.Equal(stringValue, stringFromULong);
            Assert.Equal(stringValue, stringFromDouble);
            Assert.Equal(stringValue, stringFromFloat);
            Assert.Equal(stringValue, stringFromDecimal);
            Assert.Equal(stringValue, stringFromString);

            Log.Info("Casts to Boolean");

            bool bTrue = dyn.True;
            bool bFalse = dyn.False;
            Assert.True(bTrue);
            Assert.False(bFalse);
        }

        /// <summary>
        /// Test for creating a JsonValue from a deep-nested dynamic object.
        /// </summary>
        [Fact]
        public void CreateFromDeepNestedDynamic()
        {
            int count = 5000;
            string expected = "";

            dynamic dyn = new TestDynamicObject();
            dynamic cur = dyn;

            for (int i = 0; i < count; i++)
            {
                expected += "{\"" + i + "\":";
                cur[i.ToString()] = new TestDynamicObject();
                cur = cur[i.ToString()];
            }

            expected += "{}";

            for (int i = 0; i < count; i++)
            {
                expected += "}";
            }

            JsonValue jv = JsonValueExtensions.CreateFrom(dyn);
            Assert.Equal(expected, jv.ToString());
        }

        private void ValidateResult<ResultType>(dynamic value, ResultType expectedResult)
        {
            Assert.IsAssignableFrom(typeof(ResultType), value);
            Assert.Equal<ResultType>(expectedResult, (ResultType)value);
        }

        /// <summary>
        /// Concrete DynamicObject class for testing purposes.
        /// </summary>
        internal class TestDynamicObject : DynamicObject
        {
            private IDictionary<string, object> _values = new Dictionary<string, object>();

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
                return _values.TryGetValue(binder.Name, out result);
            }

            public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
            {
                string key = indexes[0].ToString();

                if (_values.ContainsKey(key))
                {
                    _values[key] = value;
                }
                else
                {
                    _values.Add(key, value);
                }
                return true;
            }

            public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            {
                string key = indexes[0].ToString();

                if (_values.ContainsKey(key))
                {
                    result = _values[key];
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
        }
    }
}
