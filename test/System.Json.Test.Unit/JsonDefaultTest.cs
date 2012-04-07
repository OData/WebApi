// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Runtime.Serialization.Json;
using Xunit;

namespace System.Json
{
    public class JsonDefaultTest
    {
        const string IndexerNotSupportedMsgFormat = "'{0}' type indexer is not supported on JsonValue of 'JsonType.Default' type.";
        const string OperationNotAllowedOnDefaultMsgFormat = "Operation not supported on JsonValue instances of 'JsonType.Default' type.";

        [Fact]
        public void PropertiesTest()
        {
            JsonValue target = AnyInstance.DefaultJsonValue;

            Assert.Equal(JsonType.Default, target.JsonType);
            Assert.Equal(0, target.Count);
            Assert.Equal(false, target.ContainsKey("hello"));
            Assert.Equal(false, target.ContainsKey(String.Empty));
        }

        [Fact]
        public void SaveTest()
        {
            JsonValue target = AnyInstance.DefaultJsonValue;
            using (MemoryStream ms = new MemoryStream())
            {
                ExceptionHelper.Throws<InvalidOperationException>(() => target.Save(ms), "Operation not supported on JsonValue instances of 'JsonType.Default' type.");
            }
        }

        [Fact]
        public void ToStringTest()
        {
            JsonValue target;

            target = AnyInstance.DefaultJsonValue;
            Assert.Equal(target.ToString(), "Default");
        }

        [Fact(Skip = "See bug #228569 in CSDMain")]
        public void ReadAsTests()
        {
            JsonValue target = AnyInstance.DefaultJsonValue;
            string typeName = target.GetType().FullName;

            string errorMsgFormat = "Cannot read '{0}' as '{1}' type.";

            ExceptionHelper.Throws<NotSupportedException>(delegate { target.ReadAs(typeof(bool)); }, String.Format(errorMsgFormat, typeName, typeof(bool)));
            ExceptionHelper.Throws<NotSupportedException>(delegate { target.ReadAs(typeof(string)); }, String.Format(errorMsgFormat, typeName, typeof(string)));
            ExceptionHelper.Throws<NotSupportedException>(delegate { target.ReadAs(typeof(JsonObject)); }, String.Format(errorMsgFormat, typeName, typeof(JsonObject)));

            ExceptionHelper.Throws<NotSupportedException>(delegate { target.ReadAs<bool>(); }, String.Format(errorMsgFormat, typeName, typeof(bool)));
            ExceptionHelper.Throws<NotSupportedException>(delegate { target.ReadAs<string>(); }, String.Format(errorMsgFormat, typeName, typeof(string)));
            ExceptionHelper.Throws<NotSupportedException>(delegate { target.ReadAs<JsonObject>(); }, String.Format(errorMsgFormat, typeName, typeof(JsonObject)));

            bool boolValue;
            string stringValue;
            JsonObject objValue;

            object value;

            Assert.False(target.TryReadAs(typeof(bool), out value), "TryReadAs expected to return false");
            Assert.Null(value);

            Assert.False(target.TryReadAs(typeof(string), out value), "TryReadAs expected to return false");
            Assert.Null(value);

            Assert.False(target.TryReadAs(typeof(JsonObject), out value), "TryReadAs expected to return false");
            Assert.Null(value);

            Assert.False(target.TryReadAs<bool>(out boolValue), "TryReadAs expected to return false");
            Assert.False(boolValue);

            Assert.False(target.TryReadAs<string>(out stringValue), "TryReadAs expected to return false");
            Assert.Null(stringValue);

            Assert.False(target.TryReadAs<JsonObject>(out objValue), "TryReadAs expected to return false");
            Assert.Null(objValue);
        }

        [Fact]
        public void ItemTests()
        {
            JsonValue target = AnyInstance.DefaultJsonValue;

            ExceptionHelper.Throws<InvalidOperationException>(delegate { var v = target["MissingProperty"]; }, String.Format(IndexerNotSupportedMsgFormat, typeof(string).FullName));
            ExceptionHelper.Throws<InvalidOperationException>(delegate { target["NewProperty"] = AnyInstance.AnyJsonValue1; }, String.Format(IndexerNotSupportedMsgFormat, typeof(string).FullName));
        }

        [Fact]
        public void DynamicItemTests()
        {
            dynamic target = AnyInstance.DefaultJsonValue;

            var getByKey = target["SomeKey"];
            Assert.Same(getByKey, AnyInstance.DefaultJsonValue);

            var getByIndex = target[10];
            Assert.Same(getByIndex, AnyInstance.DefaultJsonValue);

            ExceptionHelper.Throws<InvalidOperationException>(delegate { target["SomeKey"] = AnyInstance.AnyJsonObject; }, String.Format(IndexerNotSupportedMsgFormat, typeof(string).FullName));
            ExceptionHelper.Throws<InvalidOperationException>(delegate { target[10] = AnyInstance.AnyJsonObject; }, String.Format(IndexerNotSupportedMsgFormat, typeof(int).FullName));
        }

        [Fact(Skip = "See bug #228569 in CSDMain")]
        public void InvalidAssignmentValueTest()
        {
            JsonValue target;
            JsonValue value = AnyInstance.DefaultJsonValue;

            target = AnyInstance.AnyJsonArray;
            ExceptionHelper.Throws<ArgumentException>(delegate { target[0] = value; }, OperationNotAllowedOnDefaultMsgFormat);

            target = AnyInstance.AnyJsonObject;
            ExceptionHelper.Throws<ArgumentException>(delegate { target["key"] = value; }, OperationNotAllowedOnDefaultMsgFormat);
        }

        [Fact]
        public void DefaultConcatTest()
        {
            JsonValue jv = JsonValueExtensions.CreateFrom(AnyInstance.AnyPerson);
            dynamic target = JsonValueExtensions.CreateFrom(AnyInstance.AnyPerson);
            Person person = AnyInstance.AnyPerson;

            Assert.Equal(JsonType.Default, target.Friends[100000].Name.JsonType);
            Assert.Equal(JsonType.Default, target.Friends[0].Age.Minutes.JsonType);

            JsonValue jv1 = target.MissingProperty as JsonValue;
            Assert.NotNull(jv1);

            JsonValue jv2 = target.MissingProperty1.MissingProperty2 as JsonValue;
            Assert.NotNull(jv2);

            Assert.Same(jv1, jv2);
            Assert.Same(target.Person.Name.MissingProperty, AnyInstance.DefaultJsonValue);
        }

        [Fact]
        public void CastingDefaultValueTest()
        {
            JsonValue jv = AnyInstance.DefaultJsonValue;
            dynamic d = jv;

            ExceptionHelper.Throws<InvalidCastException>(delegate { float p = (float)d; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { byte p = (byte)d; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { int p = (int)d; });

            Assert.Null((string)d);
        }
    }
}
