// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Moq;

namespace System.Web.OData.Formatter
{
    public class ODataTestUtil
    {
        private static IEdmModel _model;

        public const string Version4NumberString = "4.0";
        public static MediaTypeHeaderValue ApplicationJsonMediaType = MediaTypeHeaderValue.Parse("application/json");
        public static MediaTypeWithQualityHeaderValue ApplicationJsonMediaTypeWithQuality = MediaTypeWithQualityHeaderValue.Parse("application/json");

        public static void VerifyResponse(HttpContent actualContent, string expected)
        {
            string actual = actualContent.ReadAsStringAsync().Result;
            JsonAssert.Equal(expected, actual);
        }

        public static HttpRequestMessage GenerateRequestMessage(Uri address)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, address);
            requestMessage.Headers.Accept.Add(ApplicationJsonMediaTypeWithQuality);
            requestMessage.Headers.Add("OData-Version", "4.0");
            requestMessage.Headers.Add("OData-MaxVersion", "4.0");
            return requestMessage;
        }

        public static string GetDataServiceVersion(HttpContentHeaders headers)
        {
            string dataServiceVersion = null;
            IEnumerable<string> values;
            if (headers.TryGetValues("OData-Version", out values))
            {
                dataServiceVersion = values.FirstOrDefault();
            }
            return dataServiceVersion;
        }

        public static IEdmModel GetEdmModel()
        {
            if (_model == null)
            {
                ODataModelBuilder model = new ODataModelBuilder();

                var color = model.EnumType<Color>();
                color.Member(Color.Red);
                color.Member(Color.Green);
                color.Member(Color.Blue);

                var people = model.EntitySet<FormatterPerson>("People");
                people.HasFeedSelfLink(context => new Uri(context.Url.CreateODataLink(new EntitySetPathSegment(
                    context.EntitySetBase))));
                people.HasIdLink(context =>
                    {
                        return new Uri(context.Url.CreateODataLink(
                            new EntitySetPathSegment(context.NavigationSource as IEdmEntitySet),
                            new KeyValuePathSegment(context.GetPropertyValue("PerId").ToString())));
                    },
                    followsConventions: false);

                var person = people.EntityType;
                person.HasKey(p => p.PerId);
                person.Property(p => p.Age);
                person.Property(p => p.MyGuid);
                person.Property(p => p.Name);
                person.EnumProperty(p => p.FavoriteColor);
                person.ComplexProperty<FormatterOrder>(p => p.Order);

                var order = model.ComplexType<FormatterOrder>();
                order.Property(o => o.OrderAmount);
                order.Property(o => o.OrderName);

                // Add a top level function without parameter and the "IncludeInServiceDocument = true"
                var getPerson = model.Function("GetPerson");
                getPerson.ReturnsFromEntitySet<FormatterPerson>("People");
                getPerson.IncludeInServiceDocument = true;

                // Add a top level function without parameter and the "IncludeInServiceDocument = false"
                var getAddress = model.Function("GetAddress");
                getAddress.Returns<string>();
                getAddress.IncludeInServiceDocument = false;

                // Add an overload top level function with parameters and the "IncludeInServiceDocument = true"
                getPerson = model.Function("GetPerson");
                getPerson.Parameter<int>("PerId");
                getPerson.ReturnsFromEntitySet<FormatterPerson>("People");
                getPerson.IncludeInServiceDocument = true;

                // Add an overload top level function with parameters and the "IncludeInServiceDocument = false"
                getAddress = model.Function("GetAddress");
                getAddress.Parameter<int>("AddressId");
                getAddress.Returns<string>();
                getAddress.IncludeInServiceDocument = false;

                // Add an overload top level function
                var getVipPerson = model.Function("GetVipPerson");
                getVipPerson.Parameter<string>("name");
                getVipPerson.ReturnsFromEntitySet<FormatterPerson>("People");
                getVipPerson.IncludeInServiceDocument = true;

                // Add a top level function which is included in service document
                getVipPerson = model.Function("GetVipPerson");
                getVipPerson.ReturnsFromEntitySet<FormatterPerson>("People");
                getVipPerson.IncludeInServiceDocument = true;

                // Add an overload top level function
                getVipPerson = model.Function("GetVipPerson");
                getVipPerson.Parameter<int>("PerId");
                getVipPerson.Parameter<string>("name");
                getVipPerson.ReturnsFromEntitySet<FormatterPerson>("People");
                getVipPerson.IncludeInServiceDocument = true;

                // Add a top level function with parameters and without any overload
                var getSalary = model.Function("GetSalary");
                getSalary.Parameter<int>("PerId");
                getSalary.Parameter<string>("month");
                getSalary.Returns<int>();
                getSalary.IncludeInServiceDocument = true;

                // Add Singleton
                var president = model.Singleton<FormatterPerson>("President");
                president.HasIdLink(context =>
                    {
                        return new Uri(context.Url.CreateODataLink(new SingletonPathSegment((IEdmSingleton)context.NavigationSource)));
                    },
                    followsConventions: false);

                _model = model.GetEdmModel();
            }

            return _model;
        }

        public static ODataMessageWriter GetMockODataMessageWriter()
        {
            MockODataRequestMessage requestMessage = new MockODataRequestMessage();
            return new ODataMessageWriter(requestMessage);
        }

        public static ODataMessageReader GetMockODataMessageReader()
        {
            MockODataRequestMessage requestMessage = new MockODataRequestMessage();
            return new ODataMessageReader(requestMessage);
        }

        public static ODataSerializerProvider GetMockODataSerializerProvider(ODataEdmTypeSerializer serializer)
        {
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(sp => sp.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(serializer);
            return serializerProvider.Object;
        }
    }

    public class FormatterPerson
    {
        public int Age { get; set; }
        public Guid MyGuid { get; set; }
        public string Name { get; set; }
        public FormatterOrder Order { get; set; }
        public Color FavoriteColor { get; set; }
        [Key]
        public int PerId { get; set; }
    }

    public class FormatterOrder
    {
        public int OrderAmount { get; set; }
        public string OrderName { get; set; }
    }

    public class FormatterAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public IDictionary<string, object> Properties { get; set; }
    }
}
