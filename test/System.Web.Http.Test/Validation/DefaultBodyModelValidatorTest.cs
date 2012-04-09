// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Validation
{
    public class DefaultBodyModelValidatorTest
    {
        private static Person LonelyPerson;

        static DefaultBodyModelValidatorTest()
        {
            LonelyPerson = new Person() { Name = "Reallllllllly Long Name" };
            LonelyPerson.Friend = LonelyPerson;
        }

        public static IEnumerable<object[]> ValidationErrors
        {
            get
            {
                return new TheoryDataSet<object, Type, Dictionary<string, string>>()
                {
                    // Primitives
                    { null, typeof(Person), new Dictionary<string, string>() },
                    { 14, typeof(int), new Dictionary<string, string>() },
                    { "foo", typeof(string), new Dictionary<string, string>() },

                    // Object Traversal : make sure we can traverse the object graph without throwing
                    { new ValueType() { Reference = "ref", Value = 256}, typeof(ValueType), new Dictionary<string, string>()},
                    { new ReferenceType() { Reference = "ref", Value = 256}, typeof(ReferenceType), new Dictionary<string, string>()},

                    // Classes
                    { new Person() { Name = "Rick", Profession = "Astronaut" }, typeof(Person), new Dictionary<string, string>() },
                    { new Person(), typeof(Person), new Dictionary<string, string>()
                        {
                            { "Name", "The Name field is required." },
                            { "Profession", "The Profession field is required." }
                        }
                    },

                    { new Person() { Name = "Rick", Friend = new Person() }, typeof(Person), new Dictionary<string, string>()
                        {
                            { "Profession", "The Profession field is required." },
                            { "Friend.Name", "The Name field is required." },
                            { "Friend.Profession", "The Profession field is required." }
                        }
                    },

                    // Collections
                    { new Person[] { new Person(), new Person() }, typeof(Person[]), new Dictionary<string, string>()
                        {
                            { "[0].Name", "The Name field is required." },
                            { "[0].Profession", "The Profession field is required." },
                            { "[1].Name", "The Name field is required." },
                            { "[1].Profession", "The Profession field is required." }
                        }
                    },

                    { new List<Person> { new Person(), new Person() }, typeof(Person[]), new Dictionary<string, string>()
                        {
                            { "[0].Name", "The Name field is required." },
                            { "[0].Profession", "The Profession field is required." },
                            { "[1].Name", "The Name field is required." },
                            { "[1].Profession", "The Profession field is required." }
                        }
                    },

                    { new Dictionary<string, Person> { { "Joe", new Person() } , { "Mark", new Person() } }, typeof(Dictionary<string, Person>), new Dictionary<string, string>()
                        {
                            { "[0].Value.Name", "The Name field is required." },
                            { "[0].Value.Profession", "The Profession field is required." },
                            { "[1].Value.Name", "The Name field is required." },
                            { "[1].Value.Profession", "The Profession field is required." }
                        }
                    },
                    
                    // Testing we don't blow up on cycles
                    { LonelyPerson, typeof(Person), new Dictionary<string, string>()
                        {
                            { "Name", "The field Name must be a string with a maximum length of 10." },
                            { "Profession", "The Profession field is required." }
                        }
                    },
                };
            }
        }

        [Theory]
        [PropertyData("ValidationErrors")]
        public void ExpectedValidationErrorsRaised(object model, Type type, Dictionary<string, string> expectedErrors)
        {
            // Arrange
            ModelMetadataProvider metadataProvider = new DataAnnotationsModelMetadataProvider();
            HttpActionContext actionContext = ContextUtil.CreateActionContext();

            // Act
            Assert.DoesNotThrow(() =>
                new DefaultBodyModelValidator().Validate(model, type, metadataProvider, actionContext, string.Empty)
            );

            // Assert
            Dictionary<string, string> actualErrors = new Dictionary<string, string>();
            foreach (KeyValuePair<string, ModelState> keyStatePair in actionContext.ModelState)
            {
                foreach (ModelError error in keyStatePair.Value.Errors)
                {
                    actualErrors.Add(keyStatePair.Key, error.ErrorMessage);
                }
            }

            Assert.Equal(expectedErrors.Count, actualErrors.Count);
            foreach (KeyValuePair<string, string> keyErrorPair in expectedErrors)
            {
                Assert.Contains(keyErrorPair.Key, actualErrors.Keys);
                Assert.Equal(keyErrorPair.Value, actualErrors[keyErrorPair.Key]);
            }
        }

        [Fact]
        public void MultipleValidationErrorsOnSameMemberReported()
        {
            // Arrange
            ModelMetadataProvider metadataProvider = new DataAnnotationsModelMetadataProvider();
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            object model = new Address() { Street = "Microsoft Way" };

            // Act
            Assert.DoesNotThrow(() =>
                new DefaultBodyModelValidator().Validate(model, typeof(Address), metadataProvider, actionContext, string.Empty)
            );

            // Assert
            Assert.Contains("Street", actionContext.ModelState.Keys);
            ModelState streetState = actionContext.ModelState["Street"];
            Assert.Equal(2, streetState.Errors.Count);
        }
    }

    public class Person
    {
        [Required]
        [StringLength(10)]
        public string Name { get; set; }

        [Required]
        public string Profession { get; set; }

        public Person Friend { get; set; }
    }

    public class Address
    {
        [StringLength(5)]
        [RegularExpression("hehehe")]
        public string Street { get; set; }
    }

    public struct ValueType
    {
        public int Value;
        public string Reference;
    }

    public class ReferenceType
    {
        public static string StaticProperty { get { return "static"; } }
        public int Value;
        public string Reference;
    }
}
