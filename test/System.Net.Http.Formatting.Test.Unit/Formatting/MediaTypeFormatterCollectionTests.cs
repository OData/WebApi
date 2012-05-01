// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Web.Http;
using Microsoft.TestCommon;
using Xunit;
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
        public void Constructor1AcceptsEmptyList()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Equal(0, collection.Count);
        }

        [Fact]
        public void Constructor1SetsProperties()
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
        public void Constructor1SetsDerivedFormatters()
        {
            // force to array to get stable instances
            MediaTypeFormatter[] derivedFormatters = HttpUnitTestDataSets.DerivedFormatters.ToArray();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(derivedFormatters);
            Assert.True(derivedFormatters.SequenceEqual(collection));
        }

        [Fact]
        public void Constructor1ThrowsWithNullFormatters()
        {
            Assert.ThrowsArgumentNull(() => new MediaTypeFormatterCollection(null), "formatters");
        }

        [Fact]
        public void Constructor1ThrowsWithNullFormatterInCollection()
        {
            Assert.ThrowsArgument(
                () => new MediaTypeFormatterCollection(new MediaTypeFormatter[] { null }), "formatters",
                Error.Format(Properties.Resources.CannotHaveNullInList,
                typeof(MediaTypeFormatter).Name));
        }

        [Fact]
        public void Constructor1AcceptsDuplicateFormatterTypes()
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
        public void XmlFormatterSetByCtor()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.XmlFormatter);
        }

        [Fact]
        public void XmlFormatterClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.XmlFormatter);
        }



        [Fact]
        public void JsonFormatterSetByCtor()
        {
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.JsonFormatter);
        }

        [Fact]
        public void JsonFormatterClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.JsonFormatter);
        }




        [Fact]
        public void FormUrlEncodedFormatterSetByCtor()
        {
            FormUrlEncodedMediaTypeFormatter formatter = new FormUrlEncodedMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.FormUrlEncodedFormatter);
        }

        [Fact]
        public void FormUrlEncodedFormatterClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.FormUrlEncodedFormatter);
        }






        [Fact]
        public void RemoveSetsXmlFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            collection.Remove(collection.XmlFormatter);
            Assert.Null(collection.XmlFormatter);
            Assert.Equal(count - 1, collection.Count);
        }

        [Fact]
        public void RemoveSetsJsonFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            collection.Remove(collection.JsonFormatter);
            Assert.Null(collection.JsonFormatter);
            Assert.Equal(count - 1, collection.Count);
        }

        [Fact]
        public void InsertSetsXmlFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            collection.Insert(0, formatter);
            Assert.Same(formatter, collection.XmlFormatter);
            Assert.Equal(count + 1, collection.Count);
        }

        [Fact]
        public void InsertSetsJsonFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            collection.Insert(0, formatter);
            Assert.Same(formatter, collection.JsonFormatter);
            Assert.Equal(count + 1, collection.Count);
        }

    }
}
