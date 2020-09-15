// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.Containment
{
    public class ContainmentEdmModels
    {
        public static IEdmModel GetExplicitModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var accountType = builder.EntityType<Account>();
            accountType.HasKey(a => a.AccountID);
            accountType.Property(a => a.Name);
            var payoutPI = accountType.ContainsOptional(a => a.PayoutPI);
            var payinPIs = accountType.HasMany(a => a.PayinPIs)
                .Contained();

            var premiumAccountType = builder.EntityType<PremiumAccount>()
                .DerivesFrom<Account>();
            var giftCard = premiumAccountType.ContainsRequired(pa => pa.GiftCard);

            var giftCardType = builder.EntityType<GiftCard>();
            giftCardType.HasKey(g => g.GiftCardID);
            giftCardType.Property(g => g.GiftCardNO);
            giftCardType.Property(g => g.Amount);

            var paymentInstrumentType = builder.EntityType<PaymentInstrument>();
            paymentInstrumentType.HasKey(pi => pi.PaymentInstrumentID);
            paymentInstrumentType.Property(pi => pi.FriendlyName);
            var statement = paymentInstrumentType.ContainsOptional(pi => pi.Statement);

            var statementType = builder.EntityType<Statement>();
            statementType.HasKey(s => s.StatementID);
            statementType.Property(s => s.TransactionDescription);
            statementType.Property(s => s.Amount);

            var accounts = builder.EntitySet<Account>("Accounts"); 
            accounts.HasIdLink(c => c.GenerateSelfLink(false), true);
            accounts.HasEditLink(c => c.GenerateSelfLink(true), true);

            var paginatedAccounts = builder.EntitySet<Account>("PaginatedAccounts");
            accounts.HasIdLink(c => c.GenerateSelfLink(false), true);
            accounts.HasEditLink(c => c.GenerateSelfLink(true), true);

            builder.Singleton<Account>("AnonymousAccount");

            AddBoundActionsAndFunctions(builder);

            builder.Namespace = typeof(Account).Namespace;

            return builder.GetEdmModel();
        }

        public static IEdmModel GetConventionModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            var paymentInstrumentType = builder.EntityType<PaymentInstrument>();

            builder.EntitySet<Account>("Accounts");

            builder.EntitySet<Account>("PaginatedAccounts");

            builder.Singleton<Account>("AnonymousAccount");

            AddBoundActionsAndFunctions(builder);

            builder.Namespace = typeof(Account).Namespace;

            return builder.GetEdmModel();
        }

        private static void AddBoundActionsAndFunctions(ODataModelBuilder builder)
        {
            var paymentInstrumentType = builder.EntityType<PaymentInstrument>();

            var actionConfiguration = paymentInstrumentType.Collection.Action("Clear");
            actionConfiguration.Parameter<string>("nameContains");
            actionConfiguration.Returns<int>();// deleted count

            // Bug 2021-Should support Action/Function returns contained entities.
            //paymentInstrumentType.Action("Duplicate").Returns<PaymentInstrument>();

            paymentInstrumentType.Action("Delete");

            var functionConfiguration = paymentInstrumentType.Collection.Function("GetCount");
            functionConfiguration.Parameter<string>("nameContains");
            functionConfiguration.Returns<int>();

            builder.Action("ResetDataSource");
        }
    }
}
