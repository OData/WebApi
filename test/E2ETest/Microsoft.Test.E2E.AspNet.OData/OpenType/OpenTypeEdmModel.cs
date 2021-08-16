//-----------------------------------------------------------------------------
// <copyright file="OpenTypeEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.OpenType
{
    internal class OpenComplexTypeEdmModel
    {
        private const string NameSpace = "Microsoft.Test.E2E.AspNet.OData.OpenType";

        public static IEdmModel GetTypedExplicitModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var accountType = builder.EntityType<Account>();
            accountType.HasKey(c => c.Id);
            accountType.Property(c => c.Name);
            accountType.HasDynamicProperties(c => c.DynamicProperties);

            accountType.ComplexProperty<AccountInfo>(c => c.AccountInfo);
            accountType.ComplexProperty<Address>(a => a.Address);
            accountType.ComplexProperty<Tags>(a => a.Tags);

            var premiumAccountType = builder.EntityType<PremiumAccount>();
            premiumAccountType.Property(p => p.Since);
            premiumAccountType.DerivesFrom<Account>();

            var accountInfoType = builder.ComplexType<AccountInfo>();
            accountInfoType.Property(i => i.NickName);
            accountInfoType.HasDynamicProperties(i => i.DynamicProperties);

            var addressType = builder.ComplexType<Address>();
            addressType.Property(a => a.City);
            addressType.Property(a => a.Street);
            addressType.HasDynamicProperties(a => a.DynamicProperties);

            var globalAddressType = builder.ComplexType<GlobalAddress>();
            globalAddressType.Property(a => a.CountryCode);
            globalAddressType.DerivesFrom<Address>();

            var tagsType = builder.ComplexType<Tags>();
            tagsType.HasDynamicProperties(t => t.DynamicProperties);

            var gender = builder.EnumType<Gender>();
            gender.Member(Gender.Female);
            gender.Member(Gender.Male);

            var employeeType = builder.EntityType<Employee>();
            employeeType.HasKey(e => e.Id);
            employeeType.HasOptional(e => e.Account);
            builder.EntitySet<Employee>("Employees");

            var managerType = builder.EntityType<Manager>();
            managerType.Property(m => m.Heads);
            managerType.HasDynamicProperties(m => m.DynamicProperties);
            managerType.DerivesFrom<Employee>();

            AddBoundActionsAndFunctions(accountType);
            AddUnboundActionsAndFunctions(builder);

            EntitySetConfiguration<Account> accounts = builder.EntitySet<Account>("Accounts");
            builder.Namespace = typeof(Account).Namespace;
            return builder.GetEdmModel();
        }

        public static IEdmModel GetTypedConventionModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<Employee>("Employees");
            builder.EntitySet<Account>("Accounts");

            builder.EnumType<Gender>();

            AddBoundActionsAndFunctions(builder.EntityType<Account>());
            AddUnboundActionsAndFunctions(builder);

            builder.Namespace = typeof(Account).Namespace;

            return builder.GetEdmModel();
        }

        private static void AddBoundActionsAndFunctions(EntityTypeConfiguration<Account> account)
        {
            account.Function("GetAddressFunction").Returns<Address>();

            account.Function("GetShipAddresses").ReturnsCollection<Address>();
            account.Action("IncreaseAgeAction").Returns<AccountInfo>();

            ActionConfiguration addShipAddress = account.Action("AddShipAddress");
            addShipAddress.Parameter<Address>("address");
            addShipAddress.Returns<int>();// Total ship addresses count.
        }

        private static void AddUnboundActionsAndFunctions(ODataModelBuilder odataModelBuilder)
        {
            odataModelBuilder.Action("ResetDataSource");

            ActionConfiguration udpateAddress = odataModelBuilder.Action("UpdateAddressAction");
            udpateAddress.Parameter<Address>("Address");
            udpateAddress.Parameter<int>("ID");
            udpateAddress.Returns<Address>();
        }
    }
}
