// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class MediaTypeFormatterCollectionTests
    {

        [Fact]
        [Trait("Description", "MediaTypeFormatterCollection is public, concrete, and unsealed.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(MediaTypeFormatterCollection), TypeAssert.TypeProperties.IsPublicVisibleClass, typeof(Collection<MediaTypeFormatter>));
        }

        [Fact]
        [Trait("Description", "MediaTypeFormatterCollection() initializes default formatters.")]
        public void Constructor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            Assert.Equal(3, collection.Count);
            Assert.NotNull(collection.XmlFormatter);
            Assert.NotNull(collection.JsonFormatter);
            Assert.NotNull(collection.FormUrlEncodedFormatter);
        }

        [Fact]
        [Trait("Description", "MediaTypeFormatterCollection(IEnumerable<MediaTypeFormatter>) accepts empty collection and does not add to it.")]
        public void Constructor1AcceptsEmptyList()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Equal(0, collection.Count);
        }

        [Fact]
        [Trait("Description", "MediaTypeFormatterCollection(IEnumerable<MediaTypeFormatter>) sets XmlFormatter and JsonFormatter for all known collections of formatters that contain them.")]
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
        [Trait("Description", "MediaTypeFormatterCollection(IEnumerable<MediaTypeFormatter>) sets derived classes of Xml and Json formatters.")]
        public void Constructor1SetsDerivedFormatters()
        {
            // force to array to get stable instances
            MediaTypeFormatter[] derivedFormatters = HttpUnitTestDataSets.DerivedFormatters.ToArray();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(derivedFormatters);
            Assert.True(derivedFormatters.SequenceEqual(collection));
        }

        [Fact]
        [Trait("Description", "MediaTypeFormatterCollection(IEnumerable<MediaTypeFormatter>) throws with null formatters collection.")]
        public void Constructor1ThrowsWithNullFormatters()
        {
            Assert.ThrowsArgumentNull(() => new MediaTypeFormatterCollection(null), "formatters");
        }

        [Fact]
        [Trait("Description", "MediaTypeFormatterCollection(IEnumerable<MediaTypeFormatter>) throws with null formatter in formatters collection.")]
        public void Constructor1ThrowsWithNullFormatterInCollection()
        {
            Assert.ThrowsArgument(
                () => new MediaTypeFormatterCollection(new MediaTypeFormatter[] { null }), "formatters",
                RS.Format(Properties.Resources.CannotHaveNullInList,
                typeof(MediaTypeFormatter).Name));
        }

        [Fact]
        [Trait("Description", "MediaTypeFormatterCollection(IEnumerable<MediaTypeFormatter>) accepts multiple instances of same formatter type.")]
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
        [Trait("Description", "XmlFormatter is set by ctor.")]
        public void XmlFormatterSetByCtor()
        {
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.XmlFormatter);
        }

        [Fact]
        [Trait("Description", "XmlFormatter is cleared by ctor with empty collection.")]
        public void XmlFormatterClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.XmlFormatter);
        }



        [Fact]
        [Trait("Description", "JsonFormatter is set by ctor.")]
        public void JsonFormatterSetByCtor()
        {
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.JsonFormatter);
        }

        [Fact]
        [Trait("Description", "JsonFormatter is cleared by ctor with empty collection.")]
        public void JsonFormatterClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.JsonFormatter);
        }




        [Fact]
        [Trait("Description", "FormUrlEncodedFormatter is set by ctor.")]
        public void FormUrlEncodedFormatterSetByCtor()
        {
            FormUrlEncodedMediaTypeFormatter formatter = new FormUrlEncodedMediaTypeFormatter();
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[] { formatter });
            Assert.Same(formatter, collection.FormUrlEncodedFormatter);
        }

        [Fact]
        [Trait("Description", "FormUrlEncodedFormatter is cleared by ctor with empty collection.")]
        public void FormUrlEncodedFormatterClearedByCtor()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection(new MediaTypeFormatter[0]);
            Assert.Null(collection.FormUrlEncodedFormatter);
        }






        [Fact]
        [Trait("Description", "Remove(MediaTypeFormatter) sets XmlFormatter to null.")]
        public void RemoveSetsXmlFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            collection.Remove(collection.XmlFormatter);
            Assert.Null(collection.XmlFormatter);
            Assert.Equal(count - 1, collection.Count);
        }

        [Fact]
        [Trait("Description", "Remove(MediaTypeFormatter) sets JsonFormatter to null.")]
        public void RemoveSetsJsonFormatter()
        {
            MediaTypeFormatterCollection collection = new MediaTypeFormatterCollection();
            int count = collection.Count;
            collection.Remove(collection.JsonFormatter);
            Assert.Null(collection.JsonFormatter);
            Assert.Equal(count - 1, collection.Count);
        }

        [Fact]
        [Trait("Description", "Insert(int, MediaTypeFormatter) sets XmlFormatter.")]
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
        [Trait("Description", "Insert(int, MediaTypeFormatter) sets JsonFormatter.")]

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
