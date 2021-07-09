// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class ODataEntityReferenceLinkTests
    {
        private readonly ODataDeserializerProvider _deserializerProvider;
        public ODataEntityReferenceLinkTests()
        {
            _deserializerProvider = ODataDeserializerProviderFactory.Create();
        }

        /// <summary>
        /// In OData v4.0 an ODataEntityReferenceLink will be converted 
        /// to a resource then deserialized as a resource.
        /// </summary>
        [Fact]
        public void ReadResource_CanRead_AnEntityRefenceLink()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var books = builder.EntitySet<Book>("Books");
            builder.EntityType<Author>();
            builder.EntitySet<Author>("Authors");
            var author =
               books.EntityType.HasOptional<Author>((e) => e.Author);
            books.HasNavigationPropertyLink(author, (a, b) => new Uri("aa:b"), false);
            books.HasOptionalBinding((e) => e.Author, "authorr");


            IEdmModel model = builder.GetEdmModel();
            IEdmEntityTypeReference bookTypeReference = model.GetEdmTypeReference(typeof(Book)).AsEntity();
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataResource odataResource = new ODataResource
            {
                Properties = new[]
                {
                    new ODataProperty { Name = "Id", Value = 1},
                    new ODataProperty { Name = "Name", Value = "BookA"},
                },
                TypeName = "Microsoft.AspNet.OData.Test.Formatter.Book"
            };

            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Books");
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));
            var request = RequestFactory.CreateFromModel(model, path: path);

            ODataDeserializerContext readContext = new ODataDeserializerContext()
            {
                Model = model,
                Request = request,
                Path = path
            };

            ODataResourceWrapper topLevelResourceWrapper = new ODataResourceWrapper(odataResource);
            ODataNestedResourceInfo resourceInfo = new ODataNestedResourceInfo
            {
                IsCollection = false,
                Name = "Author"
            };

            ODataEntityReferenceLink refLink = new ODataEntityReferenceLink { Url = new Uri("http://localhost/Authors(2)") };
            ODataEntityReferenceLinkBase refLinkWrapper = new ODataEntityReferenceLinkBase(refLink);

            ODataNestedResourceInfoWrapper resourceInfoWrapper = new ODataNestedResourceInfoWrapper(resourceInfo);
            resourceInfoWrapper.NestedItems.Add(refLinkWrapper);
            topLevelResourceWrapper.NestedResourceInfos.Add(resourceInfoWrapper);

            // Act
            Book book = deserializer.ReadResource(topLevelResourceWrapper, bookTypeReference, readContext)
                as Book;

            // Assert
            Assert.NotNull(book);
            Assert.Equal(2, book.Author.Id);
            Assert.NotNull(book.Author);
       
        }

        [Fact]
        public void ReadResource_CanRead_ACollectionOfEntityRefenceLinks()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var books = builder.EntitySet<Book>("Books");
            builder.EntityType<Author>();
            builder.EntitySet<Author>("Authors");
            var author =
               books.EntityType.HasOptional<Author>((e) => e.Author);
            books.HasNavigationPropertyLink(author, (a, b) => new Uri("aa:b"), false);
            books.HasOptionalBinding((e) => e.Author, "authorr");


            IEdmModel model = builder.GetEdmModel();
            IEdmEntityTypeReference bookTypeReference = model.GetEdmTypeReference(typeof(Book)).AsEntity();
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataResource odataResource = new ODataResource
            {
                Properties = new[]
                {
                    new ODataProperty { Name = "Id", Value = 1},
                    new ODataProperty { Name = "Name", Value = "BookA"},
                },
                TypeName = "Microsoft.AspNet.OData.Test.Formatter.Book"
            };

            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Books");
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));
            var request = RequestFactory.CreateFromModel(model, path: path);

            ODataDeserializerContext readContext = new ODataDeserializerContext()
            {
                Model = model,
                Request = request,
                Path = path
            };

            ODataResourceWrapper topLevelResourceWrapper = new ODataResourceWrapper(odataResource);
            ODataNestedResourceInfo resourceInfo = new ODataNestedResourceInfo
            {
                IsCollection = true,
                Name = "AuthorList"
            };

            IList<ODataEntityReferenceLinkBase> refLinks = new List<ODataEntityReferenceLinkBase>()
            {
                new ODataEntityReferenceLinkBase(new ODataEntityReferenceLink{ Url = new Uri("http://localhost/Authors(2)") }),
                new ODataEntityReferenceLinkBase(new ODataEntityReferenceLink{ Url = new Uri("http://localhost/Authors(3)")})
            };


            ODataNestedResourceInfoWrapper resourceInfoWrapper = new ODataNestedResourceInfoWrapper(resourceInfo);

            foreach (ODataEntityReferenceLinkBase refLinkWrapper in refLinks)
            {
                resourceInfoWrapper.NestedItems.Add(refLinkWrapper);
            }
            topLevelResourceWrapper.NestedResourceInfos.Add(resourceInfoWrapper);

            // Act
            Book book = deserializer.ReadResource(topLevelResourceWrapper, bookTypeReference, readContext)
                as Book;

            // Assert
            Assert.NotNull(book);
            Assert.NotNull(book.AuthorList);
            Assert.Equal(2, book.AuthorList.Count());
        }

        public class Book
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public Author Author { get; set; }
            public IList<Author> AuthorList { get; set; }
        }

        public class Author
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}    
