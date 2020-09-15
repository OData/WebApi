// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Containment
{
    class ContainmentDataSource
    {
        private IList<Account> accounts = null;
        private Account anonymousAccount = null;
        private IList<Account> paginatedAccounts = null;

        public Account AnonymousAccount
        {
            get
            {
                if (anonymousAccount == null)
                {
                    anonymousAccount = new Account()
                    {
                        AccountID = 0,
                        Name = "Name0",
                        PayoutPI = new PaymentInstrument()
                        {
                            PaymentInstrumentID = 0,
                            FriendlyName = "Anonymous payout PI",
                        },
                        PayinPIs = new List<PaymentInstrument>()
                        {
                             new PaymentInstrument()
                            {
                                PaymentInstrumentID = 0,
                                FriendlyName = "Anonymous payin PI",
                                Statement =
                                    new Statement()
                                    {
                                        StatementID=0,
                                        TransactionDescription="Physical Goods.",
                                    },
                            },
                        },
                    };
                }
                return anonymousAccount;
            }
        }

        public IList<Account> Accounts
        {
            get
            {
                if (accounts == null)
                {
                    InitAccounts();
                }
                return accounts;
            }
        }

        public IList<Account> PaginatedAccounts
        {
            get
            {
                if (paginatedAccounts == null)
                {
                    InitPaginatedAccounts();
                }
                return paginatedAccounts;
            }
        }


        private void InitAccounts()
        {
            accounts = new List<Account>()
            {
                new Account()
                {
                   AccountID = 100,
                        Name="Name100",
                   PayoutPI = new PaymentInstrument()
                   {
                       PaymentInstrumentID = 100,
                       FriendlyName = "Payout PI: Paypal",
                   },
                    PayinPIs = new List<PaymentInstrument>()
                    {
                        new PaymentInstrument()
                        {
                            PaymentInstrumentID = 101,
                            FriendlyName = "101 first PI",
                            Statement =
                                new Statement()
                                {
                                    StatementID=1,
                                    TransactionDescription="Physical Goods.",
                                },
                        },
                        new PaymentInstrument()
                        {
                            PaymentInstrumentID = 102,
                            FriendlyName = "102 second PI",
                            Statement =
                                new Statement()
                                {
                                    StatementID=101,
                                    TransactionDescription="Physical Goods.",
                                },
                        },
                    },
                },
                new PremiumAccount()
                {
                    AccountID = 200,
                        Name="Name200",
                    PayoutPI = new PaymentInstrument()
                    {
                       PaymentInstrumentID = 200,
                       FriendlyName = "Payout PI: Direct Debit",
                    },
                    PayinPIs = new List<PaymentInstrument>()
                    {
                        new PaymentInstrument()
                        {
                            PaymentInstrumentID = 201,
                            FriendlyName = "201 first PI",
                            Statement =
                                new Statement()
                                {
                                    StatementID=201,
                                    TransactionDescription="Physical Goods.",
                                },
                        },
                    },
                    GiftCard = new GiftCard()
                    {
                        GiftCardID = 200,
                        GiftCardNO = "BBA1-2BBC",
                        Amount = 2000,
                    },
                },
            };
        }

        private void InitPaginatedAccounts()
        {
            paginatedAccounts = new List<Account>()
            {
                new Account()
                {
                   AccountID = 100,
                        Name="Name100",
                   PayoutPI = new PaymentInstrument()
                   {
                       PaymentInstrumentID = 100,
                       FriendlyName = "Payout PI: Paypal",
                   },
                    PayinPIs = new List<PaymentInstrument>()
                    {
                        new PaymentInstrument()
                        {
                            PaymentInstrumentID = 101,
                            FriendlyName = "101 first PI",
                            Statement =
                                new Statement()
                                {
                                    StatementID=1,
                                    TransactionDescription="Physical Goods.",
                                },
                        },
                        new PaymentInstrument()
                        {
                            PaymentInstrumentID = 102,
                            FriendlyName = "102 second PI",
                            Statement =
                                new Statement()
                                {
                                    StatementID=101,
                                    TransactionDescription="Physical Goods.",
                                },
                        },
                    },
                },
            };
        }
    }
}
