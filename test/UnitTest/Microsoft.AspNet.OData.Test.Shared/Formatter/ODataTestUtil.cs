//-----------------------------------------------------------------------------
// <copyright file="ODataTestUtil.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Formatter.Deserialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class ODataTestUtil
    {
        private static IEdmModel _model;

        public const string Version4NumberString = "4.0";
        public static MediaTypeHeaderValue ApplicationJsonMediaType = MediaTypeHeaderValue.Parse("application/json");
        public static MediaTypeWithQualityHeaderValue ApplicationJsonMediaTypeWithQuality = MediaTypeWithQualityHeaderValue.Parse("application/json");

        internal static ODataMediaTypeMapping ApplicationJsonMediaTypeWithQualityMapping =
#if NETCORE
            new ODataMediaTypeMapping(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));
#else
            new ODataMediaTypeMapping(ODataTestUtil.ApplicationJsonMediaTypeWithQuality);
#endif

        public static async Task VerifyResponse(HttpContent actualContent, string expected)
        {
            string actual = await actualContent.ReadAsStringAsync();
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

        public static string GetDataServiceVersion(HttpHeaders headers)
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

                people.HasFeedSelfLink(context => new Uri(context.InternalUrlHelper.CreateODataLink(new EntitySetSegment(
                    context.EntitySetBase as IEdmEntitySet))));
                people.HasIdLink(context =>
                {
                    var keys = new[] {new KeyValuePair<string, object>("PerId", context.GetPropertyValue("PerId"))};
                        return new Uri(context.InternalUrlHelper.CreateODataLink(
                            new EntitySetSegment(context.NavigationSource as IEdmEntitySet),
                            new KeySegment(keys, context.StructuredType as IEdmEntityType, context.NavigationSource)));
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

                // Add a function to test namespace configuration
                var getNSFunction = model.Function("GetNS");
                getNSFunction.Returns<int>();
                getNSFunction.Namespace = "CustomizeNamepace";

                // Add Singleton
                var president = model.Singleton<FormatterPerson>("President");
                president.HasIdLink(context =>
                    {
                        return new Uri(context.InternalUrlHelper.CreateODataLink(new SingletonSegment((IEdmSingleton)context.NavigationSource)));
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

    public class FormatterAccount
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public FormatterAddress Address { get; set; }

        [Required]
        public IList<FormatterAddress> Addresses { get; set; } 
    }

    public class FormatterUsAddress : FormatterAddress
    {
        public string UsProperty { get; set; }
    }

    public class ComplexBaseType
    {
        public string BaseProperty { get; set; }
    }

    public class ComplexDerivedOpenType : ComplexBaseType
    {
        public string DerivedProperty { get; set; }
        public IDictionary<string, object> Properties { get; set; }
    }

    public class ForeignCustomer
    {
        public int ForeignCustomerId { get; set; }

        public int OtherCustomerKey { get; set; }

        public IList<ForeignOrder> Orders { get; set; }
    }

    public class ForeignOrder
    {
        public int ForeignOrderId { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public ForeignCustomer Customer { get; set; }
    }

    public class ForeignCustomer2
    {
        public int Id { get; set; }

        public IList<ForeignOrder2> Orders { get; set; }
    }

    public class ForeignOrder2
    {
        public int Id { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [ActionOnDelete(EdmOnDeleteAction.Cascade)]
        public ForeignCustomer2 Customer { get; set; }
    }

    public class MultiForeignCustomer
    {
        public int CustomerId1 { get; set; }
        public string CustomerId2 { get; set; }

        public IList<MultiForeignOrder> Orders { get; set; }
    }

    public class MultiForeignOrder
    {
        public int ForeignOrderId { get; set; }

        public int CustomerForeignKey1 { get; set; }
        public string CustomerForeignKey2 { get; set; }

        public MultiForeignCustomer Customer { get; set; }
    }

    public class BasePrincipalEntity
    {
        public int Id { get; set; }
    }

    public class DerivedPrincipalEntity : BasePrincipalEntity
    {
        public string Name { get; set; }
    }

    public class DependentEntity
    {
        [Key]
        public int MyId { get; set; }

        public int DerivedPrincipalEntityId { get; set; }
        public DerivedPrincipalEntity DerivedProp { get; set; }
    }

    public abstract class AbstractEntityType
    {
        public int IntProperty { get; set; }
    }

    public class SubEntityType : AbstractEntityType
    {
        public int SubKey { get; set; }
    }

    public class AnotherSubEntityType : AbstractEntityType
    {
        public double AnotherKey { get; set; }
    }

    public class FkSupplier
    {
        public int Id { get; set; }
    }

    public class FkProduct
    {
        public int Id { get; set; }

        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }
        public FkSupplier Supplier { get; set; }

        public int? SupplierKey { get; set; }
        [ForeignKey("SupplierKey")]
        public FkSupplier SupplierNav { get; set; }
    }

    public class FkSupplier2
    {
        public string Id { get; set; }
    }

    public class FkProduct2
    {
        public int Id { get; set; }

        public string SupplierId { get; set; }

        [Required] // with [Required]
        [ForeignKey("SupplierId")]
        public FkSupplier2 Supplier { get; set; }

        [ForeignKey("SupplierNav")]
        public string SupplierKey { get; set; }
        public FkSupplier2 SupplierNav { get; set; }
    }

    public class FkSupplier3
    {
        public int Id { get; set; }
    }

    public class FkProduct3
    {
        [Key]
        public int ProductKey { get; set; }

        public int? FkSupplierId { get; set; }
        [Required] // with [Required]
        public FkSupplier Supplier { get; set; }

        public string FkSupplier2Id { get; set; }
        [Required] // with [Required]
        public FkSupplier2 Supplier2 { get; set; }

        public int? FkSupplier3Id { get; set; }
        public FkSupplier3 Supplier3 { get; set; }
    }

    public class CustomerWithConcurrencyAttribute
    {
        public int ID { get; set; }

        [ConcurrencyCheck]
        public string Name { get; set; }

        [Timestamp]
        public DateTimeOffset Birthday { get; set; }
    }

#region Navigation property binding

    public class BindingCustomer
    {
        public int Id { get; set; }

        public BindingAddress Location { get; set; }

        public BindingAddress Address { get; set; }

        public IList<BindingAddress> Addresses { get; set; }
    }

    public class BindingVipCustomer : BindingCustomer
    {
        public BindingAddress VipLocation { get; set; }

        public IList<BindingAddress> VipAddresses { get; set; }
    }

    public class BindingCity
    {
        public int Id { get; set; }
    }

    public class BindingAddress
    {
        public BindingCity City { get; set; }

        public IList<BindingCity> Cities { get; set; }
    }

    public class BindingUsAddress : BindingAddress
    {
        public BindingCity UsCity { get; set; }

        public ICollection<BindingCity> UsCities { get; set; }
    }

#endregion
}
