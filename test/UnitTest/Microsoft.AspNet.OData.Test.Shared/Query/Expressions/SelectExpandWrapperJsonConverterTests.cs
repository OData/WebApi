//-----------------------------------------------------------------------------
// <copyright file="SelectExpandWrapperJsonConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query.Expressions
{
    public class SelectExpandWrapperJsonConverterTests
    {
        [Theory]
        [InlineData(typeof(SelectExpandBinder.SelectAll<object>), true)]
        [InlineData(typeof(SelectExpandBinder.SelectAllAndExpand<object>), true)]
        [InlineData(typeof(SelectExpandBinder.SelectSome<object>), true)]
        [InlineData(typeof(SelectExpandBinder.SelectSomeAndInheritance<object>), true)]
        [InlineData(typeof(SelectExpandWrapper<object>), true)]
        [InlineData(typeof(object), false)]
        public void CanConvertWorksForSelectExpandWrapper(Type type, bool expected)
        {
            // Arrange
            var converterFactory = new SelectExpandWrapperJsonConverter();

            // Act & Assert
            Assert.Equal(expected, converterFactory.CanConvert(type));
        }

        [Fact]
        public void CanConvertThrowsExceptionForNullSelectExpandWrapper()
        {
            // Arrange
            var converterFactory = new SelectExpandWrapperJsonConverter();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => converterFactory.CanConvert(null));
        }

        [Theory]
        [InlineData(typeof(SelectExpandBinder.SelectAll<object>), "SelectAllOfTJsonConverter")]
        [InlineData(typeof(SelectExpandBinder.SelectAllAndExpand<object>), "SelectAllAndExpandOfTJsonConverter")]
        [InlineData(typeof(SelectExpandBinder.SelectSome<object>), "SelectSomeOfTJsonConverter")]
        [InlineData(typeof(SelectExpandBinder.SelectSomeAndInheritance<object>), "SelectSomeAndInheritanceOfTJsonConverter")]
        [InlineData(typeof(SelectExpandWrapper<object>), "SelectExpandWrapperOfTJsonConverter")]
        public void CreateConvertWorksForSelectExpandWrapper(Type type, string expected)
        {
            // Arrange
            var jsonSerializerOptions = new JsonSerializerOptions();
            var converterFactory = new SelectExpandWrapperJsonConverter();

            // Act
            var result = converterFactory.CreateConverter(type, jsonSerializerOptions);

            // Assert - The created converters are private so we assert on name
            Assert.Contains(expected, result.GetType().Name);
        }

        [Theory]
        [InlineData(typeof(FlatteningWrapper<object>))]
        [InlineData(typeof(object))]
        public void CreateConvertReturnsNullForUnexpectedTypeToConvertArgument(Type type)
        {
            // Arrange
            var jsonSerializerOptions = new JsonSerializerOptions();
            var converterFactory = new SelectExpandWrapperJsonConverter();

            // Act & Assert
            Assert.Null(converterFactory.CreateConverter(type, jsonSerializerOptions));
        }

        [Fact]
        public void SelectExpandWrapperJsonConverterWorksForSelectAllOfT()
        {
            // Arrange, Act & Assert
            SerializeTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectAll<TrivialEntity>>();

            // Arrange, Act & Assert
            DeserializeTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectAll<TrivialEntity>>();

            // Arrange, Act & Assert
            SerializeNonTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectAll<NonTrivialEntity>>();
        }

        [Fact]
        public void SelectExpandWrapperJsonConverterWorksForSelectAllAndExpandOfT()
        {
            // Arrange, Act & Assert
            SerializeTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectAllAndExpand<TrivialEntity>>();

            // Arrange, Act & Assert
            DeserializeTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectAllAndExpand<TrivialEntity>>();

            // Arrange, Act & Assert
            SerializeNonTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectAllAndExpand<NonTrivialEntity>>();
        }

        [Fact]
        public void SelectExpandWrapperJsonConverterWorksForSelectSomeOfT()
        {
            // Arrange, Act & Assert
            SerializeTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectSome<TrivialEntity>>();

            // Arrange, Act & Assert
            DeserializeTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectSome<TrivialEntity>>();

            // Arrange, Act & Assert
            SerializeNonTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectSome<NonTrivialEntity>>();
        }

        [Fact]
        public void SelectExpandWrapperJsonConverterWorksForSelectSomeAndInheritanceOfT()
        {
            // Arrange, Act & Assert
            SerializeTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectSomeAndInheritance<TrivialEntity>>();

            // Arrange, Act & Assert
            DeserializeTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectSomeAndInheritance<TrivialEntity>>();

            // Arrange, Act & Assert
            SerializeNonTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandBinder.SelectSomeAndInheritance<NonTrivialEntity>>();
        }

        [Fact]
        public void SelectExpandWrapperJsonConverterWorksForSelectExpandWrapperOfT()
        {
            // Arrange, Act & Assert
            SerializeTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandWrapper<TrivialEntity>>();

            // Arrange, Act & Assert
            DeserializeTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandWrapper<TrivialEntity>>();

            // Arrange, Act & Assert
            SerializeNonTrivialPayloadUsingSelectExpandWriterJsonConverter<SelectExpandWrapper<NonTrivialEntity>>();
        }

        private static void SerializeTrivialPayloadUsingSelectExpandWriterJsonConverter<T>() where T : SelectExpandWrapper
        {
            // Arrange
            var selectExpandWrapper = (T)Activator.CreateInstance(typeof(T));
            var mockPropertyContainer = new MockPropertyContainer(new Dictionary<string, object> { { "Property", "foobar" } });
            var model = GetEdmModel();

            selectExpandWrapper.Container = mockPropertyContainer;
            selectExpandWrapper.UseInstanceForProperties = true;
            selectExpandWrapper.ModelID = ModelContainer.GetModelID(model);

            var selectExpandWrapperOfT = selectExpandWrapper as SelectExpandWrapper<TrivialEntity>;
            Assert.NotNull(selectExpandWrapperOfT);

            selectExpandWrapperOfT.Instance = new TrivialEntity
            {
                Property = "foobar"
            };

            var jsonSerializerOptions = new JsonSerializerOptions();
            var converterFactory = new SelectExpandWrapperJsonConverter();
            var converter = converterFactory.CreateConverter(typeof(T), jsonSerializerOptions) as JsonConverter<T>;

            var memoryStream = new MemoryStream();
            var utf8JsonWriter = new Utf8JsonWriter(memoryStream);

            // Act
            converter.Write(utf8JsonWriter, selectExpandWrapper, jsonSerializerOptions);
            memoryStream.Position = 0;

            var result = new StreamReader(memoryStream).ReadToEnd();

            // Assert
            Assert.Equal("{\"Property\":\"foobar\"}", result);
        }

        private static void DeserializeTrivialPayloadUsingSelectExpandWriterJsonConverter<T>() where T : SelectExpandWrapper
        {
            // Arrange
            var jsonSerializerOptions = new JsonSerializerOptions();
            var converterFactory = new SelectExpandWrapperJsonConverter();
            var converter = converterFactory.CreateConverter(typeof(T), jsonSerializerOptions) as JsonConverter<T>;

            // Act & Assert
            var exception = Assert.Throws<NotImplementedException>(() =>
            {
                var utf8JsonReader = new Utf8JsonReader();

                converter.Read(ref utf8JsonReader, typeof(object), jsonSerializerOptions);
            });

            Assert.Equal($"'{typeof(T).Name}' is internal and should never be deserialized into.", exception.Message);
        }

        private void SerializeNonTrivialPayloadUsingSelectExpandWriterJsonConverter<T>() where T : SelectExpandWrapper
        {
            // Arrange
            var selectExpandWrapper = (T)Activator.CreateInstance(typeof(T));
            var mockPropertyContainer = new MockPropertyContainer(new Dictionary<string, object> {
                { "Property1", "Prop1Value" },
                { "Property2", "Prop2Value" },
                { "Property3", "Prop3Value" },
                { "Property4", "Prop4Value" }
            });

            var model = GetEdmModel();

            selectExpandWrapper.Container = mockPropertyContainer;
            selectExpandWrapper.UseInstanceForProperties = false;
            selectExpandWrapper.ModelID = ModelContainer.GetModelID(model);

            var selectExpandWrapperOfT = selectExpandWrapper as SelectExpandWrapper<NonTrivialEntity>;
            Assert.NotNull(selectExpandWrapperOfT);

            var jsonSerializerOptions = new JsonSerializerOptions();
            var converterFactory = new SelectExpandWrapperJsonConverter();
            var converter = converterFactory.CreateConverter(typeof(T), jsonSerializerOptions) as JsonConverter<T>;

            var memoryStream = new MemoryStream();
            var utf8JsonWriter = new Utf8JsonWriter(memoryStream);

            // Act
            converter.Write(utf8JsonWriter, selectExpandWrapper, jsonSerializerOptions);
            memoryStream.Position = 0;

            var result = new StreamReader(memoryStream).ReadToEnd();

            // Assert
            Assert.Equal("{\"Property1\":\"Prop1Value\",\"NsjProperty2\":\"Prop2Value\",\"StjProperty3\":\"Prop3Value\",\"NsjProperty4\":\"Prop4Value\"}", result);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<TrivialEntity>();
            builder.EntityType<NonTrivialEntity>();

            var model = builder.GetEdmModel();

            // Set an annotation value for MultiEntity.Property1 to support a test case
            var multiEntity = model.SchemaElements.First(d => d.Name.Equals(typeof(NonTrivialEntity).Name)) as EdmEntityType;
            var edmProperty1 = multiEntity.Properties().First(d => d.Name.Equals("Property1"));
            var clrPropertyAnnotion = new ClrPropertyInfoAnnotation(typeof(NonTrivialEntity).GetProperty("Property1"));
            model.SetAnnotationValue(edmProperty1, clrPropertyAnnotion);

            return model;
        }

        private class MockPropertyContainer : PropertyContainer
        {
            public MockPropertyContainer(Dictionary<string, object> properties)
            {
                Properties = properties;
            }

            public Dictionary<string, object> Properties { get; private set; }

            public override void ToDictionaryCore(
                Dictionary<string, object> dictionary,
                IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                foreach (var kvp in Properties)
                {
                    dictionary.Add(propertyMapper.MapProperty(kvp.Key), kvp.Value);
                }
            }
        }

        private class TrivialEntity
        {
            public string Property { get; set; }
        }

        private class NonTrivialEntity
        {
            public string Property1 { get; set; }

            [Newtonsoft.Json.JsonProperty("NsjProperty2")]
            public string Property2 { get; set; }

            [JsonPropertyName("StjProperty3")]
            public string Property3 { get; set; }

            [Newtonsoft.Json.JsonProperty("NsjProperty4")]
            [JsonPropertyName("StjProperty4")]
            public string Property4 { get; set; }
        }
    }
}
#endif
