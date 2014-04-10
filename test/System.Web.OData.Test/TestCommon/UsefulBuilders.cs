// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using Microsoft.OData.Edm;
using Microsoft.TestCommon.Types;

namespace System.Web.OData
{
    public static class UsefulBuilders
    {
        public static IEdmModel GetServiceModel(this ODataModelBuilder builder)
        {
            return EdmModelHelperMethods.BuildEdmModel(builder);
        }

        public static ODataModelBuilder Add_Color_EnumType(this ODataModelBuilder builder)
        {
            var color = builder.EnumType<Color>();
            color.Member(Color.Red);
            color.Member(Color.Green);
            color.Member(Color.Blue);
            return builder;
        }

        public static ODataModelBuilder Add_SimpleEnum_EnumType(this ODataModelBuilder builder)
        {
            var simpleEnum = builder.EnumType<SimpleEnum>();
            simpleEnum.Member(SimpleEnum.First);
            simpleEnum.Member(SimpleEnum.Second);
            simpleEnum.Member(SimpleEnum.Third);
            return builder;
        }

        public static ODataModelBuilder Add_FlagsEnum_EnumType(this ODataModelBuilder builder)
        {
            var flagsEnum = builder.EnumType<FlagsEnum>();
            flagsEnum.Member(FlagsEnum.One);
            flagsEnum.Member(FlagsEnum.Two);
            flagsEnum.Member(FlagsEnum.Four);
            return builder;
        }

        public static ODataModelBuilder Add_LongEnum_EnumType(this ODataModelBuilder builder)
        {
            var longEnum = builder.EnumType<LongEnum>();
            longEnum.Member(LongEnum.FirstLong);
            longEnum.Member(LongEnum.SecondLong);
            longEnum.Member(LongEnum.ThirdLong);
            return builder;
        }

        public static ODataModelBuilder Add_ByteEnum_EnumType(this ODataModelBuilder builder)
        {
            EnumTypeConfiguration<ByteEnum> byteEnum = builder.EnumType<ByteEnum>();
            byteEnum.Member(ByteEnum.FirstByte);
            byteEnum.Member(ByteEnum.SecondByte);
            byteEnum.Member(ByteEnum.ThirdByte);
            return builder;
        }

        public static ODataModelBuilder Add_SByteEnum_EnumType(this ODataModelBuilder builder)
        {
            EnumTypeConfiguration<SByteEnum> sByteEnum = builder.EnumType<SByteEnum>();
            sByteEnum.Member(SByteEnum.FirstSByte);
            sByteEnum.Member(SByteEnum.SecondSByte);
            sByteEnum.Member(SByteEnum.ThirdSByte);
            return builder;
        }

        public static ODataModelBuilder Add_ShortEnum_EnumType(this ODataModelBuilder builder)
        {
            EnumTypeConfiguration<ShortEnum> shortEnum = builder.EnumType<ShortEnum>();
            shortEnum.Member(ShortEnum.FirstShort);
            shortEnum.Member(ShortEnum.SecondShort);
            shortEnum.Member(ShortEnum.ThirdShort);
            return builder;
        }

        public static ODataModelBuilder Add_Address_ComplexType(this ODataModelBuilder builder)
        {
            var address = builder.ComplexType<Address>();
            address.Property(a => a.HouseNumber);
            address.Property(a => a.Street);
            address.Property(a => a.City);
            address.Property(a => a.State);
            return builder;
        }

        public static ODataModelBuilder Add_ZipCode_ComplexType(this ODataModelBuilder builder)
        {
            var zipCode = builder.ComplexType<ZipCode>();
            zipCode.Property(z => z.Part1);
            zipCode.Property(z => z.Part2).IsOptional();
            return builder;
        }

        public static ODataModelBuilder Add_RecursiveZipCode_ComplexType(this ODataModelBuilder builder)
        {
            var zipCode = builder.ComplexType<RecursiveZipCode>();
            zipCode.Property(z => z.Part1);
            zipCode.Property(z => z.Part2).IsOptional();
            return builder;
        }

        public static ODataModelBuilder Add_Customer_EntityType(this ODataModelBuilder builder)
        {
            var customer = builder.EntityType<Customer>();
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.CustomerId);
            customer.Property(c => c.Name).IsConcurrencyToken();
            customer.Property(c => c.Website);
            customer.Property(c => c.SharePrice);
            customer.Property(c => c.ShareSymbol);
            return builder;
        }

        public static ODataModelBuilder Add_Customer_EntityType_With_Address(this ODataModelBuilder builder)
        {
            builder.Add_Customer_EntityType();
            builder.Add_Address_ComplexType();
            var customer = builder.EntityType<Customer>();
            customer.ComplexProperty(c => c.Address);
            return builder;
        }

        public static ODataModelBuilder Add_Customer_EntityType_With_CollectionProperties(this ODataModelBuilder builder)
        {
            builder.Add_Customer_EntityType();
            builder.EntityType<Customer>().CollectionProperty(c => c.Aliases);
            builder.EntityType<Customer>().CollectionProperty(c => c.Addresses);
            return builder;
        }

        // Adds a Customer EntityType but allows caller to configure Keys()
        public static ODataModelBuilder Add_Customer_With_Keys_EntityType<TKey>(this ODataModelBuilder builder, Expression<Func<Customer, TKey>> keyDefinitionExpression)
        {
            var customer = builder.EntityType<Customer>();
            customer.HasKey(keyDefinitionExpression);
            customer.Property(c => c.CustomerId);
            customer.Property(c => c.Name);
            customer.Property(c => c.Website);
            customer.Property(c => c.SharePrice);
            customer.Property(c => c.ShareSymbol);
            return builder;
        }

