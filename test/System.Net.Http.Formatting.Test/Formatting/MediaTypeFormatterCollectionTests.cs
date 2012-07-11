// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class MediaTypeFormatterCollectionTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(MediaTypeFormatterCollection), TypeAssert.TypeProperties.IsPublicVisibleClass, typeof(Collection<MediaTypeFormatter>));
        }

        [Fact]
        public void Constructor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            Assert.Equal(3, collection.Count);
            Assert.NotNull(collection.XmlFormatter);
            Assert.NotNull(collection.JsonFormatter);
            Assert.NotNull(collection.FormUrlEncodedFormatter);
        }

        [Fact]
        public void Constructor1_AcceptsEmptyList()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Equal(0, collection.Count);
        }

        [Fact]
        public void Constructor1_SetsProperties()
        {
            // All combination of formatters presented to ctor should still set XmlFormatter
            foreach (IEnumerable<MediaTypeFormatter> formatterCollection in HttpUnitTestDataSets.AllFormatterCollections)
            {
                MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(formatterCollection);
                if (collection.OfType<XmlMediaTypeFormatter>().Any())
                {
                    Assert.NotNull(collection.XmlFormatter);
                }
                else
                {
                    Assert.Null(collection.XmlFormatter);
                }

                if (collection.OfType<JsonMediaTypeFormatter>().Any())
                {
                    Assert.NotNull(collection.JsonFormatter);
                }
                else
                {
                    Assert.Null(collection.JsonFormatter);
                }
            }
        }

        [Fact]
        public void Constructor1_SetsDerivedFormatters()
        {
            // force to array to get stable instances
            MediaTypeFormatter[] derivedFormatters = HttpUnitTestDataSets.DerivedFormatters.ToArray();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(derivedFormatters);
            Assert.True(derivedFormatters.SequenceEqual(collection));
        }

        [Fact]
        public void Constructor1_ThrowsWithNullFormatters()
        {
            Assert.ThrowsArgumentNull(() => new MediaTypeFormatterCollection(null), "formatters");
        }

        [Fact]
        public void Constructor1_ThrowsWithNullFormatterInCollection()
        {
            Assert.ThrowsArgument(
                () => new MediaTypeFormatterCollection(new MediaTypeFormatter[] { null }), "formatters",
                Error.Format(Properties.Resources.CannotHaveNullInList,
                typeof(MediaTypeFormatter).Name));
        }

        [Fact]
        public void Constructor1_AcceptsDuplicateFormatterTypes()
        {
            MediaTypeFormatter[] formatters = new MediaTypeFormatter[]
            {
                new XmlMediaTypeFormatter(),
                new JsonMediaTypeFormatter(),
                new FormUrlEncodedMediaTypeFormatter(),
                new XmlMediaTypeFormatter(),
                new JsonMediaTypeFormatter(),
                new FormUrlEncodedMediaTypeFormatter(),
            };

            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(formatters);
            Assert.True(formatters.SequenceEqual(collection));
        }

        [Fact]
        public void XmlFormatter_SetByCtor()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.XmlFormatter);
        }

        [Fact]
        public void XmlFormatter_ClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.XmlFormatter);
        }

        [Fact]
        public void JsonFormatter_SetByCtor()
        {
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.JsonFormatter);
        }

        [Fact]
        public void JsonFormatter_ClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.JsonFormatter);
        }

        [Fact]
        public void FormUrlEncodedFormatter_SetByCtor()
        {
            FormUrlEncodedMediaTypeFormatter formatter = new FormUrlEncodedMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.FormUrlEncodedFormatter);
        }

        [Fact]
        public void FormUrlEncodedFormatter_ClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.FormUrlEncodedFormatter);
        }

        [Fact]
        public void Remove_SetsXmlFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            collection.Remove(collection.XmlFormatter);
            Assert.Null(collection.XmlFormatter);
            Assert.Equal(count - 1, collection.Count);
        }

        [Fact]
        public void Remove_SetsJsonFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            collection.Remove(collection.JsonFormatter);
            Assert.Null(collection.JsonFormatter);
            Assert.Equal(count - 1, collection.Count);
        }

        [Fact]
        public void Insert_SetsXmlFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            collection.Insert(0, formatter);
            Assert.Same(formatter, collection.XmlFormatter);
            Assert.Equal(count + 1, collection.Count);
        }

        [Fact]
        public void Insert_SetsJsonFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            collection.Insert(0, formatter);
            Assert.Same(formatter, collection.JsonFormatter);
            Assert.Equal(count + 1, collection.Count);
        }

        [Fact]
        public void FindReader_ThrowsOnNullType()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/test");
            Assert.ThrowsArgumentNull(() => collection.FindReader(type: null, mediaType: mediaType), "type");
        }

        [Fact]
        public void FindReader_ThrowsOnNullMediaType()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            Assert.ThrowsArgumentNull(() => collection.FindReader(type: typeof(object), mediaType: null), "mediaType");
        }

        [Fact]
        public void FindReader_ReturnsNullOnNoMatch()
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };

            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            collection.Clear();
            collection.Add(formatter);

            MediaTypeHeaderValue contentType = new MediaTypeHeaderValue("text/test");

            // Act
            MediaTypeFormatter actualFormatter = collection.FindReader(typeof(object), contentType);

            // Assert
            Assert.Null(actualFormatter);
        }

        [Theory]
        [TestDataSet(
            typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        public void FindReader_ReturnsFormatterOnMatch(Type variationType, object testData, string mediaType)
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            foreach (string legalMediaType in HttpUnitTestDataSets.LegalMediaTypeStrings)
            {
                formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(legalMediaType));
            }

            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            collection.Clear();
            collection.Add(formatter);

            MediaTypeHeaderValue contentType = new MediaTypeHeaderValue(mediaType);

            // Act
            MediaTypeFormatter actualFormatter = collection.FindReader(variationType, contentType);

            // Assert
            Assert.Same(formatter, actualFormatter);
        }

        [Fact]
        public void FindWriter_ThrowsOnNullType()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/test");
            Assert.ThrowsArgumentNull(() => collection.FindWriter(type: null, mediaType: mediaType), "type");
        }

        [Fact]
        public void FindWriter_ThrowsOnNullMediaType()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            Assert.ThrowsArgumentNull(() => collection.FindWriter(type: typeof(object), mediaType: null), "mediaType");
        }

        [Fact]
        public void FindWriter_ReturnsNullOnNoMatch()
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };

            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            collection.Clear();
            collection.Add(formatter);

            MediaTypeHeaderValue contentType = new MediaTypeHeaderValue("text/test");

            // Act
            MediaTypeFormatter actualFormatter = collection.FindWriter(typeof(object), contentType);

            // Assert
            Assert.Null(actualFormatter);
        }

        [Theory]
        [TestDataSet(
            typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        public void FindWriter_ReturnsFormatterOnMatch(Type variationType, object testData, string mediaType)
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            foreach (string legalMediaType in HttpUnitTestDataSets.LegalMediaTypeStrings)
            {
                formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
            }

            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            collection.Clear();
            collection.Add(formatter);

            MediaTypeHeaderValue contentType = new MediaTypeHeaderValue(mediaType);

            // Act
            MediaTypeFormatter actualFormatter = collection.FindWriter(variationType, contentType);

            // Assert
            Assert.Same(formatter, actualFormatter);
        }
    }
}