        // Adds a Customer EntityType that has no key properties
        public static ODataModelBuilder Add_Customer_No_Keys_EntityType(this ODataModelBuilder builder)
        {
            var customer = builder.EntityType<Customer>();
            customer.Property(c => c.CustomerId);
            customer.Property(c => c.Name);
            customer.Property(c => c.Website);
            customer.Property(c => c.SharePrice);
            customer.Property(c => c.ShareSymbol);
            return builder;
        }

        public static ODataModelBuilder Add_Order_EntityType(this ODataModelBuilder builder)
        {
            var order = builder.EntityType<Order>();
            order.HasKey(o => o.OrderId);
            order.Property(o => o.OrderDate);
            order.Property(o => o.Price);
            order.Property(o => o.OrderDate);
            order.Property(o => o.DeliveryDate);
            order.Ignore(o => o.Cost);
            return builder;
        }

        public static ODataModelBuilder Add_Company_EntityType(this ODataModelBuilder builder)
        {
            var company = builder.EntityType<Company>();
            company.HasKey(c => c.CompanyId);
            company.Property(c => c.CompanyName).IsConcurrencyToken();
            company.Property(c => c.Website);
            return builder;
        }

        public static ODataModelBuilder Add_Company_Singleton(this ODataModelBuilder builder)
        {
            builder.Add_Company_EntityType().Singleton<Company>("OsCorp");
            return builder;
        }

        public static ODataModelBuilder Add_Employee_EntityType(this ODataModelBuilder builder)
        {
            var employee = builder.EntityType<Employee>();
            employee.HasKey(c => c.EmployeeID);
            employee.Property(c => c.EmployeeName).IsConcurrencyToken();
            employee.Property(c => c.BaseSalary);
            return builder;
        }

        public static ODataModelBuilder Add_CompanyEmployees_Relationship(this ODataModelBuilder builder)
        {
            builder.EntityType<Company>().HasMany(c => c.ComplanyEmployees);
            builder.EntityType<Company>().HasRequired(c => c.CEO);
            return builder;
        }

        public static ODataModelBuilder Add_EmployeeComplany_Relationship(this ODataModelBuilder builder)
        {
            builder.EntityType<Employee>().HasRequired(o => o.WorkCompany);
            return builder;
        }

        // EntitySet -> EntitySet
        public static ODataModelBuilder Add_CompaniesEmployees_Binding(this ODataModelBuilder builder)
        {
            builder.EntitySet<Company>("Companies").HasManyBinding(c => c.ComplanyEmployees, "Employees");
            return builder;
        }

        // EntitySet -> Singleton
        public static ODataModelBuilder Add_CompaniesCEO_Binding(this ODataModelBuilder builder)
        {
            builder.EntitySet<Company>("Companies").HasSingletonBinding(c => c.CEO, "CEO");
            return builder;
        }

        // Singleton -> EntitySet
        public static ODataModelBuilder Add_MicrosoftEmployees_Binding(this ODataModelBuilder builder)
        {
            builder.Singleton<Company>("OsCorp").HasManyBinding(c => c.ComplanyEmployees, "Employees");
            return builder;
        }

        // Singleton -> Singleton
        public static ODataModelBuilder Add_MicrosoftCEO_Binding(this ODataModelBuilder builder)
        {
            builder.Singleton<Company>("OsCorp").HasSingletonBinding(c => c.CEO, "CEO");
            return builder;
        }

        public static ODataModelBuilder Add_CustomerOrders_Relationship(this ODataModelBuilder builder)
        {
            builder.EntityType<Customer>().HasMany(c => c.Orders);
            return builder;
        }

        public static ODataModelBuilder Add_OrderCustomer_Relationship(this ODataModelBuilder builder)
        {
            builder.EntityType<Order>().HasRequired(o => o.Customer);
            return builder;
        }

        public static ODataModelBuilder Add_Customers_EntitySet(this ODataModelBuilder builder)
        {
            builder.Add_Customer_EntityType().EntitySet<Customer>("Customers");
            return builder;
        }

        public static ODataModelBuilder Add_Customers_Singleton(this ODataModelBuilder builder)
        {
            builder.Add_Customer_EntityType().Singleton<Customer>("VipCustomer");
            return builder;
        }

        // Adds a Customer EntitySet but allows caller to configure keys
        public static ODataModelBuilder Add_Customers_With_Keys_EntitySet<TKey>(this ODataModelBuilder builder, Expression<Func<Customer, TKey>> keyDefinitionExpression)
        {
            builder.Add_Customer_With_Keys_EntityType(keyDefinitionExpression).EntitySet<Customer>("Customers");
            return builder;
        }

        // Adds a Customer EntitySet with no key properties
        public static ODataModelBuilder Add_Customers_No_Keys_EntitySet(this ODataModelBuilder builder)
        {
            builder.Add_Customer_No_Keys_EntityType().EntitySet<Customer>("Customers");
            return builder;
        }

        public static ODataModelBuilder Add_Orders_EntitySet(this ODataModelBuilder builder)
        {
            builder.EntitySet<Order>("Orders");
            return builder;
        }

        public static ODataModelBuilder Add_CustomerOrders_Binding(this ODataModelBuilder builder)
        {
            builder.EntitySet<Customer>("Customers").HasManyBinding(c => c.Orders, "Orders");
            return builder;
        }

        public static ODataModelBuilder Add_OrderCustomer_Binding(this ODataModelBuilder builder)
        {
            builder.EntitySet<Order>("Orders").HasRequiredBinding(o => o.Customer, "Customer");
            return builder;
        }
    }
}
